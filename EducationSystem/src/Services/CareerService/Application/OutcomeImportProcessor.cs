using System.Data;
using System.Text.Json;
using CareerService.Infrastructure.Persistence;
using CareerService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CareerService.Application;

public sealed class OutcomeImportProcessor(
    CareerDbContext dbContext,
    IOutcomeFileStorage fileStorage,
    IOutcomeDocumentParser parser,
    IOutcomeExtractionProvider extractionProvider,
    OutcomeReconciliationService reconciliation,
    IOutcomeValidationService validationService,
    OutcomeQualityEvaluator qualityEvaluator,
    IOptions<LlmOptions> llmOptions,
    TimeProvider timeProvider,
    ILogger<OutcomeImportProcessor> logger) : IOutcomeImportProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        long? claimedId = null;
        await using (var transaction = await dbContext.Database.BeginTransactionAsync(
                         IsolationLevel.Serializable, cancellationToken))
        {
            var batch = await dbContext.OutcomeImportBatches
                .Where(item => item.Status == OutcomeImportStatuses.Uploaded)
                .OrderBy(item => item.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (batch is not null)
            {
                batch.Status = OutcomeImportStatuses.Processing;
                batch.ProcessingStage = OutcomeProcessingStages.Waiting;
                batch.ProgressPercent = 1;
                batch.ProcessingStartedAt = timeProvider.GetUtcNow().UtcDateTime;
                batch.UpdatedAt = batch.ProcessingStartedAt.Value;
                batch.ErrorCode = null;
                batch.ErrorMessage = null;
                await dbContext.SaveChangesAsync(cancellationToken);
                claimedId = batch.Id;
            }
            await transaction.CommitAsync(cancellationToken);
        }

        if (!claimedId.HasValue) return false;
        await ProcessAsync(claimedId.Value, cancellationToken);
        return true;
    }

    public async Task ProcessAsync(long batchId, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await dbContext.OutcomeImportBatches
                .Include(item => item.CurriculumVersion)
                .SingleAsync(item => item.Id == batchId, cancellationToken);
            if (batch.Status != OutcomeImportStatuses.Processing)
                throw new OutcomeImportException(
                    "INVALID_STATE", "Only a Processing batch can be processed.", 409);

            await Stage(batch, OutcomeProcessingStages.ParsingDocument, 8, cancellationToken);
            await using var source = await fileStorage.OpenReadAsync(batch.StorageKey, cancellationToken);
            var blocks = await parser.ParseAsync(source, cancellationToken);
            if (blocks.Count == 0)
                throw new OutcomeImportException(
                    "DOCUMENT_EMPTY", "No readable content was found in the DOCX.", 422);

            await dbContext.OutcomeDocumentBlocks
                .Where(item => item.ImportBatchId == batch.Id)
                .ExecuteDeleteAsync(cancellationToken);
            dbContext.OutcomeDocumentBlocks.AddRange(blocks.Select(item => new OutcomeDocumentBlock
            {
                ImportBatchId = batch.Id,
                BlockId = item.BlockId,
                BlockType = item.BlockType,
                Sequence = item.Sequence,
                HeadingLevel = item.HeadingLevel,
                ContentJson = item.ContentJson
            }));
            await dbContext.SaveChangesAsync(cancellationToken);

            var settings = llmOptions.Value;
            var selection = new OutcomeExtractionSelection(
                batch.CurriculumVersion.MajorCode,
                batch.CurriculumVersion.MajorName,
                batch.CurriculumVersion.CurriculumCode,
                batch.CurriculumVersion.CurriculumName,
                batch.CurriculumVersion.Version,
                batch.SelectedSubjectExternalId,
                batch.SelectedSubjectCode,
                batch.SelectedSubjectName);
            var subjectScoped = !string.IsNullOrWhiteSpace(batch.SelectedSubjectCode);
            var hasExplicitMatrix =
                OutcomeExtractionContextBuilder.HasExplicitMatrixEvidence(blocks);
            var requiredRequests = 1 + (subjectScoped ? 0 : 1) + (hasExplicitMatrix ? 1 : 0);
            if (settings.MaxRequestsPerBatch < requiredRequests)
                throw new OutcomeImportException(
                    "LLM_REQUEST_LIMIT_TOO_LOW",
                    $"MaxRequestsPerBatch must allow {requiredRequests} extraction request(s).", 503);
            var maximumCharacters = checked(settings.MaxInputTokensPerRequest * 4);
            var outcomesInput = OutcomeExtractionContextBuilder.BuildDocumentContext(
                2, selection, blocks, maximumCharacters);
            var expectedCourseObjectives =
                OutcomeExtractionContextBuilder.CountFormalCourseObjectives(blocks);

            await Stage(batch, OutcomeProcessingStages.ExtractingProgram, 22, cancellationToken);
            OutcomeExtractionProviderResult program;
            if (subjectScoped)
            {
                program = BuildSubjectScopedProgramResult(selection);
            }
            else
            {
                var programInput = OutcomeExtractionContextBuilder.BuildDocumentContext(
                    1, selection, blocks, maximumCharacters);
                program = await extractionProvider.ExtractAsync(
                    new OutcomeExtractionProviderRequest(
                        "Extract curriculum metadata and only explicitly identified PLO/program outcomes. " +
                        "A course objective is not a PLO; return an empty PLO list when the document has no explicit PLO.",
                        settings.PromptVersion, programInput, 1),
                    cancellationToken);
            }

            await Stage(batch, OutcomeProcessingStages.ExtractingOutcomes, 43, cancellationToken);
            var outcomes = await extractionProvider.ExtractAsync(
                new OutcomeExtractionProviderRequest(
                    batch.SelectedSubjectCode is null
                        ? "Extract subjects and formal CLOs. Top-level course objectives may be treated as CLO evidence."
                        : "Extract only the selected subject and its formal CLOs. " +
                          "Top-level MỤC TIÊU MÔN HỌC statements may be treated as CLO evidence; " +
                          "do not convert chapter content or lesson objectives into additional CLOs. " +
                          (expectedCourseObjectives > 0
                              ? $"The formal objective section contains exactly {expectedCourseObjectives} " +
                                "top-level '+' bullets. Return exactly one separate CLO per bullet, preserve " +
                                "source order, and do not merge or omit bullets."
                              : string.Empty),
                    settings.PromptVersion, outcomesInput, 2),
                cancellationToken);

            await Stage(batch, OutcomeProcessingStages.ExtractingMatrix, 64, cancellationToken);
            OutcomeExtractionProviderResult mappings;
            if (!hasExplicitMatrix)
            {
                mappings = new OutcomeExtractionProviderResult(
                    """{"mappings":[],"warnings":[]}""", null);
            }
            else
            {
                var matrixInput = OutcomeExtractionContextBuilder.BuildMatrixContext(
                    selection, blocks, program.Json, outcomes.Json, maximumCharacters);
                mappings = await extractionProvider.ExtractAsync(
                    new OutcomeExtractionProviderRequest(
                        "Extract only explicit CLO-PLO matrix values. Use draft IDs from extractedProgram and " +
                        "extractedOutcomes. If no explicit matrix exists, return an empty mappings list.",
                        settings.PromptVersion, matrixInput, 3),
                    cancellationToken);
            }

            await Stage(batch, OutcomeProcessingStages.Reconciling, 78, cancellationToken);
            var document = reconciliation.Reconcile(
                batch.Id, program.Json, outcomes.Json, mappings.Json,
                batch.SelectedSubjectCode);
            if (subjectScoped)
            {
                ReconcileFormalCourseObjectives(document, blocks, selection);
                ApplySelectedMetadataEvidence(document, blocks, selection);
            }

            await Stage(batch, OutcomeProcessingStages.Validating, 90, cancellationToken);
            var validation = (await validationService.ValidateAsync(
                document, batch.CurriculumVersionId, cancellationToken)).ToList();
            var reviewedCloCount = document.Subjects
                .Where(item => !item.Removed)
                .SelectMany(item => item.Clos)
                .Count(item => !item.Removed);
            if (subjectScoped &&
                expectedCourseObjectives > 0 &&
                reviewedCloCount != expectedCourseObjectives)
                validation.Add(new OutcomeWarning(
                    "CLO_COUNT_MISMATCH",
                    "BlockingError",
                    document.Subjects.FirstOrDefault(item => !item.Removed)?.DraftId,
                    "clos",
                    $"The source contains {expectedCourseObjectives} formal objectives but extraction returned {reviewedCloCount} CLOs."));
            var quality = qualityEvaluator.Evaluate(
                document, blocks, validation, batch.SelectedSubjectCode);

            batch.Provider = settings.Provider;
            batch.ModelName = settings.Model;
            batch.PromptVersion = settings.PromptVersion;
            batch.RawExtractionJson = JsonSerializer.Serialize(new
            {
                program = JsonSerializer.Deserialize<JsonElement>(program.Json),
                outcomes = JsonSerializer.Deserialize<JsonElement>(outcomes.Json),
                mappings = JsonSerializer.Deserialize<JsonElement>(mappings.Json)
            }, JsonOptions);
            batch.ReviewedJson = JsonSerializer.Serialize(document, JsonOptions);
            batch.ValidationJson = JsonSerializer.Serialize(validation, JsonOptions);
            batch.QualityReportJson = JsonSerializer.Serialize(quality, JsonOptions);
            batch.Status = validation.Any(item => item.Severity == "BlockingError")
                ? OutcomeImportStatuses.ValidationFailed
                : OutcomeImportStatuses.PendingReview;
            batch.ProcessingStage = OutcomeProcessingStages.Completed;
            batch.ProgressPercent = 100;
            batch.ProcessingCompletedAt = timeProvider.GetUtcNow().UtcDateTime;
            batch.UpdatedAt = batch.ProcessingCompletedAt.Value;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Outcome import {ImportBatchId} completed with {BlockCount} blocks and status {Status}.",
                batch.Id, blocks.Count, batch.Status);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Outcome import {ImportBatchId} failed.", batchId);
            dbContext.ChangeTracker.Clear();
            var failed = await dbContext.OutcomeImportBatches
                .SingleOrDefaultAsync(item => item.Id == batchId, CancellationToken.None);
            if (failed is not null && failed.Status == OutcomeImportStatuses.Processing)
            {
                failed.Status = OutcomeImportStatuses.Failed;
                failed.ErrorCode = exception is OutcomeImportException known
                    ? known.ErrorCode
                    : "PROCESSING_FAILED";
                failed.ErrorMessage = exception is OutcomeImportException
                    ? exception.Message
                    : "Outcome extraction failed.";
                failed.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
        }
    }

    public static OutcomeExtractionProviderResult BuildSubjectScopedProgramResult(
        OutcomeExtractionSelection selection) =>
        new(
            JsonSerializer.Serialize(new
            {
                curriculum = new
                {
                    draftId = "curriculum-001",
                    selection.MajorCode,
                    selection.MajorName,
                    selection.CurriculumCode,
                    selection.CurriculumName,
                    selection.Version,
                    sourceReferences = Array.Empty<object>(),
                    confidence = 1,
                    warnings = Array.Empty<object>(),
                    removed = false
                },
                plos = Array.Empty<object>(),
                warnings = Array.Empty<object>()
            }, JsonOptions),
            null);

    public static void ApplySelectedMetadataEvidence(
        OutcomeDraftDocument document,
        IReadOnlyList<DocumentBlockData> blocks,
        OutcomeExtractionSelection selection)
    {
        if (blocks.Count == 0)
            return;
        var curriculumBlock = blocks.OrderBy(item => item.Sequence).First();
        if (document.Curriculum.SourceReferences.Count == 0)
            document.Curriculum.SourceReferences.Add(Reference(curriculumBlock));

        foreach (var subject in document.Subjects.Where(item =>
                     string.Equals(
                         item.Subject.Code,
                         selection.SubjectCode,
                         StringComparison.OrdinalIgnoreCase)))
        {
            if (subject.SourceReferences.Count > 0)
                continue;
            var subjectBlock = blocks
                .OrderBy(item => item.Sequence)
                .FirstOrDefault(item =>
                    ContentText(item.ContentJson).Contains(
                        selection.SubjectName ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase))
                ?? curriculumBlock;
            subject.SourceReferences.Add(Reference(subjectBlock));
        }
    }

    public static bool ReconcileFormalCourseObjectives(
        OutcomeDraftDocument document,
        IReadOnlyList<DocumentBlockData> blocks,
        OutcomeExtractionSelection selection)
    {
        var objectives =
            OutcomeExtractionContextBuilder.GetFormalCourseObjectives(blocks);
        if (objectives.Count == 0)
            return false;

        var subject = document.Subjects.FirstOrDefault(item =>
            !item.Removed &&
            string.Equals(
                item.Subject.Code,
                selection.SubjectCode,
                StringComparison.OrdinalIgnoreCase));
        if (subject is null)
        {
            subject = new SubjectDraft
            {
                DraftId = "subject-001",
                Subject = new SubjectReference
                {
                    ExternalId = selection.SubjectExternalId,
                    Code = selection.SubjectCode ?? string.Empty,
                    Name = selection.SubjectName ?? string.Empty,
                    MatchStatus = "Selected"
                },
                Confidence = 1
            };
            document.Subjects.Add(subject);
        }

        if (subject.Clos.Count(item => !item.Removed) == objectives.Count)
            return false;

        var codePrefix = string.IsNullOrWhiteSpace(selection.SubjectCode)
            ? "selected"
            : selection.SubjectCode.ToLowerInvariant();
        subject.Clos = objectives.Select((block, index) =>
        {
            var description = ContentText(block.ContentJson).TrimStart();
            description = description.TrimStart('+', '•', ' ', '\t');
            return new CloDraft
            {
                DraftId = $"clo-{codePrefix}-{index + 1:000}",
                CloCode = $"CLO{index + 1}",
                Description = description,
                OriginalDescription = description,
                SortOrder = index + 1,
                SourceReferences = [Reference(block)],
                Confidence = 1
            };
        }).ToList();
        document.Warnings.Add(new OutcomeWarning(
            "CLO_LIST_RECONCILED_FROM_FORMAL_OBJECTIVES",
            "Warning",
            subject.DraftId,
            "clos",
            $"The provider CLO count did not match the {objectives.Count} explicit formal objectives; the review list was reconstructed from those source bullets."));
        return true;
    }

    private static SourceReference Reference(DocumentBlockData block)
    {
        using var content = JsonDocument.Parse(block.ContentJson);
        var root = content.RootElement;
        var text = root.TryGetProperty("text", out var textProperty)
            ? textProperty.GetString() ?? string.Empty
            : string.Empty;
        var headingPath = root.TryGetProperty("headingPath", out var headingProperty) &&
                          headingProperty.ValueKind == JsonValueKind.Array
            ? headingProperty.EnumerateArray()
                .Select(item => item.GetString() ?? string.Empty)
                .ToArray()
            : [];
        return new SourceReference(
            block.BlockId,
            block.BlockType,
            block.Sequence,
            headingPath,
            root.TryGetProperty("paragraphIndex", out var paragraph)
                ? paragraph.GetInt32()
                : null,
            root.TryGetProperty("tableIndex", out var table) ? table.GetInt32() : null,
            null,
            null,
            text.Length <= 300 ? text : text[..300]);
    }

    private static string ContentText(string contentJson)
    {
        using var content = JsonDocument.Parse(contentJson);
        return content.RootElement.TryGetProperty("text", out var text)
            ? text.GetString() ?? string.Empty
            : string.Empty;
    }

    private async Task Stage(
        OutcomeImportBatch batch,
        string stage,
        byte progress,
        CancellationToken cancellationToken)
    {
        batch.ProcessingStage = stage;
        batch.ProgressPercent = progress;
        batch.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

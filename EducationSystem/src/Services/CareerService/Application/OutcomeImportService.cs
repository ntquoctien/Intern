using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using CareerService.Infrastructure;
using CareerService.Infrastructure.Persistence;
using CareerService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerService.Application;

public sealed class OutcomeImportService(
    CareerDbContext dbContext,
    OutcomeDocumentFileValidator fileValidator,
    IOutcomeFileStorage fileStorage,
    IOutcomeValidationService validationService,
    OutcomeQualityEvaluator qualityEvaluator,
    ApprovedOutcomeDocumentService approvedDocumentService,
    TimeProvider timeProvider)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<CurriculumOptionDto>> GetCurriculaAsync(
        CancellationToken cancellationToken) =>
        await dbContext.CurriculumVersions.AsNoTracking()
            .Where(item => item.Status == "Draft" || item.Status == "PendingReview")
            .OrderBy(item => item.MajorCode).ThenBy(item => item.CurriculumCode).ThenBy(item => item.Version)
            .Select(item => new CurriculumOptionDto(
                item.Id, item.MajorExternalId, item.MajorCode, item.MajorName,
                item.CurriculumCode, item.CurriculumName, item.Version,
                item.EffectiveFrom, item.EffectiveTo, item.Status))
            .ToListAsync(cancellationToken);

    public async Task<CurriculumOptionDto> CreateCurriculumAsync(
        CreateCurriculumRequest request,
        CancellationToken cancellationToken)
    {
        Required(request.MajorCode, "majorCode");
        Required(request.MajorName, "majorName");
        Required(request.CurriculumCode, "curriculumCode");
        Required(request.Version, "version");
        if (request.EffectiveTo.HasValue && request.EffectiveFrom.HasValue &&
            request.EffectiveTo < request.EffectiveFrom)
            throw new OutcomeImportException(
                "INVALID_EFFECTIVE_RANGE", "EffectiveTo cannot precede EffectiveFrom.", 400);

        var entity = new CurriculumVersion
        {
            MajorExternalId = request.MajorExternalId,
            MajorCode = request.MajorCode.Trim(),
            MajorName = request.MajorName.Trim(),
            CurriculumCode = request.CurriculumCode.Trim(),
            CurriculumName = NullIfBlank(request.CurriculumName),
            Version = request.Version.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Status = "Draft",
            Description = NullIfBlank(request.Description),
            UpdatedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        dbContext.CurriculumVersions.Add(entity);
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception)
        {
            throw new OutcomeImportException(
                "CURRICULUM_DUPLICATE", "The curriculum version already exists.", 409, exception);
        }
        return new CurriculumOptionDto(
            entity.Id, entity.MajorExternalId, entity.MajorCode, entity.MajorName,
            entity.CurriculumCode, entity.CurriculumName, entity.Version,
            entity.EffectiveFrom, entity.EffectiveTo, entity.Status);
    }

    public async Task<UploadOutcomeImportResponse> UploadAsync(
        long curriculumVersionId,
        Guid? subjectExternalId,
        string? subjectCode,
        string? subjectName,
        string fileName,
        string contentType,
        ReadOnlyMemory<byte> content,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        var extension = fileValidator.Validate(fileName, contentType, content);
        var hasSubjectContext = subjectExternalId.HasValue ||
                                !string.IsNullOrWhiteSpace(subjectCode) ||
                                !string.IsNullOrWhiteSpace(subjectName);
        if (hasSubjectContext &&
            (!subjectExternalId.HasValue ||
             string.IsNullOrWhiteSpace(subjectCode) ||
             string.IsNullOrWhiteSpace(subjectName)))
            throw new OutcomeImportException(
                "INVALID_SUBJECT_CONTEXT",
                "Subject identifier, code, and name must be supplied together.", 400);

        var normalizedSubjectCode = NullIfBlank(subjectCode);
        var normalizedSubjectName = NullIfBlank(subjectName);
        if (normalizedSubjectCode?.Length > 100 || normalizedSubjectName?.Length > 255)
            throw new OutcomeImportException(
                "INVALID_SUBJECT_CONTEXT",
                "Subject code or name exceeds the supported length.", 400);

        var curriculum = await dbContext.CurriculumVersions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == curriculumVersionId, cancellationToken)
            ?? throw new OutcomeImportException(
                "CURRICULUM_NOT_FOUND", "Target curriculum was not found.", 404);
        if (curriculum.Status is not ("Draft" or "PendingReview"))
            throw new OutcomeImportException(
                "CURRICULUM_NOT_IMPORTABLE",
                "Only Draft or PendingReview curriculum versions may be imported.", 409);

        var hash = Convert.ToHexString(SHA256.HashData(content.Span)).ToLowerInvariant();
        var duplicate = await dbContext.OutcomeImportBatches.AsNoTracking()
            .Where(item => item.CurriculumVersionId == curriculumVersionId &&
                           item.FileHash == hash &&
                           item.SelectedSubjectCode == normalizedSubjectCode &&
                           item.Status != OutcomeImportStatuses.Failed &&
                           item.Status != OutcomeImportStatuses.Rejected &&
                           item.Status != OutcomeImportStatuses.Archived)
            .Select(item => (long?)item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (duplicate.HasValue)
            throw new OutcomeImportException(
                "DUPLICATE_IMPORT",
                $"The same document already exists as import batch {duplicate.Value}.", 409);

        var storageKey = $"outcome-imports/{Guid.NewGuid():N}/source{extension}";
        await fileStorage.SaveAsync(storageKey, content, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var entity = new OutcomeImportBatch
        {
            CurriculumVersionId = curriculumVersionId,
            SelectedSubjectExternalId = subjectExternalId,
            SelectedSubjectCode = normalizedSubjectCode,
            SelectedSubjectName = normalizedSubjectName,
            OriginalFileName = Path.GetFileName(fileName),
            StorageKey = storageKey,
            ContentType = contentType,
            FileSize = content.Length,
            FileHash = hash,
            Status = OutcomeImportStatuses.Uploaded,
            ProcessingStage = OutcomeProcessingStages.Waiting,
            ProgressPercent = 0,
            UploadedByExternalId = actor.ExternalId,
            UploadedByName = actor.Name,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.OutcomeImportBatches.Add(entity);
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch
        {
            await fileStorage.DeleteIfExistsAsync(storageKey, CancellationToken.None);
            throw;
        }
        return new UploadOutcomeImportResponse(
            entity.Id, entity.Status, entity.OriginalFileName, RowVersionCodec.Encode(entity.RowVersion));
    }

    public async Task<PagedResponse<OutcomeImportListItemDto>> ListAsync(
        string? status,
        string? majorCode,
        long? curriculumVersionId,
        string? uploadedBy,
        DateTime? fromDate,
        DateTime? toDate,
        string? keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = dbContext.OutcomeImportBatches.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(item => item.Status == status);
        if (!string.IsNullOrWhiteSpace(majorCode)) query = query.Where(item => item.CurriculumVersion.MajorCode == majorCode);
        if (curriculumVersionId.HasValue) query = query.Where(item => item.CurriculumVersionId == curriculumVersionId);
        if (!string.IsNullOrWhiteSpace(uploadedBy)) query = query.Where(item => item.UploadedByName.Contains(uploadedBy));
        if (fromDate.HasValue) query = query.Where(item => item.CreatedAt >= fromDate);
        if (toDate.HasValue) query = query.Where(item => item.CreatedAt < toDate.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(item =>
                item.OriginalFileName.Contains(keyword) ||
                item.CurriculumVersion.CurriculumCode.Contains(keyword) ||
                (item.SelectedSubjectCode != null && item.SelectedSubjectCode.Contains(keyword)) ||
                (item.SelectedSubjectName != null && item.SelectedSubjectName.Contains(keyword)));

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(item => item.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(item => new
            {
                item,
                item.CurriculumVersion.MajorCode,
                item.CurriculumVersion.MajorName,
                item.CurriculumVersion.CurriculumCode,
                item.CurriculumVersion.Version
            }).ToListAsync(cancellationToken);
        var items = rows.Select(row =>
        {
            var counts = Counts(row.item.ReviewedJson);
            return new OutcomeImportListItemDto(
                row.item.Id, row.item.CurriculumVersionId,
                row.item.SelectedSubjectExternalId,
                row.item.SelectedSubjectCode,
                row.item.SelectedSubjectName,
                row.MajorCode, row.MajorName, row.CurriculumCode, row.Version,
                row.item.OriginalFileName, row.item.FileSize, row.item.Status,
                row.item.ProcessingStage, row.item.ProgressPercent,
                counts.Plos, counts.Clos, counts.Mappings,
                row.item.UploadedByName, row.item.CreatedAt, row.item.UpdatedAt,
                RowVersionCodec.Encode(row.item.RowVersion));
        }).ToList();
        return new PagedResponse<OutcomeImportListItemDto>(
            items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<OutcomeImportDetailDto> DetailAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var item = await FindBatchAsync(id, cancellationToken);
        return Detail(item);
    }

    public async Task ProcessAsync(long id, CancellationToken cancellationToken)
    {
        var item = await FindBatchAsync(id, cancellationToken);
        if (item.Status == OutcomeImportStatuses.Processing) return;
        if (item.Status == OutcomeImportStatuses.Failed)
        {
            item.Status = OutcomeImportStatuses.Uploaded;
            item.ProcessingStage = OutcomeProcessingStages.Waiting;
            item.ProgressPercent = 0;
            item.ErrorCode = null;
            item.ErrorMessage = null;
            item.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }
        if (item.Status != OutcomeImportStatuses.Uploaded)
            throw new OutcomeImportException(
                "INVALID_STATE", "This import cannot be processed in its current state.", 409);
    }

    public async Task<OutcomeImportReviewDto> ReviewAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var item = await FindBatchAsync(id, cancellationToken);
        var document = DeserializeDocument(item.ReviewedJson);
        var validation = DeserializeValidation(item.ValidationJson);
        var quality = DeserializeQuality(item.QualityReportJson);
        var blocks = await LoadBlocksAsync(id, cancellationToken);
        return new OutcomeImportReviewDto(
            Detail(item), document, validation, quality, blocks, AllowedActions(item.Status));
    }

    public async Task<OutcomeImportReviewDto> PatchAsync(
        long id,
        ReviewPatchRequest request,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        if (request.Operations.Count is < 1 or > 100)
            throw new OutcomeImportException(
                "INVALID_PATCH_SIZE", "Patch must contain between 1 and 100 operations.", 400);
        var batch = await FindBatchAsync(id, cancellationToken);
        EnsureEditable(batch);
        EnsureRowVersion(batch, request.RowVersion);
        var root = JsonNode.Parse(batch.ReviewedJson ?? string.Empty)
            ?? throw new OutcomeImportException("REVIEW_NOT_READY", "Review data is not available.", 409);
        foreach (var operation in request.Operations)
        {
            var target = FindDraft(root, operation.DraftId)
                ?? throw new OutcomeImportException(
                    "DRAFT_NOT_FOUND", $"Draft {operation.DraftId} was not found.", 404);
            var oldJson = target.ToJsonString();
            Apply(target, operation);
            dbContext.OutcomeReviewLogs.Add(new OutcomeReviewLog
            {
                ImportBatchId = batch.Id,
                EntityType = operation.EntityType,
                EntityDraftId = operation.DraftId,
                FieldName = operation.FieldName,
                OldValueJson = oldJson,
                NewValueJson = target.ToJsonString(),
                Action = operation.Action,
                Note = NullIfBlank(operation.Note),
                ActorExternalId = actor.ExternalId,
                ActorName = actor.Name
            });
        }

        var document = JsonSerializer.Deserialize<OutcomeDraftDocument>(root.ToJsonString(), JsonOptions)
            ?? throw new OutcomeImportException("INVALID_REVIEW_JSON", "Edited review JSON is invalid.", 400);
        var validation = await validationService.ValidateAsync(
            document, batch.CurriculumVersionId, cancellationToken);
        var blocks = await LoadBlocksAsync(id, cancellationToken);
        var quality = qualityEvaluator.Evaluate(
            document, blocks, validation, batch.SelectedSubjectCode);
        batch.ReviewedJson = JsonSerializer.Serialize(document, JsonOptions);
        batch.ValidationJson = JsonSerializer.Serialize(validation, JsonOptions);
        batch.QualityReportJson = JsonSerializer.Serialize(quality, JsonOptions);
        batch.Status = validation.Any(item => item.Severity == "BlockingError")
            ? OutcomeImportStatuses.ValidationFailed
            : OutcomeImportStatuses.PendingReview;
        batch.ReviewedByExternalId = actor.ExternalId;
        batch.ReviewedByName = actor.Name;
        batch.ReviewedAt = timeProvider.GetUtcNow().UtcDateTime;
        batch.UpdatedAt = batch.ReviewedAt.Value;
        SetOriginalRowVersion(batch, request.RowVersion);
        await SaveWithConcurrencyAsync(cancellationToken);
        return await ReviewAsync(id, cancellationToken);
    }

    public async Task<OutcomeImportReviewDto> ValidateAsync(
        long id,
        ValidateOutcomeImportRequest request,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        var batch = await FindBatchAsync(id, cancellationToken);
        EnsureEditable(batch);
        EnsureRowVersion(batch, request.RowVersion);
        var document = DeserializeDocument(batch.ReviewedJson)
            ?? throw new OutcomeImportException("REVIEW_NOT_READY", "Review data is not available.", 409);
        var validation = await validationService.ValidateAsync(
            document, batch.CurriculumVersionId, cancellationToken);
        var blocks = await LoadBlocksAsync(id, cancellationToken);
        var quality = qualityEvaluator.Evaluate(
            document, blocks, validation, batch.SelectedSubjectCode);
        batch.ReviewedJson = JsonSerializer.Serialize(document, JsonOptions);
        batch.ValidationJson = JsonSerializer.Serialize(validation, JsonOptions);
        batch.QualityReportJson = JsonSerializer.Serialize(quality, JsonOptions);
        batch.Status = validation.Any(item => item.Severity == "BlockingError")
            ? OutcomeImportStatuses.ValidationFailed
            : OutcomeImportStatuses.PendingReview;
        batch.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.OutcomeReviewLogs.Add(Log(batch.Id, "Batch", "batch", "Validate", actor));
        SetOriginalRowVersion(batch, request.RowVersion);
        await SaveWithConcurrencyAsync(cancellationToken);
        return await ReviewAsync(id, cancellationToken);
    }

    public async Task<OutcomeImportDetailDto> ApproveAsync(
        long id,
        ApproveOutcomeImportRequest request,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        if (!request.Confirmed)
            throw new OutcomeImportException(
                "APPROVAL_CONFIRMATION_REQUIRED", "Approval must be explicitly confirmed.", 400);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var batch = await dbContext.OutcomeImportBatches
            .FromSqlInterpolated(
                $"SELECT * FROM career.OutcomeImportBatch WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}")
            .Include(item => item.CurriculumVersion)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new OutcomeImportException("IMPORT_NOT_FOUND", "Import batch was not found.", 404);
        if (batch.Status == OutcomeImportStatuses.Approved) return Detail(batch);
        if (batch.Status != OutcomeImportStatuses.PendingReview)
            throw new OutcomeImportException(
                "INVALID_STATE", "Only a PendingReview batch can be approved.", 409);
        EnsureRowVersion(batch, request.RowVersion);
        var document = DeserializeDocument(batch.ReviewedJson)
            ?? throw new OutcomeImportException("REVIEW_NOT_READY", "Review data is not available.", 409);
        var validation = await validationService.ValidateAsync(
            document, batch.CurriculumVersionId, cancellationToken);
        if (validation.Any(item => item.Severity == "BlockingError"))
            throw new OutcomeImportException(
                "VALIDATION_FAILED", "Approval is blocked by validation errors.", 409);

        var ploIds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var draft in document.Plos.Where(item => !item.Removed))
        {
            var entity = new ProgramLearningOutcome
            {
                CurriculumVersionId = batch.CurriculumVersionId,
                PloCode = draft.PloCode.Trim(),
                Description = draft.Description.Trim(),
                OriginalDescription = NullIfBlank(draft.OriginalDescription),
                NormalizedDescription = Normalize(draft.Description),
                SortOrder = draft.SortOrder,
                Status = "Approved",
                UpdatedAt = timeProvider.GetUtcNow().UtcDateTime
            };
            dbContext.ProgramLearningOutcomes.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
            ploIds[draft.DraftId] = entity.Id;
        }

        var cloIds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var subject in document.Subjects.Where(item => !item.Removed))
        foreach (var draft in subject.Clos.Where(item => !item.Removed))
        {
            var entity = new CourseLearningOutcome
            {
                CurriculumVersionId = batch.CurriculumVersionId,
                SubjectExternalId = subject.Subject.ExternalId,
                SubjectCode = subject.Subject.Code.Trim(),
                SubjectName = subject.Subject.Name.Trim(),
                Credits = subject.Subject.Credits,
                CloCode = draft.CloCode.Trim(),
                Description = draft.Description.Trim(),
                OriginalDescription = NullIfBlank(draft.OriginalDescription),
                NormalizedDescription = Normalize(draft.Description),
                SortOrder = draft.SortOrder,
                Status = "Approved",
                UpdatedAt = timeProvider.GetUtcNow().UtcDateTime
            };
            dbContext.CourseLearningOutcomes.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
            cloIds[draft.DraftId] = entity.Id;
        }

        foreach (var draft in document.Mappings.Where(item => !item.Removed))
            dbContext.CloPloMappings.Add(new CloPloMapping
            {
                CurriculumVersionId = batch.CurriculumVersionId,
                CloId = cloIds[draft.CloDraftId],
                PloId = ploIds[draft.PloDraftId],
                ProgressionLevelCode = draft.ProgressionLevel,
                IsApproved = true,
                UpdatedAt = timeProvider.GetUtcNow().UtcDateTime
            });

        await dbContext.SaveChangesAsync(cancellationToken);
        await approvedDocumentService.MaterializeAsync(
            batch.CurriculumVersionId, batch.Id, actor, cancellationToken);

        batch.Status = OutcomeImportStatuses.Approved;
        batch.ApprovedByExternalId = actor.ExternalId;
        batch.ApprovedByName = actor.Name;
        batch.ApprovedAt = timeProvider.GetUtcNow().UtcDateTime;
        batch.UpdatedAt = batch.ApprovedAt.Value;
        batch.ValidationJson = JsonSerializer.Serialize(validation, JsonOptions);
        dbContext.OutcomeReviewLogs.Add(Log(batch.Id, "Batch", "batch", "Approve", actor));
        SetOriginalRowVersion(batch, request.RowVersion);
        await SaveWithConcurrencyAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Detail(batch);
    }

    public Task<ApprovedOutcomeDocumentDto> ApprovedDocumentAsync(
        long importBatchId,
        CancellationToken cancellationToken) =>
        approvedDocumentService.GetByImportBatchAsync(importBatchId, cancellationToken);

    public async Task<OutcomeImportDetailDto> RejectAsync(
        long id,
        RejectOutcomeImportRequest request,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new OutcomeImportException("REJECTION_REASON_REQUIRED", "Rejection reason is required.", 400);
        var batch = await FindBatchAsync(id, cancellationToken);
        EnsureRowVersion(batch, request.RowVersion);
        if (batch.Status is not (OutcomeImportStatuses.PendingReview or OutcomeImportStatuses.ValidationFailed))
            throw new OutcomeImportException("INVALID_STATE", "This batch cannot be rejected.", 409);
        batch.Status = OutcomeImportStatuses.Rejected;
        batch.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.OutcomeReviewLogs.Add(Log(
            batch.Id, "Batch", "batch", "Reject", actor, request.Reason.Trim()));
        SetOriginalRowVersion(batch, request.RowVersion);
        await SaveWithConcurrencyAsync(cancellationToken);
        return Detail(batch);
    }

    public async Task<OutcomeImportDetailDto> ArchiveAsync(
        long id,
        ArchiveOutcomeImportRequest request,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        var batch = await FindBatchAsync(id, cancellationToken);
        EnsureRowVersion(batch, request.RowVersion);
        if (batch.Status is not (OutcomeImportStatuses.Approved or OutcomeImportStatuses.Rejected or OutcomeImportStatuses.Failed))
            throw new OutcomeImportException("INVALID_STATE", "This batch cannot be archived.", 409);
        batch.Status = OutcomeImportStatuses.Archived;
        batch.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.OutcomeReviewLogs.Add(Log(batch.Id, "Batch", "batch", "Archive", actor));
        SetOriginalRowVersion(batch, request.RowVersion);
        await SaveWithConcurrencyAsync(cancellationToken);
        return Detail(batch);
    }

    private async Task<OutcomeImportBatch> FindBatchAsync(long id, CancellationToken cancellationToken) =>
        await dbContext.OutcomeImportBatches.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw new OutcomeImportException("IMPORT_NOT_FOUND", "Import batch was not found.", 404);

    private static OutcomeImportDetailDto Detail(OutcomeImportBatch item) =>
        new(item.Id, item.CurriculumVersionId,
            item.SelectedSubjectExternalId, item.SelectedSubjectCode, item.SelectedSubjectName,
            item.OriginalFileName, item.FileSize,
            item.Status, item.ProcessingStage, item.ProgressPercent,
            item.ErrorCode, item.ErrorMessage,
            item.Status == OutcomeImportStatuses.Failed &&
            item.ErrorCode is not ("INVALID_DOCX" or "DOCUMENT_EMPTY"),
            item.CreatedAt, item.UpdatedAt, RowVersionCodec.Encode(item.RowVersion));

    private static IReadOnlyList<string> AllowedActions(string status) => status switch
    {
        OutcomeImportStatuses.PendingReview => ["edit", "validate", "approve", "reject"],
        OutcomeImportStatuses.ValidationFailed => ["edit", "validate", "reject"],
        OutcomeImportStatuses.Failed => ["process", "archive"],
        OutcomeImportStatuses.Approved => ["archive"],
        OutcomeImportStatuses.Rejected => ["archive"],
        _ => []
    };

    private static OutcomeDraftDocument? DeserializeDocument(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<OutcomeDraftDocument>(json, JsonOptions);

    private static IReadOnlyList<OutcomeWarning> DeserializeValidation(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<OutcomeWarning>>(json, JsonOptions) ?? [];

    private static ExtractionQualityReport? DeserializeQuality(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<ExtractionQualityReport>(json, JsonOptions);

    private Task<List<DocumentBlockData>> LoadBlocksAsync(
        long importBatchId,
        CancellationToken cancellationToken) =>
        dbContext.OutcomeDocumentBlocks.AsNoTracking()
            .Where(block => block.ImportBatchId == importBatchId)
            .OrderBy(block => block.Sequence)
            .Select(block => new DocumentBlockData(
                block.BlockId, block.BlockType, block.Sequence,
                block.HeadingLevel, block.ContentJson))
            .ToListAsync(cancellationToken);

    private static void EnsureEditable(OutcomeImportBatch batch)
    {
        if (batch.Status is not (OutcomeImportStatuses.PendingReview or OutcomeImportStatuses.ValidationFailed))
            throw new OutcomeImportException("INVALID_STATE", "This batch is not editable.", 409);
    }

    private static void EnsureRowVersion(OutcomeImportBatch batch, string encoded)
    {
        if (!batch.RowVersion.SequenceEqual(RowVersionCodec.Decode(encoded)))
            throw new OutcomeImportException(
                "CONCURRENT_MODIFICATION",
                "The import was updated by another request. Reload before continuing.", 409);
    }

    private void SetOriginalRowVersion(OutcomeImportBatch batch, string encoded) =>
        dbContext.Entry(batch).Property(item => item.RowVersion).OriginalValue =
            RowVersionCodec.Decode(encoded);

    private async Task SaveWithConcurrencyAsync(CancellationToken cancellationToken)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new OutcomeImportException(
                "CONCURRENT_MODIFICATION",
                "The import was updated by another request. Reload before continuing.", 409, exception);
        }
        catch (DbUpdateException exception)
        {
            throw new OutcomeImportException(
                "DATABASE_CONFLICT", "The requested operation conflicts with current data.", 409, exception);
        }
    }

    private static JsonObject? FindDraft(JsonNode node, string draftId)
    {
        if (node is JsonObject current &&
            string.Equals(current["draftId"]?.GetValue<string>(), draftId, StringComparison.OrdinalIgnoreCase))
            return current;
        foreach (var child in node.AsObjectOrArrayChildren())
        {
            var found = FindDraft(child, draftId);
            if (found is not null) return found;
        }
        return null;
    }

    private static void Apply(JsonObject target, ReviewPatchOperation operation)
    {
        if (operation.Action == "Remove")
        {
            if (operation.EntityType is not ("PLO" or "CLO" or "Subject" or "Mapping"))
                throw new OutcomeImportException(
                    "INVALID_PATCH_ENTITY", "This entity cannot be removed.", 400);
            target["removed"] = true;
            return;
        }
        if (operation.Action == "Restore")
        {
            if (operation.EntityType is not ("PLO" or "CLO" or "Subject" or "Mapping"))
                throw new OutcomeImportException(
                    "INVALID_PATCH_ENTITY", "This entity cannot be restored.", 400);
            target["removed"] = false;
            return;
        }
        if (operation.Action != "Edit" || string.IsNullOrWhiteSpace(operation.FieldName) ||
            !operation.Value.HasValue)
            throw new OutcomeImportException("INVALID_PATCH", "Patch operation is invalid.", 400);
        if (!EditableFields.TryGetValue(operation.EntityType, out var allowedFields) ||
            !allowedFields.Contains(operation.FieldName))
            throw new OutcomeImportException(
                "INVALID_PATCH_FIELD",
                $"Field {operation.FieldName} is not editable for {operation.EntityType}.", 400);
        var segments = operation.FieldName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        JsonObject owner = target;
        foreach (var segment in segments[..^1])
            owner = owner[segment] as JsonObject
                ?? throw new OutcomeImportException("INVALID_PATCH_FIELD", $"Field {operation.FieldName} was not found.", 400);
        owner[segments[^1]] = JsonNode.Parse(operation.Value.Value.GetRawText());
    }

    private static readonly IReadOnlyDictionary<string, HashSet<string>> EditableFields =
        new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            ["Curriculum"] = ["majorCode", "majorName", "curriculumCode", "curriculumName", "version"],
            ["PLO"] = ["ploCode", "description", "originalDescription", "sortOrder"],
            ["Subject"] = ["subject.code", "subject.name", "subject.credits"],
            ["CLO"] = ["cloCode", "description", "originalDescription", "sortOrder"],
            ["Mapping"] = ["progressionLevel"],
        };

    private static OutcomeReviewLog Log(
        long batchId,
        string entityType,
        string draftId,
        string action,
        ActorContext actor,
        string? note = null) => new()
        {
            ImportBatchId = batchId,
            EntityType = entityType,
            EntityDraftId = draftId,
            Action = action,
            Note = note,
            ActorExternalId = actor.ExternalId,
            ActorName = actor.Name
        };

    private static (int Plos, int Clos, int Mappings) Counts(string? json)
    {
        var document = DeserializeDocument(json);
        return document is null
            ? (0, 0, 0)
            : (
                document.Plos.Count(item => !item.Removed),
                document.Subjects.Where(item => !item.Removed)
                    .SelectMany(item => item.Clos).Count(item => !item.Removed),
                document.Mappings.Count(item => !item.Removed));
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Normalize(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    private static void Required(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new OutcomeImportException("VALIDATION_ERROR", $"{field} is required.", 400);
    }
}

internal static class JsonNodeTraversal
{
    public static IEnumerable<JsonNode> AsObjectOrArrayChildren(this JsonNode node) =>
        node switch
        {
            JsonObject obj => obj.Select(item => item.Value).OfType<JsonNode>(),
            JsonArray array => array.OfType<JsonNode>(),
            _ => []
        };
}

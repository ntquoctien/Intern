using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CareerService.Infrastructure.Persistence;
using CareerService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerService.Application;

public sealed class ApprovedOutcomeDocumentService(
    CareerDbContext dbContext,
    TimeProvider timeProvider)
{
    public const string SchemaVersion = "1.0";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ApprovedOutcomeJsonDocument> MaterializeAsync(
        long curriculumVersionId,
        long importBatchId,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        var existingForBatch = await dbContext.ApprovedOutcomeJsonDocuments
            .SingleOrDefaultAsync(item => item.ImportBatchId == importBatchId, cancellationToken);
        if (existingForBatch is not null)
            return existingForBatch;

        var content = await BuildContentFromSqlAsync(curriculumVersionId, cancellationToken);
        var contentJson = SerializeCanonical(content);
        var contentHash = Hash(contentJson);

        var previousDocuments = await dbContext.ApprovedOutcomeJsonDocuments
            .Where(item => item.CurriculumVersionId == curriculumVersionId)
            .OrderByDescending(item => item.DocumentVersion)
            .ToListAsync(cancellationToken);
        var generatedAt = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var active in previousDocuments.Where(item =>
                     item.Status == ApprovedOutcomeDocumentStatuses.Active))
        {
            active.Status = ApprovedOutcomeDocumentStatuses.Superseded;
            active.SupersededAt = generatedAt;
        }

        if (previousDocuments.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        var document = new ApprovedOutcomeJsonDocument
        {
            CurriculumVersionId = curriculumVersionId,
            ImportBatchId = importBatchId,
            SchemaVersion = SchemaVersion,
            DocumentVersion = previousDocuments.Count == 0
                ? 1
                : checked(previousDocuments.Max(item => item.DocumentVersion) + 1),
            ContentJson = contentJson,
            ContentHash = contentHash,
            Status = ApprovedOutcomeDocumentStatuses.Active,
            GeneratedAt = generatedAt,
            GeneratedByExternalId = actor.ExternalId,
            GeneratedByName = actor.Name,
            RowVersion = [0]
        };
        dbContext.ApprovedOutcomeJsonDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task<ApprovedOutcomeDocumentDto> GetByImportBatchAsync(
        long importBatchId,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.ApprovedOutcomeJsonDocuments.AsNoTracking()
            .SingleOrDefaultAsync(item => item.ImportBatchId == importBatchId, cancellationToken)
            ?? throw new OutcomeImportException(
                "APPROVED_DOCUMENT_NOT_FOUND",
                "Approved outcome JSON document was not found for this import.",
                404);
        return ToDto(document);
    }

    public async Task<ApprovedOutcomeContent> BuildContentFromSqlAsync(
        long curriculumVersionId,
        CancellationToken cancellationToken)
    {
        var curriculum = await dbContext.CurriculumVersions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == curriculumVersionId, cancellationToken)
            ?? throw new OutcomeImportException(
                "CURRICULUM_NOT_FOUND", "Target curriculum was not found.", 404);
        var plos = await dbContext.ProgramLearningOutcomes.AsNoTracking()
            .Where(item =>
                item.CurriculumVersionId == curriculumVersionId &&
                item.Status == "Approved")
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.PloCode)
            .ToListAsync(cancellationToken);
        var clos = await dbContext.CourseLearningOutcomes.AsNoTracking()
            .Where(item =>
                item.CurriculumVersionId == curriculumVersionId &&
                item.Status == "Approved")
            .OrderBy(item => item.SubjectCode)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.CloCode)
            .ToListAsync(cancellationToken);
        var mappings = await (
                from mapping in dbContext.CloPloMappings.AsNoTracking()
                join clo in dbContext.CourseLearningOutcomes.AsNoTracking()
                    on mapping.CloId equals clo.Id
                join plo in dbContext.ProgramLearningOutcomes.AsNoTracking()
                    on mapping.PloId equals plo.Id
                where mapping.CurriculumVersionId == curriculumVersionId && mapping.IsApproved
                orderby clo.SubjectCode, clo.CloCode, plo.PloCode, mapping.ProgressionLevelCode
                select new ApprovedMappingContent(
                    clo.SubjectCode,
                    clo.CloCode,
                    plo.PloCode,
                    mapping.ProgressionLevelCode))
            .ToListAsync(cancellationToken);

        return BuildCanonicalContent(curriculum, plos, clos, mappings);
    }

    public static ApprovedOutcomeContent BuildCanonicalContent(
        CurriculumVersion curriculum,
        IEnumerable<ProgramLearningOutcome> plos,
        IEnumerable<CourseLearningOutcome> clos,
        IEnumerable<ApprovedMappingContent> mappings)
    {
        var canonicalPlos = plos
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.PloCode, StringComparer.Ordinal)
            .Select(item => new ApprovedPloContent(
                item.PloCode.Trim(),
                item.Description.Trim(),
                NullIfBlank(item.OriginalDescription),
                item.SortOrder))
            .ToList();
        var canonicalSubjects = clos
            .GroupBy(
                item => new
                {
                    ExternalId = item.SubjectExternalId,
                    Code = item.SubjectCode.Trim(),
                    Name = item.SubjectName.Trim(),
                    item.Credits
                })
            .OrderBy(group => group.Key.Code, StringComparer.Ordinal)
            .ThenBy(group => group.Key.Name, StringComparer.Ordinal)
            .Select(group => new ApprovedSubjectContent(
                group.Key.ExternalId,
                group.Key.Code,
                group.Key.Name,
                group.Key.Credits,
                group
                    .OrderBy(item => item.SortOrder)
                    .ThenBy(item => item.CloCode, StringComparer.Ordinal)
                    .Select(item => new ApprovedCloContent(
                        item.CloCode.Trim(),
                        item.Description.Trim(),
                        NullIfBlank(item.OriginalDescription),
                        item.SortOrder))
                    .ToList()))
            .ToList();
        var canonicalMappings = mappings
            .OrderBy(item => item.SubjectCode, StringComparer.Ordinal)
            .ThenBy(item => item.CloCode, StringComparer.Ordinal)
            .ThenBy(item => item.PloCode, StringComparer.Ordinal)
            .ThenBy(item => item.ProgressionLevel, StringComparer.Ordinal)
            .Select(item => item with
            {
                SubjectCode = item.SubjectCode.Trim(),
                CloCode = item.CloCode.Trim(),
                PloCode = item.PloCode.Trim(),
                ProgressionLevel = item.ProgressionLevel.Trim()
            })
            .ToList();

        return new ApprovedOutcomeContent(
            SchemaVersion,
            new ApprovedCurriculumContent(
                curriculum.MajorCode.Trim(),
                curriculum.MajorName.Trim(),
                curriculum.CurriculumCode.Trim(),
                NullIfBlank(curriculum.CurriculumName),
                curriculum.Version.Trim()),
            canonicalPlos,
            canonicalSubjects,
            canonicalMappings);
    }

    public static string SerializeCanonical(ApprovedOutcomeContent content) =>
        JsonSerializer.Serialize(content, JsonOptions);

    public static string Hash(string contentJson) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(contentJson)))
            .ToLowerInvariant();

    private static ApprovedOutcomeDocumentDto ToDto(ApprovedOutcomeJsonDocument item) =>
        new(
            item.Id,
            item.CurriculumVersionId,
            item.ImportBatchId,
            item.SchemaVersion,
            item.DocumentVersion,
            JsonSerializer.Deserialize<JsonElement>(item.ContentJson, JsonOptions),
            item.ContentHash,
            item.Status,
            item.GeneratedAt,
            item.GeneratedByExternalId,
            item.GeneratedByName,
            item.SupersededAt,
            RowVersionCodec.Encode(item.RowVersion));

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

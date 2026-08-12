using System.Text.Json;

namespace CareerService.Application;

public static class OutcomeImportStatuses
{
    public const string Uploaded = "Uploaded";
    public const string Processing = "Processing";
    public const string PendingReview = "PendingReview";
    public const string ValidationFailed = "ValidationFailed";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Failed = "Failed";
    public const string Archived = "Archived";
}

public static class OutcomeProcessingStages
{
    public const string Waiting = "Waiting";
    public const string ParsingDocument = "ParsingDocument";
    public const string ExtractingProgram = "ExtractingProgram";
    public const string ExtractingOutcomes = "ExtractingOutcomes";
    public const string ExtractingMatrix = "ExtractingMatrix";
    public const string Reconciling = "Reconciling";
    public const string Validating = "Validating";
    public const string Completed = "Completed";
}

public static class ApprovedOutcomeDocumentStatuses
{
    public const string Active = "Active";
    public const string Superseded = "Superseded";
}

public sealed record SourceReference(
    string BlockId,
    string ElementType,
    int Sequence,
    IReadOnlyList<string> HeadingPath,
    int? ParagraphIndex,
    int? TableIndex,
    int? RowIndex,
    int? ColumnIndex,
    string Excerpt);

public sealed record OutcomeWarning(
    string Code,
    string Severity,
    string? EntityDraftId,
    string? FieldName,
    string Message);

public sealed class OutcomeDraftDocument
{
    public string SchemaVersion { get; set; } = "1.0";
    public long ImportBatchId { get; set; }
    public CurriculumDraft Curriculum { get; set; } = new();
    public List<PloDraft> Plos { get; set; } = [];
    public List<SubjectDraft> Subjects { get; set; } = [];
    public List<MappingDraft> Mappings { get; set; } = [];
    public List<JsonElement> UnresolvedItems { get; set; } = [];
    public List<OutcomeWarning> Warnings { get; set; } = [];
}

public abstract class DraftItem
{
    public string DraftId { get; set; } = string.Empty;
    public List<SourceReference> SourceReferences { get; set; } = [];
    public double Confidence { get; set; }
    public List<OutcomeWarning> Warnings { get; set; } = [];
    public bool Removed { get; set; }
}

public sealed class CurriculumDraft : DraftItem
{
    public string? MajorCode { get; set; }
    public string? MajorName { get; set; }
    public string? CurriculumCode { get; set; }
    public string? CurriculumName { get; set; }
    public string? Version { get; set; }
}

public sealed class PloDraft : DraftItem
{
    public string PloCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OriginalDescription { get; set; }
    public int SortOrder { get; set; }
}

public sealed class SubjectDraft : DraftItem
{
    public SubjectReference Subject { get; set; } = new();
    public List<CloDraft> Clos { get; set; } = [];
}

public sealed class SubjectReference
{
    public Guid? ExternalId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Credits { get; set; }
    public string MatchStatus { get; set; } = "NotChecked";
}

public sealed class CloDraft : DraftItem
{
    public string CloCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OriginalDescription { get; set; }
    public int SortOrder { get; set; }
}

public sealed class MappingDraft : DraftItem
{
    public string CloDraftId { get; set; } = string.Empty;
    public string PloDraftId { get; set; } = string.Empty;
    public string ProgressionLevel { get; set; } = string.Empty;
}

public sealed record DocumentBlockData(
    string BlockId,
    string BlockType,
    int Sequence,
    byte? HeadingLevel,
    string ContentJson);

public sealed record CurriculumOptionDto(
    long Id,
    Guid? MajorExternalId,
    string MajorCode,
    string MajorName,
    string CurriculumCode,
    string? CurriculumName,
    string Version,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status);

public sealed record CreateCurriculumRequest(
    Guid? MajorExternalId,
    string MajorCode,
    string MajorName,
    string CurriculumCode,
    string? CurriculumName,
    string Version,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    string? Description);

public sealed record OutcomeImportListItemDto(
    long Id,
    long CurriculumVersionId,
    Guid? SelectedSubjectExternalId,
    string? SelectedSubjectCode,
    string? SelectedSubjectName,
    string MajorCode,
    string MajorName,
    string CurriculumCode,
    string Version,
    string OriginalFileName,
    long FileSize,
    string Status,
    string? ProcessingStage,
    byte ProgressPercent,
    int PloCount,
    int CloCount,
    int MappingCount,
    string UploadedByName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record OutcomeImportDetailDto(
    long Id,
    long CurriculumVersionId,
    Guid? SelectedSubjectExternalId,
    string? SelectedSubjectCode,
    string? SelectedSubjectName,
    string OriginalFileName,
    long FileSize,
    string Status,
    string? ProcessingStage,
    byte ProgressPercent,
    string? ErrorCode,
    string? ErrorMessage,
    bool IsRetryable,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion);

public sealed record OutcomeImportReviewDto(
    OutcomeImportDetailDto Import,
    OutcomeDraftDocument? Document,
    IReadOnlyList<OutcomeWarning> Validation,
    ExtractionQualityReport? QualityReport,
    IReadOnlyList<DocumentBlockData> Blocks,
    IReadOnlyList<string> AllowedActions);

public sealed record ExtractionQualityReport(
    double ScopeCompliance,
    double SourceReferenceCoverage,
    int CloCountSource,
    int CloCountReviewed,
    double ExactCloCodeMatchRate,
    double ExactPloCodeMatchRate,
    double TextOverlapAverage,
    int HallucinatedSubjectCount,
    int RemovedOutOfScopeSubjectCount,
    int DanglingMappingCount,
    int DuplicateMappingCount,
    IReadOnlyList<string> BlockingErrors,
    IReadOnlyList<string> Warnings);

public sealed record ApprovedOutcomeDocumentDto(
    long Id,
    long CurriculumVersionId,
    long ImportBatchId,
    string SchemaVersion,
    int DocumentVersion,
    JsonElement Content,
    string ContentHash,
    string Status,
    DateTime GeneratedAt,
    string GeneratedByExternalId,
    string GeneratedByName,
    DateTime? SupersededAt,
    string RowVersion);

public sealed record ApprovedOutcomeContent(
    string SchemaVersion,
    ApprovedCurriculumContent Curriculum,
    IReadOnlyList<ApprovedPloContent> Plos,
    IReadOnlyList<ApprovedSubjectContent> Subjects,
    IReadOnlyList<ApprovedMappingContent> Mappings);

public sealed record ApprovedCurriculumContent(
    string MajorCode,
    string MajorName,
    string CurriculumCode,
    string? CurriculumName,
    string Version);

public sealed record ApprovedPloContent(
    string PloCode,
    string Description,
    string? OriginalDescription,
    int SortOrder);

public sealed record ApprovedSubjectContent(
    Guid? ExternalId,
    string SubjectCode,
    string SubjectName,
    int? Credits,
    IReadOnlyList<ApprovedCloContent> Clos);

public sealed record ApprovedCloContent(
    string CloCode,
    string Description,
    string? OriginalDescription,
    int SortOrder);

public sealed record ApprovedMappingContent(
    string SubjectCode,
    string CloCode,
    string PloCode,
    string ProgressionLevel);

public sealed record UploadOutcomeImportResponse(
    long Id,
    string Status,
    string FileName,
    string RowVersion);

public sealed record ReviewPatchRequest(
    string RowVersion,
    IReadOnlyList<ReviewPatchOperation> Operations);

public sealed record ReviewPatchOperation(
    string EntityType,
    string DraftId,
    string? FieldName,
    JsonElement? Value,
    string Action = "Edit",
    string? Note = null);

public sealed record ApproveOutcomeImportRequest(string RowVersion, bool Confirmed);
public sealed record ValidateOutcomeImportRequest(string RowVersion);
public sealed record RejectOutcomeImportRequest(string RowVersion, string Reason);
public sealed record ArchiveOutcomeImportRequest(string RowVersion);

public sealed record ActorContext(string ExternalId, string Name);

public static class RowVersionCodec
{
    public static string Encode(byte[] rowVersion) => Convert.ToBase64String(rowVersion);

    public static byte[] Decode(string value)
    {
        try { return Convert.FromBase64String(value); }
        catch (FormatException exception)
        {
            throw new OutcomeImportException("INVALID_ROW_VERSION", "RowVersion is not valid base64.", 400, exception);
        }
    }
}

public sealed class OutcomeImportException(
    string errorCode,
    string message,
    int statusCode,
    Exception? innerException = null,
    IReadOnlyDictionary<string, object?>? extensions = null) : Exception(message, innerException)
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
    public IReadOnlyDictionary<string, object?> Extensions { get; } =
        extensions ?? new Dictionary<string, object?>();
}

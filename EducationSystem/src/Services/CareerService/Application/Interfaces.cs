using CareerService.Application.DTOs.Resume;

namespace CareerService.Application;

public interface IOutcomeFileStorage
{
    Task SaveAsync(string storageKey, ReadOnlyMemory<byte> content, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);
    Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken);
}

public interface IOutcomeDocumentParser
{
    Task<IReadOnlyList<DocumentBlockData>> ParseAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken);
}

public sealed record OutcomeExtractionProviderRequest(
    string Task,
    string PromptVersion,
    string InputJson,
    int RequestNumber);

public sealed record OutcomeExtractionProviderResult(
    string Json,
    string? ProviderRequestId);

public interface IOutcomeExtractionProvider
{
    Task<OutcomeExtractionProviderResult> ExtractAsync(
        OutcomeExtractionProviderRequest request,
        CancellationToken cancellationToken);
}

public interface IOutcomeValidationService
{
    Task<IReadOnlyList<OutcomeWarning>> ValidateAsync(
        OutcomeDraftDocument document,
        long curriculumVersionId,
        CancellationToken cancellationToken);
}

public interface IOutcomeImportProcessor
{
    Task<bool> ProcessNextAsync(CancellationToken cancellationToken);
    Task ProcessAsync(long batchId, CancellationToken cancellationToken);
}

public interface ILlmResumeGeneratorService
{
    Task<OptimizedResumeResponseDto> GenerateOptimizedResumeAsync(
        ResumePromptPayloadDto promptPayload,
        CancellationToken cancellationToken);
}

public interface IResumeContextHydrationService
{
    Task<ResumePromptPayloadDto> HydrateResumeContextAsync(
        PrepareResumePayloadRequestDto request,
        Guid authenticatedStudentId,
        Guid? authenticatedUserId,
        string bearerToken,
        CancellationToken cancellationToken);
}

public interface IPdfOcrService
{
    Task<IReadOnlyList<DocumentBlockData>> RecognizeAsync(
        ReadOnlyMemory<byte> pdf,
        CancellationToken cancellationToken);
}

public interface IAiOutcomeMappingProvider
{
    Task<OutcomeExtractionProviderResult> MapAsync(
        OutcomeExtractionProviderRequest request,
        CancellationToken cancellationToken);
}

public interface IVectorMatchClient
{
    Task<VectorMatchResponseContract> MatchAsync(
        VectorMatchRequestContract request,
        CancellationToken cancellationToken);
}

public interface IAcademicResumeClient
{
    Task<AcademicResumeContextContract> GetContextAsync(
        Guid studentId,
        string bearerToken,
        CancellationToken cancellationToken);
}

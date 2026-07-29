namespace CareerService.Application;

public interface IOutcomeFileStorage
{
    Task SaveAsync(string storageKey, ReadOnlyMemory<byte> content, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);
    Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken);
}

public interface IOutcomeDocumentParser
{
    Task<IReadOnlyList<DocumentBlockData>> ParseAsync(Stream stream, CancellationToken cancellationToken);
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

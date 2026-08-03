using CareerService.Application;

namespace CareerService.Infrastructure;

// Text extraction is intentionally delegated to the configured OCR provider.
// Keeping this adapter separate allows a native PDF text extractor to be added
// later without changing the document pipeline contract.
public sealed class PdfOutcomeDocumentParser
{
    public Task<IReadOnlyList<DocumentBlockData>> ParseAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<DocumentBlockData>>([]);
    }
}

using CareerService.Application;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class LocalOutcomeFileStorage : IOutcomeFileStorage
{
    private readonly string rootPath;

    public LocalOutcomeFileStorage(
        IOptions<OutcomeStorageOptions> options,
        IWebHostEnvironment environment)
    {
        var configured = options.Value.RootPath;
        rootPath = Path.GetFullPath(
            string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(environment.ContentRootPath, "..", "..", "..", ".data", "outcome-imports")
                : Path.IsPathRooted(configured)
                    ? configured
                    : Path.Combine(environment.ContentRootPath, configured));
        Directory.CreateDirectory(rootPath);
    }

    public async Task SaveAsync(
        string storageKey,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken)
    {
        var path = Resolve(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous | FileOptions.WriteThrough);
        await stream.WriteAsync(content, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            Resolve(storageKey), FileMode.Open, FileAccess.Read, FileShare.Read,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string Resolve(string storageKey)
    {
        var normalized = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(rootPath, normalized));
        var requiredPrefix = rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage key escapes the configured root.");
        return path;
    }
}

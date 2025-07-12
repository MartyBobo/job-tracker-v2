namespace JobTracker.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, Guid userId, CancellationToken cancellationToken = default);
    Task<FileDownloadResult?> DownloadAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);
    string GetFileUrl(string filePath);
    Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);
}

public record FileUploadResult(
    string FilePath,
    string FileName,
    long FileSize,
    string FileUrl,
    string ContentType
);

public record FileDownloadResult(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize
);
using JobTracker.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobTracker.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IConfiguration _configuration;

    public LocalFileStorageService(
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Get base directory from configuration or use default
        _baseDirectory = configuration["FileStorage:BasePath"] ?? 
                        Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        
        // Ensure directory exists
        Directory.CreateDirectory(_baseDirectory);
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream fileStream, 
        string fileName, 
        string contentType, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create user-specific directory
            var userDirectory = Path.Combine(_baseDirectory, userId.ToString());
            Directory.CreateDirectory(userDirectory);

            // Generate unique filename to prevent collisions
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(userDirectory, uniqueFileName);

            // Save file
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs, cancellationToken);
            }

            // Get relative path for storage
            var relativePath = Path.Combine(userId.ToString(), uniqueFileName);

            // Get file info
            var fileInfo = new FileInfo(filePath);

            _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);

            return new FileUploadResult(
                FilePath: relativePath,
                FileName: fileName,
                FileSize: fileInfo.Length,
                FileUrl: GetFileUrl(relativePath),
                ContentType: contentType
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<FileDownloadResult?> DownloadAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, filePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return null;
            }

            var fileInfo = new FileInfo(fullPath);
            var fileName = Path.GetFileName(filePath);
            
            // Try to determine content type
            var contentType = GetContentType(fileName);

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            return new FileDownloadResult(
                FileStream: fileStream,
                FileName: fileName,
                ContentType: contentType,
                FileSize: fileInfo.Length
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_baseDirectory, filePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return false;
            }

            await Task.Run(() => File.Delete(fullPath), cancellationToken);
            
            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, filePath);
        return await Task.FromResult(File.Exists(fullPath));
    }

    public string GetFileUrl(string filePath)
    {
        // In a local storage scenario, we'll return a relative URL
        // This would be served by the API's static file middleware
        return $"/files/{filePath.Replace('\\', '/')}";
    }

    public async Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, filePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var fileInfo = new FileInfo(fullPath);
        return await Task.FromResult(fileInfo.Length);
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".html" => "text/html",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
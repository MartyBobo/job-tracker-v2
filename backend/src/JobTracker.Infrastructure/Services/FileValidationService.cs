using JobTracker.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace JobTracker.Infrastructure.Services;

public class FileValidationService : IFileValidationService
{
    private readonly FileValidationOptions _options;

    public FileValidationService(IOptions<FileValidationOptions> options)
    {
        _options = options.Value;
    }

    public bool IsValidFileExtension(string fileName, string[] allowedExtensions)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    public bool IsValidFileSize(long fileSize, long maxSizeInBytes)
    {
        return fileSize > 0 && fileSize <= maxSizeInBytes;
    }

    public bool IsValidContentType(string contentType, string[] allowedContentTypes)
    {
        return allowedContentTypes.Contains(contentType.ToLowerInvariant());
    }

    public (bool IsValid, string? ErrorMessage) ValidateFile(string fileName, long fileSize, string contentType)
    {
        // Check file extension
        if (!IsValidFileExtension(fileName, _options.AllowedExtensions))
        {
            var allowedExtensions = string.Join(", ", _options.AllowedExtensions);
            return (false, $"Invalid file extension. Allowed extensions: {allowedExtensions}");
        }

        // Check file size
        if (!IsValidFileSize(fileSize, _options.MaxFileSizeInBytes))
        {
            var maxSizeInMB = _options.MaxFileSizeInBytes / (1024 * 1024);
            return (false, $"File size exceeds maximum allowed size of {maxSizeInMB}MB");
        }

        // Check content type
        if (!IsValidContentType(contentType, _options.AllowedContentTypes))
        {
            return (false, "Invalid file type");
        }

        return (true, null);
    }
}
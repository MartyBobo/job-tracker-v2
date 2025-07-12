namespace JobTracker.Application.Interfaces;

public interface IFileValidationService
{
    bool IsValidFileExtension(string fileName, string[] allowedExtensions);
    bool IsValidFileSize(long fileSize, long maxSizeInBytes);
    bool IsValidContentType(string contentType, string[] allowedContentTypes);
    (bool IsValid, string? ErrorMessage) ValidateFile(string fileName, long fileSize, string contentType);
}

public class FileValidationOptions
{
    public string[] AllowedExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
    public string[] AllowedContentTypes { get; set; } = 
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain",
        "image/jpeg",
        "image/png"
    };
    public long MaxFileSizeInBytes { get; set; } = 10 * 1024 * 1024; // 10MB default
}
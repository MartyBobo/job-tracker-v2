using JobTracker.Domain.Enums;

namespace JobTracker.Domain.Entities;

public class Upload : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; // Changed from MimeType
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; } // Changed from UploadType
    public string? Description { get; set; } // Added Description
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual JobApplication? Application { get; set; }
}
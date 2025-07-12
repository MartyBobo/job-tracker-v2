namespace Shared.Errors;

public static class ErrorCodes
{
    // Authentication
    public const string Unauthorized = "Auth.Unauthorized";
    public const string InvalidCredentials = "Auth.InvalidCredentials";
    public const string UserNotFound = "Auth.UserNotFound";
    
    // Validation
    public const string ValidationFailed = "Validation.Failed";
    public const string InvalidRequest = "Validation.InvalidRequest";
    
    // Job Application
    public const string JobApplicationNotFound = "JobApplication.NotFound";
    public const string JobApplicationCreateFailed = "JobApplication.CreateFailed";
    public const string JobApplicationUpdateFailed = "JobApplication.UpdateFailed";
    public const string JobApplicationDeleteFailed = "JobApplication.DeleteFailed";
    
    // Interview
    public const string InterviewNotFound = "Interview.NotFound";
    public const string InterviewCreateFailed = "Interview.CreateFailed";
    public const string InterviewUpdateFailed = "Interview.UpdateFailed";
    public const string InterviewDeleteFailed = "Interview.DeleteFailed";
    public const string InterviewScheduleConflict = "Interview.ScheduleConflict";
    
    // File Upload
    public const string FileNotFound = "File.NotFound";
    public const string FileUploadFailed = "File.UploadFailed";
    public const string FileTooLarge = "File.TooLarge";
    public const string FileTypeNotAllowed = "File.TypeNotAllowed";
    public const string StorageQuotaExceeded = "File.StorageQuotaExceeded";
    public const string InvalidFileType = "File.InvalidType";
    
    // General
    public const string InternalServerError = "General.InternalServerError";
    public const string NotFound = "General.NotFound";
    public const string Conflict = "General.Conflict";
    public const string Forbidden = "General.Forbidden";
}
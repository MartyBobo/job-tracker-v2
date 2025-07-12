namespace JobTracker.Application.Interfaces;

public interface IResumeGenerationService
{
    Task<byte[]> GeneratePdfAsync(string resumeHtml, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateDocxAsync(string resumeHtml, CancellationToken cancellationToken = default);
    string GenerateHtmlFromTemplate(object templateData);
}
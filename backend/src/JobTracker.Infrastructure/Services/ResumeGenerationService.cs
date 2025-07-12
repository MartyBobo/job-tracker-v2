using JobTracker.Application.Interfaces;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace JobTracker.Infrastructure.Services;

public class ResumeGenerationService : IResumeGenerationService
{
    private readonly ILogger<ResumeGenerationService> _logger;

    public ResumeGenerationService(ILogger<ResumeGenerationService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfAsync(string resumeHtml, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would use a library like PuppeteerSharp or similar
        // For now, we'll just return the HTML as bytes
        _logger.LogWarning("PDF generation not implemented. Returning HTML as fallback.");
        return await Task.FromResult(Encoding.UTF8.GetBytes(resumeHtml));
    }

    public async Task<byte[]> GenerateDocxAsync(string resumeHtml, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would use DocumentFormat.OpenXml or similar
        // For now, we'll just return the HTML as bytes
        _logger.LogWarning("DOCX generation not implemented. Returning HTML as fallback.");
        return await Task.FromResult(Encoding.UTF8.GetBytes(resumeHtml));
    }

    public string GenerateHtmlFromTemplate(object templateData)
    {
        // Parse the template data
        dynamic data = JsonSerializer.Deserialize<dynamic>(JsonSerializer.Serialize(templateData))!;
        
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='UTF-8'>");
        html.AppendLine("<title>Resume</title>");
        html.AppendLine("<style>");
        html.AppendLine(@"
            body { font-family: Arial, sans-serif; line-height: 1.6; margin: 40px; }
            h1 { color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }
            h2 { color: #555; margin-top: 25px; }
            h3 { color: #666; }
            .contact { margin-bottom: 20px; }
            .section { margin-bottom: 25px; }
            .experience-item, .education-item { margin-bottom: 20px; }
            .skills { display: flex; flex-wrap: wrap; gap: 10px; }
            .skill { background: #f0f0f0; padding: 5px 10px; border-radius: 3px; }
            ul { margin: 10px 0; }
            .date { color: #666; font-style: italic; }
        ");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Contact Information
        if (data.TryGetProperty("contact", out JsonElement contact))
        {
            html.AppendLine("<div class='contact'>");
            html.AppendLine($"<h1>{GetValue(contact, "fullName")}</h1>");
            html.AppendLine($"<div>{GetValue(contact, "email")} | {GetValue(contact, "phone")} | {GetValue(contact, "location")}</div>");
            
            var links = new List<string>();
            if (TryGetValue(contact, "linkedIn", out var linkedin) && !string.IsNullOrEmpty(linkedin))
                links.Add($"LinkedIn: {linkedin}");
            if (TryGetValue(contact, "gitHub", out var github) && !string.IsNullOrEmpty(github))
                links.Add($"GitHub: {github}");
            if (TryGetValue(contact, "website", out var website) && !string.IsNullOrEmpty(website))
                links.Add($"Website: {website}");
            
            if (links.Any())
                html.AppendLine($"<div>{string.Join(" | ", links)}</div>");
            
            html.AppendLine("</div>");
        }

        // Summary
        if (data.TryGetProperty("summary", out JsonElement summary) && summary.ValueKind == JsonValueKind.String)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Professional Summary</h2>");
            html.AppendLine($"<p>{summary.GetString()}</p>");
            html.AppendLine("</div>");
        }

        // Experience
        if (data.TryGetProperty("experience", out JsonElement experience) && experience.ValueKind == JsonValueKind.Array)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Experience</h2>");
            
            foreach (var exp in experience.EnumerateArray())
            {
                html.AppendLine("<div class='experience-item'>");
                html.AppendLine($"<h3>{GetValue(exp, "jobTitle")} - {GetValue(exp, "company")}</h3>");
                
                var endDate = TryGetValue(exp, "isCurrent", out var isCurrent) && isCurrent == "true" 
                    ? "Present" 
                    : GetValue(exp, "endDate");
                html.AppendLine($"<div class='date'>{GetValue(exp, "location")} | {GetValue(exp, "startDate")} - {endDate}</div>");
                
                if (exp.TryGetProperty("responsibilities", out JsonElement responsibilities) && responsibilities.ValueKind == JsonValueKind.Array)
                {
                    html.AppendLine("<ul>");
                    foreach (var resp in responsibilities.EnumerateArray())
                    {
                        html.AppendLine($"<li>{resp.GetString()}</li>");
                    }
                    html.AppendLine("</ul>");
                }
                html.AppendLine("</div>");
            }
            html.AppendLine("</div>");
        }

        // Education
        if (data.TryGetProperty("education", out JsonElement education) && education.ValueKind == JsonValueKind.Array)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Education</h2>");
            
            foreach (var edu in education.EnumerateArray())
            {
                html.AppendLine("<div class='education-item'>");
                html.AppendLine($"<h3>{GetValue(edu, "degree")} - {GetValue(edu, "school")}</h3>");
                
                var details = new List<string>();
                if (TryGetValue(edu, "location", out var location) && !string.IsNullOrEmpty(location))
                    details.Add(location);
                if (TryGetValue(edu, "graduationDate", out var gradDate) && !string.IsNullOrEmpty(gradDate))
                    details.Add($"Graduated: {gradDate}");
                if (TryGetValue(edu, "gPA", out var gpa) && !string.IsNullOrEmpty(gpa))
                    details.Add($"GPA: {gpa}");
                
                if (details.Any())
                    html.AppendLine($"<div class='date'>{string.Join(" | ", details)}</div>");
                
                html.AppendLine("</div>");
            }
            html.AppendLine("</div>");
        }

        // Skills
        if (data.TryGetProperty("skills", out JsonElement skills) && skills.ValueKind == JsonValueKind.Array)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Skills</h2>");
            html.AppendLine("<div class='skills'>");
            
            foreach (var skill in skills.EnumerateArray())
            {
                html.AppendLine($"<span class='skill'>{skill.GetString()}</span>");
            }
            
            html.AppendLine("</div>");
            html.AppendLine("</div>");
        }

        // Projects
        if (data.TryGetProperty("projects", out JsonElement projects) && projects.ValueKind == JsonValueKind.Array && projects.GetArrayLength() > 0)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Projects</h2>");
            
            foreach (var proj in projects.EnumerateArray())
            {
                html.AppendLine("<div class='experience-item'>");
                html.AppendLine($"<h3>{GetValue(proj, "name")}</h3>");
                
                if (TryGetValue(proj, "description", out var desc) && !string.IsNullOrEmpty(desc))
                    html.AppendLine($"<p>{desc}</p>");
                
                if (proj.TryGetProperty("technologies", out JsonElement techs) && techs.ValueKind == JsonValueKind.Array)
                {
                    var techList = techs.EnumerateArray().Select(t => t.GetString()).ToList();
                    if (techList.Any())
                        html.AppendLine($"<div><strong>Technologies:</strong> {string.Join(", ", techList)}</div>");
                }
                
                if (TryGetValue(proj, "link", out var link) && !string.IsNullOrEmpty(link))
                    html.AppendLine($"<div><strong>Link:</strong> {link}</div>");
                
                html.AppendLine("</div>");
            }
            html.AppendLine("</div>");
        }

        // Certifications
        if (data.TryGetProperty("certifications", out JsonElement certifications) && certifications.ValueKind == JsonValueKind.Array && certifications.GetArrayLength() > 0)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Certifications</h2>");
            html.AppendLine("<ul>");
            
            foreach (var cert in certifications.EnumerateArray())
            {
                var certText = $"{GetValue(cert, "name")} - {GetValue(cert, "issuer")}";
                
                if (TryGetValue(cert, "issueDate", out var issueDate) && !string.IsNullOrEmpty(issueDate))
                    certText += $" ({issueDate})";
                
                html.AppendLine($"<li>{certText}</li>");
            }
            
            html.AppendLine("</ul>");
            html.AppendLine("</div>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static string GetValue(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool TryGetValue(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (element.TryGetProperty(propertyName, out JsonElement prop) && prop.ValueKind == JsonValueKind.String)
        {
            value = prop.GetString() ?? string.Empty;
            return true;
        }
        return false;
    }
}
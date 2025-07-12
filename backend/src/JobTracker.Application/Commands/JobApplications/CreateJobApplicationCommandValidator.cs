using FluentValidation;

namespace JobTracker.Application.Commands.JobApplications;

public class CreateJobApplicationCommandValidator : AbstractValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Invalid email format");

        RuleFor(x => x.ContactPhone)
            .Matches(@"^[\d\s\-\+\(\)\.]+$").When(x => !string.IsNullOrEmpty(x.ContactPhone))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.AppliedDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Applied date cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid application status");
    }
}
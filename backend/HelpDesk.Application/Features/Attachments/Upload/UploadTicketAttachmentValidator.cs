using FluentValidation;
using HelpDesk.Application.Abstractions.Attachments;

namespace HelpDesk.Application.Features.Attachments.Upload;

public sealed class UploadTicketAttachmentValidator
    : AbstractValidator<UploadTicketAttachmentCommand>
{
    public UploadTicketAttachmentValidator(IAttachmentPolicy policy)
    {
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(fileName =>
                !fileName.Contains('/') &&
                !fileName.Contains('\\') &&
                fileName != "." &&
                fileName != "..")
            .WithMessage("The attachment file name must not contain a path.");
        RuleFor(command => command.FileName)
            .Must(fileName => fileName.All(character => !char.IsControl(character)))
            .WithMessage("The attachment file name contains invalid control characters.");
        RuleFor(command => command.ContentType)
            .NotEmpty()
            .MaximumLength(127);
        RuleFor(command => command.DeclaredSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(policy.MaximumFileSizeBytes)
            .WithMessage(
                $"The attachment must not exceed {policy.MaximumFileSizeBytes / 1024 / 1024} MB.");
        RuleFor(command => command.Content)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(stream => stream.CanRead)
            .WithMessage("The attachment content must be readable.");
        RuleFor(command => command)
            .Must(command => policy.Match(command.FileName, command.ContentType) is not null)
            .WithMessage(
                $"The attachment type is not supported. Allowed extensions: {string.Join(", ", policy.SupportedExtensions)}.");
    }
}

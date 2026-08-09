using FluentValidation;

namespace HelpDesk.Application.Features.Attachments.Download;

public sealed class DownloadTicketAttachmentValidator
    : AbstractValidator<DownloadTicketAttachmentQuery>
{
    public DownloadTicketAttachmentValidator()
    {
        RuleFor(query => query.TicketId).NotEmpty();
        RuleFor(query => query.AttachmentId).NotEmpty();
    }
}

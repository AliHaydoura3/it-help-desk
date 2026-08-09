using FluentValidation;

namespace HelpDesk.Application.Features.Attachments.List;

public sealed class GetTicketAttachmentsValidator
    : AbstractValidator<GetTicketAttachmentsQuery>
{
    public GetTicketAttachmentsValidator()
    {
        RuleFor(query => query.TicketId).NotEmpty();
        RuleFor(query => query.PageNumber).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query)
            .Must(query => (long)(query.PageNumber - 1) * query.PageSize <= int.MaxValue)
            .WithMessage("The requested attachment page is too large.");
    }
}

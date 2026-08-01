using FluentValidation;

namespace HelpDesk.Application.Features.Tickets.CreateTicket;

public sealed class CreateTicketValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Category).IsInEnum();
        RuleFor(command => command.Priority).IsInEnum();
    }
}

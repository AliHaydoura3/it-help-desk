using FluentValidation;
using HelpDesk.Domain;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class EscalateTicketValidator : AbstractValidator<EscalateTicketCommand>
{
    public EscalateTicketValidator()
    {
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.Level).IsInEnum().NotEqual(TicketEscalationLevel.None);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1000);
    }
}

using FluentValidation;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class AutoAssignTicketValidator : AbstractValidator<AutoAssignTicketCommand>
{
    public AutoAssignTicketValidator()
    {
        RuleFor(command => command.TicketId).NotEmpty();
    }
}

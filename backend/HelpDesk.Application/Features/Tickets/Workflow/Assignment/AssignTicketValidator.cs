using FluentValidation;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class AssignTicketValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketValidator()
    {
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.AgentUserId).NotEmpty();
    }
}

using FluentValidation;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class AddInternalNoteValidator : AbstractValidator<AddInternalNoteCommand>
{
    public AddInternalNoteValidator()
    {
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.Content).NotEmpty().MaximumLength(4000);
    }
}

using FluentValidation;

namespace HelpDesk.Application.Features.Tickets.UpdateTicket;

public sealed class UpdateTicketValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Category).IsInEnum();
        RuleFor(command => command.Priority).IsInEnum();
    }
}

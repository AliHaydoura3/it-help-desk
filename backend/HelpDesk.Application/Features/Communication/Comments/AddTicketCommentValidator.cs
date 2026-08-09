using FluentValidation;

namespace HelpDesk.Application.Features.Communication.Comments;

public sealed class AddTicketCommentValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentValidator()
    {
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.Content).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.MentionedAgentIds)
            .Must(ids => ids is null || ids.Distinct().Count() <= 10)
            .WithMessage("A comment can mention at most 10 support agents.");
        RuleForEach(command => command.MentionedAgentIds)
            .NotEmpty()
            .When(command => command.MentionedAgentIds is not null);
    }
}

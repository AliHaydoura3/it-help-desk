using FluentValidation;

namespace HelpDesk.Application.Features.Communication.Notifications;

public sealed class MarkNotificationReadValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadValidator() =>
        RuleFor(command => command.NotificationId).NotEmpty();
}

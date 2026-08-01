using FluentValidation;

namespace HelpDesk.Application.Features.Authentication.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}

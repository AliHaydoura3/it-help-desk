using FluentValidation;

namespace HelpDesk.Application.Features.Authentication.RefreshToken;

public sealed class RefreshTokenValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}

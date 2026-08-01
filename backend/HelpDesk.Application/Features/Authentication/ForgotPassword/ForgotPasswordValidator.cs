using FluentValidation;

namespace HelpDesk.Application.Features.Authentication.ForgotPassword;

public sealed class ForgotPasswordValidator
    : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}

namespace HelpDesk.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
    string? Email { get; }
    bool IsInRole(string role);
}

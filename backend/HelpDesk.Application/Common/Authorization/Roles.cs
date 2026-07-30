namespace HelpDesk.Application.Common.Authorization;

public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string ITSupportSpecialist = nameof(ITSupportSpecialist);
    public const string Manager = nameof(Manager);
    public const string Employee = nameof(Employee);

    public static readonly IReadOnlyList<string> All =
    [
        Admin,
        ITSupportSpecialist,
        Manager,
        Employee
    ];
}
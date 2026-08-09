using HelpDesk.Application.Abstractions.Authentication;

namespace HelpDesk.Application.Common.Authorization;

public static class RolePermissions
{
    private static readonly IReadOnlySet<Permission> EmployeePermissions =
        new HashSet<Permission>
        {
            Permission.CreateTickets,
            Permission.TrackOwnTickets
        };

    private static readonly IReadOnlySet<Permission> SupportPermissions =
        EmployeePermissions.Concat(
        [
            Permission.MonitorAllTickets,
            Permission.EditAllTickets,
            Permission.CancelAllTickets,
            Permission.ChangeAssignedTicketStatus,
            Permission.CommentOnAllTickets,
            Permission.ManageTicketWorkflow,
            Permission.ViewAssignmentHistory,
            Permission.UseInternalNotes
        ]).ToHashSet();

    private static readonly IReadOnlySet<Permission> ManagerPermissions =
        EmployeePermissions.Concat(
        [
            Permission.MonitorAllTickets,
            Permission.ViewTicketReports,
            Permission.ViewAssignmentHistory
        ]).ToHashSet();

    private static readonly IReadOnlySet<Permission> AdministratorPermissions =
        Enum.GetValues<Permission>().ToHashSet();

    private static readonly IReadOnlySet<Permission> NoPermissions =
        new HashSet<Permission>();

    public static bool HasPermission(string role, Permission permission) =>
        GetPermissions(role).Contains(permission);

    public static bool HasPermission(this ICurrentUser currentUser, Permission permission) =>
        Roles.All.Any(role =>
            currentUser.IsInRole(role) && HasPermission(role, permission));

    public static IReadOnlySet<Permission> GetPermissions(string role)
    {
        if (role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
            return AdministratorPermissions;
        if (role.Equals(Roles.ITSupportSpecialist, StringComparison.OrdinalIgnoreCase))
            return SupportPermissions;
        if (role.Equals(Roles.Manager, StringComparison.OrdinalIgnoreCase))
            return ManagerPermissions;
        if (role.Equals(Roles.Employee, StringComparison.OrdinalIgnoreCase))
            return EmployeePermissions;

        return NoPermissions;
    }
}

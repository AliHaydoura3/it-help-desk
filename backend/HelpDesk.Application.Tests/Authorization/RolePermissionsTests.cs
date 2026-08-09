using HelpDesk.Application.Common.Authorization;

namespace HelpDesk.Application.Tests.Authorization;

public sealed class RolePermissionsTests
{
    [Test]
    public void RoleCatalog_ContainsExactlyTheFourSupportedSingleAssignmentRoles()
    {
        Assert.That(Roles.All, Is.EqualTo(new[]
        {
            Roles.Admin,
            Roles.ITSupportSpecialist,
            Roles.Manager,
            Roles.Employee
        }));
    }

    [TestCase(Roles.Employee)]
    [TestCase(Roles.Manager)]
    [TestCase(Roles.ITSupportSpecialist)]
    [TestCase(Roles.Admin)]
    public void EveryRole_InheritsEmployeeSelfServiceCapabilities(string role)
    {
        Assert.Multiple(() =>
        {
            Assert.That(RolePermissions.HasPermission(role, Permission.CreateTickets), Is.True);
            Assert.That(RolePermissions.HasPermission(role, Permission.TrackOwnTickets), Is.True);
        });
    }

    [Test]
    public void Employee_HasNoOperationalOrAdministrativeCapabilities()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RolePermissions.HasPermission(Roles.Employee, Permission.MonitorAllTickets), Is.False);
            Assert.That(RolePermissions.HasPermission(Roles.Employee, Permission.ManageTicketWorkflow), Is.False);
            Assert.That(RolePermissions.HasPermission(Roles.Employee, Permission.ViewTicketReports), Is.False);
            Assert.That(RolePermissions.HasPermission(Roles.Employee, Permission.ManageUsers), Is.False);
        });
    }

    [Test]
    public void Manager_HasReadOnlyOversightAndReportingCapabilities()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RolePermissions.HasPermission(Roles.Manager, Permission.MonitorAllTickets), Is.True);
            Assert.That(RolePermissions.HasPermission(Roles.Manager, Permission.ViewAssignmentHistory), Is.True);
            Assert.That(RolePermissions.HasPermission(Roles.Manager, Permission.ViewTicketReports), Is.True);
            Assert.That(RolePermissions.HasPermission(Roles.Manager, Permission.EditAllTickets), Is.False);
            Assert.That(RolePermissions.HasPermission(Roles.Manager, Permission.ChangeAssignedTicketStatus), Is.False);
            Assert.That(RolePermissions.HasPermission(Roles.Manager, Permission.CommentOnAllTickets), Is.False);
        });
    }

    [Test]
    public void SupportAgent_HasQueueCapabilitiesWithoutAdministrativeOverride()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RolePermissions.HasPermission(Roles.ITSupportSpecialist, Permission.MonitorAllTickets), Is.True);
            Assert.That(RolePermissions.HasPermission(Roles.ITSupportSpecialist, Permission.ManageTicketWorkflow), Is.True);
            Assert.That(RolePermissions.HasPermission(Roles.ITSupportSpecialist, Permission.ChangeAssignedTicketStatus), Is.True);
            Assert.That(RolePermissions.HasPermission(Roles.ITSupportSpecialist, Permission.ChangeAnyTicketStatus), Is.False);
            Assert.That(RolePermissions.HasPermission(Roles.ITSupportSpecialist, Permission.ManageUsers), Is.False);
            Assert.That(RolePermissions.HasPermission(Roles.ITSupportSpecialist, Permission.ViewActivityLogs), Is.False);
        });
    }

    [Test]
    public void Administrator_HasEveryDefinedCapability()
    {
        foreach (var permission in Enum.GetValues<Permission>())
            Assert.That(RolePermissions.HasPermission(Roles.Admin, permission), Is.True, permission.ToString());
    }
}

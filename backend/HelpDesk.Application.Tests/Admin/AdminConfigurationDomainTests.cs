using HelpDesk.Domain;

namespace HelpDesk.Application.Tests.Admin;

public sealed class AdminConfigurationDomainTests
{
    [Test]
    public void SystemSettings_Update_AppliesValidatedOperationalValues()
    {
        var actorId = Guid.NewGuid();
        var occurredAt = new DateTime(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);
        var settings = SystemSettings.CreateDefaults(occurredAt.AddHours(-1));

        settings.Update(actorId, "Example Corp", "HELP@example.com", false, false, 40, occurredAt);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Id, Is.EqualTo(SystemSettings.SingletonId));
            Assert.That(settings.OrganizationName, Is.EqualTo("Example Corp"));
            Assert.That(settings.SupportEmail, Is.EqualTo("help@example.com"));
            Assert.That(settings.AutomaticAssignmentEnabled, Is.False);
            Assert.That(settings.EmailNotificationsEnabled, Is.False);
            Assert.That(settings.MaximumOpenTicketsPerEmployee, Is.EqualTo(40));
            Assert.That(settings.UpdatedByUserId, Is.EqualTo(actorId));
        });
    }

    [TestCase(0)]
    [TestCase(1001)]
    public void SystemSettings_Update_RejectsInvalidTicketLimit(int limit)
    {
        var settings = SystemSettings.CreateDefaults(DateTime.UtcNow);
        Assert.Throws<DomainRuleException>(() => settings.Update(
            Guid.NewGuid(), "Help Desk", "support@example.com", true, true, limit, DateTime.UtcNow));
    }

    [Test]
    public void TicketCategorySetting_Deactivation_PreservesStableCategoryKey()
    {
        var setting = TicketCategorySetting.Create(
            TicketCategory.Network, "Network", "Connectivity", 30, DateTime.UtcNow);

        setting.Update(Guid.NewGuid(), "Connectivity", "Network and VPN support", false, 25, DateTime.UtcNow);

        Assert.Multiple(() =>
        {
            Assert.That(setting.Category, Is.EqualTo(TicketCategory.Network));
            Assert.That(setting.IsActive, Is.False);
            Assert.That(setting.DisplayName, Is.EqualTo("Connectivity"));
            Assert.That(setting.SortOrder, Is.EqualTo(25));
        });
    }
}

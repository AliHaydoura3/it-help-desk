namespace HelpDesk.Domain;

public sealed class SystemSettings
{
    public const int SingletonId = 1;

    private SystemSettings() { }

    public int Id { get; private set; }
    public string OrganizationName { get; private set; } = string.Empty;
    public string SupportEmail { get; private set; } = string.Empty;
    public bool AutomaticAssignmentEnabled { get; private set; }
    public bool EmailNotificationsEnabled { get; private set; }
    public int MaximumOpenTicketsPerEmployee { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public static SystemSettings CreateDefaults(DateTime createdAtUtc) =>
        new()
        {
            Id = SingletonId,
            OrganizationName = "IT Help Desk",
            SupportEmail = "support@example.com",
            AutomaticAssignmentEnabled = true,
            EmailNotificationsEnabled = true,
            MaximumOpenTicketsPerEmployee = 25,
            UpdatedAtUtc = createdAtUtc
        };

    public void Update(
        Guid actorUserId,
        string organizationName,
        string supportEmail,
        bool automaticAssignmentEnabled,
        bool emailNotificationsEnabled,
        int maximumOpenTicketsPerEmployee,
        DateTime updatedAtUtc)
    {
        if (actorUserId == Guid.Empty)
            throw new DomainRuleException("A settings change requires an administrator.");
        if (string.IsNullOrWhiteSpace(organizationName))
            throw new DomainRuleException("The organization name is required.");
        if (organizationName.Trim().Length > 120)
            throw new DomainRuleException("The organization name cannot exceed 120 characters.");
        if (string.IsNullOrWhiteSpace(supportEmail) ||
            !System.Net.Mail.MailAddress.TryCreate(supportEmail.Trim(), out _))
            throw new DomainRuleException("A valid support email is required.");
        if (maximumOpenTicketsPerEmployee is < 1 or > 1000)
            throw new DomainRuleException("The employee ticket limit must be between 1 and 1000.");

        OrganizationName = organizationName.Trim();
        SupportEmail = supportEmail.Trim().ToLowerInvariant();
        AutomaticAssignmentEnabled = automaticAssignmentEnabled;
        EmailNotificationsEnabled = emailNotificationsEnabled;
        MaximumOpenTicketsPerEmployee = maximumOpenTicketsPerEmployee;
        UpdatedAtUtc = updatedAtUtc;
        UpdatedByUserId = actorUserId;
    }
}

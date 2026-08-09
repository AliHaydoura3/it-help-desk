namespace HelpDesk.Domain;

public sealed class TicketCategorySetting
{
    private TicketCategorySetting() { }

    public TicketCategory Category { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public static TicketCategorySetting Create(
        TicketCategory category,
        string displayName,
        string description,
        int sortOrder,
        DateTime createdAtUtc) =>
        new()
        {
            Category = category,
            DisplayName = ValidateDisplayName(displayName),
            Description = ValidateDescription(description),
            IsActive = true,
            SortOrder = ValidateSortOrder(sortOrder),
            UpdatedAtUtc = createdAtUtc
        };

    public void Update(
        Guid actorUserId,
        string displayName,
        string description,
        bool isActive,
        int sortOrder,
        DateTime updatedAtUtc)
    {
        if (actorUserId == Guid.Empty)
            throw new DomainRuleException("A category change requires an administrator.");

        DisplayName = ValidateDisplayName(displayName);
        Description = ValidateDescription(description);
        IsActive = isActive;
        SortOrder = ValidateSortOrder(sortOrder);
        UpdatedAtUtc = updatedAtUtc;
        UpdatedByUserId = actorUserId;
    }

    private static string ValidateDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainRuleException("A category display name is required.");
        var trimmed = value.Trim();
        if (trimmed.Length > 80)
            throw new DomainRuleException("A category display name cannot exceed 80 characters.");
        return trimmed;
    }

    private static string ValidateDescription(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length > 300)
            throw new DomainRuleException("A category description cannot exceed 300 characters.");
        return trimmed;
    }

    private static int ValidateSortOrder(int value)
    {
        if (value is < 0 or > 1000)
            throw new DomainRuleException("A category sort order must be between 0 and 1000.");
        return value;
    }
}

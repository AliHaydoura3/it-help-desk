namespace HelpDesk.Application.Common.Authorization;

public enum Permission
{
    CreateTickets,
    TrackOwnTickets,
    MonitorAllTickets,
    EditAllTickets,
    CancelAllTickets,
    ChangeAssignedTicketStatus,
    ChangeAnyTicketStatus,
    CommentOnAllTickets,
    ViewTicketReports,
    ManageTicketWorkflow,
    ViewAssignmentHistory,
    UseInternalNotes,
    ManageUsers,
    ManageRoles,
    ManageTicketCategories,
    ManageSystemSettings,
    ViewSystemMonitoring,
    ViewActivityLogs
}

using HelpDesk.Application.Features.Admin;
using HelpDesk.Domain;

namespace HelpDesk.Application.Abstractions.Admin;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketCategorySettingResponse>> GetCategoriesAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<TicketCategorySettingResponse> UpdateCategoryAsync(TicketCategory category, UpdateTicketCategoryCommand command, CancellationToken cancellationToken);
    Task<SystemSettingsResponse> GetSettingsAsync(CancellationToken cancellationToken);
    Task<SystemSettingsResponse> UpdateSettingsAsync(UpdateSystemSettingsCommand command, CancellationToken cancellationToken);
}

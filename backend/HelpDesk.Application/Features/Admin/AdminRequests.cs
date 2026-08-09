using FluentValidation;
using HelpDesk.Application.Abstractions.Admin;
using HelpDesk.Domain;
using MediatR;

namespace HelpDesk.Application.Features.Admin;

public sealed record GetAdminDashboardQuery : IRequest<AdminDashboardResponse>;
public sealed record GetAdminRolesQuery : IRequest<IReadOnlyList<AdminRoleResponse>>;
public sealed record GetTicketCategorySettingsQuery(bool ActiveOnly = false)
    : IRequest<IReadOnlyList<TicketCategorySettingResponse>>;
public sealed record GetSystemSettingsQuery : IRequest<SystemSettingsResponse>;

public sealed record UpdateTicketCategoryCommand(
    TicketCategory Category,
    string DisplayName,
    string Description,
    bool IsActive,
    int SortOrder) : IRequest<TicketCategorySettingResponse>;

public sealed record UpdateSystemSettingsCommand(
    string OrganizationName,
    string SupportEmail,
    bool AutomaticAssignmentEnabled,
    bool EmailNotificationsEnabled,
    int MaximumOpenTicketsPerEmployee) : IRequest<SystemSettingsResponse>;

public sealed class UpdateTicketCategoryValidator : AbstractValidator<UpdateTicketCategoryCommand>
{
    public UpdateTicketCategoryValidator()
    {
        RuleFor(command => command.Category).IsInEnum();
        RuleFor(command => command.DisplayName).NotEmpty().MaximumLength(80);
        RuleFor(command => command.Description).MaximumLength(300);
        RuleFor(command => command.SortOrder).InclusiveBetween(0, 1000);
    }
}

public sealed class UpdateSystemSettingsValidator : AbstractValidator<UpdateSystemSettingsCommand>
{
    public UpdateSystemSettingsValidator()
    {
        RuleFor(command => command.OrganizationName).NotEmpty().MaximumLength(120);
        RuleFor(command => command.SupportEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.MaximumOpenTicketsPerEmployee).InclusiveBetween(1, 1000);
    }
}

public sealed class GetAdminDashboardHandler(IAdminService service)
    : IRequestHandler<GetAdminDashboardQuery, AdminDashboardResponse>
{
    public Task<AdminDashboardResponse> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken) =>
        service.GetDashboardAsync(cancellationToken);
}

public sealed class GetAdminRolesHandler(IAdminService service)
    : IRequestHandler<GetAdminRolesQuery, IReadOnlyList<AdminRoleResponse>>
{
    public Task<IReadOnlyList<AdminRoleResponse>> Handle(GetAdminRolesQuery request, CancellationToken cancellationToken) =>
        service.GetRolesAsync(cancellationToken);
}

public sealed class GetTicketCategorySettingsHandler(IAdminService service)
    : IRequestHandler<GetTicketCategorySettingsQuery, IReadOnlyList<TicketCategorySettingResponse>>
{
    public Task<IReadOnlyList<TicketCategorySettingResponse>> Handle(GetTicketCategorySettingsQuery request, CancellationToken cancellationToken) =>
        service.GetCategoriesAsync(request.ActiveOnly, cancellationToken);
}

public sealed class UpdateTicketCategoryHandler(IAdminService service)
    : IRequestHandler<UpdateTicketCategoryCommand, TicketCategorySettingResponse>
{
    public Task<TicketCategorySettingResponse> Handle(UpdateTicketCategoryCommand request, CancellationToken cancellationToken) =>
        service.UpdateCategoryAsync(request.Category, request, cancellationToken);
}

public sealed class GetSystemSettingsHandler(IAdminService service)
    : IRequestHandler<GetSystemSettingsQuery, SystemSettingsResponse>
{
    public Task<SystemSettingsResponse> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken) =>
        service.GetSettingsAsync(cancellationToken);
}

public sealed class UpdateSystemSettingsHandler(IAdminService service)
    : IRequestHandler<UpdateSystemSettingsCommand, SystemSettingsResponse>
{
    public Task<SystemSettingsResponse> Handle(UpdateSystemSettingsCommand request, CancellationToken cancellationToken) =>
        service.UpdateSettingsAsync(request, cancellationToken);
}

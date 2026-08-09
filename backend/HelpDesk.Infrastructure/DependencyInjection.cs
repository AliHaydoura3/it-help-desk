using System.Text;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Infrastructure.Authentication.Jwt;
using HelpDesk.Infrastructure.Authorization;
using HelpDesk.Infrastructure.Identity;
using HelpDesk.Infrastructure.Identity.Seeding;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Email;
using HelpDesk.Application.Abstractions.Logging;
using HelpDesk.Infrastructure.Logging;
using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Infrastructure.Authentication;
using HelpDesk.Infrastructure.Communication;
using HelpDesk.Infrastructure.Tickets;
using HelpDesk.Application.Abstractions.Reporting;
using HelpDesk.Infrastructure.Reporting;
using HelpDesk.Infrastructure.Reporting.Exports;
using HelpDesk.Application.Abstractions.Attachments;
using HelpDesk.Infrastructure.Attachments;
using HelpDesk.Application.Abstractions.Admin;
using HelpDesk.Infrastructure.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace HelpDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddIdentity()
            .AddJwt(configuration)
            .AddEmail(configuration)
            .AddCommunication(configuration)
            .AddAttachments(configuration)
            .AddReporting(configuration)
            .AddAdministration()
            .AddSeeders(configuration)
            .AddApplicationAuthorization();

        return services;
    }

    private static IServiceCollection AddAdministration(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IOperationalSettingsReader, OperationalSettingsReader>();
        services.AddScoped<ITicketCategoryCatalog, TicketCategoryCatalog>();
        return services;
    }

    private static IServiceCollection AddEmail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailMessageSender, SmtpEmailMessageSender>();
        services.AddScoped<IPasswordResetEmailSender,
            SmtpPasswordResetEmailSender>();

        return services;
    }

    private static IServiceCollection AddCommunication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<NotificationDeliveryOptions>()
            .Bind(configuration.GetSection(NotificationDeliveryOptions.SectionName))
            .Validate(options => options.PollIntervalSeconds is >= 1 and <= 300,
                "Notification delivery polling must be between 1 and 300 seconds.")
            .Validate(options => options.BatchSize is >= 1 and <= 500,
                "Notification delivery batch size must be between 1 and 500.")
            .Validate(options => options.MaximumAttempts is >= 1 and <= 10,
                "Notification delivery attempts must be between 1 and 10.")
            .ValidateOnStart();

        services.AddScoped<ICommunicationService, CommunicationService>();
        services.AddScoped<INotificationQueue, NotificationQueue>();
        services.AddScoped<INotificationDeliveryProcessor,
            NotificationDeliveryProcessor>();

        return services;
    }

    private static IServiceCollection AddReporting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ReportingOptions>()
            .Bind(configuration.GetSection(ReportingOptions.SectionName))
            .Validate(options => options.DefaultPeriodDays is >= 1 and <= 3660,
                "Reporting:DefaultPeriodDays must be between 1 and 3660.")
            .Validate(options => options.AtRiskThresholdPercentage is > 0 and < 100,
                "Reporting:AtRiskThresholdPercentage must be greater than 0 and less than 100.")
            .Validate(options =>
                    options.SlaHours.Low > 0 &&
                    options.SlaHours.Medium > 0 &&
                    options.SlaHours.High > 0 &&
                    options.SlaHours.Critical > 0,
                "Every reporting SLA target must be greater than zero hours.")
            .ValidateOnStart();

        services.AddScoped<IReportingService, ReportingService>();
        services.AddSingleton<IReportFileExporter, ExcelReportExporter>();
        services.AddSingleton<IReportFileExporter, PdfReportExporter>();
        return services;
    }

    private static IServiceCollection AddAttachments(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<AttachmentOptions>()
            .Bind(configuration.GetSection(AttachmentOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.StorageRootPath),
                "Attachments:StorageRootPath is required.")
            .Validate(options => options.MaximumFileSizeBytes is >= 1024 and <= 25 * 1024 * 1024,
                "Attachments:MaximumFileSizeBytes must be between 1 KB and 25 MB.")
            .Validate(options => options.MaximumFilesPerTicket is >= 1 and <= 100,
                "Attachments:MaximumFilesPerTicket must be between 1 and 100.")
            .Validate(options => options.AllowedExtensions is { Length: > 0 } &&
                    options.AllowedExtensions.All(AttachmentPolicy.IsKnownExtension),
                "Attachments:AllowedExtensions contains an unsupported extension.")
            .ValidateOnStart();

        services.AddSingleton<IAttachmentPolicy, AttachmentPolicy>();
        services.AddSingleton<IAttachmentStorage, LocalAttachmentStorage>();
        services.AddScoped<ITicketAttachmentService, TicketAttachmentService>();
        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    private static IServiceCollection AddIdentity(
        this IServiceCollection services)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IActivityLogReader, ActivityLogReader>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketWorkflowService, TicketWorkflowService>();

        return services;
    }

    private static IServiceCollection AddJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.Key) >= 32,
                "Jwt:Key must contain at least 32 bytes.")
            .ValidateOnStart();

        var jwt = configuration
            .GetRequiredSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrWhiteSpace(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments(
                                "/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userIdValue = context.Principal?
                            .FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? context.Principal?.FindFirstValue("sub");

                        if (!Guid.TryParse(userIdValue, out var userId))
                        {
                            context.Fail("The token subject is invalid.");
                            return;
                        }

                        var userManager = context.HttpContext.RequestServices
                            .GetRequiredService<UserManager<ApplicationUser>>();
                        var user = await userManager.FindByIdAsync(
                            userId.ToString());

                        if (user is null || !user.IsActive)
                            context.Fail("The user account is inactive.");
                    }
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    private static IServiceCollection AddSeeders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SeedAdminOptions>()
            .Bind(configuration.GetSection(SeedAdminOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<RoleSeeder>();
        services.AddScoped<UserSeeder>();
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}

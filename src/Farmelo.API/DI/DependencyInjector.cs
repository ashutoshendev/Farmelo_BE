using Farmelo.API.ApiUtils;
using Farmelo.API.Auditing;
using Farmelo.API.Services.Invoices;
using Farmelo.Business.Services.Invoices;
using Farmelo.Business.Services.Security;
using Farmelo.Data.Connections;
using Farmelo.Data.Extensions;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.EFContext;
using Farmelo.Data.Write.IRepository;
using Farmelo.Data.Write.Repository;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.Config;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.API.DI;

public static class DependencyInjector
{
    public static void RegisterServices(IServiceCollection services, ConfigurationOptions config)
    {
        RegisterRepositories(services);
        RegisterModelValidators(services);
        RegisterMappers(services);
        RegisterMediatR(services);
        RegisterDapper(services, config);
    }

    internal static void UseAppSettingsToFetchSensitiveInformation(IServiceCollection services, ConfigurationOptions config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextAuditContext>();
        services.AddScoped<IAuditContext, HttpContextAuditContext>();

        services.AddDbContext<FarmeloDbContext>(options =>
        {
            options.UseSqlServer(ResolveConnectionString(config))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.AddCustomValue("AuditTrails", config.AuditLogs.AuditLogEnable);
        });

        services.AddDataProtection();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuditEventQueue, AuditEventQueue>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandBehavior<,>));
        services.AddHostedService<AuditLogBackgroundService>();
        services.AddSingleton<IApiLogQueue, ApiLogQueue>();
        services.AddHostedService<ApiLogBackgroundService>();
        services.AddHostedService<ApiLogPurgeService>();
        services.AddScoped<IInvoiceEmailSender, SmtpInvoiceEmailSender>();
        services.AddScoped<IInvoiceService, InvoiceService>();
    }

    private static void RegisterModelValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Farmelo.Shared.AssemblyMarker).Assembly);
    }

    private static void RegisterMappers(IServiceCollection services)
    {
        services.AddAutoMapper(
            _ => { },
            typeof(Farmelo.AutoMapper.BusinessAndDatabaseDtoMapper.MappingProfile).Assembly,
            typeof(Farmelo.AutoMapper.BusinessAndIntegrationDtoMapper.MappingProfile).Assembly);
    }

    private static void RegisterMediatR(IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(Farmelo.Business.AssemblyMarker).Assembly);
        });
    }

    private static void RegisterDapper(IServiceCollection services, ConfigurationOptions config)
    {
        services.AddScoped<IDbConnectionFactory>(_ => new SqlConnectionFactory(ResolveConnectionString(config)));
        services.AddScoped<IDapperExecutor, DapperExecutor>();
    }

    private static string ResolveConnectionString(ConfigurationOptions config)
    {
        var configured = config.ConnectionStrings.DatabaseConnection;
        var resolved = MethodHelper.Unzip(configured);

        if (string.IsNullOrWhiteSpace(resolved))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DatabaseConnection is required. Configure it in appsettings or environment-specific configuration.");
        }

        return resolved;
    }
}

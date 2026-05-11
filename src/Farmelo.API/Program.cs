using Farmelo.API.ActionFilters;
using Farmelo.API.DI;
using Farmelo.API.Middleware;
using Farmelo.Shared.Config;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System.Net;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var config = builder.Configuration.Get<ConfigurationOptions>() ?? new ConfigurationOptions();

    var allowedOrigins = config.CorsAllowedOrigins.Allowed;
    var exposedHeaders = new[] { "Content-Disposition" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MyPolicy", policy =>
        {
            if (string.IsNullOrWhiteSpace(allowedOrigins) || allowedOrigins == "*")
            {
                policy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(exposedHeaders)
                    .AllowCredentials();
            }
            else
            {
                policy
                    .WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(exposedHeaders)
                    .AllowCredentials();
            }
        });
    });

    builder.Services
        .AddControllers(options => options.Filters.Add(new ModelStateFilter()))
        .AddNewtonsoftJson();

    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = string.IsNullOrWhiteSpace(config.AuthOptions.CookieName)
                ? "Farmelo.Auth"
                : config.AuthOptions.CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = config.AuthOptions.RequireHttps
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = config.AuthOptions.SameSite?.Trim().ToUpperInvariant() switch
            {
                "STRICT" => SameSiteMode.Strict,
                "NONE" => SameSiteMode.None,
                _ => SameSiteMode.Lax
            };
            options.ExpireTimeSpan = TimeSpan.FromHours(Math.Max(1, config.AuthOptions.ExpireHours));
            options.SlidingExpiration = config.AuthOptions.SlidingExpiration;
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddSingleton(config);

    DependencyInjector.RegisterServices(builder.Services, config);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
    builder.Host.UseNLog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.MapType<TimeSpan>(() => new OpenApiSchema
        {
            Type = "string",
            Example = new OpenApiString("00:00:00")
        });
    });

    DependencyInjector.UseAppSettingsToFetchSensitiveInformation(builder.Services, config);

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("MyPolicy");
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<DecodeUrlMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception: {Message}", ex.Message);
    throw;
}
finally
{
    LogManager.Shutdown();
}

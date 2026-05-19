using Farmelo.API.ActionFilters;
using Farmelo.API.DI;
using Farmelo.API.Middleware;
using Farmelo.API.Services.Auth;
using Farmelo.Data.Write.EFContext;
using Farmelo.Shared.Config;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System.Security.Claims;

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
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(exposedHeaders);
            }
            else
            {
                policy
                    .WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(exposedHeaders);
            }
        });
    });

    builder.Services
        .AddControllers(options => options.Filters.Add(new ModelStateFilter()))
        .AddNewtonsoftJson();

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = JwtTokenOptions.GetIssuer(config.AuthOptions),
                ValidateAudience = true,
                ValidAudience = JwtTokenOptions.GetAudience(config.AuthOptions),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = JwtTokenOptions.CreateSigningKey(config.AuthOptions),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? context.Principal?.FindFirstValue("sub")
                        ?? context.Principal?.FindFirstValue("userId");
                    if (!int.TryParse(userIdValue, out var userId))
                    {
                        context.Fail("Invalid token subject.");
                        return;
                    }

                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<FarmeloDbContext>();
                    var userIsActive = await dbContext.UserAccounts
                        .AsNoTracking()
                        .AnyAsync(user => user.Id == userId && user.IsActive, context.HttpContext.RequestAborted);

                    if (!userIsActive)
                    {
                        context.Fail("User is inactive.");
                    }
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
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter a JWT bearer token."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }
            ] = Array.Empty<string>()
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
    app.UseMiddleware<ApiLoggingMiddleware>();
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

namespace Farmelo.Shared.Config;

public class ConfigurationOptions
{
    public ConnectionStrings ConnectionStrings { get; set; } = new();
    public Integrations Integrations { get; set; } = new();
    public ToggleSettings ToggleSettings { get; set; } = new();
    public DataProtectionOptions DataProtection { get; set; } = new();
    public CorsAllowedOrigins CorsAllowedOrigins { get; set; } = new();
    public AuditLogs AuditLogs { get; set; } = new();
    public AuthOptions AuthOptions { get; set; } = new();
    public FileStorageOptions FileStorage { get; set; } = new();
    public InvoiceOptions Invoices { get; set; } = new();
    public EmailOptions Email { get; set; } = new();
}

public class ConnectionStrings
{
    public string DatabaseConnection { get; set; } = string.Empty;
}

public class CorsAllowedOrigins
{
    public string Allowed { get; set; } = "*";
}

public class ToggleSettings
{
    public bool DisplayStackTrace { get; set; }
    public bool RequestLogEnabled { get; set; }
}

public class Integrations
{
    public SystemApi SystemApi { get; set; } = new();
}

public class DataProtectionOptions
{
    public string Purpose { get; set; } = string.Empty;
}

public class AuditLogs
{
    public bool AuditLogEnable { get; set; }
}

public class AuthOptions
{
    public string CookieName { get; set; } = "Farmelo.Auth";
    public int ExpireHours { get; set; } = 8;
    public int RememberMeDays { get; set; } = 7;
    public bool SlidingExpiration { get; set; } = true;
    public string SameSite { get; set; } = "Lax";
    public bool RequireHttps { get; set; }
}

public class FileStorageOptions
{
    public string RootPath { get; set; } = string.Empty;
}

public class InvoiceOptions
{
    public string FirmName { get; set; } = "Kaj International";
    public string TemplatePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string NumberPrefix { get; set; } = "KAJ";
    public string DownloadBaseUrl { get; set; } = "/api/invoices";
    public string[] Recipients { get; set; } = Array.Empty<string>();
}

public class EmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Kaj International";
}

public class SystemApi : BaseApiSettings
{
}

public abstract class BaseApiSettings
{
    public string ApiClient { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
}

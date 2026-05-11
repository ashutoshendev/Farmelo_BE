using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Farmelo.Data.Extensions;

public sealed class CustomOptionsExtension : IDbContextOptionsExtension
{
    private readonly Dictionary<string, object> _customValues = new(StringComparer.OrdinalIgnoreCase);
    private DbContextOptionsExtensionInfo? _info;

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void AddCustomValue(string key, object value)
        => _customValues[key] = value;

    public object GetCustomValue(string key)
        => _customValues.TryGetValue(key, out var value) ? value : string.Empty;

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        public override bool IsDatabaseProvider => false;
        public override string LogFragment => string.Empty;

        public override int GetServiceProviderHashCode()
            => 0;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;
    }
}

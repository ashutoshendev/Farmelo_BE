using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Farmelo.Data.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder AddCustomValue(this DbContextOptionsBuilder optionsBuilder, string key, object value)
    {
        var extension = optionsBuilder.Options.FindExtension<CustomOptionsExtension>() ?? new CustomOptionsExtension();
        extension.AddCustomValue(key, value);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return optionsBuilder;
    }

    public static object GetCustomValue(this DbContextOptions options, string key)
    {
        var extension = options.FindExtension<CustomOptionsExtension>();
        return extension?.GetCustomValue(key) ?? string.Empty;
    }
}

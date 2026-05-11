using Farmelo.Data.Extensions;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.Data.Write.EFContext;

public partial class FarmeloDbContext : DbContext
{
    private readonly bool _auditTrailsEnabled;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditContext _auditContext;

    public FarmeloDbContext(
        DbContextOptions<FarmeloDbContext> options,
        ICurrentUser currentUser,
        IAuditContext auditContext)
        : base(options)
    {
        _auditTrailsEnabled = options.GetCustomValue("AuditTrails") is bool enabled && enabled;
        _currentUser = currentUser;
        _auditContext = auditContext;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyBaseEntityAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    private void ApplyBaseEntityAuditFields()
    {
        if (!_auditTrailsEnabled)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var userName = string.IsNullOrWhiteSpace(_currentUser.UserName)
            ? "SYSTEM"
            : _currentUser.UserName;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy ??= userName;
                entry.Entity.CreatedOn ??= now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedBy = userName;
                entry.Entity.ModifiedOn = now;
            }
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

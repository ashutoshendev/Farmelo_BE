using Farmelo.Data.Extensions;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.Entities;
using Farmelo.Data.Write.Entities.Abstractions;
using Farmelo.Shared.CommonHelper;
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

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApiLog> ApiLogs => Set<ApiLog>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<RawStockEntry> RawStockEntries => Set<RawStockEntry>();
    public DbSet<RawStockMovement> RawStockMovements => Set<RawStockMovement>();
    public DbSet<BoxStockMovement> BoxStockMovements => Set<BoxStockMovement>();
    public DbSet<B2BOrder> B2BOrders => Set<B2BOrder>();
    public DbSet<B2CAssignment> B2CAssignments => Set<B2CAssignment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyBaseEntityAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureUserAccounts(modelBuilder);
        ConfigureProducts(modelBuilder);
        ConfigureProductPriceHistories(modelBuilder);
        ConfigureAuditLogs(modelBuilder);
        ConfigureApiLogs(modelBuilder);
        ConfigureBusinessManagement(modelBuilder);
        SeedProducts(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    private static void ConfigureBusinessManagement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Party>(entity =>
        {
            entity.ToTable("Parties");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PartyType, x.IsActive, x.Name });
            entity.Property(x => x.PartyType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.ContactName).HasMaxLength(150);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Location).HasMaxLength(250);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<RawStockEntry>(entity =>
        {
            entity.ToTable("RawStockEntries");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.EntryDate, x.AvailableKg });
            entity.HasIndex(x => new { x.SuttaGrade, x.AvailableKg, x.EntryDate });
            entity.HasIndex(x => new { x.SellerPartyId, x.EntryDate });
            entity.Property(x => x.SuttaGrade).HasMaxLength(40).HasDefaultValue("Makhana 5 Sutta").IsRequired();
            entity.Property(x => x.QuantityKg).HasPrecision(12, 3);
            entity.Property(x => x.AvailableKg).HasPrecision(12, 3);
            entity.Property(x => x.CostPerKg).HasPrecision(12, 2);
            entity.Property(x => x.SupplierName).HasMaxLength(180);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
            entity.HasOne(x => x.SellerParty)
                .WithMany()
                .HasForeignKey(x => x.SellerPartyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RawStockMovement>(entity =>
        {
            entity.ToTable("RawStockMovements");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MovementDate, x.MovementType });
            entity.Property(x => x.MovementType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.SuttaGrade).HasMaxLength(40).HasDefaultValue("Makhana 5 Sutta").IsRequired();
            entity.Property(x => x.QuantityKg).HasPrecision(12, 3);
            entity.Property(x => x.CostPerKg).HasPrecision(12, 2);
            entity.Property(x => x.ReferenceType).HasMaxLength(60);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<BoxStockMovement>(entity =>
        {
            entity.ToTable("BoxStockMovements");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProductId, x.MovementType, x.MovementDate });
            entity.Property(x => x.MovementType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.SuttaGrade).HasMaxLength(40).HasDefaultValue("Makhana 5 Sutta").IsRequired();
            entity.Property(x => x.UnitCost).HasPrecision(12, 2);
            entity.Property(x => x.UnitPrice).HasPrecision(12, 2);
            entity.Property(x => x.ReferenceType).HasMaxLength(60);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.BoxStockMovements)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<B2BOrder>(entity =>
        {
            entity.ToTable("B2BOrders");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PartyId, x.OrderDate });
            entity.HasIndex(x => new { x.SuttaGrade, x.OrderDate });
            entity.HasIndex(x => x.DueDate);
            entity.Property(x => x.SuttaGrade).HasMaxLength(40).HasDefaultValue("Makhana 5 Sutta").IsRequired();
            entity.Property(x => x.QuantityKg).HasPrecision(12, 3);
            entity.Property(x => x.PricePerKg).HasPrecision(12, 2);
            entity.Property(x => x.TotalValue).HasPrecision(14, 2);
            entity.Property(x => x.CostPerKg).HasPrecision(12, 2);
            entity.Property(x => x.TotalCost).HasPrecision(14, 2);
            entity.Property(x => x.Margin).HasPrecision(14, 2);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
            entity.HasOne(x => x.Party)
                .WithMany()
                .HasForeignKey(x => x.PartyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<B2CAssignment>(entity =>
        {
            entity.ToTable("B2CAssignments");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PartyId, x.AssignmentDate });
            entity.HasIndex(x => new { x.ProductId, x.AssignmentDate });
            entity.HasIndex(x => x.DueDate);
            entity.Property(x => x.UnitPrice).HasPrecision(12, 2);
            entity.Property(x => x.TotalValue).HasPrecision(14, 2);
            entity.Property(x => x.UnitCost).HasPrecision(12, 2);
            entity.Property(x => x.TotalCost).HasPrecision(14, 2);
            entity.Property(x => x.Margin).HasPrecision(14, 2);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
            entity.HasOne(x => x.Party)
                .WithMany()
                .HasForeignKey(x => x.PartyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.B2CAssignments)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PartyId, x.PaymentDate });
            entity.HasIndex(x => x.B2BOrderId);
            entity.HasIndex(x => x.B2CAssignmentId);
            entity.Property(x => x.PaymentType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(14, 2);
            entity.Property(x => x.PaymentMode).HasMaxLength(80);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(120);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
            entity.HasOne(x => x.Party)
                .WithMany()
                .HasForeignKey(x => x.PartyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.B2BOrder)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.B2BOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.B2CAssignment)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.B2CAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.HasIndex(x => new { x.InvoiceType, x.InvoiceDate });
            entity.HasIndex(x => new { x.PartyId, x.InvoiceDate });
            entity.HasIndex(x => x.B2BOrderId).IsUnique().HasFilter("[B2BOrderId] IS NOT NULL");
            entity.HasIndex(x => x.B2CAssignmentId).IsUnique().HasFilter("[B2CAssignmentId] IS NOT NULL");
            entity.Property(x => x.InvoiceNumber).HasMaxLength(40).IsRequired();
            entity.Property(x => x.InvoiceType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.TotalAmount).HasPrecision(14, 2);
            entity.Property(x => x.PdfFileName).HasMaxLength(180);
            entity.Property(x => x.PdfPath).HasMaxLength(500);
            entity.Property(x => x.EmailStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.EmailError).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
            entity.HasOne(x => x.Party)
                .WithMany()
                .HasForeignKey(x => x.PartyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.B2BOrder)
                .WithMany()
                .HasForeignKey(x => x.B2BOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.B2CAssignment)
                .WithMany()
                .HasForeignKey(x => x.B2CAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OccurredOn);
            entity.HasIndex(x => new { x.UserId, x.OccurredOn });
            entity.HasIndex(x => new { x.Module, x.OccurredOn });
            entity.HasIndex(x => new { x.EventType, x.OccurredOn });
            entity.HasIndex(x => new { x.TargetLabel, x.OccurredOn });
            entity.Property(x => x.UserFullName).HasMaxLength(150);
            entity.Property(x => x.UserEmail).HasMaxLength(256);
            entity.Property(x => x.UserRole).HasMaxLength(30);
            entity.Property(x => x.EventType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Module).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(160).IsRequired();
            entity.Property(x => x.HttpMethod).HasMaxLength(12);
            entity.Property(x => x.Path).HasMaxLength(500);
            entity.Property(x => x.IpAddress).HasMaxLength(80);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.Details).HasMaxLength(1000);
            entity.Property(x => x.TargetLabel).HasMaxLength(200);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
        });
    }

    private static void ConfigureApiLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.ToTable("ApiLogs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Timestamp);
            entity.HasIndex(x => new { x.ActorId, x.Timestamp });
            entity.Property(x => x.Method).HasMaxLength(12).IsRequired();
            entity.Property(x => x.Endpoint).HasMaxLength(500).IsRequired();
        });
    }

    private static void ConfigureUserAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("UserAccounts");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => new { x.Role, x.IsActive, x.CreatedOn });
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
        });
    }

    private static void ConfigureProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => new { x.IsActive, x.IsBestseller, x.Name });
            entity.Property(x => x.Slug).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(600);
            entity.Property(x => x.Weight).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WeightGrams).HasDefaultValue(0);
            entity.Property(x => x.CurrentPrice).HasPrecision(10, 2);
            entity.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("INR").IsRequired();
            entity.Property(x => x.Ingredients).HasMaxLength(1000);
            entity.Property(x => x.Nutrition).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.AccentColor).HasMaxLength(20);
            entity.Property(x => x.BackgroundColor).HasMaxLength(20);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);
        });
    }

    private static void ConfigureProductPriceHistories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductPriceHistory>(entity =>
        {
            entity.ToTable("ProductPriceHistories");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProductId, x.ChangedOn });
            entity.Property(x => x.OldPrice).HasPrecision(10, 2);
            entity.Property(x => x.NewPrice).HasPrecision(10, 2);
            entity.Property(x => x.Reason).HasMaxLength(300);
            entity.Property(x => x.CreatedBy).HasMaxLength(100);
            entity.Property(x => x.ModifiedBy).HasMaxLength(100);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.PriceHistories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ChangedByUser)
                .WithMany(x => x.PriceChanges)
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        var createdOn = new DateTime(2026, 5, 11, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Slug = "plain",
                Name = "Plain Makhana",
                Description = "Clean roasted crunch for daily snacking.",
                Weight = "200g",
                WeightGrams = 200,
                CurrentPrice = 249m,
                Currency = "INR",
                Ingredients = "Roasted makhana, olive oil, rock salt.",
                Nutrition = "High protein, gluten-free, light roasted snack.",
                IsBestseller = true,
                IsActive = true,
                AccentColor = "#0d6042",
                BackgroundColor = "#eef6e4",
                CreatedBy = AppConstants.Roles.Admin,
                CreatedOn = createdOn
            },
            new Product
            {
                Id = 2,
                Slug = "peri-peri",
                Name = "Peri Peri",
                Description = "Fiery chilli, garlic and tang.",
                Weight = "200g",
                WeightGrams = 200,
                CurrentPrice = 269m,
                Currency = "INR",
                Ingredients = "Roasted makhana, olive oil, peri peri seasoning.",
                Nutrition = "Roasted not fried, no palm oil, no added preservatives.",
                IsBestseller = true,
                IsActive = true,
                AccentColor = "#b83224",
                BackgroundColor = "#fff3ec",
                CreatedBy = AppConstants.Roles.Admin,
                CreatedOn = createdOn
            },
            new Product
            {
                Id = 3,
                Slug = "salt-and-pepper",
                Name = "Salt and Pepper",
                Description = "Classic seasoning with a sharp pepper finish.",
                Weight = "200g",
                WeightGrams = 200,
                CurrentPrice = 269m,
                Currency = "INR",
                Ingredients = "Roasted makhana, olive oil, salt and pepper seasoning.",
                Nutrition = "Roasted not fried, no palm oil, no added preservatives.",
                IsBestseller = false,
                IsActive = true,
                AccentColor = "#25231d",
                BackgroundColor = "#f4ede0",
                CreatedBy = AppConstants.Roles.Admin,
                CreatedOn = createdOn
            },
            new Product
            {
                Id = 4,
                Slug = "cream-and-onion",
                Name = "Cream and Onion",
                Description = "Smooth onion with herby creaminess.",
                Weight = "200g",
                WeightGrams = 200,
                CurrentPrice = 269m,
                Currency = "INR",
                Ingredients = "Roasted makhana, olive oil, cream and onion seasoning.",
                Nutrition = "Roasted not fried, no palm oil, no added preservatives.",
                IsBestseller = true,
                IsActive = true,
                AccentColor = "#326f2b",
                BackgroundColor = "#edf6dc",
                CreatedBy = AppConstants.Roles.Admin,
                CreatedOn = createdOn
            },
            new Product
            {
                Id = 5,
                Slug = "cheesy-delight",
                Name = "Cheesy Delight",
                Description = "Creamy, savoury and snackable.",
                Weight = "200g",
                WeightGrams = 200,
                CurrentPrice = 269m,
                Currency = "INR",
                Ingredients = "Roasted makhana, olive oil, cheesy delight seasoning.",
                Nutrition = "Roasted not fried, no palm oil, no added preservatives.",
                IsBestseller = true,
                IsActive = true,
                AccentColor = "#d99622",
                BackgroundColor = "#fff5d7",
                CreatedBy = AppConstants.Roles.Admin,
                CreatedOn = createdOn
            });
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

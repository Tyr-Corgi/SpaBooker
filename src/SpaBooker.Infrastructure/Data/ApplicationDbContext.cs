using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Entities;

namespace SpaBooker.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<SpaService> SpaServices => Set<SpaService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<UserMembership> UserMemberships => Set<UserMembership>();
    public DbSet<MembershipCreditTransaction> MembershipCreditTransactions => Set<MembershipCreditTransaction>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<TherapistAvailability> TherapistAvailability => Set<TherapistAvailability>();
    public DbSet<ServiceTherapist> ServiceTherapists => Set<ServiceTherapist>();
    public DbSet<GiftCertificate> GiftCertificates => Set<GiftCertificate>();
    public DbSet<GiftCertificateTransaction> GiftCertificateTransactions => Set<GiftCertificateTransaction>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomServiceCapability> RoomServiceCapabilities => Set<RoomServiceCapability>();
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();
    public DbSet<ClientNote> ClientNotes => Set<ClientNote>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure all DateTime properties to use UTC
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }

        // Location configuration
        builder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.IsActive);
        });

        // SpaService configuration
        builder.Entity<SpaService>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Location)
                .WithMany(l => l.Services)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes for common queries
            entity.HasIndex(e => new { e.LocationId, e.IsActive });
            entity.HasIndex(e => e.Name);
        });

        // ServiceTherapist (many-to-many)
        builder.Entity<ServiceTherapist>(entity =>
        {
            entity.HasKey(st => new { st.ServiceId, st.TherapistId });
            
            entity.HasOne(st => st.Service)
                .WithMany(s => s.ServiceTherapists)
                .HasForeignKey(st => st.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(st => st.Therapist)
                .WithMany()
                .HasForeignKey(st => st.TherapistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Booking configuration
        builder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServicePrice).HasPrecision(18, 2);
            entity.Property(e => e.DepositAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountApplied).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.CreditsUsed).HasPrecision(18, 2);

            entity.HasOne(e => e.Client)
                .WithMany(u => u.BookingsAsClient)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Therapist)
                .WithMany(u => u.BookingsAsTherapist)
                .HasForeignKey(e => e.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Bookings)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TherapistId, e.StartTime, e.EndTime });
            entity.HasIndex(e => new { e.RoomId, e.StartTime, e.EndTime });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.ClientId, e.StartTime });
            entity.HasIndex(e => new { e.LocationId, e.StartTime });
            entity.HasIndex(e => e.StripePaymentIntentId);
            entity.HasIndex(e => e.CreatedAt);
            
            // CHECK constraint to ensure EndTime > StartTime
            entity.ToTable(t => t.HasCheckConstraint("CK_Booking_EndTimeAfterStartTime", "\"EndTime\" > \"StartTime\""));
        });

        // MembershipPlan configuration
        builder.Entity<MembershipPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MonthlyPrice).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyCredits).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            
            // Index for active plans
            entity.HasIndex(e => e.IsActive);
        });

        // UserMembership configuration
        builder.Entity<UserMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrentCredits).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MembershipPlan)
                .WithMany(p => p.UserMemberships)
                .HasForeignKey(e => e.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserId, e.Status });
        });

        // MembershipCreditTransaction configuration
        builder.Entity<MembershipCreditTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.UserMembership)
                .WithMany(m => m.CreditTransactions)
                .HasForeignKey(e => e.UserMembershipId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Index for transaction history queries
            entity.HasIndex(e => new { e.UserMembershipId, e.CreatedAt });
            entity.HasIndex(e => e.ExpiresAt);
        });

        // InventoryItem configuration
        builder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CostPerUnit).HasPrecision(18, 2);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.InventoryItems)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.LocationId, e.CurrentStock });
        });

        // InventoryTransaction configuration
        builder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.InventoryItem)
                .WithMany(i => i.Transactions)
                .HasForeignKey(e => e.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TherapistAvailability configuration
        builder.Entity<TherapistAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Therapist)
                .WithMany(u => u.Availability)
                .HasForeignKey(e => e.TherapistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TherapistId, e.DayOfWeek });
        });

        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Therapists)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes for common user queries
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => new { e.FirstName, e.LastName });
            entity.HasIndex(e => e.CreatedAt);
        });

        // GiftCertificate configuration
        builder.Entity<GiftCertificate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OriginalAmount).HasPrecision(18, 2);
            entity.Property(e => e.RemainingBalance).HasPrecision(18, 2);
            entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
            entity.Property(e => e.RecipientName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.PurchasedByUser)
                .WithMany()
                .HasForeignKey(e => e.PurchasedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RedeemedByUser)
                .WithMany()
                .HasForeignKey(e => e.RedeemedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RestrictedToLocation)
                .WithMany()
                .HasForeignKey(e => e.RestrictedToLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PurchasedByUserId);
            entity.HasIndex(e => new { e.PurchasedByUserId, e.PurchasedAt });
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.RecipientEmail);
        });

        // GiftCertificateTransaction configuration
        builder.Entity<GiftCertificateTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceBefore).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);

            entity.HasOne(e => e.GiftCertificate)
                .WithMany(gc => gc.Transactions)
                .HasForeignKey(e => e.GiftCertificateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelatedBooking)
                .WithMany()
                .HasForeignKey(e => e.RelatedBookingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PerformedByUser)
                .WithMany()
                .HasForeignKey(e => e.PerformedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Index for transaction history
            entity.HasIndex(e => new { e.GiftCertificateId, e.CreatedAt });
        });

        // Banner configuration
        builder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subtitle).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => new { e.DisplayOrder, e.IsActive });
        });

        // Room configuration
        builder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ColorCode).IsRequired().HasMaxLength(7); // #RRGGBB format

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Rooms)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.LocationId, e.DisplayOrder });
            entity.HasIndex(e => new { e.LocationId, e.IsActive });
        });

        // RoomServiceCapability configuration
        builder.Entity<RoomServiceCapability>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.ServiceCapabilities)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Service)
                .WithMany()
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: a room can only have one capability entry per service
            entity.HasIndex(e => new { e.RoomId, e.ServiceId }).IsUnique();
        });

        // ClientNote configuration
        builder.Entity<ClientNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.NoteType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedByName).HasMaxLength(100);

            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ClientId, e.CreatedAt });
        });

        // ProcessedWebhookEvent configuration
        builder.Entity<ProcessedWebhookEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StripeEventId).IsRequired().HasMaxLength(255);
            
            // Unique index for idempotency check
            entity.HasIndex(e => e.StripeEventId).IsUnique();
            entity.HasIndex(e => e.ReceivedAt);
        });

        // AuditLog configuration
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45); // IPv6 max length
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for common queries
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp });
            entity.HasIndex(e => new { e.Action, e.Timestamp });
            entity.HasIndex(e => e.Severity);
        });
    }
}


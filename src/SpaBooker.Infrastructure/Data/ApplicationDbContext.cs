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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
            entity.Property(e => e.RowVersion).IsRowVersion();

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

            entity.HasIndex(e => new { e.TherapistId, e.StartTime, e.EndTime });
            entity.HasIndex(e => e.Status);
        });

        // MembershipPlan configuration
        builder.Entity<MembershipPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MonthlyPrice).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyCredits).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
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
        });
    }
}


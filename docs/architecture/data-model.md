# Data Model

## Entity Relationship Overview

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  ApplicationUser │────<│     Booking      │>────│   SpaService     │
│  (Client/Staff)  │     │                  │     │                  │
└──────────────────┘     └──────────────────┘     └──────────────────┘
        │                        │                        │
        │                        │                        │
        ▼                        ▼                        ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  UserMembership  │     │      Room        │     │ServiceTherapist  │
│                  │     │                  │     │   (Junction)     │
└──────────────────┘     └──────────────────┘     └──────────────────┘
        │                        │
        ▼                        ▼
┌──────────────────┐     ┌──────────────────┐
│  MembershipPlan  │     │    Location      │
│                  │     │                  │
└──────────────────┘     └──────────────────┘
```

## Core Entities

### ApplicationUser
Extends ASP.NET Core Identity for all user types.

```csharp
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // For Therapists
    public int? LocationId { get; set; }
    public string? Bio { get; set; }
    public string? Specialties { get; set; }
    
    // Navigation
    public Location? Location { get; set; }
    public ICollection<Booking> BookingsAsClient { get; set; }
    public ICollection<Booking> BookingsAsTherapist { get; set; }
    public ICollection<UserMembership> Memberships { get; set; }
    public ICollection<TherapistAvailability> Availability { get; set; }
}
```

### Booking
Core appointment entity with full lifecycle tracking.

```csharp
public class Booking : ISoftDeletable
{
    public int Id { get; set; }
    
    // Relationships
    public string ClientId { get; set; }
    public string? TherapistId { get; set; }
    public int ServiceId { get; set; }
    public int? RoomId { get; set; }
    public int LocationId { get; set; }
    
    // Timing
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    // Status
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Payment
    public string? StripePaymentIntentId { get; set; }
    public string? StripeRefundId { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### SpaService
Treatment offerings with pricing and duration.

```csharp
public class SpaService
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    
    // Navigation
    public ICollection<ServiceTherapist> Therapists { get; set; }
    public ICollection<RoomServiceCapability> RoomCapabilities { get; set; }
}
```

### Location
Spa branches/locations.

```csharp
public class Location
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public ICollection<Room> Rooms { get; set; }
    public ICollection<ApplicationUser> Staff { get; set; }
}
```

### Room
Treatment rooms within locations.

```csharp
public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int LocationId { get; set; }
    public bool IsActive { get; set; }
    public string ColorCode { get; set; }  // For UI display
    public int DisplayOrder { get; set; }
    
    // Navigation
    public Location Location { get; set; }
    public ICollection<RoomServiceCapability> ServiceCapabilities { get; set; }
    public ICollection<Booking> Bookings { get; set; }
}
```

## Membership Entities

### MembershipPlan
Subscription tier definitions.

```csharp
public class MembershipPlan
{
    public int Id { get; set; }
    public string Name { get; set; }  // Bronze, Silver, Gold
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int MonthlyCredits { get; set; }
    public string? StripeProductId { get; set; }
    public string? StripePriceId { get; set; }
    public bool IsActive { get; set; }
}
```

### UserMembership
Active subscriptions with credit tracking.

```csharp
public class UserMembership
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int MembershipPlanId { get; set; }
    public MembershipStatus Status { get; set; }
    public decimal CurrentCredits { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? StripeSubscriptionId { get; set; }
    
    // Navigation
    public ApplicationUser User { get; set; }
    public MembershipPlan MembershipPlan { get; set; }
    public ICollection<MembershipCreditTransaction> CreditTransactions { get; set; }
}
```

### MembershipCreditTransaction
Credit lifecycle tracking.

```csharp
public class MembershipCreditTransaction
{
    public int Id { get; set; }
    public int UserMembershipId { get; set; }
    public string TransactionType { get; set; }  // Earned, Used, Expired
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }  // 12 months from earned
}
```

## Scheduling Entities

### TherapistAvailability
Weekly schedule definitions.

```csharp
public class TherapistAvailability
{
    public int Id { get; set; }
    public string TherapistId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
```

## Audit & Security Entities

### AuditLog
Security event tracking.

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### ProcessedWebhookEvent
Stripe webhook idempotency.

```csharp
public class ProcessedWebhookEvent
{
    public int Id { get; set; }
    public string StripeEventId { get; set; }
    public string EventType { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```

## Enumerations

### BookingStatus
```csharp
public enum BookingStatus
{
    Pending,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}
```

### MembershipStatus
```csharp
public enum MembershipStatus
{
    Active,
    Cancelled,
    Expired,
    PastDue
}
```

## Database Indexes

Key indexes for query optimization:

```csharp
// Booking queries
entity.HasIndex(e => new { e.ClientId, e.StartTime });
entity.HasIndex(e => new { e.TherapistId, e.StartTime, e.EndTime });
entity.HasIndex(e => new { e.RoomId, e.StartTime, e.EndTime });
entity.HasIndex(e => new { e.Status, e.StartTime });

// User queries
entity.HasIndex(e => e.Email);
entity.HasIndex(e => e.LocationId);

// Service queries
entity.HasIndex(e => new { e.IsActive, e.Category });

// Webhook idempotency
entity.HasIndex(e => e.StripeEventId).IsUnique();
```

## Constraints

```csharp
// Booking time validation
entity.HasCheckConstraint("CK_Booking_EndTimeAfterStartTime", 
    "\"EndTime\" > \"StartTime\"");

// Price validation
entity.Property(e => e.BasePrice)
    .HasPrecision(10, 2);
```


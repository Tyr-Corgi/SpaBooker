# Architecture Overview

## System Architecture

SpaBooker follows a **Clean Architecture** pattern with **Vertical Slice** organization, built on .NET 8 with Blazor Server.

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                   (SpaBooker.Web)                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │  Features/  │  │ Components/ │  │ Controllers │         │
│  │   Admin/    │  │   Layout/   │  │  (Webhooks) │         │
│  │  Bookings/  │  │   Shared/   │  │             │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│                (SpaBooker.Infrastructure)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │  Services/  │  │    Data/    │  │ Migrations/ │         │
│  │ BookingSvc  │  │  DbContext  │  │             │         │
│  │ EmailSvc    │  │  UnitOfWork │  │             │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      Core Layer                              │
│                    (SpaBooker.Core)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │  Entities/  │  │ Interfaces/ │  │ Validators/ │         │
│  │   Booking   │  │ IBookingSvc │  │ FluentValid │         │
│  │   Client    │  │ IEmailSvc   │  │             │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### Core Layer (`SpaBooker.Core`)
- **Entities**: Domain models (Booking, Client, Service, etc.)
- **Interfaces**: Service contracts (IBookingService, IEmailService)
- **DTOs**: Data transfer objects for layer communication
- **Enums**: Status types, booking states
- **Validators**: FluentValidation rules
- **Common**: Result pattern, Error types

### Infrastructure Layer (`SpaBooker.Infrastructure`)
- **Data**: Entity Framework DbContext, configurations
- **Services**: Business logic implementations
- **Migrations**: Database schema versions
- **External integrations**: Stripe, Email providers

### Web Layer (`SpaBooker.Web`)
- **Features**: Vertical slices organized by domain
- **Components**: Shared UI components
- **Controllers**: API endpoints (webhooks)
- **Middleware**: Security headers, error handling

## Vertical Slice Organization

Features are organized by domain, not by technical layer:

```
Features/
├── Admin/
│   ├── ClientManagement.razor
│   ├── ServiceManagement.razor
│   ├── UserManagement.razor
│   └── RoomManagement.razor
├── Auth/
│   ├── Login.razor
│   └── Register.razor
├── Bookings/
│   ├── BookAppointment.razor
│   ├── MyBookings.razor
│   └── PaymentSuccess.razor
├── Memberships/
│   ├── MembershipPlans.razor
│   └── Subscribe.razor
└── Scheduling/
    ├── TherapistSchedule.razor
    └── AdminScheduler.razor
```

## Key Design Patterns

### 1. Result Pattern
Explicit success/failure handling without exceptions:

```csharp
public async Task<Result<BookingDto>> CreateBookingAsync(CreateBookingDto dto)
{
    if (dto.StartTime < DateTime.UtcNow)
        return Result.Failure<BookingDto>(Error.Validation);
    
    // ... create booking
    return Result.Success(bookingDto);
}
```

### 2. Unit of Work
Transaction management for data consistency:

```csharp
await using var transaction = await _unitOfWork.BeginTransactionAsync();
try
{
    // Multiple operations
    await _unitOfWork.CommitAsync();
}
catch
{
    await _unitOfWork.RollbackAsync();
    throw;
}
```

### 3. Service Layer
Business logic abstracted from UI:

```csharp
// In Razor component - inject service, not DbContext
@inject IBookingService BookingService

// Use service methods
var result = await BookingService.CreateBookingAsync(dto);
```

## Concurrency Strategy

### Booking Conflict Prevention

1. **Database constraints**: CHECK constraint ensures `EndTime > StartTime`
2. **Application checks**: `IBookingConflictChecker` validates before insert
3. **Transaction isolation**: Operations wrapped in transactions
4. **Overlap detection**: `(Start1 < End2) AND (End1 > Start2)`

```csharp
var hasConflict = await _context.Bookings
    .Where(b => b.TherapistId == therapistId
        && b.Status != BookingStatus.Cancelled
        && b.StartTime < endTime
        && b.EndTime > startTime)
    .AnyAsync();
```

See [Concurrency Strategy](../decisions/adr-0003-concurrency.md) for details.

## Timezone Handling

All times stored in UTC:

```csharp
// Storage: Always UTC
booking.StartTime = DateTime.UtcNow;

// Display: Convert to local
var localTime = DateTimeHelper.ToLocal(booking.StartTime, userTimeZone);

// Input: Convert from local to UTC
booking.StartTime = DateTimeHelper.ToUtc(userInputTime, userTimeZone);
```

## Caching Strategy

Tiered caching with configurable expiration:

| Data Type | Cache Duration | Invalidation |
|-----------|---------------|--------------|
| Membership Plans | 60 minutes | On plan update |
| Services | 60 minutes | On service update |
| Locations | 120 minutes | On location update |
| Therapist Availability | 15 minutes | On schedule change |

## Security Architecture

See [Security Guide](../guides/security.md) for complete details.

### Key Security Features
- ASP.NET Core Identity with role-based access
- Rate limiting on authentication endpoints
- Security headers (CSP, HSTS, X-Frame-Options)
- Audit logging for security events
- Input validation with FluentValidation
- Stripe handles payment data (PCI-DSS compliant)

## Database Design

See [Data Model](data-model.md) for entity relationships.

### Key Entities
- **ApplicationUser**: Clients, Therapists, Admins
- **Booking**: Appointments with status tracking
- **SpaService**: Available treatments
- **Location**: Spa branches
- **Room**: Treatment rooms per location
- **MembershipPlan**: Subscription tiers
- **UserMembership**: Active subscriptions with credits

## External Integrations

### Stripe
- Payment processing for deposits
- Subscription management for memberships
- Webhook handlers for payment events

### Email Providers
- SMTP (Gmail, custom)
- SendGrid
- Amazon SES

## Performance Optimizations

1. **Database indexes**: 20+ strategic indexes on common queries
2. **Connection pooling**: 5-100 connections with retry policy
3. **NoTracking queries**: Default for read operations
4. **Caching layer**: Memory cache for static data
5. **Blazor Server**: Efficient SignalR communication


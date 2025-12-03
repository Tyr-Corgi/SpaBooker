# ADR-0003: Use Vertical Slice Architecture

## Status
Accepted

## Date
2025-11-01

## Context
We needed to decide on the architectural pattern for organizing the codebase. The application has multiple distinct features (bookings, memberships, scheduling, admin) that could be organized in different ways.

Traditional layered architecture (Controllers → Services → Repositories) can lead to:
- Scattered feature code across multiple folders
- Difficulty understanding a complete feature
- Coupling between unrelated features through shared services

## Decision
We adopted **Vertical Slice Architecture** with features organized by domain rather than technical layer.

```
Features/
├── Admin/
│   ├── ClientManagement.razor
│   ├── ServiceManagement.razor
│   └── UserManagement.razor
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

## Consequences

### Positive
- **Feature cohesion**: All code for a feature is together
- **Easier navigation**: Find everything about bookings in one place
- **Independent features**: Changes to one feature don't affect others
- **Parallel development**: Teams can work on different features simultaneously
- **Simpler testing**: Test complete features in isolation

### Negative
- **Potential duplication**: Some code might be duplicated across features
- **Learning curve**: Developers used to layered architecture need adjustment
- **Service sharing**: Need clear patterns for shared services

### Neutral
- Shared components still live in `Components/Shared/`
- Core domain entities remain in `SpaBooker.Core`
- Infrastructure services remain in `SpaBooker.Infrastructure`

## Alternatives Considered

### Traditional Layered Architecture
- Rejected: Feature code scattered across Controllers, Services, Repositories folders
- Harder to understand complete feature behavior

### Clean Architecture (Onion)
- Partially adopted: We use the layer separation (Core, Infrastructure, Web)
- Combined with vertical slices for feature organization

### Microservices
- Rejected: Overkill for current scale
- Would add deployment complexity

## References
- [Vertical Slice Architecture - Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [Feature Folders in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/areas)


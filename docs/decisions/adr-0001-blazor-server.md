# ADR-0001: Use Blazor Server for Frontend

## Status
Accepted

## Date
2025-11-01

## Context
We needed to choose a frontend technology for the SpaBooker application. The team has strong C# expertise but limited JavaScript/TypeScript experience. The application requires:
- Real-time updates for booking availability
- Rich interactive UI for scheduling
- Integration with ASP.NET Core backend
- Rapid development timeline

## Decision
We chose **Blazor Server** as the frontend technology.

## Consequences

### Positive
- **Single language**: C# for both frontend and backend
- **Real-time updates**: SignalR connection provides instant updates
- **Thin client**: No JavaScript bundle to download
- **Full .NET access**: Direct access to all .NET libraries
- **Faster development**: Team productivity with familiar language
- **SEO-friendly**: Server-side rendering by default

### Negative
- **Server dependency**: Requires constant connection to server
- **Latency sensitivity**: Every UI interaction requires server round-trip
- **Scalability concerns**: Each user maintains a SignalR circuit
- **Offline support**: Not possible without significant workarounds

### Neutral
- Requires proper SignalR configuration for production
- Need to manage circuit lifecycle and memory

## Alternatives Considered

### Blazor WebAssembly
- Rejected: Larger initial download, less suitable for real-time updates
- Would require separate API layer

### React/Angular with API
- Rejected: Team lacks JavaScript expertise
- Would slow down development significantly

### Razor Pages (MVC)
- Rejected: Less interactive, would require more JavaScript for dynamic features

## References
- [Blazor Server Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models#blazor-server)
- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)


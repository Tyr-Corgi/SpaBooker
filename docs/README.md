# SpaBooker Documentation

Welcome to the SpaBooker documentation. This folder contains all technical documentation for the spa booking management system.

## Quick Links

| Document | Description |
|----------|-------------|
| [CHANGELOG.md](CHANGELOG.md) | Version history, phase completions, and release notes |
| [architecture/](architecture/) | System architecture, patterns, and design decisions |
| [guides/](guides/) | Developer guides for security, testing, and accessibility |
| [operations/](operations/) | Deployment, secrets management, and integrations |
| [features/](features/) | Feature-specific documentation |
| [decisions/](decisions/) | Architecture Decision Records (ADRs) |

## Documentation Structure

```
docs/
├── README.md                    # This file - documentation index
├── CHANGELOG.md                 # Consolidated version/phase history
├── architecture/
│   ├── overview.md              # System architecture overview
│   ├── data-model.md            # Entity relationships
│   └── vertical-slice-guide.md  # Architecture patterns
├── guides/
│   ├── getting-started.md       # Quick start for developers
│   ├── security.md              # Security configuration
│   ├── testing.md               # Testing guidelines
│   └── accessibility.md         # Accessibility guidelines
├── operations/
│   ├── deployment.md            # Deployment instructions
│   ├── secrets-setup.md         # Secrets/environment config
│   └── stripe-integration.md    # Stripe setup and webhooks
├── features/
│   ├── booking-system.md        # Booking feature documentation
│   ├── membership-credits.md    # Membership and credits
│   ├── email-notifications.md   # Email system
│   └── therapist-availability.md # Scheduling system
└── decisions/
    ├── adr-template.md          # Template for new ADRs
    └── adr-0001-*.md            # Architecture decisions
```

## For New Developers

1. Start with [Getting Started](guides/getting-started.md)
2. Review [Architecture Overview](architecture/overview.md)
3. Read [Security Guide](guides/security.md) before handling credentials
4. Follow [Testing Guidelines](guides/testing.md) for all code changes

## Contributing to Documentation

- Keep documentation close to the code it describes
- Update docs when making related code changes
- Use [ADR format](decisions/adr-template.md) for architectural decisions
- Add entries to [CHANGELOG.md](CHANGELOG.md) for significant changes

## External Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Stripe API Documentation](https://stripe.com/docs)


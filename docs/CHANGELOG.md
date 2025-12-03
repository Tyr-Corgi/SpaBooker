# Changelog

All notable changes to the SpaBooker application are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

### Added
- Documentation restructure following industry best practices
- ADR (Architecture Decision Records) format for architectural decisions

---

## [1.0.0] - 2025-11-19

### Summary
Production-ready release after comprehensive security audit and remediation.

**Overall Status:** 24/32 issues resolved (75%)
- CRITICAL: 5/5 (100%)
- HIGH: 8/8 (100%)
- MEDIUM: 10/12 (83%)
- LOW: 1/7 (14%)

---

### Phase 3: Medium Priority Issues - 2025-11-19

**Completion:** 10/12 issues (83%)

#### Added
- `IBookingConflictChecker` service for race condition prevention
- `ICacheService` and `MemoryCacheService` with prefix-based invalidation
- `DateTimeHelper` utility for consistent timezone handling
- `ISoftDeletable` interface with global query filters
- Health check endpoints (`/health`, `/health/ready`, `/health/live`)
- Custom `StripeHealthCheck` for external API monitoring
- API versioning (URL segment, header, query string)
- CORS policy configuration
- 20+ database indexes for query optimization

#### Changed
- Database context configured with retry policy and NoTracking behavior
- Connection pooling optimized (5-100 connections)
- All DateTime columns configured as `timestamp with time zone`
- Booking entity now implements soft delete pattern

#### Fixed
- Concurrent booking race conditions with transaction-based conflict resolution
- Timezone inconsistencies (now using UTC storage)

#### Documentation
- Created `CONCURRENCY_STRATEGY.md`
- Created `TIMEZONE_STRATEGY.md`

---

### Phase 2: High Priority Issues - 2025-11-19

**Completion:** 8/8 issues (100%)

#### Added
- Rate limiting on authentication (5 attempts/15 min) and API endpoints (5 req/sec)
- Email confirmation flow with `ConfirmEmail` Razor Page
- `SecurityHeadersMiddleware` with CSP, HSTS, X-Frame-Options
- `ProcessedWebhookEvent` entity for webhook idempotency
- `AuditLog` entity and `IAuditService` for security event tracking
- `CleanupCircuitHandler` for Blazor Server resource cleanup

#### Changed
- Password requirements strengthened (12 chars, special chars required)
- Lockout policy updated (3 attempts, 30-minute lockout)
- Stripe webhook handlers completed with idempotency
- Blazor Server and SignalR hardened (message size limits, timeouts)
- Base URL now configurable via `App:BaseUrl` setting

#### Security
- All HIGH priority security issues resolved
- Application ready for staging deployment

---

### Phase 1: Critical Security Fixes - 2025-11-19

**Completion:** 5/5 issues (100%)

#### Added
- User Secrets configuration for development
- Serilog structured logging with file rotation
- `IUnitOfWork` pattern for transaction management
- FluentValidation with `BookingValidator`, `UserRegistrationValidator`, `ServiceValidator`
- `InputSanitizer` utility for XSS prevention

#### Removed
- Hardcoded credentials from `appsettings.json`
- Log files from repository (9 files)

#### Fixed
- Empty catch blocks replaced with proper error logging
- Database transactions added to critical operations

#### Security
- All CRITICAL security vulnerabilities resolved
- Secrets moved to User Secrets/environment variables

#### Documentation
- Created `SECRETS_SETUP_GUIDE.md`

---

### Service Layer Implementation - 2025-12-03

#### Added
- `IBookingService` with full CRUD operations
- `IRoomAvailabilityService` for room conflict detection
- `ITherapistAvailabilityService` for schedule-aware availability
- `IClientService` for client management operations
- Result pattern (`Result<T>`, `Error`) for explicit error handling
- DTOs: `CreateBookingDto`, `UpdateBookingDto`, `BookingDto`, `ClientStatisticsDto`

#### Changed
- `ClientManagement.razor` refactored to use service layer
- Booking modal improved with 15-minute interval dropdowns
- Location-based filtering for rooms and therapists

---

### Stripe Payment Integration - 2025-11-XX

#### Added
- Stripe Checkout for booking deposits (50%)
- Stripe subscriptions for membership plans
- Automated refund processing (24-hour cancellation window)
- Payment success pages with booking confirmation
- Subscription management (cancel, status tracking)

#### Features
- PCI-DSS compliant (Stripe handles card data)
- Webhook handlers for payment events
- Refund ID tracking in database

---

### Location Selection System - 2025-11-XX

#### Added
- Retail-style spa finder (search by city/ZIP)
- Location persistence in localStorage
- Visual location cards with selection feedback
- Integration with services page

---

### Membership Credits System - 2025-11-XX

#### Added
- Flexible membership plans with monthly credits
- Unlimited credit rollover (12-month expiration)
- Automatic expiration tracking
- Transaction history with credit lifecycle
- Stripe subscription integration

---

### Email Notifications - 2025-11-XX

#### Added
- Booking confirmation emails
- Cancellation notifications with refund details
- 24-hour appointment reminders (background service)
- Membership welcome and cancellation emails
- HTML templates with pink/rose gold theme
- Support for Gmail, SendGrid, Amazon SES, SMTP

---

### Therapist Availability System - 2025-11-XX

#### Added
- Weekly working hours configuration per day
- Date-specific blocks (vacations, personal time)
- Automatic integration with booking availability
- Client-facing available time slots only

---

### Initial Release - 2025-11-XX

#### Added
- ASP.NET Core 8 with Blazor Server
- PostgreSQL database with Entity Framework Core
- ASP.NET Core Identity (Client, Therapist, Admin roles)
- Service management with location-based filtering
- Booking system with real-time availability
- Pink/rose gold themed responsive UI
- Admin management (users, services, therapists)

---

## Migration Notes

### Database Migrations Required
After updating, run:
```bash
cd src/SpaBooker.Web
dotnet ef database update --project ../SpaBooker.Infrastructure
```

### Configuration Changes
- Move secrets to User Secrets (development) or environment variables (production)
- Configure `App:BaseUrl` in appsettings
- Set up Stripe webhook endpoint in Stripe Dashboard

---

## Pending Items

### Medium Priority (Deferred)
- [ ] Increase test coverage to 80%+
- [ ] Comprehensive error logging in Razor pages

### Low Priority (Future)
- [ ] Code style consistency
- [ ] XML documentation
- [ ] CSS minification
- [ ] Favicon set
- [ ] Accessibility improvements (ARIA, keyboard navigation)

### Future Enhancements
- GDPR compliance (data export, right to be forgotten)
- CI/CD pipeline
- Mobile app support
- Multi-language support


# Security Guide

## Overview

This guide covers security implementation, best practices, and configuration for the SpaBooker application.

## Current Security Features

### Authentication & Authorization
- ASP.NET Core Identity (industry-standard)
- Password hashing with PBKDF2
- Role-based access control (Client, Therapist, Admin)
- `[Authorize]` attributes on protected pages
- Session management with secure cookies

### Password Requirements
- Minimum 12 characters
- Requires uppercase letter
- Requires lowercase letter
- Requires digit
- Requires special character
- Account lockout: 3 failed attempts, 30-minute lockout

### Rate Limiting
- Authentication endpoints: 5 attempts per 15 minutes
- API endpoints: 5 requests per second
- IP-based tracking

### Security Headers
All responses include:
- `Content-Security-Policy` (CSP)
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: no-referrer-when-downgrade`
- `Strict-Transport-Security` (HSTS, 365 days)

### Payment Security
- Stripe handles all credit card data (PCI-DSS compliant)
- No card numbers stored in database
- Stripe Checkout hosted payment pages
- Webhook signature verification
- HTTPS required

### Database Security
- PostgreSQL with parameterized queries
- Entity Framework Core (prevents SQL injection)
- Connection pooling with retry logic
- No dynamic SQL queries

### Application Security
- HTTPS/SSL enabled by default
- XSS protection (Blazor automatic encoding)
- CSRF protection (ASP.NET Core anti-forgery tokens)
- HttpOnly cookies
- Secure cookie settings
- Input validation with FluentValidation

---

## Secrets Management

### Development: User Secrets

#### Step 1: Initialize User Secrets
```bash
cd src/SpaBooker.Web
dotnet user-secrets init
```

#### Step 2: Set Your Secrets
```bash
# Database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=spabooker;Username=postgres;Password=YOUR_PASSWORD"

# Stripe keys
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR_KEY"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_SECRET"

# Email settings
dotnet user-secrets set "EmailSettings:SmtpUsername" "your_email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "your_app_password"
```

### Production: Environment Variables

#### Azure App Service
In Azure Portal → Configuration → Application Settings:
```
ConnectionStrings__DefaultConnection = "Host=your-db.postgres.database.azure.com;..."
Stripe__PublishableKey = "pk_live_..."
Stripe__SecretKey = "sk_live_..."
Stripe__WebhookSecret = "whsec_..."
EmailSettings__SmtpUsername = "your_email@gmail.com"
EmailSettings__SmtpPassword = "your_password"
```

#### Docker
```bash
docker run -e "ConnectionStrings__DefaultConnection=Host=..." \
           -e "Stripe__SecretKey=sk_live_..." \
           -e "EmailSettings__SmtpPassword=..." \
           spabooker:latest
```

---

## Audit Logging

Security-relevant events are logged to the `AuditLog` table:

| Event Type | Description |
|------------|-------------|
| LoginSuccess | Successful authentication |
| LoginFailed | Failed authentication attempt |
| UserCreated | New user registration |
| UserModified | User profile changes |
| UserDeleted | User account deletion |
| PaymentSuccess | Successful payment |
| PaymentFailed | Failed payment attempt |
| BookingCreated | New booking |
| BookingCancelled | Booking cancellation |
| AdminAction | Administrative operations |

---

## Production Checklist

Before deploying to production:

- [ ] Move all secrets to environment variables or Azure Key Vault
- [ ] Change default admin password
- [ ] Enable HTTPS only (disable HTTP)
- [ ] Enable HSTS
- [ ] Configure firewall rules on PostgreSQL
- [ ] Use Stripe live keys (not test keys)
- [ ] Enable rate limiting
- [ ] Add security headers
- [ ] Enable email confirmation
- [ ] Strengthen password requirements
- [ ] Set up database backups
- [ ] Configure logging (Application Insights, Serilog)
- [ ] Review all `[Authorize]` attributes
- [ ] Enable Azure DDoS protection
- [ ] Set up SSL certificate
- [ ] Configure CORS properly (don't allow `*`)
- [ ] Review error messages (don't expose stack traces)

---

## Threat Model

| Threat | Likelihood | Impact | Mitigation |
|--------|-----------|--------|------------|
| SQL Injection | LOW | HIGH | EF Core parameterization |
| XSS Attacks | LOW | MEDIUM | Blazor auto-encoding |
| Brute Force Login | MEDIUM | HIGH | Rate limiting |
| Stolen API Keys | HIGH | CRITICAL | Secrets management |
| CSRF Attacks | LOW | MEDIUM | Anti-forgery tokens |
| Data Breach (DB) | MEDIUM | CRITICAL | Encryption, access control |
| DDoS Attacks | MEDIUM | HIGH | Azure DDoS, rate limiting |
| Session Hijacking | LOW | HIGH | HttpOnly cookies, HTTPS |
| Payment Fraud | LOW | HIGH | Stripe fraud detection |
| Insider Threats | LOW | HIGH | Audit logging, access control |

---

## Incident Response Plan

### If a Security Breach Occurs:

**Immediately:**
1. Disable affected user accounts
2. Rotate all API keys and passwords
3. Take affected services offline if necessary

**Within 24 Hours:**
1. Investigate breach extent (check audit logs)
2. Notify affected users
3. Document incident details

**Within 72 Hours:**
1. Report to authorities if required (GDPR, CCPA)
2. Implement fixes
3. Conduct security audit

**Follow-up:**
1. Post-mortem analysis
2. Update security policies
3. Train team on lessons learned

---

## Regular Maintenance

### Check for Vulnerable Packages
```bash
dotnet list package --vulnerable
```

### Update Packages
```bash
dotnet outdated
dotnet add package <PackageName> --version <NewVersion>
```

### Review Audit Logs
Regularly review the `AuditLog` table for:
- Failed login patterns
- Unusual administrative actions
- Payment anomalies

---

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Stripe Security Best Practices](https://stripe.com/docs/security/guide)
- [Azure Security Documentation](https://learn.microsoft.com/en-us/azure/security/)


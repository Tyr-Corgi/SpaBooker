# SpaBooker Security Guide

## 🔒 Security Implementation & Best Practices

This guide covers security measures, vulnerabilities, and recommendations for the SpaBooker application.

---

## ✅ Current Security Features

### Authentication & Authorization
- ✅ ASP.NET Core Identity (industry-standard)
- ✅ Password hashing with PBKDF2
- ✅ Role-based access control (Client/Therapist/Admin)
- ✅ `[Authorize]` attributes on protected pages
- ✅ Session management
- ✅ Account lockout after 5 failed attempts (15-minute lockout)

### Password Requirements
- ✅ Minimum 8 characters
- ✅ Requires uppercase letter
- ✅ Requires lowercase letter
- ✅ Requires digit
- ⚠️ Does NOT require special characters (by design for user-friendliness)

### Payment Security
- ✅ Stripe handles all credit card data (PCI-DSS compliant)
- ✅ No card numbers stored in database
- ✅ Stripe Checkout hosted payment pages
- ✅ Webhook signature verification
- ✅ HTTPS required

### Database Security
- ✅ PostgreSQL with parameterized queries
- ✅ Entity Framework Core (prevents SQL injection)
- ✅ Connection pooling with retry logic
- ✅ No dynamic SQL queries

### Application Security
- ✅ HTTPS/SSL enabled by default
- ✅ XSS protection (Blazor automatic encoding)
- ✅ CSRF protection (ASP.NET Core anti-forgery tokens)
- ✅ HttpOnly cookies
- ✅ Secure cookie settings

---

## ⚠️ **CRITICAL: Secrets Management** (MUST FIX BEFORE PRODUCTION)

### Current Issue
**🔴 CRITICAL**: Sensitive credentials are currently in `appsettings.json`:
- Database password
- Stripe API keys
- Email SMTP password

**Risk**: Anyone with repository access can see these credentials.

### Solution 1: User Secrets (Development)

#### Step 1: Initialize User Secrets
```bash
cd src/SpaBooker.Web
dotnet user-secrets init
```

#### Step 2: Set Your Secrets
```bash
# Database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=spabooker;Username=postgres;Password=YOUR_REAL_PASSWORD"

# Stripe keys
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR_KEY"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_SECRET"

# Email settings
dotnet user-secrets set "EmailSettings:SmtpUsername" "your_email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "your_app_password"
```

#### Step 3: Update appsettings.json
Remove sensitive values and replace with placeholders:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "OVERRIDE_WITH_USER_SECRETS_OR_ENV_VAR"
  },
  "Stripe": {
    "PublishableKey": "OVERRIDE_WITH_USER_SECRETS",
    "SecretKey": "OVERRIDE_WITH_USER_SECRETS",
    "WebhookSecret": "OVERRIDE_WITH_USER_SECRETS"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "OVERRIDE_WITH_USER_SECRETS",
    "SmtpPassword": "OVERRIDE_WITH_USER_SECRETS",
    "EnableSsl": true,
    "FromEmail": "noreply@spabooker.com",
    "FromName": "SpaBooker",
    "EnableNotifications": false
  }
}
```

### Solution 2: Environment Variables (Production)

#### For Azure App Service:
```bash
# In Azure Portal -> Configuration -> Application Settings:
ConnectionStrings__DefaultConnection = "Host=your-db.postgres.database.azure.com;..."
Stripe__PublishableKey = "pk_live_..."
Stripe__SecretKey = "sk_live_..."
Stripe__WebhookSecret = "whsec_..."
EmailSettings__SmtpUsername = "your_email@gmail.com"
EmailSettings__SmtpPassword = "your_password"
```

#### For Docker:
```bash
docker run -e "ConnectionStrings__DefaultConnection=Host=..." \
           -e "Stripe__SecretKey=sk_live_..." \
           -e "EmailSettings__SmtpPassword=..." \
           spabooker:latest
```

#### For Linux/Mac:
```bash
export ConnectionStrings__DefaultConnection="Host=..."
export Stripe__SecretKey="sk_live_..."
dotnet run
```

---

## 🛡️ **Additional Security Improvements**

### 1. Enable Two-Factor Authentication (2FA)

**Priority**: HIGH  
**Effort**: Medium

Add 2FA for admin accounts:

```csharp
// In Program.cs, add after AddIdentity:
.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

// In login page:
var requires2fa = await SignInManager.RequiresTwoFactorAsync(user);
if (requires2fa)
{
    // Redirect to 2FA verification page
}
```

### 2. Implement Rate Limiting

**Priority**: HIGH  
**Effort**: Low

Add rate limiting to prevent brute force attacks:

```csharp
// Install NuGet package:
// dotnet add package AspNetCoreRateLimit

// In Program.cs:
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// In appsettings.json:
"IpRateLimiting": {
  "EnableEndpointRateLimiting": true,
  "StackBlockedRequests": false,
  "GeneralRules": [
    {
      "Endpoint": "/auth/login",
      "Period": "1m",
      "Limit": 5
    },
    {
      "Endpoint": "*",
      "Period": "1s",
      "Limit": 10
    }
  ]
}
```

### 3. Strengthen Password Requirements

**Priority**: MEDIUM  
**Effort**: Very Low

Update in `Program.cs`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Stronger password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;  // Change to true
    options.Password.RequiredLength = 12;  // Increase from 8 to 12
    
    // Stricter lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);  // Increase
    options.Lockout.MaxFailedAccessAttempts = 3;  // Decrease from 5 to 3
    
    // Require email confirmation
    options.SignIn.RequireConfirmedEmail = true;  // Add this
});
```

### 4. Add Security Headers

**Priority**: MEDIUM  
**Effort**: Low

```csharp
// In Program.cs, before app.Run():
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://js.stripe.com; " +
        "style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; " +
        "font-src 'self' data:; connect-src 'self' https://api.stripe.com;");
    
    await next();
});
```

### 5. Enable HTTPS Redirection & HSTS

**Priority**: HIGH  
**Effort**: Very Low

```csharp
// In Program.cs:
app.UseHttpsRedirection();

app.UseHsts();  // HTTP Strict Transport Security

// Configure HSTS:
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
```

### 6. Input Validation & Sanitization

**Priority**: MEDIUM  
**Effort**: Medium

Add validation attributes to all models:

```csharp
public class BookingModel
{
    [Required]
    [StringLength(500, MinimumLength = 10)]
    [RegularExpression(@"^[a-zA-Z0-9\s\.,!?'-]*$", ErrorMessage = "Invalid characters")]
    public string Notes { get; set; }
    
    [Required]
    [Phone]
    [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$")]
    public string PhoneNumber { get; set; }
}
```

### 7. Audit Logging

**Priority**: MEDIUM  
**Effort**: Medium

Log security-relevant events:

```csharp
// Create AuditLog entity:
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }  // "Login", "FailedLogin", "DataAccess", etc.
    public string IpAddress { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
}

// Log failed logins:
await DbContext.AuditLogs.AddAsync(new AuditLog
{
    UserId = email,
    Action = "FailedLogin",
    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
    Timestamp = DateTime.UtcNow
});
```

### 8. Database Encryption at Rest

**Priority**: MEDIUM  
**Effort**: Low (if using Azure)

For Azure PostgreSQL:
- Enable "Transparent Data Encryption" in Azure Portal
- Automatically encrypts all data files

For self-hosted PostgreSQL:
```bash
# Enable pgcrypto extension:
CREATE EXTENSION pgcrypto;

# Encrypt sensitive columns:
ALTER TABLE "AspNetUsers" 
ADD COLUMN "EncryptedPhoneNumber" bytea;

UPDATE "AspNetUsers" 
SET "EncryptedPhoneNumber" = pgp_sym_encrypt("PhoneNumber", 'encryption_key');
```

### 9. Regular Security Updates

**Priority**: HIGH  
**Effort**: Ongoing

```bash
# Check for vulnerable packages:
dotnet list package --vulnerable

# Update packages regularly:
dotnet outdated
dotnet add package <PackageName> --version <NewVersion>
```

### 10. Remove Sensitive Logging

**Priority**: LOW  
**Effort**: Very Low

Remove or guard console logging in production:

```csharp
if (!_emailSettings.EnableNotifications)
{
    _logger.LogInformation("Email notifications disabled");
    return;
}

// Instead of:
Console.WriteLine($"Email sent to {toEmail}");

// Use:
_logger.LogInformation("Email sent successfully");  // Don't log email address
```

---

## 🔍 **Security Checklist for Production**

### Before Deploying to Production:

- [ ] **Move all secrets to environment variables or Azure Key Vault**
- [ ] **Change default admin password**
- [ ] **Enable HTTPS only (disable HTTP)**
- [ ] **Enable HSTS**
- [ ] **Configure firewall rules on PostgreSQL**
- [ ] **Use Stripe live keys (not test keys)**
- [ ] **Enable rate limiting**
- [ ] **Add security headers**
- [ ] **Enable email confirmation**
- [ ] **Strengthen password requirements**
- [ ] **Set up database backups**
- [ ] **Configure Azure Application Insights or logging**
- [ ] **Review and test all `[Authorize]` attributes**
- [ ] **Enable Azure DDoS protection**
- [ ] **Set up SSL certificate (Let's Encrypt or commercial)**
- [ ] **Configure CORS properly (don't allow `*`)**
- [ ] **Review all error messages (don't expose stack traces)**
- [ ] **Enable Azure Security Center scanning**

---

## 📊 **Threat Model**

### What Could Go Wrong?

| Threat | Likelihood | Impact | Mitigation |
|--------|-----------|--------|------------|
| SQL Injection | LOW | HIGH | EF Core parameterization ✅ |
| XSS Attacks | LOW | MEDIUM | Blazor auto-encoding ✅ |
| Brute Force Login | MEDIUM | HIGH | Add rate limiting ⚠️ |
| Stolen API Keys | HIGH | CRITICAL | Move to secrets ⚠️ |
| CSRF Attacks | LOW | MEDIUM | Anti-forgery tokens ✅ |
| Data Breach (DB) | MEDIUM | CRITICAL | Encryption, access control ⚠️ |
| DDoS Attacks | MEDIUM | HIGH | Azure DDoS, rate limiting ⚠️ |
| Session Hijacking | LOW | HIGH | HttpOnly cookies, HTTPS ✅ |
| Payment Fraud | LOW | HIGH | Stripe fraud detection ✅ |
| Insider Threats | LOW | HIGH | Audit logging, access control ⚠️ |

---

## 🚨 **Incident Response Plan**

### If a Security Breach Occurs:

1. **Immediately**:
   - Disable affected user accounts
   - Rotate all API keys and passwords
   - Take affected services offline if necessary

2. **Within 24 Hours**:
   - Investigate breach extent (check audit logs)
   - Notify affected users
   - Document incident details

3. **Within 72 Hours**:
   - Report to authorities if required (GDPR, CCPA)
   - Implement fixes
   - Conduct security audit

4. **Follow-up**:
   - Post-mortem analysis
   - Update security policies
   - Train team on lessons learned

---

## 📞 **Security Contacts**

- **Report vulnerabilities**: security@spabooker.com (create this!)
- **Stripe security**: security@stripe.com
- **PostgreSQL security**: https://www.postgresql.org/support/security/

---

## 🔗 **Resources**

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Stripe Security Best Practices](https://stripe.com/docs/security/guide)
- [Azure Security Documentation](https://learn.microsoft.com/en-us/azure/security/)

---

**Last Updated**: November 13, 2024  
**Next Review**: Before Production Deployment


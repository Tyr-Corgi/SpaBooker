using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Core.Validators;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;
using SpaBooker.Web.Components;
using SpaBooker.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting SpaBooker application");

    // Validate required configuration on startup
    ValidateRequiredConfiguration(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Blazor Server security and performance
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});

// Configure SignalR for Blazor Server
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB - prevent DoS via large messages
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Register circuit handler for cleanup
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, SpaBooker.Web.Handlers.CleanupCircuitHandler>();

// Configure CORS for future API integrations
var corsPolicy = builder.Configuration["Cors:PolicyName"] ?? "SpaBookerCorsPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:5226" };
var allowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials", true);
var maxAge = builder.Configuration.GetValue<int>("Cors:MaxAgeSeconds", 600);

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(maxAge));

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// Add Razor Pages for authentication
builder.Services.AddRazorPages();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Configure rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configure PostgreSQL Database with optimizations
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30); // 30 seconds timeout
            npgsqlOptions.MigrationsAssembly("SpaBooker.Infrastructure");
        });

    // Performance optimizations
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // Default to no-tracking for read queries
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment()); // Only in dev
    options.EnableDetailedErrors(builder.Environment.IsDevelopment()); // Only in dev
    options.ConfigureWarnings(warnings =>
    {
        warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
    });
});

// Add DbContextFactory for components that need to create their own contexts (Blazor Server)
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });

    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
},
ServiceLifetime.Scoped); // Use Scoped lifetime to avoid singleton/scoped conflict

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings - strengthened for security
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true; // Require special characters
    options.Password.RequiredLength = 12; // Increased from 8
    options.Password.RequiredUniqueChars = 4; // Require variety

    // Lockout settings - more aggressive
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // Increased from 15
    options.Lockout.MaxFailedAccessAttempts = 3; // Decreased from 5
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Sign-in settings - require email confirmation for security
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/auth/login";
    options.AccessDeniedPath = "/auth/access-denied";
    options.SlidingExpiration = true;
});

// Register Stripe service
builder.Services.AddScoped<IStripeService, StripeService>();

// Register membership credit service
builder.Services.AddScoped<IMembershipCreditService, MembershipCreditService>();

// Register email service
builder.Services.AddScoped<IEmailService, EmailService>();

// Register gift certificate service
builder.Services.AddScoped<IGiftCertificateService, GiftCertificateService>();

// Register analytics service
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Register Unit of Work for transaction management
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Audit Service for security logging
builder.Services.AddScoped<IAuditService, AuditService>();

// Register Booking Conflict Checker for preventing double bookings
builder.Services.AddScoped<IBookingConflictChecker, BookingConflictChecker>();

// Register Cache Service for performance optimization
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<BookingValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ServiceValidator>();

// Register background services
builder.Services.AddHostedService<BookingReminderService>();

// Configure booking settings
builder.Services.Configure<BookingSettings>(builder.Configuration.GetSection("BookingSettings"));

// Configure membership settings
builder.Services.Configure<MembershipSettings>(builder.Configuration.GetSection("MembershipSettings"));

// Configure email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure HSTS (HTTP Strict Transport Security)
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "db", "sql", "postgresql" })
    .AddCheck<StripeHealthCheck>("stripe", tags: new[] { "external", "stripe" })
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"), tags: new[] { "self" });

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Apply migrations
        await context.Database.MigrateAsync();
        
        // Seed roles and admin user
        await DbSeeder.SeedRolesAndAdminAsync(services);
        
        // Seed initial locations
        await DbSeeder.SeedLocationsAsync(context);
        
        // Seed membership plans
        await DbSeeder.SeedMembershipPlansAsync(context);
        
        // Seed spa services
        await DbSeeder.SeedServicesAsync(context);
        
        // Seed rooms
        await DbSeeder.SeedRoomsAsync(context);
        
        // Seed mock data (Development only)
        if (app.Environment.IsDevelopment())
        {
            await DbSeeder.SeedMockDataAsync(services);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Apply security headers to all responses
app.UseSecurityHeaders();

// Apply rate limiting before static files to protect all endpoints
app.UseIpRateLimiting();

// Apply CORS policy
app.UseCors(corsPolicy);

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

    app.MapControllers();
    app.MapRazorPages();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // Map health check endpoints
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    tags = e.Value.Tags,
                    data = e.Value.Data
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });

    // Separate endpoints for quick checks
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db")
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("self")
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Configuration validation method
static void ValidateRequiredConfiguration(IConfiguration configuration)
{
    var requiredSettings = new Dictionary<string, string>
    {
        { "ConnectionStrings:DefaultConnection", "Database connection string" },
        { "Stripe:PublishableKey", "Stripe publishable key" },
        { "Stripe:SecretKey", "Stripe secret key" },
        { "Stripe:WebhookSecret", "Stripe webhook secret" }
    };

    var missingSettings = new List<string>();

    foreach (var setting in requiredSettings)
    {
        var value = configuration[setting.Key];
        if (string.IsNullOrWhiteSpace(value) || 
            value.Contains("TO_BE_CONFIGURED") ||
            value.Contains("your_") ||
            value.Contains("YOUR_"))
        {
            missingSettings.Add($"  - {setting.Value} ({setting.Key})");
        }
    }

    if (missingSettings.Any())
    {
        var errorMessage = "⚠️  CONFIGURATION ERROR: Missing required secrets!\n\n" +
                          "The following secrets must be configured:\n" +
                          string.Join("\n", missingSettings) + "\n\n" +
                          "For development, use User Secrets:\n" +
                          "  cd src/SpaBooker.Web\n" +
                          "  dotnet user-secrets set \"Key:Name\" \"value\"\n\n" +
                          "See SECRETS_SETUP_GUIDE.md for detailed instructions.";
        
        throw new InvalidOperationException(errorMessage);
    }
}

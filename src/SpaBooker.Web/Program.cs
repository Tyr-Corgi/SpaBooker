using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;
using SpaBooker.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Razor Pages for authentication
builder.Services.AddRazorPages();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Configure PostgreSQL Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Sign-in settings - allow email as username
    options.SignIn.RequireConfirmedEmail = false;
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

// Register background services
builder.Services.AddHostedService<BookingReminderService>();

// Configure booking settings
builder.Services.Configure<BookingSettings>(builder.Configuration.GetSection("BookingSettings"));

// Configure membership settings
builder.Services.Configure<MembershipSettings>(builder.Configuration.GetSection("MembershipSettings"));

// Configure email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

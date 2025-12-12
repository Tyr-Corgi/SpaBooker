using Microsoft.EntityFrameworkCore;
using SpaBooker.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlite(connectionString);

using var context = new ApplicationDbContext(optionsBuilder.Options);

// Find admin user
var adminEmail = "admin@spabooker.com";
var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

if (admin == null)
{
    Console.WriteLine($"❌ Admin user '{adminEmail}' not found!");
    return;
}

Console.WriteLine($"Found admin user: {admin.Email}");
Console.WriteLine($"Current lockout status: {admin.LockoutEnd}");
Console.WriteLine($"Access failed count: {admin.AccessFailedCount}");

// Unlock the account
admin.LockoutEnd = null;
admin.AccessFailedCount = 0;
admin.LockoutEnabled = true; // Keep lockout enabled but clear the lockout

await context.SaveChangesAsync();

Console.WriteLine("✅ Admin account unlocked successfully!");
Console.WriteLine($"You can now log in with: {adminEmail}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();


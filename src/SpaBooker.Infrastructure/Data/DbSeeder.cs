using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpaBooker.Core.Entities;

namespace SpaBooker.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        string[] roleNames = { "Admin", "Therapist", "Client" };
        
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed default admin user
        var adminEmail = "admin@spabooker.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    public static async Task SeedLocationsAsync(ApplicationDbContext context)
    {
        if (!context.Locations.Any())
        {
            var locations = new List<Location>
            {
                new Location
                {
                    Name = "Downtown Spa & Wellness",
                    Address = "123 Main Street",
                    City = "Your City",
                    State = "YS",
                    ZipCode = "12345",
                    Phone = "(555) 123-4567",
                    Email = "downtown@spabooker.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Location
                {
                    Name = "Waterfront Spa Retreat",
                    Address = "456 Ocean Avenue",
                    City = "Your City",
                    State = "YS",
                    ZipCode = "12346",
                    Phone = "(555) 234-5678",
                    Email = "waterfront@spabooker.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Locations.AddRange(locations);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedMembershipPlansAsync(ApplicationDbContext context)
    {
        if (!context.MembershipPlans.Any())
        {
            var plans = new List<MembershipPlan>
            {
                new MembershipPlan
                {
                    Name = "Bronze Membership",
                    Description = "Perfect for occasional spa visits",
                    MonthlyPrice = 49.99m,
                    MonthlyCredits = 50.00m,
                    DiscountPercentage = 10.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MembershipPlan
                {
                    Name = "Silver Membership",
                    Description = "Great value for regular spa-goers",
                    MonthlyPrice = 89.99m,
                    MonthlyCredits = 100.00m,
                    DiscountPercentage = 15.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new MembershipPlan
                {
                    Name = "Gold Membership",
                    Description = "Ultimate spa experience with premium benefits",
                    MonthlyPrice = 149.99m,
                    MonthlyCredits = 175.00m,
                    DiscountPercentage = 20.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.MembershipPlans.AddRange(plans);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedServicesAsync(ApplicationDbContext context)
    {
        if (!context.SpaServices.Any())
        {
            var locations = await context.Locations.ToListAsync();
            if (!locations.Any()) return;

            var services = new List<SpaService>();

            foreach (var location in locations)
            {
                services.AddRange(new[]
                {
                    new SpaService
                    {
                        Name = "Swedish Massage",
                        Description = "A gentle, relaxing full-body massage using long strokes and kneading techniques.",
                        DurationMinutes = 60,
                        BasePrice = 95.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SpaService
                    {
                        Name = "Deep Tissue Massage",
                        Description = "Intensive massage targeting deep muscle layers to relieve chronic tension.",
                        DurationMinutes = 90,
                        BasePrice = 130.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SpaService
                    {
                        Name = "Hot Stone Massage",
                        Description = "Therapeutic massage using heated stones to melt away tension and stress.",
                        DurationMinutes = 75,
                        BasePrice = 120.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SpaService
                    {
                        Name = "Aromatherapy Massage",
                        Description = "Relaxing massage enhanced with essential oils tailored to your needs.",
                        DurationMinutes = 60,
                        BasePrice = 105.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SpaService
                    {
                        Name = "Couples Massage",
                        Description = "Side-by-side massage experience perfect for couples or friends.",
                        DurationMinutes = 60,
                        BasePrice = 190.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SpaService
                    {
                        Name = "Luxury Facial Treatment",
                        Description = "Premium facial with deep cleansing, exfoliation, and hydration.",
                        DurationMinutes = 90,
                        BasePrice = 145.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SpaService
                    {
                        Name = "Body Scrub & Wrap",
                        Description = "Full body exfoliation followed by a nourishing wrap treatment.",
                        DurationMinutes = 60,
                        BasePrice = 110.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SpaService
                    {
                        Name = "Exclusive Spa Day Package",
                        Description = "Full day experience with massage, facial, and body treatment. Members only.",
                        DurationMinutes = 240,
                        BasePrice = 395.00m,
                        LocationId = location.Id,
                        IsActive = true,
                        RequiresMembership = true,
                        CreatedAt = DateTime.UtcNow
                    }
                });
            }

            context.SpaServices.AddRange(services);
            await context.SaveChangesAsync();
        }
    }
}


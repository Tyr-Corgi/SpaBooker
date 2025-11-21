using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;

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

            var result = await userManager.CreateAsync(adminUser, "Admin123!@#$");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed mock therapist user
        var therapistEmail = "therapist@spabooker.com";
        var therapistUser = await userManager.FindByEmailAsync(therapistEmail);

        if (therapistUser == null)
        {
            therapistUser = new ApplicationUser
            {
                UserName = therapistEmail,
                Email = therapistEmail,
                FirstName = "Sarah",
                LastName = "Johnson",
                PhoneNumber = "(555) 987-6543",
                Specialties = "Swedish, Deep Tissue, Hot Stone",
                Bio = "Licensed massage therapist with 8 years of experience specializing in therapeutic and relaxation techniques.",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(therapistUser, "Therapist123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(therapistUser, "Therapist");
            }
        }

        // Seed mock client user
        var clientEmail = "client@spabooker.com";
        var clientUser = await userManager.FindByEmailAsync(clientEmail);

        if (clientUser == null)
        {
            clientUser = new ApplicationUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                FirstName = "Emma",
                LastName = "Williams",
                PhoneNumber = "(555) 123-9876",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(clientUser, "Client123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(clientUser, "Client");
            }
        }

        // Seed Cartoon Network Characters as Clients
        await SeedCartoonNetworkClientsAsync(userManager);

        // Seed Cartoon Network Characters as Therapists
        await SeedCartoonNetworkTherapistsAsync(userManager);
    }

    private static async Task SeedCartoonNetworkClientsAsync(UserManager<ApplicationUser> userManager)
    {
        var cartoonClients = new List<(string firstName, string lastName, string email, string phone)>
        {
            ("Ben", "Tennyson", "ben.tennyson@cntest.com", "(555) 101-0001"),
            ("Dexter", "McPherson", "dexter.mcpherson@cntest.com", "(555) 101-0002"),
            ("Numbuh", "One", "numbuh.one@cntest.com", "(555) 101-0003"),
            ("Ed", "Smith", "ed.smith@cntest.com", "(555) 101-0004"),
            ("Johnny", "Bravo", "johnny.bravo@cntest.com", "(555) 101-0005"),
            ("Courage", "Dog", "courage.dog@cntest.com", "(555) 101-0006"),
            ("Samurai", "Jack", "samurai.jack@cntest.com", "(555) 101-0007"),
            ("Gwen", "Tennyson", "gwen.tennyson@cntest.com", "(555) 101-0008"),
            ("Finn", "Human", "finn.human@cntest.com", "(555) 101-0009"),
            ("Jake", "Dog", "jake.dog@cntest.com", "(555) 101-0010"),
            ("Blossom", "Powerpuff", "blossom.powerpuff@cntest.com", "(555) 101-0011"),
            ("Bubbles", "Powerpuff", "bubbles.powerpuff@cntest.com", "(555) 101-0012"),
            ("Buttercup", "Powerpuff", "buttercup.powerpuff@cntest.com", "(555) 101-0013"),
            ("Mandy", "Grim", "mandy.grim@cntest.com", "(555) 101-0014"),
            ("Billy", "Grim", "billy.grim@cntest.com", "(555) 101-0015")
        };

        foreach (var (firstName, lastName, email, phone) in cartoonClients)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phone,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "CartoonTest123!@#$");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Client");
                }
            }
        }
    }

    private static async Task SeedCartoonNetworkTherapistsAsync(UserManager<ApplicationUser> userManager)
    {
        var cartoonTherapists = new List<(string firstName, string lastName, string email, string phone, string specialties, string bio)>
        {
            ("Kevin", "Levin", "kevin.levin@cntest.com", "(555) 201-0001", 
             "Deep Tissue, Sports Massage, Hot Stone", 
             "Former alien hero turned expert massage therapist. Specializes in muscle recovery."),
            
            ("Raven", "Azarath", "raven.azarath@cntest.com", "(555) 201-0002", 
             "Aromatherapy, Meditation, Energy Healing", 
             "Mystical healer with expertise in relaxation and spiritual wellness."),
            
            ("Marceline", "Abadeer", "marceline.abadeer@cntest.com", "(555) 201-0003", 
             "Swedish Massage, Reflexology, Music Therapy", 
             "Thousand-year-old vampire queen who found her calling in therapeutic massage."),
            
            ("Princess", "Bubblegum", "princess.bubblegum@cntest.com", "(555) 201-0004", 
             "Scientific Massage, Sports Recovery, Injury Rehabilitation", 
             "Scientifically-minded therapist with advanced knowledge of human anatomy."),
            
            ("Starfire", "Tamaranean", "starfire.tamaranean@cntest.com", "(555) 201-0005", 
             "Hot Stone, Thai Massage, Couples Therapy", 
             "Enthusiastic alien therapist bringing joy and warmth to every session.")
        };

        foreach (var (firstName, lastName, email, phone, specialties, bio) in cartoonTherapists)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phone,
                    Specialties = specialties,
                    Bio = bio,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "CartoonTest123!@#$");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Therapist");
                }
            }
        }
    }

    public static async Task SeedTherapistSchedulesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Get all therapists
        var therapistRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Therapist");
        if (therapistRole == null) return;

        var therapistUserIds = await context.UserRoles
            .Where(ur => ur.RoleId == therapistRole.Id)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var therapists = await context.Users
            .Where(u => therapistUserIds.Contains(u.Id))
            .ToListAsync();

        // Check if schedules already exist
        if (await context.TherapistAvailability.AnyAsync())
        {
            // Schedules already seeded
            return;
        }

        // Create standard Mon-Fri 9am-5pm schedule for all therapists
        var schedules = new List<TherapistAvailability>();
        var workDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var startTime = new TimeSpan(9, 0, 0); // 9 AM
        var endTime = new TimeSpan(17, 0, 0);  // 5 PM

        foreach (var therapist in therapists)
        {
            foreach (var day in workDays)
            {
                schedules.Add(new TherapistAvailability
                {
                    TherapistId = therapist.Id,
                    DayOfWeek = day,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsAvailable = true,
                    SpecificDate = null, // Weekly recurring
                    Notes = "Standard work schedule",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (schedules.Any())
        {
            context.TherapistAvailability.AddRange(schedules);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedMonthlyTherapistSchedulesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Get 4 specific therapists (Kevin, Raven, Marceline, Princess Bubblegum)
        var therapistEmails = new[]
        {
            "kevin.levin@cntest.com",
            "raven.azarath@cntest.com",
            "marceline.abadeer@cntest.com",
            "princess.bubblegum@cntest.com"
        };

        var therapists = new List<ApplicationUser>();
        foreach (var email in therapistEmails)
        {
            var therapist = await userManager.FindByEmailAsync(email);
            if (therapist != null)
            {
                therapists.Add(therapist);
            }
        }

        if (therapists.Count != 4)
        {
            // Not all therapists found, skip seeding
            return;
        }

        // Get today's date and the date 30 days from now (in UTC)
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        // Check if any specific date schedules already exist for this period
        var existingSchedules = await context.TherapistAvailability
            .Where(a => a.SpecificDate != null && 
                       a.SpecificDate >= startDate && 
                       a.SpecificDate < endDate &&
                       therapists.Select(t => t.Id).Contains(a.TherapistId))
            .AnyAsync();

        if (existingSchedules)
        {
            // Schedules already seeded for this period
            return;
        }

        // Create daily schedules for the next 30 days
        var schedules = new List<TherapistAvailability>();
        var startTime = new TimeSpan(9, 0, 0);  // 9 AM
        var endTime = new TimeSpan(17, 0, 0);   // 5 PM

        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            // Ensure date is UTC (AddDays may lose Kind)
            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            // Schedule all 4 therapists for each day
            foreach (var therapist in therapists)
            {
                schedules.Add(new TherapistAvailability
                {
                    TherapistId = therapist.Id,
                    DayOfWeek = utcDate.DayOfWeek,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsAvailable = true,
                    SpecificDate = utcDate,
                    Notes = $"Scheduled for {utcDate:MMM dd, yyyy}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (schedules.Any())
        {
            context.TherapistAvailability.AddRange(schedules);
            await context.SaveChangesAsync();
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

    public static async Task SeedRoomsAsync(ApplicationDbContext context)
    {
        if (!context.Rooms.Any())
        {
            var locations = await context.Locations.ToListAsync();
            if (!locations.Any()) return;

            var services = await context.SpaServices.ToListAsync();
            if (!services.Any()) return;

            var rooms = new List<Room>();

            // Define room configurations
            var roomConfigs = new[]
            {
                new { Name = "Room 1", Color = "#3B82F6", Order = 1, ServiceNames = new[] { "Swedish Massage", "Deep Tissue Massage", "Hot Stone Massage", "Aromatherapy Massage", "Couples Massage", "Luxury Facial Treatment", "Body Scrub & Wrap", "Exclusive Spa Day Package" } },
                new { Name = "Room 2", Color = "#10B981", Order = 2, ServiceNames = new[] { "Swedish Massage", "Deep Tissue Massage", "Aromatherapy Massage", "Body Scrub & Wrap" } },
                new { Name = "Room 3", Color = "#8B5CF6", Order = 3, ServiceNames = new[] { "Hot Stone Massage", "Luxury Facial Treatment", "Body Scrub & Wrap" } },
                new { Name = "Room 4", Color = "#F97316", Order = 4, ServiceNames = new[] { "Couples Massage", "Exclusive Spa Day Package" } }
            };

            // Create rooms for each location
            foreach (var location in locations)
            {
                var locationServices = services.Where(s => s.LocationId == location.Id).ToList();

                foreach (var config in roomConfigs)
                {
                    var room = new Room
                    {
                        Name = config.Name,
                        ColorCode = config.Color,
                        LocationId = location.Id,
                        DisplayOrder = config.Order,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    rooms.Add(room);
                }
            }

            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();

            // Now create room service capabilities
            var capabilities = new List<RoomServiceCapability>();

            var allRooms = await context.Rooms.Include(r => r.Location).ToListAsync();

            foreach (var room in allRooms)
            {
                var config = roomConfigs.FirstOrDefault(c => c.Name == room.Name);
                if (config != null)
                {
                    var locationServices = services.Where(s => s.LocationId == room.LocationId).ToList();

                    foreach (var serviceName in config.ServiceNames)
                    {
                        var service = locationServices.FirstOrDefault(s => s.Name == serviceName);
                        if (service != null)
                        {
                            capabilities.Add(new RoomServiceCapability
                            {
                                RoomId = room.Id,
                                ServiceId = service.Id,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            context.RoomServiceCapabilities.AddRange(capabilities);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedMockDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Check if mock data already exists
        var existingMockUser = await userManager.FindByEmailAsync("spongeb@mockdata.spabooker.com");
        if (existingMockUser != null)
        {
            // Mock data already seeded
            return;
        }

        var locations = await context.Locations.ToListAsync();
        if (!locations.Any()) return;

        var services = await context.SpaServices.ToListAsync();
        if (!services.Any()) return;

        // Get existing client for bookings
        var existingClient = await userManager.FindByEmailAsync("client@spabooker.com");

        // 1. CREATE MOCK USERS
        var mockUsers = new List<(string email, string firstName, string lastName, string role, string? phone, string? specialties, string? bio)>
        {
            ("spongeb@mockdata.spabooker.com", "[MOCK] SpongeBob", "SquarePants", "Therapist", "(555) 111-1111", "Swedish, Hot Stone", "Enthusiastic massage therapist who loves making clients happy!"),
            ("lisas@mockdata.spabooker.com", "[MOCK] Lisa", "Simpson", "Therapist", "(555) 222-2222", "Deep Tissue, Aromatherapy", "Intelligent and caring therapist with a passion for wellness."),
            ("ashk@mockdata.spabooker.com", "[MOCK] Ash", "Ketchum", "Client", "(555) 333-3333", null, null),
            ("dorae@mockdata.spabooker.com", "[MOCK] Dora", "Explorer", "Client", "(555) 444-4444", null, null),
            ("bent@mockdata.spabooker.com", "[MOCK] Ben", "Tennyson", "Client", "(555) 555-5555", null, null)
        };

        var createdMockUsers = new List<ApplicationUser>();

        foreach (var (email, firstName, lastName, role, phone, specialties, bio) in mockUsers)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                LocationId = locations.First().Id
            };

            if (role == "Therapist")
            {
                user.Specialties = specialties;
                user.Bio = bio;
            }

            var result = await userManager.CreateAsync(user, "MockPass123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                createdMockUsers.Add(user);
            }
        }

        // Get the therapists for bookings
        var mockTherapist1 = createdMockUsers.FirstOrDefault(u => u.Email == "spongeb@mockdata.spabooker.com");
        var mockTherapist2 = createdMockUsers.FirstOrDefault(u => u.Email == "lisas@mockdata.spabooker.com");
        var mockClient1 = createdMockUsers.FirstOrDefault(u => u.Email == "ashk@mockdata.spabooker.com");
        var mockClient2 = createdMockUsers.FirstOrDefault(u => u.Email == "dorae@mockdata.spabooker.com");
        var mockClient3 = createdMockUsers.FirstOrDefault(u => u.Email == "bent@mockdata.spabooker.com");

        // 2. CREATE MOCK BOOKINGS (20+ bookings with mixed dates for comprehensive dashboard testing)
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        var mockBookings = new List<Core.Entities.Booking>();

        if (mockTherapist1 != null && mockClient1 != null && services.Any())
        {
            // PAST BOOKINGS - THIS MONTH (for revenue stats)
            // 25 days ago
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0, DateTimeKind.Utc).AddDays(-25),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc).AddDays(-25),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services[0].BasePrice,
                Notes = "[MOCK] Perfect Swedish massage!",
                CreatedAt = DateTime.UtcNow.AddDays(-26)
            });

            // 20 days ago
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient2?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 1 ? services[1].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0, DateTimeKind.Utc).AddDays(-20),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 15, 30, 0, DateTimeKind.Utc).AddDays(-20),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services.Count > 1 ? services[1].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Excellent deep tissue work",
                CreatedAt = DateTime.UtcNow.AddDays(-21)
            });

            // 18 days ago
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient3?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services.Count > 2 ? services[2].Id : services[0].Id,
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc).AddDays(-18),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 12, 15, 0, DateTimeKind.Utc).AddDays(-18),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services.Count > 2 ? services[2].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Hot stones were perfect temperature",
                CreatedAt = DateTime.UtcNow.AddDays(-19)
            });

            // 15 days ago
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = existingClient?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 3 ? services[3].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0, DateTimeKind.Utc).AddDays(-15),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 16, 0, 0, DateTimeKind.Utc).AddDays(-15),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services.Count > 3 ? services[3].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Aromatherapy was heavenly",
                CreatedAt = DateTime.UtcNow.AddDays(-16)
            });

            // 12 days ago
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services.Count > 4 ? services[4].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0, DateTimeKind.Utc).AddDays(-12),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc).AddDays(-12),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services.Count > 4 ? services[4].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Couples massage was romantic",
                CreatedAt = DateTime.UtcNow.AddDays(-13)
            });

            // 10 days ago
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient2?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services.Count > 5 ? services[5].Id : services[0].Id,
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0, DateTimeKind.Utc).AddDays(-10),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 14, 30, 0, DateTimeKind.Utc).AddDays(-10),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services.Count > 5 ? services[5].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Facial left skin glowing",
                CreatedAt = DateTime.UtcNow.AddDays(-11)
            });

            // 8 days ago
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient3?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0, DateTimeKind.Utc).AddDays(-8),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0, DateTimeKind.Utc).AddDays(-8),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services[0].BasePrice,
                Notes = "[MOCK] Very relaxing",
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            });

            // 5 days ago, completed
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0, DateTimeKind.Utc).AddDays(-5),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc).AddDays(-5),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services[0].BasePrice,
                Notes = "[MOCK] Great relaxing session!",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            });

            // 3 days ago, completed
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient2?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 1 ? services[1].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0, DateTimeKind.Utc).AddDays(-3),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 15, 30, 0, DateTimeKind.Utc).AddDays(-3),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services.Count > 1 ? services[1].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Excellent deep tissue work",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            });

            // 2 days ago, completed
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services.Count > 6 ? services[6].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 16, 0, 0, DateTimeKind.Utc).AddDays(-2),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0, DateTimeKind.Utc).AddDays(-2),
                Status = Core.Enums.BookingStatus.Completed,
                TotalPrice = services.Count > 6 ? services[6].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Body scrub made skin so smooth",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            });

            // 1 day ago, cancelled
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = existingClient?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0, DateTimeKind.Utc).AddDays(-1),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0, DateTimeKind.Utc).AddDays(-1),
                Status = Core.Enums.BookingStatus.Cancelled,
                TotalPrice = services[0].BasePrice,
                Notes = "[MOCK] Client had to cancel due to emergency",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });

            // TODAY - Morning booking, confirmed
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 2 ? services[2].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 11, 15, 0, DateTimeKind.Utc),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services.Count > 2 ? services[2].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Hot stone massage - client requested extra heat",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

            // TODAY - Midday booking, confirmed
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient2?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services.Count > 3 ? services[3].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 12, 30, 0, DateTimeKind.Utc),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 13, 30, 0, DateTimeKind.Utc),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services.Count > 3 ? services[3].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Aromatherapy appointment",
                CreatedAt = DateTime.UtcNow
            });

            // TODAY - Afternoon booking, confirmed (Waterfront Spa)
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient3?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services[0].Id,
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,  // Waterfront Spa
                StartTime = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 16, 0, 0, DateTimeKind.Utc),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services[0].BasePrice,
                Notes = "[MOCK] Regular Swedish massage",
                CreatedAt = DateTime.UtcNow
            });

            // TODAY - Evening booking, pending (Downtown Spa)
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = existingClient?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 1 ? services[1].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 19, 30, 0, DateTimeKind.Utc),
                Status = Core.Enums.BookingStatus.Pending,
                TotalPrice = services.Count > 1 ? services[1].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Awaiting therapist confirmation",
                CreatedAt = DateTime.UtcNow
            });

            // TODAY - Late afternoon booking, confirmed (Waterfront Spa)
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient2?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 2 ? services[2].Id : services[0].Id,
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,  // Waterfront Spa
                StartTime = new DateTime(now.Year, now.Month, now.Day, 16, 30, 0, DateTimeKind.Utc),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 17, 45, 0, DateTimeKind.Utc),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services.Count > 2 ? services[2].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Hot stone massage at waterfront location",
                CreatedAt = DateTime.UtcNow
            });

            // FUTURE - 1 day from now
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient3?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 4 ? services[4].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0, DateTimeKind.Utc).AddDays(1),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0, DateTimeKind.Utc).AddDays(1),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services.Count > 4 ? services[4].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Couples massage anniversary special",
                CreatedAt = DateTime.UtcNow
            });

            // FUTURE - 2 days from now (Waterfront Spa)
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient2?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services.Count > 1 ? services[1].Id : services[0].Id,
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,  // Waterfront Spa
                StartTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc).AddDays(2),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 12, 30, 0, DateTimeKind.Utc).AddDays(2),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services.Count > 1 ? services[1].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Deep tissue for sports recovery",
                CreatedAt = DateTime.UtcNow
            });

            // FUTURE - 3 days from now
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 9, 30, 0, DateTimeKind.Utc).AddDays(3),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 10, 30, 0, DateTimeKind.Utc).AddDays(3),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services[0].BasePrice,
                Notes = "[MOCK] Early morning relaxation",
                CreatedAt = DateTime.UtcNow
            });

            // FUTURE - 5 days from now
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = existingClient?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 3 ? services[3].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0, DateTimeKind.Utc).AddDays(5),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0, DateTimeKind.Utc).AddDays(5),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services.Count > 3 ? services[3].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Aromatherapy stress relief",
                CreatedAt = DateTime.UtcNow
            });

            // FUTURE - 7 days from now
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services[0].Id,
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0, DateTimeKind.Utc).AddDays(7),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc).AddDays(7),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services[0].BasePrice,
                Notes = "[MOCK] Waterfront location visit",
                CreatedAt = DateTime.UtcNow
            });

            // FUTURE - 10 days from now, pending
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient3?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                ServiceId = services.Count > 2 ? services[2].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 16, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 17, 15, 0, DateTimeKind.Utc).AddDays(10),
                Status = Core.Enums.BookingStatus.Pending,
                TotalPrice = services.Count > 2 ? services[2].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Awaiting confirmation",
                CreatedAt = DateTime.UtcNow
            });

            // FUTURE - 14 days from now
            mockBookings.Add(new Core.Entities.Booking
            {
                ClientId = mockClient2?.Id ?? mockClient1.Id,
                TherapistId = mockTherapist1.Id,
                ServiceId = services.Count > 5 ? services[5].Id : services[0].Id,
                LocationId = locations[0].Id,
                StartTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc).AddDays(14),
                EndTime = new DateTime(now.Year, now.Month, now.Day, 12, 30, 0, DateTimeKind.Utc).AddDays(14),
                Status = Core.Enums.BookingStatus.Confirmed,
                TotalPrice = services.Count > 5 ? services[5].BasePrice : services[0].BasePrice,
                Notes = "[MOCK] Luxury facial treatment",
                CreatedAt = DateTime.UtcNow
            });
        }

        context.Bookings.AddRange(mockBookings);
        await context.SaveChangesAsync();

        // Assign some bookings to rooms (leaving some as "Unassigned" to demonstrate the feature)
        var rooms = await context.Rooms.Where(r => r.LocationId == locations[0].Id).OrderBy(r => r.DisplayOrder).ToListAsync();
        if (rooms.Any() && mockBookings.Any())
        {
            // Assign first 5 bookings to rooms (leaving others unassigned)
            for (int i = 0; i < Math.Min(5, mockBookings.Count); i++)
            {
                // Cycle through rooms
                var roomIndex = i % rooms.Count;
                mockBookings[i].RoomId = rooms[roomIndex].Id;
            }
            await context.SaveChangesAsync();
        }

        // 3. CREATE MOCK INVENTORY ITEMS (10-12 items)
        var mockInventoryItems = new List<InventoryItem>
        {
            new InventoryItem
            {
                Name = "[MOCK] Lavender Essential Oil",
                Description = "Premium lavender oil for aromatherapy",
                SKU = "MOCK-LAV-001",
                CurrentStock = 15,
                MinimumStock = 10,
                Unit = "bottle",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Massage Oil - Relaxing",
                Description = "Unscented massage oil",
                SKU = "MOCK-OIL-002",
                CurrentStock = 25,
                MinimumStock = 15,
                Unit = "bottle",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Hot Stones Set",
                Description = "Basalt stones for hot stone therapy",
                SKU = "MOCK-STONE-003",
                CurrentStock = 3,
                MinimumStock = 5,
                Unit = "set",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Facial Cream - Hydrating",
                Description = "Luxury hydrating facial cream",
                SKU = "MOCK-FACE-004",
                CurrentStock = 8,
                MinimumStock = 10,
                Unit = "jar",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Massage Table Sheets",
                Description = "Cotton sheets for massage tables",
                SKU = "MOCK-SHEET-005",
                CurrentStock = 50,
                MinimumStock = 30,
                Unit = "sheet",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Eucalyptus Essential Oil",
                Description = "Refreshing eucalyptus oil",
                SKU = "MOCK-EUC-006",
                CurrentStock = 12,
                MinimumStock = 8,
                Unit = "bottle",
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Body Scrub - Sea Salt",
                Description = "Exfoliating sea salt body scrub",
                SKU = "MOCK-SCRUB-007",
                CurrentStock = 6,
                MinimumStock = 10,
                Unit = "jar",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Aromatherapy Candles",
                Description = "Scented candles for ambiance",
                SKU = "MOCK-CAND-008",
                CurrentStock = 20,
                MinimumStock = 15,
                Unit = "piece",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Massage Bolsters",
                Description = "Support cushions for massage",
                SKU = "MOCK-BOLS-009",
                CurrentStock = 8,
                MinimumStock = 6,
                Unit = "piece",
                LocationId = locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Name = "[MOCK] Peppermint Essential Oil",
                Description = "Cooling peppermint oil",
                SKU = "MOCK-PEPP-010",
                CurrentStock = 4,
                MinimumStock = 8,
                Unit = "bottle",
                LocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.InventoryItems.AddRange(mockInventoryItems);
        await context.SaveChangesAsync();

        // 4. CREATE INVENTORY TRANSACTIONS (2-3 for realism)
        var adminUser = await userManager.FindByEmailAsync("admin@spabooker.com");
        if (adminUser != null && mockInventoryItems.Any())
        {
            var transactions = new List<InventoryTransaction>
            {
                new InventoryTransaction
                {
                    InventoryItemId = mockInventoryItems[0].Id,
                    Quantity = 10,
                    Type = "Add",
                    Notes = "Initial stock",
                    PerformedBy = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new InventoryTransaction
                {
                    InventoryItemId = mockInventoryItems[2].Id,
                    Quantity = 2,
                    Type = "Remove",
                    Notes = "Used for hot stone massage",
                    PerformedBy = mockTherapist1?.Id ?? adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new InventoryTransaction
                {
                    InventoryItemId = mockInventoryItems[1].Id,
                    Quantity = 5,
                    Type = "Add",
                    Notes = "Restocked",
                    PerformedBy = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            context.InventoryTransactions.AddRange(transactions);
            await context.SaveChangesAsync();
        }

        // 5. CREATE MOCK GIFT CERTIFICATES (2-3)
        if (adminUser != null)
        {
            var mockGiftCerts = new List<GiftCertificate>
            {
                new GiftCertificate
                {
                    Code = "MOCK-AANG-2024",
                    PurchasedByUserId = adminUser.Id,
                    RecipientName = "[MOCK] Aang Avatar",
                    RecipientEmail = "aang@mockdata.spabooker.com",
                    OriginalAmount = 100.00m,
                    PurchasePrice = 100.00m,
                    RemainingBalance = 100.00m,
                    Status = "Active",
                    IsActive = true,
                    PurchasedAt = DateTime.UtcNow.AddDays(-15),
                    ExpiresAt = DateTime.UtcNow.AddDays(45),
                    RestrictedToLocationId = locations[0].Id,
                    PersonalMessage = "Enjoy your spa day! From Katara",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new GiftCertificate
                {
                    Code = "MOCK-KIM-2024",
                    PurchasedByUserId = adminUser.Id,
                    RecipientName = "[MOCK] Kim Possible",
                    RecipientEmail = "kimp@mockdata.spabooker.com",
                    OriginalAmount = 150.00m,
                    PurchasePrice = 150.00m,
                    RemainingBalance = 75.00m,
                    Status = "PartiallyUsed",
                    IsActive = true,
                    IsRedeemed = true,
                    RedeemedByUserId = mockClient1?.Id ?? adminUser.Id,
                    RedeemedAt = DateTime.UtcNow.AddDays(-15),
                    PurchasedAt = DateTime.UtcNow.AddDays(-30),
                    ExpiresAt = DateTime.UtcNow.AddDays(60),
                    RestrictedToLocationId = locations[0].Id,
                    PersonalMessage = "You deserve this! From Ron",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new GiftCertificate
                {
                    Code = "MOCK-STEVEN-2024",
                    PurchasedByUserId = mockClient2?.Id ?? adminUser.Id,
                    RecipientName = "[MOCK] Steven Universe",
                    RecipientEmail = "steven@mockdata.spabooker.com",
                    OriginalAmount = 50.00m,
                    PurchasePrice = 50.00m,
                    RemainingBalance = 50.00m,
                    Status = "Active",
                    IsActive = true,
                    PurchasedAt = DateTime.UtcNow.AddDays(-5),
                    ExpiresAt = DateTime.UtcNow.AddDays(55),
                    RestrictedToLocationId = locations.Count > 1 ? locations[1].Id : locations[0].Id,
                    PersonalMessage = "Relax and shine! From Garnet",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            context.GiftCertificates.AddRange(mockGiftCerts);
            await context.SaveChangesAsync();
        }

        // 6. CREATE THERAPIST AVAILABILITY BLOCKS (3-4)
        if (mockTherapist1 != null)
        {
            var blocks = new List<TherapistAvailability>
            {
                new TherapistAvailability
                {
                    TherapistId = mockTherapist1.Id,
                    DayOfWeek = DateTime.UtcNow.AddDays(3).DayOfWeek,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    IsAvailable = false,
                    SpecificDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(3),
                    Notes = "Vacation day",
                    CreatedAt = DateTime.UtcNow
                },
                new TherapistAvailability
                {
                    TherapistId = mockTherapist2?.Id ?? mockTherapist1.Id,
                    DayOfWeek = DateTime.UtcNow.AddDays(8).DayOfWeek,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    IsAvailable = false,
                    SpecificDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(8),
                    Notes = "Training session",
                    CreatedAt = DateTime.UtcNow
                },
                new TherapistAvailability
                {
                    TherapistId = mockTherapist1.Id,
                    DayOfWeek = DateTime.UtcNow.AddDays(14).DayOfWeek,
                    StartTime = new TimeSpan(0, 0, 0),
                    EndTime = new TimeSpan(23, 59, 59),
                    IsAvailable = false,
                    SpecificDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(14),
                    Notes = "Personal day off",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.TherapistAvailability.AddRange(blocks);
            await context.SaveChangesAsync();
        }

        // 7. CREATE CARTOON CHARACTER BOOKINGS (Past, Present, Future)
        await SeedCartoonCharacterBookingsAsync(context, userManager);
    }

    private static async Task SeedCartoonCharacterBookingsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Get cartoon clients
        var benTennyson = await userManager.FindByEmailAsync("ben.tennyson@cntest.com");
        var dexter = await userManager.FindByEmailAsync("dexter.mcpherson@cntest.com");
        var numbuhOne = await userManager.FindByEmailAsync("numbuh.one@cntest.com");
        var ed = await userManager.FindByEmailAsync("ed.smith@cntest.com");
        var johnny = await userManager.FindByEmailAsync("johnny.bravo@cntest.com");
        var courage = await userManager.FindByEmailAsync("courage.dog@cntest.com");
        var jack = await userManager.FindByEmailAsync("samurai.jack@cntest.com");
        var gwen = await userManager.FindByEmailAsync("gwen.tennyson@cntest.com");
        var finn = await userManager.FindByEmailAsync("finn.human@cntest.com");
        var jake = await userManager.FindByEmailAsync("jake.dog@cntest.com");
        var blossom = await userManager.FindByEmailAsync("blossom.powerpuff@cntest.com");
        var bubbles = await userManager.FindByEmailAsync("bubbles.powerpuff@cntest.com");
        var buttercup = await userManager.FindByEmailAsync("buttercup.powerpuff@cntest.com");
        var mandy = await userManager.FindByEmailAsync("mandy.grim@cntest.com");
        var billy = await userManager.FindByEmailAsync("billy.grim@cntest.com");

        // Get cartoon therapists
        var kevin = await userManager.FindByEmailAsync("kevin.levin@cntest.com");
        var raven = await userManager.FindByEmailAsync("raven.azarath@cntest.com");
        var marceline = await userManager.FindByEmailAsync("marceline.abadeer@cntest.com");
        var bubblegum = await userManager.FindByEmailAsync("princess.bubblegum@cntest.com");
        var starfire = await userManager.FindByEmailAsync("starfire.tamaranean@cntest.com");

        // Get services and rooms
        var services = await context.SpaServices.Take(5).ToListAsync();
        var rooms = await context.Rooms.Take(3).ToListAsync();
        var locations = await context.Locations.Take(1).ToListAsync();

        if (!services.Any() || !rooms.Any() || kevin == null || benTennyson == null)
        {
            return; // Can't seed bookings without services, rooms, or users
        }

        var now = DateTime.UtcNow;
        var bookings = new List<Booking>();

        // PAST APPOINTMENTS (Completed)
        if (benTennyson != null && kevin != null)
        {
            bookings.Add(new Booking
            {
                ClientId = benTennyson.Id,
                TherapistId = kevin.Id,
                ServiceId = services[0].Id,
                RoomId = rooms[0].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(-30).AddHours(10),
                EndTime = now.AddDays(-30).AddHours(11),
                Status = BookingStatus.Completed,
                TotalPrice = 85.00m,
                Notes = "Great deep tissue massage after alien battle!",
                CreatedAt = now.AddDays(-31)
            });
        }

        if (gwen != null && raven != null)
        {
            bookings.Add(new Booking
            {
                ClientId = gwen.Id,
                TherapistId = raven.Id,
                ServiceId = services[1].Id,
                RoomId = rooms[1].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(-25).AddHours(14),
                EndTime = now.AddDays(-25).AddHours(15),
                Status = BookingStatus.Completed,
                TotalPrice = 90.00m,
                Notes = "Loved the aromatherapy and meditation session!",
                CreatedAt = now.AddDays(-26)
            });
        }

        if (finn != null && marceline != null)
        {
            bookings.Add(new Booking
            {
                ClientId = finn.Id,
                TherapistId = marceline.Id,
                ServiceId = services[2].Id,
                RoomId = rooms[2].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(-20).AddHours(16),
                EndTime = now.AddDays(-20).AddHours(17),
                Status = BookingStatus.Completed,
                TotalPrice = 95.00m,
                Notes = "Mathematical! Best massage in the Land of Ooo!",
                CreatedAt = now.AddDays(-21)
            });
        }

        if (johnny != null && starfire != null)
        {
            bookings.Add(new Booking
            {
                ClientId = johnny.Id,
                TherapistId = starfire.Id,
                ServiceId = services[0].Id,
                RoomId = rooms[0].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(-15).AddHours(11),
                EndTime = now.AddDays(-15).AddHours(12),
                Status = BookingStatus.Completed,
                TotalPrice = 85.00m,
                Notes = "Whoa, mama! This was amazing!",
                CreatedAt = now.AddDays(-16)
            });
        }

        if (blossom != null && bubblegum != null)
        {
            bookings.Add(new Booking
            {
                ClientId = blossom.Id,
                TherapistId = bubblegum.Id,
                ServiceId = services[3].Id,
                RoomId = rooms[1].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(-10).AddHours(9),
                EndTime = now.AddDays(-10).AddHours(10),
                Status = BookingStatus.Completed,
                TotalPrice = 100.00m,
                Notes = "Perfect recovery after saving Townsville!",
                CreatedAt = now.AddDays(-11)
            });
        }

        // PAST APPOINTMENTS (No-Show)
        if (ed != null && kevin != null)
        {
            bookings.Add(new Booking
            {
                ClientId = ed.Id,
                TherapistId = kevin.Id,
                ServiceId = services[0].Id,
                RoomId = rooms[0].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(-5).AddHours(13),
                EndTime = now.AddDays(-5).AddHours(14),
                Status = BookingStatus.NoShow,
                TotalPrice = 85.00m,
                Notes = "Client did not show up",
                CreatedAt = now.AddDays(-6)
            });
        }

        // PAST APPOINTMENTS (Cancelled)
        if (courage != null && raven != null)
        {
            bookings.Add(new Booking
            {
                ClientId = courage.Id,
                TherapistId = raven.Id,
                ServiceId = services[1].Id,
                RoomId = rooms[1].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(-3).AddHours(15),
                EndTime = now.AddDays(-3).AddHours(16),
                Status = BookingStatus.Cancelled,
                TotalPrice = 90.00m,
                Notes = "Cancelled - client was too scared",
                CreatedAt = now.AddDays(-4)
            });
        }

        // CURRENT/TODAY APPOINTMENTS (Confirmed)
        if (jake != null && marceline != null)
        {
            bookings.Add(new Booking
            {
                ClientId = jake.Id,
                TherapistId = marceline.Id,
                ServiceId = services[2].Id,
                RoomId = rooms[2].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddHours(2),
                EndTime = now.AddHours(3),
                Status = BookingStatus.Confirmed,
                TotalPrice = 95.00m,
                Notes = "Looking forward to stretching out!",
                CreatedAt = now.AddDays(-2)
            });
        }

        if (dexter != null && bubblegum != null)
        {
            bookings.Add(new Booking
            {
                ClientId = dexter.Id,
                TherapistId = bubblegum.Id,
                ServiceId = services[3].Id,
                RoomId = rooms[0].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddHours(4),
                EndTime = now.AddHours(5),
                Status = BookingStatus.Confirmed,
                TotalPrice = 100.00m,
                Notes = "Science and relaxation combined!",
                CreatedAt = now.AddDays(-3)
            });
        }

        // NEAR FUTURE APPOINTMENTS (Tomorrow - Next Week)
        if (numbuhOne != null && kevin != null)
        {
            bookings.Add(new Booking
            {
                ClientId = numbuhOne.Id,
                TherapistId = kevin.Id,
                ServiceId = services[0].Id,
                RoomId = rooms[1].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(1).AddHours(10),
                EndTime = now.AddDays(1).AddHours(11),
                Status = BookingStatus.Confirmed,
                TotalPrice = 85.00m,
                Notes = "Mission: Relaxation - Kids Next Door approved!",
                CreatedAt = now.AddDays(-1)
            });
        }

        if (bubbles != null && starfire != null)
        {
            bookings.Add(new Booking
            {
                ClientId = bubbles.Id,
                TherapistId = starfire.Id,
                ServiceId = services[1].Id,
                RoomId = rooms[2].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(2).AddHours(14),
                EndTime = now.AddDays(2).AddHours(15),
                Status = BookingStatus.Confirmed,
                TotalPrice = 90.00m,
                Notes = "Can't wait for the hot stone massage!",
                CreatedAt = now.AddDays(-2)
            });
        }

        if (buttercup != null && kevin != null)
        {
            bookings.Add(new Booking
            {
                ClientId = buttercup.Id,
                TherapistId = kevin.Id,
                ServiceId = services[0].Id,
                RoomId = rooms[0].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(3).AddHours(11),
                EndTime = now.AddDays(3).AddHours(12),
                Status = BookingStatus.Confirmed,
                TotalPrice = 85.00m,
                Notes = "Bring on the deep tissue!",
                CreatedAt = now.AddDays(-1)
            });
        }

        if (jack != null && raven != null)
        {
            bookings.Add(new Booking
            {
                ClientId = jack.Id,
                TherapistId = raven.Id,
                ServiceId = services[4].Id,
                RoomId = rooms[1].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(5).AddHours(13),
                EndTime = now.AddDays(5).AddHours(14),
                Status = BookingStatus.Confirmed,
                TotalPrice = 110.00m,
                Notes = "Meditation and healing after long journey",
                CreatedAt = now.AddDays(-3)
            });
        }

        // FUTURE APPOINTMENTS (Next 2-4 Weeks)
        if (mandy != null && raven != null)
        {
            bookings.Add(new Booking
            {
                ClientId = mandy.Id,
                TherapistId = raven.Id,
                ServiceId = services[1].Id,
                RoomId = rooms[2].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(14).AddHours(10),
                EndTime = now.AddDays(14).AddHours(11),
                Status = BookingStatus.Confirmed,
                TotalPrice = 90.00m,
                Notes = "Dark and relaxing, just how I like it",
                CreatedAt = now.AddDays(-1)
            });
        }

        if (billy != null && starfire != null)
        {
            bookings.Add(new Booking
            {
                ClientId = billy.Id,
                TherapistId = starfire.Id,
                ServiceId = services[0].Id,
                RoomId = rooms[0].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(21).AddHours(15),
                EndTime = now.AddDays(21).AddHours(16),
                Status = BookingStatus.Confirmed,
                TotalPrice = 85.00m,
                Notes = "I love massages! They're my favorite!",
                CreatedAt = now
            });
        }

        if (benTennyson != null && marceline != null)
        {
            bookings.Add(new Booking
            {
                ClientId = benTennyson.Id,
                TherapistId = marceline.Id,
                ServiceId = services[2].Id,
                RoomId = rooms[1].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(28).AddHours(9),
                EndTime = now.AddDays(28).AddHours(10),
                Status = BookingStatus.Confirmed,
                TotalPrice = 95.00m,
                Notes = "Back for another session!",
                CreatedAt = now
            });
        }

        // PENDING APPOINTMENTS (Awaiting confirmation)
        if (gwen != null && bubblegum != null)
        {
            bookings.Add(new Booking
            {
                ClientId = gwen.Id,
                TherapistId = bubblegum.Id,
                ServiceId = services[3].Id,
                RoomId = rooms[2].Id,
                LocationId = locations[0].Id,
                StartTime = now.AddDays(7).AddHours(16),
                EndTime = now.AddDays(7).AddHours(17),
                Status = BookingStatus.Pending,
                TotalPrice = 100.00m,
                Notes = "Awaiting therapist confirmation",
                CreatedAt = now
            });
        }

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();
    }
}


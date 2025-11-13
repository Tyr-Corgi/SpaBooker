using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class BookingReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingReminderService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public BookingReminderService(
        IServiceProvider serviceProvider,
        ILogger<BookingReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking Reminder Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync();
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Booking Reminder Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes on error
            }
        }

        _logger.LogInformation("Booking Reminder Service stopped");
    }

    private async Task SendRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Get bookings that are ~24 hours away and haven't been reminded yet
        var now = DateTime.UtcNow;
        var reminderWindow = now.AddHours(23); // 23 hours from now
        var reminderWindowEnd = now.AddHours(25); // 25 hours from now

        var bookingsToRemind = await context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed
                     && b.StartTime >= reminderWindow
                     && b.StartTime <= reminderWindowEnd)
            .Select(b => b.Id)
            .ToListAsync();

        if (bookingsToRemind.Any())
        {
            _logger.LogInformation("Found {Count} bookings to send reminders for", bookingsToRemind.Count);

            foreach (var bookingId in bookingsToRemind)
            {
                try
                {
                    await emailService.SendBookingReminderAsync(bookingId);
                    _logger.LogInformation("Reminder sent for booking {BookingId}", bookingId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for booking {BookingId}", bookingId);
                }
            }
        }
    }
}


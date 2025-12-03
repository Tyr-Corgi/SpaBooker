using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Common;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;
using Microsoft.Extensions.Options;

namespace SpaBooker.Infrastructure.Services;

public class TherapistAvailabilityService : ITherapistAvailabilityService
{
    private readonly ApplicationDbContext _context;
    private readonly BufferTimeSettings _bufferSettings;

    public TherapistAvailabilityService(
        ApplicationDbContext context,
        IOptions<BufferTimeSettings> bufferSettings)
    {
        _context = context;
        _bufferSettings = bufferSettings.Value;
    }

    public async Task<List<ApplicationUser>> GetAvailableTherapistsAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        // Get all therapists qualified for this service
        var qualifiedTherapists = await GetQualifiedTherapistsForServiceAsync(serviceId);

        // Filter to only available therapists
        var availableTherapists = new List<ApplicationUser>();
        foreach (var therapist in qualifiedTherapists)
        {
            var isAvailable = await IsTherapistAvailableAsync(therapist.Id, startTime, endTime, excludeBookingId);
            if (isAvailable.IsSuccess && isAvailable.Value == true)
            {
                availableTherapists.Add(therapist);
            }
        }

        return availableTherapists;
    }

    public async Task<Result<bool>> IsTherapistAvailableAsync(string therapistId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        // Check if therapist exists
        var therapistExists = await _context.Users.AnyAsync(u => u.Id == therapistId);
        if (!therapistExists)
        {
            return Result.Failure<bool>(Error.TherapistNotFound);
        }

        // Check therapist schedule/availability settings
        var dayOfWeek = startTime.DayOfWeek;
        var availability = await _context.TherapistAvailability
            .FirstOrDefaultAsync(ta => ta.TherapistId == therapistId &&
                                      ta.DayOfWeek == dayOfWeek &&
                                      ta.IsAvailable);

        // If no availability record, assume not available
        if (availability == null)
        {
            return Result.Success(false);
        }

        // Check if requested time is within therapist's working hours
        var requestedStartTime = startTime.TimeOfDay;
        var requestedEndTime = endTime.TimeOfDay;

        if (requestedStartTime < availability.StartTime || requestedEndTime > availability.EndTime)
        {
            return Result.Success(false);
        }

        // Get buffer time
        var bufferMinutes = _bufferSettings.BufferMinutes;

        // Check for conflicting bookings
        var query = _context.Bookings
            .Where(b => b.TherapistId == therapistId &&
                       b.Status != BookingStatus.Cancelled &&
                       b.Status != BookingStatus.NoShow);

        // Exclude specific booking if provided (for updates)
        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        var hasConflict = await query.AnyAsync(b =>
            // Check if new booking overlaps with existing booking + buffer
            (startTime >= b.StartTime && startTime < b.EndTime.AddMinutes(bufferMinutes)) ||
            (endTime > b.StartTime && endTime <= b.EndTime.AddMinutes(bufferMinutes)) ||
            (startTime <= b.StartTime && endTime >= b.EndTime.AddMinutes(bufferMinutes))
        );

        return Result.Success(!hasConflict);
    }

    public async Task<Result<ApplicationUser>> FindBestAvailableTherapistAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        var availableTherapists = await GetAvailableTherapistsAsync(serviceId, startTime, endTime, excludeBookingId);

        if (availableTherapists.Count == 0)
        {
            return Result.Failure<ApplicationUser>(Error.TherapistNotAvailable);
        }

        // Get booking counts for the day to balance workload
        var date = startTime.Date;
        var therapistBookingCounts = new Dictionary<string, int>();

        foreach (var therapist in availableTherapists)
        {
            var bookingCount = await _context.Bookings
                .CountAsync(b => b.TherapistId == therapist.Id &&
                               b.StartTime.Date == date &&
                               b.Status != BookingStatus.Cancelled &&
                               b.Status != BookingStatus.NoShow);

            therapistBookingCounts[therapist.Id] = bookingCount;
        }

        // Return therapist with least bookings for the day
        var bestTherapist = availableTherapists
            .OrderBy(t => therapistBookingCounts[t.Id])
            .First();

        return Result.Success(bestTherapist);
    }

    public async Task<List<ApplicationUser>> GetQualifiedTherapistsForServiceAsync(int serviceId)
    {
        var therapistIds = await _context.ServiceTherapists
            .Where(st => st.ServiceId == serviceId)
            .Select(st => st.TherapistId)
            .ToListAsync();

        return await _context.Users
            .Where(u => therapistIds.Contains(u.Id))
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<TimeSlot>>> GetTherapistAvailabilityForDayAsync(DateTime date, int? locationId = null)
    {
        // Get all therapists (optionally filtered by location)
        var therapistsQuery = _context.Users
            .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Therapist")));

        var therapists = await therapistsQuery.ToListAsync();

        var result = new Dictionary<string, List<TimeSlot>>();

        // Get all bookings for the day
        var bookings = await _context.Bookings
            .Where(b => b.StartTime.Date == date.Date &&
                       b.TherapistId != null &&
                       b.Status != BookingStatus.Cancelled &&
                       b.Status != BookingStatus.NoShow)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        foreach (var therapist in therapists)
        {
            var therapistBookings = bookings.Where(b => b.TherapistId == therapist.Id).ToList();
            var timeSlots = new List<TimeSlot>();

            // Get therapist availability for this day of week
            var dayOfWeek = date.DayOfWeek;
            var availability = await _context.TherapistAvailability
                .FirstOrDefaultAsync(ta => ta.TherapistId == therapist.Id &&
                                          ta.DayOfWeek == dayOfWeek &&
                                          ta.IsAvailable);

            if (availability == null)
            {
                // Therapist not available this day
                result[therapist.Id] = new List<TimeSlot>();
                continue;
            }

            // Generate hourly slots within therapist's working hours
            var startHour = availability.StartTime.Hours;
            var endHour = availability.EndTime.Hours;

            for (var hour = startHour; hour < endHour; hour++)
            {
                var slotStart = date.Date.AddHours(hour);
                var slotEnd = slotStart.AddHours(1);

                var bufferMinutes = _bufferSettings.BufferMinutes;

                // Check if this slot is available
                var hasBooking = therapistBookings.Any(b =>
                    (slotStart >= b.StartTime && slotStart < b.EndTime.AddMinutes(bufferMinutes)) ||
                    (slotEnd > b.StartTime && slotEnd <= b.EndTime.AddMinutes(bufferMinutes)) ||
                    (slotStart <= b.StartTime && slotEnd >= b.EndTime.AddMinutes(bufferMinutes))
                );

                var booking = hasBooking ? therapistBookings.FirstOrDefault(b =>
                    (slotStart >= b.StartTime && slotStart < b.EndTime.AddMinutes(bufferMinutes)) ||
                    (slotEnd > b.StartTime && slotEnd <= b.EndTime.AddMinutes(bufferMinutes)) ||
                    (slotStart <= b.StartTime && slotEnd >= b.EndTime.AddMinutes(bufferMinutes))
                ) : null;

                timeSlots.Add(new TimeSlot
                {
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsAvailable = !hasBooking,
                    BookingId = booking?.Id
                });
            }

            result[therapist.Id] = timeSlots;
        }

        return result;
    }

    public async Task<List<TherapistAvailability>> GetTherapistScheduleAsync(string therapistId, DateTime startDate, DateTime endDate)
    {
        return await _context.TherapistAvailability
            .Where(ta => ta.TherapistId == therapistId)
            .OrderBy(ta => ta.DayOfWeek)
            .ToListAsync();
    }
}


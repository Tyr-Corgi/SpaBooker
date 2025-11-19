using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class BookingConflictChecker : IBookingConflictChecker
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookingConflictChecker> _logger;

    public BookingConflictChecker(ApplicationDbContext context, ILogger<BookingConflictChecker> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsTherapistAvailableAsync(string therapistId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        var conflicts = await _context.Bookings
            .Where(b => b.TherapistId == therapistId
                && b.Status != BookingStatus.Cancelled
                && (excludeBookingId == null || b.Id != excludeBookingId)
                && b.StartTime < endTime
                && b.EndTime > startTime)
            .AnyAsync();

        return !conflicts;
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        var conflicts = await _context.Bookings
            .Where(b => b.RoomId == roomId
                && b.Status != BookingStatus.Cancelled
                && (excludeBookingId == null || b.Id != excludeBookingId)
                && b.StartTime < endTime
                && b.EndTime > startTime)
            .AnyAsync();

        return !conflicts;
    }

    public async Task<BookingConflictResult> CheckForConflictsAsync(
        string therapistId,
        int? roomId,
        DateTime startTime,
        DateTime endTime,
        int? excludeBookingId = null)
    {
        var result = new BookingConflictResult();

        // Check therapist conflicts
        var therapistConflicts = await _context.Bookings
            .Where(b => b.TherapistId == therapistId
                && b.Status != BookingStatus.Cancelled
                && (excludeBookingId == null || b.Id != excludeBookingId)
                && b.StartTime < endTime
                && b.EndTime > startTime)
            .Select(b => new { b.Id, b.StartTime, b.EndTime })
            .ToListAsync();

        if (therapistConflicts.Any())
        {
            result.HasConflict = true;
            result.ConflictingBookingIds.AddRange(therapistConflicts.Select(c => c.Id));
            result.ConflictReasons.Add($"Therapist is already booked for overlapping time slots: {string.Join(", ", therapistConflicts.Select(c => $"{c.StartTime:HH:mm}-{c.EndTime:HH:mm}"))}");
        }

        // Check room conflicts if room is specified
        if (roomId.HasValue)
        {
            var roomConflicts = await _context.Bookings
                .Where(b => b.RoomId == roomId
                    && b.Status != BookingStatus.Cancelled
                    && (excludeBookingId == null || b.Id != excludeBookingId)
                    && b.StartTime < endTime
                    && b.EndTime > startTime)
                .Select(b => new { b.Id, b.StartTime, b.EndTime })
                .ToListAsync();

            if (roomConflicts.Any())
            {
                result.HasConflict = true;
                result.ConflictingBookingIds.AddRange(roomConflicts.Select(c => c.Id).Where(id => !result.ConflictingBookingIds.Contains(id)));
                result.ConflictReasons.Add($"Room is already booked for overlapping time slots: {string.Join(", ", roomConflicts.Select(c => $"{c.StartTime:HH:mm}-{c.EndTime:HH:mm}"))}");
            }
        }

        if (result.HasConflict)
        {
            _logger.LogWarning("Booking conflict detected for therapist {TherapistId}, room {RoomId} at {StartTime}-{EndTime}. Conflicts: {Reasons}",
                therapistId, roomId, startTime, endTime, string.Join("; ", result.ConflictReasons));
        }

        return result;
    }
}


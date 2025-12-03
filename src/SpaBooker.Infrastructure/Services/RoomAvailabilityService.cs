using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Common;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;
using Microsoft.Extensions.Options;

namespace SpaBooker.Infrastructure.Services;

public class RoomAvailabilityService : IRoomAvailabilityService
{
    private readonly ApplicationDbContext _context;
    private readonly BufferTimeSettings _bufferSettings;

    public RoomAvailabilityService(
        ApplicationDbContext context,
        IOptions<BufferTimeSettings> bufferSettings)
    {
        _context = context;
        _bufferSettings = bufferSettings.Value;
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        // Get all rooms that support this service
        var compatibleRoomIds = await _context.RoomServiceCapabilities
            .Where(rsc => rsc.ServiceId == serviceId)
            .Select(rsc => rsc.RoomId)
            .ToListAsync();

        var rooms = await _context.Rooms
            .Where(r => r.IsActive && compatibleRoomIds.Contains(r.Id))
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync();

        // Filter to only available rooms
        var availableRooms = new List<Room>();
        foreach (var room in rooms)
        {
            var isAvailable = await IsRoomAvailableAsync(room.Id, startTime, endTime, excludeBookingId);
            if (isAvailable.IsSuccess && isAvailable.Value == true)
            {
                availableRooms.Add(room);
            }
        }

        return availableRooms;
    }

    public async Task<Result<bool>> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        // Check if room exists
        var roomExists = await _context.Rooms.AnyAsync(r => r.Id == roomId && r.IsActive);
        if (!roomExists)
        {
            return Result.Failure<bool>(Error.RoomNotFound);
        }

        // Get buffer time
        var bufferMinutes = _bufferSettings.BufferMinutes;

        // Check for conflicting bookings (include buffer time)
        var query = _context.Bookings
            .Where(b => b.RoomId == roomId &&
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

    public async Task<Result<Room>> FindBestAvailableRoomAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        var availableRooms = await GetAvailableRoomsAsync(serviceId, startTime, endTime, excludeBookingId);

        if (availableRooms.Count == 0)
        {
            return Result.Failure<Room>(Error.RoomNotAvailable);
        }

        // Return first room (already sorted by DisplayOrder)
        return Result.Success(availableRooms.First());
    }

    public async Task<List<Room>> GetRoomsForServiceAsync(int serviceId)
    {
        var roomIds = await _context.RoomServiceCapabilities
            .Where(rsc => rsc.ServiceId == serviceId)
            .Select(rsc => rsc.RoomId)
            .ToListAsync();

        return await _context.Rooms
            .Where(r => r.IsActive && roomIds.Contains(r.Id))
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Dictionary<int, List<TimeSlot>>> GetRoomAvailabilityForDayAsync(int locationId, DateTime date)
    {
        // Get all rooms for the location
        var rooms = await _context.Rooms
            .Where(r => r.IsActive && r.LocationId == locationId)
            .ToListAsync();

        var result = new Dictionary<int, List<TimeSlot>>();

        // Get all bookings for the day
        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.StartTime.Date == date.Date &&
                       b.RoomId.HasValue &&
                       b.Status != BookingStatus.Cancelled &&
                       b.Status != BookingStatus.NoShow)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        foreach (var room in rooms)
        {
            var roomBookings = bookings.Where(b => b.RoomId == room.Id).ToList();
            var timeSlots = new List<TimeSlot>();

            // Business hours: 9 AM to 9 PM
            var startHour = 9;
            var endHour = 21;

            for (var hour = startHour; hour < endHour; hour++)
            {
                var slotStart = date.Date.AddHours(hour);
                var slotEnd = slotStart.AddHours(1);

                var bufferMinutes = _bufferSettings.BufferMinutes;

                // Check if this slot is available
                var hasBooking = roomBookings.Any(b =>
                    (slotStart >= b.StartTime && slotStart < b.EndTime.AddMinutes(bufferMinutes)) ||
                    (slotEnd > b.StartTime && slotEnd <= b.EndTime.AddMinutes(bufferMinutes)) ||
                    (slotStart <= b.StartTime && slotEnd >= b.EndTime.AddMinutes(bufferMinutes))
                );

                var booking = hasBooking ? roomBookings.FirstOrDefault(b =>
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

            result[room.Id] = timeSlots;
        }

        return result;
    }
}


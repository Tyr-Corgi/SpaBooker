using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Common;
using SpaBooker.Core.DTOs.Booking;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingConflictChecker _conflictChecker;
    private readonly IRoomAvailabilityService _roomAvailability;
    private readonly ITherapistAvailabilityService _therapistAvailability;
    private readonly IAuditService _auditService;

    public BookingService(
        ApplicationDbContext context,
        IBookingConflictChecker conflictChecker,
        IRoomAvailabilityService roomAvailability,
        ITherapistAvailabilityService therapistAvailability,
        IAuditService auditService)
    {
        _context = context;
        _conflictChecker = conflictChecker;
        _roomAvailability = roomAvailability;
        _therapistAvailability = therapistAvailability;
        _auditService = auditService;
    }

    public async Task<Result<Booking>> CreateBookingAsync(CreateBookingDto dto)
    {
        // Validate the booking first
        var validationResult = await ValidateBookingAsync(dto);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Booking>(validationResult.Error!);
        }

        // Get the service to calculate price
        var service = await _context.SpaServices.FindAsync(dto.ServiceId);
        if (service == null)
        {
            return Result.Failure<Booking>(Error.ServiceNotFound);
        }

        if (!service.IsActive)
        {
            return Result.Failure<Booking>(Error.ServiceNotActive);
        }

        // Create the booking
        var booking = new Booking
        {
            ClientId = dto.ClientId,
            ServiceId = dto.ServiceId,
            TherapistId = dto.TherapistId ?? string.Empty,
            RoomId = dto.RoomId,
            LocationId = dto.LocationId ?? 1, // Default to location 1 if not specified
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = BookingStatus.Confirmed,
            Notes = dto.Notes ?? string.Empty,
            TotalPrice = service.BasePrice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        
        try
        {
            await _context.SaveChangesAsync();
            
            // Log audit
            await _auditService.LogBookingAsync(
                "Create",
                booking.Id,
                $"Booking created for client {dto.ClientId}"
            );
            
            return Result.Success(booking);
        }
        catch (Exception ex)
        {
            return Result.Failure<Booking>(new Error("Booking.CreateFailed", ex.Message));
        }
    }

    public async Task<Result<Booking>> UpdateBookingAsync(UpdateBookingDto dto)
    {
        var booking = await _context.Bookings
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.Id == dto.Id);

        if (booking == null)
        {
            return Result.Failure<Booking>(Error.BookingNotFound);
        }

        // Validate changes won't create conflicts
        if (dto.TherapistId != null || dto.RoomId.HasValue || dto.StartTime.HasValue || dto.EndTime.HasValue)
        {
            var startTime = dto.StartTime ?? booking.StartTime;
            var endTime = dto.EndTime ?? booking.EndTime;
            var therapistId = dto.TherapistId ?? booking.TherapistId;
            var roomId = dto.RoomId ?? booking.RoomId;

            // Check for conflicts (excluding this booking)
            if (therapistId != null)
            {
                var therapistAvailable = await _therapistAvailability.IsTherapistAvailableAsync(
                    therapistId, startTime, endTime, booking.Id);
                
                if (therapistAvailable.IsFailure || therapistAvailable.Value != true)
                {
                    return Result.Failure<Booking>(Error.TherapistNotAvailable);
                }
            }

            if (roomId.HasValue)
            {
                var roomAvailable = await _roomAvailability.IsRoomAvailableAsync(
                    roomId.Value, startTime, endTime, booking.Id);
                
                if (roomAvailable.IsFailure || roomAvailable.Value != true)
                {
                    return Result.Failure<Booking>(Error.RoomNotAvailable);
                }
            }
        }

        // Update fields
        if (dto.TherapistId != null) booking.TherapistId = dto.TherapistId;
        if (dto.RoomId.HasValue) booking.RoomId = dto.RoomId;
        if (dto.StartTime.HasValue) booking.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue) booking.EndTime = dto.EndTime.Value;
        if (dto.Notes != null) booking.Notes = dto.Notes;
        
        booking.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            
            await _auditService.LogBookingAsync(
                "Update",
                booking.Id,
                "Booking updated"
            );
            
            return Result.Success(booking);
        }
        catch (Exception ex)
        {
            return Result.Failure<Booking>(new Error("Booking.UpdateFailed", ex.Message));
        }
    }

    public async Task<Result> CancelBookingAsync(int bookingId, string reason)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        
        if (booking == null)
        {
            return Result.Failure(Error.BookingNotFound);
        }

        booking.Status = BookingStatus.Cancelled;
        booking.Notes = (booking.Notes ?? "") + $"\nCancelled: {reason}";
        booking.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            
            await _auditService.LogBookingAsync(
                "Cancel",
                booking.Id,
                $"Booking cancelled: {reason}"
            );
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Booking.CancelFailed", ex.Message));
        }
    }

    public async Task<Result> ConfirmBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        
        if (booking == null)
        {
            return Result.Failure(Error.BookingNotFound);
        }

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Booking.ConfirmFailed", ex.Message));
        }
    }

    public async Task<Result> CompleteBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        
        if (booking == null)
        {
            return Result.Failure(Error.BookingNotFound);
        }

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Booking.CompleteFailed", ex.Message));
        }
    }

    public async Task<Result> MarkAsNoShowAsync(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        
        if (booking == null)
        {
            return Result.Failure(Error.BookingNotFound);
        }

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Booking.NoShowFailed", ex.Message));
        }
    }

    public async Task<List<BookingDto>> GetBookingsForDateAsync(DateTime date, int? locationId = null)
    {
        var query = _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Service)
            .ThenInclude(s => s!.Location)
            .Include(b => b.Therapist)
            .Include(b => b.Room)
            .Where(b => b.StartTime.Date == date.Date);

        if (locationId.HasValue)
        {
            query = query.Where(b => b.Service != null && b.Service.LocationId == locationId.Value);
        }

        var bookings = await query
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetBookingsForDateRangeAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var query = _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Service)
            .ThenInclude(s => s!.Location)
            .Include(b => b.Therapist)
            .Include(b => b.Room)
            .Where(b => b.StartTime >= startDate && b.StartTime <= endDate);

        if (locationId.HasValue)
        {
            query = query.Where(b => b.Service != null && b.Service.LocationId == locationId.Value);
        }

        var bookings = await query
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetBookingsForTherapistAsync(string therapistId, DateTime date)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Service)
            .Include(b => b.Therapist)
            .Include(b => b.Room)
            .Where(b => b.TherapistId == therapistId && b.StartTime.Date == date.Date)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetBookingsForRoomAsync(int roomId, DateTime date)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Service)
            .Include(b => b.Therapist)
            .Include(b => b.Room)
            .Where(b => b.RoomId == roomId && b.StartTime.Date == date.Date)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetClientBookingsAsync(string clientId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Service)
            .Include(b => b.Therapist)
            .Include(b => b.Room)
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.StartTime)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<Result<BookingDto>> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Service)
            .Include(b => b.Therapist)
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return Result.Failure<BookingDto>(Error.BookingNotFound);
        }

        return Result.Success(MapToDto(booking));
    }

    public async Task<Result> ValidateBookingAsync(CreateBookingDto dto)
    {
        // Validate times
        if (dto.EndTime <= dto.StartTime)
        {
            return Result.Failure(Error.InvalidTimeSlot);
        }

        // Validate client exists
        var clientExists = await _context.Users.AnyAsync(u => u.Id == dto.ClientId);
        if (!clientExists)
        {
            return Result.Failure(Error.ClientNotFound);
        }

        // Validate service exists and is active
        var service = await _context.SpaServices.FindAsync(dto.ServiceId);
        if (service == null)
        {
            return Result.Failure(Error.ServiceNotFound);
        }
        if (!service.IsActive)
        {
            return Result.Failure(Error.ServiceNotActive);
        }

        // Validate therapist if provided
        if (dto.TherapistId != null)
        {
            var therapistAvailable = await _therapistAvailability.IsTherapistAvailableAsync(
                dto.TherapistId, dto.StartTime, dto.EndTime);
            
            if (therapistAvailable.IsFailure || therapistAvailable.Value != true)
            {
                return Result.Failure(Error.TherapistNotAvailable);
            }
        }

        // Validate room if provided
        if (dto.RoomId.HasValue)
        {
            var roomAvailable = await _roomAvailability.IsRoomAvailableAsync(
                dto.RoomId.Value, dto.StartTime, dto.EndTime);
            
            if (roomAvailable.IsFailure || roomAvailable.Value != true)
            {
                return Result.Failure(Error.RoomNotAvailable);
            }
        }

        return Result.Success();
    }

    private static BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            ClientId = booking.ClientId,
            ClientName = $"{booking.Client!.FirstName} {booking.Client.LastName}",
            ServiceId = booking.ServiceId ?? 0,
            ServiceName = booking.Service?.Name ?? "N/A",
            TherapistId = booking.TherapistId,
            TherapistName = booking.Therapist != null 
                ? $"{booking.Therapist.FirstName} {booking.Therapist.LastName}" 
                : null,
            RoomId = booking.RoomId,
            RoomName = booking.Room?.Name,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            Notes = booking.Notes,
            TotalPrice = booking.TotalPrice,
            CreatedAt = booking.CreatedAt
        };
    }
}


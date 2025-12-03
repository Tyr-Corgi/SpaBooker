using SpaBooker.Core.Common;
using SpaBooker.Core.DTOs.Booking;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;

namespace SpaBooker.Core.Interfaces;

public interface IBookingService
{
    /// <summary>
    /// Creates a new booking with validation and conflict checking
    /// </summary>
    Task<Result<Booking>> CreateBookingAsync(CreateBookingDto dto);
    
    /// <summary>
    /// Updates an existing booking
    /// </summary>
    Task<Result<Booking>> UpdateBookingAsync(UpdateBookingDto dto);
    
    /// <summary>
    /// Cancels a booking with reason
    /// </summary>
    Task<Result> CancelBookingAsync(int bookingId, string reason);
    
    /// <summary>
    /// Confirms a pending booking
    /// </summary>
    Task<Result> ConfirmBookingAsync(int bookingId);
    
    /// <summary>
    /// Marks a booking as completed
    /// </summary>
    Task<Result> CompleteBookingAsync(int bookingId);
    
    /// <summary>
    /// Marks a booking as no-show
    /// </summary>
    Task<Result> MarkAsNoShowAsync(int bookingId);
    
    /// <summary>
    /// Gets bookings for a specific date
    /// </summary>
    Task<List<BookingDto>> GetBookingsForDateAsync(DateTime date, int? locationId = null);
    
    /// <summary>
    /// Gets bookings for a date range
    /// </summary>
    Task<List<BookingDto>> GetBookingsForDateRangeAsync(DateTime startDate, DateTime endDate, int? locationId = null);
    
    /// <summary>
    /// Gets bookings for a specific therapist on a date
    /// </summary>
    Task<List<BookingDto>> GetBookingsForTherapistAsync(string therapistId, DateTime date);
    
    /// <summary>
    /// Gets bookings for a specific room on a date
    /// </summary>
    Task<List<BookingDto>> GetBookingsForRoomAsync(int roomId, DateTime date);
    
    /// <summary>
    /// Gets all bookings for a client
    /// </summary>
    Task<List<BookingDto>> GetClientBookingsAsync(string clientId);
    
    /// <summary>
    /// Gets a booking by ID
    /// </summary>
    Task<Result<BookingDto>> GetBookingByIdAsync(int bookingId);
    
    /// <summary>
    /// Checks if a booking can be created without conflicts
    /// </summary>
    Task<Result> ValidateBookingAsync(CreateBookingDto dto);
}


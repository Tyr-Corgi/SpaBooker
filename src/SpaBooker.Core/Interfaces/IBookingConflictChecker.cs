namespace SpaBooker.Core.Interfaces;

public interface IBookingConflictChecker
{
    /// <summary>
    /// Check if a therapist is available for the given time slot (excluding specified booking ID)
    /// </summary>
    Task<bool> IsTherapistAvailableAsync(string therapistId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Check if a room is available for the given time slot (excluding specified booking ID)
    /// </summary>
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Check for any booking conflicts and return detailed information
    /// </summary>
    Task<BookingConflictResult> CheckForConflictsAsync(string therapistId, int? roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
}

public class BookingConflictResult
{
    public bool HasConflict { get; set; }
    public List<string> ConflictReasons { get; set; } = new();
    public List<int> ConflictingBookingIds { get; set; } = new();
}


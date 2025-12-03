using SpaBooker.Core.Common;
using SpaBooker.Core.Entities;

namespace SpaBooker.Core.Interfaces;

public interface IRoomAvailabilityService
{
    /// <summary>
    /// Gets all rooms available for a specific service and time slot
    /// </summary>
    Task<List<Room>> GetAvailableRoomsAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Checks if a specific room is available for the given time slot
    /// </summary>
    Task<Result<bool>> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Finds the best available room for a service (considers room priority/order)
    /// </summary>
    Task<Result<Room>> FindBestAvailableRoomAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Gets all rooms that support a specific service
    /// </summary>
    Task<List<Room>> GetRoomsForServiceAsync(int serviceId);
    
    /// <summary>
    /// Gets room availability for a full day
    /// </summary>
    Task<Dictionary<int, List<TimeSlot>>> GetRoomAvailabilityForDayAsync(int locationId, DateTime date);
}

public class TimeSlot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public int? BookingId { get; set; }
}


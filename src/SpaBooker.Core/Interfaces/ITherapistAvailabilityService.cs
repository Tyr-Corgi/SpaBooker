using SpaBooker.Core.Common;
using SpaBooker.Core.Entities;

namespace SpaBooker.Core.Interfaces;

public interface ITherapistAvailabilityService
{
    /// <summary>
    /// Gets all therapists available for a specific service and time slot
    /// </summary>
    Task<List<ApplicationUser>> GetAvailableTherapistsAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Checks if a specific therapist is available for the given time slot
    /// </summary>
    Task<Result<bool>> IsTherapistAvailableAsync(string therapistId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Finds the best available therapist for a service (considers qualifications, workload)
    /// </summary>
    Task<Result<ApplicationUser>> FindBestAvailableTherapistAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    
    /// <summary>
    /// Gets all therapists qualified for a specific service
    /// </summary>
    Task<List<ApplicationUser>> GetQualifiedTherapistsForServiceAsync(int serviceId);
    
    /// <summary>
    /// Gets therapist availability for a full day
    /// </summary>
    Task<Dictionary<string, List<TimeSlot>>> GetTherapistAvailabilityForDayAsync(DateTime date, int? locationId = null);
    
    /// <summary>
    /// Gets therapist schedule/availability settings
    /// </summary>
    Task<List<TherapistAvailability>> GetTherapistScheduleAsync(string therapistId, DateTime startDate, DateTime endDate);
}


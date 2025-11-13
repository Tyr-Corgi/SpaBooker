namespace SpaBooker.Core.Entities;

public class TherapistAvailability
{
    public int Id { get; set; }
    public string TherapistId { get; set; } = string.Empty;
    public ApplicationUser Therapist { get; set; } = null!;
    
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    
    // For specific date overrides (vacations, special hours)
    public DateTime? SpecificDate { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


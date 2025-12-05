namespace SpaBooker.Core.Entities;

public class ServiceGroupTherapist
{
    public int ServiceGroupId { get; set; }
    public ServiceGroup ServiceGroup { get; set; } = null!;
    public string TherapistId { get; set; } = string.Empty;
    public ApplicationUser Therapist { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}


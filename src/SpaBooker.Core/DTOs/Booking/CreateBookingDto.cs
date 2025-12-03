namespace SpaBooker.Core.DTOs.Booking;

public class CreateBookingDto
{
    public string ClientId { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public string? TherapistId { get; set; }
    public int? RoomId { get; set; }
    public int? LocationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Notes { get; set; }
}


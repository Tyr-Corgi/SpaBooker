namespace SpaBooker.Core.DTOs.Booking;

public class UpdateBookingDto
{
    public int Id { get; set; }
    public string? TherapistId { get; set; }
    public int? RoomId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Notes { get; set; }
}


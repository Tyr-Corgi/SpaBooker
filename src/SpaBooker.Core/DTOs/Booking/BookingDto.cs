using SpaBooker.Core.Enums;

namespace SpaBooker.Core.DTOs.Booking;

public class BookingDto
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? TherapistId { get; set; }
    public string? TherapistName { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}


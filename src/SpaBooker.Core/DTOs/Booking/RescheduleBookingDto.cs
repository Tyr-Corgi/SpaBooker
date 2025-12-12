namespace SpaBooker.Core.DTOs.Booking;

public class RescheduleBookingDto
{
    public int BookingId { get; set; }
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
    public string? RescheduleReason { get; set; }
}


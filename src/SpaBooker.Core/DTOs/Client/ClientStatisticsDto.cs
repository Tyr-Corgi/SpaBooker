namespace SpaBooker.Core.DTOs.Client;

public class ClientStatisticsDto
{
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal LifetimeValue { get; set; }
    public DateTime? LastBookingDate { get; set; }
    public DateTime? NextBookingDate { get; set; }
    public int DaysSinceLastVisit { get; set; }
    public bool HasActiveMembership { get; set; }
    public string MembershipPlanName { get; set; } = string.Empty;
    public decimal CurrentCredits { get; set; }
    public string? FavoriteService { get; set; }
    public string? FavoriteTherapist { get; set; }
}


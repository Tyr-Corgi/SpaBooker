namespace SpaBooker.Core.Interfaces;

public interface IAnalyticsService
{
    // Dashboard Overview
    Task<DashboardMetrics> GetDashboardMetricsAsync(DateTime startDate, DateTime endDate, int? locationId = null);
    
    // Revenue Analytics
    Task<RevenueReport> GetRevenueReportAsync(DateTime startDate, DateTime endDate, int? locationId = null);
    Task<List<RevenueByService>> GetRevenueByServiceAsync(DateTime startDate, DateTime endDate, int? locationId = null);
    Task<List<RevenueByTherapist>> GetRevenueByTherapistAsync(DateTime startDate, DateTime endDate, int? locationId = null);
    Task<List<RevenueByLocation>> GetRevenueByLocationAsync(DateTime startDate, DateTime endDate);
    
    // Booking Trends
    Task<List<BookingTrendData>> GetBookingTrendsAsync(DateTime startDate, DateTime endDate, int? locationId = null);
    Task<List<PopularTimeSlot>> GetPopularTimeSlotsAsync(DateTime startDate, DateTime endDate, int? locationId = null);
    
    // Client Analytics
    Task<ClientRetentionMetrics> GetClientRetentionMetricsAsync(DateTime startDate, DateTime endDate);
    Task<List<ClientGrowthData>> GetClientGrowthDataAsync(DateTime startDate, DateTime endDate);
    Task<List<TopClient>> GetTopClientsAsync(int topCount, DateTime startDate, DateTime endDate);
    
    // Therapist Performance
    Task<List<TherapistPerformance>> GetTherapistPerformanceAsync(DateTime startDate, DateTime endDate, int? locationId = null);
}

// DTOs (Data Transfer Objects)
public class DashboardMetrics
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int NewClients { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int ActiveMemberships { get; set; }
    public decimal MembershipRevenue { get; set; }
    public int GiftCertificatesSold { get; set; }
    public decimal GiftCertificateRevenue { get; set; }
    public decimal ComparisonPercentage { get; set; } // vs previous period
}

public class RevenueReport
{
    public decimal TotalRevenue { get; set; }
    public decimal ServiceRevenue { get; set; }
    public decimal MembershipRevenue { get; set; }
    public decimal GiftCertificateRevenue { get; set; }
    public decimal DepositRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
}

public class RevenueByService
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
}

public class RevenueByTherapist
{
    public string TherapistId { get; set; } = string.Empty;
    public string TherapistName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int ClientCount { get; set; }
}

public class RevenueByLocation
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
}

public class BookingTrendData
{
    public DateTime Date { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public int NewClients { get; set; }
}

public class PopularTimeSlot
{
    public int Hour { get; set; }
    public string TimeRange { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
}

public class ClientRetentionMetrics
{
    public int TotalClients { get; set; }
    public int NewClients { get; set; }
    public int ReturningClients { get; set; }
    public decimal RetentionRate { get; set; }
    public decimal ChurnRate { get; set; }
    public int LostClients { get; set; }
    public decimal AverageBookingsPerClient { get; set; }
}

public class ClientGrowthData
{
    public DateTime Date { get; set; }
    public int NewClients { get; set; }
    public int TotalClients { get; set; }
    public decimal GrowthRate { get; set; }
}

public class TopClient
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastBooking { get; set; }
}

public class TherapistPerformance
{
    public string TherapistId { get; set; } = string.Empty;
    public string TherapistName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int UniqueClients { get; set; }
    public decimal UtilizationRate { get; set; } // % of available hours booked
    public decimal ClientSatisfactionScore { get; set; } // placeholder for future rating system
}


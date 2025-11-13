using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetrics> GetDashboardMetricsAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var bookingsQuery = _context.Bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate);

        if (locationId.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.LocationId == locationId.Value);
        }

        var bookings = await bookingsQuery.ToListAsync();

        var totalBookings = bookings.Count;
        var totalRevenue = bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
            .Sum(b => b.TotalPrice);

        var newClientsCount = await _context.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .CountAsync();

        var avgBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0;

        var activeMemberships = await _context.UserMemberships
            .Where(m => m.Status == MembershipStatus.Active)
            .CountAsync();

        var membershipRevenue = 0m; // Simplified - would need payment tracking

        var giftCertsSold = await _context.GiftCertificates
            .Where(gc => gc.PurchasedAt >= startDate && gc.PurchasedAt <= endDate)
            .CountAsync();

        var giftCertRevenue = await _context.GiftCertificates
            .Where(gc => gc.PurchasedAt >= startDate && gc.PurchasedAt <= endDate)
            .SumAsync(gc => gc.PurchasePrice);

        // Calculate comparison percentage (vs previous period)
        var periodLength = (endDate - startDate).Days;
        var previousStartDate = startDate.AddDays(-periodLength);
        var previousEndDate = startDate;

        var previousBookingsQuery = _context.Bookings
            .Where(b => b.CreatedAt >= previousStartDate && b.CreatedAt < previousEndDate);

        if (locationId.HasValue)
        {
            previousBookingsQuery = previousBookingsQuery.Where(b => b.LocationId == locationId.Value);
        }

        var previousRevenue = await previousBookingsQuery
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice);

        var comparisonPercentage = previousRevenue > 0
            ? ((totalRevenue - previousRevenue) / previousRevenue) * 100
            : 0;

        return new DashboardMetrics
        {
            TotalBookings = totalBookings,
            TotalRevenue = totalRevenue,
            NewClients = newClientsCount,
            AverageBookingValue = avgBookingValue,
            ActiveMemberships = activeMemberships,
            MembershipRevenue = membershipRevenue,
            GiftCertificatesSold = giftCertsSold,
            GiftCertificateRevenue = giftCertRevenue,
            ComparisonPercentage = comparisonPercentage
        };
    }

    public async Task<RevenueReport> GetRevenueReportAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var bookingsQuery = _context.Bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed);

        if (locationId.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.LocationId == locationId.Value);
        }

        var bookings = await bookingsQuery.ToListAsync();

        var serviceRevenue = bookings.Sum(b => b.ServicePrice);
        var depositRevenue = bookings.Sum(b => b.DepositAmount);

        var membershipRevenue = 0m; // Simplified
        var giftCertRevenue = await _context.GiftCertificates
            .Where(gc => gc.PurchasedAt >= startDate && gc.PurchasedAt <= endDate)
            .SumAsync(gc => gc.PurchasePrice);

        var totalRevenue = serviceRevenue + membershipRevenue + giftCertRevenue;
        var totalTransactions = bookings.Count;
        var avgTransactionValue = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;

        return new RevenueReport
        {
            TotalRevenue = totalRevenue,
            ServiceRevenue = serviceRevenue,
            MembershipRevenue = membershipRevenue,
            GiftCertificateRevenue = giftCertRevenue,
            DepositRevenue = depositRevenue,
            TotalTransactions = totalTransactions,
            AverageTransactionValue = avgTransactionValue
        };
    }

    public async Task<List<RevenueByService>> GetRevenueByServiceAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var query = _context.Bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed);

        if (locationId.HasValue)
        {
            query = query.Where(b => b.LocationId == locationId.Value);
        }

        return await query
            .GroupBy(b => new { b.ServiceId, b.Service.Name })
            .Select(g => new RevenueByService
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.Name,
                BookingCount = g.Count(),
                TotalRevenue = g.Sum(b => b.TotalPrice),
                AveragePrice = g.Average(b => b.TotalPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync();
    }

    public async Task<List<RevenueByTherapist>> GetRevenueByTherapistAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var query = _context.Bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed);

        if (locationId.HasValue)
        {
            query = query.Where(b => b.LocationId == locationId.Value);
        }

        return await query
            .GroupBy(b => new { b.TherapistId, b.Therapist.FirstName, b.Therapist.LastName })
            .Select(g => new RevenueByTherapist
            {
                TherapistId = g.Key.TherapistId,
                TherapistName = g.Key.FirstName + " " + g.Key.LastName,
                BookingCount = g.Count(),
                TotalRevenue = g.Sum(b => b.TotalPrice),
                AverageBookingValue = g.Average(b => b.TotalPrice),
                ClientCount = g.Select(b => b.ClientId).Distinct().Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync();
    }

    public async Task<List<RevenueByLocation>> GetRevenueByLocationAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
            .GroupBy(b => new { b.LocationId, b.Location.Name })
            .Select(g => new RevenueByLocation
            {
                LocationId = g.Key.LocationId,
                LocationName = g.Key.Name,
                BookingCount = g.Count(),
                TotalRevenue = g.Sum(b => b.TotalPrice),
                AverageBookingValue = g.Average(b => b.TotalPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync();
    }

    public async Task<List<BookingTrendData>> GetBookingTrendsAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var query = _context.Bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate);

        if (locationId.HasValue)
        {
            query = query.Where(b => b.LocationId == locationId.Value);
        }

        var bookings = await query.ToListAsync();

        return bookings
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new BookingTrendData
            {
                Date = g.Key,
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                NewClients = g.Select(b => b.ClientId).Distinct().Count()
            })
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<List<PopularTimeSlot>> GetPopularTimeSlotsAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var query = _context.Bookings
            .Where(b => b.StartTime >= startDate && b.StartTime <= endDate);

        if (locationId.HasValue)
        {
            query = query.Where(b => b.LocationId == locationId.Value);
        }

        var bookings = await query.ToListAsync();

        return bookings
            .GroupBy(b => new { Hour = b.StartTime.Hour, DayOfWeek = b.StartTime.DayOfWeek.ToString() })
            .Select(g => new PopularTimeSlot
            {
                Hour = g.Key.Hour,
                TimeRange = $"{g.Key.Hour:00}:00 - {(g.Key.Hour + 1):00}:00",
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                DayOfWeek = g.Key.DayOfWeek
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(20)
            .ToList();
    }

    public async Task<ClientRetentionMetrics> GetClientRetentionMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var allClients = await _context.Users
            .Where(u => u.CreatedAt < endDate)
            .ToListAsync();

        var newClients = allClients.Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate).Count();

        var clientsWithBookings = await _context.Bookings
            .Where(b => b.StartTime >= startDate && b.StartTime <= endDate)
            .Select(b => b.ClientId)
            .Distinct()
            .ToListAsync();

        var returningClients = clientsWithBookings
            .Where(clientId => !allClients.Any(c => c.Id == clientId && c.CreatedAt >= startDate && c.CreatedAt <= endDate))
            .Count();

        var totalClients = allClients.Count;
        var retentionRate = totalClients > 0 ? (decimal)returningClients / totalClients * 100 : 0;
        var churnRate = 100 - retentionRate;

        var bookingsInPeriod = await _context.Bookings
            .Where(b => b.StartTime >= startDate && b.StartTime <= endDate)
            .GroupBy(b => b.ClientId)
            .Select(g => g.Count())
            .ToListAsync();

        var avgBookingsPerClient = bookingsInPeriod.Any() ? (decimal)bookingsInPeriod.Average() : 0;

        return new ClientRetentionMetrics
        {
            TotalClients = totalClients,
            NewClients = newClients,
            ReturningClients = returningClients,
            RetentionRate = retentionRate,
            ChurnRate = churnRate,
            LostClients = 0, // Calculate based on clients who haven't booked in X days
            AverageBookingsPerClient = avgBookingsPerClient
        };
    }

    public async Task<List<ClientGrowthData>> GetClientGrowthDataAsync(DateTime startDate, DateTime endDate)
    {
        var clientsByDate = await _context.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var cumulativeCount = await _context.Users
            .Where(u => u.CreatedAt < startDate)
            .CountAsync();

        var result = new List<ClientGrowthData>();
        foreach (var item in clientsByDate)
        {
            cumulativeCount += item.Count;
            var previousCount = cumulativeCount - item.Count;
            var growthRate = previousCount > 0 ? (decimal)item.Count / previousCount * 100 : 0;

            result.Add(new ClientGrowthData
            {
                Date = item.Date,
                NewClients = item.Count,
                TotalClients = cumulativeCount,
                GrowthRate = growthRate
            });
        }

        return result;
    }

    public async Task<List<TopClient>> GetTopClientsAsync(int topCount, DateTime startDate, DateTime endDate)
    {
        return await _context.Bookings
            .Where(b => b.StartTime >= startDate && b.StartTime <= endDate)
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
            .GroupBy(b => new { b.ClientId, b.Client.FirstName, b.Client.LastName, b.Client.Email })
            .Select(g => new TopClient
            {
                ClientId = g.Key.ClientId,
                ClientName = g.Key.FirstName + " " + g.Key.LastName,
                ClientEmail = g.Key.Email!,
                BookingCount = g.Count(),
                TotalSpent = g.Sum(b => b.TotalPrice),
                LastBooking = g.Max(b => b.StartTime)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<List<TherapistPerformance>> GetTherapistPerformanceAsync(DateTime startDate, DateTime endDate, int? locationId = null)
    {
        var query = _context.Bookings
            .Where(b => b.StartTime >= startDate && b.StartTime <= endDate);

        if (locationId.HasValue)
        {
            query = query.Where(b => b.LocationId == locationId.Value);
        }

        return await query
            .GroupBy(b => new { b.TherapistId, b.Therapist.FirstName, b.Therapist.LastName })
            .Select(g => new TherapistPerformance
            {
                TherapistId = g.Key.TherapistId,
                TherapistName = g.Key.FirstName + " " + g.Key.LastName,
                BookingCount = g.Count(),
                CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = g.Count(b => b.Status == BookingStatus.Cancelled),
                TotalRevenue = g.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                AverageBookingValue = g.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    .Average(b => b.TotalPrice),
                UniqueClients = g.Select(b => b.ClientId).Distinct().Count(),
                UtilizationRate = 0, // Placeholder - would need availability data
                ClientSatisfactionScore = 0 // Placeholder for future rating system
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync();
    }
}


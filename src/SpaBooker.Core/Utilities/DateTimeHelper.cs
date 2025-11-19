namespace SpaBooker.Core.Utilities;

/// <summary>
/// Helper class for consistent DateTime handling across the application
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Get the current UTC time
    /// </summary>
    public static DateTime UtcNow => DateTime.UtcNow;
    
    /// <summary>
    /// Convert a DateTime to UTC if it isn't already
    /// </summary>
    public static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime
        };
    }
    
    /// <summary>
    /// Convert UTC DateTime to local timezone for display
    /// </summary>
    public static DateTime ToLocal(DateTime utcDateTime, string? timeZoneId = null)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return utcDateTime.ToLocalTime();
        }
        
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        catch
        {
            return utcDateTime.ToLocalTime();
        }
    }
    
    /// <summary>
    /// Get start of day in UTC
    /// </summary>
    public static DateTime StartOfDayUtc(DateTime date)
    {
        var utcDate = ToUtc(date);
        return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, 0, 0, 0, DateTimeKind.Utc);
    }
    
    /// <summary>
    /// Get end of day in UTC
    /// </summary>
    public static DateTime EndOfDayUtc(DateTime date)
    {
        var utcDate = ToUtc(date);
        return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, 23, 59, 59, DateTimeKind.Utc);
    }
    
    /// <summary>
    /// Check if a DateTime is in the past (UTC comparison)
    /// </summary>
    public static bool IsInPast(DateTime dateTime)
    {
        return ToUtc(dateTime) < UtcNow;
    }
    
    /// <summary>
    /// Check if a DateTime is in the future (UTC comparison)
    /// </summary>
    public static bool IsInFuture(DateTime dateTime)
    {
        return ToUtc(dateTime) > UtcNow;
    }
    
    /// <summary>
    /// Get the difference in hours between two DateTimes (UTC)
    /// </summary>
    public static double GetHoursDifference(DateTime start, DateTime end)
    {
        return (ToUtc(end) - ToUtc(start)).TotalHours;
    }
    
    /// <summary>
    /// Format DateTime for display (converts to local time)
    /// </summary>
    public static string FormatForDisplay(DateTime utcDateTime, string format = "yyyy-MM-dd HH:mm", string? timeZoneId = null)
    {
        var localTime = ToLocal(utcDateTime, timeZoneId);
        return localTime.ToString(format);
    }
}


# Timezone Handling Strategy

## Overview

SpaBooker uses **UTC (Coordinated Universal Time)** for all internal storage and processing. This ensures consistency across different timezones and eliminates ambiguity.

## Core Principles

### 1. **Storage: Always UTC**
- All DateTime values stored in the database are in UTC
- PostgreSQL column type: `timestamp with time zone`
- Entity Framework is configured to handle UTC timestamps

### 2. **Processing: Always UTC**
- All business logic operates on UTC times
- Use `DateTime.UtcNow` instead of `DateTime.Now`
- Use `DateTimeHelper.UtcNow` for consistency

### 3. **Display: Convert to Local**
- Convert to user's local timezone only for display
- Use `DateTimeHelper.ToLocal()` or `DateTimeHelper.FormatForDisplay()`
- Store user timezone preferences (future enhancement)

## Implementation

### DateTimeHelper Utility

```csharp
using SpaBooker.Core.Utilities;

// Get current UTC time
var now = DateTimeHelper.UtcNow;

// Convert to UTC if needed
var utcDate = DateTimeHelper.ToUtc(someDateTime);

// Convert to local for display
var localDate = DateTimeHelper.ToLocal(utcDateTime);
var formatted = DateTimeHelper.FormatForDisplay(utcDateTime, "yyyy-MM-dd HH:mm");

// Check if date is in past/future
if (DateTimeHelper.IsInPast(booking.StartTime))
{
    // Booking has already started
}

// Get day boundaries in UTC
var startOfDay = DateTimeHelper.StartOfDayUtc(date);
var endOfDay = DateTimeHelper.EndOfDayUtc(date);
```

### Database Configuration

```csharp
// ApplicationDbContext.OnModelCreating
foreach (var entityType in builder.Model.GetEntityTypes())
{
    foreach (var property in entityType.GetProperties())
    {
        if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        {
            property.SetColumnType("timestamp with time zone");
        }
    }
}
```

## Common Scenarios

### Scenario 1: Creating a Booking

```csharp
// ❌ WRONG
var booking = new Booking
{
    StartTime = DateTime.Now,  // Local time - DON'T DO THIS
    CreatedAt = DateTime.Now   // Local time - DON'T DO THIS
};

// ✅ CORRECT
var booking = new Booking
{
    StartTime = DateTimeHelper.ToUtc(userSelectedTime),  // Convert to UTC
    CreatedAt = DateTimeHelper.UtcNow                     // Use UTC
};
```

### Scenario 2: Querying Bookings

```csharp
// ❌ WRONG
var today = DateTime.Today;
var bookings = await _context.Bookings
    .Where(b => b.StartTime.Date == today)
    .ToListAsync();

// ✅ CORRECT
var startOfDay = DateTimeHelper.StartOfDayUtc(DateTime.Today);
var endOfDay = DateTimeHelper.EndOfDayUtc(DateTime.Today);
var bookings = await _context.Bookings
    .Where(b => b.StartTime >= startOfDay && b.StartTime <= endOfDay)
    .ToListAsync();
```

### Scenario 3: Displaying Times

```razor
@* ❌ WRONG *@
<p>Booking Time: @booking.StartTime</p>

@* ✅ CORRECT *@
<p>Booking Time: @DateTimeHelper.FormatForDisplay(booking.StartTime)</p>

@* ✅ BETTER (with timezone) *@
<p>Booking Time: @DateTimeHelper.FormatForDisplay(booking.StartTime, "yyyy-MM-dd HH:mm", userTimeZone)</p>
```

### Scenario 4: Comparing Times

```csharp
// ❌ WRONG
if (booking.StartTime < DateTime.Now)
{
    // This can fail if StartTime is UTC and DateTime.Now is local
}

// ✅ CORRECT
if (DateTimeHelper.IsInPast(booking.StartTime))
{
    // Safe comparison using UTC
}
```

## Business Hours Handling

For location-specific business hours:

```csharp
public class Location
{
    // Store as UTC offset or timezone identifier
    public string TimeZoneId { get; set; } = "America/New_York";
    
    // Store business hours in local time (easier for admins)
    public TimeOnly OpeningTime { get; set; } = new TimeOnly(9, 0);
    public TimeOnly ClosingTime { get; set; } = new TimeOnly(17, 0);
}

// Convert business hours to UTC for a specific date
public DateTime GetOpeningTimeUtc(Location location, DateTime date)
{
    var localDateTime = date.Date.Add(location.OpeningTime.ToTimeSpan());
    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
    return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
}
```

## User Timezone Preferences

Future enhancement to store user timezone:

```csharp
public class ApplicationUser
{
    // Add timezone preference
    public string? TimeZoneId { get; set; } // e.g., "America/New_York"
}

// Use in display
var localTime = DateTimeHelper.ToLocal(utcTime, user.TimeZoneId);
```

## Testing

When writing tests, always use UTC:

```csharp
[Fact]
public async Task Booking_ShouldNotOverlap()
{
    // Arrange
    var now = DateTimeHelper.UtcNow;
    var booking1 = new Booking
    {
        StartTime = now.AddHours(1),
        EndTime = now.AddHours(2)
    };
    
    var booking2 = new Booking
    {
        StartTime = now.AddHours(1.5),  // Overlaps
        EndTime = now.AddHours(2.5)
    };
    
    // Act & Assert
    var hasConflict = await _conflictChecker.CheckForConflictsAsync(...);
    Assert.True(hasConflict.HasConflict);
}
```

## Migration Notes

If migrating from local times to UTC:

1. **Identify all DateTime fields**
2. **Convert existing data to UTC:**
   ```sql
   UPDATE bookings 
   SET start_time = start_time AT TIME ZONE 'America/New_York' AT TIME ZONE 'UTC';
   ```
3. **Update application code to use DateTimeHelper**
4. **Test thoroughly in different timezones**

## Common Timezone IDs

- **US Eastern:** `America/New_York`
- **US Central:** `America/Chicago`
- **US Mountain:** `America/Denver`
- **US Pacific:** `America/Los_Angeles`
- **UK:** `Europe/London`
- **UTC:** `UTC`

Full list: [IANA Time Zone Database](https://www.iana.org/time-zones)

## Troubleshooting

### Problem: Times show wrong in UI

**Solution:** Ensure you're converting UTC to local for display:
```csharp
var localTime = DateTimeHelper.ToLocal(utcDateTime, userTimeZone);
```

### Problem: Database stores local time

**Solution:** Ensure column type is `timestamp with time zone`:
```csharp
property.SetColumnType("timestamp with time zone");
```

### Problem: Comparisons fail

**Solution:** Always convert to UTC before comparing:
```csharp
var utc1 = DateTimeHelper.ToUtc(date1);
var utc2 = DateTimeHelper.ToUtc(date2);
if (utc1 < utc2) { ... }
```

## Best Practices

1. **Never use `DateTime.Now`** - use `DateTimeHelper.UtcNow`
2. **Never use `.Date` property** - use `DateTimeHelper.StartOfDayUtc()`
3. **Always specify `DateTimeKind.Utc`** when creating DateTimes
4. **Store timezone separately** if needed for business logic
5. **Convert to local only at the UI layer**
6. **Test in multiple timezones**, including DST transitions
7. **Document timezone assumptions** in API contracts

---

**Last Updated:** November 19, 2025  
**Status:** Implemented  
**Related Issues:** #24 - Fix timezone handling inconsistencies


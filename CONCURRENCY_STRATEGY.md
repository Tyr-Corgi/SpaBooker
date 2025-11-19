# Booking Concurrency and Race Condition Prevention

## Problem Statement

Multiple users or processes attempting to book the same therapist or room at overlapping times can create race conditions, leading to double bookings.

## Solution Strategy

### 1. Database-Level Protection

**CHECK Constraint:**
- `CK_Booking_EndTimeAfterStartTime`: Ensures `EndTime > StartTime` for all bookings

**Indexes for Performance:**
- `IX_Bookings_TherapistId_StartTime_EndTime`: Optimizes therapist availability checks
- `IX_Bookings_RoomId_StartTime_EndTime`: Optimizes room availability checks

### 2. Application-Level Protection

**IBookingConflictChecker Service:**

The `IBookingConflictChecker` service provides methods to check for booking conflicts before creating or updating bookings.

```csharp
// Example usage in a booking service:
public async Task<int> CreateBookingAsync(BookingDto booking)
{
    using var transaction = await _unitOfWork.BeginTransactionAsync();
    try
    {
        // Step 1: Check for conflicts
        var conflictResult = await _conflictChecker.CheckForConflictsAsync(
            booking.TherapistId,
            booking.RoomId,
            booking.StartTime,
            booking.EndTime
        );

        if (conflictResult.HasConflict)
        {
            throw new InvalidOperationException(
                $"Booking conflict: {string.Join("; ", conflictResult.ConflictReasons)}"
            );
        }

        // Step 2: Create the booking within the transaction
        var newBooking = new Booking
        {
            // ... set properties
        };

        _context.Bookings.Add(newBooking);
        await _unitOfWork.CommitAsync();

        return newBooking.Id;
    }
    catch
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

### 3. Transaction Isolation

All booking creation/modification operations should use the `IUnitOfWork` pattern with transactions to ensure atomicity.

**Default Transaction Isolation Level:** Read Committed (PostgreSQL default)

For high-concurrency scenarios, consider using:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
```

### 4. Query Logic for Conflict Detection

**Overlap Detection:**

Two time ranges overlap if:
```
(StartTime1 < EndTime2) AND (EndTime1 > StartTime2)
```

**Implementation:**
```csharp
var conflicts = await _context.Bookings
    .Where(b => b.TherapistId == therapistId
        && b.Status != BookingStatus.Cancelled
        && b.StartTime < endTime
        && b.EndTime > startTime)
    .AnyAsync();
```

### 5. Race Condition Scenarios

#### Scenario 1: Simultaneous Booking Attempts
**Problem:**
- User A and User B both try to book Therapist X at 2:00 PM
- Both check for conflicts at the same time
- Both see no conflicts
- Both create bookings

**Solution:**
- Use database transactions with proper isolation level
- Implement optimistic concurrency with row versioning (future enhancement)

#### Scenario 2: Long-Running Transactions
**Problem:**
- Transaction holds lock for too long
- Other requests timeout or queue up

**Solution:**
- Keep transactions as short as possible
- Perform conflict checks immediately before commit
- Consider retry logic with exponential backoff

### 6. Best Practices

1. **Always Use IBookingConflictChecker:**
   - Before creating a new booking
   - Before updating an existing booking's time/therapist/room

2. **Always Use Transactions:**
   - Wrap conflict check + booking creation in a single transaction
   - Minimize transaction duration

3. **Handle Conflicts Gracefully:**
   - Provide clear error messages to users
   - Suggest alternative time slots
   - Log conflicts for analysis

4. **Status Filtering:**
   - Exclude cancelled bookings from conflict checks
   - Consider pending/confirmed status in your logic

### 7. Future Enhancements

**Optimistic Concurrency Control:**
```csharp
public class Booking : BaseEntity
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

**Distributed Locking:**
For multi-server deployments, consider Redis-based distributed locks:
```csharp
using (var redisLock = await _distributedLockFactory.CreateLockAsync($"booking:therapist:{therapistId}:{startTime:yyyyMMddHHmm}"))
{
    // Check conflicts and create booking
}
```

### 8. Testing Recommendations

**Unit Tests:**
- Test conflict detection logic with various time overlaps
- Test edge cases (exact same time, adjacent times, etc.)

**Integration Tests:**
- Test concurrent booking attempts using `Task.WhenAll`
- Verify only one booking succeeds

**Load Tests:**
- Simulate multiple users booking simultaneously
- Monitor database deadlocks and timeouts

### 9. Monitoring

Track the following metrics:
- Number of booking conflicts detected
- Failed booking attempts due to conflicts
- Average transaction duration
- Database deadlocks (if any)

Add logging to `BookingConflictChecker`:
```csharp
_logger.LogWarning("Booking conflict detected for therapist {TherapistId} at {StartTime}-{EndTime}", 
    therapistId, startTime, endTime);
```

---

**Last Updated:** November 19, 2025  
**Related Issues:** #18 - Fix concurrent booking race conditions  
**Status:** Implemented


# Therapist Availability Management System

## Overview

Complete implementation of therapist schedule management, allowing therapists to set their regular working hours and block specific dates for vacations or personal time. The booking system automatically respects these availability settings.

---

## ✅ Features Implemented

### 1. **Weekly Schedule Management**
- Set regular working hours for each day of the week
- Toggle availability on/off for specific days
- Customizable start and end times per day
- 30-minute time slot intervals
- Save and update schedule at any time

### 2. **Specific Date Blocking**
- Block individual dates (vacations, sick days, personal time)
- Add optional notes/reasons for blocked dates
- View list of all blocked dates
- Unblock dates easily
- Only shows future blocked dates

### 3. **Booking Integration**
- Booking system automatically checks therapist availability
- Respects weekly schedule settings
- Respects blocked dates
- Only shows time slots within therapist's working hours
- Prevents bookings outside availability
- Clear messaging when therapist is unavailable

---

## User Interface

### Manage Availability Page (`/schedule/availability`)

**Access**: Therapists and Admins only

#### Weekly Schedule Section
- Checkbox for each day of the week
- Start time picker (when day is enabled)
- End time picker (when day is enabled)
- "Not available" message for disabled days
- Save button to update schedule

#### Block Specific Date Section
- Date picker (only future dates)
- Optional reason/note field
- Block button to save

#### Blocked Dates List
- Shows all future blocked dates
- Displays date and reason
- Remove button to unblock
- Sorted chronologically

---

## Technical Implementation

### Database Schema

#### TherapistAvailability Table
```csharp
public class TherapistAvailability
{
    public int Id { get; set; }
    public string TherapistId { get; set; }  // Foreign key to User
    public DayOfWeek DayOfWeek { get; set; }  // Monday, Tuesday, etc.
    public TimeSpan StartTime { get; set; }    // e.g., 09:00
    public TimeSpan EndTime { get; set; }      // e.g., 17:00
    public bool IsAvailable { get; set; }      // Working this day?
    public DateTime? SpecificDate { get; set; } // For date overrides
    public string? Notes { get; set; }         // Reason for blocking
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Record Types**:
1. **Weekly Schedule**: `SpecificDate = null`, one record per day of week
2. **Blocked Dates**: `SpecificDate != null`, `IsAvailable = false`

---

### Booking Integration Logic

Located in: `CreateBooking.razor` → `LoadAvailableTimeSlots()`

#### Step 1: Check for Blocked Date
```csharp
var specificDateBlock = await DbContext.TherapistAvailability
    .FirstOrDefaultAsync(a => a.TherapistId == therapistId 
                           && a.SpecificDate == selectedDate 
                           && !a.IsAvailable);

if (specificDateBlock != null)
{
    // Don't show any time slots - date is blocked
    return;
}
```

#### Step 2: Get Weekly Availability
```csharp
var availability = await DbContext.TherapistAvailability
    .FirstOrDefaultAsync(a => a.TherapistId == therapistId 
                           && a.DayOfWeek == dayOfWeek 
                           && a.SpecificDate == null);

TimeSpan startTime = availability?.StartTime ?? new TimeSpan(9, 0, 0); // Default 9 AM
TimeSpan endTime = availability?.EndTime ?? new TimeSpan(17, 0, 0);     // Default 5 PM
bool isAvailable = availability?.IsAvailable ?? true;                    // Default available
```

#### Step 3: Generate Time Slots
```csharp
var slots = new List<TimeSpan>();
var currentSlot = startTime;

while (currentSlot < endTime)
{
    // Ensure appointment can finish within working hours
    var appointmentEnd = currentSlot.Add(TimeSpan.FromMinutes(serviceDuration));
    if (appointmentEnd <= endTime)
    {
        slots.Add(currentSlot);
    }
    currentSlot = currentSlot.Add(TimeSpan.FromMinutes(30)); // 30-min intervals
}
```

#### Step 4: Filter Out Conflicts
```csharp
foreach (var slot in slots)
{
    var slotStart = date.Add(slot);
    var slotEnd = slotStart.AddMinutes(serviceDuration);

    // Check against existing bookings
    bool hasConflict = existingBookings.Any(b => 
        (slotStart >= b.StartTime && slotStart < b.EndTime) ||
        (slotEnd > b.StartTime && slotEnd <= b.EndTime) ||
        (slotStart <= b.StartTime && slotEnd >= b.EndTime)
    );

    if (!hasConflict && slotStart > DateTime.Now)
    {
        availableTimeSlots.Add(slot);
    }
}
```

---

## User Workflows

### Therapist Setting Weekly Schedule

1. Navigate to "Manage Availability" from sidebar
2. For each day of the week:
   - Check/uncheck the day to toggle availability
   - If available, set start time (e.g., 9:00 AM)
   - If available, set end time (e.g., 6:00 PM)
3. Click "Save Weekly Schedule"
4. Success message confirms changes

**Example**:
- Monday: 9:00 AM - 5:00 PM ✓
- Tuesday: 10:00 AM - 7:00 PM ✓
- Wednesday: Not available ✗
- Thursday: 9:00 AM - 5:00 PM ✓
- Friday: 9:00 AM - 3:00 PM ✓
- Saturday: Not available ✗
- Sunday: Not available ✗

### Therapist Blocking Vacation Days

1. Navigate to "Manage Availability"
2. Scroll to "Block Specific Date" section
3. Select date from calendar picker
4. Enter reason (optional): "Vacation"
5. Click "Block Date"
6. Date appears in "Blocked Dates" list

### Therapist Unblocking a Date

1. View "Blocked Dates" list
2. Find the date to unblock
3. Click "Remove" button
4. Date is removed from list
5. Clients can now book on this date

### Client Booking with Availability

1. Client selects service and therapist
2. Client selects date
3. System checks:
   - Is date blocked? → Show "no slots available"
   - Is day of week available? → Show "no slots available"
   - Generate slots within therapist's hours
   - Filter out existing bookings
4. Client sees only truly available time slots
5. Client selects time and completes booking

---

## Default Behavior

### No Availability Set
If a therapist hasn't set their availability:
- **Default Schedule**: 9:00 AM - 5:00 PM, Monday-Friday
- **Default Status**: Available
- **Weekend**: Available (but default hours apply)

### First-Time Therapist
New therapists should:
1. Go to "Manage Availability"
2. Set their actual working hours
3. Block any known vacation dates
4. System immediately respects these settings

---

## Edge Cases Handled

### 1. Appointment Spans Outside Hours
✅ Only shows slots where the **entire appointment** fits within working hours

**Example**:
- Therapist works until 5:00 PM
- Service duration: 90 minutes
- Last slot shown: 3:30 PM (ends at 5:00 PM)
- 4:00 PM slot **NOT shown** (would end at 5:30 PM)

### 2. Blocked Date with Existing Bookings
- Therapist blocks a date
- Existing bookings on that date **remain valid**
- New bookings **cannot be made** for that date

### 3. Changing Hours with Existing Bookings
- Therapist changes working hours
- Existing bookings **remain valid** (even if outside new hours)
- New bookings respect updated hours

### 4. Multiple Therapists at Same Location
- Each therapist has independent schedule
- Client can book with different therapists on same day
- Each therapist's availability is checked separately

---

## Navigation

### Added to Sidebar Menu
**For Therapists and Admins**:
- 📅 **My Schedule** - View upcoming appointments
- 🕒 **Manage Availability** - Set hours and block dates

---

## Styling

### CSS File
`ManageAvailability.razor.css`

**Features**:
- Rose gold theme consistency
- Gradient card headers
- Animated checkboxes
- Hover effects on blocked dates list
- Responsive design

---

## Testing Scenarios

### Test 1: Set Weekly Schedule
1. Login as therapist
2. Go to Manage Availability
3. Set different hours for each day
4. Save schedule
5. Create booking as client
6. Verify only slots within therapist's hours appear

### Test 2: Block Date
1. Login as therapist
2. Block next week's Friday
3. Login as client
4. Try to book for that Friday
5. Verify "No available time slots" message appears

### Test 3: Unblock Date
1. Therapist unblocks previously blocked date
2. Client refreshes booking page
3. Verify time slots now appear for that date

### Test 4: Day Off
1. Therapist unchecks Monday (not available)
2. Client tries to book for Monday
3. Verify no time slots available

### Test 5: Different Therapists
1. Therapist A: Works 9-5
2. Therapist B: Works 12-8
3. Client books service for 6:30 PM
4. Verify only Therapist B is available

---

## Database Queries

### Get Therapist Weekly Schedule
```sql
SELECT * FROM "TherapistAvailability"
WHERE "TherapistId" = @therapistId
  AND "SpecificDate" IS NULL
ORDER BY "DayOfWeek";
```

### Get Blocked Dates
```sql
SELECT * FROM "TherapistAvailability"
WHERE "TherapistId" = @therapistId
  AND "SpecificDate" IS NOT NULL
  AND "IsAvailable" = false
  AND "SpecificDate" >= CURRENT_DATE
ORDER BY "SpecificDate";
```

### Check If Date Is Blocked
```sql
SELECT * FROM "TherapistAvailability"
WHERE "TherapistId" = @therapistId
  AND "SpecificDate" = @date
  AND "IsAvailable" = false
LIMIT 1;
```

---

## Future Enhancements (Optional)

### Phase 2
1. **Recurring Blocks**: Block same day every week/month
2. **Bulk Block**: Block date range at once
3. **Copy Schedule**: Copy one therapist's schedule to another
4. **Schedule Templates**: Save common schedules (e.g., "Summer Hours")

### Phase 3
1. **Break Times**: Add lunch breaks within working hours
2. **Variable Intervals**: Different slot intervals for different services
3. **Overtime**: Allow bookings outside normal hours with approval
4. **Team Calendar**: View all therapists' availability at once

---

## Benefits

### For Therapists
✅ Full control over their schedule
✅ Easy to block vacation time
✅ No impossible bookings
✅ Work-life balance

### For Clients
✅ Only see truly available times
✅ No booking conflicts
✅ Clear messaging when unavailable
✅ Better booking experience

### For Business
✅ Reduces scheduling errors
✅ Respects therapist preferences
✅ Professional availability management
✅ Prevents double-bookings

---

## Summary

**Status**: ✅ Complete and Production Ready

The therapist availability system provides comprehensive schedule management with:
- ✅ Weekly schedule customization
- ✅ Specific date blocking (vacations, etc.)
- ✅ Full integration with booking system
- ✅ Clear user interface
- ✅ Edge cases handled
- ✅ Professional design

Therapists now have complete control over when they work, and the booking system automatically ensures clients can only book during available times!


# Therapist Availability System

## Overview

Comprehensive schedule management allowing therapists to set their regular working hours and block specific dates. The booking system automatically respects these availability settings.

---

## Features

### Weekly Schedule Management
- Set regular working hours for each day of the week
- Toggle availability on/off for specific days
- Customizable start and end times per day
- 30-minute time slot intervals

### Specific Date Blocking
- Block individual dates (vacations, sick days, personal time)
- Add optional notes/reasons for blocked dates
- View list of all blocked dates
- Unblock dates easily

### Booking Integration
- Booking system automatically checks therapist availability
- Respects weekly schedule settings
- Respects blocked dates
- Only shows time slots within therapist's working hours
- Prevents bookings outside availability

---

## User Interface

### Manage Availability Page (`/schedule/availability`)

**Access**: Therapists and Admins only

#### Weekly Schedule Section
- Checkbox for each day of the week
- Start time picker (when day is enabled)
- End time picker (when day is enabled)
- Save button to update schedule

#### Block Specific Date Section
- Date picker (only future dates)
- Optional reason/note field
- Block button to save

#### Blocked Dates List
- Shows all future blocked dates
- Displays date and reason
- Remove button to unblock

---

## Default Behavior

If a therapist hasn't set their availability:
- **Default Schedule**: 9:00 AM - 5:00 PM, Monday-Friday
- **Default Status**: Available
- **Weekend**: Available (but default hours apply)

---

## Technical Implementation

### Database Schema

```csharp
public class TherapistAvailability
{
    public int Id { get; set; }
    public string TherapistId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? SpecificDate { get; set; } // For date overrides
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Record Types**:
1. **Weekly Schedule**: `SpecificDate = null`, one record per day of week
2. **Blocked Dates**: `SpecificDate != null`, `IsAvailable = false`

### Booking Integration Logic

1. **Check for Blocked Date**: If date is blocked, show no slots
2. **Get Weekly Availability**: Get therapist's hours for that day of week
3. **Generate Time Slots**: Create slots within working hours
4. **Filter Out Conflicts**: Remove slots with existing bookings

---

## Edge Cases Handled

### Appointment Spans Outside Hours
Only shows slots where the **entire appointment** fits within working hours.

Example:
- Therapist works until 5:00 PM
- Service duration: 90 minutes
- Last slot shown: 3:30 PM (ends at 5:00 PM)
- 4:00 PM slot NOT shown (would end at 5:30 PM)

### Blocked Date with Existing Bookings
- Existing bookings on blocked date remain valid
- New bookings cannot be made for that date

### Changing Hours with Existing Bookings
- Existing bookings remain valid (even if outside new hours)
- New bookings respect updated hours

### Multiple Therapists at Same Location
- Each therapist has independent schedule
- Client can book with different therapists on same day
- Each therapist's availability is checked separately

---

## User Workflows

### Therapist Setting Weekly Schedule

1. Navigate to "Manage Availability" from sidebar
2. For each day of the week:
   - Check/uncheck the day to toggle availability
   - If available, set start and end times
3. Click "Save Weekly Schedule"

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

### Client Booking with Availability

1. Client selects service and therapist
2. Client selects date
3. System checks availability
4. Client sees only truly available time slots
5. Client selects time and completes booking

---

## Benefits

### For Therapists
- Full control over their schedule
- Easy to block vacation time
- No impossible bookings
- Work-life balance

### For Clients
- Only see truly available times
- No booking conflicts
- Clear messaging when unavailable
- Better booking experience

### For Business
- Reduces scheduling errors
- Respects therapist preferences
- Professional availability management
- Prevents double-bookings

---

## Future Enhancements

1. **Recurring Blocks**: Block same day every week/month
2. **Bulk Block**: Block date range at once
3. **Break Times**: Add lunch breaks within working hours
4. **Variable Intervals**: Different slot intervals for different services
5. **Team Calendar**: View all therapists' availability at once


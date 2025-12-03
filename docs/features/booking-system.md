# Booking System

## Overview

The core booking system allows clients to schedule spa appointments with real-time availability checking, therapist selection, and location-based filtering.

---

## Features

### Service Selection
- Browse spa services by category
- View service details, duration, and pricing
- Filter by location

### Therapist Selection
- View available therapists for selected service
- See therapist bio and specialties
- Filter by location assignment

### Time Slot Selection
- 30-minute booking intervals
- Real-time availability checking
- Respects therapist schedules and blocked dates
- Prevents double-booking

### Location Selection
- Retail-style spa finder
- Search by city or ZIP code
- Location persistence in browser
- Room filtering by location

### Payment Processing
- 50% deposit required at booking
- Stripe Checkout integration
- Automatic refunds for eligible cancellations

---

## Booking Flow

```
1. Select Location (if not already set)
   ↓
2. Browse Services
   ↓
3. Select Service
   ↓
4. Choose Therapist (optional)
   ↓
5. Select Date
   ↓
6. Choose Available Time Slot
   ↓
7. Review Booking Details
   ↓
8. Pay Deposit (Stripe Checkout)
   ↓
9. Booking Confirmed
```

---

## Availability Checking

### Conflict Detection
The system prevents double-booking by checking:
1. Therapist's existing bookings
2. Therapist's weekly schedule
3. Therapist's blocked dates
4. Room availability (if assigned)

### Time Slot Generation
```csharp
// Only show slots where entire appointment fits
while (currentSlot < therapistEndTime)
{
    var appointmentEnd = currentSlot.Add(serviceDuration);
    if (appointmentEnd <= therapistEndTime)
    {
        // Check for conflicts with existing bookings
        if (!hasConflict)
        {
            availableSlots.Add(currentSlot);
        }
    }
    currentSlot = currentSlot.Add(TimeSpan.FromMinutes(30));
}
```

---

## Booking Status

| Status | Description |
|--------|-------------|
| Pending | Booking created, awaiting payment |
| Confirmed | Payment received, appointment scheduled |
| InProgress | Appointment currently happening |
| Completed | Appointment finished |
| Cancelled | Booking cancelled by client or admin |
| NoShow | Client did not attend |

---

## Cancellation Policy

### Within 24 Hours of Booking
- Full deposit refund
- Automatic Stripe refund processed

### Less Than 24 Hours Before Appointment
- Deposit forfeited
- No refund issued

### Cancellation Process
1. Client navigates to "My Bookings"
2. Clicks "Cancel" on upcoming booking
3. System checks cancellation window
4. Refund processed if eligible
5. Confirmation email sent

---

## Location Selection

### Spa Finder Interface
- Search by city name or ZIP code
- Visual location cards with address
- Selection feedback and confirmation
- Persistence in localStorage

### Location Integration
- Services filtered by location
- Therapists filtered by location assignment
- Rooms filtered by location
- Default to client's home location in admin booking

---

## Admin Booking

Administrators can create bookings on behalf of clients:

### Features
- Client search and selection
- Service selection
- Therapist assignment
- Room assignment
- Time selection with 15-minute intervals
- Location dropdown with client home location default
- Option to update client's home location

### Location Handling
- Defaults to client's home location
- Can select any spa location
- Checkbox to permanently update client's home location
- Rooms and therapists filter based on selected location

---

## Database Schema

### Booking Entity
```csharp
public class Booking
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public string? TherapistId { get; set; }
    public int ServiceId { get; set; }
    public int? RoomId { get; set; }
    public int LocationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeRefundId { get; set; }
}
```

---

## Concurrency Handling

### Race Condition Prevention
- Database transactions for booking creation
- `IBookingConflictChecker` service validates before insert
- CHECK constraint ensures `EndTime > StartTime`
- Optimized indexes for availability queries

See [Architecture Overview](../architecture/overview.md#concurrency-strategy) for details.

---

## Future Enhancements

1. **Waitlist**: Join waitlist when preferred time unavailable
2. **Recurring Appointments**: Schedule weekly/monthly appointments
3. **Package Booking**: Book multiple services at once
4. **Group Booking**: Book for multiple clients
5. **Preferred Therapist**: Remember client's favorite therapist


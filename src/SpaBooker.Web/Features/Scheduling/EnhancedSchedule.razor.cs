using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;

namespace SpaBooker.Web.Features.Scheduling;

public partial class EnhancedSchedule
{
    private RenderFragment RenderDayView() => builder =>
    {
        var currentDay = currentDate.Date;
        
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "row");
        
        // Day view with hourly time slots
        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "col-12");
        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "card");
        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "card-body");
        
        // Header
        builder.OpenElement(8, "h5");
        builder.AddAttribute(9, "class", "card-title text-center mb-4");
        builder.AddContent(10, currentDay.ToString("dddd, dd/MM/yyyy"));
        builder.CloseElement(); // h5
        
        // Time slots table
        builder.OpenElement(11, "div");
        builder.AddAttribute(12, "class", "day-view-grid");
        
        foreach (var timeSlot in timeSlots)
        {
            var slotEnd = timeSlot.Add(TimeSpan.FromHours(1));
            var slotBookings = bookings.Where(b => 
                b.StartTime.Date == currentDay && 
                b.StartTime.TimeOfDay >= timeSlot && 
                b.StartTime.TimeOfDay < slotEnd).ToList();
            
            var slotBlocks = blockedTimes.Where(bt => 
                bt.SpecificDate == currentDay &&
                ((bt.StartTime == TimeSpan.Zero && bt.EndTime.TotalSeconds >= 86399) || // All day
                 (bt.StartTime <= timeSlot && bt.EndTime > timeSlot))).ToList();
            
            // Time slot row
            builder.OpenElement(13, "div");
            builder.AddAttribute(14, "class", "time-slot-row");
            
            // Time label
            builder.OpenElement(15, "div");
            builder.AddAttribute(16, "class", "time-label");
            builder.AddContent(17, $"{timeSlot.Hours:00}:00");
            builder.CloseElement(); // div.time-label
            
            // Content area
            builder.OpenElement(18, "div");
            builder.AddAttribute(19, "class", "time-content");
            
            // Render blocked times
            foreach (var block in slotBlocks)
            {
                builder.OpenElement(20, "div");
                builder.AddAttribute(21, "class", "blocked-time-item");
                builder.AddAttribute(22, "onclick", EventCallback.Factory.Create(this, () => SelectBlockedTime(block)));
                
                builder.OpenElement(23, "div");
                builder.AddAttribute(24, "class", "fw-bold text-danger");
                builder.AddContent(25, "🚫 Blocked");
                builder.CloseElement();
                
                builder.OpenElement(26, "div");
                builder.AddAttribute(27, "class", "small text-muted");
                builder.AddContent(28, block.Notes);
                builder.CloseElement();
                
                builder.CloseElement(); // div.blocked-time-item
            }
            
            // Render bookings
            foreach (var booking in slotBookings)
            {
                builder.OpenElement(29, "div");
                builder.AddAttribute(30, "class", $"booking-card status-{booking.Status.ToString().ToLower()}");
                builder.AddAttribute(31, "onclick", EventCallback.Factory.Create(this, () => SelectBooking(booking)));
                
                builder.OpenElement(32, "div");
                builder.AddAttribute(33, "class", "booking-time fw-bold");
                builder.AddContent(34, $"{booking.StartTime:HH:mm} - {booking.EndTime:HH:mm}");
                builder.CloseElement();
                
                builder.OpenElement(35, "div");
                builder.AddAttribute(36, "class", "booking-service");
                builder.AddContent(37, booking.Service.Name);
                builder.CloseElement();
                
                builder.OpenElement(38, "div");
                builder.AddAttribute(39, "class", "booking-client small");
                builder.AddContent(40, $"{booking.Client.FirstName} {booking.Client.LastName}");
                builder.CloseElement();
                
                builder.CloseElement(); // div.booking-card
            }
            
            if (!slotBookings.Any() && !slotBlocks.Any())
            {
                builder.OpenElement(41, "div");
                builder.AddAttribute(42, "class", "text-muted small");
                builder.AddContent(43, "Available");
                builder.CloseElement();
            }
            
            builder.CloseElement(); // div.time-content
            builder.CloseElement(); // div.time-slot-row
        }
        
        builder.CloseElement(); // div.day-view-grid
        builder.CloseElement(); // div.card-body
        builder.CloseElement(); // div.card
        builder.CloseElement(); // div.col-12
        builder.CloseElement(); // div.row
    };

    private RenderFragment RenderWeekView() => builder =>
    {
        var weekStart = GetWeekStart(currentDate);
        var days = viewMode == ViewMode.Fortnight ? 14 : 7;
        
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "row");
        
        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "col-12");
        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "card");
        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "card-body p-0");
        
        builder.OpenElement(8, "div");
        builder.AddAttribute(9, "class", "week-grid");
        
        for (int i = 0; i < days; i++)
        {
            var day = weekStart.AddDays(i);
            var isToday = day.Date == DateTime.Today;
            var dayBookings = bookings.Where(b => b.StartTime.Date == day.Date).OrderBy(b => b.StartTime).ToList();
            var dayBlocks = blockedTimes.Where(bt => bt.SpecificDate == day.Date).ToList();
            
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", $"week-day {(isToday ? "today" : "")}");
            
            // Day header
            builder.OpenElement(12, "div");
            builder.AddAttribute(13, "class", "day-header");
            
            builder.OpenElement(14, "div");
            builder.AddAttribute(15, "class", "day-name");
            builder.AddContent(16, day.ToString("ddd"));
            builder.CloseElement();
            
            builder.OpenElement(17, "div");
            builder.AddAttribute(18, "class", $"day-date {(isToday ? "fw-bold text-primary" : "")}");
            builder.AddContent(19, day.ToString("dd/MM"));
            builder.CloseElement();
            
            builder.CloseElement(); // div.day-header
            
            // Day content
            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "class", "day-content");
            
            // Render blocked times
            foreach (var block in dayBlocks)
            {
                builder.OpenElement(22, "div");
                builder.AddAttribute(23, "class", "blocked-time-item");
                builder.AddAttribute(24, "onclick", EventCallback.Factory.Create(this, () => SelectBlockedTime(block)));
                
                var isAllDay = block.StartTime == TimeSpan.Zero && block.EndTime.TotalSeconds >= 86399;
                
                builder.OpenElement(25, "div");
                builder.AddAttribute(26, "class", "text-danger small fw-bold");
                builder.AddContent(27, isAllDay ? "🚫 Blocked (All Day)" : $"🚫 {block.StartTime:hh\\:mm}-{block.EndTime:hh\\:mm}");
                builder.CloseElement();
                
                builder.OpenElement(28, "div");
                builder.AddAttribute(29, "class", "text-muted small");
                builder.AddContent(30, block.Notes?.Length > 30 ? block.Notes.Substring(0, 30) + "..." : block.Notes);
                builder.CloseElement();
                
                builder.CloseElement(); // div.blocked-time-item
            }
            
            // Render bookings
            foreach (var booking in dayBookings)
            {
                builder.OpenElement(31, "div");
                builder.AddAttribute(32, "class", $"booking-item status-{booking.Status.ToString().ToLower()}");
                builder.AddAttribute(33, "onclick", EventCallback.Factory.Create(this, () => SelectBooking(booking)));
                
                builder.OpenElement(34, "div");
                builder.AddAttribute(35, "class", "booking-time small");
                builder.AddContent(36, booking.StartTime.ToString("HH:mm"));
                builder.CloseElement();
                
                builder.OpenElement(37, "div");
                builder.AddAttribute(38, "class", "booking-service small fw-bold");
                builder.AddContent(39, booking.Service.Name);
                builder.CloseElement();
                
                builder.OpenElement(40, "div");
                builder.AddAttribute(41, "class", "booking-client small text-muted");
                builder.AddContent(42, $"{booking.Client.FirstName} {booking.Client.LastName}");
                builder.CloseElement();
                
                builder.CloseElement(); // div.booking-item
            }
            
            if (!dayBookings.Any() && !dayBlocks.Any())
            {
                builder.OpenElement(43, "div");
                builder.AddAttribute(44, "class", "text-center text-muted py-3 small");
                builder.AddContent(45, "No appointments");
                builder.CloseElement();
            }
            
            builder.CloseElement(); // div.day-content
            builder.CloseElement(); // div.week-day
        }
        
        builder.CloseElement(); // div.week-grid
        builder.CloseElement(); // div.card-body
        builder.CloseElement(); // div.card
        builder.CloseElement(); // div.col-12
        builder.CloseElement(); // div.row
    };

    private RenderFragment RenderMonthView() => builder =>
    {
        var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var firstDayOfWeek = GetWeekStart(monthStart);
        var daysToShow = 35; // 5 weeks
        
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "row");
        
        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "col-12");
        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "card");
        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "card-body p-0");
        
        builder.OpenElement(8, "div");
        builder.AddAttribute(9, "class", "month-grid");
        
        // Day headers
        var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        foreach (var dayName in dayNames)
        {
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", "month-day-header");
            builder.AddContent(12, dayName);
            builder.CloseElement();
        }
        
        // Calendar days
        for (int i = 0; i < daysToShow; i++)
        {
            var day = firstDayOfWeek.AddDays(i);
            var isToday = day.Date == DateTime.Today;
            var isCurrentMonth = day.Month == currentDate.Month;
            var dayBookings = bookings.Where(b => b.StartTime.Date == day.Date).ToList();
            var dayBlocks = blockedTimes.Where(bt => bt.SpecificDate == day.Date).ToList();
            
            builder.OpenElement(13, "div");
            builder.AddAttribute(14, "class", $"month-day {(isToday ? "today" : "")} {(!isCurrentMonth ? "other-month" : "")}");
            
            builder.OpenElement(15, "div");
            builder.AddAttribute(16, "class", "day-number");
            builder.AddContent(17, day.Day.ToString());
            builder.CloseElement();
            
            builder.OpenElement(18, "div");
            builder.AddAttribute(19, "class", "day-indicators");
            
            if (dayBlocks.Any())
            {
                builder.OpenElement(20, "div");
                builder.AddAttribute(21, "class", "indicator blocked");
                builder.AddAttribute(22, "title", string.Join(", ", dayBlocks.Select(b => b.Notes)));
                builder.AddContent(23, "🚫");
                builder.CloseElement();
            }
            
            if (dayBookings.Any())
            {
                builder.OpenElement(24, "div");
                builder.AddAttribute(25, "class", "indicator bookings");
                builder.AddAttribute(26, "title", $"{dayBookings.Count} appointment(s)");
                builder.AddContent(27, $"{dayBookings.Count}");
                builder.CloseElement();
            }
            
            builder.CloseElement(); // div.day-indicators
            builder.CloseElement(); // div.month-day
        }
        
        builder.CloseElement(); // div.month-grid
        builder.CloseElement(); // div.card-body
        builder.CloseElement(); // div.card
        builder.CloseElement(); // div.col-12
        builder.CloseElement(); // div.row
    };
}


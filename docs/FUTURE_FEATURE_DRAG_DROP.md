# Future Feature: Drag-and-Drop Rescheduling

## Status: Deferred for Future Implementation

### Feature Description
Click and drag bookings to different time slots or rooms/therapists to reschedule them quickly without opening modals.

### Why Deferred
- Current scheduler works well for typical spa volumes
- Implementation encountered Razor syntax complexity with TimeSpan formatting in interpolated strings
- Risk of breaking existing stable functionality
- Can be added as enhancement later when time permits proper debugging

### Business Value
- **Current Priority**: Low - Manual rescheduling through modals is sufficient
- **Future Value**: Medium - Would save time for high-volume days
- **User Impact**: Nice-to-have, not critical for daily operations

### Technical Notes for Future Implementation
The partial implementation attempted:
1. Added drag state variables (`draggedBooking`, `dragOverSlot`, etc.)
2. Made booking cells draggable with `draggable="true"` attribute
3. Added drop handlers with validation for room compatibility and therapist availability
4. CSS animations for drag-over feedback

**Blocking Issue**: Razor syntax errors with `ToString()` format strings in string interpolation within drag event handlers.

**Recommended Approach for Future**:
- Use simpler slot ID format (e.g., `{Hours:D2}{Minutes:D2}`)
- Consider using JavaScript interop for HTML5 drag-and-drop API
- Test in isolation before integrating with main scheduler

### Date Deferred
November 21, 2025

### Prioritization
Will revisit after:
1. Mobile-optimized view is complete
2. Search/find booking feature is complete
3. User feedback indicates need for faster rescheduling workflow


# Location Selection Feature

## Overview

Implemented a professional location selection system similar to major retailers (Kroger, Target, etc.) where users search for their spa location by entering their city, ZIP code, or selecting from available regions.

## User Experience Flow

### 1. Initial Visit
- Users land on the home page
- Primary call-to-action: **"Find Your Spa"** button
- Directs to `/select-location` page

### 2. Location Search
Users can find their spa by:
- **Search Input**: Enter city name or ZIP code
- **Optional State Filter**: Narrow results by state
- **Quick Region Buttons**: One-click search for "Downtown" or "Seaside"

### 3. Search Results
- Displays all matching spa locations in card format
- Each location card shows:
  - Spa name
  - Full address (street, city, state, ZIP)
  - Phone number
  - Availability badge
- Users can click any location card to select it
- Selected location is visually highlighted with:
  - Different background gradient
  - Checkmark badge
  - Border color change

### 4. Confirmation
- After selection, a success banner appears
- Shows selected location details
- **"Continue"** button proceeds to services
- **"Change Location"** button allows re-selection

### 5. Persistent Selection
- Selected location is stored in browser `localStorage`
- Automatically loads on future visits
- No need to re-select unless user wants to change

### 6. Services Page Integration
- Displays selected spa at the top in an info banner
- Shows:
  - Spa icon badge
  - Location name and full address
  - **"Change Location"** button (easily accessible)
- Services are automatically filtered to show only the selected location
- If no location selected, shows a warning with "Select Location" button

## Technical Implementation

### Files Created
1. **`SelectLocation.razor`** - Main location selection page
2. **`SelectLocation.razor.css`** - Styled components for location cards

### Files Modified
1. **`Services.razor`** - Added selected location display and filtering
2. **`Services.razor.css`** - Added location badge styling
3. **`Home.razor`** - Changed primary CTA to "Find Your Spa"

### Data Storage
Uses browser `localStorage` to persist location preference:
```javascript
// Stored values
localStorage.setItem("selectedLocationId", "1");
localStorage.setItem("selectedLocationName", "Downtown Spa");

// Retrieved on page load
var locationId = localStorage.getItem("selectedLocationId");
```

### Key Components

#### LocationSearchModel
```csharp
public class LocationSearchModel
{
    [Required(ErrorMessage = "Please enter a city or ZIP code")]
    public string SearchQuery { get; set; } = string.Empty;
    
    public string State { get; set; } = string.Empty;
}
```

#### Search Logic
```csharp
var query = DbContext.Locations.Where(l => l.IsActive);

if (!string.IsNullOrWhiteSpace(searchModel.SearchQuery))
{
    var searchTerm = searchModel.SearchQuery.Trim().ToLower();
    query = query.Where(l => 
        l.City.ToLower().Contains(searchTerm) ||
        l.ZipCode.Contains(searchTerm) ||
        l.Name.ToLower().Contains(searchTerm));
}
```

## UI/UX Features

### Visual Hierarchy
1. **Unselected Location Card**
   - White background
   - Light gray border (#e0e0e0)
   - Hover effect: Rose gold border + shadow

2. **Selected Location Card**
   - Gradient background (pink tint)
   - Rose gold border
   - Checkmark badge (animated scale-in)
   - Enhanced shadow

3. **Location Badge (Services Page)**
   - Circular icon (60px)
   - Rose gold gradient background
   - White shop icon
   - Paired with location details

### Responsive Design
- Mobile-optimized layout
- Cards stack vertically on small screens
- Touch-friendly button sizes
- Flexible button wrapping

### Accessibility
- Clear visual feedback for selection
- High contrast text
- Icon + text labels
- Keyboard navigation support

## User Benefits

✅ **Clear Location Context**: Users always know which spa they're booking at
✅ **Easy to Change**: One-click access to change location if needed
✅ **Persistent Selection**: Don't need to re-select every visit
✅ **No Accidental Changes**: Deliberate location selection process
✅ **Professional Experience**: Matches expectations from major retail brands

## Business Benefits

✅ **Reduces Booking Errors**: Users less likely to book at wrong location
✅ **Location-Specific Services**: Can show only relevant services
✅ **Analytics Potential**: Track which locations users search for
✅ **Scalability**: Easy to add more locations without UI clutter

## Future Enhancements

### Phase 2 (Optional)
1. **Geolocation API**: Auto-detect user's location for faster search
2. **Distance Calculation**: Show "X miles away" for each spa
3. **Map View**: Display spas on an interactive map
4. **Favorite Location**: Allow users to save multiple preferred locations
5. **Location Hours**: Show current open/closed status
6. **Therapist Availability**: Preview available staff at each location

### Phase 3 (Advanced)
1. **Multi-Location Booking**: Book at different spas in one session
2. **Location Comparison**: Compare services/prices across locations
3. **Location Reviews**: Show ratings per location
4. **Wait Times**: Display current booking availability

## Testing Scenarios

### Happy Path
1. ✅ User searches for city → finds results → selects spa → sees services
2. ✅ User returns to site → location remembered → direct to services
3. ✅ User changes location → clears storage → select new spa → update services

### Edge Cases
1. ✅ Search with no results → show helpful message
2. ✅ No location selected → warning on services page
3. ✅ Invalid stored location ID → prompt to re-select
4. ✅ All locations inactive → show "no spas available"

### Mobile Testing
1. ✅ Touch interactions work smoothly
2. ✅ Cards properly sized for mobile
3. ✅ Buttons accessible and tappable
4. ✅ Address text wraps appropriately

## Configuration

### Adding New Locations
Locations are managed in the database (`Locations` table):
```sql
INSERT INTO "Locations" (
    "Name", "Address", "City", "State", "ZipCode", 
    "Phone", "Email", "IsActive", "CreatedAt"
)
VALUES (
    'New Spa Location',
    '123 Wellness Blvd',
    'San Diego',
    'CA',
    '92101',
    '(619) 555-0100',
    'sandiego@spabooker.com',
    true,
    NOW()
);
```

### Quick Search Regions
Customize the quick search buttons in `SelectLocation.razor`:
```csharp
<button class="btn btn-outline-secondary" @onclick="@(() => QuickSearch("Downtown"))">
    <i class="bi bi-building me-1"></i> Downtown
</button>
<button class="btn btn-outline-secondary" @onclick="@(() => QuickSearch("Seaside"))">
    <i class="bi bi-water me-1"></i> Seaside
</button>
// Add more as needed
```

## Maintenance Notes

### localStorage Cleanup
If users report issues with stuck locations:
```javascript
// Clear in browser console
localStorage.removeItem("selectedLocationId");
localStorage.removeItem("selectedLocationName");
```

### Database Migration Impact
If locations are renamed or deactivated, the system will:
- Detect invalid stored location ID
- Prompt user to re-select
- No data corruption or errors


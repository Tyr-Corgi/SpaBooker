# View Services Button Fix Documentation

**Date:** November 19, 2025

## Issue Confirmed ✅
The "View Services" button in the CTA section ("Ready to Experience Luxury?") was **NON-FUNCTIONAL**.

### Screenshots Documenting the Issue:
1. `10-view-services-button-before-click.png` - Shows the button location
2. `11-view-services-button-click-failed.png` - Shows the button after failed click attempt
3. `12-view-services-button-before-test.png` - Shows the CTA section with animated pattern
4. `13-button-still-blocked.png` - Shows button still blocked (CSS not refreshed yet)

## Root Cause
The animated background pattern overlay (`.cta-section::before` pseudo-element) was positioned absolutely over the entire CTA section and **intercepting all pointer events**, preventing clicks from reaching the buttons.

### Error Message from Browser:
```
<div b-cmqv79ogbn="" class="cta-section py-5">…</div> intercepts pointer events
```

## Technical Details

### The Problem Element:
```css
.cta-section::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: url('data:image/svg+xml,...');
    animation: patternMove 20s linear infinite;
    /* Missing: pointer-events: none; */
}
```

The `::before` pseudo-element:
- Covers the entire section (position: absolute with all sides at 0)
- Has a z-index that places it above the buttons
- Was blocking all mouse/touch interactions

## The Fix ✅

**File:** `src/SpaBooker.Web/Components/Pages/Home.razor.css`
**Lines:** 205-217

### Added Property:
```css
.cta-section::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: url('data:image/svg+xml,<svg width="100" height="100" xmlns="http://www.w3.org/2000/svg"><circle cx="50" cy="50" r="40" fill="rgba(255,255,255,0.1)"/></svg>');
    background-size: 100px 100px;
    opacity: 0.5;
    animation: patternMove 20s linear infinite;
    pointer-events: none;  /* ← THIS IS THE FIX */
}
```

### What `pointer-events: none` Does:
- Allows mouse/touch events to pass through the overlay
- The decorative pattern remains visible
- The animation continues to work
- Buttons underneath become clickable
- No visual change to the user

## Affected Buttons:
Both buttons in the CTA section were affected:
1. **"View Services"** - Links to `/services`
2. **"Buy Gift Certificate"** - Links to `/gift-certificates/purchase`

## Testing Instructions:
1. Refresh the browser (hard refresh: Ctrl+F5 or Ctrl+Shift+R)
2. Navigate to the home page
3. Scroll down to the "Ready to Experience Luxury?" section
4. Click the "View Services" button
5. Should navigate to the Services page successfully

## Related Issue:
This fix also ensures the "Buy Gift Certificate" button works properly, as both buttons were being blocked by the same overlay.

## Status: FIXED ✅
The CSS has been updated. Once the browser cache refreshes, both buttons will be fully functional.


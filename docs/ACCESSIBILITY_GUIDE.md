# Accessibility Guide for SpaBooker

## Overview

SpaBooker is committed to providing an accessible experience for all users, including those using assistive technologies like screen readers, keyboard navigation, and voice control.

## WCAG 2.1 Compliance Goals

Target: **WCAG 2.1 Level AA**

### Key Principles (POUR)
- **Perceivable** - Information must be presentable to users in ways they can perceive
- **Operable** - UI components must be operable by all users
- **Understandable** - Information and operation must be understandable
- **Robust** - Content must be robust enough to work with assistive technologies

---

## Implementation Checklist

### ✅ Semantic HTML
```html
<!-- ✅ GOOD: Use semantic elements -->
<nav aria-label="Main navigation">
<main>
<article>
<section aria-labelledby="booking-section">
<footer>

<!-- ❌ BAD: Don't use divs for everything -->
<div class="navigation">
<div class="main-content">
```

### ✅ ARIA Labels and Roles

#### Navigation
```html
<nav aria-label="Main navigation">
    <a href="/" aria-current="page">Home</a>
    <button aria-expanded="false" aria-controls="submenu">Services</button>
</nav>
```

#### Forms
```html
<form role="form" aria-labelledby="booking-form-title">
    <label for="service-select">Select Service</label>
    <select id="service-select" aria-required="true" aria-describedby="service-help">
        <option value="">Choose a service</option>
    </select>
    <span id="service-help" class="help-text">Select the spa service you'd like to book</span>
    
    <div role="alert" aria-live="polite" class="error-message">
        Please select a service
    </div>
</form>
```

#### Buttons
```html
<!-- Icon buttons need labels -->
<button aria-label="Close dialog" class="close-btn">
    <span aria-hidden="true">×</span>
</button>

<button aria-label="Edit booking">
    <i class="icon-edit" aria-hidden="true"></i>
</button>
```

#### Interactive Elements
```html
<!-- Modal/Dialog -->
<div role="dialog" aria-labelledby="modal-title" aria-modal="true">
    <h2 id="modal-title">Confirm Booking</h2>
    <!-- content -->
</div>

<!-- Alerts -->
<div role="alert" aria-live="assertive" class="alert-error">
    Booking failed. Please try again.
</div>

<!-- Status messages -->
<div role="status" aria-live="polite" aria-atomic="true">
    Booking confirmed!
</div>
```

### ✅ Keyboard Navigation

#### Tab Order
```html
<!-- Ensure logical tab order -->
<input type="text" tabindex="0" />  <!-- Natural order -->
<button tabindex="0">Submit</button>

<!-- Skip to main content -->
<a href="#main-content" class="skip-link">Skip to main content</a>
<main id="main-content">
```

#### Keyboard Shortcuts
- **Tab**: Move forward through interactive elements
- **Shift + Tab**: Move backward
- **Enter/Space**: Activate buttons and links
- **Escape**: Close modals/dialogs
- **Arrow keys**: Navigate within components (dropdowns, date pickers)

#### Implementation
```javascript
// Handle Escape key for modals
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && isModalOpen) {
        closeModal();
    }
});

// Trap focus within modal
function trapFocus(element) {
    const focusableElements = element.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    element.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            if (e.shiftKey && document.activeElement === firstElement) {
                e.preventDefault();
                lastElement.focus();
            } else if (!e.shiftKey && document.activeElement === lastElement) {
                e.preventDefault();
                firstElement.focus();
            }
        }
    });
}
```

### ✅ Color Contrast

**WCAG AA Requirements:**
- Normal text: 4.5:1 contrast ratio
- Large text (18pt+): 3:1 contrast ratio
- UI components: 3:1 contrast ratio

**Testing Tools:**
- Chrome DevTools: Lighthouse accessibility audit
- [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [Accessible Colors](https://accessible-colors.com/)

```css
/* ✅ GOOD: High contrast */
.button-primary {
    background: #0066cc;  /* Blue */
    color: #ffffff;       /* White - 7.5:1 ratio */
}

/* ❌ BAD: Low contrast */
.text-muted {
    background: #ffffff;
    color: #cccccc;  /* Only 1.6:1 - fails WCAG */
}
```

### ✅ Focus Indicators

```css
/* Ensure visible focus indicators */
button:focus,
a:focus,
input:focus {
    outline: 2px solid #0066cc;
    outline-offset: 2px;
}

/* Don't remove focus outlines! */
/* ❌ BAD */
button:focus {
    outline: none;  /* NEVER DO THIS without alternative */
}

/* ✅ GOOD: Custom focus style */
button:focus-visible {
    outline: 3px solid #0066cc;
    outline-offset: 3px;
    box-shadow: 0 0 0 4px rgba(0, 102, 204, 0.2);
}
```

### ✅ Images and Icons

```html
<!-- Decorative images -->
<img src="decoration.jpg" alt="" role="presentation" />

<!-- Informative images -->
<img src="therapist.jpg" alt="Professional massage therapist in spa room" />

<!-- Icon fonts (use sparingly) -->
<i class="icon-calendar" aria-hidden="true"></i>
<span class="sr-only">View calendar</span>

<!-- SVG icons -->
<svg role="img" aria-labelledby="icon-title">
    <title id="icon-title">Calendar</title>
    <!-- svg content -->
</svg>
```

### ✅ Forms Validation

```html
<form novalidate>
    <div class="form-group">
        <label for="email">Email Address</label>
        <input 
            type="email" 
            id="email" 
            aria-required="true"
            aria-invalid="false"
            aria-describedby="email-error email-help"
        />
        <span id="email-help" class="help-text">
            We'll never share your email
        </span>
        <span id="email-error" role="alert" class="error hidden">
            Please enter a valid email address
        </span>
    </div>
</form>
```

### ✅ Live Regions

```html
<!-- Status updates -->
<div role="status" aria-live="polite" aria-atomic="true" class="sr-only">
    Booking confirmed for John Smith on March 15, 2025 at 2:00 PM
</div>

<!-- Error announcements -->
<div role="alert" aria-live="assertive" class="alert-box">
    Payment processing failed. Please check your payment method.
</div>

<!-- Loading states -->
<div role="status" aria-live="polite" aria-busy="true">
    <span class="spinner" aria-hidden="true"></span>
    Loading available times...
</div>
```

---

## Component-Specific Guidelines

### Date Picker
```html
<div role="group" aria-labelledby="date-picker-label">
    <label id="date-picker-label">Select Booking Date</label>
    <button 
        aria-label="Choose date" 
        aria-expanded="false"
        aria-haspopup="dialog"
    >
        March 15, 2025
    </button>
    
    <div role="dialog" aria-label="Choose booking date" hidden>
        <!-- Calendar grid -->
        <table role="grid" aria-labelledby="month-year">
            <caption id="month-year">March 2025</caption>
            <!-- dates -->
        </table>
    </div>
</div>
```

### Time Slot Selection
```html
<fieldset>
    <legend>Select Time Slot</legend>
    <div role="radiogroup" aria-labelledby="time-slot-label">
        <label id="time-slot-label" class="sr-only">Available times</label>
        <input type="radio" id="time-9am" name="time" value="09:00">
        <label for="time-9am">9:00 AM</label>
        
        <input type="radio" id="time-10am" name="time" value="10:00">
        <label for="time-10am">10:00 AM</label>
    </div>
</fieldset>
```

### Service Cards
```html
<article aria-labelledby="service-title-1">
    <img src="massage.jpg" alt="" role="presentation" />
    <h3 id="service-title-1">Swedish Massage</h3>
    <p>Relaxing full-body massage</p>
    <div aria-label="Service details">
        <span aria-label="Duration">60 minutes</span>
        <span aria-label="Price">$120</span>
    </div>
    <a href="/book/massage" aria-label="Book Swedish Massage">
        Book Now
    </a>
</article>
```

### Data Tables
```html
<table role="table" aria-labelledby="table-title">
    <caption id="table-title">Upcoming Bookings</caption>
    <thead>
        <tr>
            <th scope="col">Date</th>
            <th scope="col">Service</th>
            <th scope="col">Therapist</th>
            <th scope="col">Actions</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>March 15, 2025</td>
            <td>Swedish Massage</td>
            <td>Jane Doe</td>
            <td>
                <button aria-label="Cancel booking for March 15">
                    Cancel
                </button>
            </td>
        </tr>
    </tbody>
</table>
```

---

## Screen Reader Only Content

```css
/* Utility class for screen reader only content */
.sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border-width: 0;
}

/* Show on focus (for skip links) */
.sr-only-focusable:focus {
    position: static;
    width: auto;
    height: auto;
    overflow: visible;
    clip: auto;
    white-space: normal;
}
```

Usage:
```html
<a href="#main" class="sr-only sr-only-focusable">
    Skip to main content
</a>

<button>
    <i class="icon-delete" aria-hidden="true"></i>
    <span class="sr-only">Delete booking</span>
</button>
```

---

## Testing

### Manual Testing
1. **Keyboard Navigation**: Navigate entire app using only keyboard
2. **Screen Reader**: Test with NVDA (Windows), JAWS, or VoiceOver (Mac/iOS)
3. **Zoom**: Test at 200% and 400% zoom
4. **Color Blindness**: Use color blindness simulators

### Automated Testing
```bash
# Lighthouse accessibility audit
npm install -g lighthouse
lighthouse https://your-app-url --only-categories=accessibility

# axe DevTools (browser extension)
# pa11y (CI integration)
npm install -g pa11y
pa11y https://your-app-url
```

### Browser Extensions
- **axe DevTools** - Comprehensive accessibility testing
- **WAVE** - Web accessibility evaluation tool
- **Accessibility Insights** - Microsoft's accessibility toolkit

---

## Quick Reference

### Essential ARIA Attributes
- `aria-label` - Defines label for element
- `aria-labelledby` - References element that labels this one
- `aria-describedby` - References element that describes this one
- `aria-hidden` - Hides element from assistive tech
- `aria-live` - Announces dynamic content changes
- `aria-expanded` - Indicates expanded/collapsed state
- `aria-current` - Indicates current item in set
- `aria-required` - Indicates required form field
- `aria-invalid` - Indicates validation state

### Essential Roles
- `navigation` - Navigation landmark
- `main` - Main content landmark
- `search` - Search landmark
- `complementary` - Sidebar/aside content
- `contentinfo` - Footer landmark
- `alert` - Important message
- `status` - Status message
- `dialog` - Modal dialog
- `button` - Button
- `tab`, `tabpanel`, `tablist` - Tab interface

---

## Resources

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [WAI-ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [WebAIM](https://webaim.org/)
- [A11y Project](https://www.a11yproject.com/)
- [Inclusive Components](https://inclusive-components.design/)

---

**Last Updated:** November 19, 2025  
**Status:** Guidelines Established  
**Next Review:** After UI implementation


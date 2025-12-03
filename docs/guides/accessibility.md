# Accessibility Guide

## Overview

This guide covers accessibility (a11y) implementation for the SpaBooker application to ensure compliance with WCAG 2.1 guidelines.

## Current Status

### Implemented
- Semantic HTML structure
- Form labels associated with inputs
- Focus indicators on interactive elements
- Color contrast in primary theme
- Responsive design for various screen sizes

### Needs Improvement
- ARIA labels on some interactive elements
- Keyboard navigation for complex components
- Screen reader announcements for dynamic content
- Skip navigation links
- Focus management in modals

---

## WCAG 2.1 Compliance Checklist

### Level A (Minimum)

- [ ] **1.1.1 Non-text Content**: All images have alt text
- [ ] **1.3.1 Info and Relationships**: Semantic HTML used
- [ ] **1.4.1 Use of Color**: Color not sole means of conveying info
- [ ] **2.1.1 Keyboard**: All functionality accessible via keyboard
- [ ] **2.4.1 Bypass Blocks**: Skip navigation links provided
- [ ] **2.4.2 Page Titled**: Descriptive page titles
- [ ] **3.1.1 Language of Page**: `lang` attribute set
- [ ] **4.1.1 Parsing**: Valid HTML
- [ ] **4.1.2 Name, Role, Value**: ARIA properly implemented

### Level AA (Recommended)

- [ ] **1.4.3 Contrast (Minimum)**: 4.5:1 for normal text
- [ ] **1.4.4 Resize Text**: Text scalable to 200%
- [ ] **2.4.6 Headings and Labels**: Descriptive headings
- [ ] **2.4.7 Focus Visible**: Visible focus indicator
- [ ] **3.2.3 Consistent Navigation**: Navigation consistent

---

## Implementation Guidelines

### Semantic HTML

Use appropriate HTML elements:

```html
<!-- Good -->
<nav aria-label="Main navigation">
  <ul>
    <li><a href="/bookings">Bookings</a></li>
  </ul>
</nav>

<!-- Avoid -->
<div class="nav">
  <div class="nav-item" onclick="navigate()">Bookings</div>
</div>
```

### ARIA Labels

Add ARIA labels for screen readers:

```html
<!-- Buttons with icons only -->
<button aria-label="Close dialog" class="btn-close">
  <span class="bi bi-x" aria-hidden="true"></span>
</button>

<!-- Form inputs -->
<input type="text" 
       id="search" 
       aria-label="Search clients"
       aria-describedby="search-help">
<span id="search-help" class="visually-hidden">
  Enter client name or email to search
</span>
```

### Keyboard Navigation

Ensure all interactive elements are keyboard accessible:

```csharp
// In Blazor component
@onkeydown="HandleKeyDown"

@code {
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Key == " ")
        {
            // Trigger action
            OnClick();
        }
        else if (e.Key == "Escape")
        {
            // Close modal/dropdown
            Close();
        }
    }
}
```

### Focus Management

Manage focus when content changes:

```csharp
// After opening modal, focus first interactive element
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (showModal && !modalFocused)
    {
        await JS.InvokeVoidAsync("focusElement", "modal-close-button");
        modalFocused = true;
    }
}
```

### Color Contrast

Ensure sufficient contrast ratios:

| Element | Foreground | Background | Ratio |
|---------|-----------|------------|-------|
| Body text | #2D2D2D | #FFFFFF | 14.5:1 |
| Primary button | #FFFFFF | #B76E79 | 4.5:1 |
| Links | #B76E79 | #FFFFFF | 4.6:1 |

### Screen Reader Announcements

Announce dynamic content changes:

```html
<!-- Live region for status updates -->
<div aria-live="polite" aria-atomic="true" class="visually-hidden">
  @statusMessage
</div>

@code {
    private string statusMessage = "";
    
    private void ShowSuccess()
    {
        statusMessage = "Booking created successfully";
        StateHasChanged();
    }
}
```

---

## Component Patterns

### Modal Dialogs

```html
<div class="modal" 
     role="dialog" 
     aria-modal="true"
     aria-labelledby="modal-title"
     aria-describedby="modal-description">
  <div class="modal-content">
    <h2 id="modal-title">Book Appointment</h2>
    <p id="modal-description">Fill out the form to book your appointment.</p>
    
    <!-- Modal content -->
    
    <button type="button" 
            id="modal-close-button"
            aria-label="Close"
            @onclick="CloseModal">
      Close
    </button>
  </div>
</div>
```

### Data Tables

```html
<table aria-label="Client bookings">
  <caption class="visually-hidden">
    List of upcoming appointments for this client
  </caption>
  <thead>
    <tr>
      <th scope="col">Date</th>
      <th scope="col">Service</th>
      <th scope="col">Therapist</th>
      <th scope="col">Actions</th>
    </tr>
  </thead>
  <tbody>
    @foreach (var booking in bookings)
    {
      <tr>
        <td>@booking.Date.ToShortDateString()</td>
        <td>@booking.Service.Name</td>
        <td>@booking.Therapist?.FullName</td>
        <td>
          <button aria-label="Cancel booking for @booking.Date.ToShortDateString()">
            Cancel
          </button>
        </td>
      </tr>
    }
  </tbody>
</table>
```

### Form Validation

```html
<div class="form-group">
  <label for="email">Email Address</label>
  <input type="email" 
         id="email" 
         @bind="email"
         aria-required="true"
         aria-invalid="@(emailError != null)"
         aria-describedby="email-error">
  @if (emailError != null)
  {
    <span id="email-error" class="error" role="alert">
      @emailError
    </span>
  }
</div>
```

---

## Testing Accessibility

### Automated Tools
- [axe DevTools](https://www.deque.com/axe/) - Browser extension
- [WAVE](https://wave.webaim.org/) - Web accessibility evaluation
- [Lighthouse](https://developers.google.com/web/tools/lighthouse) - Chrome DevTools

### Manual Testing
1. Navigate entire application using only keyboard
2. Test with screen reader (NVDA, VoiceOver, JAWS)
3. Zoom to 200% and verify usability
4. Disable CSS and verify content order
5. Check color contrast with contrast checker

### Screen Reader Testing

| Screen Reader | Browser | OS |
|--------------|---------|-----|
| NVDA | Firefox | Windows |
| JAWS | Chrome | Windows |
| VoiceOver | Safari | macOS |
| TalkBack | Chrome | Android |
| VoiceOver | Safari | iOS |

---

## CSS Utilities

```css
/* Visually hidden but accessible to screen readers */
.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

/* Focus indicator */
:focus-visible {
  outline: 3px solid #B76E79;
  outline-offset: 2px;
}

/* Skip link */
.skip-link {
  position: absolute;
  top: -40px;
  left: 0;
  background: #B76E79;
  color: white;
  padding: 8px;
  z-index: 100;
}

.skip-link:focus {
  top: 0;
}
```

---

## Resources

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [MDN Accessibility](https://developer.mozilla.org/en-US/docs/Web/Accessibility)
- [A11y Project Checklist](https://www.a11yproject.com/checklist/)
- [Inclusive Components](https://inclusive-components.design/)


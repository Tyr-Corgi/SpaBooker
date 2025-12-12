# Refactored Pages - Manual Testing Guide

## 🎯 Testing Overview

This guide will help you manually test the two refactored admin pages to verify that all functionality works correctly after the component extraction.

**Pages to Test:**
1. Staff Scheduling (`/admin/staff-scheduling`)
2. Client Management (`/admin/clients`)

**Application URL:** 
- HTTPS: `https://localhost:7065`
- HTTP: `http://localhost:5226`

---

## ✅ Pre-Test Checklist

- [x] Application is running (PID: 4496)
- [x] All tests pass (131/131) ✅
- [x] Clean build completed ✅
- [x] Original files backed up ✅

---

## 📋 Test Plan

### Test 1: Staff Scheduling Page

**URL:** `https://localhost:7065/admin/staff-scheduling`

#### Components to Test:

1. **AlertMessages Component** ✨
   - [ ] Success messages display correctly
   - [ ] Error messages display correctly
   - [ ] Dismiss button works
   - [ ] Messages auto-clear when expected

2. **TherapistSelector Component** ✨
   - [ ] Dropdown shows all therapists
   - [ ] "All Staff Overview" option works
   - [ ] Individual therapist selection works
   - [ ] Summary text updates when therapist changes
   - [ ] Current week status displays correctly

3. **AllStaffCalendarView Component** ✨
   - [ ] Calendar renders with all staff
   - [ ] Day/Week/Month view toggle works
   - [ ] Navigation (Previous/Next) works
   - [ ] Date picker "Jump to" works
   - [ ] "Today" button works
   - [ ] Therapist badges show on scheduled days
   - [ ] Color coding per therapist works
   - [ ] Click on day opens schedule modal

4. **IndividualStaffCalendarView Component** ✨
   - [ ] Calendar renders for selected therapist
   - [ ] Week/Month view toggle works
   - [ ] Navigation (Previous/Next) works
   - [ ] Date picker works
   - [ ] Scheduled days show in green
   - [ ] Unscheduled days show in yellow
   - [ ] Past days are grayed out
   - [ ] Click on day opens schedule modal
   - [ ] Schedule times display correctly

5. **QuickSchedulePanel Component** ✨
   - [ ] Date range inputs work
   - [ ] Time range inputs work
   - [ ] Notes textarea works
   - [ ] "Apply Schedule" button works
   - [ ] Loading spinner shows during save
   - [ ] Success message after save

6. **UpcomingAppointmentsList Component** ✨
   - [ ] List shows upcoming appointments
   - [ ] Table displays all columns correctly:
     - Date & Time
     - Client name
     - Service name
     - Duration
     - Room (with color badge)
     - Status badge (Confirmed/Pending/etc.)
   - [ ] Shows "No appointments" when empty
   - [ ] Limits to 20 appointments with count message

#### Test Scenarios:

**Scenario 1: Select a Therapist**
1. Navigate to `/admin/staff-scheduling`
2. Select a therapist from dropdown
3. Verify calendar updates
4. Verify upcoming appointments list updates
5. Verify summary text updates

**Scenario 2: Create a Quick Schedule**
1. Select a therapist
2. Fill in date range (e.g., next week)
3. Fill in time range (e.g., 9 AM - 5 PM)
4. Add notes
5. Click "Apply Schedule"
6. Verify success message appears
7. Verify calendar updates with new schedule

**Scenario 3: Navigate Calendar Views**
1. Switch between Day/Week/Month views
2. Click Previous/Next navigation
3. Use date picker to jump to specific date
4. Click "Today" button
5. Verify all views render correctly

**Scenario 4: All Staff View**
1. Select "All Staff Overview"
2. Verify all therapists appear in calendar
3. Verify color-coded badges for each therapist
4. Verify schedule times show for each

---

### Test 2: Client Management Page

**URL:** `https://localhost:7065/admin/clients`

#### Components to Test:

1. **AlertMessages Component** ✨ (reused)
   - [ ] Success messages display correctly
   - [ ] Error messages display correctly
   - [ ] Dismiss button works

2. **ClientStatsCards Component** ✨ (reused)
   - [ ] Total Clients count displays
   - [ ] Active Members count displays
   - [ ] New This Month count displays
   - [ ] Inactive (30d) count displays
   - [ ] Icons display correctly

3. **ClientFilterTabs Component** ✨
   - [ ] "All Clients" tab works
   - [ ] "Active Members" filter works
   - [ ] "Non-Members" filter works
   - [ ] "Inactive (30d+)" filter works
   - [ ] Active tab is highlighted
   - [ ] Client list updates when filter changes

4. **ClientSearchBar Component** ✨
   - [ ] Search input field works
   - [ ] Search by name works
   - [ ] Search by email works
   - [ ] Search by phone works
   - [ ] Sort toggle button works
   - [ ] Sort by Name works
   - [ ] Sort by Last Visit works

5. **ClientTableRow Component** ✨
   - [ ] Client avatar shows initials
   - [ ] Client name displays correctly
   - [ ] "Joined" date displays
   - [ ] Email displays with icon
   - [ ] Phone number displays (if present)
   - [ ] Membership badge shows (if active)
   - [ ] Credits count shows
   - [ ] Last visit date shows
   - [ ] Next visit date shows
   - [ ] Visit frequency badge displays correctly:
     - [ ] "New Client" for 0 bookings
     - [ ] "Weekly" for frequent visitors
     - [ ] "Monthly", "Quarterly", etc.
   - [ ] "Locked" badge shows if account locked
   - [ ] "Inactive" badge shows if 90+ days
   - [ ] Action buttons render:
     - [ ] "Book Appointment" button
     - [ ] "Notes" button (rose-gold gradient)
     - [ ] "Details" button

6. **Existing Modals** (not refactored)
   - [ ] Client Details modal opens
   - [ ] Edit Client modal opens
   - [ ] Add New Client modal opens
   - [ ] Notes modal opens
   - [ ] Booking modal opens

#### Test Scenarios:

**Scenario 1: View All Clients**
1. Navigate to `/admin/clients`
2. Verify stats cards show correct counts
3. Verify client table loads
4. Verify all client rows render correctly
5. Scroll through list to check multiple clients

**Scenario 2: Filter Clients**
1. Click "Active Members" tab
2. Verify only members show
3. Click "Non-Members" tab
4. Verify only non-members show
5. Click "Inactive (30d+)" tab
6. Verify only inactive clients show
7. Click "All Clients" to reset

**Scenario 3: Search Clients**
1. Type a client name in search box
2. Verify results filter immediately
3. Clear search
4. Search by email
5. Verify results update
6. Search by phone number
7. Verify results update

**Scenario 4: Client Actions**
1. Click "Book Appointment" on a client
2. Verify booking modal opens
3. Close modal
4. Click "Notes" button
5. Verify notes modal opens
6. Close modal
7. Click "Details" button
8. Verify details modal opens with tabs

**Scenario 5: Add New Client**
1. Click "Add New Client" button
2. Verify modal opens
3. Fill in client details
4. Click "Create Client"
5. Verify success message
6. Verify new client appears in list

**Scenario 6: Combined Filters**
1. Select "Active Members" filter
2. Enter search term
3. Verify both filter AND search apply
4. Verify correct results show

---

## 🐛 Known Issues to Watch For

### Potential Issues:

1. **Component State Sync**
   - If filters don't update immediately, check EventCallback wiring

2. **Calendar Rendering**
   - Large calendars may take a moment to render
   - Check console for any JavaScript errors

3. **Modal Interactions**
   - Modals not refactored, so they work as before
   - Verify they still open/close correctly

4. **Search Performance**
   - Search should be instant (no lag)
   - If slow, check that it's not re-filtering unnecessarily

---

## ✅ Success Criteria

### Staff Scheduling Page ✅
- [ ] All 6 components render without errors
- [ ] Therapist selection works
- [ ] Calendar views work (Day/Week/Month)
- [ ] Quick schedule creation works
- [ ] Appointments list displays
- [ ] No console errors

### Client Management Page ✅
- [ ] All 5 components render without errors
- [ ] Stats cards show correct data
- [ ] Filter tabs work correctly
- [ ] Search works for name/email/phone
- [ ] Client rows render with all data
- [ ] Action buttons work
- [ ] Modals open correctly
- [ ] No console errors

---

## 🔍 Browser Console Testing

Open browser DevTools (F12) and check for:

### Should NOT See:
- ❌ Red errors in console
- ❌ Component rendering errors
- ❌ Missing parameters warnings
- ❌ EventCallback errors

### Should See (Optional):
- ✅ Blazor debugging info (normal)
- ✅ Network requests (normal)
- ✅ Component initialization logs (if any)

---

## 📊 Performance Checks

### Page Load Times:
- [ ] Staff Scheduling loads in < 2 seconds
- [ ] Client Management loads in < 2 seconds
- [ ] Filter changes are instant (< 100ms)
- [ ] Search updates are instant (< 100ms)

### Component Rendering:
- [ ] Calendar renders smoothly
- [ ] No flickering when switching views
- [ ] Table rows render without lag
- [ ] Smooth scrolling through client list

---

## 🎨 Visual Verification

### Staff Scheduling:
- [ ] Layout looks clean and organized
- [ ] Components align properly
- [ ] Calendar is readable
- [ ] Colors are consistent
- [ ] Icons display correctly
- [ ] Buttons are styled correctly

### Client Management:
- [ ] Stats cards are aligned
- [ ] Filter tabs are styled correctly
- [ ] Search bar is prominent
- [ ] Table is responsive
- [ ] Avatar circles show initials
- [ ] Badges are color-coded correctly
- [ ] Rose-gold gradient on Notes button

---

## 📝 Test Results Template

Copy this template to record your test results:

```markdown
## Test Results - [Date/Time]

### Environment
- URL: https://localhost:7065
- Browser: [Chrome/Edge/Firefox]
- Version: [Browser Version]

### Staff Scheduling Tests
- [x] Page loads successfully
- [ ] AlertMessages component: ✅ / ❌
- [ ] TherapistSelector component: ✅ / ❌
- [ ] AllStaffCalendarView component: ✅ / ❌
- [ ] IndividualStaffCalendarView component: ✅ / ❌
- [ ] QuickSchedulePanel component: ✅ / ❌
- [ ] UpcomingAppointmentsList component: ✅ / ❌

**Issues Found:**
- [List any issues here]

### Client Management Tests
- [ ] Page loads successfully
- [ ] AlertMessages component: ✅ / ❌
- [ ] ClientStatsCards component: ✅ / ❌
- [ ] ClientFilterTabs component: ✅ / ❌
- [ ] ClientSearchBar component: ✅ / ❌
- [ ] ClientTableRow component: ✅ / ❌

**Issues Found:**
- [List any issues here]

### Overall Result
- [ ] ✅ All tests pass - Ready for production
- [ ] ⚠️ Minor issues found - Needs fixes
- [ ] ❌ Major issues found - Requires investigation

**Tester:** [Your Name]
**Date:** [Date]
```

---

## 🚀 Next Steps After Testing

### If All Tests Pass ✅
1. Mark pages as production-ready
2. Update team on successful refactoring
3. Plan rollout to production
4. Monitor for any edge cases

### If Issues Found ⚠️
1. Document specific issues
2. Create bug tickets
3. Fix issues
4. Re-test
5. Update documentation

---

## 📞 Quick Reference

**Application URLs:**
- Staff Scheduling: `https://localhost:7065/admin/staff-scheduling`
- Client Management: `https://localhost:7065/admin/clients`

**Documentation:**
- Component overview: `COMPONENT_REFACTORING.md`
- Complete summary: `COMPLETE_REFACTORING_SUMMARY.md`
- Test results: `TESTING_SUMMARY.md`

**Rollback Command (if needed):**
```bash
cd src/SpaBooker.Web/Features/Admin
Copy-Item StaffScheduling_Original.razor.bak StaffScheduling.razor -Force
Copy-Item ClientManagement_Original.razor.bak ClientManagement.razor -Force
```

---

**Happy Testing! 🎉**

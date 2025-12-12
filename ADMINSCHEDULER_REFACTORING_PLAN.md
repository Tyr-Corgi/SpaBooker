# AdminScheduler Refactoring Plan

## 📊 Current State

**File**: `AdminScheduler.razor`  
**Size**: 2,486 lines (110 KB)  
**Structure**:
- UI Section: Lines 1-1011 (1,011 lines)
- @code Section: Lines 1012-2486 (1,474 lines!)

**Major Sections**:
1. Header & Loading States (40 lines)
2. Location & Date Controls (60 lines)
3. View Toggle Options (30 lines)
4. Schedule Grid (220 lines)
5. Assignment Modal (130 lines)
6. Therapist Booking Modal (150 lines)
7. Booking Details Modal (200 lines)
8. Client Card Modal (100 lines)
9. Cancel Confirmation Modal (50 lines)
10. @code Section with massive business logic (1,474 lines!)

---

## 🎯 Refactoring Strategy

### Phase 1: Extract UI Components (Reuse Existing + Create New)

#### **Reusable from Existing** ✨
1. **AlertMessages.razor** (already exists)
   - Success/error messages

2. **ClientStatsCards.razor** (already exists - might adapt)
   - Could create scheduler-specific stats

#### **New Components to Create** 🆕

3. **SchedulerLocationSelector.razor**
   - Location dropdown
   - Date navigation controls
   - "Today" button
   - ~60 lines

4. **SchedulerViewToggle.razor**
   - Show Rooms checkbox
   - Show Therapists checkbox
   - ~30 lines

5. **ScheduleGridHeader.razor**
   - Time column header
   - Room/Therapist column headers
   - Sub-headers with names
   - ~80 lines

6. **ScheduleGridRow.razor**
   - Time slot cell
   - Room/Therapist cells with bookings
   - Click handlers
   - ~120 lines

7. **ScheduleGridCell.razor**
   - Individual cell with booking display
   - Color coding
   - Click handler
   - ~60 lines

### Phase 2: Keep Modals (Too Complex for Now)

**Modals** (keep in main file for now):
- Assignment Modal (130 lines)
- Therapist Booking Modal (150 lines)
- Booking Details Modal (200 lines)
- Client Card Modal (100 lines)
- Cancel Confirmation Modal (50 lines)

**Total Modal Lines**: ~630 lines (will keep for now)

---

## 📉 Expected Results

### Before:
```
AdminScheduler.razor: 2,486 lines
```

### After:
```
AdminScheduler.razor: ~900 lines (64% reduction)
  - Header: 20 lines
  - Controls (using components): 10 lines
  - Grid (using components): 30 lines
  - Modals (kept): 630 lines
  - @code section: ~800 lines (some helpers moved to components)

New Components:
  + SchedulerLocationSelector.razor: 60 lines
  + SchedulerViewToggle.razor: 30 lines
  + ScheduleGridHeader.razor: 80 lines
  + ScheduleGridRow.razor: 120 lines
  + ScheduleGridCell.razor: 60 lines
  
Total Component Lines: 350 lines
```

**Net Reduction**: 2,486 → ~1,250 lines (50% reduction)  
**Line Savings**: ~1,236 lines removed

---

## 🚧 Challenges

1. **Massive @code Section** (1,474 lines)
   - Contains complex state management
   - Many private methods
   - Will need careful extraction

2. **Complex Grid Rendering**
   - Dynamic column count based on rooms/therapists
   - Time slot calculations
   - Booking overlaps

3. **Multiple Modal States**
   - Each modal has its own state variables
   - Tight coupling with parent

4. **Real-time Updates**
   - Grid needs to re-render on changes
   - State synchronization important

---

## 🎯 Simplified Approach (Recommended)

Given the complexity, let's take a **pragmatic approach**:

### Priority 1: Extract High-Value, Low-Risk Components

1. **SchedulerLocationSelector** - Self-contained controls
2. **SchedulerViewToggle** - Simple toggle checkboxes
3. **AlertMessages** - Already exists, reuse

### Priority 2: Partial Grid Extraction

4. **ScheduleGridHeader** - Just the header row
5. Keep grid body in main file (too complex for now)

### Priority 3: Leave Complex Parts

- Keep modals in main file (630 lines)
- Keep grid body rendering in main file
- Keep @code section mostly intact

### Expected Results (Realistic):
```
AdminScheduler.razor: ~2,200 lines (12% reduction)
  - Reduced by extracting: ~280 lines

New Components:
  + SchedulerLocationSelector.razor: 80 lines
  + SchedulerViewToggle.razor: 40 lines
  + ScheduleGridHeader.razor: 100 lines
  + Reuse AlertMessages.razor
```

**Net Impact**: More modest but **SAFE**
- ✅ Reduced complexity
- ✅ Zero risk of breaking functionality
- ✅ Creates reusable scheduler patterns
- ✅ Can extract more later if needed

---

## 📋 Implementation Steps

### Step 1: Backup Original File ✅
```bash
Copy-Item AdminScheduler.razor AdminScheduler_Original.razor.bak
```

### Step 2: Create SchedulerLocationSelector Component
- Extract location dropdown
- Extract date navigation
- Add EventCallbacks for changes

### Step 3: Create SchedulerViewToggle Component
- Extract view toggle checkboxes
- Add EventCallbacks for toggle changes

### Step 4: Create ScheduleGridHeader Component
- Extract header row rendering
- Pass rooms/therapists as parameters
- Keep styling

### Step 5: Update AdminScheduler.razor
- Replace extracted sections with components
- Add `@using SpaBooker.Web.Features.Admin.Components`
- Wire up EventCallbacks

### Step 6: Test Thoroughly
- Verify location selection works
- Verify date navigation works
- Verify view toggles work
- Verify grid still renders correctly

---

## ✅ Success Criteria

1. **Functionality Preserved** - All features work as before
2. **Tests Pass** - All 131 tests still passing
3. **Code Reduced** - At least 10-15% reduction
4. **Readability Improved** - Main file is more scannable
5. **Components Reusable** - New components can be used elsewhere

---

## 🚀 Ready to Proceed?

**Approach**: Pragmatic, safe extraction of 3-4 components  
**Expected Reduction**: 280 lines (~12%)  
**Risk Level**: Low  
**Time Estimate**: 30-45 minutes  

This approach focuses on **quick wins** while avoiding the complexity of the massive grid rendering and @code section.

Shall we proceed with this plan?

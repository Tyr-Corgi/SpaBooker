# ServiceManagement Refactoring Plan

## 📊 Current State

**File**: `ServiceManagement.razor`  
**Size**: 825 lines (34.5 KB)  
**Structure**:
- Header & Loading (30 lines)
- Location Filter (45 lines)
- Category Filter (20 lines)
- Service Cards Grid (~90 lines per card logic)
- Edit Modal with Tabs (~400 lines)
- @code Section (~135 lines)
- Styles (55 lines)

**Major Sections**:
1. Header & Loading States (30 lines)
2. Location Filter (45 lines)
3. Category Filter (20 lines)
4. Service Cards (85 lines in template)
5. Edit Modal (400+ lines)
   - Details Tab
   - Durations Tab
   - Therapists Tab
   - Rooms Tab
6. @code Section (135 lines)
7. CSS Styles (55 lines)

---

## 🎯 Refactoring Strategy

### Components to Extract

#### **1. ServiceLocationFilter** (50 lines)
- Location checkboxes
- Clear filters button
- Filter summary text
- Reusable for other service pages

#### **2. ServiceCategoryFilter** (30 lines)
- Category checkboxes
- Simple filter UI
- Reusable pattern

#### **3. ServiceCard** (100 lines)
- Service tile display
- Image, name, category badge
- Durations, therapists, rooms lists
- Edit button
- Highly reusable

#### **4. Keep Complex Modal** (400+ lines)
- Too tightly coupled with parent state
- Multiple tabs with complex logic
- Would require significant refactoring
- **Decision**: Leave in main file for now

---

## 📉 Expected Results

### Before:
```
ServiceManagement.razor: 825 lines
```

### After:
```
ServiceManagement.razor: ~665 lines (19% reduction)
  - Header: 30 lines
  - Filters (using components): 10 lines
  - Service grid (using component): 20 lines
  - Edit Modal (kept): 400 lines
  - @code section: 135 lines
  - Styles: 55 lines

New Components:
  + ServiceLocationFilter.razor: 55 lines
  + ServiceCategoryFilter.razor: 35 lines
  + ServiceCard.razor: 110 lines
```

**Net Reduction**: 825 → ~665 lines (19% reduction)  
**Line Savings**: ~160 lines reduced

---

## ✅ Benefits

### Code Organization
- Filters are self-contained and reusable
- Service card display logic is isolated
- Main file focuses on data loading and editing

### Reusability
- `ServiceLocationFilter` can be used elsewhere
- `ServiceCategoryFilter` is a common pattern
- `ServiceCard` can display any service group

### Maintainability
- Changes to card UI only affect one component
- Filter logic is isolated
- EventCallback pattern is consistent

---

## 📋 Implementation Steps

### Step 1: Backup Original File
```bash
Copy-Item ServiceManagement.razor ServiceManagement_Original.razor.bak
```

### Step 2: Create ServiceLocationFilter Component
- Extract location checkbox logic
- Add EventCallback for filter changes
- Include clear filters button

### Step 3: Create ServiceCategoryFilter Component
- Extract category checkbox logic
- Add EventCallback for category changes

### Step 4: Create ServiceCard Component
- Extract service card rendering
- Pass service group as parameter
- Add EventCallback for edit action

### Step 5: Update ServiceManagement.razor
- Replace filters with components
- Replace card loop with ServiceCard component
- Wire up EventCallbacks
- Keep modal as-is

### Step 6: Test
- Verify filters work
- Verify service cards display correctly
- Verify edit modal still works
- Run full test suite

---

## 🚧 What We're NOT Extracting

### Edit Modal (400+ lines) - Keeping Intact
**Reason**: Too complex and tightly coupled
- 4 tabs with different content
- Complex state management
- Form validation logic
- Would require significant refactoring effort

---

## ✅ Success Criteria

1. **Functionality Preserved** - All features work as before
2. **Tests Pass** - All tests still passing
3. **Code Reduced** - 15-20% reduction
4. **Readability Improved** - Main file is more focused
5. **Components Reusable** - New components can be used elsewhere

---

**Approach**: Pragmatic extraction of high-value, low-risk components  
**Expected Reduction**: ~160 lines (19%)  
**Risk Level**: Low  
**Time Estimate**: 20-30 minutes

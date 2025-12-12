# Coding Standards & Best Practices

## 📏 File Size Guidelines

### ✅ Target Limits

| File Type | Ideal Size | Maximum Size | Action Required |
|-----------|-----------|--------------|-----------------|
| **Razor Components** | <500 lines | 800 lines | Extract components if exceeded |
| **C# Services** | <200 lines | 300 lines | Split into multiple services |
| **Controllers** | <150 lines | 200 lines | Create separate controllers |
| **ViewModels/DTOs** | <100 lines | 150 lines | Split into focused models |

### 🎯 Single Responsibility Principle (SRP)

**Rule**: Each file should have ONE clear responsibility.

**Examples:**
- ✅ `BookingService.cs` - Handles booking operations only
- ✅ `ClientCard.razor` - Displays client information only
- ❌ `AdminScheduler.razor` - Does grid + 5 modals + business logic (too much)

### 📊 Method Complexity Guidelines

- **Maximum methods per class**: 10-15
- **Maximum lines per method**: 50 (ideally 20-30)
- **Maximum parameters**: 5
- **Maximum nesting depth**: 3 levels

---

## 🚫 Warning Signs (When to Refactor)

If you see any of these, it's time to break things up:

### 1. **Scroll Hell**
- Can't see the entire file without scrolling for 30+ seconds
- Need to use "Go to Definition" constantly

### 2. **Method Count**
- More than 15 methods in one file
- Methods are hard to find

### 3. **Too Many Responsibilities**
- File does data loading + UI + business logic + validation
- Name is vague (e.g., "Manager", "Handler", "Utility")

### 4. **Hard to Test**
- Need to mock 5+ dependencies
- Setup takes 50+ lines
- Can only test via integration tests

### 5. **Tight Coupling**
- Changing one method breaks multiple others
- Can't extract logic without dragging 20 dependencies

---

## ✅ Best Practices for Razor Components

### 1. **Extract Reusable UI Components**

**Bad:**
```razor
@* MyPage.razor - 800 lines *@
<div class="filters">
    <!-- 100 lines of filter UI -->
</div>

<div class="card-grid">
    @foreach (var item in items)
    {
        <!-- 80 lines of card markup -->
    }
</div>
```

**Good:**
```razor
@* MyPage.razor - 100 lines *@
<FilterPanel Filters="@filters" OnFilterChanged="HandleFilterChange" />

<div class="card-grid">
    @foreach (var item in items)
    {
        <ItemCard Item="@item" OnEdit="HandleEdit" />
    }
</div>

@* Components/FilterPanel.razor - 80 lines *@
@* Components/ItemCard.razor - 60 lines *@
```

### 2. **Keep Modals Separate (When Possible)**

**Bad:**
```razor
@* MyPage.razor - 1,000 lines *@
<button @onclick="ShowEditModal">Edit</button>

@if (showEditModal)
{
    <!-- 200 lines of modal markup -->
}

@code {
    // 300 lines of modal logic
}
```

**Good:**
```razor
@* MyPage.razor - 200 lines *@
<button @onclick="() => editModal.Show(selectedItem)">Edit</button>
<EditModal @ref="editModal" OnSave="HandleSave" />

@* Components/EditModal.razor - 150 lines *@
```

### 3. **Extract Business Logic to Services**

**Bad:**
```razor
@code {
    private async Task CalculateAvailability()
    {
        // 100 lines of complex calculation logic
    }
}
```

**Good:**
```razor
@inject AvailabilityService AvailabilityService

@code {
    private async Task CalculateAvailability()
    {
        var result = await AvailabilityService.Calculate(locationId, date);
    }
}
```

---

## 📁 Recommended File Structure

### For Pages:
```
Features/
  Admin/
    AdminScheduler.razor               (200-500 lines MAX)
    Components/
      SchedulerGrid.razor              (<300 lines)
      AssignmentModal.razor            (<200 lines)
      BookingDetailsModal.razor        (<200 lines)
    Services/
      SchedulerService.cs              (<200 lines)
      AvailabilityService.cs           (<200 lines)
```

### For Services:
```
Services/
  Bookings/
    BookingService.cs                  (<200 lines)
    BookingValidationService.cs        (<150 lines)
    BookingNotificationService.cs      (<150 lines)
```

---

## 🛠️ Refactoring Checklist

Before committing a file, ask yourself:

- [ ] Is the file under 500 lines? (800 max for complex pages)
- [ ] Does the file have ONE clear responsibility?
- [ ] Can I describe what it does in one sentence?
- [ ] Are there obvious sections that could be components?
- [ ] Would a new team member understand it in <10 minutes?
- [ ] Can I see the full `@code` section without scrolling?

If you answered **NO** to 3+ questions, refactor before committing.

---

## 🎯 Quick Refactoring Strategies

### Strategy 1: Extract Repeated UI Blocks
```razor
@* Before: Repeated 5 times *@
<div class="stat-card">
    <h3>@title</h3>
    <p>@value</p>
</div>

@* After: Component *@
<StatCard Title="@title" Value="@value" />
```

### Strategy 2: Extract Modals
```razor
@* Before: 300 lines in parent *@
@if (showModal) { <!-- modal markup --> }

@* After: Separate component *@
<MyModal @bind-IsOpen="showModal" OnSave="HandleSave" />
```

### Strategy 3: Extract Business Logic
```razor
@* Before: 150 lines of calculations in @code *@
private decimal CalculateTotal() { /* complex logic */ }

@* After: Service *@
@inject CalculationService Calculator
var total = Calculator.CalculateTotal(items);
```

### Strategy 4: Extract Data Loading
```razor
@* Before: Multiple OnInitializedAsync methods *@
protected override async Task OnInitializedAsync()
{
    await LoadUsers();
    await LoadRoles();
    await LoadPermissions();
    // 200 lines of loading logic
}

@* After: Service *@
@inject DataService DataService
protected override async Task OnInitializedAsync()
{
    var data = await DataService.LoadDashboardData();
}
```

---

## 🚨 Exceptions to the Rule

Some files are **allowed** to be larger if they meet ALL criteria:

### ✅ Acceptable Large Files:
1. **Complex, specialized UI** (like AdminScheduler)
   - Must be unique (not reused elsewhere)
   - Must have clear sections with comments
   - Must have comprehensive tests

2. **DTOs with many properties** (like configuration classes)
   - Just data, no logic
   - Auto-generated is fine

3. **Database migrations**
   - Generated by tools

### ❌ Never Acceptable:
- "God classes" that do everything
- Files with 1,000+ lines that could be split
- Files that mix UI + business logic + data access

---

## 📝 Current Project Status

### ✅ Refactored (Good Examples):
- `StaffScheduling.razor` - 400 lines ✅
- `ServiceManagement.razor` - 701 lines ✅
- `BookingService.cs` - Focused, testable ✅

### ⚠️ Technical Debt (Accepted for Now):
- `AdminScheduler.razor` - 2,459 lines ⚠️
  - **Reason**: Complex scheduler, high refactor risk
  - **Status**: Accepted as-is, will revisit if problems emerge

### 🎯 Future Goal:
- Keep all NEW files under 500 lines
- Extract components proactively
- Use services for business logic

---

## 💡 Development Workflow

### When Creating New Features:

1. **Plan Component Structure First**
   ```
   New Feature: User Dashboard
   ├── UserDashboard.razor (main page, <300 lines)
   ├── Components/
   │   ├── StatsWidget.razor (<100 lines)
   │   ├── ActivityFeed.razor (<150 lines)
   │   └── QuickActions.razor (<80 lines)
   └── Services/
       └── DashboardService.cs (<200 lines)
   ```

2. **Create Components Early**
   - Don't wait until file is 800 lines
   - Extract at 300-400 lines

3. **Review Before Committing**
   - Check file size
   - Check method count
   - Check responsibilities

---

## 🏆 Summary

### **Golden Rules:**
1. ✅ Files under 500 lines (800 max)
2. ✅ One responsibility per file
3. ✅ Extract components early
4. ✅ Use services for business logic
5. ✅ Keep modals separate when possible

### **Lesson Learned:**
- AdminScheduler grew to 2,459 lines
- Hard to maintain, but works
- **Don't repeat this pattern**
- Extract as you go, don't wait

### **For This Project:**
- Existing large files are **accepted** (working, tested)
- Future files must follow **new standards**
- Refactor existing files **only if needed**

---

**Remember**: Code is read 10x more than it's written. Make it easy to read!

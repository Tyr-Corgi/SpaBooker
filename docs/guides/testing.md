# Testing Guidelines

## Core Principle

**Every production code change MUST include corresponding tests.**

This is enforced by:
- Git pre-commit hooks
- CI/CD pipeline checks
- Pull request reviews

---

## When to Write Tests

### Always Required

| Code Type | Test Type Required | Location |
|-----------|-------------------|----------|
| New Entity | Unit Tests | `tests/SpaBooker.Tests.Unit/Entities/` |
| New Service | Integration Tests | `tests/SpaBooker.Tests.Integration/Services/` |
| Modified Entity | Update Unit Tests | Existing test file |
| Modified Service | Update Integration Tests | Existing test file |
| Business Logic | Unit Tests | Appropriate test file |
| API Endpoints | Functional Tests | `tests/SpaBooker.Tests.Functional/` |

### Optional (but Recommended)
- DTOs and simple data structures
- Configuration classes
- Migration files
- Simple extension methods

---

## Test Types

### 1. Unit Tests
**Purpose:** Test individual components in isolation

**Use for:**
- Entity validation logic
- Business rule enforcement
- Domain model behavior
- Pure functions and calculations

```csharp
[Fact]
public void Booking_Should_Calculate_Total_Price_Correctly()
{
    // Arrange
    var booking = new Booking
    {
        ServicePrice = 100.00m,
        DiscountApplied = 10.00m
    };

    // Act
    booking.CalculateTotalPrice();

    // Assert
    booking.TotalPrice.Should().Be(90.00m);
}
```

### 2. Integration Tests
**Purpose:** Test component interactions with real dependencies

**Use for:**
- Service layer operations
- Database interactions (with InMemory DB)
- Multiple component integration
- Data access logic

```csharp
[Fact]
public async Task BookingService_Should_Create_Booking_Successfully()
{
    // Arrange
    var service = new BookingService(_context);
    var booking = new Booking { /* ... */ };

    // Act
    var result = await service.CreateBookingAsync(booking);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().BeGreaterThan(0);
}
```

### 3. Functional Tests
**Purpose:** Test complete user workflows end-to-end

**Use for:**
- API endpoint testing
- Complete user scenarios
- Authentication/Authorization
- Request/Response validation

---

## Writing Good Tests

### Naming Convention
```
{ComponentName}_{Should}_{ExpectedBehavior}_[Scenario]
```

Examples:
- `Booking_Should_Throw_When_EndTime_Before_StartTime`
- `GiftCertificate_Should_Reduce_Balance_After_Redemption`
- `MembershipService_Should_Add_Credits_Monthly`

### Test Structure (AAA Pattern)
```csharp
[Fact]
public void Test_Name()
{
    // Arrange - Set up test data and dependencies
    var entity = new Entity { Property = "value" };
    
    // Act - Execute the behavior being tested
    var result = entity.PerformAction();
    
    // Assert - Verify the expected outcome
    result.Should().Be(expectedValue);
}
```

### Best Practices

**DO:**
- Write one test per behavior
- Use descriptive test names
- Follow the AAA pattern
- Test edge cases and error conditions
- Use FluentAssertions for readability
- Clean up resources (IDisposable)
- Keep tests independent and isolated

**DON'T:**
- Test multiple behaviors in one test
- Rely on test execution order
- Use hard-coded values without context
- Test framework code (EF Core, ASP.NET, etc.)
- Write tests that depend on external services
- Commit failing tests

---

## Development Workflow

### 1. Before Starting Work
```bash
# Ensure hooks are installed
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit

# Run existing tests to ensure clean baseline
dotnet test
```

### 2. During Development (TDD Recommended)
```bash
# 1. Write a failing test first
# 2. Write minimal code to make it pass
# 3. Refactor
# 4. Repeat

# Run tests frequently
dotnet test --filter "FullyQualifiedName~YourTestClass"
```

### 3. Before Committing
```bash
# Run all tests
dotnet test

# Check coverage (if installed)
dotnet test --collect:"XPlat Code Coverage"

# Tests will automatically run during git commit
git add .
git commit -m "feat: Add new feature with tests"
```

---

## Test Coverage Requirements

### Minimum Thresholds
- **Overall Coverage:** 70%
- **New Code:** 80%
- **Critical Services:** 90% (Payment, Booking, Security)

### Coverage Tools
```bash
# Install coverage tool
dotnet tool install --global dotnet-coverage

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
```

---

## Test Templates

### Entity Test Template
```csharp
using FluentAssertions;
using SpaBooker.Core.Entities;

namespace SpaBooker.Tests.Unit.Entities;

public class YourEntityTests
{
    [Fact]
    public void YourEntity_Should_Create_With_Valid_Properties()
    {
        // Arrange & Act
        var entity = new YourEntity
        {
            Property1 = "value1",
            Property2 = 42
        };

        // Assert
        entity.Property1.Should().Be("value1");
        entity.Property2.Should().Be(42);
    }

    [Theory]
    [InlineData("value1", true)]
    [InlineData("value2", false)]
    public void YourEntity_Should_Validate_Property(string input, bool expected)
    {
        // Arrange
        var entity = new YourEntity { Property = input };

        // Act
        var result = entity.IsValid();

        // Assert
        result.Should().Be(expected);
    }
}
```

### Service Test Template
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;

namespace SpaBooker.Tests.Integration.Services;

public class YourServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly YourService _service;

    public YourServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new YourService(_context);
    }

    [Fact]
    public async Task YourService_Should_Perform_Operation()
    {
        // Arrange
        await _context.YourEntity.AddAsync(new YourEntity());
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.PerformOperationAsync();

        // Assert
        result.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

## Common Issues

**Q: "My tests are failing in CI but passing locally"**
- Ensure you're not relying on local state or files
- Check for timezone issues (always use UTC)
- Verify database seeds are consistent

**Q: "I have a small bug fix, do I really need tests?"**
- **Yes.** Even small changes need tests
- Bug fixes should include regression tests
- This prevents the bug from returning

**Q: "How do I test private methods?"**
- Don't test private methods directly
- Test them indirectly through public methods
- If you need to test them directly, consider refactoring

**Q: "Tests are too slow"**
- Use `[Trait]` to categorize tests (Fast, Slow, Integration)
- Run fast tests during development
- Run all tests before committing

---

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)


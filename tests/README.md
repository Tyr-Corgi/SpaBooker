# SpaBooker Test Suite

This directory contains the test suite for the SpaBooker application, organized into Unit Tests and Integration Tests.

## Test Organization

### Unit Tests (`SpaBooker.Tests.Unit`)
Tests for individual entities and domain logic in isolation, with no external dependencies.

**Location:** `tests/SpaBooker.Tests.Unit/`

**Covered Entities:**
- `Booking` - Booking entity tests
- `GiftCertificate` - Gift certificate entity tests
- `UserMembership` - User membership entity tests

**Total Unit Tests:** 37 ✅

### Integration Tests (`SpaBooker.Tests.Integration`)
Tests for service layer logic with real database interactions using EF Core InMemory database.

**Location:** `tests/SpaBooker.Tests.Integration/`

**Covered Services:**
- `MembershipCreditService` - Membership credit operations
- `GiftCertificateService` - Gift certificate operations

**Total Integration Tests:** 14 ✅

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test --filter "FullyQualifiedName~Tests.Unit"
```

### Run Integration Tests Only
```bash
dotnet test --filter "FullyQualifiedName~Tests.Integration"
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~BookingTests"
```

### Run with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Dependencies

- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library for more readable tests
- **Moq** - Mocking framework for dependencies
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for integration tests

## Test Patterns

### Unit Tests
Unit tests follow the **AAA (Arrange-Act-Assert)** pattern:

```csharp
[Fact]
public void Entity_Should_DoSomething()
{
    // Arrange - Set up test data
    var entity = new Entity { Property = "value" };
    
    // Act - Perform the action
    entity.DoSomething();
    
    // Assert - Verify the result
    entity.Property.Should().Be("expected");
}
```

### Integration Tests
Integration tests use **IDisposable** for database cleanup:

```csharp
public class ServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Service _service;
    
    public ServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _service = new Service(_context);
    }
    
    [Fact]
    public async Task Service_Should_PerformOperation()
    {
        // Arrange
        await _context.SomeEntity.AddAsync(new SomeEntity());
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

## Test Coverage

### Entity Tests (Unit)
- ✅ `Booking` - 10 tests covering status transitions, duration, notes, credits, and payment tracking
- ✅ `GiftCertificate` - 13 tests covering creation, balance management, status changes, and location restrictions
- ✅ `UserMembership` - 14 tests covering status changes, credit tracking, billing dates, and Stripe integration

### Service Tests (Integration)
- ✅ `MembershipCreditService` - 5 tests covering credit addition, deduction, availability checking, and transaction logging
- ✅ `GiftCertificateService` - 9 tests covering certificate creation, validation, redemption, and code generation

## Best Practices

1. **Test Naming:** Use descriptive names that explain what is being tested
   - Format: `{Component}_{Should}_{ExpectedBehavior}`
   - Example: `Booking_Should_Track_Membership_Credits_Usage`

2. **One Assertion Per Test:** Focus each test on a single behavior or scenario

3. **Isolation:** Each test should be independent and not rely on the state from other tests

4. **Clean Up:** Always dispose of resources properly in integration tests

5. **Readable Assertions:** Use FluentAssertions for more expressive test assertions

## Adding New Tests

### Adding Unit Tests
1. Create a new file in `tests/SpaBooker.Tests.Unit/Entities/`
2. Name it `{EntityName}Tests.cs`
3. Add test methods following the AAA pattern
4. Use `[Fact]` for single tests or `[Theory]` with `[InlineData]` for parameterized tests

### Adding Integration Tests
1. Create a new file in `tests/SpaBooker.Tests.Integration/Services/`
2. Name it `{ServiceName}Tests.cs`
3. Implement `IDisposable` for cleanup
4. Set up InMemory database in constructor
5. Add async test methods with database interactions

## Current Status

**Total Tests:** 51
- ✅ **Passing:** 51
- ❌ **Failing:** 0

**Build Status:** ✅ Success (with warnings)

All tests are passing and the test suite is ready for continuous integration.


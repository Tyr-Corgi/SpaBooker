# Testing Enforcement System

## Overview
This document explains the **hard enforcement** mechanisms that ensure all production code changes include corresponding tests.

## 🛡️ Enforcement Layers

### Layer 1: Git Pre-Commit Hook ⚡
**Location:** `.githooks/pre-commit`

**What it does:**
- Runs automatically before every commit
- Detects if production C# files were changed
- Checks if corresponding test files were also modified
- Runs the full test suite to ensure all tests pass
- **Blocks the commit** if tests are missing or failing

**How to bypass (NOT RECOMMENDED):**
```bash
git commit --no-verify
```

**Setup:**
```bash
# Automatic (if you ran setup-dev.sh or setup-dev.bat)
# Or manual:
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit  # Unix/Mac/Linux only
```

---

### Layer 2: GitHub Actions CI/CD Pipeline 🚀
**Location:** `.github/workflows/test.yml`

**What it does:**
- Runs on every push and pull request
- Executes all unit and integration tests
- Calculates code coverage
- Checks for test files corresponding to changed production files
- Posts coverage reports as PR comments
- **Blocks merge** if any checks fail

**Triggers:**
- Push to `main` or `develop` branches
- Any pull request to `main` or `develop`

**Jobs:**
1. **test** - Runs all tests with coverage reporting
2. **test-coverage-check** - Verifies test files exist for changed code
3. **status-check** - Final gate that must pass before merge

**Requirements:**
- All tests must pass
- Minimum 70% overall coverage
- Test files must exist for all changed entities/services

---

### Layer 3: Pull Request Template 📋
**Location:** `.github/pull_request_template.md`

**What it does:**
- Provides a checklist that PR authors must complete
- Forces explicit confirmation of test coverage
- Requires listing of test files added/modified
- Guides code reviewers on what to check

**Key sections:**
- Test Coverage Checklist (must be checked)
- Test Files (must be listed)
- Test Scenarios Covered
- Reviewer Checklist

---

### Layer 4: Code Review Process 👥
**What reviewers must verify:**
- [ ] Tests exist for all production code changes
- [ ] Tests adequately cover the functionality
- [ ] Tests follow best practices (AAA pattern, descriptive names)
- [ ] Edge cases and error handling are tested
- [ ] All CI/CD checks pass

---

## 📁 File Structure

```
SpaBooker/
├── .github/
│   ├── workflows/
│   │   └── test.yml                    # CI/CD pipeline
│   └── pull_request_template.md        # PR template with test checklist
├── .githooks/
│   └── pre-commit                      # Pre-commit hook script
├── tests/
│   ├── SpaBooker.Tests.Unit/
│   │   ├── Entities/                   # Entity unit tests
│   │   └── _TestTemplate.cs            # Template for new unit tests
│   ├── SpaBooker.Tests.Integration/
│   │   ├── Services/                   # Service integration tests
│   │   └── _TestTemplate.cs            # Template for new integration tests
│   └── README.md                       # Test suite documentation
├── TESTING_GUIDELINES.md               # Comprehensive testing guide
├── TESTING_ENFORCEMENT.md              # This file
├── setup-dev.sh                        # Setup script (Unix/Mac/Linux)
└── setup-dev.bat                       # Setup script (Windows)
```

---

## 🚀 Getting Started

### For New Developers

1. **Run the setup script:**
   ```bash
   # Unix/Mac/Linux
   ./setup-dev.sh
   
   # Windows
   setup-dev.bat
   ```

2. **Read the testing guidelines:**
   ```bash
   # Open in your editor
   code TESTING_GUIDELINES.md
   ```

3. **Understand the enforcement:**
   - Read this document
   - Try making a commit without tests (it will be blocked)
   - Look at existing tests as examples

### For Existing Developers

If you cloned the repo before these mechanisms were added:

1. **Update your local hooks:**
   ```bash
   git config core.hooksPath .githooks
   chmod +x .githooks/pre-commit  # Unix/Mac/Linux only
   ```

2. **Verify setup:**
   ```bash
   # This should run tests
   git commit --allow-empty -m "Test commit"
   ```

---

## 📊 Test Coverage Requirements

### Minimum Thresholds
| Category | Coverage Required |
|----------|------------------|
| Overall | 70% |
| New Code | 80% |
| Critical Services | 90% |

### What Requires Tests
| Code Type | Test Required | Test Location |
|-----------|--------------|---------------|
| **Entity** | ✅ Unit Test | `tests/SpaBooker.Tests.Unit/Entities/{EntityName}Tests.cs` |
| **Service** | ✅ Integration Test | `tests/SpaBooker.Tests.Integration/Services/{ServiceName}Tests.cs` |
| **Controller/Endpoint** | ✅ Functional Test | `tests/SpaBooker.Tests.Functional/` (future) |
| **Business Logic** | ✅ Unit Test | Appropriate test file |
| **DTO/Model** | ⚠️ Optional | Usually not needed |
| **Configuration** | ⚠️ Optional | Usually not needed |
| **Migration** | ❌ Not Required | Auto-generated |

---

## 🔄 Development Workflow

### Standard Workflow with Tests

```bash
# 1. Create a new branch
git checkout -b feature/new-booking-feature

# 2. Write a failing test (TDD recommended)
# Edit: tests/SpaBooker.Tests.Unit/Entities/BookingTests.cs

# 3. Run the test to see it fail
dotnet test --filter "BookingTests"

# 4. Implement the feature
# Edit: src/SpaBooker.Core/Entities/Booking.cs

# 5. Run tests until they pass
dotnet test --filter "BookingTests"

# 6. Run all tests to ensure nothing broke
dotnet test

# 7. Commit (pre-commit hook will run)
git add .
git commit -m "feat: Add new booking validation"

# Pre-commit hook will:
#   - Detect changed files
#   - Verify test files were modified
#   - Run all tests
#   - Allow or block commit

# 8. Push (CI/CD will run)
git push origin feature/new-booking-feature

# CI/CD pipeline will:
#   - Build the solution
#   - Run all tests
#   - Calculate coverage
#   - Post results to PR

# 9. Create Pull Request
# - PR template auto-fills
# - Complete the test checklist
# - List test files modified
# - Request review

# 10. Code Review
# - Reviewer checks tests
# - CI/CD must pass
# - Merge when approved
```

---

## 🚨 Common Scenarios

### Scenario 1: Small Bug Fix
**Question:** "Do I need tests for a tiny bug fix?"

**Answer:** **YES!**
- Add a regression test to prevent the bug from returning
- Even small changes need tests

**Example:**
```csharp
[Fact]
public void Booking_Should_Not_Allow_End_Before_Start()
{
    // Regression test for bug #123
    var booking = new Booking
    {
        StartTime = DateTime.UtcNow,
        EndTime = DateTime.UtcNow.AddHours(-1) // Bug: allowed in the past
    };
    
    // Act & Assert
    booking.Validate().Should().BeFalse();
}
```

### Scenario 2: Emergency Hotfix
**Question:** "Production is down, can I skip tests?"

**Answer:** **NO, but...**
- Use `--no-verify` for emergency commits (document why)
- Add tests in a follow-up PR within 24 hours
- Hotfix branch should still pass CI/CD before merge

### Scenario 3: Refactoring Existing Code
**Question:** "I'm just moving code around, do I need new tests?"

**Answer:** **Update existing tests**
- Ensure existing tests still pass
- Update test names/structure if needed
- Add tests if you discover untested scenarios

### Scenario 4: UI-Only Changes
**Question:** "I only changed Razor views, do I need tests?"

**Answer:** **Depends**
- Pure UI (styling, layout): No tests required
- UI logic (validation, calculations): Yes, test the logic
- Components with behavior: Yes, add component tests (future)

---

## 🛠️ Troubleshooting

### Problem: Pre-commit hook not running
**Solution:**
```bash
# Verify hooks path is set
git config core.hooksPath

# Should output: .githooks

# If not, set it:
git config core.hooksPath .githooks

# Make hook executable (Unix/Mac/Linux)
chmod +x .githooks/pre-commit
```

### Problem: Hook runs but doesn't check for tests
**Solution:**
- Verify you're on a Unix-like system or WSL (Windows Subsystem for Linux)
- Windows native Git may not execute bash scripts
- Use Windows Git Bash or WSL

### Problem: CI/CD failing but tests pass locally
**Solutions:**
1. **Timezone issues:**
   ```csharp
   // Always use UTC in tests
   var now = DateTime.UtcNow;
   ```

2. **Database state:**
   ```csharp
   // Always use unique DB names in integration tests
   .UseInMemoryDatabase(Guid.NewGuid().ToString())
   ```

3. **File paths:**
   ```csharp
   // Use Path.Combine for cross-platform compatibility
   var path = Path.Combine("dir", "file.txt");
   ```

### Problem: Coverage tool not found
**Solution:**
```bash
# Install coverage tools
dotnet tool install --global dotnet-coverage
dotnet tool install --global dotnet-reportgenerator-globaltool

# Verify installation
dotnet tool list --global
```

### Problem: Tests are too slow
**Solutions:**
1. **Use test categories:**
   ```csharp
   [Fact]
   [Trait("Category", "Fast")]
   public void Fast_Test() { }
   
   [Fact]
   [Trait("Category", "Slow")]
   public void Slow_Integration_Test() { }
   ```

2. **Run only fast tests during development:**
   ```bash
   dotnet test --filter "Category=Fast"
   ```

3. **Run all tests before commit:**
   ```bash
   dotnet test
   ```

---

## 📈 Monitoring and Metrics

### Key Metrics to Track
- **Test Coverage:** Target >70%, Critical Services >90%
- **Test Execution Time:** Monitor and optimize slow tests
- **Test Count Growth:** Should grow with codebase
- **Test Failure Rate:** Should be <5% on main branch
- **PR Test Addition Rate:** % of PRs with new tests

### Coverage Reports
- Automatically generated by CI/CD
- Posted as PR comments
- Can be viewed locally:
  ```bash
  dotnet test --collect:"XPlat Code Coverage"
  reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
  # Open coveragereport/index.html
  ```

---

## 🎓 Learning Resources

### Internal Resources
- [`TESTING_GUIDELINES.md`](TESTING_GUIDELINES.md) - Comprehensive testing guide
- [`tests/README.md`](tests/README.md) - Test suite overview
- [`tests/**/_TestTemplate.cs`](tests/) - Test templates to copy

### External Resources
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Microsoft Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/)

---

## 🤝 Contributing

### Improving the Testing System
If you have suggestions for improving the testing enforcement:

1. Open an issue describing the improvement
2. Discuss with the team
3. Submit a PR with changes to:
   - Pre-commit hook (`.githooks/pre-commit`)
   - CI/CD pipeline (`.github/workflows/test.yml`)
   - Documentation (this file, `TESTING_GUIDELINES.md`)

### Adding New Test Types
When adding new test categories (e.g., Functional, E2E):

1. Create new test project:
   ```bash
   dotnet new xunit -n SpaBooker.Tests.{Type}
   cd SpaBooker.Tests.{Type}
   dotnet add package FluentAssertions
   dotnet add reference ../../src/SpaBooker.Core
   ```

2. Update CI/CD to run new tests
3. Update documentation
4. Create template file

---

## 📋 Summary

### The Hard Rules
1. ✅ **All production code changes MUST include tests**
2. ✅ **Pre-commit hook MUST pass before commit**
3. ✅ **CI/CD pipeline MUST pass before merge**
4. ✅ **PR checklist MUST be completed**
5. ✅ **Minimum coverage thresholds MUST be met**

### Enforcement Points
| Stage | Check | Blocker | Bypass |
|-------|-------|---------|--------|
| **Commit** | Pre-commit Hook | ✅ Yes | `--no-verify` (tracked) |
| **Push** | N/A | ❌ No | N/A |
| **PR** | CI/CD Pipeline | ✅ Yes | Cannot bypass |
| **Merge** | PR Approval + CI | ✅ Yes | Requires admin override |

### Remember
> **"Code without tests is broken by design."**

Testing isn't optional—it's part of writing production code. These enforcement mechanisms ensure quality, reliability, and maintainability of the SpaBooker application.

---

**Questions?** Open an issue or ask in your team chat.


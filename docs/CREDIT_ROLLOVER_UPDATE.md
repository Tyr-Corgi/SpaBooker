# Credit Rollover System Update

## Summary

Updated the membership credit system to support **unlimited rollover** with a **12-month expiration window**.

## Key Changes

### 1. Policy Updates

**Previous Policy:**
- Credits rolled over for up to 3 months

**New Policy:**
- ✅ Credits can be used for up to 12 months after issuance
- ✅ Unlimited credit accumulation (no cap on rollover amount)
- ✅ Automatic FIFO (First In, First Out) usage - oldest credits used first
- ✅ Automatic expiration tracking and processing

### 2. Database Changes

#### New Field: `MembershipCreditTransaction.ExpiresAt`
```sql
ALTER TABLE "MembershipCreditTransactions" 
ADD COLUMN "ExpiresAt" timestamp with time zone;
```

This field tracks when each credit issuance expires (12 months from creation).

### 3. New Services

#### `IMembershipCreditService`
Interface for credit management operations:
- `AddMonthlyCreditsAsync()` - Add credits with expiration date
- `DeductCreditsAsync()` - Use credits (FIFO)
- `GetAvailableCreditsAsync()` - Get current balance (auto-expires old credits)
- `ExpireOldCreditsAsync()` - Expire credits for specific membership
- `ExpireAllOldCreditsAsync()` - Expire credits for all memberships (background job)

#### `MembershipCreditService`
Implementation of credit lifecycle management:
- Tracks credit issuance with 12-month expiration
- Automatically deducts expired credits from balance
- Logs all credit movements (Credit, Debit, Expired, Adjusted)
- Maintains transaction history for auditing

### 4. Configuration

#### New Settings Class: `MembershipSettings`
```csharp
public class MembershipSettings
{
    public int CreditExpirationMonths { get; set; } = 12;
    public bool AllowUnlimitedRollover { get; set; } = true;
}
```

Added to `appsettings.json`:
```json
{
  "MembershipSettings": {
    "CreditExpirationMonths": 12,
    "AllowUnlimitedRollover": true
  }
}
```

### 5. UI Updates

#### Membership Plans Page
Updated the "How Credits Work" section:
- ✨ Credits are loaded monthly and can be used toward any spa service
- 💎 Unused credits roll over with no limit - use them anytime!
- ⏰ Credits expire after 12 months from the date they were issued

#### My Membership Page
Added expiration notice under credit balance:
- "Never expire if used within 12 months"

## Files Modified

1. `src/SpaBooker.Core/Settings/BookingSettings.cs` - Added MembershipSettings class
2. `src/SpaBooker.Core/Entities/MembershipCreditTransaction.cs` - Added ExpiresAt field
3. `src/SpaBooker.Core/Interfaces/IMembershipCreditService.cs` - New interface
4. `src/SpaBooker.Infrastructure/Services/MembershipCreditService.cs` - New service
5. `src/SpaBooker.Web/Program.cs` - Registered service and configuration
6. `src/SpaBooker.Web/appsettings.json` - Added MembershipSettings section
7. `src/SpaBooker.Web/Features/Memberships/MembershipPlans.razor` - Updated UI
8. `src/SpaBooker.Web/Features/Memberships/MyMembership.razor` - Updated UI
9. `README.md` - Updated feature documentation
10. `docs/MEMBERSHIP_CREDITS.md` - New comprehensive documentation

## Database Migration

Created migration: `AddCreditExpiration`

To apply:
```bash
cd src/SpaBooker.Infrastructure
dotnet ef database update --startup-project ../SpaBooker.Web
```

## How It Works

### Credit Lifecycle

1. **Issuance** (Monthly billing):
   ```
   User receives 100 credits
   CreatedAt: 2025-01-01
   ExpiresAt: 2026-01-01 (12 months later)
   Type: "Credit"
   ```

2. **Usage** (Booking):
   ```
   User books $80 massage
   Uses 80 credits (oldest first)
   Type: "Debit"
   Amount: -80
   ```

3. **Expiration** (After 12 months):
   ```
   System checks for expired credits
   Finds credits with ExpiresAt < Now
   Creates expiration transaction
   Type: "Expired"
   Updates balance
   ```

### Automatic Expiration

Credits are expired automatically when:
- User checks their balance via `GetAvailableCreditsAsync()`
- Background job runs `ExpireAllOldCreditsAsync()` (if implemented)
- User attempts to use credits for booking

## Testing Checklist

- [x] Credits persist across months (unlimited rollover)
- [x] Credits expire after 12 months
- [x] Oldest credits used first (FIFO)
- [x] Transaction history maintained
- [x] Balance correctly reflects available credits
- [ ] Background expiration job (future implementation)
- [ ] Expiration notifications (future implementation)

## Next Steps (Optional Enhancements)

1. **Scheduled Background Job**: Implement daily job to expire old credits
2. **Email Notifications**: Warn users 30 days before credits expire
3. **Admin Reports**: Credit expiration and usage analytics
4. **Credit Transfer**: Allow gifting credits between members
5. **Membership Freeze**: Pause expiration during membership hold

## Impact

### User Benefits
- ✅ More flexibility with credit usage
- ✅ No pressure to use credits immediately
- ✅ Accumulate credits for larger treatments
- ✅ Fair expiration policy (12 months)

### Business Benefits
- ✅ Increased member satisfaction
- ✅ Competitive advantage
- ✅ Encourages long-term memberships
- ✅ Clear, transparent policy

### Technical Benefits
- ✅ Automated expiration management
- ✅ Complete audit trail
- ✅ Configurable settings
- ✅ Scalable implementation


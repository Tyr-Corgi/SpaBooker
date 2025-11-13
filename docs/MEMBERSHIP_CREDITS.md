# Membership Credit System

## Overview

SpaBooker's membership system provides flexible monthly credits that can be used toward any spa service. Credits offer exceptional value through unlimited rollover with a 12-month expiration window.

## Credit Rules

### 1. Monthly Credit Allocation
- Credits are automatically added to member accounts on each billing cycle
- Amount depends on membership plan:
  - **Bronze**: 100 credits/month
  - **Silver**: 200 credits/month  
  - **Gold**: 300 credits/month

### 2. Credit Rollover Policy
- ✅ **Unlimited Rollover**: No maximum cap on credit accumulation
- ✅ **12-Month Expiration**: Credits expire 12 months after they are issued
- ✅ **FIFO Usage**: Oldest credits are automatically used first (First In, First Out)

### 3. Credit Usage
- Credits can be applied to any active spa service
- Members receive additional discounts on service prices:
  - Bronze: 10% discount
  - Silver: 15% discount
  - Gold: 20% discount
- Credits are deducted at booking time
- Unused credits after expiration are automatically removed

## Examples

### Example 1: Regular Usage
```
Month 1: +100 credits (expires Month 13)
Month 2: +100 credits (expires Month 14)
- Use 50 credits for massage
- Remaining: 150 credits

Month 3: +100 credits (expires Month 15)
- Use 80 credits for facial
- Remaining: 170 credits (50 from Month 1, 100 from Month 2, 20 from Month 3)
```

### Example 2: Accumulation
```
Busy months with no visits:
Month 1-6: +600 credits accumulated (100 x 6)
Month 7: Use 200 credits for spa day package
Remaining: 400 credits
- 100 credits from Month 1 (expires Month 13)
- 100 credits from Month 2 (expires Month 14)
- 100 credits from Month 3 (expires Month 15)
- 100 credits from Month 4 (expires Month 16)
```

### Example 3: Expiration
```
Month 1: +100 credits (expires Month 13)
Months 2-12: Member accumulates more credits
Month 13: 
- Original 100 credits expire if not used
- Member receives notification before expiration
- Newer credits remain active
```

## Technical Implementation

### Database Schema

#### MembershipCreditTransaction
Tracks all credit movements with lifecycle information:

```csharp
public class MembershipCreditTransaction
{
    public int Id { get; set; }
    public int UserMembershipId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } // "Credit", "Debit", "Expired", "Adjusted"
    public string Description { get; set; }
    public int? RelatedBookingId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; } // 12 months from CreatedAt for credits
}
```

### Credit Service Methods

#### Add Credits
```csharp
await creditService.AddMonthlyCreditsAsync(
    userMembershipId: membership.Id,
    amount: 100m,
    description: "Monthly credit allocation"
);
```

#### Deduct Credits (with FIFO)
```csharp
await creditService.DeductCreditsAsync(
    userMembershipId: membership.Id,
    amount: 50m,
    description: "Swedish Massage Booking",
    relatedBookingId: booking.Id
);
```

#### Check Available Balance
```csharp
var availableCredits = await creditService.GetAvailableCreditsAsync(membership.Id);
// Automatically expires old credits before returning balance
```

#### Expire Old Credits
```csharp
// Runs automatically on balance checks, or can be scheduled
await creditService.ExpireOldCreditsAsync(membership.Id);

// Or expire for all members (background job)
await creditService.ExpireAllOldCreditsAsync();
```

## Configuration

Settings are configured in `appsettings.json`:

```json
{
  "MembershipSettings": {
    "CreditExpirationMonths": 12,
    "AllowUnlimitedRollover": true
  }
}
```

## Background Jobs (Future Enhancement)

Consider implementing a daily scheduled job to:
1. Expire old credits across all active memberships
2. Send notifications to members about upcoming expirations (30 days before)
3. Generate monthly credit allocation reports

Example using Hangfire or similar:

```csharp
RecurringJob.AddOrUpdate<IMembershipCreditService>(
    "expire-credits",
    service => service.ExpireAllOldCreditsAsync(),
    Cron.Daily
);
```

## User Interface

### Member Dashboard
- Displays current available credit balance
- Shows expiration information: "Credits expire 12 months after issuance"
- Provides transaction history with dates and amounts
- Highlights upcoming expirations

### Booking Flow
- Automatically calculates credit usage at checkout
- Shows remaining balance after booking
- Displays discount percentage applied

## Testing Considerations

### Test Scenarios
1. **Credit Allocation**: Verify monthly credits are added correctly
2. **FIFO Usage**: Confirm oldest credits are used first
3. **Expiration**: Test that credits expire after 12 months
4. **Rollover**: Validate unlimited accumulation
5. **Edge Cases**: 
   - Member with 0 credits
   - Booking that exceeds available credits
   - Cancellation and credit refund
   - Membership cancellation with remaining credits

## Future Enhancements

1. **Credit Gifting**: Allow members to transfer credits to friends/family
2. **Credit Packages**: One-time credit purchases for non-members
3. **Promotional Credits**: Bonus credits for referrals or special events
4. **Credit Freezing**: Pause expiration during membership holds
5. **Advanced Notifications**: Email/SMS alerts for expiring credits


# Membership & Credits System

## Overview

SpaBooker's membership system provides flexible monthly credits that can be used toward any spa service. Credits offer exceptional value through unlimited rollover with a 12-month expiration window.

---

## Membership Plans

| Plan | Monthly Price | Credits | Discount |
|------|--------------|---------|----------|
| Bronze | $99 | 100 | 10% |
| Silver | $149 | 200 | 15% |
| Gold | $199 | 300 | 20% |

---

## Credit Rules

### Monthly Credit Allocation
- Credits are automatically added on each billing cycle
- Amount depends on membership plan
- Stripe handles recurring billing

### Credit Rollover Policy
- **Unlimited Rollover**: No maximum cap on credit accumulation
- **12-Month Expiration**: Credits expire 12 months after issuance
- **FIFO Usage**: Oldest credits are automatically used first (First In, First Out)

### Credit Usage
- Credits can be applied to any active spa service
- Members receive additional discounts on service prices
- Credits are deducted at booking time
- Unused credits after expiration are automatically removed

---

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

---

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
    public DateTime? ExpiresAt { get; set; } // 12 months from CreatedAt
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

---

## Configuration

Settings in `appsettings.json`:

```json
{
  "MembershipSettings": {
    "CreditExpirationMonths": 12,
    "AllowUnlimitedRollover": true
  }
}
```

---

## User Interface

### Member Dashboard
- Displays current available credit balance
- Shows expiration information
- Provides transaction history with dates and amounts
- Highlights upcoming expirations

### Booking Flow
- Automatically calculates credit usage at checkout
- Shows remaining balance after booking
- Displays discount percentage applied

---

## Subscription Management

### Cancellation
- Members can cancel anytime
- Credits remain valid until billing period ends
- No new credits added after cancellation
- Access continues until end of paid period

### Reactivation
- Former members can resubscribe
- Previous credits are not restored
- Fresh credit allocation begins with new subscription

---

## Future Enhancements

1. **Credit Gifting**: Transfer credits to friends/family
2. **Credit Packages**: One-time purchases for non-members
3. **Promotional Credits**: Bonus credits for referrals
4. **Credit Freezing**: Pause expiration during holds
5. **Expiration Notifications**: Email/SMS alerts for expiring credits


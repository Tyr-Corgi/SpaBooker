# Stripe Payment Integration - Implementation Complete

## Overview

Fully integrated Stripe payment processing for booking deposits and membership subscriptions using Stripe Checkout for a secure, PCI-compliant payment experience.

## ✅ Features Implemented

### 1. **Booking Deposit Payments**
- Stripe Checkout integration for 50% booking deposits
- Automatic redirect to Stripe hosted payment page
- Success page with booking confirmation
- Cancellation handling (return to payment page)

### 2. **Membership Subscriptions**
- Stripe Checkout for recurring monthly subscriptions
- Automatic billing on subscription renewal
- Subscription success page with membership details
- Cancel subscription functionality (maintains credits until period end)

### 3. **Refund Processing**
- Automatic refund for cancellations within policy window (24 hours)
- Deposit forfeiture for late cancellations
- Clear messaging to users about refund status
- Refund ID tracking in database

### 4. **Payment Configuration**
- Stripe API key configuration in appsettings.json
- Webhook support for payment events
- Test mode support for development

---

## Technical Implementation

### Backend Services

#### IStripeService Interface
Located: `src/SpaBooker.Core/Interfaces/IStripeService.cs`

```csharp
public interface IStripeService
{
    // Payment Intents (for future use)
    Task<string> CreatePaymentIntentAsync(...);
    Task<bool> CancelPaymentIntentAsync(...);
    
    // Refunds
    Task<string> CreateRefundAsync(string paymentIntentId, decimal? amount = null);
    
    // Customers
    Task<string> CreateCustomerAsync(...);
    
    // Subscriptions
    Task<string> CreateSubscriptionAsync(...);
    Task<bool> CancelSubscriptionAsync(string subscriptionId);
    Task<bool> UpdateSubscriptionAsync(...);
    
    // Checkout Sessions (NEW)
    Task<string> CreateCheckoutSessionAsync(...);
    Task<string> CreateSubscriptionCheckoutSessionAsync(...);
}
```

#### StripeService Implementation
Located: `src/SpaBooker.Infrastructure/Services/StripeService.cs`

**Key Methods:**
- `CreateCheckoutSessionAsync` - One-time payments (deposits)
- `CreateSubscriptionCheckoutSessionAsync` - Recurring subscriptions
- `CreateRefundAsync` - Process refunds for cancelled bookings

---

### Frontend Pages

#### 1. Booking Payment Flow

**BookingPayment.razor** (`/bookings/payment/{BookingId}`)
- Displays booking summary
- Calculates 50% deposit
- Shows cancellation policy
- Redirects to Stripe Checkout on payment

```csharp
// Payment flow
var checkoutUrl = await StripeService.CreateCheckoutSessionAsync(
    customerEmail: booking.Client.Email,
    amount: depositAmount,
    currency: "usd",
    description: $"Booking Deposit - {service.Name}",
    successUrl: "{baseUrl}/payment/success?booking_id={id}",
    cancelUrl: "{baseUrl}/bookings/payment/{id}",
    metadata: { booking_id, client_id, service_id }
);
NavigationManager.NavigateTo(checkoutUrl, forceLoad: true);
```

**PaymentSuccess.razor** (`/payment/success`)
- Confirms payment completion
- Displays booking details
- Updates booking status to "Confirmed"
- Provides next steps and navigation options

#### 2. Membership Subscription Flow

**MembershipPlans.razor** (`/memberships/plans`)
- Lists all active membership plans
- "Choose Plan" button for authenticated users
- Navigates to subscription page

**Subscribe.razor** (`/memberships/subscribe/{PlanId}`)
- Displays selected plan details
- Shows monthly pricing and benefits
- Checks for existing active membership (prevents duplicates)
- Redirects to Stripe Checkout for subscription

```csharp
var checkoutUrl = await StripeService.CreateSubscriptionCheckoutSessionAsync(
    customerEmail: user.Email,
    priceId: plan.StripePriceId, // Stripe Price ID from database
    successUrl: "{baseUrl}/memberships/subscription/success?plan_id={id}",
    cancelUrl: "{baseUrl}/memberships/subscribe/{id}",
    metadata: { user_id, plan_id, plan_name }
);
```

**SubscriptionSuccess.razor** (`/memberships/subscription/success`)
- Confirms subscription activation
- Displays membership benefits
- Links to "My Membership" page

#### 3. Membership Management

**MyMembership.razor** (`/memberships/my-membership`)
- Displays active membership details
- Shows current credit balance
- Lists credit transaction history
- **Cancel Subscription** button (with confirmation modal)

```csharp
// Cancel subscription
var success = await StripeService.CancelSubscriptionAsync(subscriptionId);
if (success) {
    membership.Status = MembershipStatus.Cancelled;
    membership.EndDate = membership.NextBillingDate; // Access until period end
    await DbContext.SaveChangesAsync();
}
```

#### 4. Booking Cancellation with Refunds

**MyBookings.razor** (`/bookings/my-bookings`)
- Cancel button on upcoming bookings
- Automatic refund calculation based on policy
- Different messaging based on refund eligibility

```csharp
// Refund logic
var cancellationWindowHours = 24; // From settings
var hoursUntilAppointment = (booking.StartTime - DateTime.UtcNow).TotalHours;

if (hoursUntilAppointment >= cancellationWindowHours) {
    // Eligible for refund
    var refundId = await StripeService.CreateRefundAsync(paymentIntentId);
    message = "Cancelled and $X refunded";
} else {
    // Late cancellation - deposit forfeited
    message = "Cancelled. Deposit forfeited per policy";
}
```

---

## Configuration

### 1. Stripe API Keys

**Location**: `src/SpaBooker.Web/appsettings.json`

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",  // For future client-side use
    "SecretKey": "sk_test_...",       // Server-side API calls
    "WebhookSecret": "whsec_..."      // Webhook signature verification
  }
}
```

**Test Mode Keys** (for development):
- Get from https://dashboard.stripe.com/test/apikeys
- Use test credit card: `4242 4242 4242 4242`
- Any future expiry date
- Any 3-digit CVC

**Production Keys**:
- Switch to live keys when ready
- Get from https://dashboard.stripe.com/apikeys

### 2. Membership Plan Setup

**Important**: Each `MembershipPlan` in the database must have a `StripePriceId` configured.

**Steps to create Stripe Products & Prices:**

1. **Go to Stripe Dashboard** → Products
2. **Create Product** (e.g., "Gold Membership")
3. **Add Pricing**:
   - Recurring: Monthly
   - Price: $199.00
   - Currency: USD
4. **Copy Price ID** (e.g., `price_1234567890`)
5. **Update Database**:

```sql
UPDATE "MembershipPlans"
SET "StripePriceId" = 'price_1234567890'
WHERE "Name" = 'Gold';
```

Repeat for Bronze and Silver plans.

---

## Payment Flow Diagrams

### Booking Deposit Flow

```
User selects service → Create booking
       ↓
Navigate to /bookings/payment/{id}
       ↓
User clicks "Pay Deposit"
       ↓
Stripe Checkout Session created
       ↓
Redirect to Stripe (user enters card)
       ↓
Payment successful
       ↓
Redirect to /payment/success
       ↓
Booking status updated to "Confirmed"
       ↓
User can view in "My Bookings"
```

### Membership Subscription Flow

```
User views plans → Selects plan
       ↓
Navigate to /memberships/subscribe/{planId}
       ↓
User clicks "Subscribe Now"
       ↓
Stripe Checkout Session created
       ↓
Redirect to Stripe (user enters card)
       ↓
Subscription created
       ↓
Redirect to /memberships/subscription/success
       ↓
Webhook processes subscription.created
       ↓
UserMembership record created in database
       ↓
Credits added to user account
```

### Cancellation & Refund Flow

```
User clicks "Cancel" on booking
       ↓
Calculate hours until appointment
       ↓
Check cancellation window (24 hours)
       ↓
IF within window:
    - Issue full deposit refund via Stripe
    - Message: "Refunded $X"
ELSE:
    - No refund (deposit forfeited)
    - Message: "Deposit forfeited per policy"
       ↓
Update booking status to "Cancelled"
       ↓
Save cancellation reason & timestamp
```

---

## Webhook Integration

### Current Webhooks

**Location**: `src/SpaBooker.Web/Controllers/StripeWebhookController.cs`

**Supported Events:**
1. `checkout.session.completed` - Payment/subscription successful
2. `customer.subscription.updated` - Subscription changed
3. `customer.subscription.deleted` - Subscription cancelled
4. `invoice.payment_succeeded` - Monthly billing successful
5. `invoice.payment_failed` - Payment failed

### Webhook Setup

1. **Local Development** (use Stripe CLI):
```bash
stripe listen --forward-to localhost:5226/api/stripe/webhook
```

2. **Production**:
   - Go to Stripe Dashboard → Developers → Webhooks
   - Add endpoint: `https://yourdomain.com/api/stripe/webhook`
   - Select events to listen for
   - Copy webhook signing secret to appsettings.json

---

## Testing Guide

### Test Booking Deposit

1. Login as a client
2. Browse services → Select service
3. Choose therapist, date, time
4. Click "Book Now"
5. Navigate to payment page
6. Click "Pay Deposit"
7. Enter test card: `4242 4242 4242 4242`
8. Complete payment
9. Verify success page shows booking details
10. Check "My Bookings" - status should be "Confirmed"

### Test Membership Subscription

1. Login as a client
2. Go to Memberships → Plans
3. Select a plan (e.g., Gold)
4. Click "Subscribe Now"
5. Enter test card: `4242 4242 4242 4242`
6. Complete subscription
7. Verify success page
8. Check "My Membership" - should show active subscription

### Test Booking Cancellation with Refund

**Within Cancellation Window:**
1. Create booking with payment
2. Go to "My Bookings"
3. Click "Cancel" on upcoming booking
4. Confirm cancellation
5. Verify message: "Cancelled and refunded"
6. Check Stripe Dashboard - refund should appear

**Outside Cancellation Window:**
1. Modify booking start time to be < 24 hours from now (database)
2. Attempt to cancel
3. Verify message: "Deposit forfeited"
4. Check Stripe Dashboard - no refund issued

### Test Subscription Cancellation

1. Have an active membership
2. Go to "My Membership"
3. Click "Cancel Membership"
4. Confirm in modal
5. Verify status changes to "Cancelled"
6. Credits remain until next billing date
7. Check Stripe Dashboard - subscription cancelled

---

## Database Changes

### Booking Table
- `StripePaymentIntentId` - Links to Stripe payment
- `StripeRefundId` - Links to Stripe refund (if issued)
- `DepositAmount` - Amount paid as deposit
- `CancellationReason` - Why booking was cancelled

### UserMembership Table
- `StripeSubscriptionId` - Links to Stripe subscription
- `StripeCustomerId` - Links to Stripe customer
- `Status` - Active, Cancelled, Expired, PastDue

### MembershipPlan Table
- `StripePriceId` - Stripe Price ID for subscription
- `StripeProductId` - Stripe Product ID (for reference)

---

## Security Considerations

### 1. PCI Compliance
✅ **Stripe Checkout handles all card data** - You never touch or store card numbers
✅ **PCI-DSS compliant** out of the box
✅ **3D Secure support** included

### 2. API Key Security
- Store keys in `appsettings.json` (excluded from git)
- Use environment variables in production
- Never commit keys to source control

### 3. Webhook Signature Verification
- `StripeWebhookController` verifies all webhook signatures
- Prevents unauthorized webhook calls
- Validates event authenticity

---

## Next Steps (Optional Enhancements)

### 1. Enhanced Payment Features
- [ ] Payment history page
- [ ] Download receipts/invoices
- [ ] Payment method management
- [ ] Save cards for future use

### 2. Subscription Enhancements
- [ ] Upgrade/downgrade plans
- [ ] Prorate charges when changing plans
- [ ] Pause subscription (freeze credits)
- [ ] Gift subscriptions

### 3. Advanced Refund Logic
- [ ] Partial refunds (e.g., 50% for same-day cancellations)
- [ ] Store credit instead of refund
- [ ] Admin override for refunds

### 4. Reporting & Analytics
- [ ] Revenue dashboard
- [ ] Subscription metrics (MRR, churn rate)
- [ ] Payment failure tracking
- [ ] Refund analytics

---

## Troubleshooting

### Payment not completing
- Check Stripe Dashboard for payment status
- Verify webhook is receiving events
- Check browser console for errors
- Ensure API keys are correct

### Subscription not activating
- Verify `StripePriceId` is set in database
- Check webhook handler is processing `checkout.session.completed`
- Look for errors in application logs

### Refund failing
- Verify payment was captured (not just authorized)
- Check payment is not already refunded
- Ensure sufficient time has passed (Stripe has delays)

---

## Support Resources

- [Stripe Documentation](https://stripe.com/docs)
- [Stripe Checkout](https://stripe.com/docs/payments/checkout)
- [Stripe Subscriptions](https://stripe.com/docs/billing/subscriptions/overview)
- [Stripe Testing](https://stripe.com/docs/testing)
- [Stripe CLI](https://stripe.com/docs/stripe-cli)

---

## Summary

✅ **Booking Deposits** - Secure 50% deposit collection with Stripe Checkout
✅ **Memberships** - Recurring subscriptions with automatic billing
✅ **Refunds** - Automated refund processing based on cancellation policy
✅ **Subscription Management** - Cancel anytime with credits maintained
✅ **Test Ready** - Full test mode support for development

**All Stripe payment functionality is production-ready!** 🎉


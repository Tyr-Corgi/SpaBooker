# Stripe Payment Integration

## Overview

SpaBooker uses Stripe for all payment processing, including booking deposits and membership subscriptions. Stripe Checkout provides a secure, PCI-compliant payment experience.

---

## Features

### Booking Deposits
- 50% deposit collected at booking
- Stripe Checkout hosted payment page
- Automatic refunds for eligible cancellations
- Payment confirmation and receipt

### Membership Subscriptions
- Recurring monthly billing
- Automatic credit allocation on payment
- Cancel anytime (credits maintained until period end)
- Subscription management in user dashboard

### Refund Processing
- Automatic refund for cancellations within 24-hour window
- Deposit forfeiture for late cancellations
- Refund tracking in database

---

## Configuration

### API Keys

Add to `appsettings.json` or User Secrets:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

**Get your keys:**
- Test mode: https://dashboard.stripe.com/test/apikeys
- Live mode: https://dashboard.stripe.com/apikeys

### Test Cards

| Card Number | Description |
|-------------|-------------|
| `4242 4242 4242 4242` | Successful payment |
| `4000 0000 0000 3220` | 3D Secure required |
| `4000 0000 0000 9995` | Declined |

Use any future expiry date and any 3-digit CVC.

---

## Membership Plan Setup

Each `MembershipPlan` in the database must have a `StripePriceId`.

### Create Products in Stripe Dashboard

1. Go to Stripe Dashboard → Products
2. Create Product (e.g., "Gold Membership")
3. Add Pricing:
   - Recurring: Monthly
   - Price: $199.00
   - Currency: USD
4. Copy Price ID (e.g., `price_1234567890`)

### Update Database

```sql
UPDATE "MembershipPlans"
SET "StripePriceId" = 'price_1234567890',
    "StripeProductId" = 'prod_1234567890'
WHERE "Name" = 'Gold';
```

Repeat for all membership plans.

---

## Webhook Setup

### Supported Events

| Event | Handler Action |
|-------|---------------|
| `checkout.session.completed` | Confirm payment, update booking/membership |
| `customer.subscription.updated` | Update membership status |
| `customer.subscription.deleted` | Cancel membership |
| `invoice.payment_succeeded` | Add monthly credits |
| `invoice.payment_failed` | Mark membership as past due |

### Local Development

Use Stripe CLI to forward webhooks:

```bash
# Install Stripe CLI
# Windows: scoop install stripe
# Mac: brew install stripe/stripe-cli/stripe

# Login
stripe login

# Forward webhooks
stripe listen --forward-to localhost:5226/api/stripe/webhook
```

Copy the webhook signing secret and add to User Secrets.

### Production Setup

1. Go to Stripe Dashboard → Developers → Webhooks
2. Add endpoint: `https://yourdomain.com/api/stripe/webhook`
3. Select events:
   - `checkout.session.completed`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
4. Copy signing secret to production configuration

---

## Payment Flows

### Booking Deposit

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
```

### Membership Subscription

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
UserMembership record created
       ↓
Credits added to user account
```

### Cancellation & Refund

```
User clicks "Cancel" on booking
       ↓
Calculate hours until appointment
       ↓
Check cancellation window (24 hours)
       ↓
IF within window:
    - Issue full deposit refund
    - Message: "Refunded $X"
ELSE:
    - No refund (deposit forfeited)
    - Message: "Deposit forfeited per policy"
       ↓
Update booking status to "Cancelled"
```

---

## Database Schema

### Booking Table
- `StripePaymentIntentId` - Links to Stripe payment
- `StripeRefundId` - Links to Stripe refund (if issued)
- `DepositAmount` - Amount paid as deposit

### UserMembership Table
- `StripeSubscriptionId` - Links to Stripe subscription
- `StripeCustomerId` - Links to Stripe customer
- `Status` - Active, Cancelled, Expired, PastDue

### MembershipPlan Table
- `StripePriceId` - Stripe Price ID for subscription
- `StripeProductId` - Stripe Product ID

---

## Security

### PCI Compliance
- Stripe Checkout handles all card data
- No card numbers stored in database
- 3D Secure support included

### Webhook Security
- All webhooks verified with signing secret
- Idempotency tracking prevents duplicate processing
- Failed events logged for debugging

### API Key Security
- Never commit keys to source control
- Use User Secrets for development
- Use environment variables for production

---

## Troubleshooting

### Payment not completing
- Check Stripe Dashboard for payment status
- Verify webhook is receiving events
- Check browser console for errors
- Ensure API keys are correct

### Subscription not activating
- Verify `StripePriceId` is set in database
- Check webhook handler is processing events
- Look for errors in application logs

### Refund failing
- Verify payment was captured (not just authorized)
- Check payment is not already refunded
- Ensure sufficient time has passed

### Webhook not receiving events
- Verify endpoint URL is correct
- Check firewall allows Stripe IPs
- Verify signing secret matches
- Check application logs for errors

---

## Resources

- [Stripe Documentation](https://stripe.com/docs)
- [Stripe Checkout](https://stripe.com/docs/payments/checkout)
- [Stripe Subscriptions](https://stripe.com/docs/billing/subscriptions/overview)
- [Stripe Testing](https://stripe.com/docs/testing)
- [Stripe CLI](https://stripe.com/docs/stripe-cli)
- [Webhook Events](https://stripe.com/docs/webhooks)


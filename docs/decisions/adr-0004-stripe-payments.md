# ADR-0004: Use Stripe for Payment Processing

## Status
Accepted

## Date
2025-11-01

## Context
The application requires payment processing for:
- Booking deposits (50% of service price)
- Membership subscriptions (recurring monthly)
- Refunds for eligible cancellations

We needed a payment provider that:
- Handles PCI-DSS compliance
- Supports both one-time and recurring payments
- Has good developer experience
- Works in New Zealand market

## Decision
We chose **Stripe** as the payment processor, using **Stripe Checkout** for the payment flow.

## Consequences

### Positive
- **PCI-DSS compliant**: No card data touches our servers
- **Hosted checkout**: Professional, secure payment pages
- **Subscription support**: Built-in recurring billing
- **Refund API**: Easy programmatic refunds
- **Webhooks**: Reliable event notifications
- **Developer experience**: Excellent documentation and SDKs
- **Test mode**: Full testing without real payments
- **Global support**: Works in New Zealand and internationally

### Negative
- **Transaction fees**: 2.9% + $0.30 per transaction (NZ rates may vary)
- **Vendor lock-in**: Switching payment providers requires significant work
- **Webhook complexity**: Need to handle various event types

### Neutral
- Requires Stripe account setup
- Need to create Products/Prices in Stripe Dashboard for subscriptions
- Webhook endpoint needs to be publicly accessible

## Alternatives Considered

### PayPal
- Rejected: Less developer-friendly API
- Checkout experience less polished

### Square
- Rejected: Less mature subscription support
- Smaller developer ecosystem

### Braintree
- Rejected: More complex setup
- Similar to Stripe but less documentation

### Custom Payment Integration
- Rejected: PCI-DSS compliance burden
- Security risk of handling card data

## Implementation Details

### Stripe Checkout
Used for both one-time payments and subscriptions:
- Creates hosted payment page
- Handles 3D Secure authentication
- Returns to success/cancel URLs

### Webhooks
Handle asynchronous events:
- `checkout.session.completed` - Payment successful
- `invoice.payment_succeeded` - Subscription renewed
- `invoice.payment_failed` - Payment failed
- `customer.subscription.deleted` - Subscription cancelled

### Idempotency
`ProcessedWebhookEvent` table prevents duplicate processing.

## References
- [Stripe Documentation](https://stripe.com/docs)
- [Stripe Checkout](https://stripe.com/docs/payments/checkout)
- [Stripe Billing](https://stripe.com/docs/billing)
- [Stripe Webhooks](https://stripe.com/docs/webhooks)


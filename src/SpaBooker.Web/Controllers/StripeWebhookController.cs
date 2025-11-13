using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Enums;
using SpaBooker.Infrastructure.Data;
using Stripe;

namespace SpaBooker.Web.Controllers;

[ApiController]
[Route("api/stripe/webhook")]
public class StripeWebhookController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<StripeWebhookController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret
            );

            _logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);

            // Handle the event
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentSucceeded(paymentIntent);
                    break;

                case "payment_intent.payment_failed":
                    var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentFailed(failedPaymentIntent);
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                    var subscription = stripeEvent.Data.Object as Subscription;
                    await HandleSubscriptionUpdated(subscription);
                    break;

                case "customer.subscription.deleted":
                    var deletedSubscription = stripeEvent.Data.Object as Subscription;
                    await HandleSubscriptionDeleted(deletedSubscription);
                    break;

                case "invoice.payment_succeeded":
                    var invoice = stripeEvent.Data.Object as Invoice;
                    await HandleInvoicePaymentSucceeded(invoice);
                    break;

                case "invoice.payment_failed":
                    var failedInvoice = stripeEvent.Data.Object as Invoice;
                    await HandleInvoicePaymentFailed(failedInvoice);
                    break;

                default:
                    _logger.LogWarning("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe webhook error");
            return BadRequest();
        }
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent? paymentIntent)
    {
        if (paymentIntent == null) return;

        // Find booking by payment intent ID
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.StripePaymentIntentId == paymentIntent.Id);

        if (booking != null)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Booking {BookingId} confirmed after payment", booking.Id);
        }
    }

    private async Task HandlePaymentIntentFailed(PaymentIntent? paymentIntent)
    {
        if (paymentIntent == null) return;

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.StripePaymentIntentId == paymentIntent.Id);

        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Payment failed";
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogWarning("Booking {BookingId} cancelled due to payment failure", booking.Id);
        }
    }

    private async Task HandleSubscriptionUpdated(Subscription? subscription)
    {
        if (subscription == null) return;

        var membership = await _context.UserMemberships
            .FirstOrDefaultAsync(m => m.StripeSubscriptionId == subscription.Id);

        if (membership != null)
        {
            membership.Status = subscription.Status switch
            {
                "active" => MembershipStatus.Active,
                "past_due" => MembershipStatus.Active,
                "canceled" => MembershipStatus.Cancelled,
                "unpaid" => MembershipStatus.Inactive,
                _ => MembershipStatus.Inactive
            };

            // TODO: Fix Stripe API property names
            // if (subscription.CurrentPeriodEnd.HasValue)
            // {
            //     membership.NextBillingDate = subscription.CurrentPeriodEnd.Value;
            // }

            membership.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Membership {MembershipId} updated", membership.Id);
        }
    }

    private async Task HandleSubscriptionDeleted(Subscription? subscription)
    {
        if (subscription == null) return;

        var membership = await _context.UserMemberships
            .FirstOrDefaultAsync(m => m.StripeSubscriptionId == subscription.Id);

        if (membership != null)
        {
            membership.Status = MembershipStatus.Cancelled;
            membership.EndDate = DateTime.UtcNow;
            membership.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Membership {MembershipId} cancelled", membership.Id);
        }
    }

    private async Task HandleInvoicePaymentSucceeded(Invoice? invoice)
    {
        // TODO: Fix Stripe API property names
        _logger.LogInformation("Invoice payment succeeded: {InvoiceId}", invoice?.Id);
        await Task.CompletedTask;
        // if (invoice?.SubscriptionId == null) return;
        //
        // var membership = await _context.UserMemberships
        //     .Include(m => m.MembershipPlan)
        //     .FirstOrDefaultAsync(m => m.StripeSubscriptionId == invoice.SubscriptionId);
        //
        // if (membership != null)
        // {
        //     // Add monthly credits
        //     membership.CurrentCredits += membership.MembershipPlan.MonthlyCredits;
        //     membership.UpdatedAt = DateTime.UtcNow;
        //
        //     // Log credit transaction
        //     var transaction = new Core.Entities.MembershipCreditTransaction
        //     {
        //         UserMembershipId = membership.Id,
        //         Amount = membership.MembershipPlan.MonthlyCredits,
        //         Type = "Credit",
        //         Description = $"Monthly credits for {DateTime.UtcNow:MMMM yyyy}",
        //         CreatedAt = DateTime.UtcNow
        //     };
        //
        //     _context.MembershipCreditTransactions.Add(transaction);
        //     await _context.SaveChangesAsync();
        //     _logger.LogInformation("Added monthly credits to membership {MembershipId}", membership.Id);
        // }
    }

    private async Task HandleInvoicePaymentFailed(Invoice? invoice)
    {
        // TODO: Fix Stripe API property names
        _logger.LogWarning("Invoice payment failed: {InvoiceId}", invoice?.Id);
        await Task.CompletedTask;
        // if (invoice?.SubscriptionId == null) return;
        //
        // var membership = await _context.UserMemberships
        //     .FirstOrDefaultAsync(m => m.StripeSubscriptionId == invoice.SubscriptionId);
        //
        // if (membership != null)
        // {
        //     membership.Status = MembershipStatus.Inactive;
        //     membership.UpdatedAt = DateTime.UtcNow;
        //     await _context.SaveChangesAsync();
        //     _logger.LogWarning("Membership {MembershipId} marked inactive due to payment failure", membership.Id);
        // }
    }
}


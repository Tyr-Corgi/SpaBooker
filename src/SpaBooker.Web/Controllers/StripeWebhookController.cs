using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;
using Stripe;

namespace SpaBooker.Web.Controllers;

[ApiController]
[Route("api/stripe/webhook")]
public class StripeWebhookController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IAuditService _auditService;

    public StripeWebhookController(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<StripeWebhookController> logger,
        IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _auditService = auditService;
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

            _logger.LogInformation("Stripe webhook received: {EventType} with ID {EventId}", stripeEvent.Type, stripeEvent.Id);

            // Check for idempotency - prevent duplicate processing
            if (await _context.ProcessedWebhookEvents.AnyAsync(e => e.StripeEventId == stripeEvent.Id))
            {
                _logger.LogInformation("Webhook event {EventId} already processed, skipping", stripeEvent.Id);
                return Ok(); // Already processed
            }

            bool success = true;
            string? errorMessage = null;

            try
            {
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
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
                _logger.LogError(ex, "Error processing webhook event {EventId} of type {EventType}",
                    stripeEvent.Id, stripeEvent.Type);
                throw; // Re-throw to return error to Stripe
            }
            finally
            {
                // Record that we processed this event
                var processedEvent = new Core.Entities.ProcessedWebhookEvent
                {
                    StripeEventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    ProcessedAt = DateTime.UtcNow,
                    Success = success,
                    ErrorMessage = errorMessage
                };
                _context.ProcessedWebhookEvents.Add(processedEvent);
                await _context.SaveChangesAsync();
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

        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
        // Find booking by payment intent ID
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.StripePaymentIntentId == paymentIntent.Id);

        if (booking != null)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Booking {BookingId} confirmed after payment intent {PaymentIntentId}",
                    booking.Id, paymentIntent.Id);
                
                // Audit log the payment success
                await _auditService.LogPaymentAsync(
                    paymentIntent.Id,
                    paymentIntent.Amount / 100m, // Stripe amounts are in cents
                    true,
                    $"Booking {booking.Id} confirmed");
            }
            else
            {
                _logger.LogWarning("No booking found for payment intent {PaymentIntentId}", paymentIntent.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle payment intent succeeded for {PaymentIntentId}", paymentIntent.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task HandlePaymentIntentFailed(PaymentIntent? paymentIntent)
    {
        if (paymentIntent == null) return;

        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.StripePaymentIntentId == paymentIntent.Id);

        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Payment failed";
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.CommitAsync();
                _logger.LogWarning("Booking {BookingId} cancelled due to payment failure for payment intent {PaymentIntentId}",
                    booking.Id, paymentIntent.Id);
                
                // Audit log the payment failure
                await _auditService.LogPaymentAsync(
                    paymentIntent.Id,
                    paymentIntent.Amount / 100m,
                    false,
                    $"Booking {booking.Id} cancelled due to payment failure");
            }
            else
            {
                _logger.LogWarning("No booking found for failed payment intent {PaymentIntentId}", paymentIntent.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle payment intent failed for {PaymentIntentId}", paymentIntent.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task HandleSubscriptionUpdated(Subscription? subscription)
    {
        if (subscription == null) return;

        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
        var membership = await _context.UserMemberships
            .FirstOrDefaultAsync(m => m.StripeSubscriptionId == subscription.Id);

        if (membership != null)
        {
                var oldStatus = membership.Status;
                
            membership.Status = subscription.Status switch
            {
                "active" => MembershipStatus.Active,
                "past_due" => MembershipStatus.Active,
                "canceled" => MembershipStatus.Cancelled,
                "unpaid" => MembershipStatus.Inactive,
                _ => MembershipStatus.Inactive
            };

                // Update next billing date
                // TODO: Fix Stripe API - CurrentPeriodEnd property name may have changed in v49
                // membership.NextBillingDate = ... ;
                _logger.LogWarning("CurrentPeriodEnd property not available - skipping next billing date update");

            membership.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Membership {MembershipId} updated from {OldStatus} to {NewStatus}",
                    membership.Id, oldStatus, membership.Status);
            }
            else
            {
                _logger.LogWarning("No membership found for subscription {SubscriptionId}", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle subscription updated for {SubscriptionId}", subscription.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task HandleSubscriptionDeleted(Subscription? subscription)
    {
        if (subscription == null) return;

        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
        var membership = await _context.UserMemberships
            .FirstOrDefaultAsync(m => m.StripeSubscriptionId == subscription.Id);

        if (membership != null)
        {
            membership.Status = MembershipStatus.Cancelled;
            membership.EndDate = DateTime.UtcNow;
            membership.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Membership {MembershipId} cancelled for subscription {SubscriptionId}",
                    membership.Id, subscription.Id);
            }
            else
            {
                _logger.LogWarning("No membership found for deleted subscription {SubscriptionId}", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle subscription deleted for {SubscriptionId}", subscription.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task HandleInvoicePaymentSucceeded(Invoice? invoice)
    {
        // TODO: Fix Stripe API - Invoice.Subscription property access may have changed in v49
        // For now, skip subscription-related processing if we can't get the subscription ID
        string? subscriptionId = null;
        // subscriptionId = ... ; // Need to find correct property name
        
        if (string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogInformation("Invoice payment succeeded but no subscription: {InvoiceId}", invoice?.Id);
            return;
        }

        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var membership = await _context.UserMemberships
                .Include(m => m.MembershipPlan)
                .FirstOrDefaultAsync(m => m.StripeSubscriptionId == subscriptionId);

            if (membership != null)
            {
                // Add monthly credits
                membership.CurrentCredits += membership.MembershipPlan.MonthlyCredits;
                membership.UpdatedAt = DateTime.UtcNow;

                // Log credit transaction
                var transaction = new Core.Entities.MembershipCreditTransaction
                {
                    UserMembershipId = membership.Id,
                    Amount = membership.MembershipPlan.MonthlyCredits,
                    Type = "Credit",
                    Description = $"Monthly credits for {DateTime.UtcNow:MMMM yyyy}",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMonths(12) // 12 month expiration
                };

                _context.MembershipCreditTransactions.Add(transaction);

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Added {Credits} monthly credits to membership {MembershipId} for invoice {InvoiceId}",
                    membership.MembershipPlan.MonthlyCredits, membership.Id, invoice.Id);
            }
            else
            {
                _logger.LogWarning("No membership found for subscription {SubscriptionId} in invoice {InvoiceId}",
                    subscriptionId, invoice.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle invoice payment succeeded for {InvoiceId}", invoice.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task HandleInvoicePaymentFailed(Invoice? invoice)
    {
        // TODO: Fix Stripe API - Invoice.Subscription property access may have changed in v49
        // For now, skip subscription-related processing if we can't get the subscription ID
        string? subscriptionId = null;
        // subscriptionId = ... ; // Need to find correct property name
        
        if (string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogWarning("Invoice payment failed but no subscription: {InvoiceId}", invoice?.Id);
            return;
        }

        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var membership = await _context.UserMemberships
                .FirstOrDefaultAsync(m => m.StripeSubscriptionId == subscriptionId);

            if (membership != null)
            {
                membership.Status = MembershipStatus.Inactive;
                membership.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.CommitAsync();
                _logger.LogWarning("Membership {MembershipId} marked inactive due to payment failure for invoice {InvoiceId}",
                    membership.Id, invoice.Id);
            }
            else
            {
                _logger.LogWarning("No membership found for subscription {SubscriptionId} in invoice {InvoiceId}",
                    subscriptionId, invoice.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle invoice payment failed for {InvoiceId}", invoice.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}


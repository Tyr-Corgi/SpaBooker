using Microsoft.Extensions.Configuration;
using SpaBooker.Core.Interfaces;
using Stripe;

namespace SpaBooker.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly string _secretKey;

    public StripeService(IConfiguration configuration)
    {
        _secretKey = configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe SecretKey not configured");
        StripeConfiguration.ApiKey = _secretKey;
    }

    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency, string description, Dictionary<string, string>? metadata = null)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Convert to cents
            Currency = currency.ToLower(),
            Description = description,
            Metadata = metadata,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);
        return paymentIntent.Id;
    }

    public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            await service.CancelAsync(paymentIntentId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> CreateRefundAsync(string paymentIntentId, decimal? amount = null)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId
        };

        if (amount.HasValue)
        {
            options.Amount = (long)(amount.Value * 100);
        }

        var service = new RefundService();
        var refund = await service.CreateAsync(options);
        return refund.Id;
    }

    public async Task<string> CreateCustomerAsync(string email, string name, Dictionary<string, string>? metadata = null)
    {
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
            Metadata = metadata
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options);
        return customer.Id;
    }

    public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, Dictionary<string, string>? metadata = null)
    {
        var options = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Price = priceId
                }
            },
            Metadata = metadata,
            PaymentBehavior = "default_incomplete",
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = "on_subscription"
            },
            Expand = new List<string> { "latest_invoice.payment_intent" }
        };

        var service = new SubscriptionService();
        var subscription = await service.CreateAsync(options);
        return subscription.Id;
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            await service.CancelAsync(subscriptionId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionAsync(string subscriptionId, string newPriceId)
    {
        try
        {
            var service = new SubscriptionService();
            var subscription = await service.GetAsync(subscriptionId);

            var options = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Id = subscription.Items.Data[0].Id,
                        Price = newPriceId
                    }
                }
            };

            await service.UpdateAsync(subscriptionId, options);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> CreateCheckoutSessionAsync(string customerEmail, decimal amount, string currency, string description, string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null)
    {
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            CustomerEmail = customerEmail,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        Currency = currency.ToLower(),
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = description
                        },
                        UnitAmount = (long)(amount * 100) // Convert to cents
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = metadata
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> CreateSubscriptionCheckoutSessionAsync(string customerEmail, string priceId, string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null)
    {
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            CustomerEmail = customerEmail,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = metadata,
            SubscriptionData = new Stripe.Checkout.SessionSubscriptionDataOptions
            {
                Metadata = metadata
            }
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> CreateGiftCertificateCheckoutSessionAsync(int giftCertificateId, decimal amount, string purchasedByUserId)
    {
        var baseUrl = "https://localhost:5226"; // TODO: Make this configurable
        var successUrl = $"{baseUrl}/gift-certificates/success?giftCertId={giftCertificateId}";
        var cancelUrl = $"{baseUrl}/gift-certificates/purchase";

        var metadata = new Dictionary<string, string>
        {
            { "gift_certificate_id", giftCertificateId.ToString() },
            { "purchased_by_user_id", purchasedByUserId },
            { "type", "gift_certificate" }
        };

        var options = new Stripe.Checkout.SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "SpaBooker Gift Certificate",
                            Description = $"Gift certificate for ${amount:F2}",
                            Images = new List<string> { } // TODO: Add gift certificate image
                        },
                        UnitAmount = (long)(amount * 100) // Convert to cents
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = metadata
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }
}


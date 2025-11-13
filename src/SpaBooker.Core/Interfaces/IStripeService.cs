namespace SpaBooker.Core.Interfaces;

public interface IStripeService
{
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency, string description, Dictionary<string, string>? metadata = null);
    Task<bool> CancelPaymentIntentAsync(string paymentIntentId);
    Task<string> CreateRefundAsync(string paymentIntentId, decimal? amount = null);
    Task<string> CreateCustomerAsync(string email, string name, Dictionary<string, string>? metadata = null);
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, Dictionary<string, string>? metadata = null);
    Task<bool> CancelSubscriptionAsync(string subscriptionId);
    Task<bool> UpdateSubscriptionAsync(string subscriptionId, string newPriceId);
    
    // Stripe Checkout
    Task<string> CreateCheckoutSessionAsync(string customerEmail, decimal amount, string currency, string description, string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null);
    Task<string> CreateSubscriptionCheckoutSessionAsync(string customerEmail, string priceId, string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null);
    Task<string> CreateGiftCertificateCheckoutSessionAsync(int giftCertificateId, decimal amount, string purchasedByUserId);
}


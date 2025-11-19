using Microsoft.Extensions.Diagnostics.HealthChecks;
using Stripe;

namespace SpaBooker.Web.HealthChecks;

public class StripeHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeHealthCheck> _logger;

    public StripeHealthCheck(IConfiguration configuration, ILogger<StripeHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            
            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Contains("TO_BE_CONFIGURED"))
            {
                return HealthCheckResult.Degraded("Stripe API key not configured");
            }

            StripeConfiguration.ApiKey = secretKey;
            
            // Try to retrieve balance as a health check
            var service = new BalanceService();
            var balance = await service.GetAsync(cancellationToken: cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "available_balance", balance.Available.Sum(b => b.Amount) / 100m },
                { "pending_balance", balance.Pending.Sum(b => b.Amount) / 100m },
                { "currency", balance.Available.FirstOrDefault()?.Currency ?? "usd" }
            };

            _logger.LogInformation("Stripe health check passed");
            return HealthCheckResult.Healthy("Stripe API is accessible", data);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe health check failed: {Error}", ex.Message);
            return HealthCheckResult.Unhealthy($"Stripe API error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe health check failed unexpectedly");
            return HealthCheckResult.Unhealthy("Unexpected error checking Stripe API", ex);
        }
    }
}


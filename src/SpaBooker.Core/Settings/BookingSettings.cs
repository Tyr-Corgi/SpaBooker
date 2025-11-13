namespace SpaBooker.Core.Settings;

public class BookingSettings
{
    public decimal DepositPercentage { get; set; } = 50.0m; // 50% default deposit
    public int CancellationWindowHours { get; set; } = 24; // 24 hours before appointment
    public bool RefundDeposit { get; set; } = true; // Refund if cancelled within window
    public decimal LateCancellationFeePercentage { get; set; } = 100.0m; // 100% = full deposit forfeited
}

public class MembershipSettings
{
    public int CreditExpirationMonths { get; set; } = 12; // Credits expire after 12 months
    public bool AllowUnlimitedRollover { get; set; } = true; // No limit on credit rollover amount
}


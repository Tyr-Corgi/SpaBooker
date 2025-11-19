namespace SpaBooker.Core.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Time-related constants
    /// </summary>
    public static class Time
    {
        public const int DefaultCancellationWindowHours = 24;
        public const int DefaultCreditExpirationMonths = 12;
        public const int EmailConfirmationExpirationHours = 24;
        public const int DefaultSessionTimeoutMinutes = 30;
        public const int MaxBookingAdvanceDays = 90;
    }

    /// <summary>
    /// Financial constants
    /// </summary>
    public static class Financial
    {
        public const decimal DefaultDepositPercentage = 50.0m;
        public const decimal LateCancellationFeePercentage = 100.0m;
        public const decimal MinimumBookingAmount = 10.0m;
        public const decimal MaximumBookingAmount = 10000.0m;
        public const int StripeCentsMultiplier = 100; // Stripe amounts are in cents
    }

    /// <summary>
    /// Pagination constants
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 5;
    }

    /// <summary>
    /// Cache expiration times (in minutes)
    /// </summary>
    public static class CacheExpiration
    {
        public const int MembershipPlans = 60;
        public const int Services = 60;
        public const int Locations = 120;
        public const int TherapistAvailability = 15;
        public const int Default = 30;
    }

    /// <summary>
    /// Cache key prefixes
    /// </summary>
    public static class CacheKeys
    {
        public const string MembershipPlanPrefix = "membershipplan:";
        public const string ServicePrefix = "service:";
        public const string LocationPrefix = "location:";
        public const string AvailabilityPrefix = "availability:";
        public const string UserPrefix = "user:";
    }

    /// <summary>
    /// Validation constants
    /// </summary>
    public static class Validation
    {
        public const int MinPasswordLength = 12;
        public const int MinUsernameLength = 3;
        public const int MaxUsernameLength = 50;
        public const int MinServiceNameLength = 3;
        public const int MaxServiceNameLength = 100;
        public const int MaxDescriptionLength = 500;
        public const int MaxNotesLength = 2000;
    }

    /// <summary>
    /// Security constants
    /// </summary>
    public static class Security
    {
        public const int MaxLoginAttempts = 3;
        public const int LockoutDurationMinutes = 30;
        public const int PasswordRequiredUniqueChars = 4;
        public const int MaxFileSizeMB = 5;
    }

    /// <summary>
    /// Database constants
    /// </summary>
    public static class Database
    {
        public const int CommandTimeoutSeconds = 30;
        public const int MaxRetryCount = 3;
        public const int MaxRetryDelaySeconds = 5;
        public const int MinConnectionPoolSize = 5;
        public const int MaxConnectionPoolSize = 100;
    }

    /// <summary>
    /// Rate limiting constants
    /// </summary>
    public static class RateLimiting
    {
        public const int ApiRequestsPerSecond = 5;
        public const int LoginAttemptsPerWindow = 5;
        public const int RegistrationAttemptsPerWindow = 5;
        public const int RateLimitWindowMinutes = 15;
    }

    /// <summary>
    /// Booking constants
    /// </summary>
    public static class Booking
    {
        public const int MinDurationMinutes = 15;
        public const int MaxDurationMinutes = 480; // 8 hours
        public const int DefaultDurationMinutes = 60;
        public const int ReminderHoursBeforeBooking = 24;
    }

    /// <summary>
    /// Gift certificate constants
    /// </summary>
    public static class GiftCertificate
    {
        public const int CodeLength = 16;
        public const int DefaultValidityDays = 365;
        public const decimal MinAmount = 25.0m;
        public const decimal MaxAmount = 1000.0m;
    }

    /// <summary>
    /// File upload constants
    /// </summary>
    public static class FileUpload
    {
        public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx" };
    }

    /// <summary>
    /// API versioning constants
    /// </summary>
    public static class ApiVersioning
    {
        public const int CurrentMajorVersion = 1;
        public const int CurrentMinorVersion = 0;
        public const string CurrentVersion = "1.0";
        public const string HeaderName = "X-API-Version";
        public const string QueryParameterName = "api-version";
    }
}


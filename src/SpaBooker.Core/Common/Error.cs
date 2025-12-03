namespace SpaBooker.Core.Common;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    public Error(string code, string message, ErrorType type = ErrorType.Failure)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error None => new(string.Empty, string.Empty);
    public static Error NullValue => new("Error.NullValue", "Null value was provided");
    
    // Booking errors
    public static Error BookingNotFound => new("Booking.NotFound", "Booking was not found", ErrorType.NotFound);
    public static Error BookingConflict => new("Booking.Conflict", "Booking conflicts with existing booking", ErrorType.Conflict);
    public static Error InvalidTimeSlot => new("Booking.InvalidTimeSlot", "Invalid time slot provided", ErrorType.Validation);
    
    // Room errors
    public static Error RoomNotAvailable => new("Room.NotAvailable", "Room is not available for the selected time", ErrorType.Conflict);
    public static Error RoomNotFound => new("Room.NotFound", "Room was not found", ErrorType.NotFound);
    public static Error RoomNotCompatible => new("Room.NotCompatible", "Room does not support this service", ErrorType.Validation);
    
    // Therapist errors
    public static Error TherapistNotAvailable => new("Therapist.NotAvailable", "Therapist is not available for the selected time", ErrorType.Conflict);
    public static Error TherapistNotFound => new("Therapist.NotFound", "Therapist was not found", ErrorType.NotFound);
    public static Error TherapistNotQualified => new("Therapist.NotQualified", "Therapist is not qualified for this service", ErrorType.Validation);
    
    // Client errors
    public static Error ClientNotFound => new("Client.NotFound", "Client was not found", ErrorType.NotFound);
    
    // Service errors
    public static Error ServiceNotFound => new("Service.NotFound", "Service was not found", ErrorType.NotFound);
    public static Error ServiceNotActive => new("Service.NotActive", "Service is not active", ErrorType.Validation);
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}


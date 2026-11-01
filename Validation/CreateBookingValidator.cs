using BookingApi.Models;

namespace BookingApi.Validation;

public static class CreateBookingValidator
{
    public static Dictionary<string, string[]> Validate(
        CreateBookingRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            errors["customerName"] =
                new[] { "Customer name is required." };
        }

        if (string.IsNullOrWhiteSpace(request.PackageName))
        {
            errors["packageName"] =
                new[] { "Package name is required." };
        }

        if (request.BookingDate is null ||
            request.BookingDate == DateOnly.MinValue)
        {
            errors["bookingDate"] =
                new[] { "A valid booking date is required." };
        }

        if (request.StartTime is null)
{
    errors["startTime"] = ["Start time is required."];
}
else if (request.Hours is int hours &&
         request.StartTime.Value.ToTimeSpan()
         + TimeSpan.FromHours(hours) > TimeSpan.FromHours(24))
{
    errors["startTime"] =
    [
        "The booking must end by midnight."
    ];
}

        if (request.HourlyRate is null ||
            request.HourlyRate <= 0 ||
            request.HourlyRate > 10000)
        {
            errors["hourlyRate"] =
                new[] { "Hourly rate must be greater than 0 and no more than 10000." };
        }

        if (request.Hours is null ||
            request.Hours < 1 ||
            request.Hours > 8)
        {
            errors["hours"] =
                new[] { "Hours must be between 1 and 8." };
        }

        if (request.DiscountPercent is null ||
            request.DiscountPercent < 0 ||
            request.DiscountPercent > 100)
        {
            errors["discountPercent"] =
                new[] { "Discount percent must be between 0 and 100." };
        }

        if (request.BookingFee is null ||
            request.BookingFee < 0 ||
            request.BookingFee > 10000)
        {
            errors["bookingFee"] =
                new[] { "Booking fee must be between 0 and 10000." };
        }

        return errors;
    }
}
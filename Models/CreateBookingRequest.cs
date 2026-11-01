namespace BookingApi.Models;

public sealed class CreateBookingRequest
{
    public string? CustomerName { get; init; }

    public string? PackageName { get; init; }

    public DateOnly? BookingDate { get; init; }

    public TimeOnly? StartTime { get; init; }

    public decimal? HourlyRate { get; init; }

    public int? Hours { get; init; }

    public decimal? DiscountPercent { get; init; }

    public decimal? BookingFee { get; init; }
}
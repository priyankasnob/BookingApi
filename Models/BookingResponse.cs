namespace BookingApi.Models;

public sealed class BookingResponse
{
    public int Id { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string PackageName { get; init; } = string.Empty;

    public DateOnly BookingDate { get; init; }

    public decimal HourlyRate { get; init; }

    public int Hours { get; init; }

    public decimal DiscountPercent { get; init; }

    public decimal BookingFee { get; init; }

    public string Status { get; init; } = "Pending";

    public decimal TotalPrice =>
        HourlyRate * Hours
        - HourlyRate * Hours * DiscountPercent / 100m
        + BookingFee;
}
namespace BookingApi.Entities;

public sealed class BookingEntity
{
    public int Id { get; set; }

    public string? OwnerUserId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string PackageName { get; set; } = string.Empty;

    public DateOnly BookingDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public decimal HourlyRate { get; set; }

    public int Hours { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal BookingFee { get; set; }

    public string Status { get; set; } = "Pending";
}
namespace BookingApi.Models;

public sealed class PagedBookingsResponse
{
    public BookingResponse[] Items { get; init; }
        = Array.Empty<BookingResponse>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);
}
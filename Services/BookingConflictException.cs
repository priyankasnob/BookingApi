namespace BookingApi.Services;

public sealed class BookingConflictException : Exception
{
    public BookingConflictException()
        : base("The requested time overlaps an existing active booking.")
    {
    }
}
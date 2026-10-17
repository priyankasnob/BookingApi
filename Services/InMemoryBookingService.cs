using BookingApi.Models;
using BookingApi.Validation;

namespace BookingApi.Services;

public sealed class InMemoryBookingService : IBookingService
{
    private readonly object _bookingsLock = new();

    private readonly List<BookingResponse> _bookings = new()
    {
        new BookingResponse
        {
            Id = 1,
            CustomerName = "Alex",
            PackageName = "Portrait",
            BookingDate = new DateOnly(2026, 11, 10),
            HourlyRate = 125m,
            Hours = 2,
            DiscountPercent = 10m,
            BookingFee = 20m,
            Status = "Confirmed"
        },

        new BookingResponse
        {
            Id = 2,
            CustomerName = "Maya",
            PackageName = "Family",
            BookingDate = new DateOnly(2026, 11, 12),
            HourlyRate = 150m,
            Hours = 3,
            DiscountPercent = 10m,
            BookingFee = 20m,
            Status = "Pending"
        },

        new BookingResponse
        {
            Id = 3,
            CustomerName = "Jordan",
            PackageName = "Event",
            BookingDate = new DateOnly(2026, 11, 15),
            HourlyRate = 200m,
            Hours = 4,
            DiscountPercent = 5m,
            BookingFee = 30m,
            Status = "Confirmed"
        }
    };

    private int _nextBookingId = 3;

    public BookingResponse[] GetAll()
    {
        lock (_bookingsLock)
        {
            return _bookings.ToArray();
        }
    }

    public PagedBookingsResponse Search(
    string? customerName,
    string? status,
    int page,
    int pageSize)
{
    if (page < 1)
    {
        throw new ArgumentOutOfRangeException(nameof(page));
    }

    if (pageSize < 1 || pageSize > 100)
    {
        throw new ArgumentOutOfRangeException(nameof(pageSize));
    }

    BookingResponse[] snapshot = GetAll();

    IEnumerable<BookingResponse> query = snapshot;

    if (!string.IsNullOrWhiteSpace(customerName))
    {
        string nameFilter = customerName.Trim();

        query = query.Where(booking =>
            booking.CustomerName.Contains(
                nameFilter,
                StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        string statusFilter = status.Trim();

        query = query.Where(booking =>
            string.Equals(
                booking.Status,
                statusFilter,
                StringComparison.OrdinalIgnoreCase));
    }

    BookingResponse[] matchingBookings = query
        .OrderBy(booking => booking.Id)
        .ToArray();

    int totalCount = matchingBookings.Length;

    long skip = ((long)page - 1) * pageSize;

    BookingResponse[] items = skip >= totalCount
        ? Array.Empty<BookingResponse>()
        : matchingBookings
            .Skip((int)skip)
            .Take(pageSize)
            .ToArray();

    return new PagedBookingsResponse
    {
        Items = items,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };
}

    public BookingResponse? GetById(int id)
    {
        lock (_bookingsLock)
        {
            return _bookings.FirstOrDefault(
                booking => booking.Id == id);
        }
    }

    public BookingResponse Create(CreateBookingRequest request)
    {
        EnsureValid(request);

        lock (_bookingsLock)
        {
            _nextBookingId++;

            BookingResponse booking = BuildBooking(
                _nextBookingId,
                request,
                "Pending");

            _bookings.Add(booking);

            return booking;
        }
    }

    public BookingResponse? Update(
        int id,
        CreateBookingRequest request)
    {
        EnsureValid(request);

        lock (_bookingsLock)
        {
            int index = _bookings.FindIndex(
                booking => booking.Id == id);

            if (index == -1)
            {
                return null;
            }

            BookingResponse existingBooking = _bookings[index];

            BookingResponse updatedBooking = BuildBooking(
                existingBooking.Id,
                request,
                existingBooking.Status);

            _bookings[index] = updatedBooking;

            return updatedBooking;
        }
    }

    public bool Delete(int id)
    {
        lock (_bookingsLock)
        {
            int index = _bookings.FindIndex(
                booking => booking.Id == id);

            if (index == -1)
            {
                return false;
            }

            _bookings.RemoveAt(index);

            return true;
        }
    }

    private static BookingResponse BuildBooking(
        int id,
        CreateBookingRequest request,
        string status)
    {
        return new BookingResponse
        {
            Id = id,

            CustomerName = request.CustomerName!.Trim(),
            PackageName = request.PackageName!.Trim(),

            BookingDate = request.BookingDate!.Value,
            HourlyRate = request.HourlyRate!.Value,
            Hours = request.Hours!.Value,
            DiscountPercent = request.DiscountPercent!.Value,
            BookingFee = request.BookingFee!.Value,

            Status = status
        };
    }

    private static void EnsureValid(CreateBookingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = CreateBookingValidator.Validate(request);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "The booking request contains invalid values.",
                nameof(request));
        }
    }
}
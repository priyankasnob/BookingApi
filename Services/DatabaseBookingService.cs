using System.Linq.Expressions;
using BookingApi.Data;
using BookingApi.Entities;
using BookingApi.Models;
using BookingApi.Validation;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Services;

public sealed class DatabaseBookingService : IBookingService
{
    private readonly BookingDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DatabaseBookingService(
        BookingDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private static readonly Expression<Func<BookingEntity, BookingResponse>>
        ToResponse = entity => new BookingResponse
        {
            Id = entity.Id,
            CustomerName = entity.CustomerName,
            PackageName = entity.PackageName,
            BookingDate = entity.BookingDate,
            HourlyRate = entity.HourlyRate,
            Hours = entity.Hours,
            DiscountPercent = entity.DiscountPercent,
            BookingFee = entity.BookingFee,
            Status = entity.Status
        };

    private IQueryable<BookingEntity> OwnedBookings()
    {
        string userId = _currentUser.GetRequiredUserId();

        return _db.Bookings.Where(
            entity => entity.OwnerUserId == userId);
    }

    public async Task<BookingResponse[]> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await OwnedBookings()
            .AsNoTracking()
            .OrderBy(entity => entity.Id)
            .Select(ToResponse)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedBookingsResponse> SearchAsync(
        string? customerName,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        IQueryable<BookingEntity> query =
            OwnedBookings().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            string nameFilter = customerName.Trim().ToLowerInvariant();

            query = query.Where(entity =>
                entity.CustomerName.ToLower().Contains(nameFilter));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            string statusFilter = status.Trim().ToLowerInvariant() switch
            {
                "pending" => "Pending",
                "confirmed" => "Confirmed",
                "cancelled" => "Cancelled",
                _ => throw new ArgumentException(
                    "Unsupported booking status.", nameof(status))
            };

            query = query.Where(entity => entity.Status == statusFilter);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        long skip = ((long)page - 1) * pageSize;

        BookingResponse[] items = Array.Empty<BookingResponse>();

        if (skip < totalCount)
        {
            items = await query
                .OrderBy(entity => entity.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .Select(ToResponse)
                .ToArrayAsync(cancellationToken);
        }

        return new PagedBookingsResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BookingResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await OwnedBookings()
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(ToResponse)
            .FirstOrDefaultAsync(cancellationToken);
    }

private async Task<bool> HasSchedulingConflictAsync(
    DateOnly bookingDate,
    TimeOnly startTime,
    int hours,
    int? bookingIdToExclude,
    CancellationToken cancellationToken)
{
    var candidates = await _db.Bookings
        .AsNoTracking()
        .Where(booking =>
            booking.BookingDate == bookingDate &&
            booking.StartTime != null &&
            booking.Status != "Cancelled" &&
            (!bookingIdToExclude.HasValue ||
             booking.Id != bookingIdToExclude.Value))
        .Select(booking => new
        {
            booking.StartTime,
            booking.Hours
        })
        .ToListAsync(cancellationToken);

    var requestedEnd = startTime.AddHours(hours);

    return candidates.Any(booking =>
    {
        var existingStart = booking.StartTime!.Value;
        var existingEnd = existingStart.AddHours(booking.Hours);

        return startTime < existingEnd && existingStart < requestedEnd;
    });
}
    public async Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        string ownerUserId = _currentUser.GetRequiredUserId();

        EnsureValid(request);

        if (await HasSchedulingConflictAsync(
        request.BookingDate!.Value,
        request.StartTime!.Value,
        request.Hours!.Value,
        bookingIdToExclude: null,
        cancellationToken))
{
    throw new BookingConflictException();
}

        cancellationToken.ThrowIfCancellationRequested();

        var entity = new BookingEntity
        {
            OwnerUserId = ownerUserId,
            Status = "Pending",
            StartTime = request.StartTime!.Value,

        };

        if (await HasSchedulingConflictAsync(
        request.BookingDate!.Value,
        request.StartTime!.Value,
        request.Hours!.Value,
        bookingIdToExclude: entity.Id,
        cancellationToken))
{
    throw new BookingConflictException();
}

        ApplyRequest(entity, request);

        _db.Bookings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return MapEntity(entity);
    }

    public async Task<BookingResponse?> UpdateAsync(
        int id,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await OwnedBookings()
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        EnsureValid(request);
        ApplyRequest(entity, request);

        await _db.SaveChangesAsync(cancellationToken);

        return MapEntity(entity);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await OwnedBookings()
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _db.Bookings.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
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

    private static void ApplyRequest(
        BookingEntity entity,
        CreateBookingRequest request)
    {
        entity.CustomerName = request.CustomerName!.Trim();
        entity.PackageName = request.PackageName!.Trim();
        entity.BookingDate = request.BookingDate!.Value;
        entity.StartTime = request.StartTime!.Value;

        entity.HourlyRate = request.HourlyRate!.Value;
        entity.Hours = request.Hours!.Value;
        entity.DiscountPercent = request.DiscountPercent!.Value;
        entity.BookingFee = request.BookingFee!.Value;
    }

    private static BookingResponse MapEntity(BookingEntity entity)
    {
        return new BookingResponse
        {
            Id = entity.Id,
            CustomerName = entity.CustomerName,
            PackageName = entity.PackageName,
            BookingDate = entity.BookingDate,
            StartTime = entity.StartTime,
            HourlyRate = entity.HourlyRate,
            Hours = entity.Hours,
            DiscountPercent = entity.DiscountPercent,
            BookingFee = entity.BookingFee,
            Status = entity.Status
        };
    }
}
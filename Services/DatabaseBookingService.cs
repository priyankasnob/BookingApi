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

    public DatabaseBookingService(BookingDbContext db)
    {
        _db = db;
    }

    private static readonly Expression<
        Func<BookingEntity, BookingResponse>> ToResponse =
        entity => new BookingResponse
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

    public async Task<BookingResponse[]> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Bookings
            .AsNoTracking()
            .OrderBy(entity => entity.Id)
            .Select(ToResponse)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<BookingResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.Bookings
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(ToResponse)
            .FirstOrDefaultAsync(cancellationToken);
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
            _db.Bookings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            string nameFilter =
                customerName.Trim().ToLowerInvariant();

            query = query.Where(entity =>
                entity.CustomerName.ToLower().Contains(nameFilter));
        }

        if (!string.IsNullOrWhiteSpace(status))
{
    string statusFilter =
        status.Trim().ToLowerInvariant() switch
        {
            "pending" => "Pending",
            "confirmed" => "Confirmed",
            "cancelled" => "Cancelled",
            _ => throw new ArgumentException(
                "Unsupported booking status.",
                nameof(status))
        };

    query = query.Where(entity =>
        entity.Status == statusFilter);
}

        int totalCount =
            await query.CountAsync(cancellationToken);

        long skip = ((long)page - 1) * pageSize;

        BookingResponse[] items;

        if (skip >= totalCount)
        {
            items = Array.Empty<BookingResponse>();
        }
        else
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

    public async Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        EnsureValid(request);

        cancellationToken.ThrowIfCancellationRequested();

        var entity = new BookingEntity
        {
            Status = "Pending"
        };

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
        EnsureValid(request);

        BookingEntity? entity = await _db.Bookings
            .FirstOrDefaultAsync(
                booking => booking.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        ApplyRequest(entity, request);

        await _db.SaveChangesAsync(cancellationToken);

        return MapEntity(entity);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        BookingEntity? entity = await _db.Bookings
            .FirstOrDefaultAsync(
                booking => booking.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _db.Bookings.Remove(entity);

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ApplyRequest(
        BookingEntity entity,
        CreateBookingRequest request)
    {
        entity.CustomerName = request.CustomerName!.Trim();
        entity.PackageName = request.PackageName!.Trim();
        entity.BookingDate = request.BookingDate!.Value;
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
            HourlyRate = entity.HourlyRate,
            Hours = entity.Hours,
            DiscountPercent = entity.DiscountPercent,
            BookingFee = entity.BookingFee,
            Status = entity.Status
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
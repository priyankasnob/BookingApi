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

    public BookingResponse[] GetAll()
    {
        return _db.Bookings
            .AsNoTracking()
            .OrderBy(entity => entity.Id)
            .Select(ToResponse)
            .ToArray();
    }

    public BookingResponse? GetById(int id)
    {
        return _db.Bookings
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(ToResponse)
            .FirstOrDefault();
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

        IQueryable<BookingEntity> query =
            _db.Bookings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            string nameFilter = customerName.Trim().ToLowerInvariant();

            query = query.Where(entity =>
                entity.CustomerName.ToLower().Contains(nameFilter));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            string statusFilter = status.Trim().ToLowerInvariant();

            query = query.Where(entity =>
                entity.Status.ToLower() == statusFilter);
        }

        int totalCount = query.Count();

        long skip = ((long)page - 1) * pageSize;

        BookingResponse[] items = skip >= totalCount
            ? Array.Empty<BookingResponse>()
            : query
                .OrderBy(entity => entity.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .Select(ToResponse)
                .ToArray();

        return new PagedBookingsResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public BookingResponse Create(CreateBookingRequest request)
    {
        EnsureValid(request);

        var entity = new BookingEntity
        {
            Status = "Pending"
        };

        ApplyRequest(entity, request);

        _db.Bookings.Add(entity);
        _db.SaveChanges();

        return MapEntity(entity);
    }

    public BookingResponse? Update(
        int id,
        CreateBookingRequest request)
    {
        EnsureValid(request);

        BookingEntity? entity = _db.Bookings.Find(id);

        if (entity is null)
        {
            return null;
        }

        ApplyRequest(entity, request);

        _db.SaveChanges();

        return MapEntity(entity);
    }

    public bool Delete(int id)
    {
        BookingEntity? entity = _db.Bookings.Find(id);

        if (entity is null)
        {
            return false;
        }

        _db.Bookings.Remove(entity);
        _db.SaveChanges();

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
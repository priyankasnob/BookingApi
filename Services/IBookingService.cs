using BookingApi.Models;

namespace BookingApi.Services;

public interface IBookingService
{
    Task<BookingResponse[]> GetAllAsync(
        CancellationToken cancellationToken);

    Task<PagedBookingsResponse> SearchAsync(
        string? customerName,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<BookingResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken);

    Task<BookingResponse?> UpdateAsync(
        int id,
        CreateBookingRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}
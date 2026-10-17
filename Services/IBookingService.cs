using BookingApi.Models;

namespace BookingApi.Services;

public interface IBookingService
{
    BookingResponse[] GetAll();

    PagedBookingsResponse Search(
        string? customerName,
        string? status,
        int page,
        int pageSize);

    BookingResponse? GetById(int id);

    BookingResponse Create(CreateBookingRequest request);

    BookingResponse? Update(
        int id,
        CreateBookingRequest request);

    bool Delete(int id);
}
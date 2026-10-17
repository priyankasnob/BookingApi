using BookingApi.Models;
using BookingApi.Services;
using BookingApi.Validation;
using BookingApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("BookingDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'BookingDatabase' is missing.");

builder.Services.AddDbContext<BookingDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<
    IBookingService,
    DatabaseBookingService>();

    builder.Services.AddProblemDetails();

    builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "Booking API is running"
    });
});

app.MapGet(
    "/bookings",
    (IBookingService bookingService,
     string? customerName,
     string? status,
     int? page,
     int? pageSize) =>
{
    int requestedPage = page ?? 1;
    int requestedPageSize = pageSize ?? 10;

    string? normalizedCustomerName =
        string.IsNullOrWhiteSpace(customerName)
            ? null
            : customerName.Trim();

    string? normalizedStatus =
        string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

    var errors = new Dictionary<string, string[]>();

    if (requestedPage < 1)
    {
        errors["page"] =
            new[] { "Page must be at least 1." };
    }

    if (requestedPageSize < 1 || requestedPageSize > 100)
    {
        errors["pageSize"] =
            new[] { "Page size must be between 1 and 100." };
    }

    string[] allowedStatuses =
    {
        "Pending",
        "Confirmed",
        "Cancelled"
    };

    if (normalizedStatus is not null &&
        !allowedStatuses.Any(allowed =>
            string.Equals(
                allowed,
                normalizedStatus,
                StringComparison.OrdinalIgnoreCase)))
    {
        errors["status"] =
            new[]
            {
                "Status must be Pending, Confirmed, or Cancelled."
            };
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    PagedBookingsResponse result = bookingService.Search(
        normalizedCustomerName,
        normalizedStatus,
        requestedPage,
        requestedPageSize);

    return Results.Ok(result);
})

.WithName("SearchBookings")
.WithSummary("Filter and paginate bookings")
.WithTags("Bookings")
.Produces<PagedBookingsResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem();

app.MapGet(
    "/bookings/{id:int}",
    (int id, IBookingService bookingService) =>
{
    BookingResponse? booking = bookingService.GetById(id);

    if (booking is null)
    {
        return Results.Problem(
    statusCode: StatusCodes.Status404NotFound,
    title: "Booking not found",
    detail: $"Booking {id} was not found.");
    }

    return Results.Ok(booking);
})

.WithName("GetBookingById")
.WithSummary("Get a booking by its ID")
.WithTags("Bookings")
.Produces<BookingResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound);


app.MapPost(
    "/bookings",
    (CreateBookingRequest request, IBookingService bookingService) =>
{
    var errors = CreateBookingValidator.Validate(request);

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    BookingResponse booking = bookingService.Create(request);

    app.Logger.LogInformation(
    "Created booking {BookingId} for date {BookingDate}.",
    booking.Id,
    booking.BookingDate);

    return Results.Created(
        $"/bookings/{booking.Id}",
        booking);
})

.WithName("CreateBooking")
.WithSummary("Create a booking")
.WithTags("Bookings")
.Produces<BookingResponse>(StatusCodes.Status201Created)
.ProducesValidationProblem();

app.MapPut(
    "/bookings/{id:int}",
    (int id,
     CreateBookingRequest request,
     IBookingService bookingService) =>
{
    var errors = CreateBookingValidator.Validate(request);

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    BookingResponse? booking =
        bookingService.Update(id, request);

    if (booking is null)
    {
        return Results.Problem(
    statusCode: StatusCodes.Status404NotFound,
    title: "Booking not found",
    detail: $"Booking {id} was not found.");
    }

    return Results.Ok(booking);
})

.WithName("UpdateBooking")
.WithSummary("Replace a booking's editable details")
.WithTags("Bookings")
.Produces<BookingResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status404NotFound);


app.MapDelete(
    "/bookings/{id:int}",
    (int id, IBookingService bookingService) =>
{
    bool deleted = bookingService.Delete(id);

    if (!deleted)
    {
        return Results.Problem(
    statusCode: StatusCodes.Status404NotFound,
    title: "Booking not found",
    detail: $"Booking {id} was not found.");
    }

    app.Logger.LogInformation(
    "Deleted booking {BookingId}.",
    id);

    return Results.NoContent();
})

.WithName("DeleteBooking")
.WithSummary("Delete a booking")
.WithTags("Bookings")
.Produces(StatusCodes.Status204NoContent)
.ProducesProblem(StatusCodes.Status404NotFound);




    
app.Run();


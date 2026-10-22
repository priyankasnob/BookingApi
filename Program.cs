using BookingApi.Models;
using BookingApi.Services;
using BookingApi.Validation;
using BookingApi.Data;
using Microsoft.EntityFrameworkCore;

using BookingApi.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;



var builder = WebApplication.CreateBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("BookingDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'BookingDatabase' is missing.");

builder.Services.AddDbContext<BookingDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 12;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        // Local learning setup: no email delivery configured yet.
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<BookingDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddScoped<
    IBookingService,
    DatabaseBookingService>();

    builder.Services.AddProblemDetails();

    builder.Services.AddOpenApi();

    builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseStatusCodePages();


app.UseAuthentication();
app.UseAuthorization();

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
    async (IBookingService bookingService,
       string? customerName,
       string? status,
       int? page,
       int? pageSize,
       CancellationToken cancellationToken) =>
    
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

    PagedBookingsResponse result =
    await bookingService.SearchAsync(
        normalizedCustomerName,
        normalizedStatus,
        requestedPage,
        requestedPageSize,
        cancellationToken);

    return Results.Ok(result);
})

.WithName("SearchBookings")
.WithSummary("Filter and paginate bookings")
.WithTags("Bookings")
.Produces<PagedBookingsResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem()
.RequireAuthorization();



app.MapGet(
    "/bookings/{id:int}",
    async (
        int id,
        IBookingService bookingService,
        CancellationToken cancellationToken) =>
{
    BookingResponse? booking =
        await bookingService.GetByIdAsync(
            id,
            cancellationToken);

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
.ProducesProblem(StatusCodes.Status404NotFound)
.RequireAuthorization();


app.MapPost(
    "/bookings",
    async (
        CreateBookingRequest request,
        IBookingService bookingService,
        CancellationToken cancellationToken) =>
{
    var errors = CreateBookingValidator.Validate(request);

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    BookingResponse booking =
        await bookingService.CreateAsync(
            request,
            cancellationToken);

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
.ProducesValidationProblem()
.RequireAuthorization();


app.MapPut(
    "/bookings/{id:int}",
    async (
        int id,
        CreateBookingRequest request,
        IBookingService bookingService,
        CancellationToken cancellationToken) =>
{
    var errors = CreateBookingValidator.Validate(request);

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    BookingResponse? booking =
        await bookingService.UpdateAsync(
            id,
            request,
            cancellationToken);

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
.ProducesProblem(StatusCodes.Status404NotFound)
.RequireAuthorization();

app.MapDelete(
    "/bookings/{id:int}",
    async (
        int id,
        IBookingService bookingService,
        CancellationToken cancellationToken) =>
{
    bool deleted =
        await bookingService.DeleteAsync(
            id,
            cancellationToken);

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
.ProducesProblem(StatusCodes.Status404NotFound)
.RequireAuthorization();

 app.MapGroup("/auth")
    .MapIdentityApi<ApplicationUser>();  

    app.MapGet("/auth/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        userId = user.FindFirstValue(ClaimTypes.NameIdentifier),
        email = user.FindFirstValue(ClaimTypes.Email)
    });
})
.RequireAuthorization()
.WithName("GetCurrentUser")
.WithTags("Authentication")
.WithSummary("Returns the authenticated user's ID and email.");

app.Run();


using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingApi.Data;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingApi.Tests;

public sealed class BookingEndpointsTests
    : IClassFixture<BookingApiFactory>
{
    private readonly BookingApiFactory _factory;

    public BookingEndpointsTests(BookingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetBookings_WithoutAuthentication_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/bookings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Users_CannotReadEachOthersBookings()
    {
        using var maya = await CreateAuthenticatedClientAsync("maya");
        using var jordan = await CreateAuthenticatedClientAsync("jordan");

        var created = await maya.PostAsJsonAsync(
            "/bookings",
            CreateBookingRequestFor(
                GetUniqueFutureDate(),
                new TimeOnly(10, 0),
                hours: 2));

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var mayaBooking =
            await created.Content.ReadFromJsonAsync<BookingResponse>();

        Assert.NotNull(mayaBooking);

        var mayaCanRead = await maya.GetAsync(
            $"/bookings/{mayaBooking.Id}");

        var jordanCannotRead = await jordan.GetAsync(
            $"/bookings/{mayaBooking.Id}");

        Assert.Equal(HttpStatusCode.OK, mayaCanRead.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, jordanCannotRead.StatusCode);
    }

    [Fact]
    public async Task BookingLists_ContainOnlyTheCurrentUsersBookings()
    {
        using var maya = await CreateAuthenticatedClientAsync("maya");
        using var jordan = await CreateAuthenticatedClientAsync("jordan");

        var date = GetUniqueFutureDate();

        var mayaCreated = await maya.PostAsJsonAsync(
            "/bookings",
            CreateBookingRequestFor(date, new TimeOnly(9, 0), 1));

        var jordanCreated = await jordan.PostAsJsonAsync(
            "/bookings",
            CreateBookingRequestFor(date, new TimeOnly(10, 0), 1));

        var mayaBooking =
            await mayaCreated.Content.ReadFromJsonAsync<BookingResponse>();

        var jordanBooking =
            await jordanCreated.Content.ReadFromJsonAsync<BookingResponse>();

        Assert.NotNull(mayaBooking);
        Assert.NotNull(jordanBooking);

        var mayaList = await maya.GetFromJsonAsync<PagedBookingsResponse>(
            "/bookings?page=1&pageSize=100");

        Assert.NotNull(mayaList);

        Assert.Contains(
            mayaList.Items,
            item => item.Id == mayaBooking.Id);

        Assert.DoesNotContain(
            mayaList.Items,
            item => item.Id == jordanBooking.Id);
    }

    [Fact]
    public async Task OverlappingBookings_Return409Conflict()
    {
        using var maya = await CreateAuthenticatedClientAsync("maya");
        using var jordan = await CreateAuthenticatedClientAsync("jordan");

        var date = GetUniqueFutureDate();

        var firstBooking = await maya.PostAsJsonAsync(
            "/bookings",
            CreateBookingRequestFor(date, new TimeOnly(10, 0), 2));

        Assert.Equal(HttpStatusCode.Created, firstBooking.StatusCode);

        var overlappingBooking = await jordan.PostAsJsonAsync(
            "/bookings",
            CreateBookingRequestFor(date, new TimeOnly(11, 0), 1));

        Assert.Equal(HttpStatusCode.Conflict, overlappingBooking.StatusCode);
    }

    [Fact]
    public async Task InvalidHours_Return400BadRequest()
    {
        using var maya = await CreateAuthenticatedClientAsync("maya");

        var invalidRequest = CreateBookingRequestFor(
            GetUniqueFutureDate(),
            new TimeOnly(14, 0),
            hours: 0);

        var response = await maya.PostAsJsonAsync(
            "/bookings",
            invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostgreSql_RejectsInvalidHours_WhenValidationIsBypassed()
    {
        using var maya = await CreateAuthenticatedClientAsync("maya");

        var created = await maya.PostAsJsonAsync(
            "/bookings",
            CreateBookingRequestFor(
                GetUniqueFutureDate(),
                new TimeOnly(15, 0),
                hours: 1));

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var booking =
            await created.Content.ReadFromJsonAsync<BookingResponse>();

        Assert.NotNull(booking);

        using var scope = _factory.Services.CreateScope();

        var database = scope.ServiceProvider
            .GetRequiredService<BookingDbContext>();

        var entity = await database.Bookings.SingleAsync(
            item => item.Id == booking.Id);

        entity.Hours = 0;

        await Assert.ThrowsAsync<DbUpdateException>(
            () => database.SaveChangesAsync());
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(
        string userName)
    {
        var client = _factory.CreateClient();

        var email = $"{userName}-{Guid.NewGuid():N}@example.com";
        const string password = "TestingOnly!2026";

        var registration = await client.PostAsJsonAsync(
            "/auth/register",
            new
            {
                email,
                password
            });

        Assert.Equal(HttpStatusCode.OK, registration.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/auth/login?useCookies=false&useSessionCookies=false",
            new
            {
                email,
                password
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var token = await login.Content
            .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token.AccessToken);

        return client;
    }

    private static CreateBookingRequest CreateBookingRequestFor(
        DateOnly bookingDate,
        TimeOnly startTime,
        int hours)
    {
        return new CreateBookingRequest
        {
            CustomerName = "Automated Test Customer",
            PackageName = "Portrait",
            BookingDate = bookingDate,
            StartTime = startTime,
            HourlyRate = 125,
            Hours = hours,
            DiscountPercent = 0,
            BookingFee = 20
        };
    }

    private static DateOnly GetUniqueFutureDate()
    {
        return DateOnly.FromDateTime(
            DateTime.UtcNow.Date.AddDays(
                Random.Shared.Next(100, 10000)));
    }

    private sealed record LoginResponse(
        string TokenType,
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);
}
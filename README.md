# Booking API

A learning project built with ASP.NET Core Minimal APIs and .NET 10.

## Features

- Retrieve bookings by ID.
- Filter bookings by customer name and status.
- Paginate collection results.
- Create and validate bookings.
- Update and delete bookings.
- Separate booking operations through an injected service.
- Return Problem Details error responses.
- Record creation and deletion logs.
- Generate an OpenAPI document.

## Requirements

- .NET 10 SDK
- Postman desktop application for manual API testing

## Run the Application

Open a terminal in the folder containing `BookingApi.csproj`.

Build:

```bash
dotnet build
```

Run in Development on macOS:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --no-launch-profile -- --urls http://localhost:5050
```

Base URL: `http://localhost:5050`

Stop the application with Control + C.

## Endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/` | API status |
| GET | `/bookings` | Filtered and paginated bookings |
| GET | `/bookings/{id}` | Retrieve one booking |
| POST | `/bookings` | Create a booking |
| PUT | `/bookings/{id}` | Replace editable booking details |
| DELETE | `/bookings/{id}` | Delete a booking |

## Filtering and Pagination

Supported query parameters:

- `customerName`: Case-insensitive partial name match.
- `status`: Pending, Confirmed, or Cancelled; case-insensitive.
- `page`: Defaults to 1; must be at least 1.
- `pageSize`: Defaults to 10; must be between 1 and 100.

Example:

`GET /bookings?status=Confirmed&page=1&pageSize=2`

The collection response includes `items`, `page`, `pageSize`, `totalCount`, and `totalPages`.

## Create or Update a Booking

Use Content-Type `application/json` and supply:

```json
{
  "customerName": "Priyanka",
  "packageName": "Portrait",
  "bookingDate": "2026-11-20",
  "hourlyRate": 125,
  "hours": 2,
  "discountPercent": 10,
  "bookingFee": 20
}
```

Hours must be between 1 and 8. Customer name, package name, date, and numeric fields are required. Pricing limits are defined in `CreateBookingValidator`.

The server generates booking IDs, initializes new bookings as Pending, and calculates the total price. PUT preserves the existing ID and status.

## Responses

- 200: Successful retrieval or update.
- 201: Booking created; Location identifies its URL.
- 204: Booking deleted; response body is empty.
- 400: Invalid input.
- 404: Booking or route not found.
- 500: Unexpected server failure.

## OpenAPI

When running in Development, retrieve:

`http://localhost:5050/openapi/v1.json`

This is an OpenAPI JSON document. An interactive documentation UI is not installed.

## Project Structure

- `Program.cs`: Application configuration, routes, HTTP responses, and endpoint metadata.
- `Models`: Request and response models.
- `Services`: Booking interface and in-memory implementation.
- `Validation`: Booking input validation.
- `postman`: Exported Postman collection.

## Manual Testing

Import `postman/BookingApi.postman_collection.json` into Postman.

Start with a fresh application instance. The sample bookings have IDs 1, 2, and 3.

Create a booking, note its returned ID, and use that ID for subsequent GET, PUT, and DELETE requests. Saved request IDs may need updating.

## Current Limitations

- Data is stored in memory and resets when the application restarts.
- PostgreSQL integration has not been implemented.
- Authentication and authorization have not been implemented.
- Pricing inputs are supplied by the client for learning purposes.
- Filtering and pagination process an in-memory snapshot.
- The singleton service shares data only within one application instance.

## Learning Topics

HTTP methods, JSON, validation, status codes, interfaces, dependency injection, service lifetimes, filtering, pagination, logging, error handling, and OpenAPI.




## Authentication and booking access

BookingApi uses ASP.NET Core Identity with PostgreSQL.

- Register: `POST /auth/register`
- Sign in: `POST /auth/login?useCookies=false&useSessionCookies=false`
- Confirm the current signed-in account: `GET /auth/me`
- Booking routes require a bearer token.
- Users can access only bookings they own.
- Requests for another user's booking return `404 Not Found`.

Never commit passwords, access tokens, refresh tokens, or PostgreSQL connection strings.

## Booking schedule rules

- New bookings require `bookingDate`, `startTime`, and `hours`.
- A booking must end by midnight.
- Pending and confirmed bookings block overlapping appointments.
- Adjacent appointments are allowed.
- Overlapping requests return `409 Conflict`.
- The application performs availability checking before save. A future improvement is database-level protection for simultaneous conflicting requests.

## Tests

The API test project is located at:

`tests/BookingApi.Tests`

Tests use the separate PostgreSQL database `booking_api_tests`.

Set the local test connection before testing:

```bash
export BOOKING_API_TEST_CONNECTION='Host=localhost;Port=5432;Database=booking_api_tests;Username=postgres;Password=postgrey'
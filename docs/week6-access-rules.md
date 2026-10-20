# Week 6: Authentication and Access Rules

## Concepts

- Authentication establishes who made the request.
- Authorization decides whether that user may perform an action.
- Ownership links a booking to its user account.
- CustomerName is a business field, not proof of identity.

## Current behavior

GET /bookings without authentication returned:

GET /bookings/{id} without authentication returned:

## Target behavior

- Booking endpoints require authentication.
- The server assigns ownership using the authenticated user.
- Customers can list only their own bookings.
- Customers can retrieve, update, and delete only their own bookings.
- Another customer's booking returns 404.
- Filtering and pagination operate within the user's bookings.
- TotalCount includes only matching bookings owned by that user.

## Existing bookings

Existing records need an explicit ownership migration strategy.
Customer names must not be used as proof of ownership.

## Future test checklist

- [ ] Unauthenticated access returns 401.
- [ ] Maya can create and retrieve her own booking.
- [ ] Jordan cannot retrieve Maya's booking.
- [ ] Jordan cannot update Maya's booking.
- [ ] Jordan cannot delete Maya's booking.
- [ ] Jordan's booking list does not include Maya's booking.
- [ ] Pagination metadata does not count another user's bookings.
- [ ] Request-body values cannot override server-assigned ownership.
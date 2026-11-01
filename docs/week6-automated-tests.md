# Week 6: Automated Tests

## Test database
- Database name: booking_api_tests
- The test database is separate from booking_api.
- Test credentials are supplied through a local environment variable.
- Passwords, connection strings, and access tokens are not committed.

## Automated checks
- [ ] Anonymous booking requests return 401.
- [ ] A user can read their own booking.
- [ ] A different user receives 404 for that booking.
- [ ] Each user's list excludes other users' bookings.
- [ ] Overlapping appointments return 409.
- [ ] Invalid hours return 400.
- [ ] PostgreSQL rejects invalid hours when API validation is bypassed.

## Result
- Number of passing tests:
- Problems found and fixes:
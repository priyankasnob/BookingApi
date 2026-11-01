# Week 6 Final Review

## Authentication
- [ x] Anonymous /auth/me returns 401.
- [ x] Valid Maya token returns Maya's identity.
- [ x] Invalid token returns 401.

## Booking ownership
- [ x] Anonymous booking access returns 401.
- [ x] Maya can create and read her own booking.
- [ x] Jordan receives 404 for Maya's booking.
- [ x] Maya's booking list excludes Jordan's bookings.
- [ x] Jordan's filtered count does not reveal Maya's booking.

## Booking reliability
- [ x] Overlapping appointment returns 409.
- [ x] Adjacent appointment is accepted.
- [ x] Invalid end time returns 400.
- [ x] Rejected overlap is absent from PostgreSQL.
- [ x] An owner can delete their own booking.
- [ x] Another user cannot delete that booking.

## Delivery
- [ x] dotnet build succeeds.
- [ x] Automated tests pass.
- [ x] OpenAPI returns 200 in Development.
- [ x] Postman collection contains Week 6 requests.
- [x ] No tokens, passwords, or connection strings are committed.
- [ x] README documents authentication, ownership, scheduling, and testing.

## Known limitation
The availability check happens in application code before save.
A future production improvement is database-level conflict protection for
simultaneous requests.
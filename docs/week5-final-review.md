# Week 5 Database Review

Review date:10/17/26
Practice booking ID:9

## Build and migrations
- [ x] The project builds successfully.
- [ x] The expected migrations appear in the database history.

## Database behavior
- [ x] POST returns 201 and the expected price of 245.
- [ x] The created booking is visible in PostgreSQL.
- [ x] GET retrieves the booking after an API restart.
- [ x] PUT updates hours to 3 and total price to 357.50.
- [ x] PostgreSQL contains the updated values.
- [ x] Invalid PUT returns 400 and leaves saved values unchanged.
- [ x] PostgreSQL rejects zero hours directly.
- [ x] The direct SQL experiment was rolled back.

## Queries and responses
- [ x] Filtering returns matching bookings.
- [ x] Pagination returns the expected items and metadata.
- [ x] Invalid pagination returns 400.
- [ x] OpenAPI returns 200 in Development.
- [ x] Unknown and removed debug routes return 404.

## Deletion
- [ x] DELETE returns 204.
- [ x] Repeated GET and DELETE return 404.
- [ x] The deleted row is absent from PostgreSQL.
- [ x] The booking remains absent after restarting the API.

## Delivery
- [ x] Postman collection exported.
- [ x] README updated.
- [ x] Changes committed and pushed.
- [ x] GitHub changes reviewed.

## Problems found and fixes
Record any failures and how they were resolved.
# Week 6: Booking Ownership

## Design
- New bookings receive the authenticated user's ID.
- OwnerUserId is not part of the booking request model.
- Booking queries filter by the current user.
- Counts and pagination use the ownership-filtered query.
- Other users' bookings return 404.
- Legacy bookings have no owner and remain hidden.

## Verification
- [ ] Anonymous booking access returns 401.
- [ ] Maya can retrieve her own booking.
- [ ] Jordan can retrieve his own booking.
- [ ] Cross-user reads return 404.
- [ ] Cross-user updates return 404 and leave data unchanged.
- [ ] Cross-user deletes return 404 and leave data unchanged.
- [ ] Each user's list contains only their bookings.
- [ ] Filtered counts exclude other users' bookings.
- [ ] Request-body owner values cannot override server ownership.
- [ ] Owners can update and delete their own bookings.
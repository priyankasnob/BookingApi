# Week 6: Booking Conflicts

## Scheduling rules
- New bookings require a start time.
- A booking must end by midnight.
- Pending and confirmed bookings block overlapping appointments.
- Cancelled bookings do not block appointments.
- Adjacent appointments are allowed.
- Conflicts are checked across all users.
- Conflict responses do not reveal another user's booking details.

## Verification
- [ ] A booking with a start time is created successfully.
- [ ] An overlapping booking returns 409.
- [ ] An adjacent booking is accepted.
- [ ] A booking ending after midnight returns 400.
- [ ] Rejected overlaps are not stored in PostgreSQL.
- [ ] Existing bookings without start times remain unchanged.

## Limitation
The application checks availability before saving. A later improvement should add
database-level protection against two simultaneous overlapping requests.
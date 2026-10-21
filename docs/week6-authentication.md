# Week 6: Registration and Sign-in

## Implementation
- Added ASP.NET Core Identity with PostgreSQL storage.
- Created ApplicationUser.
- Added the AddIdentityAccounts migration.
- Added registration and login routes under /auth.
- Protected /auth/me with RequireAuthorization.

## Verification
- [ x] Registration succeeds.
- [ x] Correct credentials return an access token.
- [ x] A valid access token allows access to /auth/me.
- [ x] Wrong passwords return 401.
- [ x] Invalid tokens return 401.
- [x ] Maya and Jordan have different user IDs.
- [ x] Accounts remain available after restarting the API.

## Remaining work
Booking ownership protection will be added in Lesson 3.
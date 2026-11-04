# ADR-005: Initial Authentication and Session Security

| Field | Value |
|---|---|
| Status | Accepted and implemented on `feat/backend-core-api`, with production hardening pending |
| Date | 2026-10-29 |
| Scope | API access tokens, refresh sessions and technical account persistence |

## Context

Core customer, contract, project and warehouse endpoints need an authentication boundary. The business-role document is approved but defines organizational responsibilities, not endpoint-by-endpoint software permissions. The full business-domain User aggregate also remains under review.

## Decision

- Use a separate technical account model (`UserAccounts`) and refresh-token table. Do not treat it as the final personnel/User domain aggregate.
- Normalize and uniquely index email; store a password hash using ASP.NET Core's password hasher; never store raw passwords.
- Issue short-lived HMAC-SHA256 JWT access tokens and random refresh tokens. Store only SHA-256 hashes of refresh tokens.
- Rotate refresh tokens on successful refresh and revoke them on logout. A reused/revoked/expired token is rejected. Reusing a token that had already been rotated is treated as a replay signal and revokes the account's active refresh sessions and increments the JWT token version.
- Temporarily lock an account for 15 minutes after five failed password attempts. Missing-user and wrong-password responses are generic.
- Disable public self-registration. Create users through an endpoint protected by the SystemAdministrator role. Bootstrap the first administrator only when the operator sets explicit environment configuration.
- Read signing key and bootstrap credentials from environment variables or .NET user-secrets, never committed appsettings secrets.
- Until a role permission matrix is approved, all existing operational controllers and the audit endpoint require SystemAdministrator. Other role names come from `docs/02_business/02_BusinessRoles.md` but are not granted speculative endpoint access.

## Consequences and limits

- The initial login/session boundary is implemented and tested, but is not yet the full identity/personnel module.
- MFA, email verification, password reset/recovery, user deactivation/re-activation endpoints, refresh-session administration, distributed rate limiting and a complete security-event audit stream remain future work.
- Logout revokes the refresh token; already issued access tokens can remain valid until their short expiry.
- The frontend currently keeps tokens in memory and requires a new login after a reload. Production should adopt a reviewed browser-session pattern; do not persist bearer/refresh tokens in localStorage as a shortcut.
- The development bootstrap setup is for local use only and must not be used with shared or production credentials.

## Verification

The integration suite covers login, current-user access, generic authentication failures, failed-login lockout, refresh-token rotation/replay rejection, logout/revocation, anonymous rejection on protected routes and administrator-only user creation/operational access. The latest confirmed green backend run on the active feature line passes 78 tests; the newest audit-query/API validation update is undergoing a separate full CI run.

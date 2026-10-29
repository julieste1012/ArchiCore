# ADR-005: Initial Authentication and Session Security

| Field | Value |
|---|---|
| Status | Accepted for the backend foundation |
| Date | 2026-10-29 |
| Scope | API access tokens, refresh sessions and technical account persistence |

## Context

The core API exposes customer, contract, project and warehouse operations. The project has approved business-role definitions, but the final permission matrix and the domain boundaries for the full User aggregate remain unresolved. Anonymous access to operational endpoints is not acceptable.

## Decision

- Introduce a minimal infrastructure authentication account model for credentials and sessions. It is not a replacement for the future business-domain User aggregate; that aggregate must be reconciled against the domain model before broader personnel workflows are implemented.
- Persist email (normalized and unique), display name, password hash, one approved business-role name, activation state, timestamps, failed-login counter and temporary lockout.
- Use ASP.NET Core's password hasher rather than storing or comparing raw passwords. Enforce a strong minimum password policy on account creation and bootstrap.
- Issue signed short-lived JWT access tokens and random refresh tokens. Persist only SHA-256 hashes of refresh tokens. Rotate refresh tokens on every successful refresh and revoke them on logout.
- Apply a five-failure temporary lockout to reduce online password guessing; authentication failure responses must not disclose whether an email exists.
- Disable public self-registration. Account creation is restricted to an authenticated System Administrator. Bootstrap the first administrator only when explicit environment configuration is supplied.
- Read JWT signing material and bootstrap credentials from environment/user-secrets, never commit secrets to appsettings.
- Until a permission matrix derived from the approved responsibilities is documented, core operational controllers require the SystemAdministrator role. Do not infer the final permissions for the other business roles from their job descriptions.

## Database changes

Add `UserAccounts` and `RefreshTokens` via a versioned EF Core migration. Account email and refresh-token hashes are unique. Refresh tokens store expiry, revoke and replacement metadata so sessions can be audited and replay rejected.

## Consequences and limits

- This delivers a secure initial authentication boundary, not a final personnel/identity module.
- Role names come from `docs/02_business/02_BusinessRoles.md`; permissions for each business module remain a documented follow-up decision.
- MFA, password reset, email verification, breached-password checks, refresh-token replay response beyond rejecting the used token, audit processing and distributed rate limiting remain future work.
- The initial lockout is database-backed but not a replacement for gateway/API rate limiting.
- The transactional outbox remains responsible only for event capture; authentication activity still needs the separate audit trail.

## Verification

Integration tests must cover bootstrap behavior, login success/failure, lockout, JWT-protected endpoints, user-creation authorization, refresh rotation/replay rejection, logout/revocation and persistence in disposable PostgreSQL.

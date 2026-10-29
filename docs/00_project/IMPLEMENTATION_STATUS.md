# Implementation Status

| Field | Value |
|---|---|
| Project | ArchiCore ERP |
| Last audit | 2026-10-29 |
| Stable branch | `main` |
| Integration target | `develop` |
| Active implementation branch | `feat/backend-core-api` (PR #7, not yet merged) |
| Current phase | Core API/security, transactional event audit, object-storage adapters and the first functional React operations slice are implemented and CI-validated |
| Next critical work | Approve documentary domain boundaries and per-role permissions; then implement document metadata, physical locations and prioritized end-to-end business workflows |

> A passing test suite validates the implemented slice, not the completeness of the ERP. This document distinguishes completed infrastructure from business workflows that still lack approved specifications.

## 1. Changes already integrated into `develop`

- **Repository hygiene:** root `.gitignore` and removal of tracked .NET build output via [PR #1](https://github.com/julieste1012/ArchiCore/pull/1).
- **Implementation status:** introduced via [PR #2](https://github.com/julieste1012/ArchiCore/pull/2).
- **Domain regression tests and CI:** integrated via [PR #3](https://github.com/julieste1012/ArchiCore/pull/3).
- **OpenAPI package security update:** `Microsoft.AspNetCore.OpenApi` upgraded to `10.0.14` via [PR #4](https://github.com/julieste1012/ArchiCore/pull/4).
- **Runtime standardization:** .NET 10 LTS recorded in ADR-003 through [PR #6](https://github.com/julieste1012/ArchiCore/pull/6).

The current backend/security/frontend work described below is on the active feature branch until PR #7 is reviewed and merged. Do not assume that `main` or `develop` already contains it.

## 2. Current repository state on `feat/backend-core-api`

### Core API and persistence
- Solution: `src/ArchiCore.slnx`; backend targets .NET 10 (`net10.0`).
- REST endpoints and application services for Customer, Contract, Project and Warehouse, including list pagination, creation, retrieval and the lifecycle operations already defined in the current domain code.
- EF Core 10/Npgsql mappings, PostgreSQL 17 development Compose service, versioned migrations, repository implementations and `/health/live` / `/health/ready`.
- API validation errors are returned as HTTP 422 with `Api.ValidationFailed`; explicit domain error codes map to 404/409/401 as appropriate.
- Local Compose binds PostgreSQL to `127.0.0.1:5433` to avoid collision with a host PostgreSQL service listening on 5432.

### Authentication and authorization
- `UserAccounts` and `RefreshTokens` persistence, separate from the not-yet-approved personnel/domain User aggregate.
- Password hashing through ASP.NET Core's password hasher; minimum password policy; generic login failure responses; temporary 15-minute lockout after five failed attempts.
- Short-lived HMAC-SHA256 JWT access tokens. Only SHA-256 hashes of refresh tokens are stored. Refresh tokens rotate on use, are revoked on logout, and replay is rejected. Concurrent refresh-token redemption is covered by an integration test; the frontend coordinates simultaneous refresh attempts into one in-flight rotation.
- Authenticated self-service password change verifies the current password, enforces the password policy, requires the new value to differ, increments token version and revokes existing refresh sessions. Old access tokens are invalidated; the user must sign in again.
- Administrator-only paginated user listing, account creation, account activation/deactivation, including token/session revocation. The last active SystemAdministrator cannot be deactivated, and an administrator cannot deactivate their own account.
- Bootstrap administrator creation is opt-in via environment configuration. No signing key or bootstrap password is committed to source control.
- All four current operational controllers and the audit endpoint require `SystemAdministrator` because the approved business-role document defines responsibilities but does not yet provide a final endpoint permission matrix. Non-admin users see a restricted frontend screen but can still change their own password.
- This is a technical authentication boundary, not the full personnel/User domain module. MFA, password reset/recovery, login/security-event auditing, distributed rate limiting, and a final role/endpoint permission matrix remain future work.

### Object storage
- Application exposes `IObjectStorage` for storing, streaming and deleting object bytes, with a shared object-key policy that blocks traversal paths and unsafe keys.
- Development defaults to a filesystem provider under the API content root, with temporary-file writes and declared-length verification. Object data under `App_Data/Objects` is git-ignored.
- An S3-compatible adapter using the MinIO .NET SDK is available through `ObjectStorage:Provider=Minio`; endpoint, bucket, credentials and size limit are configurable via environment variables. No public file endpoint was added because file metadata, permissions, retention and business lifecycle are not yet approved.
- Important infrastructure decision: the upstream MinIO server repository was archived on 2026-04-25 and states that the community server is no longer maintained. The adapter is S3-compatible, but the archived community server is not treated as a production-ready dependency. Select a supported S3-compatible service and review licensing/security before release.

### Transactional outbox and domain-event audit
- `ArchiCoreDbContext.SaveChanges/SaveChangesAsync` persists domain events to `OutboxMessages` in the same transaction as aggregate updates, and clears the in-memory events only after successful saving.
- A hosted worker processes pending rows outside the Testing environment. PostgreSQL `FOR UPDATE SKIP LOCKED` coordinates multiple instances. Audit projection and marking the outbox row processed commit in the same transaction; failed batches remain pending and are retried on a later poll.
- `DomainEventAuditEntries` stores an immutable payload copy. `GET /api/audit/domain-events` is administrator-only and supports event-type/date filters and pagination.
- This is durable *domain-event* auditing. It is not yet a complete actor-level security audit: events do not consistently include the authenticated actor, and user login/refresh/logout actions are not recorded in a separate security-audit stream. There is no retry counter, poison-message quarantine or retention/cleanup policy yet.

### Initial frontend
- First slice lives in `src/Frontend/archicore-web`: React, TypeScript and Vite; login/session flow; API-backed dashboard; and list/create screens for customers, contracts, projects and warehouses.
- The customer, contract, project and warehouse screens expose guarded lifecycle operations already implemented in the backend. They handle loading/error/success feedback and ask for confirmation before consequential transitions.
- Project management supports assigning and releasing warehouses and displays assignment history. Counts show active assignments rather than historic rows.
- Contract management can finish an active contract using an explicit date. The UI warns that recording `Finished` does not replace documentary return, pending-loan resolution, billing and archive closure.
- SystemAdministrator can list/create/deactivate/reactivate accounts and view the paged domain-event audit with filters. All authenticated roles can change their own password; modules remain restricted to SystemAdministrator until the business permission matrix is approved.
- API calls use the shared client, which handles access-token refresh with single-flight coordination. Session and tokens stay in browser memory; a full page reload requires a new login. Do not replace this with localStorage token persistence as a shortcut.
- [API_REFERENCE.md](../06_development/API_REFERENCE.md) records current routes, request bodies, status codes, role constraints and explicit non-implemented areas.
- Frontend CI typechecks and produces a production build. Component tests, end-to-end release validation, detail/edit screens and approved non-admin operational views remain pending.

### Automated validation
- Domain/model test project: 38 tests, including the EF model test that generates PostgreSQL DDL.
- Integration suite uses `WebApplicationFactory<Program>` and an isolated PostgreSQL 17 Testcontainers database. It applies the real migrations and does not use the developer's Docker Compose volume.
- Coverage includes core API flows and associations; HTTP 404/409/422 error mapping; lifecycle transitions; object-storage safety and size checks; JWT authentication and role boundaries; refresh-token rotation/replay/concurrency; password change and token revocation; user administration; outbox persistence/rollback; audit projection; and database isolation.
- Latest backend CI on the current code/documentation head: [96 passed, 0 failed, 0 skipped](https://github.com/julieste1012/ArchiCore/actions/runs/40969300019) (38 domain/model + 58 integration). Restore and Release build succeeded without warnings.
- Latest frontend CI on the same head: [Typecheck and production build succeeded](https://github.com/julieste1012/ArchiCore/actions/runs/40969300022).
- These results validate the implemented slice only. They do not establish that all business processes of the ERP have been implemented or approved.

## 3. Domain and requirements blockers

Do not implement documentary persistence based on contradictory draft documents. These items require a documented decision or an approved specification:

1. **Archive / Folder / Box boundaries:** `DOMAIN_REVIEW.md` and the draft aggregate specifications disagree about which are aggregate roots and which are internal entities.
2. **Box-to-location relationship:** `Warehouse.md` describes an internal `WarehouseLocation`, but its fields and movement rules are not formalized.
3. **Domain ownership map:** domain documents differ on the aggregate-root list, including User, Loan and Archive.
4. **Contract/project cardinality:** `03_OrganizationalStructure.md` says one project per contract while the contract specification permits multiple projects.
5. **Contract cancellation/event catalog:** `ContractStatus` contains Cancelled, but no `Cancel()` transition exists and event names differ between documents.
6. **Customer profile:** Customer currently stores NIT/status/contract references. The business specification leaves legal name, identity and contact fields open.
7. **Lifecycle gaps:** some Box/Warehouse states and catalogued events are not fully reachable or emitted by current domain methods.
8. **Business processes:** BP-001 and BP-002 are written as initial workflows, but their operational API/use-case coverage is not complete; BP-003 to BP-014 remain pending in the index.
9. **Security and audit requirements:** the technical login and domain-event audit are implemented, but the final per-role permission matrix, actor-level audit requirements, retention and incident-response policy are not approved.
10. **Object storage:** the provider abstraction and development filesystem/MinIO-SDK adapters now exist; selecting and validating a supported production storage service and approving file metadata/lifecycle remain pending.

The project collaboration protocol prohibits silently inventing business rules. Resolve these decisions before adding the Archive/Folder/Box/Document/Folio and Loan models.

## 4. Next implementation sequence

1. **Review/release the core slice:** verify current backend/frontend CI, review the diff and perform local browser/API smoke tests before merging PR #7.
2. **Approve domain boundaries:** document decisions for Archive/Folder/Box/Document/Folio/Loan, WarehouseLocation, contract/project cardinality, lifecycle transitions and the Customer profile.
3. **Approve authorization policy:** map the documented business roles to explicit read/write/actions for each endpoint. Keep current admin-only restriction until the matrix is approved and tested.
4. **Finish security/audit policy:** add credential recovery/reset, actor-level security audit, rate limiting, outbox retry/poison-message policy, retention/cleanup, monitoring and deployment secrets guidance.
5. **Documentary metadata and APIs:** after decisions are approved, design the metadata, object lifecycle, physical location and public upload/download contracts. Select a supported production S3-compatible provider and validate bucket/access policies.
6. **Business workflows:** implement BP-001/BP-002 end-to-end and then approved verification/classification/labeling, search, loans/returns, digitization, billing and contract closure.
7. **Frontend completion:** add details/edit flows, form validation polish, role-specific modules after approval, component tests and API-backed E2E tests.
8. **Deployment:** API/frontend Dockerfiles, production secret configuration, logging/metrics, backups and restore rehearsal, and migration smoke test.
9. **Completion gate:** backend is complete only when approved in-scope processes have secured endpoints, durable data, agreed audit/storage behavior and automated E2E coverage, and a clean deployment starts reliably.

## 5. Definition of backend complete

The backend is complete only when the approved in-scope workflows have server-side validation and working endpoints; PostgreSQL migrations are reproducible; permissions protect each operation; document storage and audit meet approved requirements; domain/application/API/database behavior is covered by automated tests; and a clean deployment starts reliably with documented secrets and backup/restore operations.

## 6. Git and collaboration rules

- `main` is the stable release branch; `develop` is the integration target; feature branches remain short-lived.
- Keep one logical change per commit and review through pull requests.
- Update this document after each significant implementation slice with the commit/PR and evidence.
- Architecture and business rules must be recorded before implementation. No silent assumptions.

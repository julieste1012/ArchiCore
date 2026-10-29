# Implementation Status

| Field | Value |
|---|---|
| Project | ArchiCore ERP |
| Last audit | 2026-10-29 |
| Stable branch | `main` |
| Integration target | `develop` |
| Active implementation branch | `feat/backend-core-api` (PR #7, not yet merged) |
| Current phase | Core API, PostgreSQL persistence, JWT authentication, transactional outbox/audit projection, provider-neutral object storage and the initial React frontend are implemented |
| Next critical work | Complete CI review; define approved document metadata and lifecycle; select a supported production S3-compatible service and implement the documentary workflows |

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
- `UserAccounts` and `RefreshTokens` persistence.
- Password hashing through ASP.NET Core's password hasher; password minimum policy; generic login failure responses; temporary 15-minute lockout after five failed attempts.
- Short-lived HMAC-SHA256 JWT access tokens; only SHA-256 hashes of refresh tokens are stored. Refresh tokens rotate on use and are revoked on logout; reused tokens are rejected.
- Bootstrap administrator creation is opt-in via environment configuration. No application signing key or bootstrap password is committed to source control.
- All four current operational controllers and the audit endpoint currently require the `SystemAdministrator` role because the approved business-role document defines responsibilities but no final endpoint permission matrix.
- This is a technical authentication boundary, not yet the full personnel/User domain module. MFA, password reset, security-event auditing and distributed rate limiting remain future work.

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
- First slice lives in `src/Frontend/archicore-web`: React, TypeScript and Vite with login, dashboard and pages to list/create customers, contracts, projects and warehouses.
- UI requests go to the existing API; it is not a disconnected mockup. Dashboard counts are queried from the API.
- Session tokens remain in browser memory. A full page reload requires a new login. Do not replace this with localStorage token persistence as a shortcut; production browser-session design still needs a reviewed HttpOnly-cookie/CSRF strategy.
- Frontend CI typechecks and produces a production build. Documentary modules, permission-aware role-specific UI, editing/deletion and end-to-end release validation remain pending.

### Automated validation
- Domain/model test project: 38 tests.
- Integration suite uses `WebApplicationFactory<Program>` and an isolated PostgreSQL 17 Testcontainers instance; its actual migration path is applied to the temporary database.
- Latest confirmed green backend CI before this documentation-only/configuration update: [78 tests passed, 0 failed, 0 skipped](https://github.com/julieste1012/ArchiCore/actions/runs/40965570439) (38 domain/model + 40 integration). The newer audit endpoint commit must also remain green before the PR is merged.
- Frontend production build has passed in GitHub Actions on the same feature line: [Frontend CI](https://github.com/julieste1012/ArchiCore/actions/runs/40965570536).

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

1. **Release gate for this slice:** confirm the latest backend and frontend CI runs for the current HEAD; review the diff before merging PR #7.
2. **Local setup and smoke test:** apply all migrations to the local development database; create the bootstrap administrator using local environment variables; verify login and the four core modules in the browser.
3. **Security hardening:** add explicit role/endpoint permission matrix from approved business responsibilities; add actor-level security audit; define rate limiting, credential recovery and token/session policy.
4. **Operational audit:** define retries, poison-message handling, retention, cleanup and monitoring for the domain-event processor.
5. **Object storage:** select a supported production S3-compatible service and validate its security/licensing posture; the application abstraction/adapters are in place, while file metadata and authorization contracts await approval.
6. **Domain decision record:** resolve aggregate boundaries, cardinality, WarehouseLocation, lifecycle transitions and event catalog.
7. **Documentary workflows:** implement the approved reception/counting, classification, folder/box storage, search, loans/returns, digitization, billing and contract-closure flows with integration tests.
8. **Frontend:** complete module forms, role-aware interactions and error/loading states against the agreed API; add frontend component/flow tests.
9. **Deployment:** production secrets, container health, logging/metrics, backup/restore rehearsal and migration smoke test.
10. **Completion gate:** backend is complete only after approved in-scope business processes have working secured endpoints, persistent data, audit semantics, file handling and end-to-end test coverage.

## 5. Definition of backend complete

The backend is complete only when the approved in-scope workflows have server-side validation and working endpoints; PostgreSQL migrations are reproducible; permissions protect each operation; document storage and audit meet approved requirements; domain/application/API/database behavior is covered by automated tests; and a clean deployment starts reliably with documented secrets and backup/restore operations.

## 6. Git and collaboration rules

- `main` is the stable release branch; `develop` is the integration target; feature branches remain short-lived.
- Keep one logical change per commit and review through pull requests.
- Update this document after each significant implementation slice with the commit/PR and evidence.
- Architecture and business rules must be recorded before implementation. No silent assumptions.

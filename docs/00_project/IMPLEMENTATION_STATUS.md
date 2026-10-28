# Implementation Status

| Field | Value |
|---|---|
| Project | ArchiCore ERP |
| Last audit | 2026-10-28 |
| Baseline reviewed | `d86b56aefadc374a1a657e38211fc2d2f4252dd9` |
| Integration branch | `develop` |
| Current phase | Core API/persistence, transactional outbox and automated integration coverage validated; security and documentary-domain decisions pending |
| Next critical work | Define user/role security rules, add an idempotent outbox dispatcher and durable audit trail, then implement approved documentary workflows |

> This document records observed repository state. A successful build does not mean that the ERP backend is functionally complete. Update this file after each significant phase.

## 1. Changes already integrated into `develop`

- **Repository hygiene:** root `.gitignore` added and tracked .NET `bin/` and `obj/` build output removed. See [PR #1](https://github.com/julieste1012/ArchiCore/pull/1).
- **Project status:** this file was introduced via [PR #2](https://github.com/julieste1012/ArchiCore/pull/2).
- **Domain test baseline and CI:** xUnit v3 test project and GitHub Actions workflow integrated via [PR #3](https://github.com/julieste1012/ArchiCore/pull/3).
- **OpenAPI security update:** `Microsoft.AspNetCore.OpenApi` upgraded from `10.0.9` to `10.0.14` via [PR #4](https://github.com/julieste1012/ArchiCore/pull/4). The successful restore/build/test log no longer reports NU1903 for `Microsoft.OpenApi`.
- **Automated validation:** GitHub Actions restore/build/test succeeded, with 37 domain unit tests passing. See [CI run for security update](https://github.com/julieste1012/ArchiCore/actions/runs/40450759606).

These changes are on `develop`; `main` remains the stable branch and must not be assumed to contain them yet.

## 2. Current repository state

### Solution and runtime
- Solution: `src/ArchiCore.slnx`.
- Backend projects: `ArchiCore.Domain`, `ArchiCore.Application`, `ArchiCore.Infrastructure`, `ArchiCore.Shared`, and `ArchiCore.API`.
- Test projects: `ArchiCore.Domain.Tests` and `ArchiCore.Integration.Tests`.
- All projects target .NET 10 (`net10.0`).
- Tests use Microsoft Testing Platform with xUnit v3, consistent with `global.json`.
- Local development uses PostgreSQL 17 through Docker Compose. The Compose host binding is `127.0.0.1:5433 -> container 5432` to avoid conflicts with a Windows-host PostgreSQL service. This setting is development-only.

### Implemented and manually validated
- Domain primitives, value objects, and the current Customer, Contract, Project, Warehouse and Box domain model.
- Core application services, DTOs and repositories for Customer, Contract, Project and Warehouse.
- PostgreSQL persistence via EF Core 10/Npgsql, mappings and the `InitialCoreSchema` migration.
- REST controllers for customers, contracts, projects and warehouses; pagination, selected lifecycle operations and `/health/live` / `/health/ready`.
- Manual end-to-end verification against the local PostgreSQL container: customer and warehouse creation/read-back; customer-contract association; contract-project association; warehouse assignment to a project and read-back of its active assignment.

### Automated tests and CI evidence
- Domain/test project: 38 tests pass, including the EF model test that generates PostgreSQL DDL without connecting to a database.
- Integration-test project: 28 tests use `WebApplicationFactory<Program>` and a disposable PostgreSQL 17 Alpine container through Testcontainers. They apply real EF migrations to an isolated temporary database and do not use the local Docker Compose volume.
- Integration coverage: live/readiness health endpoints; customer/warehouse create, read and list; duplicate rejection; 404/409/422 error responses; customer-contract and contract-project references in both directions; active warehouse assignment persistence; complete tested lifecycle paths for Customer, Contract, Project and Warehouse; outbox persistence and event clearing after successful save; migration application; connection-string isolation; identifier uniqueness; and concurrent independent flows.
- CI evidence: [GitHub Actions run for commit `2041ee0`](https://github.com/julieste1012/ArchiCore/actions/runs/40788607652) completed successfully. Restore and Release build succeeded; test summary: 66 passed, 0 failed, 0 skipped (38 domain/model + 28 integration).
- The suite validates the implemented core slice only. It does not establish that the full ERP backend or all business workflows are complete.

### Known limits of the core API slice
- The API maps known domain error codes explicitly to HTTP 404/409; other domain validation failures return 422. New domain error codes must be added to the mapping intentionally.
- Request validation is primarily domain Value Object validation and query-parameter ranges; there is no general application validation pipeline yet.
- Customer currently stores NIT, status and contract references only. Required organization/profile/contact fields remain open in `CustomerSpecification.md`.
- `ContractStatus` includes `Cancelled`, but the domain has no `Cancel()` transition.
- Box currently exists in the domain only: it has no EF mapping, application service or API endpoint.
- Domain events are now stored transactionally in `OutboxMessages` as JSONB and cleared from aggregates only after successful `SaveChanges`. There is not yet a dispatcher, retry/retention policy, or durable audit-trail processor. Unprocessed outbox rows will accumulate until that work is implemented.

### Not yet implemented / pending
- User identity and authentication: password handling, JWT access tokens, refresh-token lifecycle, revocation, roles and authorization policies.
- Outbox dispatch/consumer pipeline, retry and poison-message policy, retention/cleanup, and persistent audit trail. The current outbox only guarantees durable transactional event capture.
- File-storage abstraction and MinIO integration.
- Documentary modules (Archive, Folder, Box persistence/API, Document and Folio), blocked by the unresolved domain decisions in section 3.
- Business processes BP-001 to BP-014: the current API slice is not their implementation.
- General application-level validation pipeline and a versioned/documented API contract suitable for release.
- Deployment hardening, production secret handling, backup and restore verification beyond local Compose.
- Frontend application (intentionally deferred until the backend contracts and workflows are ready).

## 3. Domain and requirements blockers

Do not create persistence mappings or additional aggregate classes based on contradictory documents. The following require a documented decision or an approved specification:

1. **Archive / Folder / Box boundaries.** `DOMAIN_REVIEW.md` says Archive requires refinement and lists Folder, Box, Document and Folio as entities. `Archive.md` calls Archive an aggregate root containing the documentary hierarchy. `Folder.md` and `Box.md` are draft specifications that conflict over whether Folder and Box are aggregate roots or internal entities.
2. **Box-to-location relationship.** `Warehouse.md` v2.0 says physical `WarehouseLocation` is an internal entity of Warehouse and Box should reference a location by identity; location fields and movement rules are not formalized.
3. **Domain ownership map.** `01_DomainModel.md`, `DomainMap.md`, `DOMAIN_REVIEW.md` and `MasterDevelopmentPlan.md` differ on the aggregate-root list (including User, Loan and Archive).
4. **Contract/project cardinality.** `03_OrganizationalStructure.md` says each contract generates one operational project, while the Contract specification and ubiquitous language allow multiple projects per contract.
5. **Contract cancellation and event catalog.** `ContractStatus` includes Cancelled, but no `Cancel()` transition is implemented; documents also disagree on ContractClosed/ContractFinished and related events.
6. **Customer profile.** Current `Customer` stores NIT and status only. `CustomerSpecification.md` explicitly leaves name/business identity/contact fields open; do not invent required fields silently.
7. **Incomplete status/event transitions.** Box has status values whose transitions are not reachable through current public methods; Warehouse maintenance/archive transitions do not all emit catalogued events.
8. **Business-process coverage.** `05_BusinessProcesses.md` marks BP-001 and BP-002 completed in documentation, but no application/use-case/API implementation was found for these workflows. BP-003 to BP-014 are still pending in the process index.
9. **Technology documentation.** Resolved by ADR-003: the solution targets .NET 10 LTS (`net10.0`), and the Master Blueprint now reflects ASP.NET Core 10. Keep patch-level dependencies aligned with the .NET 10 release train.

The project AI Collaboration Protocol prohibits inventing business rules. Record decisions in the documentation first, then implement them.

## 4. Next implementation sequence

1. **Domain decision record:** reconcile aggregate-root/entity ownership, contract/project cardinality, WarehouseLocation, lifecycle transitions and domain-event inconsistencies before implementing documentary persistence.
2. **Close the approved domain model:** implement only documented decisions and strengthen tests for invariants and permitted/forbidden transitions.
3. **Application/API hardening:** add an application validation pipeline and publish a stable, versioned API contract for the core modules.
4. **Outbox processing and audit:** implement an allow-listed event dispatcher, idempotent handling, retries, poison-message strategy, retention and durable audit entries. The transactional outbox already captures events, but it does not deliver them.
5. **Security:** formalize the user identity/role model from requirements, then implement password handling, JWT, refresh-token rotation/revocation, authorization policies and integration tests.
6. **Documentary workflows:** after the decisions in section 3 are approved, implement archive/inventory, box/folder, location, search, loans/returns, digitization and billing in prioritized increments.
7. **File storage:** implement the documented storage abstraction and MinIO integration, including authorization and audit behavior.
8. **Integration/operational tests:** extend the passing PostgreSQL Testcontainers suite for validation, security, event dispatch and clean migration/start-up checks.
9. **Release gate:** clean restore/build/test, no high-severity package advisory, migration/deployment smoke test, secure configuration, backup guidance and documented endpoints.
10. **Frontend:** begin only after the approved API contracts and priority end-to-end workflows are verified.

## 5. Definition of backend complete

The backend is complete only when the approved in-scope workflows have server-side validation and working endpoints; data persists in PostgreSQL through versioned migrations; authentication and permissions protect applicable endpoints; file handling and audit behavior meet the documented requirements; domain/application logic and API/database flows are covered by automated tests; and a clean deployment starts reliably.

## 6. Git and collaboration rules

- `main` is the stable release branch; develop and test changes on `develop` or short-lived feature branches.
- Keep one logical change per commit and open pull requests for review.
- Every implementation task includes objective, scope, files, constraints, tests and acceptance criteria.
- Update this file after significant changes, including commit/PR, test evidence and next task.
- Architecture and business rules are documented before implementation. No silent assumptions.

# Implementation Status

| Field | Value |
|---|---|
| Project | ArchiCore ERP |
| Last audit | 2026-10-26 |
| Baseline reviewed | `d86b56aefadc374a1a657e38211fc2d2f4252dd9` |
| Integration branch | `develop` |
| Current phase | Backend foundation and domain hardening |
| Next critical work | Resolve domain boundaries and implement application/persistence/API layers |

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
- Projects: `ArchiCore.Domain`, `ArchiCore.Application`, `ArchiCore.Infrastructure`, `ArchiCore.Shared`, and `ArchiCore.API`.
- All existing projects target `.NET 10` (`net10.0`).
- The test runner is configured for Microsoft Testing Platform in `global.json`, as required for xUnit v3 with the .NET 10 SDK.

### Implemented, but not yet fully validated against approved requirements
- Domain primitives: `Entity`, `IAggregateRoot`, `Result`, `Error`, `DomainEvent`, `IDomainEvent`.
- Value objects: codes, NIT, email, phone, address, person name and money.
- Domain classes: `Customer`, `Contract`, `Project`, `Warehouse`, `Box`, their states and the event classes currently present.
- Tests cover selected current behavior for Result, value objects, Customer, Contract, Project, Warehouse and Box.

The current tests are a regression baseline, not proof that the business model is complete. Some existing transitions and emitted events are incomplete or inconsistent with the specifications.

### Not yet implemented / not found in the repository audit
- Application use cases, commands, queries, DTOs and validation pipeline.
- Infrastructure persistence implementation, EF Core `DbContext`, repository implementations and database migrations.
- PostgreSQL configuration and operational startup.
- Business controllers/endpoints and dependency-injection wiring in the API.
- User/authentication implementation, JWT access tokens, refresh-token lifecycle and role-based authorization.
- Persistent audit trail, file-storage abstraction / MinIO integration, and integration tests.
- Docker/Compose configuration, health checks and deployment/backup setup.
- Frontend application (intentionally deferred until the backend is ready).

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

1. **Domain decision record:** reconcile aggregate-root/entity ownership, contract/project cardinality, WarehouseLocation, lifecycle transitions and domain-event inconsistencies.
2. **Close the domain model:** implement only approved gaps and complete tests for all invariants and permitted/forbidden state transitions.
3. **Application layer:** commands/use cases, queries, DTOs, validation, repository interfaces and application services.
4. **Infrastructure:** EF Core mappings for approved aggregates, PostgreSQL, repositories, migrations, transaction handling and file-storage abstraction.
5. **API:** dependency-injection wiring, controllers/endpoints, request/response contracts, consistent errors, OpenAPI and health endpoints.
6. **Security and audit:** user/role model, password handling, JWT and refresh tokens, authorization policies, token revocation and audit persistence.
7. **Business workflows:** implement the approved reception/counting, documentary inventory, search, loans/returns, digitization and billing flows in prioritized increments.
8. **Integration and operational tests:** PostgreSQL integration tests, API tests, Docker Compose, health checks, configuration and secret handling.
9. **Release gate:** clean restore/build/test, no high-severity package advisory, migration/deployment smoke test, and documented API contract.
10. **Frontend:** begin after backend integration contracts and end-to-end flows are verified.

## 5. Definition of backend complete

The backend is complete only when the approved in-scope workflows have server-side validation and working endpoints; data persists in PostgreSQL through versioned migrations; authentication and permissions protect applicable endpoints; file handling and audit behavior meet the documented requirements; domain/application logic and API/database flows are covered by automated tests; and a clean deployment starts reliably.

## 6. Git and collaboration rules

- `main` is the stable release branch; develop and test changes on `develop` or short-lived feature branches.
- Keep one logical change per commit and open pull requests for review.
- Every implementation task includes objective, scope, files, constraints, tests and acceptance criteria.
- Update this file after significant changes, including commit/PR, test evidence and next task.
- Architecture and business rules are documented before implementation. No silent assumptions.

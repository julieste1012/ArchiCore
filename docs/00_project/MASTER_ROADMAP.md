# ArchiCore ERP — Master Roadmap

Last reviewed: 2026-10-29  
Stable branch: `main`  
Integration target: `develop`  
Active implementation: `feat/backend-core-api` (PR #7, pending final review)  
Current phase: Core API + authentication/audit + operational frontend slice; documentary business workflows pending

> This roadmap describes the active feature branch, not code already merged into `develop`. See [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) for CI evidence, exact limits and domain blockers.
>
> Domain and business decisions required before documentary persistence: [DOMAIN_DECISIONS_TO_APPROVE.md](../03_domain/DOMAIN_DECISIONS_TO_APPROVE.md). Proposals there are not approved requirements.

## Phase 0 — Foundation ✅
- [x] Repository and solution structure
- [x] Git/GitHub workflow established
- [x] Root `.gitignore`; generated .NET build output removed from version control on develop
- [x] Documentation foundation
- [x] Backend CI workflow and .NET 10 LTS ADR

## Phase 1 — Business analysis ⚠️ In progress
- [x] Ubiquitous language, organizational structure, business roles and service portfolio documented
- [ ] Complete and approve all business processes (BP-003 through BP-014 remain pending in the process index)
- [ ] Resolve contract/project cardinality discrepancy
- [ ] Confirm customer identity/contact details and documentary metadata
- [ ] Approve endpoint permissions by business role
- [ ] Finalize actor-level audit, retention and security requirements
- [ ] Close and approve the documented domain decisions before designing documentary schema or APIs

## Phase 2 — Domain ⚠️ Partial implementation
- [x] Domain base primitives and Result pattern
- [x] Initial value objects
- [x] Initial Customer, Contract, Project, Warehouse and Box classes/events
- [x] Domain regression baseline
- [ ] Resolve aggregate/entity boundaries: Archive, Folder, Box, Document, Folio, User and Loan
- [ ] Formalize WarehouseLocation and Box location/movement model
- [ ] Reconcile lifecycle transitions and event catalog
- [ ] Complete domain invariants and negative-path tests
- [ ] Implement approved remaining domain concepts

## Phase 3 — Application ✅ Core modules implemented; broader workflows pending
- [x] Core use cases/services and DTOs for Customer, Contract, Project and Warehouse
- [x] Repository/service interfaces for current core modules
- [x] Paged list queries and basic request validation
- [x] Domain-event audit query service
- [ ] General validation pipeline and application use cases for approved documentary workflows
- [ ] Application-level use cases for security/administration audit

## Phase 4 — Infrastructure ✅ Core persistence/security/audit; object storage pending
- [x] EF Core configuration and PostgreSQL mappings for four core modules
- [x] Versioned migrations for core schema, authentication and domain-event audit
- [x] Repository implementations and transaction support
- [x] JWT signing/validation, password hashing, refresh-token rotation/revocation and temporary lockout
- [x] Transactional Outbox, row-lock coordinated worker and durable domain-event audit projection
- [x] Provider-neutral object storage abstraction with key validation and filesystem/MinIO-SDK adapters
- [ ] Select and validate a supported production S3-compatible service, bucket policy and object authorization
- [ ] Actor-level security audit, outbox retention/poison-message handling and operational metrics

## Phase 5 — API ✅ Core endpoints/security implemented; permissions and workflows pending
- [x] Dependency injection and API wiring
- [x] Customer, Contract, Project and Warehouse endpoints
- [x] Login, refresh, logout, current-user and administrator-only user creation endpoints
- [x] Administrator-only paginated domain-event audit endpoint
- [x] Standard validation/error responses and health endpoints
- [x] Backend integration tests using PostgreSQL Testcontainers
- [ ] Final per-role authorization policy matrix and credential recovery/reset flow
- [x] Authenticated self-service password change with session revocation
- [ ] APIs for approved archive/folder/box/document/folio/loan workflows
- [ ] OpenAPI contract review for all endpoint request/response models

## Phase 6 — End-to-end business workflows ⏳ Not complete
- [ ] Documentary reception (BP-001) and counting (BP-002) end to end
- [ ] Verification, classification and labeling
- [ ] ERP documentary registration and physical storage/location assignment
- [ ] Search and retrieval
- [ ] Documentary loans and returns
- [ ] Digitization and file metadata
- [ ] Billing support and contract closure/documentary return

## Phase 7 — Testing and hardening ✅ Started
- [x] Domain unit-test baseline
- [x] Automated restore/build/test on pull requests
- [x] PostgreSQL/API integration suite and migration smoke coverage
- [x] Authentication/refresh-token and role-boundary tests
- [x] Domain-event outbox/audit transaction and API tests
- [ ] Application unit tests for all use cases
- [ ] Architecture/dependency-boundary tests
- [x] Concurrency test for simultaneous refresh-token redemption
- [ ] Full security review, rate limiting and package-advisory recheck before release

## Phase 8 — Containers and deployment ⚠️ Local stack validated; production deployment pending
- [x] Local PostgreSQL Compose service and health check
- [x] Local connection port avoids conflict with Windows PostgreSQL
- [x] API and frontend Dockerfiles with optional Compose profile for PostgreSQL + API + web
- [x] Container smoke CI builds both images, applies migrations in the Container environment, checks readiness and verifies login through the frontend proxy
- [ ] Configure a supported production object-store service and production environment credentials
- [ ] Validate provider lifecycle and retention policies
- [ ] Production secret management, logging/metrics
- [ ] Backup and restore validation; production smoke test

## Phase 9 — Frontend 🚧 First slice implemented
- [x] React + TypeScript + Vite foundation and frontend CI
- [x] Login/session flow connected to `POST /api/auth/login`
- [x] Dashboard metrics queried from API
- [x] Customer, contract, project and warehouse list/create screens
- [x] Guarded lifecycle actions for existing customer/contract/project/warehouse transitions
- [x] Project warehouse assignment/release UI with assignment history
- [x] Contract finish action with an explicit finish date
- [x] Administrator user management and domain-event audit viewer
- [x] Self-service password change for authenticated users
- [x] CI typecheck and production build
- [ ] Edit/detail screens and broader form validation
- [ ] Approved role-aware navigation/actions
- [ ] Documentary modules, actor-aware audit improvements and approved role-specific module views
- [ ] Frontend component tests and end-to-end workflows against a clean deployment

## Current completed maintenance work
- [x] PR #1: repository hygiene merged into `develop`
- [x] PR #2: implementation status document introduced and merged into `develop`
- [x] PR #3: domain tests and CI merged into `develop`
- [x] PR #4: OpenAPI dependency security update merged into `develop`
- [x] Optional local full-stack Docker Compose profile and end-to-end container smoke check validated in CI
- [ ] PR #7: core API, PostgreSQL, security, domain-event audit, operational frontend and local container profile (open; do not merge until final CI/diff review)

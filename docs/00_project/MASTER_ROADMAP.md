# ArchiCore ERP — Master Roadmap

Last reviewed: 2026-10-26  
Current integration branch: `develop`  
Current phase: Backend foundation and domain hardening

> This roadmap reflects the repository state verified in GitHub. The backend is not yet end-to-end functional. See [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) for audit details and blockers.

## Phase 0 — Foundation ✅
- [x] Repository and solution structure
- [x] Git/GitHub workflow established
- [x] Root `.gitignore`; generated .NET build output removed from version control
- [x] Documentation foundation
- [x] Backend CI workflow

## Phase 1 — Business analysis ⚠️ In progress
- [x] Ubiquitous language, organizational structure, business roles and service portfolio documented
- [ ] Complete and approve all business processes (BP-003 through BP-014 remain pending in the process index)
- [ ] Resolve contract/project cardinality discrepancy
- [ ] Confirm requirements for customer identity/contact details and documentary metadata
- [ ] Finalize audit, authentication and role/permission requirements

## Phase 2 — Domain ⚠️ Partial implementation
- [x] Domain base primitives and Result pattern
- [x] Initial value objects
- [x] Initial Customer, Contract, Project, Warehouse and Box classes and events
- [x] Initial unit-test baseline (37 tests passing in GitHub Actions)
- [ ] Resolve aggregate-root/entity boundaries: Archive, Folder, Box, Document, Folio, User and Loan
- [ ] Formalize WarehouseLocation and Box location/movement model
- [ ] Reconcile lifecycle transitions and event catalog
- [ ] Complete domain invariants and negative-path tests
- [ ] Implement remaining approved domain concepts (including User and Loan where approved)

## Phase 3 — Application ⏳ Not started
- [ ] Commands/use cases and queries
- [ ] DTOs and request models
- [ ] Validation
- [ ] Repository/service interfaces
- [ ] Domain-event handling and audit application flows

## Phase 4 — Infrastructure ⏳ Not started
- [ ] EF Core configuration and mappings
- [ ] PostgreSQL and migrations
- [ ] Repository implementations
- [ ] Transaction and concurrency strategy
- [ ] File-storage abstraction and MinIO integration
- [ ] Persisted audit records

## Phase 5 — API ⏳ Not started
- [ ] Dependency injection and application wiring
- [ ] Business endpoints/controllers
- [ ] Standard validation/error responses
- [ ] Authentication, JWT, refresh tokens and authorization policies
- [ ] OpenAPI and health checks
- [ ] API integration tests

## Phase 6 — End-to-end business workflows ⏳ Not started
- [ ] Documentary reception and counting
- [ ] Verification, classification and labeling
- [ ] ERP registration and physical storage
- [ ] Search and retrieval
- [ ] Documentary loans and returns
- [ ] Digitization and billing support
- [ ] Contract closure and documentary return

## Phase 7 — Testing and hardening ⚠️ Started
- [x] Domain unit-test baseline
- [x] Automated restore/build/test on pull requests
- [ ] Complete domain coverage
- [ ] Application unit tests
- [ ] PostgreSQL and API integration tests
- [ ] Architecture/dependency-boundary tests
- [ ] Resolve/document all remaining security and package advisories

## Phase 8 — Containers and deployment ⏳ Not started
- [ ] Dockerfiles and Compose
- [ ] Backend + PostgreSQL + object storage
- [ ] Safe configuration/secrets
- [ ] Logs, monitoring, backup and restore validation
- [ ] Production smoke test

## Phase 9 — Frontend ⏸️ Deferred by design
- [ ] React + TypeScript foundation
- [ ] Login and dashboard
- [ ] Customer, contract, project, warehouse and documentary modules
- [ ] API integration, error states and permission-aware UI
- [ ] End-to-end validation against the completed backend

## Current completed maintenance work
- [x] PR #1: repository hygiene merged into `develop`
- [x] PR #2: implementation status document introduced and merged into `develop`
- [x] PR #3: domain tests and CI merged into `develop`
- [x] PR #4: OpenAPI dependency security update merged into `develop`

# ArchiCore API Reference

Status: working contract for the current `feat/backend-core-api` branch. This reference describes implemented endpoints only. It does not imply that the full documentary ERP workflow is complete.

## Base URL and authentication

- Local frontend development uses the Vite proxy: `/api` is forwarded to the local ASP.NET Core API.
- Send `Authorization: Bearer <accessToken>` to authenticated endpoints.
- Access tokens are short-lived. The frontend keeps the session in memory, rotates refresh tokens on demand, and logs the user out when refresh fails.
- The bootstrap administrator is created only when the documented environment configuration is enabled. Never commit signing keys or bootstrap credentials.
- Operational access follows the approved responsibility mapping in [ROLE_PERMISSION_MATRIX.md](ROLE_PERMISSION_MATRIX.md). Administrative account management and both audit streams remain restricted to `SystemAdministrator`.
- In Development, the OpenAPI document is exposed by ASP.NET Core at `/openapi/v1.json`.

## Response and paging conventions

Successful list responses use this shape:

```json
{
  "page": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0,
  "items": []
}
```

List endpoints default to page 1 and page size 20. Page size is limited to 100. Domain or validation failures use Problem Details with an additional `code` property. The implemented API maps invalid authentication to 401, insufficient role to 403, missing records to 404, duplicate/conflicting lifecycle state to 409, and invalid input/domain rules to 422. Framework-level malformed requests may also be rejected as 400.

## Authentication and account management

| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | Anonymous | Authenticate with email and password; returns access token, refresh token and account DTO. |
| POST | `/api/auth/refresh` | Anonymous | Rotate a refresh token; a previously used token is rejected. |
| POST | `/api/auth/logout` | Anonymous | Revoke the supplied refresh token; returns 204. |
| GET | `/api/auth/me` | Authenticated | Return the current account DTO. |
| POST | `/api/auth/change-password` | Authenticated | Verify current password, enforce new-password policy, update password and revoke existing sessions; returns 204. The user must log in again. |
| POST | `/api/auth/users` | SystemAdministrator | Create a user account. |
| GET | `/api/auth/users?page=1&pageSize=20` | SystemAdministrator | List accounts with paging. |
| PUT | `/api/auth/users/{id}/status` | SystemAdministrator | Activate or deactivate an account using `{ "isActive": true }`. Deactivation increments token version and revokes sessions. |

Create-user body:

```json
{
  "email": "person@example.com",
  "displayName": "Full Name",
  "password": "Choose_A_Strong_Password_2026!",
  "role": "ArchiveTechnician"
}
```

Supported role identifiers currently defined by the application are `SystemAdministrator`, `AdministrativeManager`, `ProjectCoordinator`, `ArchiveTechnician`, `ArchiveAssistant`, `DigitizationOperator`, `BillingAssistant` and `CustomerRepresentative`. Role-specific access to operational modules is intentionally not enabled yet, except for SystemAdministrator, until the endpoint permission matrix is approved.

## Customers

Customer read/create/lifecycle operations require `SystemAdministrator` or `AdministrativeManager`.

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/customers?page=1&pageSize=20` | Paged list. |
| GET | `/api/customers/{id}` | Get one customer. |
| POST | `/api/customers` | Create a customer. |
| POST | `/api/customers/{id}/activate` | Apply the existing Activate transition. |
| POST | `/api/customers/{id}/deactivate` | Apply the existing Deactivate transition. |
| POST | `/api/customers/{id}/archive` | Apply the existing Archive transition. |

Create body:

```json
{ "nit": "900123456-7" }
```

The current Customer model stores NIT, status and contract references. Legal name, contact details and other profile requirements remain unresolved in the approved customer specification.

## Contracts

Contract reads are available to `SystemAdministrator`, `AdministrativeManager`, `ProjectCoordinator` and `BillingAssistant`. Create and lifecycle operations require `SystemAdministrator` or `AdministrativeManager`.

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/contracts?page=1&pageSize=20&clientId={customerId}` | Paged list; `clientId` is optional. |
| GET | `/api/contracts/{id}` | Get one contract. |
| POST | `/api/contracts` | Create a contract. |
| POST | `/api/contracts/{id}/activate` | Activate a Draft or reactivate a Suspended contract. |
| POST | `/api/contracts/{id}/suspend` | Suspend an Active contract. |
| POST | `/api/contracts/{id}/finish` | Finish an Active contract using an explicit date. |

Create body:

```json
{
  "contractNumber": "CTR-2026-000001",
  "clientId": "00000000-0000-0000-0000-000000000000",
  "startDate": "2026-01-15T05:00:00Z",
  "totalValue": 1500000.75,
  "currency": "COP"
}
```

Finish body:

```json
{ "finishDate": "2026-10-29T17:00:00Z" }
```

The finish endpoint only changes the contract lifecycle. It does not currently enforce the complete documentary-return, pending-loan, billing or archive-closure workflow described in business documentation.

## Projects and warehouse assignments

Project reads are available to `SystemAdministrator`, `AdministrativeManager`, `ProjectCoordinator`, `ArchiveTechnician`, `ArchiveAssistant`, `DigitizationOperator` and `BillingAssistant`. Project creation/lifecycle and warehouse assignment require `SystemAdministrator` or `ProjectCoordinator`.

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/projects?page=1&pageSize=20&contractId={contractId}` | Paged list; `contractId` is optional. |
| GET | `/api/projects/{id}` | Get one project including warehouse assignment history. |
| POST | `/api/projects` | Create a project for a contract. |
| POST | `/api/projects/{id}/plan` | Plan a Draft project. |
| POST | `/api/projects/{id}/start` | Start a Planned project. |
| POST | `/api/projects/{id}/suspend` | Suspend an Active project. |
| POST | `/api/projects/{id}/resume` | Resume a Suspended project. |
| POST | `/api/projects/{id}/complete` | Complete an Active project. |
| POST | `/api/projects/{id}/close` | Close a Completed project. |
| POST | `/api/projects/{id}/archive` | Archive a Closed project. |
| POST | `/api/projects/{id}/warehouses` | Assign a warehouse. |
| POST | `/api/projects/{id}/warehouses/release` | Release an active warehouse assignment without deleting its history. |

Create body:

```json
{
  "projectCode": "PRJ-2026-000001",
  "contractId": "00000000-0000-0000-0000-000000000000"
}
```

Assign body:

```json
{
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "assignedFrom": "2026-10-29T17:00:00Z"
}
```

Release body:

```json
{
  "warehouseId": "00000000-0000-0000-0000-000000000000",
  "releasedOn": "2026-10-29T18:00:00Z"
}
```

The project API records a project-to-warehouse assignment only. Physical aisles, shelves, cabinets and WarehouseLocation are not implemented because the domain specification is still open.

## Warehouses

Warehouse reads are available to `SystemAdministrator`, `ProjectCoordinator`, `ArchiveTechnician` and `ArchiveAssistant`. Warehouse creation and lifecycle administration remain restricted to `SystemAdministrator` until the dedicated physical-location operations are implemented.

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/warehouses?page=1&pageSize=20` | Paged list. |
| GET | `/api/warehouses/{id}` | Get one warehouse. |
| POST | `/api/warehouses` | Create a warehouse. |
| POST | `/api/warehouses/{id}/activate` | Activate an Available warehouse. |
| POST | `/api/warehouses/{id}/maintenance/start` | Start maintenance for an Operational warehouse. |
| POST | `/api/warehouses/{id}/maintenance/finish` | Finish maintenance. |
| POST | `/api/warehouses/{id}/close` | Close a warehouse from Maintenance. |
| POST | `/api/warehouses/{id}/archive` | Archive a Closed warehouse. |

Create body:

```json
{ "warehouseCode": "WH-000001" }
```

Physical location management is not yet exposed via API.

## Documentary receptions and counting (BP-001 / BP-002)

Reception reads are available to `SystemAdministrator`, `ProjectCoordinator`, `ArchiveTechnician` and `ArchiveAssistant`. Creating a reception requires `SystemAdministrator` or `ProjectCoordinator`. Inspection and counting require `SystemAdministrator`, `ProjectCoordinator` or `ArchiveAssistant`. Final count approval and manual suspension require `SystemAdministrator` or `ProjectCoordinator`.

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/receptions?page=1&pageSize=20&contractId={contractId}&status={status}` | Paged reception list; contract and status filters are optional. |
| GET | `/api/receptions/{id}` | Get reception and inspection/count history. |
| POST | `/api/receptions` | Register a delivery against an active contract. |
| POST | `/api/receptions/{id}/inspection` | Record physical inspection. |
| POST | `/api/receptions/{id}/counting` | Record declared and physically counted boxes/folders. |
| POST | `/api/receptions/{id}/counting/approve` | Coordinator approves the recorded count. |
| POST | `/api/receptions/{id}/suspend` | Suspend a reception with a reason. |

Create body:

```json
{
  "contractId": "00000000-0000-0000-0000-000000000000",
  "initialObservations": "Inventario de entrega y novedades iniciales"
}
```

Inspection body:

```json
{
  "safeToAccept": true,
  "observations": "Cajas íntegras, etiquetas legibles y sellos completos."
}
```

Counting body:

```json
{
  "declaredBoxCount": 10,
  "declaredFolderCount": 120,
  "countedBoxCount": 9,
  "countedFolderCount": 118,
  "discrepancyReport": "Se registra una caja y dos carpetas faltantes."
}
```

Suspension body:

```json
{ "reason": "Novedad física o discrepancia que impide continuar." }
```

The implemented state flow is `Received → Inspected → CountingRecorded → Counted`. Unsafe physical inspection or an explicit coordinator suspension leads to `Suspended`. An inspection marked unsafe requires an observation; any difference between declared and counted boxes/folders requires a discrepancy report; negative counts are rejected. The API records the user ID and timestamp for receiving, inspection, counting, approval and suspension, and domain events are published through the transactional outbox.

**Current scope boundary:** the workflow stores delivery-level box/folder totals and a discrepancy record. It does not yet model each physical box/folder, validate a scheduled delivery, reserve physical capacity, or assign an aisle/shelf/cabinet position. Those capabilities depend on the unresolved WarehouseLocation and documentary hierarchy decisions. Therefore this is an implemented BP-001/BP-002 API slice, not a complete warehouse inventory or final business acceptance.
## Domain-event audit

Requires SystemAdministrator.

`GET /api/audit/domain-events?page=1&pageSize=20&eventType={fullTypeName}&fromUtc={ISO-8601}&toUtc={ISO-8601}`

All query filters are optional. Results contain event ID, type, occurrence time, record time and JSON payload. The transactional outbox captures domain events and the hosted processor projects them into this immutable technical audit table. This is not yet a complete actor-level security/business audit because current domain events do not consistently identify the authenticated user.

## Security audit

Requires `SystemAdministrator`.

`GET /api/audit/security-events?page=1&pageSize=20&action={action}&outcome={Succeeded|Failed}&fromUtc={ISO-8601}&toUtc={ISO-8601}`

Filters are optional. Each record returns the action, result, UTC timestamp, actor account/role and target account/role when available. The stream records login outcomes, refresh outcomes, logout, user creation, activation/deactivation and password changes. Unknown-account login failures do not store the submitted email. Passwords, access tokens and refresh tokens are never written to this audit table.

This endpoint is deliberately separate from `/api/audit/domain-events`: domain-event audit stores business event payloads, while security audit records authenticated actor attribution and authentication/account-security outcomes. Both endpoints are administrator-only. Retention, privacy review and security-event alerting remain operational release requirements.

## Authorization policy summary

| Resource | Read | Write / lifecycle |
|---|---|---|
| Customers | SystemAdministrator, AdministrativeManager | SystemAdministrator, AdministrativeManager |
| Contracts | SystemAdministrator, AdministrativeManager, ProjectCoordinator, BillingAssistant | SystemAdministrator, AdministrativeManager |
| Projects | SystemAdministrator, AdministrativeManager, ProjectCoordinator, ArchiveTechnician, ArchiveAssistant, DigitizationOperator, BillingAssistant | SystemAdministrator, ProjectCoordinator |
| Warehouses | SystemAdministrator, ProjectCoordinator, ArchiveTechnician, ArchiveAssistant | SystemAdministrator |
| Receptions | SystemAdministrator, ProjectCoordinator, ArchiveTechnician, ArchiveAssistant | Receive: SystemAdministrator/ProjectCoordinator; inspect/count: SystemAdministrator/ProjectCoordinator/ArchiveAssistant; approve/suspend: SystemAdministrator/ProjectCoordinator |
| Users and audit | SystemAdministrator | SystemAdministrator |

The customer portal role is reserved for a future tenant-scoped portal. It has no access to internal records in the current API. Documentary technicians, assistants, digitization operators and billing staff receive only the read context needed for their approved duties until their specific workflows are implemented.

## Health and infrastructure

| Method | Route | Purpose |
|---|---|---|
| GET | `/health/live` | Process liveness. |
| GET | `/health/ready` | Readiness check including database connectivity. |

An object-storage abstraction is configured for local development and an S3-compatible adapter, but there are no public document upload/download routes yet. Document metadata, access rules, retention and lifecycle must be agreed before those endpoints can be implemented safely.

# ArchiCore API Reference

Status: working contract for the current `feat/backend-core-api` branch. This reference describes implemented endpoints only. It does not imply that the full documentary ERP workflow is complete.

## Base URL and authentication

- Local frontend development uses the Vite proxy: `/api` is forwarded to the local ASP.NET Core API.
- Send `Authorization: Bearer <accessToken>` to authenticated endpoints.
- Access tokens are short-lived. The frontend keeps the session in memory, rotates refresh tokens on demand, and logs the user out when refresh fails.
- The bootstrap administrator is created only when the documented environment configuration is enabled. Never commit signing keys or bootstrap credentials.
- Until the business permission matrix is approved, core operational endpoints, user administration and the event-audit endpoint are restricted to the `SystemAdministrator` role.
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

All customer routes require SystemAdministrator.

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

All contract routes require SystemAdministrator.

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

All project routes require SystemAdministrator.

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

All warehouse routes require SystemAdministrator.

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

## Domain-event audit

Requires SystemAdministrator.

`GET /api/audit/domain-events?page=1&pageSize=20&eventType={fullTypeName}&fromUtc={ISO-8601}&toUtc={ISO-8601}`

All query filters are optional. Results contain event ID, type, occurrence time, record time and JSON payload. The transactional outbox captures domain events and the hosted processor projects them into this immutable technical audit table. This is not yet a complete actor-level security/business audit because current domain events do not consistently identify the authenticated user.

## Health and infrastructure

| Method | Route | Purpose |
|---|---|---|
| GET | `/health/live` | Process liveness. |
| GET | `/health/ready` | Readiness check including database connectivity. |

An object-storage abstraction is configured for local development and an S3-compatible adapter, but there are no public document upload/download routes yet. Document metadata, access rules, retention and lifecycle must be agreed before those endpoints can be implemented safely.

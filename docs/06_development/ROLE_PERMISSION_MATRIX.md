# Role and Permission Matrix

**Source of truth:** approved `docs/02_business/02_BusinessRoles.md` (version 1.0.0), including its responsibilities and process responsibility matrix.

This document maps approved business responsibilities to API authorization policies. It does not grant permissions beyond the duties already documented. A missing module or endpoint remains unavailable until its business process and acceptance criteria are implemented.

## Implemented policy mapping

| Module / operation | System Administrator | Administrative Manager | Project Coordinator | Archive Technician | Archive Assistant | Digitization Operator | Billing Assistant | Customer Representative |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Customers: read and manage | ✓ | ✓ | — | — | — | — | — | — |
| Contracts: read | ✓ | ✓ | ✓ | — | — | — | ✓ | — |
| Contracts: create and lifecycle | ✓ | ✓ | — | — | — | — | — | — |
| Projects: read | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | — |
| Projects: operational lifecycle | ✓ | — | ✓ | — | — | — | — | — |
| Project-to-warehouse assignment | ✓ | — | ✓ | — | — | — | — | — |
| Warehouses: read | ✓ | — | ✓ | ✓ | ✓ | — | — | — |
| Warehouse administration / lifecycle | ✓ | — | — | — | — | — | — | — |
| User administration and technical audit | ✓ | — | — | — | — | — | — | — |
| Documentary verification, classification, labeling | Not exposed yet | — | — | ✓ | — | — | — | — |
| Physical storage, movement, loan hand-off/return | Not exposed yet | — | — | — | ✓ | — | — | — |
| Reception creation and count approval/suspension | ✓ | — | ✓ | — | — | — | — | — |
| Physical inspection and count recording | ✓ | — | ✓ | — | ✓ | — | — | — |
| Reception record read | ✓ | — | ✓ | ✓ | ✓ | — | — | — |
| Loan authorization | Not exposed yet | — | ✓ | — | — | — | — | — |
| Digitization workflow | Not exposed yet | — | — | — | — | ✓ | — | — |
| Operational billing preparation and validation | Not exposed yet | ✓ | — | — | — | — | ✓ | — |
| Customer portal requests and own-inventory consultation | Not exposed yet | — | — | — | — | — | — | Reserved for customer portal |

## Policy names

- `Customers.Read`, `Customers.Manage`
- `Contracts.Read`, `Contracts.Manage`
- `Projects.Read`, `Projects.Operate`, `Projects.AssignWarehouses`
- `Warehouses.Read`, `Warehouses.Manage`
- `Receptions.Read`, `Receptions.Receive`, `Receptions.Inspect`, `Receptions.Count`, `Receptions.Approve`

## Boundaries and safeguards

1. System Administrator handles technical administration; the business workflows remain assigned to their operational roles.
2. Administrative Manager supervises customer and contract administration, billing supervision, and operational indicators. Project Coordinator may read contract data needed to monitor contract capacity; lifecycle and commercial changes remain with Administrative Manager.
3. Project Coordinator operates projects, registers reception, approves counting, suspends problematic deliveries, assigns storage and authorizes loans.
4. Archive Technician can read project and warehouse context needed for verification, classification, labeling, documentary hierarchy validation, and barcode association. Dedicated write workflow endpoints are not yet implemented.
5. Archive Assistant can read project, warehouse and reception context; inspect received material and record physical box/folder counts; physical storage/movement and loan hand-off/return endpoints remain pending.
6. Digitization Operator can read project context to associate digitization work; warehouse registry and unrelated CRUD are not available. Billing Assistant reads contract/project context only. Their dedicated workflows are not yet implemented.
7. Project Coordinator receives contract read access only, supporting the documented capacity-monitoring responsibility. Customer Representative remains reserved for the future customer portal; no customer-owned data endpoint is exposed by this API.
8. All authorization remains deny-by-default. A role's inclusion in the account catalogue does not create access to modules without an explicit policy.
9. The current API's `DomainEventAudit` remains restricted to System Administrator. Actor-level audit is a separate backend work item.

## Implementation status

The policies are applied to Customer, Contract, Project, Warehouse and the initial DocumentaryReception workflow. The reception workflow covers delivery registration, inspection, count/discrepancy recording, suspension and coordinator approval. Detailed verification/classification/labeling, item-level inventory and physical location operations remain unexposed until their domain specifications are reconciled. Billing and customer-portal workflows remain pending.

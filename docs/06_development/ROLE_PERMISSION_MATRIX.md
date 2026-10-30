# Role and Permission Matrix

**Source of truth:** approved `docs/02_business/02_BusinessRoles.md` (version 1.0.0), including its responsibilities and process responsibility matrix.

This document maps approved business responsibilities to API authorization policies. It does not grant permissions beyond the duties already documented. A missing module or endpoint remains unavailable until its business process and acceptance criteria are implemented.

## Implemented policy mapping

| Module / operation | System Administrator | Administrative Manager | Project Coordinator | Archive Technician | Archive Assistant | Digitization Operator | Billing Assistant | Customer Representative |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Customers: read and manage | ✓ | ✓ | — | — | — | — | — | — |
| Contracts: read | ✓ | ✓ | ✓ | — | — | — | ✓ | — |
| Contracts: create and lifecycle | ✓ | ✓ | — | — | — | — | — | — |
| Projects: read | ✓ | ✓ | ✓ | — | — | — | ✓ | — |
| Projects: operational lifecycle | ✓ | — | ✓ | — | — | — | — | — |
| Project-to-warehouse assignment | ✓ | — | ✓ | — | — | — | — | — |
| Warehouses: read | ✓ | — | ✓ | — | ✓ | — | — | — |
| Warehouse administration / lifecycle | ✓ | — | — | — | — | — | — | — |
| User administration and technical audit | ✓ | — | — | — | — | — | — | — |
| Documentary verification, classification, labeling | Not exposed yet | — | — | ✓ | — | — | — | — |
| Physical storage, movement, loan hand-off/return | Not exposed yet | — | — | — | ✓ | — | — | — |
| Reception and counting | Not exposed yet | — | ✓ | — | ✓ | — | — | — |
| Loan authorization | Not exposed yet | — | ✓ | — | — | — | — | — |
| Digitization workflow | Not exposed yet | — | — | — | — | ✓ | — | — |
| Operational billing preparation and validation | Not exposed yet | ✓ | — | — | — | — | ✓ | — |
| Customer portal requests and own-inventory consultation | Not exposed yet | — | — | — | — | — | — | Reserved for customer portal |

## Policy names

- `Customers.Read`, `Customers.Manage`
- `Contracts.Read`, `Contracts.Manage`
- `Projects.Read`, `Projects.Operate`, `Projects.AssignWarehouses`
- `Warehouses.Read`, `Warehouses.Manage`

## Boundaries and safeguards

1. System Administrator handles technical administration; the business workflows remain assigned to their operational roles.
2. Administrative Manager supervises customer and contract administration, billing supervision, and operational indicators. Project Coordinator may read contract data needed to monitor contract capacity; lifecycle and commercial changes remain with Administrative Manager.
3. Project Coordinator operates projects and performs the documented reception, counting, storage assignment, and loan authorization responsibilities.
4. Archive Technician owns verification, classification, labeling, documentary hierarchy validation, and barcode association. Dedicated workflow endpoints are not yet implemented.
5. Archive Assistant handles physical reception support, storage/movement, and physical loan hand-off/return. Dedicated workflow endpoints are not yet implemented.
6. Digitization Operator and Billing Assistant receive no unrelated CRUD privileges. Their dedicated workflows are not yet implemented.
7. Project Coordinator receives contract read access only, supporting the documented capacity-monitoring responsibility. Customer Representative remains reserved for the future customer portal; no customer-owned data endpoint is exposed by this API.
8. All authorization remains deny-by-default. A role's inclusion in the account catalogue does not create access to modules without an explicit policy.
9. The current API's `DomainEventAudit` remains restricted to System Administrator. Actor-level audit is a separate backend work item.

## Implementation status

The current policies are applied to existing Customer, Contract, Project and Warehouse routes. The documentary and billing rows are deliberately marked "Not exposed yet", rather than granting access to endpoints that do not exist.

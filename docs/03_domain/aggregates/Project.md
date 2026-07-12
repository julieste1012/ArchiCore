# Project Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Project Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Contract Management |
| Aggregate Root | Project |

---

# Purpose

The Project Aggregate represents the operational execution of a contract.

A contract defines the commercial agreement between Multisertec and its customer.

A project represents the operational work required to fulfill one or more contracted services.

Every documentary operation belongs to a project.

---

# Responsibilities

The Project Aggregate is responsible for:

- Executing contracted services.
- Managing documentary operations.
- Assigning personnel.
- Managing assigned warehouses.
- Tracking project status.
- Recording operational history.
- Controlling project dates.
- Coordinating documentary reception.
- Serving as the operational context for documentary units.

---

# Business Justification

A single contract may generate multiple independent projects.

Examples:

- Documentary Organization
- Digitization
- Equipment Rental
- Technical Information Management

Each project has its own operational lifecycle while sharing the same contract.

---

# Aggregate Root

Project

---

# Business Identity

Every project must have a unique identifier.

The identifier never changes during the project lifecycle.

---

# Lifecycle

Draft

↓

Planned

↓

Active

↓

Suspended

↓

Completed

↓

Closed

↓

Archived

---

# Invariants

The following rules must always be true.

- Every Project belongs to exactly one Contract.
- Every Project belongs to exactly one Customer through its Contract.
- A Project cannot exist without an active Contract.
- A closed Project cannot receive new documentary material.
- An archived Project cannot be modified.
- Every documentary reception belongs to one Project.
- Every documentary loan belongs to one Project.
- Every assigned warehouse must belong to the Project during a valid time period.

---

# Main Responsibilities

The Project Aggregate coordinates:

- Documentary Reception
- Documentary Processing
- Documentary Storage
- Documentary Loans
- Digitization
- Operational Billing
- Personnel Assignment

The aggregate does not execute these processes directly.

It orchestrates them.

---

# Domain Events

- ProjectCreated
- ProjectStarted
- ProjectSuspended
- ProjectResumed
- ProjectCompleted
- ProjectClosed
- ProjectArchived
- CoordinatorAssigned
- WarehouseAssigned
- WarehouseReleased

---

# Related Aggregates

- Customer
- Contract
- Documentary Reception
- Warehouse
- Documentary Unit
- Billing

---

# Pending Definitions

The following business concepts require validation.

- Official AGN metadata for documentary units.
- Official documentary file identification rules.
- Barcode standard.
- Physical labeling standard.

---

# Version History

| Version | Date | Description |
|----------|------|-------------|
|1.0.0|2026-07-11|Initial Project Aggregate.|
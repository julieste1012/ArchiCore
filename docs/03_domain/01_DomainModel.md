# Domain Model

| Campo | Valor |
|--------|-------|
| Documento | 01_DomainModel |
| Versión | 0.1.0 |
| Estado | Draft |
| Fase | 2 - Software Architecture |
| Sprint | 2.1 |
| Última actualización | 2026-07-11 |

---

# Purpose

The Domain Model represents the business concepts of the ERP independently of databases, frameworks or user interfaces.

It defines the language of the business and serves as the foundation for software implementation.

---

# Domain Modeling Principles

The model follows these principles:

- Business first.
- Persistence ignorance.
- High cohesion.
- Low coupling.
- Explicit business rules.
- Rich domain model.
- Separation of concerns.

---

# Bounded Contexts

The ERP is divided into the following bounded contexts.

## Core

Shared concepts used across the entire platform.

Examples:

- Users
- Roles
- Permissions
- Audit
- Configuration
- Notifications

---

## Customer Management

Responsible for customer information.

---

## Contract Management

Responsible for contracts and contracted services.

---

## Documentary Management

Core business context.

Responsible for documentary lifecycle.

---

## Warehouse Management

Responsible for physical storage.

---

## Billing

Responsible for billable concepts.

---

## Equipment

Responsible for equipment rental and assignment.

---

# Aggregate Roots

The following aggregate roots have been identified.

| Aggregate | Status |
|------------|--------|
| Customer | Candidate |
| Contract | Candidate |
| Project | Candidate |
| Warehouse | Candidate |
| Documentary Unit | Candidate |
| Documentary Reception | Candidate |
| Documentary Loan | Candidate |
| Billing Period | Candidate |

---

# Domain Evolution

The domain model evolves incrementally.

New entities are incorporated only after being justified by business processes.

The model is the single source of truth for business concepts.

---

# Version History

| Version | Date | Description |
|----------|------|-------------|
| 0.1.0 | 2026-07-11 | Initial domain model structure. |
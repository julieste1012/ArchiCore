# Master Blueprint

| Campo | Valor |
|--------|-------|
| Documento | 01_MasterBlueprint |
| Versión | 0.1.0 |
| Estado | Draft |
| Fase | 1 - Business Analysis |
| Sprint | 1.1 |
| Última actualización | 2026-07-11 |

---

# 1. Purpose

The Master Blueprint defines the architectural vision, engineering principles and technical direction of the DMS ERP project.

It establishes the rules that every future module, service, database object, API and user interface must follow.

This document is the highest-level technical reference for the entire project.

---

# 2. Project Vision

The objective is to build a modern, modular and maintainable ERP platform specialized in Document Management for companies providing documentary administration services.

The first implementation targets MULTISERTEC LTDA, but the architecture must remain generic enough to support future organizations with similar business models.

The platform will centralize operational processes, documentary assets and administrative information into a single web application.

---

# 3. Design Philosophy

Every architectural decision in this project follows five fundamental ideas.

## Business Driven

Technology exists to support the business.

Business requirements always drive technical decisions.

---

## Simplicity

Choose the simplest solution that correctly solves the problem.

Avoid unnecessary frameworks, abstractions and patterns.

---

## Evolution

The architecture must support continuous evolution without requiring major redesigns.

---

## Separation of Concerns

Business rules, infrastructure and presentation must remain independent.

Each layer has a single responsibility.

---

## Documentation First

Every important decision must be documented before implementation.

Documentation is considered part of the software.

---

# 4. Core Principles

The project adopts the following engineering principles.

## Maintainability

Code should be understandable after several years.

---

## Modularity

Each module should evolve independently whenever possible.

---

## Scalability

Growth should require configuration changes before architectural changes.

---

## Traceability

Every important business operation must be auditable.

---

## Security by Design

Security is incorporated from the beginning instead of being added later.

---

## Consistency

The same architectural patterns must be used throughout the project.

---

## Testability

Business logic should be testable independently from infrastructure.

---

## Long-Term Thinking

Short-term shortcuts that generate technical debt are avoided whenever possible.

---

# 5. System Architecture

The DMS ERP is designed as a **Modular Monolith**.

The system is deployed as a single application while keeping clear boundaries between business modules.

Each module owns its business logic and communicates through well-defined interfaces.

This architecture provides:

- Simpler deployment.
- Lower operational complexity.
- Better maintainability.
- Easier debugging.
- Future migration to microservices if business growth requires it.

The architecture favors modularity over distribution.

---

# 6. Architectural Layers

The system follows a layered architecture.

Presentation Layer

Responsible for user interaction.

Technology:

- React
- TypeScript
- Tailwind CSS

Responsibilities:

- User interface
- Forms
- Validation
- Navigation

---

Application Layer

Coordinates business use cases.

Responsibilities:

- Execute business workflows
- Validation orchestration
- Transactions
- Authorization

---

Domain Layer

Contains the business rules.

Responsibilities:

- Entities
- Value Objects
- Business Rules
- Domain Services

This layer must not depend on infrastructure.

---

Infrastructure Layer

Provides technical services.

Responsibilities:

- Database
- File Storage
- Authentication
- Logging
- Email
- External integrations

Infrastructure depends on the Domain, never the opposite.

---

Persistence Layer

Responsible for data storage.

Technology:

- PostgreSQL
- Entity Framework Core

Responsibilities:

- Repositories
- Migrations
- Queries
- Transactions

---

# 7. Domain Strategy

The ERP is organized around business domains rather than technical components.

Each domain represents a business capability.

The initial domains are:

## Customer Management

Administration of contracting companies.

---

## Contract Management

Management of contracts, services, validity periods and capacities.

---

## Warehouse Management

Administration of warehouses, aisles, shelves, cabinets and physical locations.

---

## Documentary Management

Management of documentary hierarchy according to AGN.

Series

↓

Subseries

↓

Expedients

↓

Folders

↓

Documents

↓

Folios

---

## Barcode Management

Generation and validation of unique identifiers.

Tracking of physical assets.

---

## Documentary Loans

Control of physical and digital loans.

Authorization.

Return.

History.

Traceability.

---

## Digitization

Management of digitization requests.

Control of generated digital files.

Association with physical records.

---

## Billing

Support for monthly billing.

Calculation by:

- Folder
- Meter
- Hours
- Digitization
- Other contractual concepts

Accounting remains managed in SIIGO.

---

## User Management

Authentication.

Authorization.

Roles.

Permissions.

---

## Audit

Registration of every relevant business event.

No critical operation should occur without leaving an audit trail.


# Version History

| Version | Date | Description |
|----------|------|-------------|
| 0.1.0 | 2026-07-11 | Initial structure and architectural principles. |
| 0.2.0 | 2026-07-11 | Defined architecture, layers and business domains. |
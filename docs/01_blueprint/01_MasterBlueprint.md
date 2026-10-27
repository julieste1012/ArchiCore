# Master Blueprint

| Campo | Valor |
|--------|-------|
| Documento | 01_MasterBlueprint |
| Versión | 1.0.1 |
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

---

# 8. Modular Structure

The ERP is organized into independent business modules.

Each module encapsulates its own business rules, services, repositories and user interfaces.

Modules communicate through application services instead of directly accessing each other's data.

Initial modules include:

## Core

Shared kernel of the system.

Responsibilities:

- Authentication
- Authorization
- Configuration
- Notifications
- Logging
- Audit
- Shared utilities

---

## Customers

Responsible for contractor companies.

Main responsibilities:

- Customer information
- Contacts
- Status
- Business relationships

---

## Contracts

Responsible for contractual information.

Responsibilities:

- Contract lifecycle
- Services
- Storage capacity
- Billing configuration
- Contract validity

---

## Warehouses

Responsible for physical storage.

Responsibilities:

- Warehouses
- Aisles
- Shelves
- Cabinets
- Physical locations

---

## Documentary Management

The central module of the ERP.

Responsibilities:

- Documentary hierarchy
- Registration
- Classification
- Barcode association
- Physical location
- Documentary status

---

## Loans

Controls document loans.

Responsibilities:

- Requests
- Authorization
- Delivery
- Returns
- Overdue tracking

---

## Digitization

Manages digitization requests and digital assets.

---

## Billing

Supports operational billing.

It calculates billable concepts according to contractual agreements.

Accounting remains external through SIIGO.

---

## Administration

Responsible for:

- Users
- Roles
- Permissions
- System configuration

---

## Dashboard

Provides operational and managerial indicators.

---

# Module Independence

Each module must:

- expose services
- own its business logic
- avoid direct dependencies
- expose only necessary interfaces

Business modules must never become tightly coupled.

---

# 9. Technology Stack

Frontend

- React
- TypeScript
- Vite
- Tailwind CSS
- React Router
- TanStack Query
- React Hook Form
- Zod

---

Backend

- ASP.NET Core 10 (.NET 10 LTS, target framework `net10.0`)
- C#
- Entity Framework Core
- FluentValidation

---

Database

- PostgreSQL

---

Storage

- MinIO (S3 compatible)

Local Storage implementation may be used during early development while preserving the abstraction layer.

---

Authentication

- JWT
- Refresh Tokens

---

Infrastructure

- Docker
- Docker Compose

---

Development

- Git
- GitHub

---

Documentation

Markdown

---

# Technology Selection Criteria

Technologies must satisfy:

- Long-term support
- Strong community
- Good documentation
- High performance
- Enterprise adoption
- Active maintenance

---

# 10. Security Strategy

Security is implemented from the first iteration.

The following principles are mandatory.

Authentication

JWT with Refresh Tokens.

---

Authorization

Role-Based Access Control (RBAC).

Permissions are assigned to roles.

Users inherit permissions through roles.

---

Password Policy

Passwords must:

- be hashed
- never be stored in plain text
- use strong hashing algorithms

---

Audit

Critical operations generate immutable audit records.

Examples:

- Login
- Logout
- Document loan
- Contract creation
- Deletion attempts
- Permission changes

---

Input Validation

Every request must be validated on the server.

Client validation is only for user experience.

---

File Security

Uploaded files must:

- be validated
- receive unique identifiers
- never trust client filenames

---

Least Privilege

Users receive only the permissions required to perform their work.

---

# 11. Scalability Strategy

The architecture prioritizes vertical scalability during the first stages.

Future horizontal scaling must be possible without redesigning business modules.

The ERP should support:

- increasing document volume
- increasing customers
- increasing warehouses
- increasing contracts

without changing the domain architecture.

Future improvements may include:

- distributed caching
- asynchronous processing
- search indexing
- load balancing

These improvements should not require changes to business logic.
---

# 12. Performance Strategy

Performance is considered a design requirement.

The system must remain responsive while managing large documentary collections.

## Performance Principles

- Optimize database queries.
- Avoid unnecessary data loading.
- Use pagination for large datasets.
- Use asynchronous operations whenever appropriate.
- Minimize network requests.
- Cache data only when justified.
- Index frequently queried database columns.

The objective is predictable performance rather than premature optimization.

---

# 13. Coding Standards

All source code must follow the project coding standards.

General principles:

- Clean Architecture.
- SOLID principles.
- DRY (Don't Repeat Yourself).
- KISS (Keep It Simple).
- YAGNI (You Aren't Gonna Need It).

Business rules must never be duplicated.

Every public class, API endpoint and service should have a single responsibility.

---

# 14. Documentation Strategy

Documentation evolves together with the software.

Documentation is version-controlled inside the repository.

Every architectural decision must be documented before implementation.

The documentation hierarchy is:

Foundation

↓

Blueprint

↓

Business

↓

Architecture

↓

Database

↓

Backend

↓

Frontend

↓

Deployment

If documentation and implementation differ, documentation must be reviewed immediately.

---

# 15. Testing Strategy

Testing is implemented progressively.

Testing levels:

## Unit Tests

Validate business rules.

---

## Integration Tests

Validate interaction between modules.

---

## API Tests

Validate REST endpoints.

---

## End-to-End Tests

Validate complete business processes.

---

Business-critical modules should always receive priority.

Testing focuses on reliability rather than test quantity.

---

# 16. Deployment Strategy

The application is deployed using Docker.

Initial deployment consists of:

- Frontend
- Backend
- PostgreSQL
- MinIO

Future environments:

- Development
- Testing
- Production

Deployment must remain reproducible.

Configuration is environment-based.

---

# 17. Roadmap

The ERP will be developed incrementally.

Phase 1

Business Analysis

---

Phase 2

Software Architecture

---

Phase 3

Database Design

---

Phase 4

Backend Development

---

Phase 5

Frontend Development

---

Phase 6

Testing

---

Phase 7

Deployment

---

Phase 8

Maintenance

Each phase produces documented deliverables before implementation continues.

---

# 18. Quality Attributes

The architecture prioritizes the following quality attributes.

- Maintainability
- Reliability
- Security
- Scalability
- Traceability
- Performance
- Availability
- Modularity
- Testability
- Usability

Architectural decisions should improve these attributes whenever possible.

---

# 19. Risks

Known architectural risks include:

- Requirement changes.
- Regulatory updates.
- Business growth.
- Large document volumes.
- External integrations.
- Single developer dependency.

The architecture should minimize the impact of these risks.

---

# 20. Architecture Decision Records

Significant architectural decisions will be documented as ADRs.

Initial ADR list:

ADR-001 Modular Monolith

ADR-002 Modular Business Domains

Future ADRs include:

- Authentication Strategy
- Database Strategy
- File Storage Strategy
- API Versioning
- Logging Strategy
- Error Handling Strategy
- Backup Strategy

Every major architectural change requires a new ADR.

---

# Conclusion

This Master Blueprint establishes the architectural foundation of the DMS ERP project.

All future design and implementation decisions must remain consistent with the principles defined in this document.

The blueprint is a living document and may evolve through controlled revisions as the project matures.

---

# Version History

| Version | Date | Description |
|----------|------|-------------|
| 0.1.0 | 2026-07-11 | Initial structure and architectural principles. |
| 0.2.0 | 2026-07-11 | Defined architecture, layers and business domains. |
| 0.3.0 | 2026-07-11 | Defined modules, technology stack, security and scalability strategy. |
| 1.0.0 | 2026-07-11 | Completed the Master Blueprint. |
| 1.0.1 | 2026-10-26 | Aligned backend technology stack with the implemented .NET 10 LTS target; see ADR-003. |

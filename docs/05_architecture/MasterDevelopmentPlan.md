# Master Development Plan

## Fase 0 - Fundación ✅

- Entorno de desarrollo
- Git y GitHub
- Estructura del repositorio
- Convenciones
- Documentación base

---

## Fase 1 - Modelado del Negocio ✅

- Ubiquitous Language
- Domain Map
- Aggregate Design

---

## Fase 2 - Dominio

### Infraestructura Base ✅

- Entity
- ValueObject
- CodeValueObject
- Result
- DomainEvent

### Value Objects

- Barcode
- ContractNumber
- ProjectCode
- FolderCode
- BoxCode
- WarehouseCode
- PersonName
- Email
- PhoneNumber
- Address
- Nit
- Money

### Aggregate Roots

- Contract
- Project
- Warehouse
- Loan
- User

### Entities

- Box
- Folder
- Document
- Annex

### Domain Events

- ContractCreated
- ProjectCreated
- FolderMoved
- BoxMoved
- LoanCreated
- LoanReturned
- DocumentDigitized

---

## Fase 3 - Application

- Commands
- Queries
- DTOs
- Validators
- Interfaces
- Use Cases

---

## Fase 4 - Infrastructure

- EF Core
- PostgreSQL
- Repositories
- MinIO
- JWT
- Refresh Tokens

---

## Fase 5 - API

- Controllers
- Authentication
- Authorization
- Swagger
- Versionado

---

## Fase 6 - Frontend

- React
- Layout
- Login
- Dashboard
- Módulos

---

## Fase 7 - Testing

- Unit Tests
- Integration Tests
- Architecture Tests

---

## Fase 8 - Docker

- Backend
- Frontend
- PostgreSQL
- MinIO

---

## Fase 9 - Producción

- CI/CD
- Logs
- Monitoring
- Backups
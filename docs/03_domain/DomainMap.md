# Domain Map

## Objetivo

Este documento identifica todos los conceptos principales del dominio y su clasificación dentro de la arquitectura DDD.

---

# Aggregate Roots

- Contract
- Project
- User
- Warehouse
- Loan

---

# Entities

- Folder
- Box
- Document
- Annex


---

# Value Objects

## Identificadores

- Barcode
- ContractNumber
- ProjectCode
- FolderCode
- BoxCode
- WarehouseCode

## Personas

- PersonName
- Email
- PhoneNumber

## Empresa

- Nit
- Address

## Financieros

- Money

---

# Domain Services

- LoanAuthorizationService
- StorageAllocationService
- DocumentDigitizationService

---

# Domain Events

- ContractCreated
- ProjectCreated
- FolderMoved
- BoxMoved
- LoanCreated
- LoanReturned
- DocumentDigitized

---

# Repositories

- IContractRepository
- IProjectRepository
- IUserRepository
- IWarehouseRepository
- ILoanRepository

---

# Relaciones principales

Cliente
└── Contrato
    └── Proyecto
        ├── Carpetas
        │   └── Documentos
        │       └── Anexos
        └── Coordinador

Bodega
└── Caja
    └── Carpetas

Usuario
└── Préstamos
# Nota
un coordinador es un usuario con el rol Coordinator.
# Domain Map

## Objetivo

Identificar los conceptos principales de ArchiCore y sus límites actuales de consistencia. La clasificación indicada es la implementada de forma provisional en el código; los puntos pendientes se mantienen en DOMAIN_IMPLEMENTATION_ASSUMPTIONS.md.

## Aggregate Roots

- Customer
- Contract
- Project
- Warehouse
- Box
- Archive
- Folder
- DocumentaryReception
- DigitizationRequest
- Loan (pendiente de implementación completa)
- UserAccount (identidad/autenticación técnica, no sustituye el futuro agregado de personal)

## Entities internas

- WarehouseLocation (dentro de Warehouse)
- ArchiveSeries, ArchiveSubseries, Expedient (dentro de Archive)
- FolderDocument, Folio, Annex (dentro de Folder)

## Referencias entre agregados

- Customer -> Contract por ContractId
- Contract -> Project por ProjectId
- Project -> Archive por ProjectId
- Archive -> Expedient (árbol interno de clasificación)
- Folder -> ArchiveId + ExpedientId
- Folder -> BoxId
- Box -> FolderIds
- Box -> WarehouseLocationId (identidad física)
- DocumentaryReception -> ContractId + CustomerId + ProjectId
- DigitizationRequest -> ProjectId + FolderId
- Loan -> ProjectId + FolderId/BoxId (modelo pendiente)

Las relaciones bidireccionales Folder.BoxId y Box.FolderIds deben sincronizarse en un único caso de uso transaccional. Infrastructure debe impedir más de una asignación activa de caja para la misma carpeta.

## Value Objects

### Identificadores
- Barcode
- ContractNumber
- ProjectCode
- FolderCode
- BoxCode
- WarehouseCode

### Personas y contacto
- PersonName
- Email
- PhoneNumber

### Empresa
- Nit
- Address

### Financieros
- Money

## Domain Services

- LoanAuthorizationService (pendiente)
- StorageAllocationService (pendiente)
- DocumentDigitizationService (pendiente de integración de Application/Infrastructure)

## Domain Events

Consultar 02_DomainEvents.md como catálogo central.

## Repositories

- ICustomerRepository
- IContractRepository
- IProjectRepository
- IWarehouseRepository
- IBoxRepository (pendiente)
- IArchiveRepository (pendiente)
- IFolderRepository (pendiente)
- IDigitizationRequestRepository (pendiente)
- ILoanRepository (pendiente)

## Relaciones conceptuales

Customer
└── Contracts
    └── Projects
        ├── Archive
        │   └── Series
        │       └── Subseries
        │           └── Expedients
        │               └── Folders
        │                   └── Documents
        │                       ├── Folios
        │                       └── Annexes
        ├── Documentary Receptions
        ├── Digitization Requests
        └── Loans

Warehouse
└── WarehouseLocations
    └── Boxes
        └── Folder references

UserAccount
└── Security sessions and audit

> The tree above describes conceptual relationships. Aggregate references are by identity and do not imply that a child entity is physically persisted inside every displayed parent.

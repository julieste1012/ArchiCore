# Project Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Project Aggregate |
| Versión | 2.0.0 |
| Estado | Draft |
| Contexto | Contract Management |
| Aggregate Root | Project |

---

# Propósito

El Aggregate `Project` representa la ejecución operativa de un contrato.

Un contrato (`Contract`) define el acuerdo comercial entre MULTISERTEC y su cliente. Un proyecto representa el trabajo operativo requerido para cumplir uno o más servicios contratados.

Toda operación documental pertenece a un proyecto.

## Justificación de negocio

Un mismo contrato puede generar múltiples proyectos independientes. Ejemplos:

- Organización Documental
- Digitalización
- Alquiler de Equipos
- Gestión de Información Técnica

Cada proyecto posee su propio ciclo de vida operativo, aunque comparta el mismo contrato.

---

# Responsabilidades

- Ejecutar los servicios contratados.
- Gestionar operaciones documentales.
- Asignar personal (coordinador).
- Gestionar bodegas asignadas, con su vigencia temporal.
- Rastrear el estado del proyecto.
- Registrar historial operacional.
- Controlar las fechas propias de ejecución del proyecto.
- Coordinar la recepción documental.
- Servir como contexto operativo para las unidades documentales (Carpetas, Cajas, Documentos, Anexos, según `DomainMap.md`).

El Aggregate **coordina** estos procesos; no los ejecuta directamente. La ejecución detallada de Recepción Documental, Procesamiento Documental, Almacenamiento Documental, Préstamos Documentales, Digitalización y Facturación Operacional corresponde a sus propios Aggregates/Servicios de dominio.

---

# Business Identity

Todo proyecto posee un identificador único (`Id`), que nunca cambia durante su ciclo de vida.

`ProjectCode` (Value Object ya implementado en el dominio, clasificado en `DomainMap.md` bajo Value Objects → Identificadores) se adopta como el código de negocio legible del proyecto, siguiendo el mismo patrón ya utilizado por `Contract` con `ContractNumber`. La unicidad de `ProjectCode` se garantiza dentro del alcance de su contrato (invariante ya definida y exigida en `ContractAggregate.md`/`Contract.AddProject()`); `Project` no debe reimplementar esta validación para evitar duplicar una regla que ya es responsabilidad de `Contract`.

---

# Atributos del Aggregate

Propiedades que pertenecen realmente al Aggregate `Project` (no son referencias a otro Aggregate):

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único e inmutable del proyecto. |
| ProjectCode | ProjectCode | `ProjectCode` | Sí | Código de negocio legible del proyecto (prefijo `PRJ-`). |
| Status | ProjectStatus (enum) | N/A (enum de dominio) | Sí | Estado actual del proyecto dentro de su ciclo de vida. |
| StartDate | DateTime? | N/A | No (se asigna al iniciar el proyecto, vía `Start()`) | Fecha en la que el proyecto pasa a ejecución activa. |
| EndDate | DateTime? | N/A | No (se asigna al completar el proyecto, vía `Complete()`) | Fecha en la que el proyecto se da por completado operativamente. |

**Referencias a otros Aggregates (no son atributos propios del proyecto, sino identidad de otro Aggregate Root):**

| Nombre | Tipo | Obligatorio | Descripción |
|--------|------|-------------|-------------|
| ContractId | Guid | Sí | Referencia al `Contract` al que pertenece el proyecto. Invariante: "Every Project belongs to exactly one Contract"; "A Project cannot exist without an active Contract". |
| CoordinatorId | Guid? | No | Referencia al `User` (con rol Coordinator, según `UbiquitousLanguage.md`) actualmente responsable del proyecto. Puede cambiar durante la vida del proyecto (evento `CoordinatorAssigned`). |

**Nota de bajo acoplamiento:** `Project` **no** almacena `CustomerId`. La invariante "Every Project belongs to exactly one Customer through its Contract" es transitiva: el cliente se resuelve navegando `Project → Contract → Customer`, evitando duplicar en `Project` una relación que ya pertenece a `Contract`.

---

# Colecciones internas

| Nombre | Tipo de elemento | Descripción |
|--------|-------------------|-------------|
| AssignedWarehouses | `ProjectWarehouseAssignment` (WarehouseId, AssignedFrom, ReleasedOn?) | Colección de asignaciones de bodega al proyecto, cada una con su vigencia temporal. Respaldada por la invariante "Every assigned warehouse must belong to the Project during a valid time period" y los eventos `WarehouseAssigned` / `WarehouseReleased`. Almacena únicamente la referencia (`WarehouseId`) y las fechas de vigencia — no el Aggregate `Warehouse` completo, para mantener bajo acoplamiento (mismo criterio ya usado en `Contract.Projects` con `ContractProjectReference`). |

**No se modelan como colecciones internas** (son relaciones inversas, gestionadas desde el otro Aggregate):

- Recepciones documentales (`Every documentary reception belongs to one Project"` — la Recepción referencia a `Project`, no al revés).
- Préstamos documentales (`Every documentary loan belongs to one Project"` — el Préstamo referencia a `Project`, no al revés).

---

# Lifecycle

- Draft
- Planned
- Active
- Suspended
- Completed
- Closed
- Archived

---

# Diagrama de estados

```
Draft ──Plan()──▶ Planned ──Start()──▶ Active ──Complete()──▶ Completed ──Close()──▶ Closed ──Archive()──▶ Archived
                                          │▲
                                Suspend() ││ Resume()
                                          ▼│
                                       Suspended
```

- `Draft → Planned` mediante `Plan()`. No existe un evento de dominio dedicado a esta transición (no hay un evento equivalente a "ProjectPlanned" en la documentación fuente); se trata como una transición interna de preparación, sin significancia de negocio suficiente para un evento propio.
- `Planned → Active` mediante `Start()`, emite `ProjectStarted`.
- `Active → Suspended` mediante `Suspend()`, emite `ProjectSuspended`.
- `Suspended → Active` mediante `Resume()`, emite `ProjectResumed`.
- `Active → Completed` mediante `Complete()`, emite `ProjectCompleted`.
- `Completed → Closed` mediante `Close()`, emite `ProjectClosed`.
- `Closed → Archived` mediante `Archive()`, emite `ProjectArchived`.

---

# Invariantes

- Every Project belongs to exactly one Contract.
- Every Project belongs to exactly one Customer through its Contract.
- A Project cannot exist without an active Contract.
- A closed Project cannot receive new documentary material.
- An archived Project cannot be modified.
- Every documentary reception belongs to one Project.
- Every documentary loan belongs to one Project.
- Every assigned warehouse must belong to the Project during a valid time period.
- Un proyecto solo puede iniciarse (`Start()`) desde el estado `Planned`.
- Un proyecto solo puede suspenderse (`Suspend()`) desde el estado `Active`.
- Un proyecto solo puede reanudarse (`Resume()`) desde el estado `Suspended`.
- Un proyecto solo puede completarse (`Complete()`) desde el estado `Active`.
- Un proyecto solo puede cerrarse (`Close()`) desde el estado `Completed`.
- Un proyecto solo puede archivarse (`Archive()`) desde el estado `Closed`.
- Un proyecto solo puede tener un coordinador asignado a la vez; asignar uno nuevo reemplaza al anterior.
- Una asignación de bodega debe tener una fecha de inicio de vigencia (`AssignedFrom`); si se libera, la fecha de liberación (`ReleasedOn`) no puede ser anterior a `AssignedFrom`.
- No se puede asignar una bodega que ya se encuentra actualmente asignada (sin liberar) al mismo proyecto.

---

# Reglas de negocio

- La unicidad de `ProjectCode` dentro de un contrato es responsabilidad de `Contract` (ver `Contract.AddProject()`); `Project` no revalida esta regla para evitar duplicidad de lógica entre Aggregates.
- El coordinador asignado autoriza préstamos documentales y aprueba procesos internos operativos del proyecto (según `UbiquitousLanguage.md`); esta autoridad se ejercita desde los Aggregates correspondientes (Préstamo, procesos internos), no mediante métodos adicionales en `Project`.
- Un proyecto `Closed` no admite nuevo material documental, pero conserva su historial operativo (coherente con la misma política de conservación de historial ya aplicada a `Customer`).

---

# Métodos públicos del Aggregate

- `Create(ProjectCode projectCode, Guid contractId)`
- `Plan()`
- `Start()`
- `Suspend()`
- `Resume()`
- `Complete()`
- `Close()`
- `Archive()`
- `AssignCoordinator(Guid coordinatorId)`
- `AssignWarehouse(Guid warehouseId, DateTime assignedFrom)`
- `ReleaseWarehouse(Guid warehouseId, DateTime releasedOn)`

---

# Métodos privados (si aplica)

- `EnsureStatusIs(ProjectStatus expected)` — validación interna reutilizada por los métodos de transición de estado, análoga al patrón de validación por estado ya usado en `Contract`.
- `ContainsActiveWarehouseAssignment(Guid warehouseId)` — encapsula la verificación de si una bodega ya se encuentra asignada (sin liberar) al proyecto, análoga a `Contract.ContainsProject()` y `Customer.ContainsContract()`.

---

# Factory Method

`Create(ProjectCode projectCode, Guid contractId)`

- Crea un nuevo proyecto en estado `Draft`.
- Requiere `projectCode` y `contractId` no vacíos/nulos (`ContractId` obligatorio por invariante).
- Registra el evento `ProjectCreated`.

---

# Domain Events

- `ProjectCreated`
- `ProjectStarted`
- `ProjectSuspended`
- `ProjectResumed`
- `ProjectCompleted`
- `ProjectClosed`
- `ProjectArchived`
- `CoordinatorAssigned`
- `WarehouseAssigned`
- `WarehouseReleased`

**Nota de consistencia:** `02_DomainEvents.md` no incluye actualmente una categoría "Project Events", y `DomainMap.md` solo cataloga `ProjectCreated`. Ambos documentos deben actualizarse para reflejar el catálogo completo aquí definido; esta versión de `ProjectAggregate.md` es la fuente autoritativa para los eventos de `Project`.

---

# Relaciones con otros Aggregates

- **Contract → Project**: relación estructural obligatoria. `Project` almacena `ContractId`. Un contrato puede tener múltiples proyectos (ya modelado desde `Contract` mediante `ContractProjectReference`).
- **Customer → Project**: relación conceptual/transitiva únicamente, resuelta vía `Project.ContractId → Contract.ClientId`. `Project` no almacena `CustomerId` (ver "Atributos del Aggregate").
- **User (Coordinator) → Project**: `Project` almacena una referencia opcional y mutable (`CoordinatorId`) al `User` actualmente asignado como coordinador.
- **Warehouse → Project**: relación estructural mediante la colección interna `AssignedWarehouses`, que referencia bodegas por `WarehouseId` junto con su vigencia temporal. No se embebe el Aggregate `Warehouse` completo.
- **Documentary Reception → Project** y **Documentary Loan → Project**: relaciones inversas; ambos Aggregates referencian a `Project`, no al contrario.
- **Billing → Project**: relación mencionada únicamente a nivel conceptual. La naturaleza exacta de la relación con `Billing` / `Billing Period` permanece pendiente (ver "Pendientes").

---

# Value Objects utilizados

- `ProjectCode` — código de negocio del proyecto.

**No se utilizan en este Aggregate** (pertenecen a otros Aggregates y no deben duplicarse aquí): `ContractNumber` (Contract), `WarehouseCode` (Warehouse), `Nit` (Customer), `Money` (Contract).

---

# Pendientes

- Definir metadatos oficiales AGN para unidades documentales.
- Definir reglas oficiales de identificación de expedientes documentales.
- Definir estándar de código de barras.
- Definir estándar de etiquetado físico.
- Confirmar si "Documentary Unit" (mencionado en la versión 1.0.0 de este documento) corresponde al conjunto de entidades `Folder`, `Box`, `Document` y `Annex` ya definidas en `DomainMap.md`/`UbiquitousLanguage.md`, o si es un concepto distinto pendiente de definición formal.
- Precisar la relación estructural (si existe) entre `Project` y `Billing` / `Billing Period`.
- Actualizar `02_DomainEvents.md` y `DomainMap.md` para reflejar el catálogo completo de eventos de `Project` definido en esta versión.
- Confirmar si `StartDate`/`EndDate` del proyecto requieren validaciones cruzadas contra `Contract.StartDate`/`Contract.EndDate` (por ejemplo, que la ejecución del proyecto no exceda la vigencia del contrato); no se documenta actualmente ninguna regla en ese sentido y no se asume.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Initial Project Aggregate. |
| 2.0.0 | 2026-07-21 | Enriquecimiento del Aggregate: se incorporan Atributos, Colecciones internas, Diagrama de estados, Métodos públicos y privados, Factory Method, y precisión de las Relaciones con otros Aggregates y Value Objects utilizados, con base en `ContractAggregate.md`, `CustomerAggregate.md`, `DomainMap.md`, `UbiquitousLanguage.md`, `01_DomainModel.md` y las Specifications ya generadas para `Contract`, `Customer` y `Project`. |

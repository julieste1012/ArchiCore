# Project Specification

| Campo | Valor |
|--------|-------|
| Documento | ProjectSpecification |
| Versión | 2.0.0 |
| Aggregate | Project |
| Bounded Context | Contract Management |
| Fuentes | ProjectAggregate.md v2.0 (autoritativa), ContractAggregate.md, CustomerAggregate.md, ContractSpecification.md, CustomerSpecification.md, DomainMap.md, UbiquitousLanguage.md, 01_DomainModel.md, 02_DomainEvents.md |
| Estado | Draft |

> Este documento consolida, sin añadir reglas nuevas, lo ya definido en `ProjectAggregate.md` versión 2.0. Donde exista diferencia entre documentos, `ProjectAggregate.md v2.0` se considera la fuente autoritativa. Los puntos que dicho documento deja explícitamente pendientes se señalan aquí como **Punto abierto**.

---

# Propósito

Representar la ejecución operativa de un contrato. Un contrato (`Contract`) define el acuerdo comercial entre MULTISERTEC y su cliente; un proyecto representa el trabajo operativo requerido para cumplir uno o más servicios contratados. Toda operación documental pertenece a un proyecto.

Un mismo contrato puede generar múltiples proyectos independientes (por ejemplo: Organización Documental, Digitalización, Alquiler de Equipos, Gestión de Información Técnica), cada uno con su propio ciclo de vida operativo.

---

# Aggregate Root

`Project`

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

El Aggregate coordina estos procesos; no los ejecuta directamente. La ejecución detallada de Recepción Documental, Procesamiento Documental, Almacenamiento Documental, Préstamos Documentales, Digitalización y Facturación Operacional corresponde a sus propios Aggregates/Servicios de dominio.

---

# Propiedades

Propiedades que pertenecen realmente al Aggregate `Project`:

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único e inmutable del proyecto. |
| ProjectCode | ProjectCode | `ProjectCode` | Sí | Código de negocio legible del proyecto (prefijo `PRJ-`). Su unicidad dentro del contrato es responsabilidad de `Contract` (`Contract.AddProject()`), no de `Project`. |
| Status | ProjectStatus (enum) | N/A (enum de dominio) | Sí | Estado actual del proyecto dentro de su ciclo de vida. |
| StartDate | DateTime? | N/A | No — se asigna al invocar `Start()` | Fecha en la que el proyecto pasa a ejecución activa. |
| EndDate | DateTime? | N/A | No — se asigna al invocar `Complete()` | Fecha en la que el proyecto se da por completado operativamente. |

---

# Referencias a otros Aggregates

Identidad de otros Aggregate Roots almacenada por `Project` (no son atributos propios, sino referencias):

| Nombre | Tipo | Obligatorio | Descripción |
|--------|------|-------------|-------------|
| ContractId | Guid | Sí | Referencia al `Contract` al que pertenece el proyecto. Invariante: "Every Project belongs to exactly one Contract"; "A Project cannot exist without an active Contract". |
| CoordinatorId | Guid? | No | Referencia al `User` (rol Coordinator, según `UbiquitousLanguage.md`) actualmente responsable del proyecto. Puede cambiar durante la vida del proyecto (evento `CoordinatorAssigned`). |

**Nota de bajo acoplamiento (confirmada en `ProjectAggregate.md v2.0`):** `Project` no almacena `CustomerId`. La invariante "Every Project belongs to exactly one Customer through its Contract" es transitiva y se resuelve navegando `Project → Contract → Customer`.

---

# Colecciones

| Nombre | Tipo de elemento | Descripción |
|--------|-------------------|-------------|
| AssignedWarehouses | `ProjectWarehouseAssignment` (WarehouseId, AssignedFrom, ReleasedOn?) | Colección de asignaciones de bodega al proyecto, cada una con su vigencia temporal. Respaldada por la invariante "Every assigned warehouse must belong to the Project during a valid time period" y los eventos `WarehouseAssigned` / `WarehouseReleased`. Almacena únicamente `WarehouseId` y las fechas de vigencia, no el Aggregate `Warehouse` completo. |

**No se modelan como colecciones internas** (relaciones inversas, gestionadas desde el otro Aggregate):

- Recepciones documentales ("Every documentary reception belongs to one Project" — la Recepción referencia a `Project`, no al revés).
- Préstamos documentales ("Every documentary loan belongs to one Project" — el Préstamo referencia a `Project`, no al revés).

---

# Estados

- Draft
- Planned
- Active
- Suspended
- Completed
- Closed
- Archived

---

# Diagrama de transiciones

```
Draft ──Plan()──▶ Planned ──Start()──▶ Active ──Complete()──▶ Completed ──Close()──▶ Closed ──Archive()──▶ Archived
                                          │▲
                                Suspend() ││ Resume()
                                          ▼│
                                       Suspended
```

- `Draft → Planned` mediante `Plan()`. No existe evento de dominio dedicado a esta transición (no hay equivalente a "ProjectPlanned"); se trata como transición interna de preparación sin significancia de negocio suficiente para un evento propio.
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

- La unicidad de `ProjectCode` dentro de un contrato es responsabilidad de `Contract` (`Contract.AddProject()`); `Project` no revalida esta regla, para evitar duplicidad de lógica entre Aggregates.
- El coordinador asignado autoriza préstamos documentales y aprueba procesos internos operativos del proyecto (según `UbiquitousLanguage.md`); esta autoridad se ejercita desde los Aggregates correspondientes (Préstamo, procesos internos), no mediante métodos adicionales en `Project`.
- Un proyecto `Closed` no admite nuevo material documental, pero conserva su historial operativo.

---

# Eventos de dominio

Según `ProjectAggregate.md v2.0` (fuente autoritativa):

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

**Punto abierto:** `02_DomainEvents.md` no incluye actualmente una categoría "Project Events", y `DomainMap.md` solo cataloga `ProjectCreated`. `ProjectAggregate.md v2.0` señala explícitamente que ambos documentos deben actualizarse para reflejar este catálogo completo; dicha actualización aún no se ha realizado.

---

# Métodos públicos

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

# Métodos privados

- `EnsureStatusIs(ProjectStatus expected)` — validación interna reutilizada por los métodos de transición de estado, análoga al patrón de validación por estado ya usado en `Contract`.
- `ContainsActiveWarehouseAssignment(Guid warehouseId)` — encapsula la verificación de si una bodega ya se encuentra asignada (sin liberar) al proyecto, análoga a `Contract.ContainsProject()` y `Customer.ContainsContract()`.

---

# Factory Method

`Create(ProjectCode projectCode, Guid contractId)`

- Crea un nuevo proyecto en estado `Draft`.
- Requiere `projectCode` y `contractId` no vacíos/nulos.
- Registra el evento `ProjectCreated`.

---

# Restricciones

Las mismas utilizadas en `ContractSpecification.md` y `CustomerSpecification.md`:

- Domain puro (sin dependencias de otras capas).
- Debe heredar de `Entity`.
- Debe implementar `IAggregateRoot`.
- Debe utilizar `Result<T>` para el manejo de resultados.
- Debe utilizar `Error` para representar fallos.
- Sin excepciones para el control de reglas de negocio.
- Sin dependencias de EF Core, ASP.NET, Infrastructure ni librerías externas.
- Uso obligatorio del Value Object `ProjectCode` para el código de negocio del proyecto.

---

# Relaciones con otros Aggregates

- **Contract → Project**: relación estructural obligatoria. `Project` almacena `ContractId`. Un contrato puede tener múltiples proyectos (ya modelado desde `Contract` mediante `ContractProjectReference`).
- **Customer → Project**: relación conceptual/transitiva únicamente, resuelta vía `Project.ContractId → Contract.ClientId`. `Project` no almacena `CustomerId`.
- **User (Coordinator) → Project**: `Project` almacena una referencia opcional y mutable (`CoordinatorId`) al `User` actualmente asignado como coordinador.
- **Warehouse → Project**: relación estructural mediante la colección interna `AssignedWarehouses`, que referencia bodegas por `WarehouseId` junto con su vigencia temporal. No se embebe el Aggregate `Warehouse` completo.
- **Documentary Reception → Project** y **Documentary Loan → Project**: relaciones inversas; ambos Aggregates referencian a `Project`, no al contrario.
- **Billing → Project**: relación mencionada únicamente a nivel conceptual.

**Punto abierto:** la naturaleza exacta de la relación entre `Project` y `Billing` / `Billing Period` permanece pendiente, según lo señalado en `ProjectAggregate.md v2.0`.

---

# Dependencias

- `Entity` (infraestructura base del dominio).
- `IAggregateRoot` (infraestructura base del dominio).
- `Result` (manejo de resultados).
- `Error` (representación de fallos).
- `ProjectCode` (Value Object).

**No se utilizan en este Aggregate** (pertenecen a otros Aggregates y no deben duplicarse aquí, según `ProjectAggregate.md v2.0`): `ContractNumber` (Contract), `WarehouseCode` (Warehouse), `Nit` (Customer), `Money` (Contract).

---

# Consideraciones para EF Core

(Aplicables únicamente a la capa de Infrastructure; el Aggregate en sí no debe conocer EF Core.)

- `ProjectCode` deberá mapearse como Owned Type o mediante conversión de propiedad, siguiendo el mismo criterio aplicado a `ContractNumber` en `Contract` y a `Nit` en `Customer`.
- El campo `Status` (enum: Draft/Planned/Active/Suspended/Completed/Closed/Archived) deberá mapearse con conversión explícita a valor almacenado, evitando dependencia del orden del enum.
- `ContractId` deberá mapearse como clave foránea no nula hacia `Contract`.
- `CoordinatorId` deberá mapearse como clave foránea nula hacia `User`.
- La colección `AssignedWarehouses` requiere una tabla de asignación independiente (WarehouseId, AssignedFrom, ReleasedOn) en lugar de una relación muchos-a-muchos simple, dado que cada asignación tiene vigencia temporal propia.
- Los eventos de dominio no deben persistirse como parte del propio Aggregate; deben despacharse tras el `SaveChanges` mediante el mecanismo de outbox u otro definido en Infrastructure.

---

# Consideraciones para Testing

- Probar que `Create()` solo produce un `Project` válido cuando `ProjectCode` y `ContractId` son válidos, y falla (vía `Result.Failure`) en caso contrario.
- Probar cada transición de estado documentada (`Plan()`, `Start()`, `Suspend()`, `Resume()`, `Complete()`, `Close()`, `Archive()`) tanto en su camino exitoso como en transiciones no permitidas desde el estado actual.
- Probar que un proyecto `Closed` no puede recibir nuevo material documental.
- Probar que un proyecto `Archived` no puede modificarse.
- Probar `AssignCoordinator()` reemplazando un coordinador previamente asignado.
- Probar `AssignWarehouse()` rechazando una bodega ya asignada (sin liberar) al mismo proyecto.
- Probar `ReleaseWarehouse()` validando que `ReleasedOn` no sea anterior a `AssignedFrom`.

---

# Consideraciones futuras

- Confirmar si "Documentary Unit" corresponde al conjunto de entidades `Folder`, `Box`, `Document` y `Annex` ya definidas en `DomainMap.md`/`UbiquitousLanguage.md`, o si es un concepto distinto pendiente de definición formal (pendiente señalado en `ProjectAggregate.md v2.0`).
- Precisar la relación estructural (si existe) entre `Project` y `Billing` / `Billing Period`.
- Actualizar `02_DomainEvents.md` y `DomainMap.md` para reflejar el catálogo completo de eventos de `Project` definido en `ProjectAggregate.md v2.0`.
- Confirmar si `StartDate`/`EndDate` del proyecto requieren validaciones cruzadas contra `Contract.StartDate`/`Contract.EndDate` (por ejemplo, que la ejecución del proyecto no exceda la vigencia del contrato); no se documenta actualmente ninguna regla en ese sentido.
- Definir metadatos oficiales AGN para unidades documentales, reglas oficiales de identificación de expedientes documentales, estándar de código de barras y estándar de etiquetado físico (pendientes ya señalados desde la versión 1.0.0 de `ProjectAggregate.md`).

# Warehouse Specification

| Campo | Valor |
|--------|-------|
| Documento | WarehouseSpecification |
| Versión | 2.0.0 |
| Aggregate | Warehouse |
| Bounded Context | Warehouse Management |
| Fuentes | WarehouseAggregate.md v2.0 (autoritativa), ContractAggregate.md, CustomerAggregate.md, ProjectAggregate.md v2.0, ContractSpecification.md, CustomerSpecification.md, ProjectSpecification.md, DomainMap.md, UbiquitousLanguage.md, 01_DomainModel.md, 02_DomainEvents.md |
| Estado | Draft |

> Este documento consolida, sin añadir reglas nuevas, lo ya definido en `WarehouseAggregate.md` versión 2.0. Donde exista diferencia entre documentos, `WarehouseAggregate.md v2.0` se considera la fuente autoritativa. Los puntos que dicho documento deja explícitamente pendientes se señalan aquí como **Punto abierto**.

---

# Propósito

Representar una bodega física administrada por MULTISERTEC, destinada al almacenamiento seguro de documentación, equipos y demás elementos asociados a uno o varios proyectos. La bodega constituye la ubicación física principal donde se conserva la información custodiada.

Una bodega puede organizarse internamente mediante una estructura jerárquica de ubicaciones físicas (pasillo, estantería, nivel, posición), cada una capaz de alojar cajas documentales. Esta estructura interna es responsabilidad exclusiva de la bodega.

---

# Aggregate Root

`Warehouse`

---

# Responsabilidades

- Administrar la capacidad física.
- Organizar la estructura interna mediante ubicaciones físicas (`WarehouseLocation`).
- Registrar movimientos.
- Permitir la ubicación rápida del material.
- Garantizar la trazabilidad física.
- Controlar disponibilidad de espacio.
- Rastrear el estado operativo de la bodega.

El Aggregate coordina la creación y el ciclo de vida de sus ubicaciones internas; las reglas específicas de capacidad, ocupación y movimiento físico de cajas quedan asociadas a la Entity interna `WarehouseLocation`.

---

# Propiedades

Propiedades que pertenecen realmente al Aggregate `Warehouse`:

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único e inmutable de la bodega. |
| WarehouseCode | WarehouseCode | `WarehouseCode` | Sí | Código de negocio legible de la bodega. Único Value Object confirmado para este Aggregate. |
| Status | WarehouseStatus (enum) | N/A (enum de dominio) | Sí | Estado actual de la bodega dentro de su ciclo de vida. |

**Punto abierto:** no se documentan atributos adicionales (nombre descriptivo, dirección física, etc.). Aunque el Value Object `Address` ya existe implementado en el dominio, ningún documento fuente lo asigna explícitamente como atributo de `Warehouse`.

---

# Referencias a otros Aggregates

`Warehouse` **no almacena ninguna referencia a otro Aggregate Root.**

La relación con `Project` es inversa: `Project` mantiene la colección `AssignedWarehouses` (referencias por `WarehouseId` con vigencia temporal). `Warehouse` no mantiene una colección propia de proyectos asignados, para evitar duplicar la misma relación en ambos extremos.

---

# Colecciones

| Nombre | Tipo de elemento | Descripción |
|--------|-------------------|-------------|
| Locations | `WarehouseLocation` (Entity interna, no Aggregate Root) | Ubicaciones físicas que componen la estructura interna de la bodega. Cada `WarehouseLocation` pertenece exclusivamente a una `Warehouse` y no existe fuera de ese contexto. |

**Decisión arquitectónica confirmada:** `WarehouseLocation` no es un Aggregate Root; es una Entity interna de `Warehouse`, gestionada y protegida exclusivamente a través de este Aggregate. `Box` no pertenece directamente a `Warehouse`; referenciará una `WarehouseLocation` por identidad.

**Punto abierto:** los atributos propios de `WarehouseLocation` (código/identificador de ubicación, nivel jerárquico, capacidad, estado de ocupación) no están documentados. No existe actualmente un Value Object para el código de ubicación. No se inventan sus atributos en esta Specification.

---

# Estados

- Available
- Operational
- Maintenance
- Closed
- Archived

---

# Diagrama de transiciones

```
Available ──Activate()──▶ Operational ──StartMaintenance()──▶ Maintenance ──Close()──▶ Closed ──Archive()──▶ Archived
                                            ▲                      │
                                            └──FinishMaintenance()─┘
```

- `Available → Operational` mediante `Activate()`, emite `WarehouseActivated`.
- `Operational → Maintenance` mediante `StartMaintenance()`. No existe un evento de dominio dedicado a esta transición.
- `Maintenance → Operational` mediante `FinishMaintenance()`. Tampoco cuenta con un evento de dominio documentado.
- `Maintenance → Closed` mediante `Close()`, emite `WarehouseClosed`.
- `Closed → Archived` mediante `Archive()`. **Punto abierto:** no existe un evento equivalente a "WarehouseArchived", a diferencia del patrón ya establecido en `Project` (`ProjectArchived`).

Se mantiene la secuencia lineal (`Available → Operational → Maintenance → Closed → Archived`), añadiendo únicamente la transición de retorno `Maintenance → Operational`. No se asume que `Operational` pueda cerrarse directamente sin pasar por `Maintenance`.

---

# Invariantes

Invariantes propias del Aggregate `Warehouse` (estado y ciclo de vida):

- Una bodega solo puede activarse (`Activate()`) desde el estado `Available`.
- Una bodega solo puede iniciar mantenimiento (`StartMaintenance()`) desde el estado `Operational`.
- Una bodega solo puede finalizar mantenimiento (`FinishMaintenance()`) desde el estado `Maintenance`, retornando a `Operational`.
- Una bodega solo puede cerrarse (`Close()`) desde el estado `Maintenance`.
- Una bodega solo puede archivarse (`Archive()`) desde el estado `Closed`.

Invariantes cuya implementación queda diferida a la Entity interna `WarehouseLocation` y a la futura definición de `Box`; se conservan textualmente:

- Una caja solamente podrá ocupar una ubicación física al mismo tiempo.
- Una ubicación física solamente podrá contener una caja cuando exista capacidad disponible.
- Toda caja deberá pertenecer a una ubicación registrada.
- Todo movimiento físico deberá quedar auditado.
- Ninguna caja podrá quedar sin ubicación.

---

# Reglas de negocio

**Punto abierto:** `WarehouseAggregate.md v2.0` no distingue entre invariantes y reglas de negocio como secciones separadas; mantiene una única sección "Invariantes" (reproducida arriba). No se listan reglas de negocio adicionales.

La regla "Todo movimiento físico deberá quedar auditado" es consistente con el "Audit Principle" ya establecido en `02_DomainEvents.md` ("Every domain event must generate an audit entry"); no introduce una obligación nueva.

---

# Eventos de dominio

Según `WarehouseAggregate.md v2.0` (fuente autoritativa):

- `WarehouseCreated` — emitido por `Create()`.
- `WarehouseActivated` — emitido por `Activate()`.
- `WarehouseClosed` — emitido por `Close()`.
- `LocationCreated` — asociado a la gestión de `WarehouseLocation`; emisión diferida.
- `LocationOccupied` — asociado a la gestión de `WarehouseLocation`; emisión diferida.
- `LocationReleased` — asociado a la gestión de `WarehouseLocation`; emisión diferida.
- `BoxMoved` — asociado al movimiento físico de cajas; emisión diferida hasta que `Box` y `WarehouseLocation` estén implementados.
- `CapacityExceeded` — asociado al control de capacidad de `WarehouseLocation`; emisión diferida.

**Punto abierto:**

- No existe evento documentado para `StartMaintenance()` ni `FinishMaintenance()`.
- No existe evento documentado para `Archive()` (a diferencia de `ProjectArchived` en `Project`).
- No se inventan `WarehouseArchived`, `WarehouseMaintenanceStarted` ni `WarehouseMaintenanceFinished`; estas ausencias se mantienen documentadas exactamente como en `WarehouseAggregate.md v2.0`.

---

# Métodos públicos

- `Create(WarehouseCode warehouseCode)`
- `Activate()`
- `StartMaintenance()`
- `FinishMaintenance()`
- `Close()`
- `Archive()`

**Punto abierto:** no se documentan métodos para la gestión de `WarehouseLocation` (creación, modificación o eliminación de ubicaciones internas), dado que los atributos de dicha Entity aún no están definidos.

---

# Métodos privados

- `EnsureStatusIs(WarehouseStatus expected)` — validación interna reutilizada por los métodos de transición de estado, análoga al patrón ya usado en `Contract`, `Customer` y `Project`.

---

# Factory Method

`Create(WarehouseCode warehouseCode)`

- Crea una nueva bodega en estado `Available` (primer estado del Lifecycle).
- Requiere `warehouseCode` no nulo.
- Registra el evento `WarehouseCreated`.

---

# Restricciones

Las mismas utilizadas en `ContractSpecification.md`, `CustomerSpecification.md` y `ProjectSpecification.md`:

- Domain puro (sin dependencias de otras capas).
- Debe heredar de `Entity`.
- Debe implementar `IAggregateRoot`.
- Debe utilizar `Result<T>` para el manejo de resultados.
- Debe utilizar `Error` para representar fallos.
- Sin excepciones para el control de reglas de negocio.
- Sin dependencias de EF Core, ASP.NET, Infrastructure ni librerías externas.
- Uso obligatorio del Value Object `WarehouseCode` para el código de negocio de la bodega.

---

# Relaciones con otros Aggregates

- **Project → Warehouse**: relación estructural inversa. `Project` referencia bodegas por `WarehouseId` mediante su colección `AssignedWarehouses`, con vigencia temporal. `Warehouse` no almacena ninguna referencia a `Project`.
- **Box → WarehouseLocation**: relación conceptual pendiente. `Box` no pertenecerá directamente a `Warehouse`, sino que referenciará una `WarehouseLocation` por identidad. `Box` aún no está definido como Aggregate o Entity en ningún documento fuente.

---

# Dependencias

- `Entity` (infraestructura base del dominio).
- `IAggregateRoot` (infraestructura base del dominio).
- `Result` (manejo de resultados).
- `Error` (representación de fallos).
- `WarehouseCode` (Value Object).

**No se utilizan en este Aggregate** (pertenecen a otros Aggregates y no deben duplicarse aquí): `ContractNumber` (Contract), `ProjectCode` (Project), `BoxCode` / `FolderCode` (pendientes de asociarse a `Box`/`Folder`, aún no definidos), `Nit` (Customer), `Address`, `PhoneNumber`, `Email`, `PersonName`, `Money`.

---

# Consideraciones para EF Core

(Aplicables únicamente a la capa de Infrastructure; el Aggregate en sí no debe conocer EF Core.)

- `WarehouseCode` deberá mapearse como Owned Type o mediante conversión de propiedad, siguiendo el mismo criterio aplicado a `ContractNumber`, `Nit` y `ProjectCode`.
- El campo `Status` (enum: Available/Operational/Maintenance/Closed/Archived) deberá mapearse con conversión explícita a valor almacenado, evitando dependencia del orden del enum.
- `WarehouseLocation`, al ser una Entity interna, deberá mapearse como colección propiedad de `Warehouse` (owned collection) una vez definidos sus atributos; no debe exponerse como Aggregate independiente en el modelo de persistencia.
- Los eventos de dominio no deben persistirse como parte del propio Aggregate; deben despacharse tras el `SaveChanges` mediante el mecanismo de outbox u otro definido en Infrastructure.

---

# Consideraciones para Testing

- Probar que `Create()` solo produce un `Warehouse` válido cuando `WarehouseCode` es válido, y falla (vía `Result.Failure`) en caso contrario.
- Probar cada transición de estado documentada (`Activate()`, `StartMaintenance()`, `FinishMaintenance()`, `Close()`, `Archive()`) tanto en su camino exitoso como en transiciones no permitidas desde el estado actual.
- Probar explícitamente que `Close()` solo es válido desde `Maintenance`, no desde `Operational`.

**Punto abierto:** al no estar definidos los atributos ni métodos de `WarehouseLocation`, no es posible especificar casos de prueba para capacidad, ocupación o movimiento físico de cajas.

---

# Consideraciones futuras

- Definir estructura física estándar utilizada por MULTISERTEC.
- Definir capacidad máxima por estantería.
- Definir reglas de numeración de ubicaciones.
- Definir los atributos propios de `WarehouseLocation` y, si corresponde, el Value Object necesario para su código.
- Definir los métodos del Aggregate `Warehouse` para crear, modificar y liberar instancias internas de `WarehouseLocation`.
- Definir formalmente `Box` y su relación por identidad con `WarehouseLocation`.
- Definir el evento de dominio (si corresponde) para `StartMaintenance()` y `FinishMaintenance()`.
- Definir el evento de dominio (si corresponde) para `Archive()`.
- Confirmar si `Warehouse` requiere un atributo de nombre descriptivo o dirección física (`Address`).

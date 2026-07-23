# Warehouse Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Warehouse Aggregate |
| Versión | 2.0.0 |
| Estado | Draft |
| Contexto | Warehouse Management |
| Aggregate Root | Warehouse |

---

# Propósito

El Aggregate `Warehouse` representa una bodega física administrada por MULTISERTEC, destinada al almacenamiento seguro de documentación, equipos y demás elementos asociados a uno o varios proyectos.

La bodega constituye la ubicación física principal donde se conserva la información custodiada.

## Justificación de negocio

Una bodega puede organizarse internamente mediante una estructura jerárquica de ubicaciones físicas (por ejemplo: pasillo, estantería, nivel, posición), cada una capaz de alojar cajas documentales. Esta estructura interna es responsabilidad exclusiva de la bodega: ninguna ubicación física tiene sentido de negocio fuera del contexto de la bodega a la que pertenece, por lo que se modela como parte del mismo Aggregate y no como un Aggregate independiente.

---

# Responsabilidades

- Administrar la capacidad física.
- Organizar la estructura interna mediante ubicaciones físicas (`WarehouseLocation`).
- Registrar movimientos.
- Permitir la ubicación rápida del material.
- Garantizar la trazabilidad física.
- Controlar disponibilidad de espacio.
- Rastrear el estado operativo de la bodega.

El Aggregate coordina la creación y el ciclo de vida de sus ubicaciones internas; las reglas específicas de capacidad, ocupación y movimiento físico de cajas quedan asociadas a la Entity interna `WarehouseLocation` (ver "Pendientes").

---

# Business Identity

Toda bodega posee un identificador único (`Id`), que nunca cambia durante su ciclo de vida.

`WarehouseCode` (Value Object ya implementado en el dominio) se adopta como el código de negocio legible de la bodega, siguiendo el mismo patrón ya utilizado por `Contract` con `ContractNumber` y por `Project` con `ProjectCode`.

---

# Atributos del Aggregate

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único e inmutable de la bodega. |
| WarehouseCode | WarehouseCode | `WarehouseCode` | Sí | Código de negocio legible de la bodega. |
| Status | WarehouseStatus (enum) | N/A (enum de dominio) | Sí | Estado actual de la bodega dentro de su ciclo de vida. |

**Punto abierto:** no se documentan atributos adicionales (nombre descriptivo, dirección física, etc.) para `Warehouse`. Aunque el Value Object `Address` ya existe implementado en el dominio, ningún documento fuente lo asigna explícitamente como atributo de `Warehouse`; no se agrega para evitar inventar atributos.

---

# Referencias a otros Aggregates

`Warehouse` **no almacena ninguna referencia a otro Aggregate Root.**

La relación con `Project` es inversa: `Project` mantiene la colección `AssignedWarehouses` (referencias por `WarehouseId` con vigencia temporal), según `ProjectAggregate.md v2.0`. `Warehouse` no necesita, y por tanto no debe, mantener una colección propia de proyectos asignados, para evitar duplicar la misma relación en ambos extremos.

---

# Colecciones internas

| Nombre | Tipo de elemento | Descripción |
|--------|-------------------|-------------|
| Locations | `WarehouseLocation` (Entity interna, no Aggregate Root) | Ubicaciones físicas que componen la estructura interna de la bodega. Cada `WarehouseLocation` pertenece exclusivamente a una `Warehouse` y no existe fuera de ese contexto. |

**Decisión arquitectónica (definitiva a partir de esta versión):** `WarehouseLocation` **no es un Aggregate Root.** Es una Entity interna de `Warehouse`, gestionada y protegida exclusivamente a través del Aggregate `Warehouse`. `Box` no pertenecerá directamente a `Warehouse`; en su lugar, referenciará una `WarehouseLocation` por identidad (relación pendiente de definir formalmente, ver "Pendientes").

**Punto abierto:** los atributos propios de `WarehouseLocation` (código/identificador de ubicación, nivel jerárquico, capacidad, estado de ocupación) no están documentados con suficiente detalle para definirse en esta versión. No existe actualmente un Value Object para el código de ubicación dentro del catálogo de Value Objects ya implementados (`WarehouseCode`, `BoxCode`, `FolderCode`, `ProjectCode`, `ContractNumber`, `Nit`, `Address`, `PhoneNumber`, `Email`, `Money`, `PersonName`). Definir su estructura queda como responsabilidad futura, sin inventarla en este documento.

---

# Lifecycle

- Available
- Operational
- Maintenance
- Closed
- Archived

---

# Diagrama de estados

```
Available ──Activate()──▶ Operational ──StartMaintenance()──▶ Maintenance ──Close()──▶ Closed ──Archive()──▶ Archived
                                            ▲                      │
                                            └──FinishMaintenance()─┘
```

- `Available → Operational` mediante `Activate()`, emite `WarehouseActivated`.
- `Operational → Maintenance` mediante `StartMaintenance()`. No existe un evento de dominio dedicado a esta transición en el catálogo original (`02_DomainEvents.md`/`WarehouseAggregate.md v1.0`); se documenta como transición sin evento propio, sin inventar uno nuevo.
- `Maintenance → Operational` mediante `FinishMaintenance()`. Al igual que la transición anterior, no cuenta con un evento de dominio documentado.
- `Maintenance → Closed` mediante `Close()`, emite `WarehouseClosed`.
- `Closed → Archived` mediante `Archive()`. **Punto abierto:** no existe un evento equivalente a "WarehouseArchived" en el catálogo original de eventos, a diferencia del patrón ya establecido en `Project` (`ProjectArchived`). Se documenta la inconsistencia sin inventar el evento.

Se mantiene la secuencia lineal original (`Available → Operational → Maintenance → Closed → Archived`) documentada en la versión 1.0.0, añadiendo únicamente la transición de retorno `Maintenance → Operational`, explícitamente solicitada. No se asume que `Operational` pueda cerrarse directamente sin pasar por `Maintenance`, por no estar justificado en la documentación fuente.

---

# Invariantes

Invariantes propias del Aggregate `Warehouse` (estado y ciclo de vida):

- Una bodega solo puede activarse (`Activate()`) desde el estado `Available`.
- Una bodega solo puede iniciar mantenimiento (`StartMaintenance()`) desde el estado `Operational`.
- Una bodega solo puede finalizar mantenimiento (`FinishMaintenance()`) desde el estado `Maintenance`, retornando a `Operational`.
- Una bodega solo puede cerrarse (`Close()`) desde el estado `Maintenance`.
- Una bodega solo puede archivarse (`Archive()`) desde el estado `Closed`.

Invariantes documentadas en la versión 1.0.0, cuya implementación queda diferida a la Entity interna `WarehouseLocation` y a la futura definición de `Box` (ver "Pendientes"); se conservan textualmente sin eliminarlas:

- Una caja solamente podrá ocupar una ubicación física al mismo tiempo.
- Una ubicación física solamente podrá contener una caja cuando exista capacidad disponible.
- Toda caja deberá pertenecer a una ubicación registrada.
- Todo movimiento físico deberá quedar auditado.
- Ninguna caja podrá quedar sin ubicación.

---

# Reglas de negocio

**Punto abierto:** `WarehouseAggregate.md` (en su versión 1.0.0 y en esta versión 2.0) no distingue entre invariantes y reglas de negocio como secciones separadas. Se mantiene una única sección "Invariantes" (reproducida arriba). No se listan reglas de negocio adicionales para evitar reinterpretar el contenido ya cubierto.

La regla "Todo movimiento físico deberá quedar auditado" es consistente con el "Audit Principle" ya establecido en `02_DomainEvents.md` ("Every domain event must generate an audit entry"); no introduce una obligación nueva, sino que reafirma un principio ya vigente en el dominio.

---

# Métodos públicos

- `Create(WarehouseCode warehouseCode)`
- `Activate()`
- `StartMaintenance()`
- `FinishMaintenance()`
- `Close()`
- `Archive()`

**Punto abierto:** no se agregan métodos para la gestión de `WarehouseLocation` (creación, modificación o eliminación de ubicaciones internas) en esta versión, dado que los atributos de dicha Entity aún no están definidos (ver "Colecciones internas"). Estos métodos deberán incorporarse en una futura revisión de este documento, una vez definida la estructura de `WarehouseLocation`.

---

# Métodos privados

- `EnsureStatusIs(WarehouseStatus expected)` — validación interna reutilizada por los métodos de transición de estado, análoga al patrón de validación por estado ya usado en `Contract`, `Customer` y `Project`.

---

# Factory Method

`Create(WarehouseCode warehouseCode)`

- Crea una nueva bodega en estado `Available` (primer estado del Lifecycle).
- Requiere `warehouseCode` no nulo.
- Registra el evento `WarehouseCreated`.

---

# Domain Events

Según `WarehouseAggregate.md` (catálogo original, conservado sin modificaciones):

- `WarehouseCreated` — emitido por `Create()`.
- `WarehouseActivated` — emitido por `Activate()`.
- `WarehouseClosed` — emitido por `Close()`.
- `LocationCreated` — asociado a la gestión de `WarehouseLocation`; emisión diferida hasta que dicha Entity se implemente formalmente.
- `LocationOccupied` — asociado a la gestión de `WarehouseLocation`; emisión diferida.
- `LocationReleased` — asociado a la gestión de `WarehouseLocation`; emisión diferida.
- `BoxMoved` — asociado al movimiento físico de cajas entre ubicaciones; emisión diferida hasta que `Box` y `WarehouseLocation` estén implementados.
- `CapacityExceeded` — asociado al control de capacidad de `WarehouseLocation`; emisión diferida.

**Punto abierto — inconsistencias documentadas (sin resolver, siguiendo el mismo criterio aplicado en `Project`):**

- No existe un evento documentado para las transiciones `StartMaintenance()` (`Operational → Maintenance`) ni `FinishMaintenance()` (`Maintenance → Operational`).
- No existe un evento documentado para `Archive()` (`Closed → Archived`), a diferencia del patrón ya establecido en `Project` con `ProjectArchived`.
- No se inventan eventos nuevos para cubrir estos vacíos; quedan señalados como pendientes de definición formal por parte del negocio.

---

# Relaciones con otros Aggregates

- **Project → Warehouse**: relación estructural inversa. `Project` referencia bodegas por `WarehouseId` mediante su colección `AssignedWarehouses`, con vigencia temporal (`ProjectAggregate.md v2.0`). `Warehouse` no almacena ninguna referencia a `Project`.
- **Box → WarehouseLocation**: relación conceptual pendiente. Según la decisión arquitectónica de esta versión, `Box` no pertenecerá directamente a `Warehouse`, sino que referenciará una `WarehouseLocation` por identidad. `Box` aún no está definido como Aggregate o Entity en ningún documento fuente; esta relación queda pendiente hasta que se formalice `Box`.

---

# Value Objects utilizados

- `WarehouseCode` — código de negocio de la bodega.

**No se utilizan en este Aggregate** (pertenecen a otros Aggregates y no deben duplicarse aquí): `ContractNumber` (Contract), `ProjectCode` (Project), `BoxCode` / `FolderCode` (pendientes de asociarse a `Box`/`Folder`, aún no definidos como Aggregates o Entities), `Nit` (Customer), `Address`, `PhoneNumber`, `Email`, `PersonName`, `Money`.

---

# Pendientes

- Definir estructura física estándar utilizada por MULTISERTEC.
- Definir capacidad máxima por estantería.
- Definir reglas de numeración de ubicaciones.
- Definir los atributos propios de la Entity interna `WarehouseLocation` (identificador/código de ubicación, nivel jerárquico, capacidad, estado de ocupación) y, si corresponde, el Value Object necesario para su código.
- Definir los métodos del Aggregate `Warehouse` para crear, modificar y liberar instancias internas de `WarehouseLocation`, una vez definidos sus atributos.
- Definir formalmente `Box` (actualmente solo listado como Entity en `DomainMap.md`) y su relación por identidad con `WarehouseLocation`.
- Definir el evento de dominio (si corresponde) para las transiciones `StartMaintenance()` y `FinishMaintenance()`.
- Definir el evento de dominio (si corresponde) para la transición `Archive()`, alineando el comportamiento con el ya establecido en `Project` (`ProjectArchived`).
- Confirmar si `Warehouse` requiere un atributo de nombre descriptivo o dirección física (`Address`), actualmente no documentado.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Versión inicial del Aggregate Warehouse. |
| 2.0.0 | 2026-07-21 | Se define `WarehouseStatus` con su ciclo de vida y diagrama de estados (incluyendo el retorno `Maintenance → Operational`); se establece formalmente que `WarehouseLocation` es una Entity interna del Aggregate `Warehouse` y no un Aggregate Root independiente; se incorporan Atributos del Aggregate, Referencias a otros Aggregates, Colecciones internas, Métodos públicos y privados, Factory Method, y se documentan las inconsistencias de eventos no resueltas (`StartMaintenance`/`FinishMaintenance`/`Archive` sin evento), siguiendo la misma metodología aplicada en `ProjectAggregate.md v2.0`. |

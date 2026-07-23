# Box Specification

| Campo | Valor |
|--------|-------|
| Documento | BoxSpecification |
| Versión | 2.0.0 |
| Aggregate | Box |
| Bounded Context | Documentary Storage |
| Fuentes | BoxAggregate.md v2.0 (autoritativa), ContractAggregate.md, CustomerAggregate.md, ProjectAggregate.md v2.0, WarehouseAggregate.md v2.0, ContractSpecification.md, CustomerSpecification.md, ProjectSpecification.md, WarehouseSpecification.md, DomainMap.md, UbiquitousLanguage.md, 01_DomainModel.md, 02_DomainEvents.md |
| Estado | Draft |

> Este documento consolida, sin añadir reglas nuevas, lo ya definido en `BoxAggregate.md` versión 2.0. Donde exista diferencia entre documentos, `BoxAggregate.md v2.0` se considera la fuente autoritativa. Los puntos que dicho documento deja explícitamente pendientes se señalan aquí como **Punto abierto**.

> **Punto abierto — inconsistencia de clasificación:** `DomainMap.md` clasifica a `Box` bajo "Entities" (no bajo "Aggregate Roots"), y `01_DomainModel.md` no lista a `Box` entre sus Aggregate Roots candidatos (solo menciona "Documentary Unit" como candidato). `BoxAggregate.md` lo define como Aggregate Root desde su versión 1.0.0. Esta Specification continúa tratando a `Box` como Aggregate Root, siguiendo la fuente autoritativa, sin resolver la discrepancia.

---

# Propósito

Representar la unidad física de almacenamiento utilizada por MULTISERTEC para custodiar carpetas documentales. Cada caja posee una identidad única, un código de barras, y un historial de movimientos. La caja constituye la unidad física mínima de almacenamiento administrada por el sistema.

Las cajas son el mecanismo físico mediante el cual las carpetas documentales quedan efectivamente almacenadas y localizables dentro de una bodega. El control de identidad (código de negocio y código de barras), estado de conservación y movimientos de cada caja es indispensable para garantizar la trazabilidad física exigida por la gestión documental bajo las normas del AGN.

---

# Aggregate Root

`Box`

---

# Responsabilidades

- Identificar la caja.
- Registrar código de barras.
- Asociar carpetas.
- Registrar movimientos.
- Mantener el estado de conservación.
- Registrar auditoría.

**Punto abierto:** la responsabilidad "Controlar la ubicación física", presente en la versión 1.0.0, no puede mantenerse tal cual, dado que `WarehouseAggregate.md v2.0` estableció que la ubicación física (`WarehouseLocation`) es una Entity interna de `Warehouse`, y que `Box` referenciará una `WarehouseLocation` por identidad una vez esta se formalice. Hasta entonces, `Box` no puede implementar control alguno sobre la ubicación física.

---

# Propiedades

Propiedades que pertenecen realmente al Aggregate `Box`:

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único e inmutable de la caja. |
| BoxCode | BoxCode | `BoxCode` | Sí | Código de negocio legible de la caja. |
| Barcode | Barcode? | `Barcode` | No — se asigna mediante el evento `BarcodeAssigned`, no necesariamente en la creación | Código de barras físico de la caja. |
| Status | BoxStatus (enum) | N/A (enum de dominio) | Sí | Estado actual de la caja dentro de su ciclo de vida. |

**Punto abierto:** no se documentan atributos adicionales (fecha de creación operativa, condición de conservación, etc.) más allá de los ya confirmados.

---

# Referencias a otros Aggregates

`Box` no almacena actualmente ninguna referencia confirmada a otro Aggregate Root.

**Punto abierto — referencia pendiente a `WarehouseLocation`:** según la decisión arquitectónica ya establecida en `WarehouseAggregate.md v2.0`, `Box` referenciará una `WarehouseLocation` por identidad (y no directamente a `Warehouse`). Dado que `WarehouseLocation` aún no cuenta con atributos formalizados, no se define el nombre ni el tipo exacto del campo de referencia correspondiente.

**Punto abierto — relación con `Project`:** `BoxAggregate.md v2.0` documenta, por herencia de la versión 1.0.0, una cadena conceptual `Box → Warehouse → Folders → Project`. Ningún invariante documentado exige que `Box` mantenga una referencia directa a `Project`; se trata de una relación puramente conceptual.

---

# Colecciones

| Nombre | Tipo de elemento | Descripción |
|--------|-------------------|-------------|
| Folders | `Folder` (Entity interna, no Aggregate Root) | Carpetas documentales contenidas físicamente en la caja. |

**Decisión arquitectónica (por analogía con `WarehouseLocation` en `Warehouse`):** `Folder` se modela como Entity interna de `Box`, no como Aggregate Root, siguiendo el mismo criterio ya aplicado a `WarehouseLocation`.

**Punto abierto:** los atributos propios de `Folder` no están documentados con suficiente detalle. `FolderCode` es candidato razonable para su identificación, por analogía con `BoxCode`, `ProjectCode` y `WarehouseCode`, pero no se confirma como definitivo ni se diseñan otros atributos de `Folder`.

---

# Estados

- Created
- Available
- In Storage
- In Transit
- Loan Process
- Returned
- Archived

---

# Diagrama de transiciones

```
Created ──?──▶ Available ──?──▶ In Storage ──Move()──▶ In Transit ──?──▶ Loan Process ──Return()──▶ Returned ──Archive()──▶ Archived
```

- `Created` es el estado inicial, alcanzado mediante `Create()`, que emite `BoxCreated`.
- `In Storage → In Transit` mediante `Move()` (nombre tentativo, por convención de nomenclatura con el evento `BoxMoved`), que emite `BoxMoved`.
- `Loan Process → Returned` mediante `Return()` (nombre tentativo, por convención con el evento `BoxReturned`), que emite `BoxReturned`.
- `Returned → Archived` mediante `Archive()` (nombre tentativo, por convención con el evento `BoxArchived`), que emite `BoxArchived`.

**Punto abierto:** las transiciones `Created → Available`, `Available → In Storage` y `In Transit → Loan Process` no cuentan con ningún evento de dominio ni método documentado.

---

# Invariantes

- Cada caja tiene un único código de barras.
- Una caja ocupa una única ubicación física a la vez.
- Una carpeta solo puede pertenecer a una caja a la vez.
- Toda reubicación genera un movimiento.
- Una caja archivada no puede modificarse.

**Punto abierto:** la invariante "Una caja ocupa una única ubicación física a la vez" no puede implementarse plenamente hasta que `WarehouseLocation` esté formalizada. La invariante "Una carpeta solo puede pertenecer a una caja a la vez" solo puede garantizarse parcialmente desde `Box`; su cumplimiento pleno entre distintas cajas requeriría validación desde la propia Entity `Folder` o mediante un servicio de dominio, no definido en la documentación fuente.

---

# Reglas de negocio

**Punto abierto:** `BoxAggregate.md v2.0` no distingue entre invariantes y reglas de negocio como secciones separadas; mantiene una única sección "Invariantes" (reproducida arriba). No se listan reglas de negocio adicionales.

La regla "Toda reubicación genera un movimiento" es consistente con el "Audit Principle" ya establecido en `02_DomainEvents.md` ("Every domain event must generate an audit entry").

---

# Eventos de dominio

Según `BoxAggregate.md v2.0` (catálogo original, conservado sin modificaciones):

- `BoxCreated` — emitido por `Create()`.
- `BarcodeAssigned` — emitido por `AssignBarcode()`.
- `FolderAdded` — emisión diferida hasta que `Folder` se implemente formalmente.
- `FolderRemoved` — emisión diferida hasta que `Folder` se implemente formalmente.
- `BoxMoved` — emitido por `Move()`.
- `BoxArchived` — emitido por `Archive()`.
- `BoxReturned` — emitido por `Return()`.

**Punto abierto:** no existe evento documentado para las transiciones `Created → Available`, `Available → In Storage` ni `In Transit → Loan Process`. No se inventan eventos nuevos para cubrir estos vacíos.

---

# Métodos públicos

- `Create(BoxCode boxCode)`
- `AssignBarcode(Barcode barcode)`
- `Move(...)` — nombre tentativo por convención con `BoxMoved`; parámetros pendientes de definición, dado que dependen de la referencia a `WarehouseLocation`, aún no formalizada.
- `Return()` — nombre tentativo por convención con `BoxReturned`.
- `Archive()` — nombre tentativo por convención con `BoxArchived`.

**Punto abierto:** `AddFolder()` y `RemoveFolder()` (correspondientes a los eventos `FolderAdded` y `FolderRemoved`) no se definen formalmente, dado que sus firmas dependen de los atributos de `Folder`, que aún no están definidos.

---

# Métodos privados

- `EnsureStatusIs(BoxStatus expected)` — validación interna reutilizada por los métodos de transición de estado, análoga al patrón ya usado en `Contract`, `Customer`, `Project` y `Warehouse`.

---

# Factory Method

`Create(BoxCode boxCode)`

- Crea una nueva caja en estado `Created` (primer estado del Lifecycle).
- Requiere `boxCode` no nulo.
- Registra el evento `BoxCreated`.

---

# Restricciones

Las mismas utilizadas en `ContractSpecification.md`, `CustomerSpecification.md`, `ProjectSpecification.md` y `WarehouseSpecification.md`:

- Domain puro (sin dependencias de otras capas).
- Debe heredar de `Entity`.
- Debe implementar `IAggregateRoot`.
- Debe utilizar `Result<T>` para el manejo de resultados.
- Debe utilizar `Error` para representar fallos.
- Sin excepciones para el control de reglas de negocio.
- Sin dependencias de EF Core, ASP.NET, Infrastructure ni librerías externas.
- Uso obligatorio de los Value Objects `BoxCode` y `Barcode`.

---

# Relaciones con otros Aggregates

- **Warehouse → Box**: relación pendiente. `Box` referenciará una `WarehouseLocation` (Entity interna de `Warehouse`) por identidad; no se referencia a `Warehouse` directamente. La estructura exacta de esta referencia queda pendiente hasta que `WarehouseLocation` se formalice.
- **Project → Box**: relación puramente conceptual, sin invariante que la exija ni Aggregate que la modele estructuralmente.
- **Box → Folder**: relación de composición interna. `Folder` se modela como Entity interna de `Box`, por analogía con `WarehouseLocation` en `Warehouse`.

---

# Dependencias

- `Entity` (infraestructura base del dominio).
- `IAggregateRoot` (infraestructura base del dominio).
- `Result` (manejo de resultados).
- `Error` (representación de fallos).
- `BoxCode` (Value Object).
- `Barcode` (Value Object).

**No se utilizan en este Aggregate** (pertenecen a otros Aggregates y no deben duplicarse aquí): `ContractNumber` (Contract), `ProjectCode` (Project), `WarehouseCode` (Warehouse), `Nit` (Customer), `Address`, `PhoneNumber`, `Email`, `PersonName`, `Money`.

**Punto abierto:** `FolderCode` es candidato para identificar la Entity interna `Folder`, pero no se confirma como dependencia definitiva hasta formalizar dicha Entity.

---

# Consideraciones para EF Core

(Aplicables únicamente a la capa de Infrastructure; el Aggregate en sí no debe conocer EF Core.)

- `BoxCode` y `Barcode` deberán mapearse como Owned Types o mediante conversión de propiedad, siguiendo el mismo criterio aplicado a `ContractNumber`, `Nit`, `ProjectCode` y `WarehouseCode`.
- El campo `Status` (enum: Created/Available/In Storage/In Transit/Loan Process/Returned/Archived) deberá mapearse con conversión explícita a valor almacenado, evitando dependencia del orden del enum.
- `Folder`, al ser una Entity interna, deberá mapearse como colección propiedad de `Box` (owned collection) una vez definidos sus atributos.

**Punto abierto:** no es posible definir el mapeo de la futura referencia a `WarehouseLocation` hasta que dicha Entity se formalice.

---

# Consideraciones para Testing

- Probar que `Create()` solo produce un `Box` válido cuando `BoxCode` es válido, y falla (vía `Result.Failure`) en caso contrario.
- Probar `AssignBarcode()` asignando correctamente el `Barcode`.
- Probar las transiciones documentadas (`Move()`, `Return()`, `Archive()`) tanto en su camino exitoso como en transiciones no permitidas desde el estado actual.
- Probar que una caja en estado `Archived` no puede modificarse.

**Punto abierto:** al no estar definidos los atributos de `Folder` ni los métodos `AddFolder()`/`RemoveFolder()`, no es posible especificar casos de prueba para la asociación de carpetas. Tampoco es posible definir casos de prueba para las transiciones `Created → Available`, `Available → In Storage` ni `In Transit → Loan Process`, al no existir método ni evento documentado.

---

# Consideraciones futuras

- Definir estándar de numeración de cajas.
- Definir capacidad máxima recomendada.
- Validar reglas físicas de conservación.
- Resolver la inconsistencia de clasificación: `DomainMap.md` ubica a `Box` como Entity (no Aggregate Root) y `01_DomainModel.md` no lo lista en absoluto.
- Definir los atributos completos de la Entity interna `Folder` y confirmar formalmente si `FolderCode` es su identificador definitivo.
- Definir los métodos `AddFolder()` y `RemoveFolder()` una vez `Folder` esté formalmente definida.
- Definir la referencia de `Box` a `WarehouseLocation` (nombre y tipo del campo), una vez `WarehouseLocation` cuente con atributos propios.
- Definir el evento o método (si corresponde) para las transiciones `Created → Available`, `Available → In Storage` e `In Transit → Loan Process`.
- Definir los parámetros exactos de `Move()`, actualmente dependientes de la referencia pendiente a `WarehouseLocation`.
- Confirmar si `Box` requiere alguna relación estructural con `Project`, actualmente solo sugerida conceptualmente.
- Precisar si el término "Documentary Unit" corresponde a `Box`, a `Folder`, o a ambos en conjunto.
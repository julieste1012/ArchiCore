# Box Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Box Aggregate |
| Versión | 2.0.0 |
| Estado | Draft |
| Contexto | Documentary Storage |
| Aggregate Root | Box |

> **Punto abierto — inconsistencia de clasificación:** `DomainMap.md` clasifica a `Box` bajo la sección "Entities" (no bajo "Aggregate Roots"), y `01_DomainModel.md` no lista a `Box` en absoluto entre sus Aggregate Roots candidatos (solo menciona "Documentary Unit" como candidato). Sin embargo, `BoxAggregate.md` lo define como Aggregate Root desde su versión 1.0.0. Esta versión 2.0 continúa tratando a `Box` como Aggregate Root, siguiendo la definición ya establecida en el propio documento fuente, pero deja constancia explícita de la discrepancia con `DomainMap.md` y `01_DomainModel.md` sin resolverla.

---

# Propósito

El Aggregate `Box` representa la unidad física de almacenamiento utilizada por MULTISERTEC para custodiar carpetas documentales.

Cada caja posee una identidad única, un código de barras, y un historial de movimientos. La caja constituye la unidad física mínima de almacenamiento administrada por el sistema.

## Justificación de negocio

Las cajas son el mecanismo físico mediante el cual las carpetas documentales quedan efectivamente almacenadas y localizables dentro de una bodega. El control de identidad (código de negocio y código de barras), estado de conservación y movimientos de cada caja es indispensable para garantizar la trazabilidad física exigida por la gestión documental bajo las normas del AGN.

---

# Responsabilidades

- Identificar la caja.
- Registrar código de barras.
- Asociar carpetas.
- Registrar movimientos.
- Mantener el estado de conservación.
- Registrar auditoría.

**Punto abierto:** la responsabilidad "Controlar la ubicación física", presente en la versión 1.0.0, no puede mantenerse tal cual, dado que `WarehouseAggregate.md v2.0` estableció que la ubicación física (`WarehouseLocation`) es una Entity interna de `Warehouse`, y que `Box` referenciará una `WarehouseLocation` por identidad una vez esta se formalice. Hasta entonces, `Box` no puede implementar control alguno sobre la ubicación física; esta responsabilidad queda diferida (ver "Referencias a otros Aggregates" y "Pendientes").

---

# Business Identity

Toda caja posee un identificador único (`Id`), que nunca cambia durante su ciclo de vida.

`BoxCode` (Value Object ya implementado en el dominio) se adopta como el código de negocio legible de la caja, siguiendo el mismo patrón ya utilizado por `Contract` (`ContractNumber`), `Project` (`ProjectCode`) y `Warehouse` (`WarehouseCode`).

`Barcode` (Value Object ya implementado en el dominio) se mantiene como atributo independiente de identificación física, consistente con la invariante "Cada caja tiene un único código de barras" y con `UbiquitousLanguage.md`, que indica que el código de barras "se utiliza principalmente para cajas".

---

# Aggregate Root

`Box`

---

# Atributos del Aggregate

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único e inmutable de la caja. |
| BoxCode | BoxCode | `BoxCode` | Sí | Código de negocio legible de la caja. |
| Barcode | Barcode? | `Barcode` | No — se asigna mediante el evento `BarcodeAssigned`, no necesariamente en la creación | Código de barras físico de la caja. |
| Status | BoxStatus (enum) | N/A (enum de dominio) | Sí | Estado actual de la caja dentro de su ciclo de vida. |

**Punto abierto:** no se documentan atributos adicionales (fecha de creación operativa, condición de conservación, etc.) más allá de los ya confirmados.

---

# Referencias a otros Aggregates

`Box` **no almacena actualmente ninguna referencia confirmada a otro Aggregate Root.**

**Punto abierto — referencia pendiente a `WarehouseLocation`:** según la decisión arquitectónica ya establecida en `WarehouseAggregate.md v2.0`, `Box` referenciará una `WarehouseLocation` por identidad (y no directamente a `Warehouse`). Dado que `WarehouseLocation` aún no cuenta con atributos formalizados, este documento no define el nombre ni el tipo exacto del campo de referencia correspondiente, para no inventar una estructura que depende de una Entity todavía no definida.

**Punto abierto — relación con `Project`:** la versión 1.0.0 de este documento sugiere, en su diagrama "Relaciones", una cadena conceptual `Box → Warehouse → Folders → Project`. Ningún invariante documentado exige que `Box` mantenga una referencia directa a `Project`; se documenta como relación puramente conceptual, sin modelarse como atributo.

---

# Colecciones internas

| Nombre | Tipo de elemento | Descripción |
|--------|-------------------|-------------|
| Folders | `Folder` (Entity interna, no Aggregate Root) | Carpetas documentales contenidas físicamente en la caja. |

**Decisión arquitectónica (por analogía con `WarehouseLocation` en `Warehouse`):** `DomainMap.md` clasifica a `Folder` bajo "Entities" (no bajo "Aggregate Roots"), igual que clasifica a `Box`. Siguiendo el mismo criterio ya aplicado a `WarehouseLocation` (Entity interna de `Warehouse`, no Aggregate Root independiente), `Folder` se modela como Entity interna de `Box`.

**Punto abierto:** los atributos propios de `Folder` no están documentados con suficiente detalle en esta versión, más allá de que `FolderCode` (Value Object ya implementado en el dominio) es candidato razonable para su identificación, por analogía directa con `BoxCode`, `ProjectCode` y `WarehouseCode`. No se confirma como definitivo ni se diseñan otros atributos de `Folder` (por ejemplo, estado de ocupación o metadatos AGN), para no inventar una estructura no respaldada por la documentación.

---

# Lifecycle

- Created
- Available
- In Storage
- In Transit
- Loan Process
- Returned
- Archived

---

# Diagrama de estados

```
Created ──?──▶ Available ──?──▶ In Storage ──Move()──▶ In Transit ──?──▶ Loan Process ──Return()──▶ Returned ──Archive()──▶ Archived
```

- `Created` es el estado inicial, alcanzado mediante `Create()`, que emite `BoxCreated`.
- `In Storage → In Transit` mediante `Move()` (nombre tentativo, por convención de nomenclatura con el evento `BoxMoved`), que emite `BoxMoved`.
- `Loan Process → Returned` mediante `Return()` (nombre tentativo, por convención con el evento `BoxReturned`), que emite `BoxReturned`.
- `Returned → Archived` mediante `Archive()` (nombre tentativo, por convención con el evento `BoxArchived`), que emite `BoxArchived`.

**Punto abierto:** las transiciones `Created → Available`, `Available → In Storage` y `In Transit → Loan Process` no cuentan con ningún evento de dominio ni método documentado en la fuente. No se inventan nombres de método ni eventos para completarlas; quedan señaladas como inconsistencias pendientes entre el "Ciclo de Vida" y los "Eventos de Dominio" ya documentados desde la versión 1.0.0.

---

# Invariantes

Copiadas textualmente de la versión 1.0.0:

- Cada caja tiene un único código de barras.
- Una caja ocupa una única ubicación física a la vez.
- Una carpeta solo puede pertenecer a una caja a la vez.
- Toda reubicación genera un movimiento.
- Una caja archivada no puede modificarse.

**Punto abierto:** la invariante "Una caja ocupa una única ubicación física a la vez" no puede implementarse plenamente hasta que `WarehouseLocation` esté formalizada (ver "Referencias a otros Aggregates"). La invariante "Una carpeta solo puede pertenecer a una caja a la vez" solo puede garantizarse parcialmente desde `Box` (evitando agregar la misma carpeta dos veces dentro de una misma caja); su cumplimiento pleno entre distintas cajas requeriría validación desde la propia Entity `Folder` o mediante un servicio de dominio, lo cual no está definido en la documentación fuente.

---

# Reglas de negocio

**Punto abierto:** `BoxAggregate.md` no distingue entre invariantes y reglas de negocio como secciones separadas; mantiene una única sección "Invariantes" (reproducida arriba). No se listan reglas de negocio adicionales para evitar reinterpretar el contenido ya cubierto.

La regla "Toda reubicación genera un movimiento" es consistente con el "Audit Principle" ya establecido en `02_DomainEvents.md` ("Every domain event must generate an audit entry"); no introduce una obligación nueva.

---

# Métodos públicos

- `Create(BoxCode boxCode)`
- `AssignBarcode(Barcode barcode)`
- `Move(...)` — nombre tentativo por convención con `BoxMoved`; parámetros pendientes de definición, dado que dependen de la referencia a `WarehouseLocation`, aún no formalizada.
- `Return()` — nombre tentativo por convención con `BoxReturned`.
- `Archive()` — nombre tentativo por convención con `BoxArchived`.

**Punto abierto:** `AddFolder()` y `RemoveFolder()` (correspondientes a los eventos `FolderAdded` y `FolderRemoved`) no se definen formalmente en esta versión, siguiendo el mismo criterio ya aplicado a la gestión de `WarehouseLocation` en `WarehouseAggregate.md v2.0`: sus firmas dependen de los atributos de `Folder`, que aún no están definidos.

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

# Domain Events

Según `BoxAggregate.md` (catálogo original, conservado sin modificaciones):

- `BoxCreated` — emitido por `Create()`.
- `BarcodeAssigned` — emitido por `AssignBarcode()`.
- `FolderAdded` — emisión diferida hasta que `Folder` se implemente formalmente.
- `FolderRemoved` — emisión diferida hasta que `Folder` se implemente formalmente.
- `BoxMoved` — emitido por `Move()`.
- `BoxArchived` — emitido por `Archive()`.
- `BoxReturned` — emitido por `Return()`.

**Punto abierto:** no existe evento documentado para las transiciones `Created → Available`, `Available → In Storage` ni `In Transit → Loan Process`. No se inventan eventos nuevos para cubrir estos vacíos, siguiendo el mismo criterio ya aplicado en `Project` y `Warehouse`.

---

# Relaciones con otros Aggregates

- **Warehouse → Box**: relación pendiente. Según `WarehouseAggregate.md v2.0`, `Box` referenciará una `WarehouseLocation` (Entity interna de `Warehouse`) por identidad; no se referencia a `Warehouse` directamente. La estructura exacta de esta referencia queda pendiente hasta que `WarehouseLocation` se formalice.
- **Project → Box**: relación puramente conceptual, sugerida en el diagrama "Relaciones" de la versión 1.0.0, pero sin invariante que la exija ni Aggregate que la modele estructuralmente.
- **Box → Folder**: relación de composición interna. `Folder` se modela como Entity interna de `Box` (no como Aggregate Root ni como referencia externa), por analogía con la decisión ya tomada para `WarehouseLocation` en `Warehouse`.

---

# Value Objects utilizados

- `BoxCode` — código de negocio de la caja.
- `Barcode` — código de barras físico de la caja.
- `FolderCode` — candidato para identificar la Entity interna `Folder`; no confirmado como definitivo hasta que se formalicen los atributos de `Folder` (ver "Colecciones internas").

**No se utilizan en este Aggregate** (pertenecen a otros Aggregates y no deben duplicarse aquí): `ContractNumber` (Contract), `ProjectCode` (Project), `WarehouseCode` (Warehouse), `Nit` (Customer), `Address`, `PhoneNumber`, `Email`, `PersonName`, `Money`.

---

# Pendientes

- Definir estándar de numeración de cajas.
- Definir capacidad máxima recomendada.
- Validar reglas físicas de conservación.
- Resolver la inconsistencia de clasificación: `DomainMap.md` ubica a `Box` como Entity (no Aggregate Root) y `01_DomainModel.md` no lo lista en absoluto; `BoxAggregate.md` lo define como Aggregate Root desde su versión 1.0.0.
- Definir los atributos completos de la Entity interna `Folder` y confirmar formalmente si `FolderCode` es su identificador definitivo.
- Definir los métodos `AddFolder()` y `RemoveFolder()` una vez `Folder` esté formalmente definida.
- Definir la referencia de `Box` a `WarehouseLocation` (nombre y tipo del campo), una vez `WarehouseLocation` cuente con atributos propios.
- Definir el evento o método (si corresponde) para las transiciones `Created → Available`, `Available → In Storage` e `In Transit → Loan Process`.
- Definir los parámetros exactos de `Move()`, actualmente dependientes de la referencia pendiente a `WarehouseLocation`.
- Confirmar si `Box` requiere alguna relación estructural con `Project`, actualmente solo sugerida conceptualmente.
- Precisar si el término "Documentary Unit" (mencionado como pendiente en `ProjectAggregate.md v2.0`) corresponde a `Box`, a `Folder`, o a ambos en conjunto.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Versión inicial del Aggregate Box. |
| 2.0.0 | 2026-07-21 | Se incorporan Atributos del Aggregate, Referencias a otros Aggregates, Colecciones internas (`Folder` como Entity interna, por analogía con `WarehouseLocation`), Diagrama de estados, Métodos públicos y privados, y Factory Method. Se documenta la inconsistencia de clasificación de `Box` respecto a `DomainMap.md` y `01_DomainModel.md`, y se dejan señaladas como pendientes las transiciones de Lifecycle sin evento asociado, siguiendo la misma metodología aplicada en `ProjectAggregate.md v2.0` y `WarehouseAggregate.md v2.0`. |

# Contract Specification

| Campo | Valor |
|--------|-------|
| Documento | ContractSpecification |
| Aggregate | Contract |
| Bounded Context | Contract Management |
| Fuentes | ContractAggregate.md, 01_DomainModel.md, 02_DomainEvents.md, DomainMap.md, UbiquitousLanguage.md |
| Estado | Draft |

> Este documento consolida, sin añadir reglas nuevas, lo ya definido en la documentación de dominio existente. Donde la documentación fuente es ambigua o incompleta, se señala explícitamente como punto abierto en lugar de asumirse.

---

# Propósito

Servir como referencia oficial y única fuente de verdad para la futura implementación en C# del Aggregate `Contract`, consolidando su modelo, estados, invariantes, eventos y relaciones tal como están definidos en la documentación de dominio de ArchiCore.

---

# Aggregate Root

`Contract`

---

# Responsabilidades

Según `ContractAggregate.md` y `UbiquitousLanguage.md`, el Aggregate `Contract` es responsable de:

- Representar el acuerdo comercial firmado entre un cliente y MULTISERTEC.
- Actuar como el punto de inicio de la operación dentro del ERP.
- Garantizar que todo proyecto pertenezca obligatoriamente a un contrato.
- Controlar su propio ciclo de vida (estados) y las transiciones entre ellos.
- Gestionar la colección de proyectos asociados, incluyendo la unicidad de sus códigos.

---

# Propiedades

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único del contrato. |
| ContractNumber | ContractNumber | `ContractNumber` | Sí | Número identificador del contrato (formato `CTR-`). Listado como atributo obligatorio del aggregate. |
| ClientId | Guid | N/A — referencia a otro Aggregate (`Customer`) | Sí | Referencia al cliente propietario del contrato. La regla "un contrato siempre pertenece a un cliente" (`ContractAggregate.md`) hace obligatoria esta relación. |
| StartDate | DateTime | No definido en la documentación fuente | Sí | Fecha de inicio del contrato, listada como atributo en `ContractAggregate.md` y como parte de lo que "define" un contrato en `UbiquitousLanguage.md`. |
| EndDate | DateTime | No definido en la documentación fuente | No especificado explícitamente | Fecha de finalización del contrato. `UbiquitousLanguage.md` la menciona como parte de lo que un contrato "define"; la documentación no aclara si es obligatoria desde la creación o se asigna al finalizar. Ver "Consideraciones futuras". |
| TotalValue | Money | `Money` | Sí | Valor económico del contrato ("valor económico" en `UbiquitousLanguage.md`; `TotalValue` en `ContractAggregate.md`). Clasificado como Value Object financiero en `DomainMap.md`. |
| Status | ContractStatus (enum) | N/A (enum de dominio) | Sí | Estado actual del contrato dentro de su ciclo de vida. |

---

# Colecciones

| Nombre | Descripción |
|--------|-------------|
| Projects | Colección de proyectos asociados al contrato. `ContractAggregate.md` indica que el contrato "Contiene: Projects" y expone los métodos `AddProject()` y `RemoveProject()`. `Project` es un Aggregate Root independiente (`DomainMap.md`), por lo que —siguiendo el principio DDD de referenciar otros aggregates por identidad— la colección debería almacenar referencias (p. ej. `ProjectId`, y los datos mínimos necesarios como `ProjectCode` para validar la invariante de unicidad) en lugar de instancias completas de `Project`. La documentación fuente no especifica el tipo exacto de elemento almacenado; este punto queda abierto para la fase de implementación. |

---

# Estados

Definidos en `ContractAggregate.md`:

- Draft
- Active
- Suspended
- Finished
- Cancelled

---

# Diagrama de transiciones

```
Draft ──Activate()──▶ Active
Active ──Suspend()──▶ Suspended
Suspended ──Activate()──▶ Active   (reanudación; no se define un método explícito distinto en la documentación)
Active ──Finish()──▶ Finished
```

**Punto abierto:** la documentación fuente lista el estado `Cancelled` y menciona en las reglas que un contrato "puede cerrarse", pero no define un método `Cancel()` ni especifica desde qué estado(s) es alcanzable `Cancelled`, ni si `Finish()` es equivalente a "cerrarse". Esta transición no puede representarse sin inventar una regla y debe aclararse con el negocio antes de implementarse.

---

# Invariantes

Basadas en la sección "Reglas" de `ContractAggregate.md`:

- Un contrato siempre pertenece a un cliente (`ClientId` no puede ser nulo/vacío).
- Un contrato no puede tener dos proyectos con el mismo código (`ProjectCode` único dentro de la colección `Projects`).
- Un contrato no puede finalizar antes de su fecha de inicio.

---

# Reglas de negocio

Tomadas literalmente de `ContractAggregate.md`:

- Un contrato puede contener múltiples proyectos.
- Un contrato puede cambiar de estado.
- Un contrato puede cerrarse.
- Un contrato puede suspenderse.

---

# Eventos de dominio

Según `ContractAggregate.md`:

- `ContractCreated`
- `ContractActivated`
- `ContractSuspended`
- `ContractFinished`
- `ProjectAdded`

**Punto abierto — inconsistencia entre documentos:** `02_DomainEvents.md` cataloga para el contexto de contratos los eventos `ContractCreated`, `ContractActivated`, `ContractExpired` y `ContractClosed` (sin `ContractSuspended`, `ContractFinished` ni `ProjectAdded`); y `DomainMap.md` solo lista `ContractCreated` (además de `ProjectCreated`, no `ProjectAdded`). No se unifica aquí ni se decide cuál nomenclatura es la correcta — se deja constancia de la discrepancia para resolución antes de implementar los eventos en código.

---

# Métodos públicos

Según `ContractAggregate.md`:

- `Create()`
- `Activate()`
- `Suspend()`
- `Finish()`
- `AddProject()`
- `RemoveProject()`

---

# Métodos privados

La documentación fuente no especifica métodos privados para el Aggregate `Contract`. No se infieren ni se inventan; deberán definirse en la fase de implementación en función de necesidades internas de encapsulamiento (p. ej. validaciones reutilizadas entre métodos públicos).

---

# Factory Methods

- `Create()`: único factory method documentado en `ContractAggregate.md` para la construcción de una instancia de `Contract`.

---

# Restricciones

Heredadas de los estándares arquitectónicos ya establecidos para el proyecto ArchiCore:

- El Aggregate debe residir en `ArchiCore.Domain` y no depender de EF Core, ASP.NET, Infrastructure ni librerías externas.
- Debe heredar de `Entity` e implementar `IAggregateRoot`.
- Debe utilizar `Result<T>` / `Error` para el manejo de fallos de validación, sin lanzar excepciones para errores de negocio.
- Debe utilizar los Value Objects ya definidos (`ContractNumber`, `Money`) en lugar de tipos primitivos para las propiedades correspondientes.

---

# Relaciones con otros Aggregates

Según `DomainMap.md`:

- **Customer → Contract**: relación de propiedad; un contrato siempre pertenece a un cliente (referenciado vía `ClientId`).
- **Contract → Project**: un contrato contiene múltiples proyectos; cada proyecto pertenece a un único contrato (`UbiquitousLanguage.md`). `Project` es un Aggregate Root independiente.

---

# Dependencias

- `ContractNumber` (Value Object)
- `Money` (Value Object)
- `ContractStatus` (enum de dominio, pendiente de definición formal)
- `Entity`, `IAggregateRoot` (infraestructura base del dominio)
- `Result`, `Error` (manejo de resultados)
- Referencia por identidad a `Customer` (Aggregate) vía `ClientId`
- Referencia por identidad a `Project` (Aggregate) dentro de la colección `Projects`

---

# Consideraciones para EF Core

(Aplicables únicamente a la capa de Infrastructure; el Aggregate en sí no debe conocer EF Core.)

- Los Value Objects (`ContractNumber`, `Money`) deberán mapearse como Owned Types o mediante conversiones de propiedad.
- La colección `Projects`, al almacenar referencias a otro Aggregate Root, no debe mapearse como una navegación de composición fuerte; debe evaluarse si se persiste como lista de `ProjectId` o mediante una tabla de relación gestionada desde Infrastructure.
- El campo `Status` (enum) deberá mapearse con conversión explícita a valor almacenado (string o int), evitando dependencia del orden del enum.
- Los eventos de dominio no deben persistirse como parte del propio Aggregate; deben despacharse tras el `SaveChanges` mediante el mecanismo de outbox u otro definido en Infrastructure.

---

# Consideraciones para Testing

- Probar que `Create()` solo produce un `Contract` válido cuando `ContractNumber`, `ClientId`, `StartDate` y `TotalValue` son válidos, y falla (vía `Result.Failure`) en caso contrario.
- Probar cada transición de estado documentada (`Activate()`, `Suspend()`, `Finish()`) tanto en su camino exitoso como en transiciones no permitidas desde el estado actual.
- Probar la invariante de unicidad de `ProjectCode` al invocar `AddProject()` con un código ya existente en la colección.
- Probar la invariante "no puede finalizar antes de su fecha de inicio".
- Probar que cada método público que cambia el estado del Aggregate registra el evento de dominio correspondiente.
- Probar `RemoveProject()` sobre un proyecto existente y sobre uno inexistente.

---

# Consideraciones futuras

Puntos que la documentación actual no resuelve y que deberán aclararse con el negocio antes o durante la implementación:

- Definir el método y las condiciones para alcanzar el estado `Cancelled` (no documentado).
- Aclarar si `EndDate` es obligatorio en la creación del contrato o se asigna únicamente al finalizar.
- Resolver la discrepancia de nomenclatura de eventos entre `ContractAggregate.md` (`ContractSuspended`, `ContractFinished`, `ProjectAdded`) y `02_DomainEvents.md` (`ContractExpired`, `ContractClosed`), y la ausencia de estos eventos en `DomainMap.md`.
- `UbiquitousLanguage.md` menciona "servicios contratados" como parte de lo que un contrato define; este concepto no aparece como atributo en `ContractAggregate.md` y no ha sido modelado aún.
- Definir formalmente el tipo `ContractStatus` (enum) y sus valores permitidos, actualmente solo enumerados como texto.
- Definir el mecanismo exacto de referencia dentro de la colección `Projects` (solo `ProjectId`, o `ProjectId` + `ProjectCode` para soportar la validación de unicidad sin consultar el repositorio de `Project`).

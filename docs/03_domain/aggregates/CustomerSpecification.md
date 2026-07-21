# Customer Specification

| Campo | Valor |
|--------|-------|
| Documento | CustomerSpecification |
| Aggregate | Customer |
| Bounded Context | Customer Management |
| Fuentes | CustomerAggregate.md, 01_DomainModel.md, DomainMap.md, UbiquitousLanguage.md |
| Estado | Draft |

> Este documento consolida, sin añadir reglas nuevas, lo ya definido en `CustomerAggregate.md` y en la documentación de dominio existente. `CustomerAggregate.md` tiene un nivel de detalle considerablemente menor que `ContractAggregate.md` (no enumera atributos, métodos ni transiciones con el mismo detalle); donde falta información, se señala explícitamente como **Punto abierto** en lugar de asumirse.

---

# Propósito

Representar a la empresa contratante que adquiere los servicios ofrecidos por MULTISERTEC. El cliente constituye el nivel más alto de la relación comercial y puede mantener múltiples contratos, activos o históricos. Toda la operación documental ejecutada por MULTISERTEC pertenece, en última instancia, a un cliente.

---

# Aggregate Root

`Customer`

---

# Responsabilidades

Según `CustomerAggregate.md`:

- Registrar la información general del cliente.
- Mantener el estado del cliente.
- Gestionar la información de contacto.
- Mantener la información tributaria.
- Asociar contratos.
- Mantener el historial comercial.

---

# Propiedades

`CustomerAggregate.md` no incluye una sección de "Atributos" equivalente a la de `ContractAggregate.md`. Únicamente pueden confirmarse las siguientes propiedades:

| Nombre | Tipo | Value Object utilizado | Obligatorio | Descripción |
|--------|------|--------------------------|-------------|-------------|
| Id | Guid | N/A (heredado de `Entity`) | Sí | Identificador único e inmutable del cliente, según la sección "Identidad de Negocio". |
| Nit | string / VO | `Nit` (ya implementado en el dominio; clasificado en `DomainMap.md` bajo Value Objects → Empresa) | No especificado como obligatorio en el documento fuente, aunque se indica que "podrá utilizarse como clave de negocio cuando aplique" | Número de identificación tributaria del cliente. La invariante "El NIT no podrá duplicarse" confirma su uso como atributo del Aggregate. |
| Status | Enum (Prospect / Active / Inactive / Archived) | N/A (enum de dominio) | Sí (implícito, ya que el ciclo de vida siempre exige un estado) | Estado actual del cliente dentro de su ciclo de vida. |

**Punto abierto:** las Responsabilidades mencionan explícitamente "información general del cliente", "información de contacto" e "información tributaria", pero `CustomerAggregate.md` no enumera los atributos concretos (p. ej. nombre/razón social, correo de contacto, teléfono de contacto, dirección). `DomainMap.md` clasifica los Value Objects `PersonName`, `Email`, `PhoneNumber` (categoría "Personas") y `Address` (categoría "Empresa") en el dominio general, y estos encajan conceptualmente con "información de contacto", pero ningún documento fuente los asigna explícitamente como propiedades del Aggregate `Customer`. No se listan como propiedades confirmadas para evitar inventar atributos.

---

# Colecciones

`CustomerAggregate.md` no declara ninguna colección interna del Aggregate `Customer` (a diferencia de `Contract`, que sí declara y expone `Projects`).

La sección "Relaciones" del propio documento presenta únicamente una jerarquía conceptual (`Customer → Contracts → Projects → Billing`), no una colección gestionada por el Aggregate.

**Punto abierto:** no se especifica si `Customer` debe mantener una colección interna de referencias a `Contract` (p. ej. `ContractId`), o si la relación se resuelve exclusivamente desde el lado de `Contract` (que ya almacena `ClientId`, según `ContractSpecification.md`). Dado que el invariante "El NIT no podrá duplicarse" y el conteo de contratos no requieren necesariamente que `Customer` posea dicha colección, este punto queda pendiente de definición.

---

# Estados

Según la sección "Ciclo de Vida" de `CustomerAggregate.md`:

- Prospect
- Active
- Inactive
- Archived

---

# Diagrama de transiciones

El documento fuente presenta el ciclo de vida como una secuencia lineal:

```
Prospect ──▶ Active ──▶ Inactive ──▶ Archived
```

**Punto abierto:** `CustomerAggregate.md` no define los métodos que producen cada transición, ni si el flujo es estrictamente unidireccional (por ejemplo, si un cliente `Inactive` puede volver a `Active`, o si `Prospect` puede pasar directamente a `Inactive`/`Archived`). Tampoco se especifica qué evento de negocio dispara cada cambio de estado más allá de los nombres de eventos listados (ver sección "Eventos de dominio"). Esta información debe aclararse con el negocio antes de implementarse.

---

# Invariantes

Listadas textualmente en `CustomerAggregate.md` bajo "Invariantes" (el documento fuente no distingue entre invariantes estructurales y reglas de negocio; ver nota en la siguiente sección):

- Todo contrato pertenece a un único cliente.
- Un cliente puede tener múltiples contratos.
- Un cliente archivado no podrá generar nuevos contratos.
- El cliente conserva su historial aun cuando todos sus contratos finalicen.
- El NIT no podrá duplicarse.

---

# Reglas de negocio

**Punto abierto:** a diferencia de `ContractAggregate.md` (que separa "Reglas" de forma independiente), `CustomerAggregate.md` no distingue entre invariantes y reglas de negocio; presenta una única sección "Invariantes" (reproducida arriba). No se listan reglas de negocio adicionales para evitar duplicar o reinterpretar el contenido ya cubierto en la sección anterior.

---

# Eventos de dominio

Según `CustomerAggregate.md`:

- `CustomerCreated`
- `CustomerUpdated`
- `CustomerActivated`
- `CustomerDeactivated`
- `CustomerArchived`

---

# Métodos públicos

`CustomerAggregate.md` no incluye una sección equivalente a "Métodos del Aggregate" de `ContractAggregate.md`.

**Punto abierto:** no existen métodos públicos documentados explícitamente. Por simetría con los eventos de dominio listados (que siguen la convención de nombrarse en pasado tras la ejecución de un método), podrían inferirse candidatos como `Create()`, `Update()`, `Activate()`, `Deactivate()` y `Archive()`; sin embargo, ni sus firmas, ni sus parámetros, ni las condiciones exactas de cada transición están definidos en la documentación fuente. No se asumen como definitivos.

---

# Métodos privados

No especificados en la documentación fuente.

---

# Factory Methods

**Punto abierto:** no se documenta explícitamente un factory method. Siguiendo el mismo patrón utilizado en `Contract` (`Create()`) y la existencia del evento `CustomerCreated`, es razonable anticipar un método estático `Create()`, pero su firma y validaciones no están definidas en `CustomerAggregate.md`.

---

# Restricciones

Heredadas de los estándares arquitectónicos ya establecidos para el proyecto ArchiCore:

- El Aggregate debe residir en `ArchiCore.Domain` y no depender de EF Core, ASP.NET, Infrastructure ni librerías externas.
- Debe heredar de `Entity` e implementar `IAggregateRoot`.
- Debe utilizar `Result<T>` / `Error` para el manejo de fallos de validación, sin lanzar excepciones para errores de negocio.
- Debe utilizar Value Objects ya definidos (p. ej. `Nit`) en lugar de tipos primitivos, cuando corresponda.

---

# Relaciones con otros Aggregates

Según la sección "Relaciones" de `CustomerAggregate.md` y `DomainMap.md`:

- **Customer → Contract**: un cliente puede tener múltiples contratos; cada contrato pertenece a un único cliente (relación ya materializada desde el lado de `Contract` mediante `ClientId`, según `ContractSpecification.md`).
- **Contract → Project → Billing**: la jerarquía documentada (`Customer → Contracts → Projects → Billing`) es puramente conceptual; `Billing` aparece como bounded context y `Billing Period` como Aggregate Root candidato en `01_DomainModel.md`, pero no existe documentación adicional que precise la relación directa entre `Customer` y `Billing`.

**Punto abierto — inconsistencia entre documentos:** `DomainMap.md`, en su sección "Aggregate Roots", lista `Contract`, `Project`, `User`, `Warehouse` y `Loan`, mas **no incluye `Customer`**, pese a que `01_DomainModel.md` sí lo lista explícitamente como Aggregate Root candidato ("Customer | Candidate") y a que `CustomerAggregate.md` está enteramente dedicado a definirlo como Aggregate Root. Esta discrepancia debe resolverse (actualizando `DomainMap.md`) antes de formalizar la implementación.

---

# Dependencias

- `Nit` (Value Object, ya implementado en el dominio).
- `Entity`, `IAggregateRoot` (infraestructura base del dominio).
- `Result`, `Error` (manejo de resultados).

**Punto abierto:** posibles dependencias adicionales de Value Objects ya implementados (`PersonName`, `Email`, `PhoneNumber`, `Address`) no pueden confirmarse como dependencias del Aggregate `Customer` porque ningún documento fuente los asigna explícitamente a sus propiedades (ver sección "Propiedades").

---

# Consideraciones para EF Core

(Aplicables únicamente a la capa de Infrastructure; el Aggregate en sí no debe conocer EF Core.)

- El Value Object `Nit` deberá mapearse como Owned Type o mediante conversión de propiedad, siguiendo el mismo criterio aplicado a `ContractNumber` y `Money` en `Contract`.
- El campo `Status` (enum: Prospect/Active/Inactive/Archived) deberá mapearse con conversión explícita a valor almacenado, evitando dependencia del orden del enum.
- La invariante "El NIT no podrá duplicarse" requiere una restricción de unicidad a nivel de infraestructura (índice único sobre la columna correspondiente), además de cualquier validación en el dominio.
- Los eventos de dominio no deben persistirse como parte del propio Aggregate; deben despacharse tras el `SaveChanges` mediante el mecanismo de outbox u otro definido en Infrastructure.

---

# Consideraciones para Testing

- Probar que un cliente `Archived` no puede asociarse a un nuevo contrato (invariante documentada).
- Probar que el NIT no puede duplicarse entre dos instancias de `Customer` (requiere coordinación con el repositorio, ya que es una invariante cruzada entre agregados).
- Probar que el historial comercial del cliente se conserva aun cuando todos sus contratos finalicen.
- Probar que cada método que cambie el estado del cliente registra el evento de dominio correspondiente (`CustomerActivated`, `CustomerDeactivated`, `CustomerArchived`).

**Punto abierto:** sin métodos públicos ni transiciones formalmente definidos, no es posible especificar aún casos de prueba para transiciones inválidas de estado (p. ej. intentar archivar un cliente que ya está archivado).

---

# Consideraciones futuras

Puntos que la documentación actual no resuelve y que deberán aclararse con el negocio antes o durante la implementación:

- Definir explícitamente los atributos del Aggregate `Customer` más allá de `Id`, `Nit` y `Status` (nombre/razón social, datos de contacto, etc.), tal como lo hace `ContractAggregate.md` para `Contract`.
- Definir los métodos públicos del Aggregate y sus firmas (`Create()`, y los que correspondan a `Update`, `Activate`, `Deactivate`, `Archive`).
- Definir las reglas exactas de transición entre los estados `Prospect`, `Active`, `Inactive` y `Archived` (incluyendo si son reversibles).
- Resolver la ausencia de `Customer` en la sección "Aggregate Roots" de `DomainMap.md`, pese a estar listado como candidato en `01_DomainModel.md` y definido en `CustomerAggregate.md`.
- Aclarar si `Customer` debe mantener una colección interna de referencias a `Contract`, o si la relación se resuelve únicamente desde `Contract.ClientId`.
- Precisar la relación entre `Customer` y el bounded context `Billing` / Aggregate candidato `Billing Period`, mencionada solo como jerarquía conceptual.
- Los "Pendientes" ya señalados en el propio `CustomerAggregate.md` ("Validar si existen diferentes tipos de clientes", "Validar información tributaria obligatoria", "Validar políticas de eliminación lógica") permanecen sin resolver y deben abordarse antes de finalizar esta especificación.

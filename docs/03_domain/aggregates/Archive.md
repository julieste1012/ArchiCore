# Archive Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Archive Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Documentary Management |
| Aggregate Root | Archive |

---

# Propósito

Representa la estructura archivística administrada por MULTISERTEC conforme a las normas del Archivo General de la Nación (AGN).

Permite organizar, custodiar, localizar y administrar la documentación física y digital perteneciente a un proyecto.

---

# Jerarquía Archivística

La estructura documental será:

Fondo

↓

Sección

↓

Serie Documental

↓

Subserie Documental

↓

Expediente

↓

Carpeta

↓

Documento

↓

Folio

---

# Responsabilidades

- Organizar la documentación.
- Mantener la jerarquía archivística.
- Permitir búsquedas.
- Controlar la ubicación física.
- Controlar la ubicación digital.
- Mantener trazabilidad.
- Registrar movimientos.
- Mantener estados documentales.

---

# Invariantes

- Todo expediente pertenece a una única subserie.
- Toda carpeta pertenece a un único expediente.
- Todo documento pertenece a una única carpeta.
- Todo folio pertenece a un único documento.
- Ningún elemento puede pertenecer simultáneamente a dos padres.
- Todo movimiento debe quedar auditado.

---

# Eventos de Dominio

- ArchiveCreated
- SeriesCreated
- SubseriesCreated
- FileCreated
- FolderCreated
- DocumentRegistered
- FolioAdded
- FolderMoved
- BoxAssigned
- BarcodeAssigned

---

# Relaciones

Archive

↓

Project

↓

Warehouse

↓

Loans

↓

Digitization

---

# Pendientes

- Validar metadatos obligatorios definidos por el AGN.
- Validar reglas oficiales de identificación de expedientes.
- Validar reglas de foliación.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Archive.|

---

# Implementation Addendum 2.0 - Classification boundary

The current technical implementation uses Archive as the Aggregate Root associated with a Project. Its consistency boundary contains:

Archive -> Series -> Subseries -> Expedient.

Series and Subseries are internal entities. Expedient is internal to Subseries. The minimum fields implemented are code/reference and display name/title. The Expedient reference is supplied by the business/application layer; no official AGN numbering semantics are encoded.

Folder is maintained as an independent Aggregate Root and references ArchiveId and ExpedientId. Documents, folios and annexes are internal to Folder. Box remains an independent physical Aggregate Root and references Folder identities rather than owning Folder instances. Cross-aggregate ownership validation and transactional consistency are Application/Infrastructure responsibilities.

This is a provisional technical boundary resolving contradictory classifications found in DomainMap, Folder.md and Box.md. It requires business/architectural review before production release. It does not define TRD/CCD, retention, disposition or official AGN metadata.

Events implemented by this classification increment: ArchiveCreated, SeriesCreated, SubseriesCreated, ExpedientCreated.


---

# Implementation Addendum 2.1 - Full classification hierarchy

The Archive Aggregate Root represents the Project's Fondo documental. The implemented internal hierarchy now follows the source document:

Fondo (Archive) -> Section -> Series -> Subseries -> Expedient.

- Archive owns Sections.
- Each ArchiveSection owns its Series.
- Each ArchiveSeries owns its Subseries.
- Each ArchiveSubseries owns its Expedients.
- Folder remains an independent Aggregate Root referencing ArchiveId and ExpedientId by identity.

The minimum implementation uses code/name for Section and Series, code/name for Subseries, and reference/title for Expedient. These fields provide an initial registration structure, not an approved TRD/CCD catalogue or official AGN identification format. Those metadata and code rules remain pending.

Events: ArchiveCreated, SectionCreated, SeriesCreated, SubseriesCreated and ExpedientCreated.

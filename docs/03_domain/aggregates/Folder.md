# Folder Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Folder Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Documentary Management |
| Aggregate Root | Folder |

---

# Propósito

Representa la carpeta física que contiene uno o varios documentos pertenecientes a un expediente.

La carpeta constituye la unidad documental física manipulada durante los procesos operativos de MULTISERTEC.

---

# Responsabilidades

- Identificar la carpeta.
- Asociar documentos.
- Asociar expediente.
- Asociar caja.
- Mantener ubicación.
- Registrar movimientos.
- Controlar préstamos.
- Registrar digitalización.

---

# Ciclo de Vida

Created

↓

Received

↓

Organized

↓

Stored

↓

Loaned

↓

Returned

↓

Archived

---

# Invariantes

- Toda carpeta pertenece a un único expediente.
- Toda carpeta pertenece a una única caja a la vez.
- Una carpeta archivada no puede modificarse.
- Todo préstamo debe quedar registrado.
- Todo movimiento físico debe generar trazabilidad.

---

# Eventos de Dominio

- FolderCreated
- FolderAssignedToBox
- FolderMoved
- FolderLoaned
- FolderReturned
- FolderDigitized
- FolderArchived

---

# Relaciones

Folder

↓

Archive

↓

Box

↓

Project

↓

Loans

↓

Documents

---

# Pendientes

- Validar reglas de foliación por carpeta.
- Validar metadatos obligatorios del AGN.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Folder.|

---

# Implementation Addendum 1.1 - Aggregate and document model

Folder is implemented as an independent Aggregate Root. It references ArchiveId and ExpedientId; the application layer must verify that the Expedient belongs to the Archive and Project before creation.

## Properties

- FolderCode: required business identifier.
- ArchiveId and ExpedientId: required aggregate identities.
- BoxId: optional current physical-container reference.
- LoanId: optional identity of the active Loan process.
- Status: Created, Received, Organized, Stored, Loaned, Returned or Archived.

## Lifecycle

Created -> Received -> Organized -> Stored -> Loaned -> Returned -> Archived.

- Receive(): Created -> Received.
- Organize(): Received -> Organized.
- Store(boxId): Organized -> Stored.
- MoveToBox(newBoxId): Stored -> Stored, with a new BoxId and a FolderMoved event.
- StartLoan(loanId): Stored -> Loaned.
- ReturnLoan(): Loaned -> Returned.
- Archive(): Returned -> Archived.

## Internal entities

- FolderDocument: required Title and optional DocumentDate.
- Folio: positive sequential number inside one FolderDocument.
- Annex: title and sequence appended after the document's folios.

The current technical assumption starts folio numbering at 1 and requires consecutive additions. Annex sequence follows all folios and previously added annexes. These rules are provisional and must not be represented as official AGN foliation requirements.

## Domain events

FolderCreated, FolderReceived, FolderOrganized, FolderAssignedToBox, FolderMoved, FolderLoaned, FolderReturned, FolderArchived, DocumentRegistered, FolioAdded and AnnexAdded.

## Cross-aggregate consistency

Folder.Store/MoveToBox updates Folder.BoxId. The corresponding Box.FolderIds collection must be synchronized in the same database transaction by an Application use case. Infrastructure must prevent more than one current Box per Folder. The Folder aggregate does not directly mutate a Box aggregate.

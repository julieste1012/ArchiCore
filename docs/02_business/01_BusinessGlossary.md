# Business Glossary

| Campo | Valor |
|--------|-------|
| Documento | 01_BusinessGlossary |
| Versión | 1.0.0 |
| Estado | Approved |
| Fase | 1 - Business Analysis |
| Sprint | 1.2 |
| Última actualización | 2026-07-11 |

---

# Purpose

This glossary defines the official business terminology used throughout the DMS ERP project.

Every document, database entity, API endpoint and source code element must use these definitions consistently.

---

# General Rules

- Each business concept has a single official definition.
- Synonyms should be avoided.
- Technical names may differ from business names only when justified.
- Colombian archival terminology follows AGN standards.

---

# Business Terms

## Customer (Cliente)

The company that hires MULTISERTEC to provide one or more services.

A customer may have multiple active or historical contracts.

---

## Contract (Contrato)

A legal agreement between MULTISERTEC and a customer.

Each contract defines:

- Services
- Start date
- End date
- Economic value
- Documentary capacity
- Billing conditions

A customer may have multiple contracts simultaneously.

---

## Service

A business service provided under a contract.

Examples:

- Documentary management
- Equipment rental
- Digitization
- Reprography
- Technical information management

---

## Warehouse (Bodega)

Physical facility assigned to store documentary assets belonging to one or more contracts.

A warehouse contains aisles, shelves and storage positions.

---

## Aisle (Pasillo)

Subdivision inside a warehouse.

Used to organize physical storage.

---

## Shelf (Estantería)

Physical storage structure located inside an aisle.

---

## Cabinet (Archivador)

Furniture used to store documentary units.

---

## Documentary Series (Serie Documental)

Highest documentary classification level according to AGN.

Groups documents with similar administrative functions.

---

## Documentary Subseries (Subserie Documental)

Subdivision of a documentary series.

Represents a more specific documentary classification.

---

## Expedient (Expediente)

Organized set of documents related to the same administrative matter.

Contains one or more folders.

---

## Folder (Carpeta)

Physical documentary unit that stores one or more documents.

Folders are stored inside boxes.

---

## Document

Individual documentary item belonging to a folder.

A document contains one or more folios.

---

## Folio

Single page belonging to a document.

Represents the minimum documentary unit.

---

## Box (Caja)

Physical container used to store folders.

Each box belongs to a contract.

Each box has a unique barcode.

---

## Barcode

Unique identifier assigned to physical documentary assets.

Used for traceability.

---

## Documentary Loan

Temporary delivery of documentary material.

Loans may be:

- Physical
- Digital

Every loan must be authorized and recorded.

---

## Digitization

Process of converting physical documentation into digital format.

Digitization is performed on demand.

---

## Documentary Custody

Service of preserving documentary material under controlled conditions.

---

## Documentary Retrieval

Process of locating and delivering documentary material requested by the customer.

---

## Billing

Monthly calculation of billable services.

Billing concepts may include:

- Folders
- Linear meters
- Digitization
- Hours
- Other contractual services

Accounting remains managed in SIIGO.

---

## Project Coordinator

Responsible for operational management of a contract.

Main responsibilities include:

- Receiving documentation
- Counting documentation
- Assigning storage
- Authorizing loans

---

## Archive Assistant

Responsible for handling documentary material.

Examples:

- Receiving boxes
- Moving folders
- Delivering loans
- Returning documentation

---

## Archive Technician

Responsible for documentary verification.

Examples:

- Validate contents
- Label folders
- Documentary organization

---

## Audit Log

Immutable record of relevant business events.

Supports traceability.

---

## Business Rule

Mandatory operational rule governing business processes.

Business rules are documented separately.

---

# Glossary Governance

New terms may only be added after business analysis.

Existing definitions must never change without updating dependent documentation.

---

# Version History

| Version | Date | Description |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Initial business glossary. |
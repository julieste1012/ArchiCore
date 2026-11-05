# Domain Implementation Assumptions

| Field | Value |
|---|---|
| Status | Technical assumptions for implementation; not business approval |
| Reviewed | 2026-11-04 |
| Active branch | feat/documentary-domain-foundation |

## Archive and documentary hierarchy

- Archive is an Aggregate Root associated with one Project. Uniqueness of one Archive per Project must be enforced by persistence/application validation before release.
- Archive represents the Project's Fonds (Fondo) and owns the classification catalog Section -> Series -> Subseries -> Expedient.
- Section, Series and Subseries codes are unique within their parent; Expedient references are unique within their Subseries.
- Expedient stores a user-supplied reference and title. The reference is not an official AGN numbering scheme.
- Folder remains an independent Aggregate Root, following its standalone specification. It references ArchiveId and ExpedientId by identity.
- Folder owns its Document entities; each Document owns its Folio and Annex entities.
- Box remains an Aggregate Root and maintains Folder IDs as references. Folder stores its current BoxId. A database unique constraint on active Folder-to-Box assignment and an application transaction must keep both sides consistent.
- A Folder cannot be created until the application layer confirms that ExpedientId belongs to ArchiveId and that both belong to the intended Project.
- Document title is required; document date is optional. Official AGN metadata remains outside this minimum implementation.
- Folio numbering starts at 1 and increments consecutively within each Document. Annexes are appended after the folio sequence. This is a technical placeholder, not a substitute for official foliation rules.

## Box lifecycle

- Box remains an Aggregate Root despite the older DomainMap classifying it as an Entity; this follows the dedicated Box aggregate specification.
- A barcode can be assigned once, only while Box is Created.
- Created -> Available requires a barcode.
- Available -> InStorage requires a WarehouseLocation identity. The Application use case must atomically occupy that location inside Warehouse and update Box.
- InStorage -> InTransit represents removing the box from its current physical location for a movement/loan workflow. The Application use case must release occupancy in Warehouse in the same transaction.
- InTransit -> LoanProcess -> Returned -> Archived completes the documented loan-related box lifecycle.
- Box retains folder identity references when archived for historical traceability.
- Box stores only Folder IDs. Existence, unique current membership and atomic coordination with Folder.BoxId must be enforced by Application/Infrastructure.

## Warehouse physical hierarchy

- Warehouse remains the Aggregate Root.
- WarehouseLocation is an internal Entity and can only be mutated through Warehouse.
- The chosen hierarchy for the initial implementation is Aisle -> Shelf -> Level -> Position.
- Only Position stores boxes. Its capacity is a positive integer; parent levels do not carry box capacity.
- Location codes are unique within one warehouse, compared case-insensitively after trimming and uppercasing.
- A box may occupy at most one position within the same warehouse.
- A position cannot exceed its configured capacity.
- Moving a box between positions in one warehouse validates destination capacity before releasing the source and emits release, occupy and relocation events.
- A warehouse cannot close or archive while any position still contains a box.
- Cross-warehouse movement must be coordinated by an application use case and a database transaction.

## Digitization

- DigitizationRequest is an Aggregate Root linked by identity to Project and Folder.
- Its initial state machine is Requested -> Processing -> Completed.
- A request requires project, folder and requester identities.
- Starting and completing the request require an authenticated operator identity.
- A completed request must contain at least one digital file.
- Digital files are represented by metadata only: opaque object key, original filename, MIME content type, byte length, SHA-256 and page count.
- Binary content remains in IObjectStorage; the domain does not accept streams or implement download authorization.
- Object keys must be relative, unique within the request and free of path traversal segments.
- The application layer must verify that the folder belongs to the project, store and scan the bytes, validate actual MIME/checksum/page count, and persist object metadata consistently.

## Implementation status

Reviewed: 2026-11-05. Active delivery: PR #8, \`feat/documentary-domain-foundation\`.

| Capability | Implementation status |
|---|---|
| EF Core mappings and PostgreSQL migrations for Archive, Folder, Box, WarehouseLocation, Loan and DigitizationRequest | Implemented |
| EF-backed repositories and dependency-injection registration | Implemented |
| Archive hierarchy read/write API | Implemented |
| Box inventory, barcode, availability, storage and relocation API | Implemented |
| Warehouse physical-location hierarchy and occupancy operations | Implemented |
| Folder inventory, document registration, foliation, annexes and box assignment API | Implemented |
| Loan request, authorization, delivery, return and history API | Implemented with provisional workflow rules |
| Digitization request, start, PDF upload, metadata registration and authorized download API | Implemented for PDF |
| Role-based API policies and role-visible React documentary workspace | Implemented |
| Cross-aggregate operations coordinated before one EF Core SaveChanges call | Implemented for the documented application workflows |
| Automated overdue detection and notification delivery | Pending |
| Antivirus/malware scanning and digital preservation controls | Pending |
| Business acceptance and detailed rules for business processes still marked Pending | Pending |

The API exposes authenticated endpoints through \`DocumentaryOperationsController\`. PDF upload currently allows 1–10 files per request, with a maximum of 40 MiB per file and 80 MiB per batch. The server verifies the PDF signature, parses the document to determine its page count, computes SHA-256, generates the storage key, stores the object and then persists metadata. Downloads are authorized through the Digitization.Read policy. These controls do not constitute antivirus scanning or PDF/A validation.

The React workspace exposes tabs for classification, boxes and warehouses, folders, loans and digitization. The interface uses the existing API client and role visibility model. Backend integration tests cover the documentary archive route separately from the Project lifecycle archive route.

## Explicitly not implemented

- Official AGN mandatory metadata, TRD/CCD versioning, retention schedules, disposition, official expediente numbering and foliation exceptions.
- Loan rejection, cancellation, renewal, partial return, maximum duration, business-day calculation, grace periods and escalation.
- Scheduled overdue detection and notifications. The \`Overdue\` state exists in the domain, but no recurring worker or scheduler currently changes loans to that state.
- Antivirus/malware scanning, OCR, PDF/A conversion, signatures, legal holds, retention enforcement or digital preservation certification.
- Fine-grained authorization by project/customer/contract scope. Current policies authorize by authenticated business role; project-scope checks must be added where business requirements require them.
- Customer portal operations. CustomerRepresentative remains reserved for a future portal.

These are tracked implementation gaps, not silently assumed as completed requirements.


## Loan lifecycle

- Loan is an Aggregate Root because the business language describes a controlled temporary checkout of a Folder with its own requester, authorization, dates and return.
- Loan references Project and Folder by identity; it does not own the Folder or mutate it directly.
- The implemented state machine is Requested -> Authorized -> Delivered -> Returned, with an optional Delivered -> Overdue -> Returned path.
- DueAtUtc is required and must be after the request timestamp.
- Authorization and delivery require identified users; role and project-scope authorization belongs to Application/API.
- A delivered loan becomes overdue only when a background/application operation evaluates it after DueAtUtc. This increment does not schedule a worker or send notifications.
- Return is allowed from Delivered or Overdue and records the receiving user and UTC timestamp.
- Application must synchronize Loan.Start/Return operations with Folder.StartLoan/ReturnLoan in a single transaction.
- Rejection, cancellation, renewal, partial return, maximum loan duration, working-day calculation, grace periods and escalation policy are not specified and remain unimplemented.


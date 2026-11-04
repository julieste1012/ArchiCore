# Domain Implementation Assumptions

| Field | Value |
|---|---|
| Status | Technical assumptions for implementation; not business approval |
| Reviewed | 2026-11-03 |
| Active branch | feat/documentary-domain-foundation |

## Purpose

This register makes explicit the technical assumptions used to progress where existing drafts are incomplete or contradictory. It does not claim that the business owner has formally approved these rules. Any future business decision can supersede an assumption through a documented specification/ADR update and a controlled migration.

## Warehouse physical hierarchy

- Warehouse remains the Aggregate Root.
- WarehouseLocation is an internal Entity and can only be mutated through Warehouse.
- The chosen hierarchy for the initial implementation is Aisle -> Shelf -> Cabinet -> Position.
- Only Position stores boxes. Its capacity is a positive integer; parent levels do not carry box capacity.
- Location codes are unique within one warehouse, compared case-insensitively after trimming and uppercasing.
- A box may occupy at most one position within the same warehouse.
- A position cannot exceed its configured capacity.
- Moving a box between positions in one warehouse validates destination capacity before releasing the source and emits release, occupy and relocation events.
- A warehouse cannot close or archive while any position still contains a box.
- Cross-warehouse movement must be coordinated by an application use case and a database transaction; it is not provided by this domain increment.

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

## Explicitly not implemented by this increment

- Official AGN mandatory metadata, classification schedules, expediente numbering and foliation exceptions.
- Final aggregate boundaries among Archive, Folder, Box, Document and Folio.
- Digitization rejection, cancellation, rework, approval, SLA and operator assignment policies.
- File retention, legal holds, OCR, PDF/A conversion, signatures or digital preservation requirements.
- EF Core mappings, migrations, repositories, API endpoints and frontend screens for WarehouseLocation and DigitizationRequest.

These are tracked implementation gaps, not silently assumed as completed requirements.

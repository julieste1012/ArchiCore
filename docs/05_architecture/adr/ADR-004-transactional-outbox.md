# ADR-004: Transactional Outbox and Domain-Event Audit Projection

| Field | Value |
|---|---|
| Status | Accepted and implemented on `feat/backend-core-api` |
| Date | 2026-10-28 (updated 2026-10-29) |
| Scope | EF Core domain-event capture, processing and audit projection |

## Context

Domain aggregates collect `IDomainEvent` instances in `Entity.DomainEvents`. Dispatching only in memory leaves a crash window in which an aggregate update is committed but its event is lost. The core API also needs a durable history of domain events.

## Decision

- `ArchiCoreDbContext.SaveChanges/SaveChangesAsync` collects events from tracked entities and adds `OutboxMessages` before EF Core saves changes.
- Aggregate updates and outbox rows are saved in the same relational transaction.
- The event ID is the outbox primary key. The outbox stores the fully qualified event type, UTC occurrence/enqueue times and JSONB payload.
- In-memory domain events are cleared only after a successful save.
- A hosted worker runs outside the Testing environment. It polls pending records and uses PostgreSQL `FOR UPDATE SKIP LOCKED` to coordinate multiple API instances.
- For each batch, the processor writes an immutable `DomainEventAuditEntries` projection and marks the outbox rows processed in the same database transaction. A failed batch rolls back and remains pending for a later poll.
- `GET /api/audit/domain-events` exposes a paginated, filterable view of the audit projection to the SystemAdministrator role only.

## Consequences and limits

- The aggregate state and its outbox event are atomically persisted.
- The audit projection is idempotent by event ID; the same ID cannot create multiple audit entries.
- A failed processor batch is retried on a later poll. There is not yet a retry counter, poison-message quarantine, retention/cleanup policy, delivery metrics or alerting.
- This is a domain-event audit, not a complete actor-level audit. Current domain events do not consistently carry actor/user/request context, and authentication events are not written to a separate security-audit stream.
- Audit payloads preserve event JSON. Before production, approve PII visibility, retention and redaction policies. The audit endpoint is currently restricted to administrators.
- External event delivery/handlers are still future work; no arbitrary .NET type activation is used by this generic audit processor.

## Verification

The integration suite runs against a disposable PostgreSQL 17 Testcontainers database and verifies event persistence, event clearing after successful saves, rollback on failed aggregate writes, outbox processing, audit projection, processed-state marking, idempotent reprocessing and administrator-only API access.

## Alternatives considered

- **In-memory-only dispatch:** rejected because a crash can lose events after the aggregate is persisted.
- **Dispatch before persistence:** rejected because side effects can occur if the database transaction fails.
- **Treat the outbox itself as the audit log:** rejected because pending/processed delivery state is not a replacement for an immutable audit projection.

# ADR-004: Transactional Outbox for Domain Events

| Field | Value |
|---|---|
| Status | Accepted for the current backend foundation |
| Date | 2026-10-28 |
| Scope | EF Core persistence for existing domain events |

## Context

Domain aggregates already collect `IDomainEvent` instances in `Entity.DomainEvents`. EF mappings deliberately ignore that in-memory collection, and no dispatcher currently processes events. Dispatching only in memory would leave a crash window in which an aggregate update is committed but its event is lost.

## Decision

- When `ArchiCoreDbContext.SaveChanges` or `SaveChangesAsync` runs, collect events from tracked domain entities and add corresponding `OutboxMessages` before EF Core saves changes.
- Save aggregate changes and outbox rows in the same relational transaction.
- Use the domain event ID as the outbox primary key so one event ID cannot produce multiple persisted rows.
- Store the event's full type name, UTC occurrence time, enqueue time and JSONB payload.
- Clear domain events from aggregates only after `SaveChanges` succeeds. If saving throws, the aggregate retains its pending events.
- Use the `AddTransactionalOutbox` migration to create the outbox table and a filtered index for unprocessed records.

## Consequences

- Aggregate state and its event record are committed atomically for the current EF Core/PostgreSQL unit of work.
- The outbox provides durable event capture only. It is not an audit log and it does not yet dispatch events to handlers, external systems or an audit processor.
- Until a dispatcher and cleanup policy are implemented, unprocessed rows remain in the table.
- Future dispatch must use an allow-listed event-type registry rather than arbitrary type activation, be idempotent, and mark a message processed only after handling succeeds.
- Retry, poison-message handling, delivery metrics and retention remain follow-up work.

## Verification

The integration suite applies the real migration to a disposable PostgreSQL 17 Testcontainers instance and checks that a customer-creation event is persisted, and that a successful second save does not duplicate the event.

## Alternatives considered

- **In-memory-only dispatch:** rejected because a process crash can lose events after aggregate persistence.
- **Dispatch before persistence:** rejected because external side effects can occur even if the database transaction fails.
- **Treat the outbox as an audit trail:** rejected because delivery/processing state and a durable business audit record have different purposes and retention semantics.

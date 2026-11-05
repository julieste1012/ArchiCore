# Loan Aggregate Specification

| Field | Value |
|---|---|
| Aggregate | Loan |
| Bounded Context | Documentary Management |
| Status | Draft technical specification |
| Reviewed | 2026-11-04 |

> BP-010 (Loan) and BP-011 (Return) remain Pending in the Business Process index. This specification implements only the minimum lifecycle supported by the Ubiquitous Language and Domain Events catalog. It is not a claim that the full business process is approved.

## Purpose

Control temporary checkout of one documentary Folder, including requester, authorization, custody delivery, due date and return.

## Aggregate Root

Loan.

## References

- ProjectId: project context for the request.
- FolderId: the requested documentary folder.
- RequestedByUserId: the person requesting the loan.
- AuthorizedByUserId: user who authorizes the request.
- DeliveredByUserId: user who records physical custody transfer.
- ReturnedByUserId: user who receives the physical return.

Project, Folder and User are referenced by identity. Loan does not contain or directly mutate these aggregates.

## Lifecycle

Requested -> Authorized -> Delivered -> Returned.

Optional overdue path:

Delivered -> Overdue -> Returned.

- Create(projectId, folderId, requestedByUserId, dueAtUtc) creates Requested and emits LoanRequested.
- Authorize(authorizedByUserId) transitions Requested -> Authorized and emits LoanAuthorized.
- Deliver(deliveredByUserId) transitions Authorized -> Delivered and emits LoanDelivered.
- MarkOverdue(evaluatedAtUtc) transitions Delivered -> Overdue only after DueAtUtc and emits LoanOverdue.
- Return(returnedByUserId, returnedAtUtc) transitions Delivered/Overdue -> Returned and emits LoanReturned.

## Invariants

- ProjectId, FolderId and requester/actor user IDs must be non-empty.
- DueAtUtc must be later than RequestedAtUtc.
- Only Requested can be Authorized.
- Only Authorized can be Delivered.
- Only Delivered can become Overdue and only after its due date.
- Only Delivered or Overdue can be Returned.
- ReturnedAtUtc cannot be earlier than DeliveredAtUtc.
- A returned loan is terminal under the implemented lifecycle.

## Folder consistency

The application use case that delivers a loan must also transition the Folder from Stored to Loaned and set Folder.LoanId to this Loan.Id. The return use case must transition the Folder from Loaned to Returned and clear the active LoanId. Both operations must commit atomically with the Loan aggregate.

## Security and process limits

Role-based approval and project scope are enforced by Application/API. The domain stores the actor IDs but does not decide whether a particular role may authorize a specific Folder. Rejection, cancellation, renewals, partial returns, maximum duration, business-day calculation, grace periods, reminders and escalation are not implemented because the business process specifications remain pending.

## Domain events

- LoanRequested
- LoanAuthorized
- LoanDelivered
- LoanOverdue
- LoanReturned

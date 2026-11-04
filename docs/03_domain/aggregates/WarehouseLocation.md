# WarehouseLocation Entity Specification

| Field | Value |
|---|---|
| Parent Aggregate | Warehouse |
| Entity | WarehouseLocation |
| Status | Draft technical specification |
| Reviewed | 2026-11-03 |

## Purpose

Represent a physical position in a warehouse hierarchy. A location has no lifecycle independent of the Warehouse aggregate and is created or mutated only through Warehouse operations.

## Hierarchy

Aisle -> Shelf -> Cabinet -> Position.

This is the initial implementation assumption based on the warehouse documentation and the physical-storage model. It can be adjusted when Multisertec confirms the exact physical layout.

## Attributes

- Id: inherited immutable entity identity.
- Code: required, trimmed, uppercased and unique within the parent Warehouse; maximum 50 characters.
- Type: Aisle, Shelf, Cabinet or Position.
- ParentLocationId: null only for Aisle; each lower level must point to an existing parent at the immediately preceding level in the same Warehouse.
- Capacity: required and positive only for Position; null for parent levels.
- BoxIds: identities of boxes occupying a Position.

## Invariants and operations

- Aisle has no parent.
- Shelf belongs to an Aisle.
- Cabinet belongs to a Shelf.
- Position belongs to a Cabinet.
- Only Position can store boxes.
- A box cannot occupy two positions inside the same Warehouse.
- Occupancy cannot exceed Position capacity.
- Release requires the box to be currently stored at the specified position.
- MoveBox validates the destination before releasing the source and records both location events plus BoxRelocated.
- Warehouse cannot close or archive while any position remains occupied.

## Events

- LocationCreated
- LocationOccupied
- LocationReleased
- BoxRelocated

## Persistence status

The domain entity and aggregate operations have been implemented and unit tests added. EF Core owned-entity mapping, migrations, repository projections and API endpoints are not yet implemented.

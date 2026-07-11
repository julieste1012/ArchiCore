# Document Lifecycle

| Campo | Valor |
|--------|-------|
| Documento | 07_DocumentLifecycle |
| Versión | 1.0.0 |
| Estado | Approved |
| Fase | 1 - Business Analysis |

---

# Purpose

This document defines the lifecycle of documentary units managed by the ERP.

Every documentary unit transitions through predefined business states.

These transitions are controlled by business processes.

---

# Lifecycle

Pending Reception

↓

Received

↓

Counted

↓

Verified

↓

Classified

↓

Labeled

↓

Registered

↓

Stored

↓

Available

↓

Loaned

↓

Returned

↓

Archived

↓

Delivered to Customer

---

# State Definitions

## Pending Reception

The documentary unit has not yet been received.

---

## Received

Reception process completed.

---

## Counted

Physical counting completed.

---

## Verified

Content verification completed.

---

## Classified

AGN classification completed.

---

## Labeled

Barcode and labels assigned.

---

## Registered

Registered inside ERP.

---

## Stored

Assigned to physical location.

---

## Available

Available for consultation or loan.

---

## Loaned

Temporarily delivered.

---

## Returned

Returned after loan.

---

## Archived

Historical state.

---

## Delivered to Customer

Contract completed.

Documentation returned.

Lifecycle finished.

---

# Transition Rules

State transitions are controlled exclusively by business processes.

No manual state modification is allowed.

---

# Version History

| Version | Date | Description |
|----------|------|-------------|
|1.0.0|2026-07-11|Initial lifecycle definition.|
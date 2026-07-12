# Box Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Box Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Documentary Storage |
| Aggregate Root | Box |

---

# Propósito

Representa la unidad física de almacenamiento utilizada por MULTISERTEC para custodiar carpetas documentales.

Cada caja posee una identidad única, una ubicación física y un historial de movimientos.

La caja constituye la unidad física mínima de almacenamiento administrada por el sistema.

---

# Responsabilidades

- Identificar la caja.
- Registrar código de barras.
- Asociar carpetas.
- Controlar la ubicación física.
- Registrar movimientos.
- Mantener el estado de conservación.
- Registrar auditoría.

---

# Ciclo de Vida

Created

↓

Available

↓

In Storage

↓

In Transit

↓

Loan Process

↓

Returned

↓

Archived

---

# Invariantes

- Cada caja tiene un único código de barras.
- Una caja ocupa una única ubicación física a la vez.
- Una carpeta solo puede pertenecer a una caja a la vez.
- Toda reubicación genera un movimiento.
- Una caja archivada no puede modificarse.

---

# Eventos de Dominio

- BoxCreated
- BarcodeAssigned
- FolderAdded
- FolderRemoved
- BoxMoved
- BoxArchived
- BoxReturned

---

# Relaciones

Box

↓

Warehouse

↓

Folders

↓

Project

---

# Pendientes

- Definir estándar de numeración de cajas.
- Definir capacidad máxima recomendada.
- Validar reglas físicas de conservación.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Box.|
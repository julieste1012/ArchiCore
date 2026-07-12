# Archive Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Archive Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Documentary Management |
| Aggregate Root | Archive |

---

# Propósito

Representa la estructura archivística administrada por MULTISERTEC conforme a las normas del Archivo General de la Nación (AGN).

Permite organizar, custodiar, localizar y administrar la documentación física y digital perteneciente a un proyecto.

---

# Jerarquía Archivística

La estructura documental será:

Fondo

↓

Sección

↓

Serie Documental

↓

Subserie Documental

↓

Expediente

↓

Carpeta

↓

Documento

↓

Folio

---

# Responsabilidades

- Organizar la documentación.
- Mantener la jerarquía archivística.
- Permitir búsquedas.
- Controlar la ubicación física.
- Controlar la ubicación digital.
- Mantener trazabilidad.
- Registrar movimientos.
- Mantener estados documentales.

---

# Invariantes

- Todo expediente pertenece a una única subserie.
- Toda carpeta pertenece a un único expediente.
- Todo documento pertenece a una única carpeta.
- Todo folio pertenece a un único documento.
- Ningún elemento puede pertenecer simultáneamente a dos padres.
- Todo movimiento debe quedar auditado.

---

# Eventos de Dominio

- ArchiveCreated
- SeriesCreated
- SubseriesCreated
- FileCreated
- FolderCreated
- DocumentRegistered
- FolioAdded
- FolderMoved
- BoxAssigned
- BarcodeAssigned

---

# Relaciones

Archive

↓

Project

↓

Warehouse

↓

Loans

↓

Digitization

---

# Pendientes

- Validar metadatos obligatorios definidos por el AGN.
- Validar reglas oficiales de identificación de expedientes.
- Validar reglas de foliación.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Archive.|
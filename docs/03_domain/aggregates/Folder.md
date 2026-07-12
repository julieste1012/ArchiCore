# Folder Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Folder Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Documentary Management |
| Aggregate Root | Folder |

---

# Propósito

Representa la carpeta física que contiene uno o varios documentos pertenecientes a un expediente.

La carpeta constituye la unidad documental física manipulada durante los procesos operativos de MULTISERTEC.

---

# Responsabilidades

- Identificar la carpeta.
- Asociar documentos.
- Asociar expediente.
- Asociar caja.
- Mantener ubicación.
- Registrar movimientos.
- Controlar préstamos.
- Registrar digitalización.

---

# Ciclo de Vida

Created

↓

Received

↓

Organized

↓

Stored

↓

Loaned

↓

Returned

↓

Archived

---

# Invariantes

- Toda carpeta pertenece a un único expediente.
- Toda carpeta pertenece a una única caja a la vez.
- Una carpeta archivada no puede modificarse.
- Todo préstamo debe quedar registrado.
- Todo movimiento físico debe generar trazabilidad.

---

# Eventos de Dominio

- FolderCreated
- FolderAssignedToBox
- FolderMoved
- FolderLoaned
- FolderReturned
- FolderDigitized
- FolderArchived

---

# Relaciones

Folder

↓

Archive

↓

Box

↓

Project

↓

Loans

↓

Documents

---

# Pendientes

- Validar reglas de foliación por carpeta.
- Validar metadatos obligatorios del AGN.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Folder.|
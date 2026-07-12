# Warehouse Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Warehouse Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Warehouse Management |
| Aggregate Root | Warehouse |

---

# Propósito

Representa una bodega física administrada por MULTISERTEC destinada al almacenamiento seguro de documentación, equipos y demás elementos asociados a uno o varios proyectos.

La bodega constituye la ubicación física principal donde se conserva la información custodiada.

---

# Responsabilidades

- Administrar la capacidad física.
- Organizar la estructura interna.
- Registrar movimientos.
- Permitir la ubicación rápida del material.
- Garantizar la trazabilidad física.
- Controlar disponibilidad de espacio.

---

# Estructura Física

Una bodega podrá estar organizada mediante niveles jerárquicos.

Ejemplo:

Bodega

↓

Pasillo

↓

Estantería

↓

Nivel

↓

Posición

↓

Caja

↓

Carpeta

---

# Identidad

Cada bodega posee un código único.

---

# Ciclo de Vida

Available

↓

Operational

↓

Maintenance

↓

Closed

↓

Archived

---

# Invariantes

- Una caja solamente podrá ocupar una ubicación física al mismo tiempo.
- Una ubicación física solamente podrá contener una caja cuando exista capacidad disponible.
- Toda caja deberá pertenecer a una ubicación registrada.
- Todo movimiento físico deberá quedar auditado.
- Ninguna caja podrá quedar sin ubicación.

---

# Eventos de Dominio

- WarehouseCreated
- WarehouseActivated
- WarehouseClosed
- LocationCreated
- LocationOccupied
- LocationReleased
- BoxMoved
- CapacityExceeded

---

# Relaciones

Warehouse

↓

Projects

↓

Locations

↓

Boxes

↓

Inventory

---

# Pendientes

- Definir estructura física estándar utilizada por MULTISERTEC.
- Definir capacidad máxima por estantería.
- Definir reglas de numeración de ubicaciones.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Warehouse.|
# Customer Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Customer Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Customer Management |
| Aggregate Root | Customer |

---

# Propósito

Representa la empresa contratante que adquiere los servicios ofrecidos por MULTISERTEC.

El cliente constituye el nivel más alto de la relación comercial y puede mantener múltiples contratos activos o históricos.

Toda la operación documental ejecutada por MULTISERTEC pertenece, en última instancia, a un cliente.

---

# Responsabilidades

- Registrar la información general del cliente.
- Mantener el estado del cliente.
- Gestionar la información de contacto.
- Mantener la información tributaria.
- Asociar contratos.
- Mantener el historial comercial.

---

# Identidad de Negocio

Cada cliente posee un identificador único e inmutable.

El NIT podrá utilizarse como clave de negocio cuando aplique.

---

# Ciclo de Vida

Prospect

↓

Active

↓

Inactive

↓

Archived

---

# Invariantes

- Todo contrato pertenece a un único cliente.
- Un cliente puede tener múltiples contratos.
- Un cliente archivado no podrá generar nuevos contratos.
- El cliente conserva su historial aun cuando todos sus contratos finalicen.
- El NIT no podrá duplicarse.

---

# Eventos de Dominio

- CustomerCreated
- CustomerUpdated
- CustomerActivated
- CustomerDeactivated
- CustomerArchived

---

# Relaciones

Customer

↓

Contracts

↓

Projects

↓

Billing

---

# Pendientes

- Validar si existen diferentes tipos de clientes.
- Validar información tributaria obligatoria.
- Validar políticas de eliminación lógica.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Customer.|
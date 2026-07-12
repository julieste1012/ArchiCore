# Contract Aggregate

| Campo | Valor |
|--------|-------|
| Documento | Contract Aggregate |
| Versión | 1.0.0 |
| Estado | Draft |
| Contexto | Contract Management |
| Aggregate Root | Contract |

---

# Propósito

El Aggregate Contract representa el acuerdo legal y comercial entre MULTISERTEC y un cliente.

Define los servicios contratados, las obligaciones de las partes, la vigencia, la capacidad contratada y las reglas generales bajo las cuales se ejecutarán uno o varios proyectos.

El contrato no administra la operación diaria.

Su función es servir como marco contractual para los proyectos.

---

# Responsabilidades

- Mantener la información contractual.
- Asociarse a un único cliente.
- Definir la vigencia contractual.
- Definir los servicios contratados.
- Definir las condiciones de facturación.
- Definir la capacidad contratada cuando aplique.
- Permitir la creación de múltiples proyectos.
- Mantener el historial contractual.

---

# Identidad de Negocio

Cada contrato posee un identificador único e inmutable.

---

# Ciclo de Vida

Draft

↓

Pending Approval

↓

Active

↓

Suspended

↓

Expired

↓

Closed

↓

Archived

---

# Invariantes

- Todo contrato pertenece a un único cliente.
- Un contrato puede generar múltiples proyectos.
- Un contrato cerrado no puede crear nuevos proyectos.
- Un contrato archivado no puede modificarse.
- Un proyecto siempre debe pertenecer a un contrato activo.
- La fecha de finalización debe ser posterior a la fecha de inicio.

---

# Eventos de Dominio

- ContractCreated
- ContractActivated
- ContractSuspended
- ContractRenewed
- ContractExpired
- ContractClosed
- ContractArchived
- ProjectCreatedUnderContract

---

# Relaciones

Contract

↓

Customer

↓

Projects

↓

Billing

↓

Warehouse Assignments

---

# Pendientes

- Política de renovación contractual.
- Tipos oficiales de contrato utilizados por MULTISERTEC.
- Reglas específicas para ampliaciones contractuales.

---

# Historial

| Versión | Fecha | Descripción |
|----------|------|-------------|
|1.0.0|2026-07-11|Versión inicial del Aggregate Contract.|
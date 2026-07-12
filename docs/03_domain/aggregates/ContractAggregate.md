# Contract Aggregate

## Aggregate Root

Contract

---

## Responsabilidad

Representa un acuerdo comercial firmado entre un cliente y MULTISERTEC.

Es el punto de inicio de la operación.

Todo proyecto pertenece obligatoriamente a un contrato.

---

# Estado

- Draft
- Active
- Suspended
- Finished
- Cancelled

---

# Atributos

- Id
- ContractNumber
- ClientId
- StartDate
- EndDate
- TotalValue
- Status

---

# Contiene

- Projects

---

# Reglas

Un contrato:

- siempre pertenece a un cliente.

- puede contener múltiples proyectos.

- no puede finalizar antes de su fecha de inicio.

- no puede tener dos proyectos con el mismo código.

- puede cambiar de estado.

- puede cerrarse.

- puede suspenderse.

---

# Eventos

ContractCreated

ContractActivated

ContractSuspended

ContractFinished

ProjectAdded

---

# Métodos del Aggregate

Create()

Activate()

Suspend()

Finish()

AddProject()

RemoveProject()
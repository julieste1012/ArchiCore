# Domain Review

## Objetivo

Validar el modelo de dominio antes de iniciar el diseño de base de datos.

---

# Aggregate Roots

## Customer

Estado:
✅ Válido

---

## Contract

Estado:
✅ Válido

---

## Project

Estado:
✅ Válido

---

## Warehouse

Estado:
✅ Válido

---

## Archive

Estado:
⚠ Requiere refinamiento.

Contendrá entidades internas.

- Series
- Subseries
- Expedient
- Folder
- Document
- Folio
- Box

---

# Entidades

Las siguientes entidades dejan de modelarse como Aggregate Root.

- Folder
- Box
- Document
- Folio

---

# Próxima Fase

Sprint 3

Database Design
# Business Roles

| Campo | Valor |
|--------|-------|
| Documento | 02_BusinessRoles |
| Versión | 1.0.0 |
| Estado | Approved |
| Fase | 1 - Business Analysis |
| Sprint | 1.3 |
| Última actualización | 2026-07-11 |

---

# Purpose

This document defines the official business roles of the DMS ERP.

Business roles represent organizational responsibilities, not software permissions.

System permissions will later be derived from these responsibilities.

---

# Organizational Principle

Permissions are assigned to responsibilities.

Users inherit permissions through their assigned role.

The ERP never assigns permissions directly to individual users except in exceptional administrative situations.

---

# Business Roles

## System Administrator

Responsible for the technical administration of the ERP.

Responsibilities:

- User administration
- Role administration
- System configuration
- Security settings
- Backup supervision

---

## Administrative Manager

Responsible for business administration.

Responsibilities:

- Customer administration
- Contract supervision
- Billing supervision
- Operational indicators

---

## Project Coordinator

Responsible for the operational execution of documentary contracts.

Responsibilities:

- Receive documentary material
- Verify received quantities
- Assign storage locations
- Authorize documentary loans
- Supervise archive personnel
- Monitor contract capacity

---

## Archive Technician

Responsible for documentary organization.

Responsibilities:

- Verify documentary content
- Documentary classification
- Labeling
- Documentary hierarchy validation
- Barcode association

---

## Archive Assistant

Responsible for physical documentary operations.

Responsibilities:

- Receive boxes
- Move documentary material
- Deliver documentary loans
- Receive returned documentation
- Update physical locations

---

## Digitization Operator

Responsible for digitization activities.

Responsibilities:

- Scan documentation
- Associate digital files
- Validate digital quality
- Deliver digital copies

---

## Billing Assistant

Responsible for operational billing support.

Responsibilities:

- Review contractual services
- Generate billing information
- Verify monthly calculations

Accounting remains managed externally in SIIGO.

---

## Customer Representative

External user representing the contracting company.

Possible future responsibilities:

- Request documentary loans
- Request digitization
- Track requests
- Consult documentary inventory

This role is reserved for future customer portal development.

---

# Responsibility Matrix

| Process | Coordinator | Technician | Assistant | Billing |
|----------|------------|------------|-----------|----------|
| Reception | X | | X | |
| Documentary Verification | | X | | |
| Classification | | X | | |
| Labeling | | X | | |
| Storage Assignment | X | | | |
| Physical Storage | | | X | |
| Documentary Loan | X | | X | |
| Billing | | | | X |

---

# Role Evolution

New business roles may be incorporated as the company grows.

Every new role requires business analysis before implementation.

---

# Version History

| Version | Date | Description |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Initial business roles definition. |
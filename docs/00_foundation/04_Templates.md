# Documentation Templates

| Campo | Valor |
|--------|-------|
| Documento | 04_Templates |
| Versión | 1.0.0 |
| Estado | Approved |
| Sprint | Sprint 0 |
| Última actualización | 2026-07-11 |

---

# Purpose

This document defines the official templates used throughout the DMS ERP project.

Templates ensure consistency, readability and maintainability across all project documentation.

The template files are stored in:

docs/09_templates/

---

# Available Templates

| Template | Purpose |
|----------|---------|
| TemplateDocument.md | General project documentation |
| TemplateADR.md | Architecture Decision Records |
| TemplateBusiness.md | Business documentation |
| TemplateArchitecture.md | Architecture documentation |
| TemplateDatabase.md | Database documentation |
| TemplateAPI.md | API documentation |
| TemplateModule.md | Module documentation |
| TemplateDeployment.md | Deployment documentation |

---

# Template Selection

## General Documentation

Use:

TemplateDocument.md

---

## Business

Use:

TemplateBusiness.md

Examples:

- Business Rules
- Business Processes
- Use Cases
- Roles
- Scope

---

## Architecture

Use:

TemplateArchitecture.md

Examples:

- System Architecture
- Security
- Domain Model
- Integrations

---

## Database

Use:

TemplateDatabase.md

Examples:

- ER Diagram
- Tables
- Indexes
- Constraints

---

## APIs

Use:

TemplateAPI.md

Examples:

- REST Endpoints
- Authentication
- Response Models

---

## ADR

Use:

TemplateADR.md

Every architectural decision must have its own ADR.

---

## Modules

Use:

TemplateModule.md

Examples:

- Contracts Module
- Customers Module
- Billing Module
- Document Management Module

---

## Deployment

Use:

TemplateDeployment.md

Examples:

- Docker
- Production
- Backup
- Monitoring

---

# Rules

Every new document must:

- Start from an official template.
- Include metadata.
- Include version history.
- Follow the document standards.
- Follow naming conventions.

---

# Ownership

Templates are maintained by the software architect.

Changes to templates must be documented before adoption.

---

# History

| Version | Date | Description |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Initial template policy. |
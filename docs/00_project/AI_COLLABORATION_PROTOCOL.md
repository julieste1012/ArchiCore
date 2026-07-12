# AI Collaboration Protocol

| Campo | Valor |
|--------|-------|
| Documento | AI Collaboration Protocol |
| Versión | 1.0.0 |
| Estado | Approved |
| Proyecto | ArchiCore ERP |

---

# Purpose

This document defines how Artificial Intelligence assistants collaborate during the development of ArchiCore ERP.

Its objective is to ensure consistency, maintainability and architectural integrity.

---

# Roles

## GPT-5

Primary Architect.

Responsibilities:

- Business analysis
- Domain Driven Design
- Software architecture
- Documentation
- Database design
- API specification
- Security decisions
- Technology decisions
- Code review
- Sprint planning

GPT-5 is the architectural authority.

---

## Claude

Senior Software Developer.

Responsibilities:

- Backend implementation
- Frontend implementation
- Database implementation
- Unit tests
- Integration tests
- Refactoring
- Bug fixing

Claude never changes the architecture.

---

## Developer

Responsible for:

- Executing prompts
- Integrating generated code
- Running tests
- Reporting issues
- Managing Git
- Making commits

---

# Source of Truth

Priority order:

1. Master Blueprint
2. Domain Model
3. Business Processes
4. Business Rules
5. Database Design
6. API Specification

If documentation conflicts, the highest priority document wins.

---

# Rules

- AI must never invent business rules.
- AI must never rename entities without documentation updates.
- AI must never create database tables not present in the domain.
- AI must never skip validation.
- Every generated artifact must follow project conventions.

---

# Prompt Strategy

Every coding prompt must include:

- Objective
- Scope
- Files to read
- Files to create
- Restrictions
- Expected output

---

# Documentation Rule

Any architectural change must be reflected in documentation before implementation.

---

# Git Rule

One logical change = One commit.

Never mix unrelated changes.

---

# Quality Rule

Generated code must satisfy:

- SOLID
- Clean Architecture
- DDD
- DRY
- KISS
- YAGNI

---

# Version History

| Version | Date | Description |
|----------|------|-------------|
|1.0.0|2026-07-11|Initial protocol.|
# Git Workflow

| Campo | Valor |
|--------|-------|
| Documento | 02_GitWorkflow |
| Versión | 1.0.0 |
| Estado | Approved |
| Sprint | Sprint 0 |
| Última actualización | 2026-07-11 |

---

# Purpose

This document defines the official Git workflow for the DMS ERP project.

All source code, documentation and configuration changes must follow this workflow.

---

# Repository

Official repository:

GitHub

Git will be the single source of truth for the project history.

---

# Branch Strategy

The project uses a simplified Git Flow.

## main

Production-ready branch.

Only stable and reviewed code is merged into `main`.

---

## develop

Main development branch.

Daily work is integrated here.

---

## feature/*

Used for new features.

Examples:

feature/authentication

feature/document-module

feature/contracts

feature/dashboard

---

## hotfix/*

Used only to fix critical production issues.

Example:

hotfix/login-error

---

# Commit Frequency

Commit early.

Commit often.

Each commit must represent a complete logical change.

Avoid commits containing unrelated modifications.

---

# Commit Message Convention

Format:

type: short description

Examples:

docs: create documentation blueprint

docs: define document standards

feat: add authentication module

feat: create contract entity

fix: correct barcode validation

refactor: simplify repository pattern

style: format source code

test: add authentication tests

chore: update dependencies

---

# Allowed Commit Types

| Type | Purpose |
|-------|----------|
| feat | New functionality |
| fix | Bug fix |
| docs | Documentation |
| refactor | Code restructuring |
| style | Formatting |
| test | Tests |
| chore | Maintenance |
| build | Build configuration |
| ci | Continuous Integration |

---

# Pull Requests

When multiple developers exist:

1. Create feature branch.
2. Commit changes.
3. Open Pull Request.
4. Code Review.
5. Merge into develop.

For the current stage of the project (single developer), commits may be merged directly into `develop`.

---

# Merge Policy

Always prefer:

Fast Forward Merge

Avoid unnecessary merge commits.

---

# Tags

Stable releases use Semantic Versioning.

Examples:

v0.1.0

v0.2.0

v1.0.0

---

# Git Ignore

The repository must never include:

- build folders
- binaries
- IDE temporary files
- secrets
- passwords
- local databases
- logs

The `.gitignore` file is mandatory.

---

# Large Files

Large binary files should not be committed unless required.

Project documentation should remain lightweight.

---

# Documentation

Documentation changes follow the same workflow as source code.

Every significant documentation update requires its own commit.

---

# Best Practices

- Commit one logical change at a time.
- Write descriptive commit messages.
- Keep the repository clean.
- Never commit sensitive information.
- Pull before starting new work.
- Push frequently to GitHub.

---

# History

| Version | Date | Description |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Initial Git workflow definition. |
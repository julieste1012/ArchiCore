# Naming Conventions

| Campo | Valor |
|--------|-------|
| Documento | 03_NamingConventions |
| Versión | 1.0.0 |
| Estado | Approved |
| Sprint | Sprint 0 |
| Última actualización | 2026-07-11 |

---

# Purpose

This document defines the official naming conventions for the DMS ERP project.

Every artifact in the project must follow these conventions to ensure consistency, readability and maintainability.

---

# General Rules

- Use English for all technical names.
- Use meaningful names.
- Avoid abbreviations unless they are industry standards.
- Prefer complete words over shortened forms.
- Use singular names for entities.
- Business terminology from Colombian archival regulations may remain in Spanish when required.

---

# Repository

Repository names use:

Pascal-Case

Example:

DMS-ERP

---

# Folders

Use:

kebab-case

Examples:

document-management

authentication

shared

file-storage

---

# Files

Use:

PascalCase

Examples:

BusinessRules.md

ContractService.cs

CreateContractCommand.cs

DocumentController.cs

Dashboard.tsx

---

# C# Classes

Use:

PascalCase

Examples:

Document

Contract

Customer

Loan

InvoiceService

AuthenticationService

---

# Interfaces

Use prefix I

Examples:

IDocumentRepository

IContractService

IFileStorage

---

# Methods

Use:

PascalCase

Examples:

CreateContract()

GetCustomer()

GenerateInvoice()

UploadDocument()

---

# Properties

Use:

PascalCase

Examples:

FirstName

LastName

CreatedAt

ContractNumber

---

# Private Fields

Use:

_camelCase

Examples:

_database

_logger

_currentUser

---

# Local Variables

Use:

camelCase

Examples:

customer

document

contract

totalFolders

---

# Constants

Use:

PascalCase

Examples:

MaxUploadSize

DefaultPageSize

---

# Enums

Use:

PascalCase

Examples:

ContractStatus

DocumentType

LoanStatus

InvoiceStatus

---

# Enum Values

Use:

PascalCase

Examples:

Active

Pending

Returned

Cancelled

Archived

---

# Database

## Tables

Use:

snake_case

Examples:

customers

contracts

folders

documents

document_loans

contract_services

---

## Columns

Use:

snake_case

Examples:

created_at

updated_at

customer_id

contract_number

barcode

---

## Primary Keys

Always:

id

Example:

customers.id

---

## Foreign Keys

Format:

entity_id

Examples:

customer_id

contract_id

folder_id

warehouse_id

---

# API Routes

Use:

kebab-case

Examples:

/api/contracts

/api/document-loans

/api/document-types

---

# JSON

Use:

camelCase

Example:

{
  "customerId": 5,
  "contractNumber": "CT-001",
  "createdAt": "2026-07-11"
}

---

# React Components

Use:

PascalCase

Examples:

Sidebar

DashboardLayout

DocumentTable

ContractForm

---

# React Hooks

Use:

useSomething

Examples:

useAuth

useDocuments

useContracts

---

# CSS Classes

Use:

kebab-case

Examples:

main-layout

sidebar-item

document-card

contract-table

---

# Git Branches

Use:

kebab-case

Examples:

feature/document-module

feature/authentication

feature/invoice-module

hotfix/login-error

---

# Docker

Container names:

kebab-case

Examples:

backend-api

postgres-db

minio-storage

frontend-app

---

# Environment Variables

Use:

UPPER_SNAKE_CASE

Examples:

DATABASE_URL

JWT_SECRET

MINIO_ENDPOINT

POSTGRES_PASSWORD

---

# Migration Names

Use:

PascalCase

Examples:

CreateContractsTable

AddBarcodeToFolder

CreateInvoices

---

# Documentation Files

Use:

PascalCase

Examples:

BusinessProcesses.md

SystemArchitecture.md

ApiDesign.md

DeploymentGuide.md

---

# Forbidden

Do not use:

- Spaces
- Accented characters
- Random abbreviations
- Names like:
  - temp
  - final
  - new
  - test1
  - prueba
  - archivo_final_v2

---

# History

| Version | Date | Description |
|----------|------|-------------|
| 1.0.0 | 2026-07-11 | Initial naming conventions. |
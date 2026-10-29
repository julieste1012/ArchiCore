# Local Development Guide

This guide is for the active feature branch `feat/backend-core-api`. Keep local credentials out of Git.

## Prerequisites

- .NET 10 SDK
- Node.js 22.12+ and npm
- Docker Desktop running
- Git

## 1. Synchronize the branch

From PowerShell:

```powershell
Set-Location C:\Proyectos\ArchiCore
git status --short
git fetch origin
git pull --ff-only origin feat/backend-core-api
```

If `git status --short` shows local work before pulling, do not discard it. Reconcile it first.

## 2. Start PostgreSQL

The default object-storage provider is a local filesystem rooted at `src/Backend/ArchiCore.API/App_Data/Objects` during development. The directory is git-ignored. This keeps local work independent from an object-store server while the document metadata/lifecycle requirements are being finalized.


At the repository root:

```powershell
docker compose up -d postgres
docker compose ps
```

The development database is published at `127.0.0.1:5433`, database `archicore`, user `archicore`. The password in Compose is a local-only development value, not a production secret. The default connection in `appsettings.Development.json` and the EF design-time factory matches this port.

## 3. Configure the API secrets for this PowerShell session

Create a random signing key and configure the initial administrator. Keep the values in your local terminal/user-secrets, never in tracked files:

```powershell
$env:ConnectionStrings__ArchiCoreDb = "Host=127.0.0.1;Port=5433;Database=archicore;Username=archicore;Password=archicore_dev"
$env:Jwt__Issuer = "ArchiCore"
$env:Jwt__Audience = "ArchiCore.Client"
$env:Jwt__SigningKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
$env:BootstrapAdmin__Email = "admin@archicore.local"
$env:BootstrapAdmin__DisplayName = "Administrador ArchiCore"
$env:BootstrapAdmin__Password = Read-Host "Enter a strong local admin password (12+ chars, uppercase, lowercase, number and symbol)"
```

This example prompts for a password. Use a strong local value and do not commit it. If the key is regenerated on each restart, previously issued access tokens become invalid; for a stable local setup, save the signing key in .NET user-secrets and reuse it.

### Optional: configure an S3-compatible object-storage endpoint

The backend supports the MinIO .NET SDK adapter for MinIO/S3-compatible endpoints. Set these only when testing that adapter:

```powershell
$env:ObjectStorage__Provider = "Minio"
$env:ObjectStorage__Endpoint = "127.0.0.1:9000"
$env:ObjectStorage__AccessKey = "your-local-access-key"
$env:ObjectStorage__SecretKey = "your-local-secret-key"
$env:ObjectStorage__Bucket = "archicore-documents"
$env:ObjectStorage__Secure = "false"
$env:ObjectStorage__MaxObjectSizeBytes = "104857600"
```

Use credentials from a supported local object-store installation, never copy production credentials into the repository. The upstream MinIO community server repository was archived on April 25, 2026; its README states the server is no longer maintained. Do not use the archived community server as a production dependency. The app adapter is provider-configurable, but the exact production service must be selected and validated before release.

## 4. Apply migrations and start the API

Keep the variables above in the same PowerShell window:

```powershell
dotnet tool restore

dotnet tool run dotnet-ef database update --project .\src\Backend\ArchiCore.Infrastructure\ArchiCore.Infrastructure.csproj --startup-project .\src\Backend\ArchiCore.API\ArchiCore.API.csproj --connection $env:ConnectionStrings__ArchiCoreDb

dotnet run --project .\src\Backend\ArchiCore.API\ArchiCore.API.csproj --launch-profile https
```

On the first API startup, the configured bootstrap administrator is created if the email does not already exist. The seeder does not overwrite an existing account. The HTTPS API URL for the configured launch profile is `https://localhost:7197`.

Useful checks:

- `https://localhost:7197/health/live`
- `https://localhost:7197/health/ready`

## 5. Start the frontend in a second PowerShell window

```powershell
Set-Location C:\Proyectos\ArchiCore\src\Frontend\archicore-web
npm install
npm run dev
```

Open http://127.0.0.1:5173 and log in using the bootstrap administrator email and the local password you supplied above. Vite proxies `/api` and `/health` to the API on port 7197; no backend signing key is used in frontend code.

## 6. Run tests

From repository root:

```powershell
dotnet restore .\src\ArchiCore.slnx
dotnet build .\src\ArchiCore.slnx --no-restore
dotnet test .\src\ArchiCore.slnx --no-build
```

Docker Desktop must remain available for Testcontainers integration tests, which create a disposable PostgreSQL test database. They do not use the Compose database volume.

From the frontend directory:

```powershell
npm run typecheck
npm run build
```

## Current scope limits

The frontend currently includes login, dashboard and initial list/create screens for Customer, Contract, Project and Warehouse. The backend currently restricts operational controllers and the audit endpoint to SystemAdministrator until the endpoint permission matrix is approved. Documentary modules, file metadata/API workflows and actor-level security audit are not complete yet. Object storage is an infrastructure capability only; no upload/download API exposes it until document permissions and lifecycle are approved.

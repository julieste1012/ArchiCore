# ADR-003: Standardize on .NET 10 LTS

| Field | Value |
|---|---|
| Status | Accepted |
| Date | 2026-10-26 |
| Decision owner | Architecture lead |
| Scope | Backend target framework, ASP.NET Core, EF Core and SDK baseline |

## Context

The solution's five existing backend projects target `net10.0`. The developer environment uses SDK `10.0.403`, and the GitHub Actions workflow restores, builds and tests with .NET 10. However, the Master Blueprint still listed ASP.NET Core 9, creating a documentation/runtime mismatch.

At the time of this decision, Microsoft's official support policy lists .NET 10 as an active LTS release through November 14, 2028, while .NET 9 is in maintenance and reaches end of support on November 10, 2026. See the [official .NET support policy](https://dotnet.microsoft.com/es-es/platform/support/policy).

## Decision

- Standardize ArchiCore's backend on **.NET 10 LTS**, target framework `net10.0`, and ASP.NET Core 10.
- Keep the five projects, test project, and CI workflow aligned to .NET 10.
- Use compatible 10.0.x releases of Microsoft-owned ASP.NET Core and Entity Framework Core packages, applying supported servicing updates rather than pinning old patches.
- For third-party providers such as Npgsql, choose a stable package version explicitly compatible with EF Core 10.
- Do not retarget the project to .NET 9 just to match the stale blueprint.
- Keep the test runner configuration in `global.json` that opts xUnit v3 into Microsoft Testing Platform for the .NET 10 SDK.

## Consequences

- Documentation matches the implemented target framework.
- Package versions and CI should be updated together when applying servicing releases.
- This decision does not resolve domain-model conflicts or authorize new business rules; those remain governed by the domain documents and separate decisions.

## Verification

- Backend project files and the solution target `net10.0`.
- GitHub Actions builds and executes the domain test suite on .NET 10.

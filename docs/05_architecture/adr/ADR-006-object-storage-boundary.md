# ADR-006: Provider-Neutral Object Storage Boundary

| Field | Value |
|---|---|
| Status | Accepted for the backend foundation |
| Date | 2026-10-29 |
| Scope | Storage of binary object content, separate from document metadata |

## Context

ArchiCore must store digitized files and other binary content without treating a disk path or object-store key as documentary metadata. The domain documents do not yet approve Document/Folio metadata, the complete file lifecycle, access rules, retention or deletion policies. The original architecture mentioned MinIO, but the upstream MinIO server repository was archived on 2026-04-25 and states that the community server is no longer maintained: https://github.com/minio/minio.

The MinIO .NET client package is available as an S3-compatible client, but choosing a server implementation for production is a separate decision: https://www.nuget.org/packages/Minio/.

## Decision

- Define an application-level `IObjectStorage` interface for storing bytes under an opaque key, streaming an object to a destination and deleting an object.
- Enforce the same storage-key policy for all providers: no root/empty paths, traversal segments, backslash, query/hash or control characters; a maximum 1024-byte UTF-8 key length.
- Provide a development-only filesystem adapter under a configured root directory, with path containment checks, temporary-file writes, upload-size limits and verification that received byte count matches the declared length.
- Provide an S3-compatible adapter using the MinIO .NET SDK; it is configured through environment variables and can target a compatible endpoint. The app does not expose any public upload/download endpoint yet.
- Default development to the filesystem provider. Do not automatically add the archived MinIO community server to Docker Compose or treat it as a production dependency.
- Before production, select a supported object-store service, review license/security/support terms, configure non-default credentials and bucket policy, and approve document metadata, authorization, retention and deletion requirements.

## Consequences

- Application/use-case code can depend on `IObjectStorage` without embedding a vendor SDK or a filesystem path.
- Development can continue without a separate object-store container.
- The S3-compatible adapter still needs an end-to-end test against the chosen supported provider.
- Binary storage alone is not the document-management module. No metadata, document ownership, archival hierarchy or public file API is inferred by this decision.

## Verification

Local filesystem tests cover storage round-trip, deletion, object-key traversal rejection, size limits and declared byte-length verification. Full provider integration tests and the document API are deferred until the production provider and document contracts are approved.

# Verification

Verified on 2026-09-25 using the installed .NET SDK 10.0.200 and local Microsoft SQL Server 2025 Developer edition.

- Solution build: succeeded with zero warnings and zero errors.
- Release HTTP integration tests with SQLite: 22 passed, 0 failed; 2 SQL-only migration tests skipped.
- Release HTTP integration tests with MSSQL and all five migrations: 24 passed, 0 failed.
- Release API publish: succeeded; output is in `artifacts/publish` (ignored by Git).
- EF migration/model consistency: no pending model changes.

The API now uses FastEndpoints 8.3.0 and FluentValidation 12.1.1. Regression coverage includes all twenty-one operations, ownership checks, malformed JSON, unknown fields, validation on create/update, pagination, Swagger request schemas and UI, the development root redirect, and private user-profile access. Routes use /user, /dictionary with nested /word paths, and the shared /language catalog. Language tests verify shared CRUD access, Unicode names, optional flag icons, authentication, and field validation.

The MSSQL tests created and removed isolated test databases. They did not initialize a persistent application database. Follow README setup to configure your private JWT key and apply migrations to the application database.

The optional Docker Compose database was not started because the Docker daemon was unavailable; SQL Server verification used the running native SQL Server instance.



Dictionary ordering is persisted per owner through PUT /dictionary/order. SQL Server verification includes downgrading an isolated test database to AddLanguages and upgrading populated tables, checking that the backfill preserves rows and the previous per-user ordering.



The Word rename is verified with populated SQL Server tables through downgrade and upgrade. Existing IDs, dictionary associations, translations, transcriptions, and cascade deletion survive the rename. Current code, routes, and documentation use Word; historical migrations retain their original identifiers.


Roles: Administrator read access across users and dictionaries is verified, along with owner-only mutations, blocked registration role injection, live database role changes, and User defaults for upgraded accounts. No account was promoted in the application database.


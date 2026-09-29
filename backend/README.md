# Vocabularity

A runnable C# 14 / .NET 10 LTS ASP.NET Core Web API backed by Microsoft SQL Server. ASP.NET Core is the C# API framework equivalent for this project; FastAPI is a Python framework and is not used.

.NET 10 is supported through November 2028 ([Microsoft support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)). Microsoft packages and local EF tooling are pinned to 10.0.12. Use the latest .NET 10 SDK and runtime servicing release.

## Structure

- `Vocabularity.Api`: FastEndpoints, FluentValidation, authentication, Swagger, centralized errors.
- `Vocabularity.Core`: shared `Entity` base and application exceptions.
- `Vocabularity.Service`: entities, request/response DTOs, use cases, persistence/security interfaces.
- `Vocabularity.Infrastructure`: EF Core SQL Server persistence, migrations, PasswordHasher, JWT issuance.
- `Vocabularity.Tests`: HTTP integration tests, SQLite by default and optional real MSSQL.

The four application projects compile into **one deployed API monolith**. The API is the composition root. Service depends on Core; Infrastructure implements Service interfaces. EF query abstractions are deliberately shared with Service to keep the application small.

## Finding your way around the code

- Start at `src/Vocabularity.Api/Program.cs` for the application pipeline.
- `Api/Endpoints/Auth`, `Api/Endpoints/Dictionaries`, `Api/Endpoints/Words`, and `Api/Endpoints/Languages` each contain one endpoint class per operation.
- Validators live next to their endpoints. They inherit `FastEndpoints.Validator<TRequest>` and use FluentValidation rules; FastEndpoints discovers and runs them automatically before the handler. DataAnnotations validation has been removed.
- `Api/Configuration` contains separate files for dependency registration, JWT authentication, Swagger, and Newtonsoft.Json serialization.
- `Service/*/Models` contains one request or response DTO per file. Business logic stays in `AuthService` and `DictionaryService`.
- `Infrastructure/Database/Configurations` contains one EF mapping per entity.

For example, follow `Endpoints/Dictionaries/CreateEndpoint.cs` → `DictionaryValidator.cs` → `Service/Dictionary/DictionaryService.cs` to see a dictionary request validated and saved. All twenty-one API operations use FastEndpoints; there are no MVC controllers.

FastEndpoints 8.3.0 and FluentValidation 12.1.1 are pinned. The Newtonsoft.Json request/response adapter preserves the existing snake_case contract and rejects unknown fields. Validation failures return HTTP 400 with a readable validation message. See [FastEndpoints validation documentation](https://fast-endpoints.com/docs/validation) for the validator integration.

## Prerequisites

- .NET 10 SDK (10.0.200 or later; `global.json` rolls forward within .NET 10).
- SQL Server 2022 or newer, locally or in Docker. Developer edition is for development/testing.
- Optional Docker Desktop with Linux containers and Docker Compose.

Run all commands from the `backend` directory (the folder containing `Vocabularity.sln`). PowerShell examples follow.

## Start the database

**Option A: existing Windows SQL Server**

The default connection uses your Windows identity:

```text
Server=localhost;Database=Vocabularity;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
```

Your identity needs permission to create the development database/apply migrations.

**Option B: Docker MSSQL with SQL authentication**

Copy `.env.example` to `.env`, then replace its password with a strong unique value.

```powershell
Copy-Item .env.example .env
# Edit .env before starting the container.
docker compose up -d --wait
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=Vocabularity;User Id=sa;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True;"
```

Use the same password as `.env`. The database volume survives `docker compose down`.
Port 1433 must be available; if your installed SQL Server uses it, change the Compose host port and connection string together.

Connection strings with `TrustServerCertificate=True` are development examples. Production should use a trusted server certificate (`TrustServerCertificate=False`) and a dedicated least-privilege SQL login. Never commit real credentials.

## Configure, migrate, and run

Generate a private signing key and store it outside source control:

```powershell
dotnet restore
dotnet tool restore
$key = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "Jwt:Key" "$key" --project src/Vocabularity.Api
dotnet ef database update --project src/Vocabularity.Infrastructure --startup-project src/Vocabularity.Api
dotnet run --project src/Vocabularity.Api
```

Open [Swagger](http://localhost:5080/swagger). The root URL redirects there in Development, and the Visual Studio launch profile opens Swagger automatically. Launch settings select Development and port 5080. No signing key is committed; startup rejects missing/short keys. You can instead supply `Jwt__Key` via environment variable.

The design-time EF factory reads `ConnectionStrings__DefaultConnection`, falling back to the Windows connection above. For SQL authentication, keep that environment variable set for **both** EF commands and API startup. EF tooling does not need a JWT key.

The initial migration is included. To create a future schema change:

```powershell
dotnet ef migrations add DescribeYourChange --project src/Vocabularity.Infrastructure --startup-project src/Vocabularity.Api --output-dir Database/Migrations
dotnet ef migrations script --idempotent --project src/Vocabularity.Infrastructure --startup-project src/Vocabularity.Api --output schema.sql
```

Migrations run explicitly, never automatically during normal API startup. Review migration scripts before production deployment.

## API contract

The nested vocabulary resource is named **Word** throughout the current API and code. Use
`/dictionary/{id}/word` and `/dictionary/{id}/word/{wordId}`; the former routes are no longer available.
The database table is `Words`. The rename migration preserves existing IDs, dictionary links,
translations, transcriptions, timestamps, and cascade deletion. Historical migrations retain their
original names to support upgrading existing databases.

After pulling this change, stop the API, apply the migration, then restart:

```powershell
dotnet ef database update --project src/Vocabularity.Infrastructure --startup-project src/Vocabularity.Api --configuration Release
```

All JSON model properties use Newtonsoft.Json and explicit snake_case attributes. User IDs are UUID strings. Password hashes and normalized email are internal and never returned. The entity base provides `id`, UTC timestamps, and `is_active`; public DTOs expose only relevant fields.

`language_id` is a language tag such as `en`, `uk`, or `en-GB`, not a foreign key into an unspecified language catalog. Dictionary names are limited to 200 characters; original words/transcriptions to 500; translations to 2,000. Registration passwords are 12–128 characters; optional icons are URL strings up to 2,048 characters. Email uniqueness is enforced on a trimmed, invariant-uppercase normalized value.

| Method | Route | Result |
| --- | --- | --- |
| POST | /user/register | 201 with JWT and user |
| POST | /user/login | 200 with JWT and user |
| GET | /users | Administrator-only paginated user list |
| GET | /user/{id} | Own profile; administrators can view any user |
| GET | /user/{id}/dictionaries | Own dictionaries; administrators can view any user's dictionaries |
| GET | /dictionaries | Own dictionaries for User; all dictionaries for Administrator |
| POST | /dictionary | 201 with operation result and Location |
| GET / PUT / DELETE | /dictionary/{id} | Read / replace / delete |
| PUT | /dictionary/order | Save the signed-in user’s complete dictionary order |
| GET | /dictionary/{id}/words | List words (paginated) |
| POST | /dictionary/{id}/word | Create a word |
| GET / PUT / DELETE | /dictionary/{id}/word/{wordId} | Read / replace / delete word |
| GET | /languages | List shared languages (paginated) |
| POST | /language | Create a shared language |
| GET / PUT / DELETE | /language/{id} | Read / replace / delete shared language |

Languages are a shared catalog: any authenticated user can read and manage them. There is no per-user ownership for languages. Both names are required and limited to 100 characters; `name` is the English label and `original_name` is the native label. The API validates presence and length; choosing the correct English translation is the caller's responsibility. The optional `icon` holds a flag emoji or image URL (up to 2,048 characters). Omitting it on PUT clears it. Language lists use the shared pagination wrapper.

Example body for `POST /language`:

```json
{
  "name": "Ukrainian",
  "original_name": "Українська",
  "icon": "🇺🇦"
}
```

The `AddLanguages` migration adds the `Languages` table. Apply it to an existing local database with:

```powershell
dotnet ef database update --project src/Vocabularity.Infrastructure --startup-project src/Vocabularity.Api --configuration Release
```

Language IDs are generated UUID strings. This catalog is currently independent of the existing dictionary `language_id` language tags (`en`, `uk`, etc.); this migration does not change existing dictionaries.

Lists use optional pageNumber (default 1) and pageSize (default 12, maximum 100) query parameters, with no request body. PUT replaces editable fields; omitting an optional transcription clears it. DELETE returns 204; deleting a dictionary cascades to its words.

### Saved dictionary order

Dictionary responses include a zero-based `position`. `GET /dictionaries` sorts by this position. New dictionaries append to the end of their owner's list. Deleting a dictionary can leave gaps in positions; the remaining relative order stays unchanged.

After a drag-and-drop action, send the complete desired order:

```http
PUT /dictionary/order
Authorization: Bearer <token>
Content-Type: application/json

{
  "dictionary_ids": ["third-dictionary-id", "first-dictionary-id", "second-dictionary-id"]
}
```

Success returns 200 with an operation result. Send every current dictionary ID exactly once, including dictionaries outside the currently displayed page. Duplicate IDs, missing JSON fields, and blank IDs return 400. An incomplete, stale, unknown, or foreign set of IDs returns 409 without saving any changes. Refresh the full list before retrying. An empty array is valid only when the user has no dictionaries. Ownership comes from JWT; ordinary create/update requests cannot set `position`.

Ordering and creation use serializable transactions. Reorders are saved atomically, with later successful requests taking precedence. Concurrent database deadlocks return 409 so the client can refresh and retry.

Apply the `AddDictionaryPosition` migration using the existing database-update command. It preserves existing dictionaries and assigns positions independently per user using the previous CreatedAt/Id order. No application database migration runs automatically.

Dictionary and word writes verify ownership using the validated JWT `sub`. Read operations permit the owner or an Administrator. Request DTOs have no `user_id` or `dictionary_id`; unknown JSON properties return 400. Words are also scoped to the dictionary in the URL. Missing resources and reads disallowed by ownership both return 404. JWT signatures, issuer, audience, expiration, algorithm, and active user are validated.

### User roles

The roles are `User` and `Administrator`. All registrations and all existing accounts upgraded by the `AddUserRoles` migration default to `User`. Auth/profile responses include `role`; signed JWTs include a role claim. The database role is reloaded during token validation, so promotion and demotion affect existing sessions immediately.

Users can read only their own profile, dictionaries, and words. Administrators can list all users and read any user's profile, dictionaries, and words. Administrator access to other people's data is read-only: updating, deleting, adding words, and ordering still require ownership. An ordinary user requesting `GET /users` receives 403.

List endpoints accept optional pageNumber and pageSize query parameters, with no request body. An administrator can use `GET /user/{their-own-id}/dictionaries` when assembling their own dictionary reorder request.

To set up an administrator:

1. Apply migrations and register the account normally.
2. Open `scripts/promote-administrator.sql` in SQL Server Management Studio against the **Vocabularity** database.
3. Replace its email placeholder with that account's email and execute it using your trusted database operator account.

There is no public endpoint for setting roles, and registration rejects a supplied `role` property. No existing account is automatically promoted.

Errors use Problem Details: 400 validation, 401 authentication, 404 unavailable resource, 409 duplicate email/concurrent change, 429 authentication rate limit, and a generic 500 with server-side logging. Auth requests are limited to 20 per minute per remote IP per process. Tokens expire after 60 minutes by default; login issues a new token. Refresh tokens, email verification, password reset, and immediate logout revocation are outside this basic-auth scope.

## Try it

In Swagger, register or log in, copy `access_token`, select **Authorize**, and paste the token without the `Bearer` prefix. Swagger adds the header.

PowerShell end-to-end example:

```powershell
$base = "http://localhost:5080"
$auth = Invoke-RestMethod "$base/user/register" -Method Post -ContentType "application/json" -Body (@{
    email = "alice@example.com"
    password = "A-unique-long-password-123!"
    icon = "https://example.com/avatar.png"
} | ConvertTo-Json)
# On subsequent runs use /user/login with email and password only.
$headers = @{ Authorization = "Bearer $($auth.access_token)" }

$dictionary = Invoke-RestMethod "$base/dictionary" -Method Post -Headers $headers -ContentType "application/json" -Body (@{
    dictionary_name = "My English"
    language_id = "en"
} | ConvertTo-Json)

$wordsUrl = "$base/dictionary/$($dictionary.id)/word"
$word = Invoke-RestMethod $wordsUrl -Method Post -Headers $headers -ContentType "application/json; charset=utf-8" -Body ([Text.Encoding]::UTF8.GetBytes((@{
    original_word = "cat"
    original_transcriptioned_word = "kæt"
    translated_word = "кіт"
} | ConvertTo-Json)))
Invoke-RestMethod $wordsUrl -Headers $headers
Invoke-RestMethod "$wordsUrl/$($word.id)" -Method Put -Headers $headers -ContentType "application/json" -Body (@{
    original_word = "cat"
    translated_word = "a small domestic animal"
} | ConvertTo-Json)
Invoke-RestMethod "$wordsUrl/$($word.id)" -Method Delete -Headers $headers
Invoke-RestMethod "$base/dictionary/$($dictionary.id)" -Method Delete -Headers $headers
```

## Build and tests

```powershell
dotnet build Vocabularity.sln --configuration Release
dotnet test Vocabularity.sln --configuration Release
```

The default tests use an isolated in-memory SQLite relational database; the application itself always uses MSSQL. To exercise the actual SQL Server provider and included migrations:

```powershell
$env:VOCABULARITY_TEST_SQLSERVER = "Server=localhost;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
dotnet test Vocabularity.sln --configuration Release
Remove-Item Env:VOCABULARITY_TEST_SQLSERVER
```

SQL tests require create/drop database privileges. Each test creates a uniquely named `VocabularityTests_<guid>` database and deletes only that database on completion. They do not reuse the database named in the supplied connection string.

Tests cover registration/login, password hashes, normalized-email uniqueness, full CRUD, cascade deletion, cross-user denial on all resource operations, wrong-parent word access, validation, forged ownership, invalid tokens, and Swagger serialization.

## Deployment

```powershell
dotnet publish src/Vocabularity.Api --configuration Release --output artifacts/publish
```

Deploy that single output with the .NET 10 ASP.NET Core runtime. Set `ASPNETCORE_ENVIRONMENT=Production`, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, and `ConnectionStrings__DefaultConnection` through your hosting secret/configuration system. Configure HTTPS at the host and application (and trusted forwarded headers when using a reverse proxy). Production enables HTTPS redirection/HSTS and disables Swagger. Apply reviewed migrations using a separate deployment identity. Local user-secrets are a development convenience, not an encrypted production secret store.







## User activity history

Successful EF saves automatically write to `UserActivities` in the same transaction
as the domain change. No frontend logging request is needed. Each row records the
JWT user ID, function (for example `create_dictionary`, `update_dictionary`,
`delete_dictionary`, `reorder_dictionary`), entity ID, and UTC `date`.
User registration and tracked changes to users, languages, and words are also recorded.
Dictionary cascade deletion is represented by its dictionary deletion event; words
removed by SQL cascade do not each generate an additional event. Login/read requests,
failed requests, direct SQL changes, and frontend-only changes are not activity changes.
No passwords, tokens, or full entity/request snapshots are stored.

Read your own history (including for administrators):

```http
GET /user/activities
Authorization: Bearer YOUR_TOKEN
```

Results are paginated and newest first. pageNumber defaults to 1 and pageSize to 12. No request body is required.
There is no public endpoint to insert, edit, or delete history. The deleted entity ID
remains in the history, so deleting a dictionary does not erase its events.

Apply the new table from the backend directory before running the API:

```powershell
dotnet ef database update --project src/Vocabularity.Infrastructure --startup-project src/Vocabularity.Api
```

## Local demo mode

Development enables `Demo:Enabled` in `appsettings.Development.json`. Starting the
API applies migrations and restores missing demo fixtures. Production ignores this
flag. To disable the demo in Development, set `Demo__Enabled=false`.

The demo contains an Administrator account, English/Spanish/French languages with
flags, one essentials dictionary per language, and three words per dictionary
(with transcription and Ukrainian translation). Demo language IDs are en/es/fr.

```http
POST /user/login
Content-Type: application/json

{ "username": "admin", "password": "admin" }
```

The account email is admin@vocabularity.test; email login also works. Its password
is stored using PasswordHasher. Existing passwords and fixture edits are preserved.
Missing demo fixtures are recreated on startup/migration; unrelated data is not reset.

To migrate and seed from the backend folder (including when no migration is pending):

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project src/Vocabularity.Infrastructure --startup-project src/Vocabularity.Api --configuration Release
```

EF Core seeding callbacks run after migration; seed data is deliberately not embedded
in a production migration. Development-only anonymous GET access is enabled for
users (safe response fields), languages, dictionaries, and nested words, including
single-item routes. Anonymous readers can see all users' dictionaries in this mode.
Writes and activity history still require JWT authentication. Outside demo mode,
normal user ownership and administrator rules apply. The frontend must call these
API endpoints to display database fixtures; its current in-memory state is separate.

## Pagination and errors

Every collection GET returns `{ "pageNumber": 1, "pageSize": 12, "data": [] }`.
Example: `GET /dictionaries?pageNumber=2&pageSize=12`. The parameters are optional,
positive integers; pageSize is capped at 100. Beyond the last page, data is empty.
Paging is applied to database queries after authorization and stable ordering.

Read/delete/auth errors use `{ "ErrorMessage": "..." }`, retaining HTTP 400/401/403/404/409/429/500
status codes. Validation errors include field names. Missing or inaccessible
resources share a generic not-found/access message to avoid revealing private IDs.
Development demo anonymous reads remain enabled. Writes and activity history still
require authentication; production access rules are unchanged. No migration is needed.
### Create and update results

Dictionary, language, and word POST/PUT endpoints (including dictionary ordering) return exactly `{"isSuccessfull":true,"message":"Dictionary created successfully."}` on success, and `{"isSuccessfull":false,"message":"..."}` on failure. The spelling `isSuccessfull` is part of the API contract. Creation retains HTTP 201 and a Location header pointing to the created resource; updates return HTTP 200. Errors retain their appropriate 4xx/5xx status, including validation, malformed JSON, and permission errors. Fetch the resource with GET when its fields are needed. Registration and login retain their JWT responses. No database migration is needed.

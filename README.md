# Vocabularity

This workspace separates the backend and future frontend:

```text
backend/
  Vocabularity.sln
  src/
  tests/
  compose.yaml
  README.md
frontend/
  README.md
```

Open `backend/Vocabularity.sln` in Visual Studio for the existing ASP.NET Core API.
The frontend contains the React application shell with Library and Settings navigation.

Run it from `frontend` with `npm install` followed by `npm start`.
See [frontend setup](frontend/README.md).

## Run the backend

From this workspace folder:

```powershell
cd backend
dotnet tool restore
dotnet run --project src/Vocabularity.Api
```

See [backend setup and migrations](backend/README.md) for database configuration,
Swagger, API routes, and tests. Run its commands from the `backend` directory.

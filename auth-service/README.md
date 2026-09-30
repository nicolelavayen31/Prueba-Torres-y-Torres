# AuthService

The service is split into five .NET projects under `src`:

- `AuthService.Api` exposes the HTTP endpoints.
- `AuthService.Application` coordinates registration, sign-in, and session rotation through ports.
- `AuthService.Domain` owns account rules and the email/password policies.
- `AuthService.Infrastructure` provides JWT, password hashing, and persistence adapters.
- `AuthService.Shared` contains transport-neutral result types and shared authentication codes.

Run locally with:

```powershell
dotnet run --project src/AuthService.Api/AuthService.Api.csproj
```

The local launch profile selects `Development`, connects to the local `SQLEXPRESS06` instance with Windows authentication, and uses a signing key intended only for local use. The existing Docker Compose file does not configure SQL Server; when running the API in a container, configure `ConnectionStrings__AuthDb` to a database host reachable from that container. In other environments, also set `Authentication__Jwt__SigningKey` to a secret of at least 32 bytes.

Accounts and refresh-token digests use SQL Server in the API; automated integration tests replace those stores with isolated in-memory adapters. Refresh tokens rotate on use and only their SHA-256 digests are retained. The welcome notification adapter is a logging placeholder until an email provider is configured.

Available routes are `POST /api/auth/register`, `POST /api/auth/sign-in` (also `POST /api/auth/login`), `POST /api/auth/refresh-token`, `POST /api/auth/revoke-token` (authenticated), `GET /api/auth/me`, and `GET /health`.
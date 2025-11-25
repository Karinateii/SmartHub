# SmartHub

SmartHub is a modular .NET 8 web API following Clean Architecture principles. This repository contains API, Application, Domain, and Infrastructure layers.

## Development notes
- Secrets: Replace or set `Jwt:Key` using environment variables in production. A sample configuration is available at `SmartHub.Api/appsettings.example.json`.
- To run the API locally:

```bash
dotnet build
dotnet ef database update --project SmartHub.Infrastructure --startup-project SmartHub.Api
dotnet run --project SmartHub.Api
```

## Testing
- Run unit tests:
```bash
dotnet test
```

## Environment variables for secrets (recommended)
Set the following environment variables instead of storing them in code or appsettings.json:
- JWT_KEY: Your symmetric key for HS256 signing (at least 32 characters)
- JWT_ISSUER: Token issuer (e.g., SmartHub)
- JWT_AUDIENCE: Token audience (e.g., SmartHubClient)
- ADMIN_EMAIL: Email for an admin seed user (optional)
- ADMIN_PASSWORD: Admin password used for seeding (optional)

Example (PowerShell):
```powershell
$env:JWT_KEY = "01234567890123456789012345678901"
$env:JWT_ISSUER = "SmartHub"
$env:JWT_AUDIENCE = "SmartHubClient"
$env:ADMIN_EMAIL = "admin@example.com"
$env:ADMIN_PASSWORD = "ChangeMe!123"
```

Local setup script:
You can use the provided script to create a local `appsettings.json` from the example and optionally set local env variables in your current PowerShell session:

```powershell
cd .\scripts
./setup-dev.ps1 -SetEnvVars
Install Git hooks (optional):
You can install the local git hooks (pre-commit) to help prevent committing secrets accidentally:

```powershell
cd .\scripts
./install-hooks.ps1
```

This will copy `scripts/pre-commit.ps1` into your repository's `.git/hooks/pre-commit` as a shim script that runs the PowerShell check before commit.
```

CI requirement:
The GitHub Actions workflow now requires a repository secret named `JWT_KEY` to be set. The CI will fail (intentionally) if the secret is missing, and will set the `JWT_KEY` environment variable for the job if provided.
dotnet run --project SmartHub.Api
```

Note: The repository no longer contains `appsettings.json` to reduce the risk of accidentally committing secrets; use `appsettings.example.json` as a template.

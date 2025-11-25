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

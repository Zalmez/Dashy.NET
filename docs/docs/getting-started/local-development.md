---
sidebar_position: 2
---

# Local Development

Dashy uses .NET Aspire to orchestrate all required services during development.

## Prerequisites

- .NET SDK 10
- Docker Desktop (for Redis and PostgreSQL containers)
- Node.js 20+ (only required for editing docs)

## Run The Application

From the repository root:

```bash
dotnet run --project dashy3.AppHost
```

This starts:

- PostgreSQL (`dashgres` container)
- Redis cache
- Migration service
- API service
- Web frontend

When startup completes, open the Aspire dashboard and use the `webfrontend` endpoint.

## Development Notes

- Changes in `dashy3.AppHost/AppHost.cs` require restarting the AppHost process.
- API and Web code changes are typically picked up on rebuild/restart of those projects.
- The migration service runs before the API service to keep schema in sync.

## Troubleshooting

- If startup fails, check container health in the Aspire dashboard first.
- If database issues persist, verify PostgreSQL is running and reachable from the API service.
- If authentication appears broken, confirm the API is reachable from the web frontend and that OIDC startup settings are valid.

---
sidebar_position: 1
---

# How-to: Run Dashy Locally

## Use This Guide When

You already know Dashy and want a fast, repeatable local run process.

## Steps

1. Open a terminal at repository root.
2. Start the application:

```bash
dotnet run --project dashy3.AppHost
```

3. Open Aspire dashboard.
4. Open webfrontend endpoint.

## Operational Notes

- Changes to AppHost require a restart of the AppHost process.
- Migration service runs before API service.
- Web and API projects use Aspire service discovery.

## Common Recovery Actions

- Restart AppHost if resource dependency order gets out of sync.
- Confirm postgres and cache resources are healthy.
- Review API and web resource logs in Aspire when requests fail.

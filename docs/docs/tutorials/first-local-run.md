---
sidebar_position: 1
---

# Tutorial: First Local Run

## Audience

Developers new to Dashy who want to get the application running locally for the first time.

## Goal

Start the full Dashy stack with Aspire and open the web UI.

## Before You Start

- Install .NET SDK 10.
- Start Docker Desktop.

## Steps

1. Open a terminal at the repository root.
2. Run the AppHost:

```bash
aspire run
```

3. Wait for Aspire resources to become healthy.
4. Open the Aspire dashboard in your browser.
5. Open the endpoint named webfrontend.

## What You Should See

- PostgreSQL and Redis resources are running.
- The migration service completes before the API starts.
- The web frontend opens and can call the API.

## If Something Fails

- If containers fail, restart Docker Desktop and rerun AppHost.
- If API startup fails, verify the database resource is healthy first.
- If the frontend loads but data fails, check API logs in Aspire.

## Next Step

Continue with [How-to: Run Dashy Locally](../how-to/run-dashy-locally.md) for repeatable daily workflow.

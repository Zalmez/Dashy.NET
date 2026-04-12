---
sidebar_position: 1
---

# Explanation: System Architecture

Dashy is designed as a small distributed application orchestrated by .NET Aspire.

## Why Aspire Is Central

Aspire provides a single development entry point and coordinates dependency startup order.

This is important because Dashy depends on:

- PostgreSQL for persistent configuration and user data.
- Redis for cache features.
- Migration service to apply schema changes before API startup.

## Component Responsibilities

- AppHost: declares infrastructure and service dependencies.
- MigrationService: applies EF Core migrations.
- ApiService: business logic, identity, dashboard APIs, integrations.
- Web: Blazor UI, cookie auth, optional OIDC flow.

## Data And Collaboration Model

Dashboards are owned entities with ordered widgets.

Collaboration is scoped per dashboard using explicit collaborator permissions.

This keeps sharing flexible without requiring full admin-level role changes.

## Why API Keys Exist Alongside User Headers

Dashy supports both interactive user sessions and automation.

- User flow: X-User-Id context from authenticated frontend behavior.
- Automation flow: X-Api-Key for scripts and integrations.

This dual model allows integrations without coupling to interactive sign-in.

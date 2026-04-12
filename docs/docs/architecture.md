---
sidebar_position: 3
---

# Architecture Overview

Dashy is split into orchestration, frontend, backend, and data layers.

## Runtime Topology

1. Aspire AppHost starts infrastructure resources (PostgreSQL, Redis) and application projects.
2. Migration service applies EF Core migrations to the database.
3. API service exposes HTTP endpoints for dashboards, auth, settings, widgets, and integrations.
4. Blazor web frontend calls the API through service discovery.

## Core Components

- `dashy3.AppHost`
  - Defines resources and project dependencies.
  - Coordinates startup order and health checks.
- `dashy3.ApiService`
  - Minimal API endpoints for product features.
  - ASP.NET Core Identity for users and roles.
  - EF Core data access for dashboards, widgets, invites, and keys.
- `dashy3.Web`
  - Blazor server UI.
  - Cookie authentication with optional OIDC provider integration.
- `dashy3.MigrationService`
  - Applies pending migrations at startup.

## Data And Access Model

- Dashboards have an owner, visibility setting, and ordered widgets.
- Collaborators can have `Viewer` or `Editor` permissions.
- Role model includes `Admin` and `User`.
- API keys can be global or scoped to specific dashboards.

## Health And Observability

- API service exposes `/health`.
- AppHost configures health checks for API and Web projects.
- Service defaults provide shared platform behavior across services.

---
sidebar_position: 6
---

# API Overview

Dashy backend endpoints are exposed under the `/api` prefix.

## API Exploration

In development, OpenAPI and Scalar API reference are enabled by the API service.

## Authentication Headers

The API supports two primary request identity headers:

- `X-User-Id` for user-context operations from authenticated frontend sessions
- `X-Api-Key` for automation and integration scenarios

Depending on endpoint and key scope, API key operations may be restricted by dashboard ownership, collaborator access, or admin role.

## Endpoint Groups

### Dashboards

- `GET /api/dashboards`
- `GET /api/dashboards/{id}`
- `POST /api/dashboards`
- `PUT /api/dashboards/{id}`
- `PATCH /api/dashboards/{id}/visibility`
- `DELETE /api/dashboards/{id}`

### Widgets

- `POST /api/dashboards/{dashboardId}/widgets`
- `PUT /api/dashboards/{dashboardId}/widgets/{widgetId}`
- `DELETE /api/dashboards/{dashboardId}/widgets/{widgetId}`
- `PUT /api/widgets/{widgetId}/content`

### Collaborators

- `GET /api/dashboards/{id}/collaborators`
- `POST /api/dashboards/{id}/collaborators`
- `PUT /api/dashboards/{id}/collaborators/{targetUserId}`
- `DELETE /api/dashboards/{id}/collaborators/{targetUserId}`

### Authentication

- `GET /api/auth/check-setup`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `PUT /api/auth/profile`
- `POST /api/auth/change-password`
- `POST /api/auth/oidc-login`

### Invites

- `GET /api/auth/invite/{token}`
- `POST /api/auth/invite/{token}/accept`
- `GET /api/admin/invites`
- `POST /api/admin/invites`
- `DELETE /api/admin/invites/{id}`

### Admin

- `GET /api/admin/users`
- `PUT /api/admin/users/{id}/role`
- `DELETE /api/admin/users/{id}`
- `GET /api/admin/stats`

### Settings

- `GET /api/settings/oidc`
- `PUT /api/settings/oidc`
- `GET /api/settings/oidc/startup`
- `GET /api/settings/email`
- `PUT /api/settings/email`

### API Keys

- `POST /api/keys`
- `GET /api/keys`
- `DELETE /api/keys/{id}`

### Utilities And Integrations

- `GET /api/stats`
- `GET /api/weather`
- `GET /api/rss`
- `GET /api/cve/nvd`
- `GET /api/cve/euvd`

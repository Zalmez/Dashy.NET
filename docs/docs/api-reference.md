---
sidebar_position: 7
---

# API Reference

Dashy.NET exposes a REST API served by the `apiservice` project. All endpoints are prefixed with `/api`.

An interactive **Scalar API explorer** is available at `/scalar/v1` when the API service is running.

## Authentication

Most API endpoints accept two authentication mechanisms:

| Mechanism | Header | Description |
|-----------|--------|-------------|
| User session | `X-User-Id: <userId>` | Sent automatically by the Blazor frontend |
| API key | `X-Api-Key: <key>` | For external integrations (see [API Keys](#api-keys)) |

## Health

```
GET /health
```

Returns `{"status":"healthy"}`. Useful for container health checks and reverse-proxy readiness probes.

---

## Dashboards

### List dashboards

```
GET /api/dashboards
```

Returns all dashboards visible to the authenticated user (own + shared + collaborated).

### Get a dashboard

```
GET /api/dashboards/{id}
```

### Create a dashboard

```
POST /api/dashboards
Content-Type: application/json

{
  "name": "My Dashboard",
  "layoutMode": "Auto",
  "visibility": "Private"
}
```

Visibility values: `Private`, `Shared`.

### Update a dashboard

```
PUT /api/dashboards/{id}
Content-Type: application/json

{
  "name": "Renamed Dashboard",
  "layoutMode": "Auto"
}
```

### Delete a dashboard

```
DELETE /api/dashboards/{id}
```

### Update visibility

```
PATCH /api/dashboards/{id}/visibility
Content-Type: application/json

{ "visibility": "Shared" }
```

---

## Widgets

### Add a widget to a dashboard

```
POST /api/dashboards/{dashboardId}/widgets
Content-Type: application/json

{
  "type": "markdown",
  "title": "Notes",
  "icon": "file-text-icon",
  "size": "Medium",
  "parentWidgetId": null,
  "config": { "content": "# Hello!" }
}
```

Widget types: `link`, `link-grid`, `system-stats`, `weather`, `markdown`, `status`, `media`, `rss`, `cve`, `section`, `api`.

### Update a widget

```
PUT /api/dashboards/{dashboardId}/widgets/{widgetId}
Content-Type: application/json

{
  "title": "Updated Title",
  "icon": "star-icon",
  "size": "Large",
  "order": 2,
  "config": { ... }
}
```

### Delete a widget

```
DELETE /api/dashboards/{dashboardId}/widgets/{widgetId}
```

### Push content to an API Widget

```
PUT /api/widgets/{widgetId}/content
X-Api-Key: <your-api-key>
Content-Type: application/json

{
  "content": "<p>Hello from the API!</p>",
  "css": "p { color: green; }"
}
```

This endpoint is used to push HTML/CSS content to an **API Widget** (`type: "api"`) from an external service or script. Supports both `X-Api-Key` and `X-User-Id` authentication.

---

## Collaborators

### List collaborators

```
GET /api/dashboards/{id}/collaborators
```

### Add a collaborator

```
POST /api/dashboards/{id}/collaborators
Content-Type: application/json

{ "userId": "<userId>", "permission": "Viewer" }
```

Permission values: `Viewer`, `Editor`.

### Update a collaborator's permission

```
PUT /api/dashboards/{id}/collaborators/{targetUserId}
Content-Type: application/json

{ "permission": "Editor" }
```

### Remove a collaborator

```
DELETE /api/dashboards/{id}/collaborators/{targetUserId}
```

---

## Users

### Search users

```
GET /api/users/search?q=alice
```

Returns up to 10 users matching the query (by email or display name). Requires authentication.

---

## System Stats

```
GET /api/stats
```

Returns server runtime metrics used by the **System Stats** widget:

```json
{
  "memoryUsedMb": 128.4,
  "memoryTotalMb": 8192.0,
  "memoryPercent": 1.6,
  "uptimeHours": 24.3,
  "processCount": 42,
  "processorCount": 4
}
```

---

## Weather

```
GET /api/weather?location=London&unit=celsius
```

Returns current weather conditions using [Open-Meteo](https://open-meteo.com/) (no API key required).

| Parameter | Required | Description |
|-----------|----------|-------------|
| `location` | Yes | City name |
| `unit` | No | `celsius` (default) or `fahrenheit` |

---

## RSS

```
GET /api/rss?url=https://example.com/feed.xml&limit=10
```

Fetches and parses an RSS or Atom feed, returning up to `limit` items (maximum 20).

---

## Authentication Endpoints

### Check if setup is needed

```
GET /api/auth/check-setup
```

Returns `{"needsSetup": true}` when no users exist yet.

### Register (first run / invite)

```
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass1",
  "displayName": "Alice",
  "role": "User"
}
```

### Login

```
POST /api/auth/login
Content-Type: application/json

{ "email": "user@example.com", "password": "SecurePass1" }
```

Returns a `UserDto` on success.

### Get current user

```
GET /api/auth/me
X-User-Id: <userId>
```

### Update profile

```
PUT /api/auth/profile
X-User-Id: <userId>
Content-Type: application/json

{ "displayName": "Alice B.", "avatarUrl": "https://..." }
```

### Change password

```
POST /api/auth/change-password
Content-Type: application/json

{ "userId": "<userId>", "currentPassword": "...", "newPassword": "..." }
```

---

## OIDC Settings

### Get OIDC configuration

```
GET /api/settings/oidc
```

### Update OIDC configuration

```
PUT /api/settings/oidc
Content-Type: application/json

{
  "authority": "https://accounts.example.com",
  "clientId": "dashy",
  "clientSecret": "secret",
  "scopes": "openid profile email",
  "callbackPath": "/signin-oidc",
  "isEnabled": true,
  "allowAutoRegistration": true
}
```

---

## Admin Endpoints

All `/api/admin` endpoints require the caller to have the **Admin** role.

### List all users

```
GET /api/admin/users
```

### Update a user's role

```
PUT /api/admin/users/{id}/role
Content-Type: application/json

{ "role": "Admin", "action": "add" }
```

Action values: `add`, `remove`.

### Delete a user

```
DELETE /api/admin/users/{id}
```

### List pending invites

```
GET /api/admin/invites
```

### Create an invite

```
POST /api/admin/invites
Content-Type: application/json

{ "email": "newuser@example.com", "role": "User" }
```

### Resend / extend an invite

```
POST /api/admin/invites/{id}/resend
```

### Revoke an invite

```
DELETE /api/admin/invites/{id}
```

---

## API Keys

API keys allow external scripts and services to interact with Dashy.NET without user sessions. Manage your API keys from the **Profile** page in the UI.

Pass the key in every request using the `X-Api-Key` header:

```bash
curl -H "X-Api-Key: your-key-here" https://your-dashy/api/dashboards
```

Keys can be global (access all dashboards) or scoped to specific dashboards. They can also be given an expiry date and revoked at any time.


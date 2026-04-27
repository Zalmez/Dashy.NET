---
sidebar_position: 4
---

# Authentication And Authorization

Dashy supports local account auth and optional OpenID Connect (OIDC) sign-in.

## Local Authentication

The API includes endpoints for:

- Initial setup detection
- User registration and login
- Profile updates
- Password changes

Users are persisted with ASP.NET Core Identity.

## OIDC Authentication

OIDC settings are stored in the database and read by the web frontend at startup.

Configuration endpoints:

- `GET /api/settings/oidc`
- `PUT /api/settings/oidc`
- `GET /api/settings/oidc/startup` (internal startup payload, includes secret)

If OIDC is enabled and configured, the frontend activates OIDC auth flow and exchanges user identity with the API via `POST /api/auth/oidc-login`.

## Roles

- `Admin`: can manage users and privileged operations.
- `User`: standard dashboard and collaboration operations.

## Dashboard Permissions

- Owner: full control over dashboard settings and collaborators.
- Editor: can modify dashboard content.
- Viewer: read-only collaborator access.

Visibility:

- `Private`: owner and collaborators only.
- `Shared`: visible to broader audience according to API access path.

## Invite Flow

Dashy supports invite-driven onboarding:

- Admin creates invite
- Invitee validates token
- Invitee accepts invite and creates account

Core endpoints:

- `GET /api/auth/invite/{token}`
- `POST /api/auth/invite/{token}/accept`
- `GET /api/admin/invites`
- `POST /api/admin/invites`

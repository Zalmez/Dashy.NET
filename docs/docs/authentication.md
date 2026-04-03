---
sidebar_position: 5
---

# Authentication

Dashy.NET requires users to sign in before accessing the dashboard. It supports two authentication methods: **local accounts** (username/password) and **OIDC/OAuth2** (single sign-on).

## First-Run Setup

When you access Dashy.NET for the first time and no users exist yet, a setup wizard will guide you through creating the initial Admin account. This account has full access to all settings, user management, and admin features.

## Local Accounts

Local accounts are the default authentication method. Users log in with their email address and password.

### Registering

- During initial setup, the first user is automatically assigned the **Admin** role.
- After setup, new accounts can only be created by an Admin via the **Invite** system (see below) or, if OIDC auto-registration is enabled, through the OIDC provider.

### Changing Your Password

Users can change their own password from their profile settings page.

### User Roles

| Role | Permissions |
|------|-------------|
| **Admin** | Full access: manage users, configure OIDC, create/edit/delete any dashboard |
| **User** | Access own dashboards and any shared dashboards |

## OIDC / OAuth2 (Single Sign-On)

Dashy.NET supports any **OpenID Connect** (OIDC)-compatible identity provider, including Keycloak, Auth0, Microsoft Entra ID (Azure AD), Authentik, Authelia, and others.

### Configuring OIDC

1. Navigate to the **Admin Panel** → **Authentication** settings.
2. Enter your provider details:
   - **Authority**: The OIDC discovery URL (e.g. `https://accounts.example.com`)
   - **Client ID**: OAuth2 client ID from your provider
   - **Client Secret**: OAuth2 client secret
   - **Scopes**: Space-separated scopes (default: `openid profile email`)
3. Toggle **Enable OIDC** and save.
4. The application will restart automatically to apply the new configuration.

> **Note:** The OIDC callback URL registered in your provider must be `https://<your-dashy-domain>/signin-oidc`.

### Auto-Registration

When OIDC is enabled, you can control whether new users are allowed to register automatically:

- **Allowed** (default): Any user who can authenticate with your IdP will get a Dashy.NET account on first login.
- **Disabled**: Only users with a pending **Invite** can sign in. All other OIDC logins are rejected.

### OIDC + Local Account Coexistence

OIDC and local accounts can be used simultaneously. Users who registered via OIDC are linked by their `sub` claim and can continue to use OIDC. Admin-created local accounts continue to use password login.

## Invite System

Admins can invite new users by email via **Admin Panel** → **Users** → **Invite User**. An invite:
- Is scoped to a specific email address.
- Assigns a role (Admin or User) at the time of invite creation.
- Expires after 7 days.
- Can be resent (extending the expiry) or revoked by an Admin.

The invited user can register using the invite link, regardless of whether OIDC auto-registration is enabled.

## Session Management

- Sessions persist for **30 days** with sliding expiration.
- Logging out immediately invalidates the session cookie.

## Security Recommendations

1. **Use a strong Admin password** for local accounts.
2. **Enable OIDC** where possible and rely on your identity provider for MFA.
3. **Disable auto-registration** in OIDC settings if you want to control who can access Dashy.NET.
4. **Use HTTPS** in production (see [Docker Deployment](/docs/docker) for reverse-proxy SSL examples).


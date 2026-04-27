---
sidebar_position: 2
---

# How-to: Configure OIDC

## Use This Guide When

You want to enable single sign-on for Dashy with an OpenID Connect provider.

## Required Values

- Authority URL
- Client ID
- Client secret
- Scopes
- Callback path

## Configure OIDC In Dashy

1. Open your Dashy instance.
2. Update OIDC settings through the API or admin UI workflow.
3. Ensure the settings include valid authority, client ID, and client secret.
4. Enable OIDC.

## Relevant Endpoints

- GET /api/settings/oidc
- PUT /api/settings/oidc
- GET /api/settings/oidc/startup

## Verification

1. Sign out of Dashy.
2. Start sign-in flow.
3. Confirm redirection to your identity provider.
4. Confirm user returns and is provisioned or mapped correctly.

## Troubleshooting

- If callback fails, verify callback path and provider redirect URI match.
- If login succeeds at provider but fails in Dashy, inspect API logs for oidc-login failure details.
- If users are blocked, check auto-registration and invite constraints.

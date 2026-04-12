---
sidebar_position: 2
---

# Explanation: Auth And Identity Model

Dashy uses layered identity support so teams can start simple and grow into enterprise sign-in.

## Local Identity First

ASP.NET Core Identity provides immediate local account support.

This gives Dashy a self-contained setup path for personal or small-team deployments.

## Optional OIDC

OIDC can be enabled when external identity providers are required.

The web app requests startup OIDC settings from the API and conditionally enables OIDC middleware.

This keeps one deployment artifact compatible with both local-auth and SSO-enabled environments.

## Invite And Registration Controls

Invite flow and auto-registration controls support stricter onboarding policies.

When auto-registration is disabled, invited users can still onboard through controlled acceptance.

## Authorization Separation

Global roles and dashboard permissions solve different problems:

- Roles control platform-wide administrative privileges.
- Dashboard collaborator permissions control content ownership and edit rights.

This separation avoids overusing admin roles for normal collaboration.

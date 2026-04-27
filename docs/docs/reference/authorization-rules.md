---
sidebar_position: 2
---

# Reference: Authorization Rules

This page describes access behavior implemented by the current API.

## Roles

- Admin
- User

## Dashboard Visibility

- Private: owner and collaborators only.
- Shared: readable by broader callers depending on endpoint and auth path.

## Collaborator Permissions

- Owner: full control of dashboard and collaborator management.
- Editor: can edit dashboard content.
- Viewer: read-only collaborator access.

## API Key Rules

- Keys are provided in X-Api-Key header.
- Revoked or expired keys are rejected.
- Non-global keys are restricted to owner visibility rules or explicit allowed dashboard list.
- Global keys and scoped dashboard lists are admin-controlled capabilities.

## Content Update Constraints

- Widget content length max: 100000 characters.
- Widget css length max: 20000 characters.

## Widget Hierarchy Constraints

- Parent widget must exist in same dashboard.
- Parent must be a section widget.
- Section nesting deeper than one level is blocked.

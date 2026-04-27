---
sidebar_position: 5
---

# Dashboards And Widgets

Dashboards are the primary organizational unit in Dashy.

## Dashboards

Each dashboard includes:

- Name
- Layout mode
- Visibility
- Ordered widgets
- Ownership and collaborator permissions

Supported operations:

- List dashboards visible to the current user
- Create, update, and delete dashboards
- Change visibility between `Private` and `Shared`

## Widgets

Widgets are ordered components inside a dashboard.

Supported operations:

- Add widget
- Update widget metadata and configuration
- Delete widget
- Update content payload for content-driven widgets

Rules enforced by the API:

- A widget can reference a section parent by `ParentWidgetId`.
- Sections cannot be nested deeper than one level.
- Widget content and CSS payload sizes are bounded by server-side validation.

## Collaborators

Owners can add collaborators and assign permission levels:

- `Viewer` for read access
- `Editor` for content modification

Editors can perform limited collaborator actions based on role checks.

## API Key Access

API keys provide automation access without interactive login.

Key types:

- Global keys
- Dashboard-scoped keys

API key calls use the `X-Api-Key` request header.

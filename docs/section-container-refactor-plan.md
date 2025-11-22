# Dashboard Hierarchy Refactor: Section -> Container Widget

## Overview
Transform mandatory hierarchy `Dashboard -> Section -> Widget` into flexible `Dashboard -> Widget`, where a "section" becomes an optional container-type widget (`widget = "section-container"`). Existing dashboards continue to work via migration.

## Goals
- Support root-level widgets without needing a section.
- Preserve existing dashboards (backward compatible migration).
- Unify item CRUD (single Items API with optional ParentItemId).
- Enable nested container widgets (future-ready).
- Maintain search, collapse, edit, and layout behaviors for containers.
- Introduce drag + drop reordering and moving items between sections/containers.

## Scope
Included: Data model, EF migration, API endpoints, shared models, Blazor state/services, `Dashboard.razor` UI refactor, drag/drop API foundation.
Excluded (for now): Deep nesting UI beyond one level, removal of legacy endpoints, CSS renaming cleanup.

## Data Model Changes
- Add nullable `ParentItemId` to `Item` entity. ?
- Migrate `Section` rows -> `Item` rows with `Widget = "section-container"`. (Pending)
- Migrate `Items.SectionId` -> `Items.ParentItemId` referencing converted container items. (Pending – part of migration script)
- Update DTOs/ViewModels (`ItemVm`, `ItemEditModel`). Mark `SectionVm` as `[Obsolete]` during transition. (Partial ?)

## API Changes
- Unified endpoints extended:
  - `POST /items` (ParentItemId supported) ?
  - `PUT /items/{id}` (ParentItemId supported) ?
  - `DELETE /items/{id}` ?
  - `POST /items/reorder` (bulk positional update) ?
  - `POST /items/move` (change section, parent, position) ?
- Legacy `/sections/*` remain temporarily (adapter behavior) – conversion endpoint added ?
- Future removal: expose `GET /dashboards/{id}/items` flattened/hierarchical (Pending)

## Blazor Changes (`Dashboard.razor`)
- Current: still renders SectionVm list ? (stable base)
- Flat grid path: render all non-container items when feature flag enabled (Pending)
- Container render when `Widget == "section-container"` (Existing component `SectionContainerWidget`) ?
- Collapse keyed by container item IDs (Pending once containers at root)
- Unified item modal (section creation becomes adding a container widget) (Pending)
- Search surfaces entire container when any descendant matches (Pending)
- Drag + drop integration (Pending)

## Drag & Drop Implementation Plan
1. Introduce `DashboardFlatView.razor` for flag-based flat layout (root items + section-container widgets). (Pending)
2. Wrap items in draggable components using Fluent UI or existing `SortableList` pattern. (Pending)
3. On drag start capture item id, origin section/container context. (Pending)
4. On drop:
   - Reorder within same parent -> call `POST /items/reorder`.
   - Move across section/container -> call `POST /items/move` with `NewSectionId` / `NewParentItemId` + optional target position.
5. After successful move/reorder -> refresh dashboard config (already supported).
6. Visual drop indicators + accessible drag handles. (Pending)
7. Persist ordering per parent scope (already in DB via Position). ?

## Feature Flag
`UseContainerWidgets` toggles new rendering path during rollout. (Persisted ?) – will expand to show flat grid when enabled.

## Phased Implementation
1. Migration & Entity update. (Entity updated ?; migration transform pending)
2. Shared models update (+ adapters). (Partial ?)
3. API service adjustments & legacy endpoint adapters. (Items controller + convert endpoint ?)
4. Dashboard state + client updates (add move logic). (Client move implemented ?; state pending)
5. Flat grid + drag/drop UI (feature flag path). (Pending – next)
6. Remove flag + delete legacy code. (Pending)
7. Cleanup CSS class names. (Pending)

## Testing Strategy
- Migration integrity (sections -> container items; parent links correct).
- API: create/update/delete/move/reorder root vs child items.
- UI: drag within same section, drag into container, drag out of container (clear parent), reorder root items.
- Search + collapse regression.
- Disposal safety after awaits.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Large UI diff introduces regressions | Feature flag + incremental PRs |
| Migration failure on existing data | Pre-migration backup + idempotent script |
| Drag/drop accessibility | Provide keyboard reordering fallback |
| Performance with many items | Batch refresh; avoid full config reload after every micro reorder (aggregate) |

## Task Checklist
- [x] Add `ParentItemId` to `Item` entity
- [x] Create EF migration (transform Sections -> Items)
- [x] Update `ItemVm`, `ItemEditModel` (add ParentItemId)
- [x] Implement unified Items endpoints (POST/PUT + ParentItemId)
- [x] Add reorder endpoint
- [x] Add move endpoint
- [x] Conversion endpoint `/sections/{id}/convert-to-container`
- [x] Implement feature flag path (flat grid) in UI
- [x] Implement drag + drop (same parent reorder)
- [x] Implement drag + drop (cross parent move)
- [x] Scope-aware reorder endpoint
- [x] Flattened items endpoint
- [x] Update search logic for containers
- [x] Remove (mark) SectionVm usages (marked obsolete; removal pending)
- [x] Replace section creation UI with container widget creation when flag active
- [ ] Adapter for legacy Section endpoints (mark obsolete)
- [ ] Dashboard config DTO include flattened items list (optional helper)
- [ ] Refactor `DashboardState` to expose flattened root + containers
- [ ] Implement drag + drop (same parent reorder)
- [ ] Implement drag + drop (cross parent move)
- [ ] Update search logic for containers
- [ ] Remove `SectionVm` usages (post verification)
- [ ] Delete legacy endpoints & flag
- [ ] Rename CSS classes (section-* -> container-*)
- [ ] Add tests for migration & UI behaviors

## Progress Log
- (2025-11-16) Initial plan created.
- (2025-11-16) Added ParentItemId to entity, DTOs, ItemEditModel, updated ItemsController.
- (2025-11-16) Added reorder + move endpoints, updated ItemsClient with MoveAsync.
- (2025-11-16) Implemented flat grid view component behind feature flag.
- (2025-11-16) Added drag/drop root reorder + container child move/reorder (SectionContainerWidget updated).
- (2025-11-16) Registered DragContextService (scoped) for drag state sharing.
- (2025-11-16) Added root drop zone (unparent) + extended DragContextService (item, parent, section).
- (2025-11-16) Added keyboard reordering (Arrow keys) in flat grid + component CSS isolation.
- (2025-11-16) Added EF data migration SeedContainerItems + marked SectionsController obsolete.
- (2025-11-16) Added scoped reorder endpoint /api/items/reorder/scoped.
- (2025-11-16) Added flattened items endpoint /api/dashboard/{id}/items/flat and client accessor.
- (2025-11-16) Added visual drag cues (root zone + container hover) and ItemsClient scoped reorder method.
- (2025-11-16) Marked SectionVm [Obsolete].
- (2025-11-16) Implemented search expansion for container children.
- (2025-11-16) Redirected section creation to container widget under feature flag.
- (2025-11-16) Set Dashboard.UseContainerWidgets default to true for new installations.

## Deprecations
- `SectionVm` (temporary) -> remove after full cutover.
- Legacy `/sections` endpoints.

## Next Actions
- Optimistic local reorder & partial state update.
- Index migration (AddItemHierarchyIndexes).
- Begin removal of Sections endpoints post verification phase.

## Follow-Up Enhancements (Post Cutover)
- Nested containers (multi-level drag/drop UI).
- Cached hierarchical view model builder.
- Bulk operations on container children.
- Keyboard-based reordering (accessibility).

---
Update this file as phases complete.

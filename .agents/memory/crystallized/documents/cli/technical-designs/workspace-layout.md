---
open-forge:
  description: Resolve one context folder with fixed native Skills and controls, retain layout evidence, and map installed destinations
  responsibility: Define the pure workspace layout, settings observation, coordinate mapping and revalidation boundaries
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Layout, Settings]
---

# Workspace Layout Technical Design

## Boundary

The [CLI Architecture](../architecture.md) places the pure `WorkspaceLayout` in Framework Workspace, below Sources. Framework Settings owns `WorkspaceLayoutReader` and `WorkspaceLayoutRevalidator`. Sources receives the pure value and never reads settings to discover a layout. Operations reads one layout observation, carries it through selection and planning, and revalidates it under the workspace lease before grants or target effects.

The [shared operation contract](../shared-operation-contract.md#workspace-layout) defines the cross-command behavior. [Install](../contracts/install/interface.md) defines the first-installation option and question. This design does not add a mount system or make loading membership a property of folder placement.

## Content And Fixed Paths

A workspace has one main root, the context folder. Its default is `.agents`. A custom root may be nested, such as `docs/forge`.

| Content | Location |
| --- | --- |
| Loader, human guide and ordinary categories | `<mainRoot>/loader.md`, `<mainRoot>/README.md` and category folders below the main root |
| Native Skills and their catalogues | `.agents/skills`, even with a custom root |
| Authored settings | `.agents/open-forge.json` |
| Generated ownership | `.agents/open-forge.lock.json` |
| Host instructions | Workspace-root `AGENTS.md` and `CLAUDE.md` |
| Mutation lock and new recovery bundles | Workspace-root `.open-forge-state/` |

The host sections point to the selected loader. `CLAUDE.md` retains its `@AGENTS.md` bridge. The custom loader can list the native Skills catalogue using a relative link. A competing `skills` category inside a custom root is invalid. An absent native Skills folder is optional, while an inaccessible or unsafe existing folder is an observation failure.

`SourceRoots` is `[.agents]` for the default layout and `[MainRoot, .agents/skills]` otherwise. These are primary content locations only. They neither select what agents load nor grant unrestricted write authority. Outside-route registrations extend inventory and maintenance classification separately.

## Settings v4

The optional `mainRoot` string introduced in settings schema v4 names the canonical workspace-relative context folder. Absence means `.agents`. An absent default value is never inserted. Unrelated rewrites preserve existing schema declarations, unknown values and property order. New settings files and explicit custom-root changes declare the current settings version. Published schemas v1 through v4 remain unchanged, and CLI JSON results remain schema 3.

`WorkspaceMainRootPath.IsCanonical` rejects empty or noncanonical paths, absolute paths, traversal, the workspace root, Git metadata, host filenames and their descendants, `.open-forge-state/` and its descendants, and descendants or case aliases of `.agents`. Exactly `.agents` remains valid. Physical observation separately rejects unsafe ancestry and collisions.

`WorkspaceSettingsDocument.DeclaredMainRoot` retains the optional declaration and `MainRoot` exposes its effective value. `WorkspaceSettingsCodec.SetMainRoot` returns no change when the effective value is unchanged. Settings `roots` registrations are validated against both primary content locations. They remain literal workspace paths.

Editing `mainRoot` moves no files. A disagreement with installation evidence is reported by readers and stops writers. Older CLIs do not support custom roots.

## Outside Routes And Settings v5

Settings v5 adds optional `outsideRoutes`, an ordered list of canonical workspace-relative directory paths. Absence means empty. The separate `roots` list still contains included files. Folder registrations may coexist with an independent file registration inside the folder.

Outside-route maintenance requires a CLI that supports settings v5. Registration never makes an outside path an implicit package destination.

Older CLIs do not maintain directory registrations. Preserving an unknown settings property is not downgrade compatibility. This is separate from configurable-main-root support and does not relocate an established installation.

Each outside route has its declared directory and canonical catalogue path. `WithOutsideRoutes` replaces the pure layout's registrations without changing primary mappings. It rejects portable duplicates, case aliases and overlaps in either ancestor direction with the main root, `.agents`, another outside route, Git metadata or workspace runtime storage. The workspace root is invalid.

`InventoryRoots` combines primary source roots and outside directories in declaration order. It conveys discovery scope. `TryGetMaintenanceBoundary` classifies paths ordinally, checking native Skills before the encompassing default main root. An outside boundary ends at its registered directory and uses its canonical catalogue as anchor. Existing command restrictions, ownership, physical safety and explicit permissions remain separate checks.

Adding a registration declares settings v5. Removing the last one retains an empty array and keeps the existing declaration. Both mutations return no change when the requested state already holds. Unrelated properties and their order survive rewrites. `WorkspaceRoutePath` owns canonical catalogue construction for layout and source classification.

## Pure Mapping

`WorkspaceLayout.Create` establishes the workspace and main root. `LoaderPath`, `HumanGuidePath`, `SourceRoots` and `IsDefault` expose derived facts. The mapping methods perform no filesystem or settings reads.

- `CategoryDirectory` and `SourcePath` map primary source coordinates. `skills/...` always maps to `.agents/skills/...`.
- `MapPayloadPath` maps `.agents/` package content once, preserving native Skills, settings, ownership and host filenames. Other package destinations retain their paths.
- `TryGetSourceRelativePath` supplies identity relative to the relevant content location, including the `skills/` prefix for native Skills.
- `IsContentPath` supports content classification. `IsImplicitContentDestination` separately determines implicit destination admission.

Short IDs, literal operands, package paths and installed paths are different coordinates. Literal `./<workspace-path>` operands never pass through package mapping. Package validation and source hashes use package coordinates. Exclusions, permissions, collisions, ownership and installed comparisons use mapped destinations. Recorded and retired ownership paths are already installed destinations and are checked literally, never mapped again. [Embedded payload](embedded-payload.md) and [lifecycle provenance](lifecycle-provenance.md) define rendering and comparison.

## Ordinal Authority Checks

Authority checks use ordinal, case-sensitive content-root prefixes even where observation or portable collision detection recognizes case aliases. A differently cased folder can be a different folder on a case-sensitive filesystem. It must not gain implicit write permission or become a Route mutation target through case-insensitive classification. Explicit grants remain literal and retain their shared portable comparison rules. Containment, reserved controls, ownership, overwrite and command-specific restrictions still apply after admission.

## Observation And Revalidation

`WorkspaceLayoutRead` retains `Settings`, `Ownership`, nullable `Layout`, `State`, `Issues` and file/ancestry `Expectations`. Its states are `Ready`, `Uninstalled`, `Mismatch`, `Invalid` and `Unavailable`. The reader observes fixed settings and ownership, the selected loader, the known default loader and recognized managed host pointers. It does not search arbitrary folders for another installation. Missing optional host files remain lifecycle facts.

The final observation rules incorporate chair amendments 5 through 7:

- Whenever settings supply a valid layout, retain it for diagnosis even if an anchor is unsafe, loaders conflict or are missing, ownership is unreadable, or the layout changes during observation. Retaining the value does not make a blocked observation writable.
- Only invalid or unavailable settings leave `Layout` empty. Read-only observers retain every domain, mark layout-dependent domains unavailable with the settings issue as the cause, and never substitute `.agents`.
- A loader recorded as removed is intentional. Raise `MissingLoader` only when installation evidence exists, the selected loader is absent and settings do not record its removal.
- A missing loader alone is drift. The layout stays `Ready` with the issue retained, allowing Update and Repair to apply their existing restoration rules. Evidence of another layout, such as a conflicting loader or an ownership or host pointer naming another loader, blocks writers. A competing Skills folder is unsafe.
- Invalid or unavailable ownership is recorded and its layout cross-check is skipped. It does not by itself make the layout unavailable. Each command retains its own ownership requirements.
- A layout that changes during observation is `Unavailable`. No writer silently chooses another layout after planning.

`WorkspaceLayoutRevalidator.ValidateAsync` uses the held physical-workspace lease and existing expected-state validation. It runs before settings grants as well as target effects. Post-effect verification compares intended state, not obsolete pre-effect settings. Runtime locking and recovery locations stay fixed under the [mutation and recovery design](mutation-and-recovery.md).

Outside registrations come from that same settings snapshot. `WorkspaceLayoutRead.OutsideRoutes` retains their declaration order, descriptors, physical resolutions, directory and catalogue observations, failures and issues. Each outside observation is `Available`, `Missing`, `Unsafe` or `Unavailable`. A safe ordinary directory with a missing catalogue is available for later scaffolding. A missing directory or ancestor stays diagnosable. Unsupported mappings, reserved overlaps and inaccessible content are never treated as safe absence.

Outside-only problems do not change the primary layout state. Maintenance within an outside boundary requires its safe available observation and command-specific protection checks. Deregistration can use the canonical registration and loader row without depending on available tree contents. The reader observes ancestry and the anchor without recursively scanning, creating or repairing anything.

The descriptor's anchor is canonical. Commands that admit an existing recognized compatibility entrypoint retain that actual filename and require an unambiguous structural chain. A stale Loader row naming the missing canonical anchor is not authority to replace the compatibility file, relist the route or repair the row implicitly. Generic outside metadata uses Workspace defaults, regardless of directory names.

Safe directory, ancestry and catalogue expectations join the layout's existing expectations. Lease validation checks those facts and retained directory accessibility before effects. A changed expected state still blocks the planned operation. Each target mutation separately proves containment within the selected physical boundary. Registration grants no ownership or implicit package permission.

## Source Transport

Install forms its intended layout with the observed outside registrations and preserves outside observations when copying the retained read. Ordinary setup, configuration, form conversion and Update keep those facts through expected-state validation and final verification. Package mapping and implicit destination permission remain primary/native only.

Catalogue requests, catalogues, read sessions, loading closure and mutation plans carry the selected layout. Sessions derive it from their catalogue. Read sessions scan `InventoryRoots` and retain explicit default-selection scopes for the primary content locations and registered directories. Included-file registrations do not scan siblings. Catalogue reconstruction preserves included `./path` identities, aliases, conditions and order, including sources that have no inventory candidate. Loader links resolve relative to the actual loader directory.

Primary source IDs stay relative to their primary content location. Outside sources keep exact `./path` identities, including catalogue filenames and extensions, before and after registration. Outside `SKILL.md` files are ordinary Markdown. Discovery retains readable lexical names independently from portable write eligibility. Inventory grants no loading membership, ownership or implicit package permission. The [source-reference contracts](../contracts/shared/source-references/interface.md) define the public forms, and [generated navigation](generated-navigation.md) defines retained loader rows and projection.

An inventoried outside catalogue can also carry authored inclusion policy. Established traversal precedes inspection of disconnected registered catalogues, so discovery cannot replace the first-reaching row's metadata. The merge retains every row, condition, span and alias, including unresolved targets. Outside physical aliases merge without removing existing primary identities or separating an overwrite from its base. Explicit registered catalogue selection covers its inventory scope, while ordinary included catalogue selection follows retained edges without scanning siblings.

Shared maintenance carries the exact selected logical coordinates separately from physically merged sources. Its retained layout observation must use the catalogue's layout instance. A selected alias cannot substitute another source coordinate for mutation. Selected anchors determine the bounded chain, while structural registration and authored loading remain separate. [Forgiving maintenance](forgiving-maintenance.md#eligible-files) defines completion admission and defaults.

## Delivery Boundary

Delivery 1 supports choosing an absent or empty custom destination at first installation and using it through every command family. Default-root adoption remains supported. An omitted or unchanged root preserves ordinary Install behavior. Changing an established installation's root is refused before effects, including with `--configure`.

Delivery 2 adds moving an established installation. It must inventory ignored and untracked files, protect both locations with ignore rules throughout the move, define preserved metadata, verify destination bytes before deleting originals, preserve later edits during restoration, and refuse Library source intersections or mappings that cannot retain one destination root and unchanged suffixes. These are future move requirements, not available behavior.

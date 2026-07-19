# Extensions

## Description

This descriptor governs first-party and local Open Forge extension mechanics.

Extensions add optional routed files to an installed workspace without changing the base framework contract.

## Represents

An extension is the content-agnostic installable unit for optional #Core, #Memory, or #Extension material.

An extension can contain one or more workflows, skills, patterns, guidance, directives, memory routes, workspace routes, or support files when those routes belong in the normal Open Forge tree. It may also contain only dependencies as a convenience pack. Its installed files retain their native runtime meanings; extension is never a competing primitive type.

Extension payload routes use #Extension plus the route type and useful scope tags when the file format is Open Forge-authored. They use a reserved load-policy tag only when the extension intentionally adds baseline-loaded material.

## Source Contract

A first-party extension lives under `src/extensions/{extension-id}/`.

It may contain maintainer files such as `extension.json` and `README.md`, but standalone installable runtime content must live under `payload/`. A dependency-only or augmentation-only package may omit `payload/` when its manifest declares at least one dependency or augmentation; the shape works by bundled id or from a copied local directory containing only those maintainer files and declared fragments.

Only `payload/` is copied when the CLI installs a package-shaped extension. Installed markdown remains runtime truth.

Extension payload route files should use #Extension metadata by default when that metadata does not break a native runtime format. Runtime-native files such as `SKILL.md` keep native metadata and may be indexed with default route tags. Load-policy tags stay deliberate choices so installed extensions do not become permanent baseline context by accident.

`extension.json` may provide a lowercase stable `id`, non-empty `name`, `description`, and `version` strings, a duplicate-free `dependencies` array of bundled extension ids, and explicit `augmentations`. A dependency may name any bundled extension regardless of its payload contents. Bundled packages receive their id from the catalogue; a declared id must match. A local source that declares an id opts into receipt-managed ownership, even with direct-overlay layout. Any local source declaring dependencies or augmentations requires that stable identity; only a plain idless overlay remains unmanaged.

The manifest is CLI metadata and must not be required by agents at runtime. `version` is descriptive until a compatibility contract exists. Installed files and materialized augmentation blocks remain complete runtime truth.

## Install Contract

The CLI resolves extension arguments in this order:

1. existing local directory
2. bundled first-party extension id

A local directory with `payload/` installs only `payload/`. A local directory without `payload/` uses direct-overlay layout. Stable manifest identity, not that layout choice, determines whether Open Forge records ownership.

Multiple bundled ids can be installed in one command. One invocation and its complete dependency closure form one review unit; index generation runs once after all selected payloads are copied. Users install roots in separate commands when they want separate diffs.

Bundled dependencies resolve transitively, offline, and before dependents. Local packages may depend on bundled ids, but dependency resolution never fetches remote content or treats an arbitrary local path as a named dependency.

Before writing, the installer validates strict manifest and receipt fields plus the complete dependency graph, then builds one source plan. Manifest and payload paths are portable slash-separated relative paths; literal backslashes are rejected rather than treated as separators. Portable path identity is Unicode-normalized and case-folded, and path segments also reject Windows-invalid characters, trailing dots or spaces, and reserved device basenames. Different bytes targeting one portable path are a blocking collision, identical planned bytes are deduplicated, file-parent conflicts and existing target aliases are rejected, and linked local or direct bundled package roots, target links, or multiply linked files cannot redirect writes. A managed package cannot adopt an existing unowned file, even when bytes match; an unmanaged overlay cannot replace a receipt-owned path. A shared managed file can update only when every current owner participates and provides the same bytes. Git control paths are rejected—`.git` as any path segment and `.gitignore` at any depth—so an extension cannot mutate repository internals or hide its own output. The selected index tree is checked independently even for an empty or outside-`.agents` payload.

`--dry-run` reports resolved order and create/update/delete/unchanged effects without writing, and validates existing index shape without previewing generated-index body changes. Applied payload and augmentation changes, generated indexes, and the ownership receipt roll back together on a handled in-process failure; there is no persistent crash-recovery journal.

Normal writes require recognizable tracked Core anchors for extensions, a clean target-scoped checkpoint when Git is present, and Git-visible planned plus derived-index output. The installer asks the user to review and commit each completed review unit before another mutation. Outside Git, an interactive mutation may proceed only after recommending `git init` and receiving explicit confirmation; non-interactive mutation stops. Catalogue listing and dry-run planning are read-only and remain ungated.

`--pro` intentionally bypasses the Git/Core lifecycle checks only. It never bypasses dependency, manifest, receipt, ownership, containment, portable collision, link, index, preservation, or rollback safety.

Catalogue content labels are derived from payload paths rather than declared as runtime truth in manifests. Interactive selection shows every bundled extension, immediately marks transitive dependencies as required, and prevents removing a dependency while a selected extension needs it. Directly selecting a required dependency preserves that direct selection when its former dependent is removed. Unattended use must have a deterministic id list and resolves the same closure.

## Augmentation Contract

Augmentation is the explicit mechanism for a managed extension to contribute to an existing already-loaded Markdown file without taking ownership of the whole file or adding another agent decision.

- The base target opts in with one named `open-forge-augment.{slot}` marker pair.
- The manifest names the portable base target, lowercase slot id, and package-local Markdown fragment.
- The installer places one `open-forge-extension.{id}` block inside the slot and orders blocks by extension id.
- Slots contain only whitespace and owned blocks. Fragments contain no reserved markers and are not installed or routed separately; a declared source is excluded even when it lives below `payload/`.
- Targets must exist and cannot be overwrite files or the ownership receipt. The installer never guesses headings or merges unmarked prose.

A managed payload may declare empty slots in a base Markdown file for later extension composition. It may not ship pre-owned blocks or malformed, nested, duplicate, or non-empty slots.

Managed extensions cannot own `.overwrite.md` files as a shared-file mutation strategy. Materialized augmentation blocks are part of the base file; a workspace-owned overwrite is loaded after that base and remains the final local-precedence layer.

## Ownership And Removal Contract

Managed installations persist transparent Git-visible CLI state in root `open-forge.extensions.json`. It records explicitly requested roots, extension versions and dependencies, owned payload paths and digests, owner sets, and augmentation block digests. Receipt `sha256` protects extension-authored bytes. A bounded generated `Entries` body is owned by the CLI and may change through `index` or Core without invalidating ownership of its entrypoint. Agents do not load the receipt; materialized runtime files remain sufficient.

Before managed install, update, or removal, the CLI verifies receipt reciprocity and every recorded file and block digest. It also validates every retained block against the complete planned final target. It refuses externally modified or missing owned content, or a plan that erases a retained slot or block, instead of adopting or overwriting uncertain state. Reinstalling a stable id reconciles dropped payload paths and augmentations when their recorded bytes still match.

Core reinstall validates the same receipt boundary. It may leave receipt-owned files and occupied slots unchanged, but refuses to modify or remove them; managed extension commands own those transitions.

Removal acts on exactly the requested ids and blocks removal while a retained extension still depends on one of them. Files survive while another owner remains or an installed augmentation still needs them. Updating or removing an owned entrypoint is blocked when the final route tree would strand retained descendants; the same plan must move or remove them, or another owner must keep the route host. The current lifecycle does not automatically prune dependencies that become installed orphans; users preview and remove those ids explicitly. Any future `--prune` behavior requires an explicit root and legacy-package policy.

## Sharing Contract

Shared behavior must not be duplicated when a small shared route or shared extension is clearer. Every first-party installed path has one canonical owning package.

Put a shared native skill in one skill-only extension and declare its id from each dependent workflow or pack. A convenience pack contains dependency edges rather than duplicate payload files. Keep the graph small and acyclic. Runtime workflows still name the concrete installed Required Routes they need; a manifest dependency never substitutes for the route contract.

Native skills installed directly by a user or an external manager may coexist under `.agents/skills/{skill-name}/`. Open Forge routes that standard package shape and does not redefine skill activation or packaging. After an external install, `open-forge index` adds the package to generated routing without modifying or adopting its contents. Two managers must not own the same installed skill path.

Idless plain overlays and direct/APM skill installs are intentionally unmanaged. A local source explicitly opts into Open Forge ownership by declaring a stable manifest id, and must do so before declaring dependencies or augmentations. The receipt supports local install, update, and explicit removal, but remains visible CLI state rather than hidden agent routing state. Registry resolution, compatibility solving, migration hooks, automatic orphan pruning, and crash-recovery journaling remain outside the current contract.

## Why

Extensions keep the default install small while letting Open Forge offer useful workflows, skills, and pattern packs without forcing them on every workspace.

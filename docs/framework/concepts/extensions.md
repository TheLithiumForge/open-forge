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

It may contain maintainer files such as `extension.json` and `README.md`, but installable runtime content must live under `payload/`. A dependency-only pack may omit `payload/` when its manifest declares at least one dependency; the shape works by bundled id or from a copied local directory containing only those maintainer files.

Only `payload/` is copied when the CLI installs a package-shaped extension. Installed markdown remains runtime truth.

Extension payload route files should use #Extension metadata by default when that metadata does not break a native runtime format. Runtime-native files such as `SKILL.md` keep native metadata and may be indexed with default route tags. Load-policy tags stay deliberate choices so installed extensions do not become permanent baseline context by accident.

`extension.json` may provide non-empty `name`, `description`, and `version` strings plus a duplicate-free `dependencies` array of bundled extension ids. A dependency may name any bundled extension regardless of its payload contents. The manifest is install-time metadata and must not be required by agents at runtime. `version` is descriptive until a compatibility contract exists.

## Install Contract

The CLI resolves extension arguments in this order:

1. existing local directory
2. bundled first-party extension id

A local directory with `payload/` installs only `payload/`. A local directory without `payload/` is treated as a direct overlay.

Multiple bundled ids can be installed in one command. Index generation runs once after all selected payloads are copied.

Bundled dependencies resolve transitively, offline, and before dependents. Local packages may depend on bundled ids, but dependency resolution never fetches remote content or treats an arbitrary local path as a named dependency.

Before writing, the installer validates strict manifest fields and the complete dependency graph, then builds one source plan. Portable path identity is Unicode-normalized and case-folded; different bytes targeting one portable path are a blocking collision, identical bytes are deduplicated, file-parent conflicts and existing target aliases are rejected, and target links or multiply linked files cannot redirect writes. The selected index tree is checked independently even for an empty or outside-`.agents` payload. `--dry-run` reports resolved order and create/update/unchanged counts without writing, and validates existing index shape without previewing generated-index body changes. Applied payload and generated-index changes roll back on an in-process failure; there is no persistent crash-recovery journal.

Catalogue content labels are derived from payload paths rather than declared as runtime truth in manifests. Interactive selection shows every bundled extension, immediately marks transitive dependencies as required, and prevents removing a dependency while a selected extension needs it. Directly selecting a required dependency preserves that direct selection when its former dependent is removed. Unattended use must have a deterministic id list and resolves the same closure.

## Sharing Contract

Shared behavior must not be duplicated when a small shared route or shared extension is clearer. Every first-party installed path has one canonical owning package.

Put a shared native skill in one skill-only extension and declare its id from each dependent workflow or pack. A convenience pack contains dependency edges rather than duplicate payload files. Keep the graph small and acyclic. Runtime workflows still name the concrete installed Required Routes they need; a manifest dependency never substitutes for the route contract.

Native skills installed directly by a user or an external manager may coexist under `.agents/skills/{skill-name}/`. Open Forge routes that standard package shape and does not redefine skill activation or packaging. After an external install, `open-forge index` adds the package to generated routing without modifying its contents. Two managers must not own the same installed skill path.

Direct overlays are intentionally unmanaged. The current installer has no persistent ownership receipt, lock, compatibility solver, update, remove, or migration lifecycle. Those capabilities may use visible install metadata later, but must not become hidden agent routing state.

## Why

Extensions keep the default install small while letting Open Forge offer useful workflows, skills, and pattern packs without forcing them on every workspace.

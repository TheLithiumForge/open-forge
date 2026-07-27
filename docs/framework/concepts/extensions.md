# Extensions

## Description

This concept governs optional Open Forge extension sources, installation, ownership, and removal.

An extension is a content-agnostic install unit whose payload adds whole files to existing routes. Installed files retain their ordinary primitive meaning and remain complete runtime truth.

## Represents

Extensions represent optional capabilities and composition at install time.

One extension may contain a skill, workflow, directives, patterns, guidance, workspace or memory routes, support files, a deliberate mix, or only dependency edges as a convenience pack.

Extension is not a runtime primitive. Agents use installed routed files without consulting a manifest, catalogue, receipt, source group, or CLI.

## Source Contract

Bundled sources live under organizational groups:

```text
src/extensions/{skills,workflows,packs,support}/{package-folder}/
  extension.json
  README.md
  payload/
```

The group and package folder aid maintenance and catalogue presentation only. The manifest `id` is the stable install and ownership identity. Runtime meaning comes from files below `payload/` after installation.

`extension.json` may define a stable lowercase `id`, display `name`, `description`, descriptive `version`, and duplicate-free bundled `dependencies`. Every bundled source has an id. A local source may remain an idless unmanaged payload or opt into managed ownership by declaring an id.

Package-shaped standalone runtime content lives under `payload/`. A dependency-only pack may omit `payload/` when it declares at least one bundled dependency. An extension never uses a shared-file mutation block or companion addition; every installed contribution is a whole file in the ordinary routed tree.

Extension-authored Open Forge route files normally use #Extension plus their primitive and useful scope tags. Standard files such as `SKILL.md` keep their runtime-required metadata. Markdown route links use containing-file-relative destinations in the assembled workspace. Load-policy tags are added only when the installed content intentionally belongs in baseline or continuity loading.

## Plain-File Contract

A manual install may copy `payload/` into the workspace and update affected generated `Entries` by hand. After that, the installed files are fully usable through plain Markdown routing.

The CLI automates payload copying, dependency selection, index regeneration, lifecycle checks, and managed receipts. Those services improve safety and convenience but never become a runtime dependency or hidden source of meaning.

## Install Contract

The installer validates complete resolved payload plans before writing. Paths stay portable, target-relative, and inside the selected workspace. Links, reserved repository-control files, portable collisions, and conflicting bytes stop the operation.

Bundled dependencies resolve offline, transitively, and dependency-first. One invocation plus its selected closure is one review unit. Separate invocations create separate review units.

Normal writes use a clean target-scoped Git checkpoint and end with a review-and-commit prompt. Outside Git, interactive use asks before proceeding and non-interactive mutation stops. `--pro` may bypass only these Git/Core lifecycle guards, never containment, collision, ownership, dependency, or rollback safety.

`--dry-run` performs the same planning and validation without writing. Payload, generated-index, and receipt changes form one rollback-capable in-process transaction; Git remains the durable recovery boundary.

## Ownership And Removal Contract

A local source with a manifest id opts into the same managed lifecycle as a bundled source. Managed state is transparent and Git-visible in `open-forge.extensions.json`.

The receipt records requested roots, dependency edges, versions, owned payload paths, content digests, and shared owner sets. It is install metadata only and is never routed to agents.

Reinstalling the same id reconciles owned whole files. Removal acts only on requested ids, refuses to break retained dependents or route reachability, and deletes a file only when its recorded bytes still match and no owner remains. Orphan dependencies are not pruned automatically.

Idless overlays and externally installed skills remain unmanaged. Regenerating or manually editing `Entries` makes them routable without transferring ownership. Two managers must not own the same installed path.

Workspace-owned `.overwrite.md` files are outside extension ownership. Extensions add whole routed files rather than using overwrites to change shared targets. The [current overwrite contract](../../../.agents/memory/crystallized/documents/framework/routing/overwrites.md) owns the companion's meaning and lifecycle.

## Sharing Contract

A reusable skill belongs in one skill extension. Dependent workflows declare the extension dependency for installation and link the concrete installed `SKILL.md` route in Required Routes for runtime use with `- [Reason](relative/path.md) - #Skill #Tags`.

Same-package Markdown links must resolve inside the isolated extension source payload. Framework and declared-dependency links may remain unresolved in isolated source because their containing-file-relative destinations target the final assembled workspace; they must resolve after #Core and the complete dependency closure are installed. Extension validation must not require fake duplicates or source-only stubs for those cross-package targets. `doctor` and `find --follow-required` remain manifest-agnostic complete-workspace validators, so isolated-source cross-package links are validated through the assembled workspace rather than tolerated through a hidden source-mode exception.

Convenience packs contain dependency edges rather than duplicate payload files. Keep dependency graphs small, acyclic, and explicit.

Physical grouping may change without changing stable ids. Catalogue groups are presentation only and never substitute for manifest identity or installed route semantics.

## Why

Extensions keep the base small while preserving the framework's plain-file boundary: installation may be automated, but agents need only the installed routes.

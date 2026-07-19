# Open Forge Extensions

First-party Open Forge extensions live here.

An extension is the one installable unit, not a content type. Its payload may contain one skill, one workflow, directives, any other routed material, or a deliberate mix. An extension may also be a dependency-only convenience pack.

## Current Catalogue

Skill-only capabilities:

- `architecture-capability`
- `vision-capability`
- `planning-capability`
- `implementation-capability`
- `quality-capability`

Separately selectable workflows:

- `architecture-workflow`
- `vision-workflow`
- `brainstorming-workflow`
- `implementation-workflow`
- `testing-workflow`
- `dev-workflow` - adaptive implement, test, improve, retest, diagnose, and fix loop

Broader packs and integrations:

- `workflow-essentials` - dependency-only architecture, vision, and implementation convenience pack
- `planning-workflows` - planning and task creation plus brainstorming
- `quality-workflows` - review, refactoring, and debugging plus testing
- `design-workflows` - UX exploration, experience-design review, and implementation handoff
- `reliability-defaults` - directive-only workspace safeguards
- `cli-testing-patterns` - pattern-only tiered CLI testing with fast pure feedback and explicit OS/process closure coverage
- `rune-bridge` - guidance and workspace routes for optional Rune-assisted relevance

Install only the capabilities that earn their context and maintenance cost. `open-forge extend --list` reads the shipped catalog, and dependency packs install automatically.

Most bundled extensions use this shape:

```text
{extension-id}/
  extension.json
  README.md
  augmentations/              # optional package fragments, never routed directly
  payload/
    .agents/
      ...
```

A dependency-only or augmentation-only package may omit `payload/`; it must declare at least one dependency or augmentation. A dependency-only pack installs only its dependency closure. An augmentation-only package materializes owned blocks into explicit slots without adding a standalone routed file. This shape works by bundled id or from a copied local directory containing only `extension.json`, optional `README.md`, and declared fragments.

The MVP CLI installs bundled extensions with:

```sh
open-forge extend [--dry-run] [--pro]
open-forge extend --list
open-forge extend --ids {extension-id},{extension-id}
open-forge extend {extension-id}
open-forge extend {extension-id} --dry-run
open-forge extend --remove {installed-extension-id} --dry-run
```

`extension.json` is optional only for an idless plain local payload or overlay with no dependencies or augmentations. It may contain a lowercase stable `id`; non-empty `name`, `description`, and `version`; a duplicate-free `dependencies` array of bundled extension ids; and unique `augmentations` with `target`, `slot`, and `source`. Unknown fields are rejected so misspelled ownership or dependency declarations cannot silently change installation. Bundled packages receive their id from their catalogue folder, and a declared id must match. A local source that declares an id opts into receipt-managed lifecycle even without `payload/`; every local source declaring dependencies or augmentations requires one. Idless plain overlays remain unmanaged.

Dependencies may point to any bundled extension regardless of whether it contains skills, workflows, directives, mixed routes, augmentations, or only more dependencies. They resolve transitively, offline, and before their dependents. Installed payload files and materialized augmentation blocks remain runtime truth; agents never need the manifest or ownership receipt.

Payload files should use #Extension plus their route type and useful scope tags when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` should keep native metadata. Do not use load-policy tags in extension payloads unless baseline loading is deliberately intended.

An augmentation target opts in with an explicit marker pair such as:

```md
<!-- open-forge-augment.workflow-selection:start -->
<!-- open-forge-augment.workflow-selection:end -->
```

The manifest points to that existing base Markdown target, the lowercase slot id, and a package-local Markdown fragment. The CLI inserts `open-forge-extension.{id}` blocks in deterministic id order. Slots may contain only whitespace and owned blocks; fragments are not copied or routed separately—even when their source lives below `payload/`—and cannot contain reserved markers. A managed payload may ship empty slots but not pre-owned blocks or malformed, nested, duplicate, or non-empty slots. Do not target `.overwrite.md`: a workspace-owned overwrite loads after the materialized base and remains the final local-precedence layer.

Shared behavior should have one canonical package owner. Put a shared native skill in a skill-only extension, declare that extension from every dependent workflow, keep the graph acyclic, and ensure installed workflows name the concrete Required Routes they need. Identical-file deduplication is a safety boundary, not a reason to duplicate first-party payloads.

The CLI validates every manifest and existing receipt, then builds a complete source plan before writing. Manifest and payload paths are portable slash-separated relative paths; literal backslashes are rejected. Portable paths are Unicode-normalized and case-folded, and path segments also reject Windows-invalid characters, trailing dots or spaces, and reserved device basenames. Different bytes targeting the same portable path are a collision error, identical planned files are deduplicated, file-parent conflicts and existing portable aliases are rejected, and linked local or direct bundled package roots, target links, or multiply linked files cannot redirect writes. A managed package cannot adopt an existing unowned file, even when bytes match; an unmanaged overlay cannot replace a receipt-owned path. Shared managed files can update only when every existing owner participates and supplies the same bytes. Every retained augmentation block is revalidated against the complete planned final target. The selected index tree is validated independently even for an empty or outside-`.agents` payload. Source roots and entries must be regular directories/files; links and special entries are rejected. Use `--dry-run` to inspect dependency order, scope counts, receipt changes, and every planned relative path with its create/update/delete/unchanged status without writes. Dry runs validate existing index shape but do not preview generated-index body changes. Normal installs also report counts for routed, baseline-loading, direct skill-script, and outside-`.agents` files.

Normal writes require recognizable tracked Core anchors and a clean target-scoped Git checkpoint, then ask for review and commit of the complete selected dependency closure. Planned paths, derived indexes, and `open-forge.extensions.json` must remain Git-visible; payloads may not write `.git/` or `.gitignore` and must leave repository-control changes to a separate review. Install roots separately when separate diffs matter. `--list` and `--dry-run` stay ungated; `--pro` bypasses only the Git/Core checkpoint lifecycle and never manifest, receipt, ownership, or installation-safety preflight.

Stable-id packages are recorded in transparent root `open-forge.extensions.json`, which owns payload digests, owner sets, dependency edges, requested roots, and augmentation block digests. Receipt `sha256` protects extension-authored bytes; CLI-owned generated `Entries` bodies may change through `index` or Core without invalidating entrypoint ownership. Reinstalling the same id reconciles changed or dropped owned content after verifying the existing hashes. `extend --remove` changes exactly the requested ids and blocks when a retained extension still depends on one. Updating or removing an owned entrypoint also blocks when it would strand retained routed descendants; move or remove them in the same plan, or keep another owner for the route host. Orphan dependencies are not pruned automatically. Idless overlays and direct/APM skills remain unmanaged and are never adopted by `index`.

Payload and augmentation writes, index regeneration, and receipt updates roll back together on handled in-process failures. There is no registry, network resolution, compatibility solver, migration lifecycle, automatic prune policy, persistent recovery journal, or crash recovery. Fast development tests own pure dependency-selection, portable-path, augmentation-slot, receipt, workflow/directive, and generated-index invariants. Slower closure/CI command-contract tests use fresh OS temporary roots, real CLI subprocesses and Git, then prove lifecycle, rollback, catalogue, packaged-layout, `doctor`, and Required Route behavior.

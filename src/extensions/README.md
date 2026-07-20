# Open Forge Extensions

First-party Open Forge extensions live here.

An extension is the one installable unit, not a content type. Its payload may contain one skill, one workflow, directives, any other routed material, or a deliberate mix. An extension may also be a dependency-only convenience pack.

## Current Catalogue

Skills:

- `architecture-capability`
- `vision-capability`
- `planning-capability`
- `implementation-capability`
- `quality-capability`

Workflows:

- `architecture-workflow`
- `vision-workflow`
- `brainstorming-workflow`
- `implementation-workflow`
- `testing-workflow`
- `dev-workflow` - adaptive implement, test, improve, retest, diagnose, and fix loop

Packs:

- `workflow-essentials` - dependency-only architecture, vision, and implementation convenience pack
- `planning-workflows` - planning and task creation plus brainstorming
- `quality-workflows` - review, refactoring, and debugging plus testing
- `design-workflows` - UX exploration, experience-design review, and implementation handoff

Support:

- `reliability-defaults` - directive-only workspace safeguards
- `cli-testing-patterns` - pattern-only tiered CLI testing with fast pure feedback and explicit OS/process closure coverage
- `rune-bridge` - guidance and workspace routes for optional Rune-assisted relevance

Install only the capabilities that earn their context and maintenance cost. `open-forge extend --list` reads the shipped catalog, and dependency packs install automatically.

Most bundled extensions use this shape. Catalogue grouping and source placement are organization only; neither defines package identity or runtime meaning:

```text
{optional-group}/{package-folder}/
  extension.json
  README.md
  payload/
    .agents/
      ...
```

A dependency-only package may omit `payload/` when it declares at least one dependency; it installs only that dependency closure. This shape works by bundled id or from a copied local directory containing `extension.json`, optional `README.md`, and an optional payload.

The MVP CLI installs bundled extensions with:

```sh
open-forge extend [--dry-run] [--pro]
open-forge extend --list
open-forge extend --ids {extension-id},{extension-id}
open-forge extend {extension-id}
open-forge extend {extension-id} --dry-run
open-forge extend --remove {installed-extension-id} --dry-run
```

`extension.json` is optional only for an idless plain local payload or overlay with no dependencies. It may contain a lowercase stable `id`; non-empty `name`, `description`, and `version`; and a duplicate-free `dependencies` array of bundled extension ids. Unknown fields are rejected so misspelled ownership or dependency declarations cannot silently change installation. Every bundled package declares an explicit id that remains stable independently of its source location. Missing and duplicate bundled ids are errors. A local source that declares an id opts into receipt-managed lifecycle even without `payload/`; every local source declaring dependencies requires one. Idless plain overlays remain unmanaged.

Dependencies may point to any bundled extension regardless of whether it contains skills, workflows, directives, mixed routes, or only more dependencies. They resolve transitively, offline, and before their dependents. The manifest is CLI installation metadata; installed payload files remain runtime truth and stay usable through ordinary file copying and reading without the CLI, manifest, or ownership receipt.

Payload files should use #Extension plus their route type and useful scope tags when the file format is Open Forge-authored. Standard files such as `SKILL.md` should keep their runtime-required metadata. Do not use load-policy tags in extension payloads unless baseline loading is deliberately intended.

Shared behavior should have one canonical package owner. Put a shared skill in a skill-only extension, declare that extension from every dependent workflow, keep the graph acyclic, and ensure installed workflows name the concrete Required Routes they need. Identical-file deduplication is a safety boundary, not a reason to duplicate first-party payloads.

The CLI validates every manifest and existing receipt, then builds a complete source plan before writing. Manifest and payload paths are portable slash-separated relative paths; literal backslashes are rejected. Portable paths are Unicode-normalized and case-folded, and path segments also reject Windows-invalid characters, trailing dots or spaces, and reserved device basenames. Different bytes targeting the same portable path are a collision error, identical planned files are deduplicated, file-parent conflicts and existing portable aliases are rejected, and linked local or direct bundled package roots, target links, or multiply linked files cannot redirect writes. A managed package cannot adopt an existing unowned file, even when bytes match; an unmanaged overlay cannot replace a receipt-owned path. Shared managed files can update only when every existing owner participates and supplies the same bytes. The selected index tree is validated independently even for an empty or outside-`.agents` payload. Source roots and entries must be regular directories/files; links and special entries are rejected. Use `--dry-run` to inspect dependency order, scope counts, receipt changes, and every planned relative path with its create/update/delete/unchanged status without writes. Dry runs validate existing index shape but do not preview generated-index body changes. Normal installs also report counts for routed, baseline-loading, direct skill-script, and outside-`.agents` files.

Normal writes require recognizable tracked Core anchors and a clean target-scoped Git checkpoint, then ask for review and commit of the complete selected dependency closure. Planned paths, derived indexes, and `open-forge.extensions.json` must remain Git-visible; payloads may not write `.git/` or `.gitignore` and must leave repository-control changes to a separate review. Install roots separately when separate diffs matter. `--list` and `--dry-run` stay ungated; `--pro` bypasses only the Git/Core checkpoint lifecycle and never manifest, receipt, ownership, or installation-safety preflight.

Stable-id packages are recorded in transparent root `open-forge.extensions.json`, which owns payload digests, owner sets, dependency edges, and requested roots. Receipt `sha256` protects extension-authored bytes. CLI-owned generated `Entries` bodies may change through `index` or Core without invalidating entrypoint ownership. Reinstalling the same id reconciles changed or dropped owned content after verifying the existing hashes. `extend --remove` changes exactly the requested ids and blocks when a retained extension still depends on one. Updating or removing an owned entrypoint also blocks when it would strand retained routed descendants; move or remove them in the same plan, or keep another owner for the route host. Orphan dependencies are not pruned automatically. Idless overlays and directly installed skills remain unmanaged and are never adopted by `index`.

Payload writes, index regeneration, and receipt updates roll back together on handled in-process failures. There is no registry, network resolution, compatibility solver, migration lifecycle, automatic prune policy, persistent recovery journal, or crash recovery. Fast development tests own pure dependency-selection, portable-path, receipt, workflow/directive, and generated-index invariants. Slower closure/CI command-contract tests use fresh OS temporary roots, real CLI subprocesses and Git, then prove lifecycle, rollback, catalogue, packaged-layout, `doctor`, and Required Route behavior.

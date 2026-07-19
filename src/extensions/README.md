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
- `cli-testing-patterns` - pattern-only OS-temporary black-box CLI command testing
- `rune-bridge` - guidance and workspace routes for optional Rune-assisted relevance

Install only the capabilities that earn their context and maintenance cost. `open-forge extend --list` reads the shipped catalog, and dependency packs install automatically.

Most bundled extensions use this shape:

```text
{extension-id}/
  extension.json
  README.md
  payload/
    .agents/
      ...
```

A dependency-only convenience pack may omit `payload/`; it must declare at least one dependency. It remains an ordinary extension and installs only its dependency closure. This shape works by bundled id or from a copied local directory containing only `extension.json` and optional `README.md`.

The MVP CLI installs bundled extensions with:

```sh
open-forge extend [--dry-run] [--pro]
open-forge extend --list
open-forge extend --ids {extension-id},{extension-id}
open-forge extend {extension-id}
open-forge extend {extension-id} --dry-run
```

`extension.json` is optional for local payloads and required for every dependency-only pack. It may contain only non-empty string fields `name`, `description`, and `version`, plus a duplicate-free `dependencies` array of bundled extension ids; unknown fields are rejected so a misspelled dependency declaration cannot silently change installation. Dependencies may point to any bundled extension regardless of whether it contains skills, workflows, directives, mixed routes, or only more dependencies. They resolve transitively, offline, and before their dependents. Installed payload files remain runtime truth; agents never need the manifest.

Payload files should use #Extension plus their route type and useful scope tags when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` should keep native metadata. Do not use load-policy tags in extension payloads unless baseline loading is deliberately intended.

Shared behavior should have one canonical package owner. Put a shared native skill in a skill-only extension, declare that extension from every dependent workflow, keep the graph acyclic, and ensure installed workflows name the concrete Required Routes they need. Identical-file deduplication is a safety boundary, not a reason to duplicate first-party payloads.

The CLI validates every manifest and builds a complete source plan before writing. Portable paths are Unicode-normalized and case-folded; different bytes targeting the same portable path are a collision error, identical files are deduplicated, file-parent conflicts and existing portable aliases are rejected, and target links or multiply linked files cannot redirect writes. The selected index tree is validated independently even for an empty or outside-`.agents` payload. Local source roots and entries must be regular directories/files; links and special entries are rejected. Use `--dry-run` to inspect dependency order, scope counts, and every planned relative path with its create/update/unchanged status without writes. Dry runs validate existing index shape but do not preview generated-index body changes. Normal installs also report counts for routed, baseline-loading, direct skill-script, and outside-`.agents` files.

Normal writes require recognizable tracked Core anchors and a clean target-scoped Git checkpoint, then ask for review and commit of the complete selected dependency closure. Planned and derived-index paths must remain Git-visible; payloads may not write `.git/` or `.gitignore` and must leave repository-control changes to a separate review. Install roots separately when separate diffs matter. `--list` and `--dry-run` stay ungated; `--pro` bypasses only the Git/Core checkpoint lifecycle and never the installation-safety preflight.

Payload and index writes roll back on in-process installation failures. This is installation metadata, not a package manager lifecycle: there is no registry, network resolution, lock file, persistent ownership or recovery journal, version solver, update, remove, or migration yet. First-party command-contract tests use a fresh OS temporary root, invoke the real CLI subprocess, use real Git for lifecycle behavior, assert exit/output/filesystem/Git effects plus no partial writes on rejection, then run `doctor` and prove workflow Required Routes resolve.

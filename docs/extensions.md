# Open Forge Extensions

Extensions are the content-agnostic installable unit for optional Open Forge material.

One extension may contain a single native skill, a single workflow, directives, patterns, guidance, workspace or memory routes, support files, or a deliberate mix. A dependency-only extension is a convenience pack over other extensions. “Extension” describes installation and composition, never the runtime meaning of its files.

The bundled catalogue separates skill-only architecture, vision, planning, implementation, and quality capabilities from the workflows that require them. Architecture, vision, brainstorming, implementation, testing, and the mixed development cycle are individually selectable. Broader workflow, planning, quality, and design packs remain available for convenience. Run `open-forge extend --list` for the installed package's exact ids, derived contents, descriptions, and dependency hints.

They do not create another framework root. They add #Core, #Memory, or #Extension material where those files naturally belong.

## Source Shapes

A local overlay is copied as-is:

```text
my-extension/
  .agents/
    patterns/
      react/
        _react.md
        components.md
```

A local or bundled extension package can keep maintainer metadata outside the installed payload:

```text
my-extension/
  extension.json
  README.md
  payload/
    .agents/
      skills/
        implementation/
          SKILL.md
```

When `payload/` exists, only `payload/` is installed.

A dependency-only pack may omit `payload/`:

```text
workflow-essentials/
  extension.json
  README.md
```

Its manifest must declare at least one dependency. It writes no runtime file of its own and installs only the resolved dependency closure. The same shape works from the bundled catalogue or from a copied local directory when the only package files are `extension.json` and an optional `README.md`. A local directory with any other root file and no `payload/` retains direct-overlay semantics.

Payload-bearing bundled first-party extensions use:

```text
src/extensions/{extension-id}/
  extension.json
  README.md
  payload/
    .agents/
      ...
```

`extension.json` is optional install-time metadata for payload-bearing packages and required for dependency-only packs. Installed payload files remain runtime truth. Unknown manifest fields are rejected rather than ignored.

Extension payload files should use #Extension plus their route type and useful scope tags when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` should keep native metadata and may be indexed with default route tags. Use a reserved load-policy tag only when the extension intentionally adds baseline-loaded material.

## Install

```sh
open-forge extend [--dry-run]
open-forge extend --list
open-forge extend --select [target] [--dry-run]
open-forge extend --ids <id[,id...]> [target] [--dry-run]
open-forge extend <extension-source-or-id> [target] [--dry-run]
```

The CLI resolves a local folder first. If no local folder exists and the value is a valid bundled id, it installs the bundled first-party extension.

Interactive selection requires a TTY. It shows the complete catalogue with payload contents and direct dependencies. Selecting an extension immediately marks its transitive dependencies as required. A dependency cannot be toggled off while another direct selection requires it; it becomes optional when no selected dependent needs it. A dependency that the user also selected directly remains selected after its former dependent is removed. Use `--ids` for scripts, CI, or unattended installs; the same dependency closure is resolved before installation.

`--dry-run` can appear anywhere in an extension-install form. It resolves and validates the same source and dependency closure, prints dependency order plus create/update/unchanged counts, lists every planned relative path with its status, and writes nothing. It also validates existing generated-index markers/layout and entrypoint ambiguity that can be determined without applying the payload, but it does not preview generated-index body changes or count generated index writes. It cannot be combined with `--list`.

Both dry runs and normal installs print scope counts for normally routed files; baseline-loading files (`AGENTS.md`, both loader forms, and direct Markdown files in `.agents/directives/`); files in a native skill package's direct `scripts/` subtree; and files outside `.agents/`. A dry run exposes the paths behind those counts so broader or executable effects are visible before installation.

## Manifest

A package may contain `extension.json` beside `payload/`:

```json
{
  "name": "Development Workflow",
  "description": "Contract-first development workflow",
  "version": "0.1.0",
  "dependencies": ["implementation-capability"]
}
```

All fields are optional for payload-bearing packages. `name`, `description`, and `version` must be non-empty strings when present. `dependencies` must be a duplicate-free array of lowercase bundled extension ids. A dependency-only extension without `payload/` must have a manifest and at least one dependency.

The CLI validates manifests before installing. It rejects malformed JSON, invalid field types or ids, duplicate dependencies, missing bundled dependencies, and dependency cycles. `version` is descriptive metadata only; there is no version solver or compatibility negotiation.

Dependencies resolve transitively and offline from the bundled first-party extensions shipped with the CLI. A dependency id may identify any extension shape: skill-only, workflow-only, directives-only, mixed, or dependency-only. Dependencies install before their dependents and are deduplicated across all requested roots. A local package or a direct local overlay may name bundled dependencies, but dependency values never resolve to arbitrary local paths.

Manifests help the current install command plan files. They are not copied from package-shaped extensions, are not required for routing, and do not become runtime state. Agents use the installed payload files as truth.

## Preflight And Collisions

The CLI builds the complete source plan before writing anything.

- Two resolved extensions providing the same path with different bytes cause an error.
- Identical files at the same path are deduplicated.
- Relative paths use a portable Unicode-normalized, case-folded identity, so collisions within the plan and aliases already present in the target are rejected on every host.
- A planned file cannot also be another planned file's parent.
- Sources and targets must remain lexically and physically separate. A linked target root, symbolic links or junctions below it, and multiply linked files that may be rewritten are rejected; real-path projection also prevents an aliased target ancestor from redirecting installation back into the extension package. The selected index tree is checked independently even when a payload is empty or writes only outside `.agents/`.
- Local source roots and payload entries must be regular directories/files; symbolic links, junctions, and special entries are rejected instead of followed or silently omitted.
- Existing target files are classified as create, update, or unchanged.
- Matching marked blocks in existing Markdown targets are preserved.
- Indexes rebuild once after the complete plan is written.

This preflight prevents ambiguous source composition. Payload writes and generated-index writes also roll back within the running process: overwritten files are restored, files created by the failed attempt are removed, newly created empty directories are cleaned up, and partially written index plans are restored. There is no persistent transaction journal or recovery after an abrupt process or machine failure.

## Sharing And Packs

Avoid duplicating shared workflow or skill behavior. Give every first-party installed path one canonical package owner. Put a shared native skill in a skill-only extension and declare its id in each dependent workflow extension. Keep dependency graphs small, acyclic, and meaningful at the file-contract level.

An extension pack is not a second package kind. It is an ordinary dependency-only extension that selects a useful group. This lets users choose one narrow workflow or a broader pack without duplicating runtime routes. Technology and role specialization should normally enter as additional skill extensions; generic architecture, planning, implementation, testing, and delivery workflows should not be forked merely to create frontend, backend, game, or product variants.

Dependencies are installation convenience, not runtime indirection. Workflows still name concrete Required Routes, and those installed files must be sufficient for an agent with no manifest access.

## Native Skill Interoperability

Open Forge does not require its extension installer to own every skill. Native skill packages may be copied or installed directly under `.agents/skills/{skill-name}/`; run `open-forge index` afterward so the shared skills route lists the new package, then run `open-forge doctor` to validate routing.

This layout is compatible with Microsoft APM's Agent Skills support. APM accepts root `SKILL.md`, skill bundles, and `.apm/skills/{name}/SKILL.md` sources, and deploys cross-tool skills to `.agents/skills/`. Use APM when its remote sources, manifest, pre-deploy hidden-Unicode scan, lockfile, update, or removal lifecycle is wanted; use an Open Forge extension when local catalogue composition and Open Forge preflight are wanted. A skill may also be wrapped without modification at `payload/.agents/skills/{skill-name}/` inside a local extension.

Do not assign the same installed skill path to both managers. APM records deployed-file ownership and hashes in its lockfile, while the current Open Forge installer intentionally has no persistent ownership receipt. `open-forge index` changes generated route regions, not the skill package contents, so it is the safe handoff after an external skill install.

Official APM references: [package anatomy](https://microsoft.github.io/apm/concepts/package-anatomy/), [skills authoring](https://microsoft.github.io/apm/producer/author-primitives/skills/), and [package installation](https://microsoft.github.io/apm/consumer/install-packages/).

## Lifecycle Boundary

Direct overlays are unmanaged: they add or replace files but create no ownership record. Package manifests are also install-time only. The current CLI does not provide a registry, network fetch, lock file, persistent install receipt or recovery journal, version selection, update, remove, or migration lifecycle. Reinstalling can create, update, or leave files unchanged, but it does not delete files that disappeared from a package.

## Tests

CLI extension tests install into OS temp folders and inspect the resulting files and generated indexes. First-party integration tests use the real bundled packages, install each advertised extension over a fresh core workspace, run `doctor`, and verify every workflow Required Route resolves. Packaged-layout tests bundle the real CLI and exercise both adjacent `dist/extensions/` and npm-style `src/extensions/` resolution. A direct-install fixture proves a native multiline-frontmatter skill is indexed without an Open Forge wrapper.

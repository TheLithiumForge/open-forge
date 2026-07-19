# Open Forge Extensions

Extensions are the content-agnostic installable unit for optional Open Forge material.

One extension may contain a single native skill, a single workflow, directives, patterns, guidance, workspace or memory routes, support files, augmentations, or a deliberate mix. A dependency-only extension is a convenience pack over other extensions. “Extension” describes installation and composition, never the runtime meaning of its files.

The bundled catalogue separates skill-only architecture, vision, planning, implementation, and quality capabilities from the workflows that require them. Architecture, vision, brainstorming, implementation, testing, and the mixed development cycle are individually selectable. Broader workflow, planning, quality, and design packs remain available for convenience. Run `open-forge extend --list` for the installed package's exact ids, derived contents, descriptions, and dependency hints.

They do not create another framework root. They add #Core, #Memory, or #Extension material where those files naturally belong.

## Source Shapes

An idless local overlay is copied as-is and remains unmanaged:

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

When `payload/` exists, only `payload/` is installed. Give a local source a stable manifest `id` when Open Forge should manage its files across reinstall, update, and removal. An id-bearing local source opts into that receipt-backed lifecycle even when it uses direct-overlay layout; `payload/` remains the clearest package boundary.

A dependency-only or augmentation-only package may omit `payload/`:

```text
workflow-essentials/
  extension.json
  README.md
```

Its manifest must declare at least one dependency or augmentation. It may write no standalone runtime file: a dependency-only pack installs the resolved dependency closure, while an augmentation-only package changes only an explicit owned slot in an existing Markdown target. The same shape works from the bundled catalogue or from a copied local directory when the only package files are `extension.json`, an optional `README.md`, and declared augmentation fragments. A copied or otherwise local source that declares dependencies or augmentations must also declare a stable `id`; the bundled catalogue supplies identity for bundled packages.

A local directory with other root files and no `payload/` retains direct-overlay layout. Without a manifest `id`, those files are unmanaged. With a stable `id`, Open Forge records and manages them.

Payload-bearing bundled first-party extensions use:

```text
src/extensions/{extension-id}/
  extension.json
  README.md
  payload/
    .agents/
      ...
```

`extension.json` is optional install-time metadata only for an idless plain payload or overlay with no dependencies or augmentations. A local package needs it for managed ownership and must declare a stable `id` when it declares either dependencies or augmentations. Bundled packages receive their stable id from the catalogue folder; a declared manifest `id`, when present, must match. Installed Markdown and native files remain runtime truth. Unknown manifest fields are rejected rather than ignored.

Extension payload files should use #Extension plus their route type and useful scope tags when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` should keep native metadata and may be indexed with default route tags. Use a reserved load-policy tag only when the extension intentionally adds baseline-loaded material.

## Install

```sh
open-forge extend [--dry-run] [--pro]
open-forge extend --list
open-forge extend --select [target] [--dry-run] [--pro]
open-forge extend --ids <id[,id...]> [target] [--dry-run] [--pro]
open-forge extend --remove <id[,id...]> [target] [--dry-run] [--pro]
open-forge extend <extension-source-or-id> [target] [--dry-run] [--pro]
```

The CLI resolves a local folder first. If no local folder exists and the value is a valid bundled id, it installs the bundled first-party extension.

Interactive selection requires a TTY. It shows the complete catalogue with payload contents and direct dependencies. Selecting an extension immediately marks its transitive dependencies as required. A dependency cannot be toggled off while another direct selection requires it; it becomes optional when no selected dependent needs it. A dependency that the user also selected directly remains selected after its former dependent is removed. Use `--ids` for scripts, CI, or unattended installs; the same dependency closure is resolved before installation.

`--dry-run` can appear anywhere in an extension install or removal form. Installation previews resolve and validate the same source and dependency closure, print dependency order plus create/update/delete/unchanged counts, and list every planned relative path. Removal previews print the owned block, file, and receipt updates for the explicitly requested ids. Both write nothing. They also validate existing generated-index markers/layout and entrypoint ambiguity that can be determined without applying the plan, but they do not preview generated-index body changes or count generated index writes. `--dry-run` cannot be combined with `--list`.

Extension installation dry runs and normal installs print scope counts for normally routed files; baseline-loading files (`AGENTS.md`, both loader forms, and direct Markdown files in `.agents/directives/`); files in a native skill package's direct `scripts/` subtree; and files outside `.agents/`. An installation dry run exposes the paths behind those counts so broader or executable effects are visible before installation.

Normal install, update, and removal writes are checkpointed. Recognizable Core anchors must already be installed and tracked, and the target scope must be clean in its containing Git repository. Outside Git, an interactive terminal recommends `git init` and requires explicit approval; non-interactive writes stop without mutation. Ignored planned, receipt, or derived-index paths are rejected because the resulting change could not be reviewed or committed. One install command owns one complete selected dependency closure; one removal command owns exactly its requested ids. Each then asks the user to review and commit before another mutation. Run roots separately for separate diffs; multi-select or `--ids` deliberately combines them into one review unit.

`--list` and `--dry-run` are read-only and need neither Core nor a clean checkpoint. `--pro` intentionally bypasses only the Git/Core lifecycle guard for expert workflows; strict manifests and receipts, dependency closure, containment, portable collision checks, link protection, ownership checks, index validation, local-block preservation, and rollback remain mandatory.

## Manifest

A package may contain `extension.json` beside `payload/`:

```json
{
  "id": "development-workflow",
  "name": "Development Workflow",
  "description": "Contract-first development workflow",
  "version": "0.1.0",
  "dependencies": ["implementation-capability"],
  "augmentations": [
    {
      "target": ".agents/loader.md",
      "slot": "workflow-selection",
      "source": "augmentations/workflow-selection.md"
    }
  ]
}
```

All fields are optional only for an idless plain payload or overlay with no dependencies or augmentations. `id` is a lowercase extension id and opts a local source into the managed lifecycle; a local source declaring dependencies or augmentations requires one. Bundled packages already have their catalogue id, and a manifest `id`, when supplied, must match it. `name`, `description`, and `version` must be non-empty strings when present. `dependencies` must be a duplicate-free array of lowercase bundled extension ids. `augmentations` must be a duplicate-free array by portable target path plus slot. A package without `payload/` must declare at least one dependency or augmentation.

Each augmentation has exactly `target`, `slot`, and `source`. Target, source, and payload paths are portable slash-separated relative paths; literal backslashes are rejected rather than interpreted as separators. The target must be an existing base Markdown file, never an `.overwrite.md` file or `open-forge.extensions.json`; the source must be a non-empty Markdown fragment inside the package. Slot ids are lowercase and may contain letters, digits, dots, and hyphens. A package may declare a target-slot pair only once, and fragments cannot contain reserved augmentation markers. A declared fragment is excluded from payload copying even when it lives below `payload/`; its only runtime form is the owned materialized block.

The CLI validates manifests before installing. It rejects malformed JSON, unknown fields, invalid field types or ids, duplicate dependencies or augmentations, missing bundled dependencies, and dependency cycles. `version` is descriptive metadata only; there is no version solver or compatibility negotiation.

Dependencies resolve transitively and offline from the bundled first-party extensions shipped with the CLI. A dependency id may identify any extension shape: skill-only, workflow-only, directives-only, mixed, or dependency-only. Dependencies install before their dependents and are deduplicated across all requested roots. A local package or a direct local overlay may name bundled dependencies, but dependency values never resolve to arbitrary local paths.

Manifests help the CLI plan an installation and, with stable identity, own it. They are not copied from package-shaped extensions, are not required for routing, and do not become agent runtime truth. Agents use the installed payload files and materialized augmentation blocks as truth.

## Augmentation Slots

Augmentation is the explicit exception for an extension that needs to contribute to a shared file which is already loaded. The target file owns a named empty slot:

```md
<!-- open-forge-augment.workflow-selection:start -->
<!-- open-forge-augment.workflow-selection:end -->
```

The installer materializes one block per extension inside that slot:

```md
<!-- open-forge-extension.example-workflow:start -->
- Apply the extension-specific behavior supplied for this loading point.
<!-- open-forge-extension.example-workflow:end -->
```

Blocks are sorted by extension id, so install order does not change the file. A slot may contain only whitespace and owned extension blocks; manual prose inside it is rejected. A block may appear only inside one declared slot. The extension fragment is not installed as a separate file and never creates another route or agent loading decision.

A managed payload may ship a base Markdown file with empty augmentation slots so later extensions can opt in. It may not ship pre-owned extension blocks, malformed markers, nested or duplicate slots, or unowned prose inside a slot; ownership begins only when the installer materializes a declared augmentation.

Use augmentation only when a standalone route cannot provide the needed behavior at the correct loading point. The installer never guesses a heading, scans for semantic insertion points, or merges unmarked prose. A managed extension may not own an `.overwrite.md` path merely to modify shared behavior.

The precedence is:

1. base file, including materialized extension-owned blocks
2. workspace-owned `{name}.overwrite.md`, loaded after the base

That keeps extension contribution removable while preserving the workspace's final local choice.

## Preflight And Collisions

The CLI builds the complete source plan before writing anything.

- Two resolved extensions providing the same path with different bytes cause an error.
- Identical files at the same path are deduplicated within a plan. Receipt-managed packages may share that file only when the bytes match and all owners are recorded.
- A managed package never silently adopts an existing unowned target, even when its bytes match. Use an explicit augmentation slot, remove the unowned path after review, or keep the source unmanaged.
- An unmanaged overlay cannot replace a receipt-owned path. Update or remove the owning managed extension explicitly first.
- Updating a shared managed file requires every existing owner to participate in the same plan and provide the same new bytes.
- Relative paths use a portable Unicode-normalized, case-folded identity, so collisions within the plan and aliases already present in the target are rejected on every host. Manifest and payload paths must be slash-separated and also reject literal backslashes, Windows-invalid characters, trailing dots or spaces, and reserved device basenames.
- A planned file cannot also be another planned file's parent.
- Sources and targets must remain lexically and physically separate. A linked target root, symbolic links or junctions below it, and multiply linked files that may be rewritten are rejected; real-path projection also prevents an aliased target ancestor from redirecting installation back into the extension package. The selected index tree is checked independently even when a payload is empty or writes only outside `.agents/`.
- Local source roots, direct bundled catalogue package roots, and payload entries must be regular directories/files; symbolic links, junctions, and special entries are rejected instead of followed or silently omitted.
- Extension payloads cannot contain `.git` as any path segment or write any `.gitignore`; those controls could mutate repository state or hide the same transaction and must be applied as a separate reviewed change.
- Planned target effects are classified as create, update, delete, or unchanged.
- Matching marked blocks in existing Markdown targets are preserved.
- Indexes rebuild once after the complete plan is written.

Before any managed install, update, or removal, the CLI validates receipt consistency and verifies the recorded digest of every owned file and augmentation block. Receipt `sha256` protects extension-authored bytes; a bounded generated `Entries` body is CLI-owned and may change through `index` or Core without invalidating ownership of its entrypoint. The CLI then validates every retained augmentation block against the complete planned final state, including changes to a block's target from another payload in the same transaction. Local modification, deletion, missing ownership reciprocity, or a malformed or destructive final plan stops the operation rather than guessing how to recover.

Core reinstall follows the same ownership boundary. It validates the existing receipt, refuses to modify or remove a receipt-owned extension file, and rejects a Core plan that would remove an occupied augmentation slot or alter a retained owned block. Managed extension lifecycle commands, not Core reinstall or an unmanaged overlay, own those changes.

This preflight prevents ambiguous source composition. Payload and augmentation changes, generated-index changes, and the ownership receipt are applied as one in-process transaction. A handled failure restores overwritten files and blocks, removes files created by the attempt, cleans up newly created empty directories, restores index files, and restores the previous receipt. There is no persistent transaction journal or automatic recovery after an abrupt process or machine failure; Git remains the durable recovery boundary.

## Managed Lifecycle And Receipt

A stable extension id is the ownership key. Bundled catalogue entries always have one. A local source opts in by declaring `id` in `extension.json`; an idless local overlay remains unowned by Open Forge.

Managed state is recorded at the target root in transparent, Git-visible `open-forge.extensions.json`. The schema records:

- explicitly requested root ids
- every installed extension's version, dependency ids, payload paths, and augmentation target-slot digests
- each owned payload path's SHA-256 digest and complete owner list

This file is CLI state, not agent context. Do not route or load it. Installed Markdown and native files remain sufficient runtime truth. The receipt is reserved from payload and augmentation targets and disappears when no managed state remains.

Reinstalling the same managed id is an update. The CLI reconciles its current manifest: changed owned files and blocks update, paths or augmentations removed from the package are released, and an unmodified stale path is deleted when no owner or augmentation still needs it. An update that removes an owned entrypoint as a route host is blocked if retained descendants would become unreachable; move or remove them in the same plan, or keep another owner for that host. All ownership and digest safeguards still apply.

Removal is explicit and conservative:

```sh
open-forge extend --remove vision-workflow
open-forge extend --remove vision-workflow,vision-capability ./my-project --dry-run
```

- Only the requested installed ids are removed.
- Removal stops if an installed extension that is being retained still depends on a requested id.
- An owned block or file is changed only when its recorded content still matches.
- A shared file remains while any owner remains; a file that still carries another extension block cannot be deleted.
- An owned entrypoint remains while retained routed descendants still need it; move or remove them in the same plan, or keep another owner for that route host.
- Required dependencies that become installed orphans are not pruned automatically. Remove them explicitly after a dry run when that is the intended review unit.

An eventual `--prune` mode would need an explicit previewable root and legacy-package policy. Do not infer that behavior from the interactive selector: selector requirements are pre-install choice state, while installed ownership is governed by the receipt.

## Sharing And Packs

Avoid duplicating shared workflow or skill behavior. Give every first-party installed path one canonical package owner. Put a shared native skill in a skill-only extension and declare its id in each dependent workflow extension. Keep dependency graphs small, acyclic, and meaningful at the file-contract level.

An extension pack is not a second package kind. It is an ordinary dependency-only extension that selects a useful group. This lets users choose one narrow workflow or a broader pack without duplicating runtime routes. Technology and role specialization should normally enter as additional skill extensions; generic architecture, planning, implementation, testing, and delivery workflows should not be forked merely to create frontend, backend, game, or product variants.

Dependencies are installation convenience, not runtime indirection. Workflows still name concrete Required Routes, and those installed files must be sufficient for an agent with no manifest access.

## Native Skill Interoperability

Open Forge does not require its extension installer to own every skill. Native skill packages may be copied or installed directly under `.agents/skills/{skill-name}/`; run `open-forge index` afterward so the shared skills route lists the new package, then run `open-forge doctor` to validate routing.

This layout is compatible with Microsoft APM's Agent Skills support. APM accepts root `SKILL.md`, skill bundles, and `.apm/skills/{name}/SKILL.md` sources, and deploys cross-tool skills to `.agents/skills/`. Use APM when its remote sources, manifest, pre-deploy hidden-Unicode scan, lockfile, update, or removal lifecycle is wanted; use an Open Forge extension when local catalogue composition and Open Forge preflight are wanted. A skill may also be wrapped without modification at `payload/.agents/skills/{skill-name}/` inside a local extension.

Do not assign the same installed skill path to both managers. APM records its deployed-file ownership and hashes in its own lockfile; Open Forge records only stable-id packages in `open-forge.extensions.json`. A direct/APM skill install stays outside that receipt, and `open-forge index` changes generated route regions without adopting or modifying the skill package, so it is the safe handoff after an external install.

Current interoperability is additive, not capability substitution. A bundled workflow dependency still resolves its named Open Forge capability extension; an unrelated direct/APM skill at a distinct route can satisfy a local workflow Required Route and remains byte-unchanged, but an externally owned skill does not automatically satisfy or replace a first-party extension id. Today, use a local workflow/Required Route for the external skill and keep paths distinct. Future stronger options are an explicit reviewed `extension-id=route` satisfaction map or manifest `provides`/`requires` plus ownership receipts.

Official APM references: [package anatomy](https://microsoft.github.io/apm/concepts/package-anatomy/), [skills authoring](https://microsoft.github.io/apm/producer/author-primitives/skills/), and [package installation](https://microsoft.github.io/apm/consumer/install-packages/).

## Lifecycle Boundary

Idless plain overlays and direct/APM skill installs are unmanaged: they add files outside Open Forge ownership, and later `index` calls do not adopt them. An idless local source cannot declare dependencies or augmentations. Any local source that deliberately declares a stable manifest `id` opts into the receipt-managed lifecycle, including a source that uses direct-overlay layout.

The current CLI provides local install, receipt-backed update, and explicit removal. It does not provide a registry or network fetch, remote trust policy, compatibility or version solving, migration hooks, automatic orphan pruning, a persistent transaction journal, or crash recovery. Keep `open-forge.extensions.json` in Git, preview lifecycle changes with `--dry-run`, and review each resulting diff.

## Tests

Fast development tests cover pure dependency selection, portable-path, augmentation-slot, receipt, workflow/directive, and generated-index invariants without creating repositories. Closure/CI tests own the slower evidence: fresh OS temporary folders, real CLI subprocesses and Git checkpoints, install/update/remove and rollback effects, first-party catalogue integration, packaged layouts, and Required Route validation. Structural file assertions are used only when they prove a packaging, routing, ownership, or transaction boundary. The optional `cli-testing-patterns` extension makes the black-box command shape reusable in installed workspaces.

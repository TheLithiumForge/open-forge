# Open Forge Extensions

Extensions add optional whole files through existing Open Forge routes.

An Extension may provide any combination of Skills, Workflows, Directives, Patterns, Guidance, Templates, Map `routes`, Memory `routes`, and support files. A dependency-only pack may provide only dependency edges.

The term Extension describes optional packaging and managed ownership, not runtime meaning. Installed files retain the meaning of their destination routes and remain understandable and usable without the package manifest, catalogue, ownership receipt, or CLI.

## New CLI contract (not shipping)

The new native CLI is not released. Its accepted Extension surface is grouped
under `extension` and has six actual operations:

```text
open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global flags]
open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]
open-forge extension create [<stable-id>] [--path <catalogue-path>] [--automatic] [--dry-run] [--skip-git-check] [global flags]
open-forge extension install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [--skip-git-check] [global flags]
open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]
open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]
```

The bare group shows help and performs no operation or wizard. `list` and
`inspect` are read-only. `create` writes only
`<catalogue>/<id>/extension.json` and `payload/.agents/` under its catalogue
destination. Its `--path` is not a package source, and the shared `--workspace`
flag is a no-op for create.

Install and update use one exact embedded or explicitly selected local package or
catalogue source. An external source is read-only and must be lexically and
physically disjoint from the target workspace. Dependencies resolve offline,
transitively, and only within that source universe. A multi-package source
requires explicit IDs or `--all`; `--automatic` never means `--all`.

Install establishes managed ownership for selected absent IDs or verifies an
exact managed no-op. Managed divergence directs to `extension update`, and
initial `--force` may replace only an eligible exact current occupant without
adopting its old bytes. Update requires trusted installed lifecycle state and a
trustworthy Framework anchor. Normal update preserves changed, missing, and
retired divergence; `--force` handles only changed or missing current expected
paths; `--prune` handles only eligible retired content.

Remove does not need package source bytes. It releases explicit trusted
ownership, retains shared files, deletes safe unchanged final-owner files, and
preserves changed final-owner files as unmanaged unless same-request `--prune`
selects the eligible Delete boundary. Automatic mode adds no force, prune,
ownership, or deletion authority. Manual, idless, and direct-overlay content
remains unmanaged.

Installed and available facts remain separate. New-CLI installed facts are
classified as `absent`, `trusted`, `untrusted`, or `incomplete`; unsafe ambiguity
is `blocked` under the contracts. Safely readable installed facts remain visible
to read-only operations when the package source is unavailable, but source
unavailability cannot form an update plan or promote lifecycle trust. The only
lifecycle document is `.agents/open-forge.lifecycle.json`, schema v1, with
isolated `framework` and `extensions` sections. It does not merge their
authority. Supported parseable kinds use syntax-aware semantic fingerprints,
while exact bytes remain fresh operation-time facts. The replacement executes no
formatter and persists no formatter state.

The replacement does not read, recognize, migrate, alias, or fall back to an old
lifecycle or Extension file, including `open-forge.extensions.json`. Old-format
files remain ordinary untouched workspace content outside replacement authority.
The accepted parser, serialization, filesystem, recovery, and Native AOT choices
are defined in the [CLI Architecture](../.agents/memory/crystallized/documents/cli/architecture.md).

These are accepted non-shipping replacement contracts, not runtime or release
evidence.
See the [CLI command contracts](cli.md#extension-operations) for the public
overview and the routed [Extension contract group](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md)
for complete Interface and Behavior contracts.

## Frozen executable: `open-forge-old`

The commands in the remainder of this section document the frozen TypeScript
MVP executable only. `open-forge-old` remains the available executable while the
new CLI is non-shipping. Its `extend` and `create` syntax does not define the new
`extension` group.

The receipt commands and receipt facts in this frozen section describe
`open-forge-old`'s historical `open-forge.extensions.json` behavior only. They do
not describe lifecycle state, migration, or compatibility for the replacement
CLI.

Install and commit the standard Framework before installing an Extension. Follow the [README Quick Start](../README.md#quick-start) first. Catalogue listing and previews are read-only, but a normal Extension installation expects recognizable tracked Core anchors.

List the bundled catalogue:

```sh
npx open-forge-old extend --list
```

Preview one Extension and its dependencies:

```sh
npx open-forge-old extend development-toolkit --dry-run
```

Install the reviewed plan:

```sh
npx open-forge-old extend development-toolkit
```

Review and commit the resulting files:

```sh
git status
git diff
git add .agents open-forge.extensions.json
git commit -m "Add development toolkit"
```

The receipt is created only for managed packages. Review the actual changed paths reported by Git rather than assuming every installation changes both paths above.

## How Extensions Fit Open Forge

An Extension is a content-agnostic package, not another Framework primitive or root.

For example, the current first-party Extension installs:

```text
.agents/
  workflows/
    architecture.md
    debugging.md
    development.md
    planning.md
    review.md
    vision.md
  skills/
    experience-design/
      SKILL.md
      references/
        ...
  templates/
    documents/
      ...
    memory/
      ...
```

After installation:

- Each Workflow is selected and used as a Workflow
- The Skill follows its native `SKILL.md` contract
- The Templates provide copy-ready starting content
- Generated `Entries` expose the installed routes
- #Extension records optional package provenance without creating authority or loading behavior

Package metadata ensures the complete selected unit arrives together. Installed files still express runtime relationships through ordinary links and route meaning.

## Choose And Install

Use interactive selection:

```sh
npx open-forge-old extend
npx open-forge-old extend --select
```

Install the bundled toolkit as one review unit:

```sh
npx open-forge-old extend development-toolkit
```

Install a local package or direct overlay:

```sh
npx open-forge-old extend ./my-extension
```

One invocation and its complete dependency closure form one review unit. Install roots in separate commands when separate diffs matter.

Normal writes require recognizable tracked Open Forge Core anchors, a clean target scope, and Git-visible planned output. Outside Git, interactive use asks for approval after recommending initialization; non-interactive writes stop.

`--pro` bypasses only the Git and Core lifecycle guards. It never bypasses manifest, dependency, containment, collision, ownership, route-integrity, or rollback safety.

## Preview, Update, And Remove

Preview an installation:

```sh
npx open-forge-old extend development-toolkit --dry-run
```

An installation preview performs package discovery, dependency resolution, and complete preflight, then summarizes planned file and receipt effects without writing.

Preview a removal:

```sh
npx open-forge-old extend --remove development-toolkit --dry-run
```

A removal preview reads installed receipt state, validates owned files and retained dependents, and summarizes the removal plan without writing.

Use `open-forge-old --help` for the frozen legacy syntax. Its output does not define the new CLI.

Reinstall a managed id to reconcile its owned files:

```sh
npx open-forge-old extend development-toolkit
```

Remove explicitly selected ids:

```sh
npx open-forge-old extend --remove development-toolkit
```

Removal stops when a retained Extension still depends on the selected id, an owned file has changed, or deleting an entrypoint would strand retained routes. When another installed Extension shares an identical owned file, removal drops only the selected owner and retains the file. Dependencies that become orphans are not pruned automatically. Preview and remove them explicitly when desired.

## Pre-Release Catalogue Reset

The first-party catalogue was consolidated before a stable release into the single `development-toolkit` package. Earlier package identities are not aliases and are not current installation or update sources.

Files already installed from an earlier package remain ordinary usable workspace files. Their existing ownership receipt is sufficient for previewing and removing that installed identity without its original source package. Review or remove those files explicitly, then install `development-toolkit` when the new package is wanted.

The toolkit is intentionally installed as one small unit. The current CLI does not select individual package features. Users own the installed files and may remove routes that provide no local value after reviewing the resulting route tree.

## Manual Installation

The CLI is optional.

To install manually:

1. Copy the package's `payload/` contents into the target workspace
2. Rebuild or manually update each affected generated `Entries` region
3. Review the assembled files and their links

The installed files then work like any other Open Forge files. Manual installation does not create managed update or removal state.

## Create A Local Extension

Create a managed package scaffold:

```sh
npx open-forge create extension my-extension {package-parent}
```

This creates `{package-parent}/my-extension/` with:

```text
my-extension/
  extension.json
  README.md
  payload/
    .agents/
```

Then:

1. Replace the manifest and README placeholders
2. Add complete routed files and any required entrypoints beneath `payload/.agents/`
3. Author ordinary links for their final containing-file-relative locations
4. Preview installation into a separate Open Forge workspace
5. Install, run `open-forge-old doctor` against the assembled workspace, and review the Git diff

```sh
npx open-forge-old extend {package-parent}/my-extension {target-workspace} --dry-run
npx open-forge-old extend {package-parent}/my-extension {target-workspace}
npx open-forge-old doctor {target-workspace}
```

Using a separate target workspace keeps package sources distinct from installed output. The target must not be the package source or a directory inside it.

## Package Shapes

The current first-party package lives directly beneath the source catalogue:

```text
src/extensions/
  development-toolkit/
    extension.json
    README.md
    payload/
```

A normal managed package contains:

```text
{package-folder}/
  extension.json
  README.md
  payload/
    .agents/
      ... complete target-relative files ...
```

A dependency-only pack may omit `payload/`:

```text
{package-folder}/
  extension.json
  README.md
```

It selects a dependency closure without installing a placeholder runtime file.

A local source may use:

- The complete package shape
- A directory containing `payload/`
- A direct overlay shaped like the target workspace

A local source with a manifest id opts into managed lifecycle. An idless payload or direct overlay remains unmanaged.

Source folders help catalogue presentation and maintenance. The manifest id defines managed package identity. Installed files receive runtime meaning from their destination routes and contents.

## Manifest

Managed packages use `extension.json`:

```json
{
  "id": "example-workflow",
  "name": "Example Workflow",
  "description": "Adds an example routed workflow",
  "version": "1.0.0",
  "dependencies": ["example-skill"]
}
```

- `id` is the stable lowercase install and ownership key, independent of source location
- `name`, `description`, and `version` are optional display metadata and must be non-empty strings when present
- `version` is descriptive and has no compatibility semantics in the MVP
- `dependencies` is an optional duplicate-free list of bundled Extension ids

Every bundled package declares an id. A local source may declare one to opt into managed installation, reconciliation, and removal. A dependency-only package must declare at least one dependency.

Unknown fields, invalid ids, duplicate dependencies, missing bundled dependencies, and dependency cycles stop installation before any write.

The manifest is install metadata. It is not copied into agent context.

## Payload And Routing

Payload paths are portable and relative to the target.

Open Forge-authored routed files normally carry:

- #Extension
- Their Core primitive or Memory classification
- Useful scope and topic tags

A complete Workflow payload uses level-2 `Goal`, `Steps`, and `Completion` sections in that order. Recipe-specific headings may organize details without expanding the Framework schema. Use ordinary links and explicit Steps when another installed source must be read or invoked.

Standard formats such as `SKILL.md` retain their native metadata. Use #LoadNow or #KeepInMind only when the installed file deliberately belongs in baseline or continuity loading.

Markdown links resolve relative to their containing file in the assembled workspace. Links within one package should resolve in the isolated payload. Links to Core or declared dependencies may resolve only after the complete package closure is assembled.

Do not duplicate dependency files or add source-only stubs merely to make a cross-package link resolve in isolation.

Validate cross-package links after assembly because an isolated payload does not contain its declared package dependencies.

## Dependencies, Packs, And Skills

Dependencies:

- Use stable bundled ids
- Resolve transitively and offline
- Install before their dependents
- Are deduplicated across the selected closure

Put reusable content in one canonical package. A Workflow that depends on a Skill package links to the concrete installed `SKILL.md` through an ordinary link and states in its Steps when the Skill must be used. Manifest dependencies remain installation metadata rather than runtime context.

Convenience packs should contain dependency edges instead of duplicated payloads. Keep dependency graphs small and acyclic.

Open Forge does not need to manage every Skill. An ordinary Skill may be copied or installed directly under `.agents/skills/{skill-name}/`. Run `open-forge index` afterward or update the Skills `Entries` manually.

Do not assign the same installed Skill path to two package managers. Routing an external Skill does not transfer ownership or rewrite its internals.

## Managed Ownership And Safety

Managed state is stored transparently at the workspace root in `open-forge.extensions.json`.

The receipt records:

- Explicitly requested roots
- Dependency edges and descriptive versions
- Owned payload paths and content digests
- Complete owner sets for shared identical files

The receipt enables safe reconciliation and removal. It is not agent context and does not affect runtime meaning.

Before writing, the CLI validates the complete resolved plan:

- Paths must remain inside the target and use portable slash-separated spelling
- Source and target paths may not escape through symlinks, junctions, hard links, or special files
- Payloads may not write `.git/`, `.gitignore`, `open-forge.extensions.json`, or workspace-owned `.overwrite.md` files
- Different bytes may not target the same portable path
- Identical managed bytes may share explicit owners
- A managed package may not silently adopt an unowned path
- An unmanaged overlay may not replace a receipt-owned path
- A route host may not be removed while retained descendants depend on it

Payload, generated-index, and receipt changes form one rollback-capable in-process transaction. Git remains the durable review and recovery boundary.

## Current MVP Boundaries

The current Extension system is local and offline. It has no:

- Remote registry or network resolution
- Compatibility solver or version ranges
- Migration hooks
- Automatic orphan pruning
- Persistent crash-recovery journal
- Remote trust policy
- Automatic substitution between external capabilities and bundled ids

These are explicit MVP boundaries, not hidden behavior.

## Related Documentation

- [CLI command reference](cli.md)
- [Open Forge README](../README.md)
- [First-party Extension catalogue](../src/extensions/README.md)

# Open Forge Extensions

Extensions add optional whole files through existing Open Forge routes.

An Extension may provide any combination of Skills, Workflows, Directives, Patterns, Guidance, Templates, Map `routes`, Memory `routes`, and support files. A dependency-only pack may provide only dependency edges.

The term Extension describes optional packaging and managed ownership, not runtime meaning. Installed files retain the meaning of their destination routes and remain understandable and usable without the package manifest, catalogue, ownership receipt, or CLI.

## CLI Availability

The native CLI implements all six Extension operations but is not released. These examples assume the locally built global `open-forge` command described in the [development guide](development.md#link-the-native-cli-locally).

```text
open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global flags]
open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]
open-forge extension create [<stable-id>] [--path <catalogue-path>] [--name <text>] [--description <text>] [--package-version <text>] [--dependency <stable-id>]... [--automatic] [--dry-run] [global flags]
open-forge extension install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [global flags]
open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global flags]
open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [global flags]
```

The bare `extension` group shows help. `list` and `inspect` are read-only. The [CLI guide](cli.md#extension-operations) explains the shared options and result rules; the [Extension contracts](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md) define the complete behavior.

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

Install the Framework first with `open-forge install`. Follow the [README Quick Start](../README.md#quick-start), then inspect the bundled catalogue:

```sh
open-forge extension list --available
open-forge extension inspect development-toolkit
```

Preview the toolkit and its complete dependency closure, then apply the reviewed plan:

```sh
open-forge extension install development-toolkit --dry-run
open-forge extension install development-toolkit
```

Review the resulting files and lifecycle state with `git status` and `git diff`. Commit the actual changed paths as one reviewable change.

Omitting `--source` selects the embedded catalogue. To install from a local package, use one exact package or catalogue path and a separate consumer workspace:

```sh
open-forge extension install my-extension --source ../packages/my-extension --workspace ../consumer --dry-run
open-forge extension install my-extension --source ../packages/my-extension --workspace ../consumer
```

External sources and target workspaces must be lexically and physically disjoint. The source is read-only. Dependencies resolve offline, transitively, and only within the selected source universe. A package with dependencies may require its containing catalogue as `--source`.

Select exact package IDs or `--all`. A source containing exactly one completely validated package can supply its manifest ID when no ID is given. A prompt-capable human request can ask for an unresolved multi-package selection or an eligible initial replacement. The complete dependency closure is mandatory; dependencies cannot be deselected. `--automatic`, JSON, and redirected requests do not prompt or infer a multi-package selection. `--automatic` never means `--all`.

Install establishes managed ownership for absent packages or verifies an exact managed no-op. Managed divergence directs to `extension update`. Initial `--force` may replace only an eligible exact current occupant without adopting its old bytes or overriding another owner, an unsafe boundary, or a route collision.

One invocation and its complete dependency closure form one review unit. Install roots in separate commands when separate diffs matter.

## Preview, Update, And Remove

Dry-run performs discovery, dependency resolution where needed, and complete preflight. It reports intended content, generated-navigation, and lifecycle effects without writing.

Preview an update, then apply it:

```sh
open-forge extension update development-toolkit --dry-run
open-forge extension update development-toolkit
```

Update requires trusted installed lifecycle state, a trustworthy Framework anchor, and one available source. Normal update preserves changed, missing, and retired managed content. `--force` replaces or restores eligible current expected content; `--prune` removes eligible retired content. Automatic mode suppresses interaction but grants no force, prune, ownership, or deletion authority.

Preview and remove explicitly selected IDs:

```sh
open-forge extension remove development-toolkit --dry-run
open-forge extension remove development-toolkit
```

Remove needs trusted installed ownership but does not need package source bytes. It releases selected ownership, retains shared files, and deletes safe unchanged final-owner files. Changed final-owner files become unmanaged unless this same removal request includes `--prune`. A later prune cannot remove content whose ownership has already been released.

Retained dependents block removal of their dependency. A route host cannot be removed while retained descendants depend on it. Orphan dependencies remain installed; preview and remove them explicitly when wanted. Manual, idless, and directly copied content remains unmanaged.

## Current Catalogue

The first-party catalogue contains the `development-toolkit` package. Earlier package IDs are not aliases or current sources. Files installed earlier remain ordinary workspace files; the native CLI does not infer ownership from old receipts or matching bytes.

The toolkit is intentionally installed as one small unit. The current CLI does not select individual package features. Users own the installed files and may remove routes that provide no local value after reviewing the resulting route tree.

## Manual Installation

The CLI is optional.

To install manually:

1. Copy the package's `content/` contents into the target workspace after reviewing the destination paths.
2. Rebuild or manually update each affected generated `Entries` region.
3. Review the assembled files and their links.

The installed files then work like any other Open Forge files. Manual installation does not create managed update or removal state.

## Create A Local Extension

Select an existing catalogue parent and create a package scaffold:

```sh
open-forge extension create my-extension --path ../packages
```

The `../packages` directory must already exist. The command creates only:

```text
my-extension/
  extension.json
  content/
    .agents/
```

The scaffold does not include a README or installed content. `--path` selects the catalogue destination; `--workspace` is a no-op for Create. An exact matching scaffold is a verified no-op. Divergent, partial, additional, or colliding content blocks the request.

A prompt-capable human request asks only for a missing stable ID or catalogue parent. JSON, automatic, and redirected requests must provide both. The generated manifest contains deterministic defaults: a name derived from the hyphen-separated ID, description `Open Forge Extension package <stable-id>.`, version `0.1.0`, and no dependencies. Use `--name`, `--description`, or `--package-version` to override those values. Repeat `--dependency <stable-id>` to record dependencies without resolving their availability.

Then:

1. Review the generated manifest and optionally add a README outside `content/`.
2. Add complete routed files and required entrypoints beneath `content/.agents/`.
3. Author ordinary links for their final containing-file-relative locations.
4. Preview installation into a separate Open Forge workspace.
5. Install, run Doctor against the assembled workspace, and review the Git diff.

```sh
open-forge extension install my-extension --source ../packages/my-extension --workspace ../consumer --dry-run
open-forge extension install my-extension --source ../packages/my-extension --workspace ../consumer
open-forge doctor --workspace ../consumer
```

The source and target workspace must be disjoint. Create changes only its catalogue destination, acquires no workspace lease, and creates no recovery bundle. See the [Create contract](../.agents/memory/crystallized/documents/cli/contracts/extension/create/interface.md) for complete prompting, collision, and result behavior.

## Package Shapes

The current first-party package lives directly beneath the source catalogue:

```text
src/extensions/
  development-toolkit/
    extension.json
    README.md
    content/
```

A managed package contains:

```text
{package-folder}/
  extension.json
  README.md             # optional author documentation
  content/
    .agents/
      ... complete target-relative files ...
```

A dependency-only package may omit `content/`. It selects a dependency closure without installing a placeholder runtime file.

`--source` selects one package directory or a catalogue of packages. The manifest ID defines package identity. Files below `content/` retain their workspace-relative destination paths. Source folders and an optional README support package maintenance; they do not become installed context.

External destination files, such as `content/.apm/agents/reviewer.md`, require the consumer grants described below. Existing serialized members such as `payload` and `payloadTargets` keep their names; they describe contributed data rather than the source directory.

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

All five members are required:

- `id` is the stable lowercase package identity, independent of source location.
- `name`, `description`, and `version` must be nonblank strings.
- `version` is descriptive; it does not select a compatibility range.
- `dependencies` is an array, including `[]` when empty. Values must be valid, unique, ordinally sorted stable IDs and must not include the package's own ID.

Dependencies resolve within the selected source universe. Unknown or duplicate members, invalid IDs, malformed values, missing dependencies, and dependency cycles stop installation before writes.

The manifest is install metadata. It is not copied into agent context.

## Content And Routing

Content paths are portable and relative to the target.

Open Forge-authored routed files normally carry:

- #Extension
- Their Core primitive or Memory classification
- Useful scope and topic tags

A complete Workflow uses level-2 `Goal`, `Steps`, and `Completion` sections in that order. Recipe-specific headings may organize details without expanding the Framework schema. Use ordinary links and explicit Steps when another installed source must be read or invoked.

Standard formats such as `SKILL.md` retain their native metadata. Use #LoadNow or #KeepInMind only when the installed file deliberately belongs in baseline or continuity loading.

Markdown links resolve relative to their containing file in the assembled workspace. Links within one package should resolve in its isolated content. Links to Core or declared dependencies may resolve only after the complete package closure is assembled.

Do not duplicate dependency files or add source-only stubs merely to make a cross-package link resolve in isolation. Validate cross-package links after assembly because isolated package content does not contain its declared dependencies.

## Dependencies, Packs, And Skills

Dependencies:

- Use stable IDs in the selected source universe
- Resolve transitively and offline
- Install before their dependents
- Are deduplicated across the selected closure

Put reusable content in one canonical package. A Workflow that depends on a Skill package links to the concrete installed `SKILL.md` through an ordinary link and states in its Steps when the Skill must be used. Manifest dependencies remain installation metadata rather than runtime context.

Convenience packs should contain dependency edges instead of duplicated content. Keep dependency graphs small and acyclic.

Open Forge does not need to manage every Skill. An ordinary Skill may be copied or installed directly under `.agents/skills/{skill-name}/`. Run `open-forge index` afterward or update the Skills `Entries` manually.

Do not assign the same installed Skill path to two package managers. Routing an external Skill does not transfer ownership or rewrite its internals.

## Managed Ownership And Safety

Managed Framework and Extension state is stored in `.agents/open-forge.lifecycle.json`, with separate `framework` and `extensions` sections. An Extension operation preserves the unrelated Framework section. Permission for external destination files is stored separately in `.agents/open-forge.permissions.json`.

Lifecycle facts record requested roots, dependency edges, descriptive versions, owned paths, semantic fingerprints, and owner sets for shared files. They support safe update and removal; they do not become agent context or alter runtime meaning. Installed facts are classified as absent, trusted, untrusted, or incomplete, with unsafe ambiguity blocked. Safely readable installed facts remain visible when source bytes are unavailable, but that does not permit a source-dependent update or establish lifecycle trust.

Before writing, the CLI validates the complete plan, including source disjointness, portable paths, physical containment, ownership, protected controls, generated regions, and retained route relationships. Library projections and paths owned by another manager are not adopted or overwritten. Ordinary content effects reject final symlink or reparse-point leaves. Matching bytes or fingerprints do not establish ownership.

Before existing-target effects, the CLI prepares and verifies an external recovery bundle. A multi-file operation is not atomic. Failure or cancellation reports completed effects and retained recovery evidence; it does not automatically roll back, compensate, or infer current target state from recovery bytes. Successful effects remain successful if recovery-bundle cleanup fails; the result reports `attention` and the exact retained path. The [CLI guide](cli.md#extension-operations) and [Architecture](../.agents/memory/crystallized/documents/cli/architecture.md) describe the complete recovery boundary.

Files outside `.agents/` require consumer grants for the exact package ID and file path. Human application can ask once to remember the displayed grants, with No as the default. Dry-run, JSON, automatic, and redirected requests do not prompt; missing grants block the request. Force and prune do not bypass permission. A grant saved before a later content failure remains saved and is reported. Extension grants do not cover directories or future files and never override protected paths, source safety, or ownership.

## Current Boundaries

The Extension system is local and offline. It has no remote package registry or network resolution, compatibility solver or version ranges, migration hooks, automatic orphan pruning, persistent crash-recovery journal, remote trust policy, or automatic substitution between external capabilities and package IDs. External recovery bundles preserve bounded evidence for explicit recovery; they do not provide automatic rollback.

The frozen `open-forge-old` executable uses a different command interface and state format. It does not support current first-party package layout, and the native CLI does not migrate its receipts. The [historical MVP architecture](../.agents/memory/crystallized/documents/cli/mvp-architecture.md) records that former behavior.

## Related Documentation

- [CLI command reference](cli.md)
- [Open Forge README](../README.md)
- [First-party Extension catalogue](../src/extensions/README.md)

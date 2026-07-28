# Open Forge CLI

The Open Forge CLI is an optional deterministic helper for the human-readable Open Forge contract.

It makes installation, route loading, navigation, validation, scaffolding, and Extension lifecycle work cheaper and safer. It does not replace the Markdown contract or privately determine what workspace content means.

The distributed CLI requires Node.js 18 or later. Bun is used only for development in the Open Forge repository.

## Quick Start

Install the standard Framework into the current Git repository:

```sh
npx open-forge install
```

Review and commit the installed foundation:

```sh
git status
git diff
git add AGENTS.md CLAUDE.md .agents
git commit -m "Install Open Forge"
```

Load effective startup and continuity context:

```sh
npx open-forge load --bodies
```

Validate the route tree:

```sh
npx open-forge doctor
```

List optional Extensions:

```sh
npx open-forge extend --list
```

## Command Guide

| Command | Use it to | Writes |
|---|---|---|
| `install` | Install or reconcile the standard Framework | Yes |
| `extend` | List, preview, install, reconcile, or remove Extensions | Only installation, reconciliation, and removal |
| `index` | Rebuild generated `Entries` for `routes` | Yes |
| `load` | Emit effective baseline and continuity context | No |
| `find` | Select routed files by tag or explicit route | No |
| `chain` | Inspect inherited context for one routed file | No |
| `doctor` | Validate deterministic Framework structure | No |
| `create` | Scaffold a route chain or local Extension package | Yes |

## Shared Conventions

- A missing `target` means the current directory
- CLI route arguments are relative to the selected target
- Read-only commands never require a Git checkpoint
- `install` and managed `extend` operations compute and validate their mechanical plans before mutation
- Normal installation and Extension writes use Git-visible review checkpoints
- `--pro` bypasses only the lifecycle guards documented for `install` and `extend`; it is not a general safety bypass
- `load`, `find`, `chain`, and `doctor` provide JSON output for deterministic consumers

The CLI is currently an MVP. This guide documents behavior that exists now, and the interface may change before a stable release.

## Complete Syntax

```sh
open-forge install [target] [--pro]
open-forge extend [--dry-run] [--pro]
open-forge extend --list
open-forge extend --select [target] [--dry-run] [--pro]
open-forge extend --ids <id[,id...]> [target] [--dry-run] [--pro]
open-forge extend --remove <id[,id...]> [target] [--dry-run] [--pro]
open-forge extend <extension-source-or-id> [target] [--dry-run] [--pro]
open-forge index [target]
open-forge load [--bodies|--paths|--json] [target]
open-forge find [--tag <Tag>]... [--route <path>] [--depth <n>] [--follow-required] [--bodies|--paths|--json] [target]
open-forge chain <route> [--heading <title>] [--json] [target]
open-forge doctor [--json] [target]
open-forge create category <route-path> [target]
open-forge create extension <id> [directory]
```

If `target` is omitted, the current directory is used.

## install

```sh
open-forge install
open-forge install {target-folder}
open-forge install {target-folder} --pro
```

`install` is idempotent.

### Review And Safety

By default, `install` is the first review checkpoint. It detects the Git repository that contains the target and requires the target scope to be clean before writing. If the target is outside Git, an interactive terminal asks for explicit confirmation after recommending `git init`; a non-interactive invocation stops without mutation. Planned and derived index files ignored by Git are also rejected because they cannot produce a reviewable checkpoint.

The command installs only the Open Forge base Framework: Core plus the standard Memory routes, with no optional Extensions. After it succeeds, review `git diff` and `git status`, then commit that baseline before installing optional Extensions. The CLI prints the catalogue and selector commands for the next step. A no-op reinstall reports that no commit is needed.

`--pro` is an explicit expert bypass for the Git-repository, clean-checkpoint, Git-visible-output, and Core-first lifecycle guards. It does not bypass manifest validation, dependency resolution, source or target containment, collision checks, link and hardlink protection, index validation, local-block preservation, or rollback. Files managed by Open Forge, recognized `entrypoints` for `managed routes`, and generated index regions are preflighted and rolled back together on failure. Use `--pro` when combining diffs is an intentional expert decision, not as a generic force flag.

Core reinstall also reads and validates `open-forge.extensions.json` when managed extensions are present. It refuses a Core plan that would modify or remove a receipt-owned file. Update or remove the owning extension explicitly; `--pro` does not cross this ownership boundary.

### Effects

Running it will:

- create or update `AGENTS.md`
- create or update the minimal `CLAUDE.md` bridge
- append each Open Forge block if missing
- replace only the Open Forge block if it already exists
- preserve workspace-owned `AGENTS.md` and `CLAUDE.md` text outside those blocks
- overwrite existing Open Forge-managed files with the same name
- reconcile `entrypoint` files managed by Open Forge whose `route` shapes are recognized by the source catalogue
- rebuild generated index regions
- leave user-added files outside managed paths alone

There is no wizard. A first installation writes the current release payload. On an existing Core installation, an absent shipped file is treated as a deliberate removal: ordinary `install` updates the managed files that remain, but does not restore deleted defaults or add newly shipped defaults. The MVP does not yet provide an explicit completion or restoration operation. If you want a different local shape, install first, then edit, add, move, or remove files. The Framework is plain Markdown for exactly this reason.

### Managed Reconciliation

The current CLI indexes any explicit `route` chain whose folders have `entrypoints`. `open-forge create category <route-path>` can scaffold missing `entrypoints` and rebuild their indexes, but scaffolding does not declare them managed. You may also author the `entrypoints` directly, then run `open-forge index`.

`install` reconciles `entrypoint` files managed by Open Forge when their complete routed chains match `route` shapes derived from `src/open-forge/.agents/`. Every folder in the chain must already have an `entrypoint`. A recognized match stays beneath the same `root route` and may contain any number of consecutive scope `slugs` between source-defined non-root `route` segments. Scopes after the last managed segment remain user-owned descendants. The CLI does not create missing scope `entrypoints` during installation.

Managed reconciliation requires the canonical `_{folder-name}.md` `entrypoint`. Compatibility names such as `index.md` remain valid for generic routing and indexing, but the current Open Forge manager does not reconcile them.

Examples of `managed routes` recognized through scopes:

```text
.agents/memory/{scope-1}/{scope-2}/working/_working.md
.agents/memory/{scope}/working/{scope}/sessions/_sessions.md
.agents/memory/emerging/{scope}/observations/_observations.md
.agents/memory/{scope}/crystallized/{scope}/decisions/_decisions.md
.agents/memory/{scope}/crystallized/{scope-1}/{scope-2}/documents/_documents.md
.agents/memory/{scope}/archived/_archived.md
```

`{scope}` means a concrete `slug` folder with its own `entrypoint`, not a literal folder name. These examples assume each intermediate scope folder is already visible through its own `entrypoint`.

The source-defined `managed route` order remains fixed. Each scope narrows the `route` segments and content that follow it. For example, Documents keeps this sequence:

```text
memory/.../crystallized/.../documents/_documents.md
```

It does not match `memory/.../documents/.../crystallized/_crystallized.md`, combine two Memory states, or treat a familiar `slug` beneath one `root route` as another managed `root route`. `.agents/workflows/frontend/directives/_directives.md` therefore remains below Workflows rather than becoming the managed Directives `root route`.

Scopes beneath a Core `root route` inherit that `root route`'s meaning and do not repeat its managed `entrypoint`. A scope such as `.agents/patterns/mobile-app/react/_react.md` remains user-owned and is not replaced with `_patterns.md`.

The MVP uses source-defined folder names to recognize Memory roles. A neutral Memory scope whose `slug` equals one of those role names is ambiguous to this updater and should be renamed. The CLI overhaul must introduce a human-readable distinction between manager-recognized `route` segments and ordinary scope `slugs`.

Manual edits to files managed by Open Forge are visible in Git diffs after install. Prefer sibling files, deeper `routes`, or `.overwrite.md` companions for durable local customization.

## extend

```sh
open-forge extend [--dry-run] [--pro]
open-forge extend {extension-source}
open-forge extend {bundled-extension-id}
open-forge extend {extension-source} {target-folder}
open-forge extend {bundled-extension-id} {target-folder}
open-forge extend --list
open-forge extend --select {target-folder}
open-forge extend --ids {bundled-extension-id},{bundled-extension-id}
open-forge extend --remove {installed-extension-id}
```

### Sources And Dependencies

`extend` resolves an extension and its bundled dependencies, plans the complete payload, copies whole files into the target, and rebuilds affected generated index regions. Extension is a content-agnostic installation unit: installed payload files retain their ordinary runtime meaning.

Sources may be:

- A bundled first-party package selected by stable id
- A local package containing `payload/`
- A local direct overlay shaped like the target workspace

The manifest, catalogue, source grouping, and ownership receipt are install metadata only. Agents route from installed files and never need those install surfaces.

A local manifest id opts into managed lifecycle. Idless local sources remain unmanaged. Local package dependencies may select bundled ids, while arbitrary local-to-local dependency graphs are outside the MVP.

The [Extensions guide](extensions.md#package-shapes) defines package shapes, manifest fields, payload authoring, dependency semantics, and manual installation. The CLI automates those plain-file operations without creating another runtime contract.

Use `open-forge extend --list` for the grouped bundled catalogue, `open-forge extend` or `--select` for interactive selection, and `--ids` for unattended selection. Dependency resolution is offline, transitive, dependency-first, and deduplicated.

### Review And Safety

Normal installation requires recognizable tracked Open Forge Core anchors, a clean target scope, and Git-visible planned output. Outside Git, interactive use asks for approval after recommending initialization; non-interactive writes stop. One invocation and its dependency closure form one review unit. `--pro` bypasses only these Git/Core lifecycle guards.

Before writing, the CLI validates the complete resolved payload plan. Paths must be portable and target-relative. Linked roots, physical escapes, Git control paths, reserved receipt and overwrite paths, portable aliases, file/parent collisions, and conflicting bytes stop the operation. Identical managed bytes may share owners. A managed package never silently adopts an unowned path, and an unmanaged overlay cannot replace a receipt-owned path.

Extensions add whole files through ordinary routes. They do not inject blocks into shared Markdown, and managed payloads cannot claim workspace-owned `.overwrite.md` files.

Payload, generated-index, and receipt changes form one rollback-capable in-process transaction. Git remains the durable recovery boundary.

### Preview

An installation dry-run discovers selected packages, resolves dependencies, validates the complete plan, and prints dependency order, receipt effects, scope counts, and create, update, delete, and unchanged file effects:

```sh
open-forge extend --dry-run workflow-essentials ./my-project
open-forge extend --ids planning-workflows,quality-workflows --dry-run ./my-project
```

A removal dry-run reads installed receipt state, validates owned files and retained dependents, and prints delete, update, and unchanged file effects plus the planned receipt change:

```sh
open-forge extend --remove vision-workflow ./my-project --dry-run
```

Removal does not discover source packages or report installation dependency order and scope counts.

### Managed Receipt, Update, And Removal

A bundled id or local manifest id is the managed ownership key. The CLI stores transparent Git-visible state at `open-forge.extensions.json`: requested roots, dependency edges, descriptive versions, owned payload paths and SHA-256 digests, and complete owner sets. Generated `Entries` bodies may be rebuilt without invalidating ownership of the surrounding `entrypoint`.

This receipt is CLI state, not agent context. Installed routed files remain complete runtime truth.

Before a managed write, the CLI validates receipt reciprocity and every recorded file digest. Missing or modified owned content, destructive route changes, or inconsistent owner sets stop the command. `--pro` never bypasses ownership checks.

Reinstalling the same id reconciles owned whole files. Dropped paths are removed only when their recorded bytes still match and no owner remains. Removing an entrypoint is blocked when retained descendants would become unreachable.

Remove installed ids with:

```sh
open-forge extend --remove vision-workflow
```

Removal acts on exactly the named ids and stops while a retained extension depends on one of them. Dependencies that become orphans are not pruned automatically.

Installation previews and normal installs report:

- `routed` - files under `.agents/` that use ordinary relevance routing
- `baseline-loading` - root agent entry files such as `AGENTS.md` and `CLAUDE.md`, the loader, overwrites of baseline files, and files explicitly tagged #LoadNow or #KeepInMind
- `skill-executable` - files in a skill's direct `scripts/` subtree
- `outside-.agents` - workspace files outside the routed tree

Extension-authored Open Forge files normally use #Extension plus their primitive and useful scope tags. Runtime-native files keep native metadata. Use a load-policy tag only when the installed content deliberately belongs in baseline or continuity loading.

This remains a local, offline command. It has no external registry, network resolution, compatibility solver, migration hooks, automatic orphan pruning, persistent crash-recovery journal, or remote trust policy.

## index

```sh
open-forge index
open-forge index {target-folder}
```

`index` rebuilds the loader category registry and the generated regions inside category `entrypoints`.

When `.agents/` exists, the CLI scans `.agents/`. Otherwise it scans the target folder.

### Route Discovery

When `loader.md` exists at the scan root, the CLI generates one loader `entry` for every direct child folder that contains one recognized category `entrypoint`. Loader `descriptions` and tags come from the category `entrypoint`, preferring supported metadata and falling back to its first body `description` and #Index.

Generated `Entries` use standard Markdown links whose destinations resolve relative to the file containing them. A loader at `.agents/loader.md` therefore links to `workspace/_workspace.md`; a category `entrypoint` links from its own folder. CLI arguments and output `route` identities remain relative to the selected target, such as `.agents/workspace/_workspace.md`. Git and submodule boundaries do not redefine that logical root. For deterministic CLI reads and writes, the `route` tree must also be physically contained below the target: a symlinked or junction-mounted `.agents/` tree that resolves outside it is rejected. Plain Markdown agents may still follow an explicitly trusted external mount, but the CLI will not read or mutate through that trust boundary.

Nested categories stay behind their parent category `entrypoint`. Folders without a matching `entrypoint` do not become loader `routes`.

Open Forge-authored category `entrypoints` are named `_{folder-name}.md`:

```text
.agents/patterns/
  _patterns.md
  local-docs.md
```

The category `entrypoint` contains stable category meaning followed by a generated region. The generated region reads direct Markdown `route` files and direct child category `entrypoints`:

```md
- [Local documentation patterns](local-docs.md) - #Pattern #Documentation
- [React patterns](react/_react.md) - #Pattern #React
```

Child folders are routed through their own `_{folder-name}.md` category `entrypoint`. Parent `entrypoints` stay at one folder boundary.

### Skills And Scopes

The link label carries the generated `description`, and the link destination carries the containing-file-relative `route`. The same canonical shape applies to authored `Required Routes`; each line keeps a tag suffix with useful tags, including at least the target primitive type.

The standard Skills `route` recognizes ordinary native packages:

```text
.agents/skills/
  _skills.md
  implementation/
    SKILL.md
    references/
      fit-change-to-system.md
```

The generated Skill `entry` points to `implementation/SKILL.md`. The Skill's `SKILL.md` is authoritative for its metadata, applicability, instructions, resource organization, and loading behavior.

The same rule applies to a skill copied directly or deployed by an external manager: place its complete folder at `.agents/skills/{skill-name}/`, then run `open-forge index` and `open-forge doctor`. Indexing updates only generated `route` regions; it does not rewrite the skill or generate `Entries` inside `SKILL.md`.

Routed scopes beneath the Skills `root route` can also expose direct native Skill packages. Open Forge indexes those packages and exposes them through `routes` under the same contract. A `route` elsewhere remains generically routable, but a familiar name or #Skill tag does not grant native Skill-package indexing outside the Skills `root route`.

The loader's universal scoping rules apply below every `root route`. A scope is expressed by an ordinary concrete `slug` folder with its own `entrypoint`. Every folder in the visible `route` chain needs its own `entrypoint`:

```text
.agents/memory/
  _memory.md
  {scope}/
    _{scope}.md
    crystallized/
      _crystallized.md
      decisions/
        _decisions.md
      documents/
        _documents.md

.agents/guidance/
  _guidance.md
  {scope}/
    _{scope}.md
    cross-platform-apps.md
```

Open Forge does not require a folder named `projects`, `scope`, `domain`, or `team`. Use those only when they improve your local routing.

Scope placement changes meaning:

```text
.agents/memory/crystallized/{scope}/decisions/
.agents/memory/{scope}/crystallized/decisions/
```

The first means `{scope}` is inside Crystallized Memory. The second gives `{scope}` its own Memory state `routes`. Both are valid when every folder has an `entrypoint` and the `descriptions` of those `entrypoints` make the scope clear.

### Entrypoints And Generated Regions

For cross-tool compatibility, the CLI also recognizes these `entrypoint` names:

- `_index.md`
- `index.md`
- `_references.md`
- `references.md`

Open Forge itself uses only `_{folder-name}.md`. A folder must contain exactly one recognized `entrypoint` name. If multiple candidates exist, the CLI stops before changing any generated region.

The generated region is bounded explicitly:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

The CLI replaces only the content between the markers. It preserves frontmatter and category content above the region.

When a legacy category `entrypoint` or loader has a final `## Entries` section containing only generated list `entries`, the CLI adds the markers automatically. The parser continues to accept the former backtick-path entry shape during migration, but `index` emits only canonical relative Markdown links. Authored legacy `Required Routes` remain readable with their former workspace-root-relative resolution and should be converted to containing-file-relative canonical links when touched. If the heading is absent, the CLI appends the complete section. Malformed or non-final markers stop generation without changing the file.

Change index output by adding, editing, moving, or removing route files and child category `entrypoints` in the indexed folder.

### Indexed Content And Metadata

The index generator reads:

- direct `*.md` route files, including underscore-prefixed routed files
- direct child category `entrypoints` named `_{folder-name}.md`
- direct native Skill packages under the Skills `root route` or within routed scopes beneath it

Inside the standard Skills tree, loose Markdown files are not indexed as native Skills. Use Skill folders with `SKILL.md`.

Reserved filenames in an indexed folder are:

- `_{folder-name}.md`
- `_index.md`
- `index.md`
- `_references.md`
- `references.md`

The index generator ignores tool paths:

- `.git/`
- `.obsidian/`
- `node_modules/`

Metadata comes from frontmatter:

```md
---
description: Local documentation patterns
tags: [Doc, Pattern]
---
```

Nested metadata works too:

```md
---
open-forge:
  description: Local documentation patterns
  tags: [Doc, Pattern]
---
```

The CLI also accepts `rune:` scoped metadata in user-added route files for cross-tool compatibility. Open Forge-authored files use `open-forge:` metadata.

`SKILL.md` files should keep runtime-required metadata such as `name` and `description`. The CLI reads the root `description` for generated skill entries and defaults their tag to #Skill when no Open Forge tags are present.

The indexer ignores `.overwrite.md` companions because they inherit the base route and load immediately after it rather than through generated navigation.

## load

```sh
open-forge load --bodies
open-forge load --paths
open-forge load --json
```

`load` optionally batches the same plain traversal: the loader, each transitive #LoadNow entry reachable through already-loaded parents in generated order, and every routed #KeepInMind result with its visible #LoadNow closure. Each base is immediately followed by its user-owned `.overwrite.md` when present. Installed files and ordinary traversal remain complete without this command.

The traversal does not enter an on-demand parent merely because a hidden descendant has #LoadNow. #KeepInMind is the deliberate catalogue-wide exception. Use `--bodies` when an agent needs the actual context, `--paths` for a compact audit, or `--json` for tooling.

## find

```sh
open-forge find --tag Decision --tag Routing
open-forge find --tag Workflow --tag PhaseDelivery
open-forge find --route .agents/workflows/dev/_dev.md --follow-required --bodies
open-forge find --route .agents/memory/crystallized/_crystallized.md --depth 1
open-forge find --tag Workflow --json
```

`find` is deterministic routing-contract lookup, not search. It walks routed files only: the loader, category `entrypoints`, their direct `route` files, and Skill `entrypoints`. It never guesses relevance or expands user-owned overwrites.

Combine `Workflow` with one of `PhaseDiscovery`, `PhaseDefinition`, `PhasePlanning`, `PhaseDelivery`, or `PhaseVerification` to inspect phase candidates. Those tags provide non-waterfall wayfinding; the workflow Goal and routed current truth still decide fit.

- `--tag <Tag>` filters by effective tags (metadata tags, or the generated defaults); repeat the flag to require every tag. Matching is case-insensitive; canonical spelling still comes from the loader.
- `--route <path>` selects one file or routable folder using a workspace-relative CLI path such as `.agents/workflows/dev/_dev.md`; `--depth <n>` also follows its generated `entries` n levels.
- `--follow-required` adds every target of the selected files' `## Required Routes` sections. Markdown link destinations resolve relative to the workflow file containing them. A required route that cannot be read fails the command - it is a blocker, not a skip.
- Output is entry lines by default; `--paths` prints paths only, `--bodies` prints file contents with `----- {route} -----` separators, `--json` prints structured output.

Use `open-forge load --bodies` when batched effective baseline context is convenient. `find` remains useful for catalogue queries; neither command replaces the complete parent-aware plain traversal defined by the loader.

Explicit CLI routes, generated-entry expansion, and Required Routes are lexical and physical containment boundaries. `find --route` rejects absolute paths, parent traversal, drive changes, and real-path link escapes from the selected target. A containing-file-relative Markdown link may use `..` only when its resolved target remains inside the selected workspace. Global routed discovery likewise rejects linked or special entries before reading them.

## chain

```sh
open-forge chain .agents/patterns/react/components.md
open-forge chain .agents/patterns/react/components.md --heading Axioms
open-forge chain .agents/workflows/dev/_dev.md --heading Constraints --json
```

`chain` explains inherited Markdown context for one routed file. It emits the loader, each visible ancestor category `entrypoint`, a skill's `SKILL.md` when the target is inside its folder, the target, and each user-owned overwrite immediately after its base. With no `--heading`, it lists the route chain. With `--heading`, it reports every matching section from every chain member as `content`, `absent`, `empty`, `declared-inherited`, or `declared-none`.

The heading is arbitrary, so the same command can inspect `Axioms`, Mode, Goal, Constraints, or a local category heading. `declared-none` applies only where the selected heading's contract defines `none`, such as Workflow Constraints. A local category `Axioms` section may be missing, empty, or state `inherited`; all three forms add no local `Axioms` while loaded ancestor `Axioms` remain active. `none` is not a valid `Axioms` sentinel. `--json` provides stable structured output for tools.

Each `route` is resolved inside the selected logical target. Absolute paths, parent traversal, drive changes, and real-path or symlink escapes are rejected without mutation. `chain` reports the currently visible file chain; run `doctor` when indexed `route` continuity itself must be validated.

## doctor

```sh
open-forge doctor
open-forge doctor --json
```

`doctor` validates route integrity without writing anything. It reports:

- folders with multiple recognized `entrypoints` (error)
- malformed generated-region markers (error)
- generated `entries` that do not resolve to files (error)
- `Required Routes` that do not resolve (error), or sections that state neither routes nor `none` (warning)
- workflow recipes whose level-2, ordered Mode, Goal, Required Routes, Constraints, Steps, Loop, Outputs, and Completion contract is missing or invalid (error); Goal may contain the optional advisory `- helpful before: ...` item, and category-only workflow `entrypoints` may omit the recipe contract until they declare any recipe heading
- direct directive files without #LoadNow metadata or exactly one substantive level-2 `Axioms` section (error)
- directive files that retain the legacy `Applies To` second applicability gate (error)
- complete workflow recipes without exactly one recognized primary phase tag (error)
- category `Axioms` sections that use `none` as a sentinel (error), or mix `inherited` with substantive local `Axioms` (warning)
- stale generated regions that no longer match what `index` would produce (warning; run `open-forge index`)
- retired load-policy tags in metadata (warning)
- `.overwrite.md` companions without a base file (warning)
- markdown files not reachable through generated routing (warning)

Exit code is non-zero when errors exist, so `doctor` is safe for CI. `index` remains the repair tool for generated regions; `doctor` only reports.

`doctor` uses the same target-containment boundary as `find`: an external symlink/junction route tree or linked routed entry is an error and is not consumed as workspace context.

`doctor` and `find --follow-required` treat the selected target as a complete workspace and do not consult extension manifests or dependency metadata. Running either command against an isolated extension source payload therefore reports intentionally absent #Core or declared-dependency routes; validate those cross-package routes after assembly instead of adding source-only stubs.

## create

```sh
open-forge create category patterns/react/components
open-forge create category .agents/memory/crystallized/mobile-app
open-forge create extension my-patterns
```

`create category` scaffolds a `route` chain. Every missing folder gets a canonical `_{folder}.md` `entrypoint` with placeholder metadata, the containing `root route`'s single primitive tag such as #Workflow or #Skill when one exists, an `inherited` `Axioms` sentinel, and an empty generated region. The command then rebuilds all indexes.

The command does not turn a child into another `root route` because its `slug` is familiar. `workflows/frontend/patterns/` therefore remains a Workflow scope, while `patterns/frontend/` remains a Pattern scope.

The command refuses paths that are already routable. Fill in the generated TODO `descriptions`, then run `open-forge index` again. A local category may instead omit `Axioms` or leave the section empty; all three forms add no local `Axioms` while loaded ancestor `Axioms` remain active.

`create extension` scaffolds a managed extension package: `extension.json` with the requested stable `id` plus starter `name`, `description`, `version`, and `dependencies` fields; a README with authoring rules; and an empty `payload/.agents/` tree ready for whole routed files. Install it with `open-forge extend <directory-or-id>` or copy the payload and update generated `Entries` manually.

## Related Documentation

- [Open Forge README](../README.md)
- [Extensions](extensions.md)

# Open Forge CLI

The Open Forge CLI is intentionally small. It installs the released framework files, composes local and bundled extensions, manages stable-id extension ownership, updates `AGENTS.md`, and rebuilds generated index regions.

The distributed CLI runs on Node.js.

## Commands

```sh
open-forge install [target] [--pro]
open-forge extend [--dry-run] [--pro]
open-forge extend --list
open-forge extend --select [target] [--dry-run] [--pro]
open-forge extend --ids <id[,id...]> [target] [--dry-run] [--pro]
open-forge extend --remove <id[,id...]> [target] [--dry-run] [--pro]
open-forge extend <extension-source-or-id> [target] [--dry-run] [--pro]
open-forge index [target]
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

By default, `install` is the first review checkpoint. It detects the Git repository that contains the target and requires the target scope to be clean before writing. If the target is outside Git, an interactive terminal asks for explicit confirmation after recommending `git init`; a non-interactive invocation stops without mutation. Planned and derived index files ignored by Git are also rejected because they cannot produce a reviewable checkpoint.

The command installs only the Open Forge base payload: #Core plus the minimum #Memory routes needed for resumability, with no optional extensions. After it succeeds, review `git diff` and `git status`, then commit that base baseline before installing optional extensions. The CLI prints the catalogue and selector commands for the next step. A no-op reinstall reports that no commit is needed.

`--pro` is an explicit expert bypass for the Git-repository, clean-checkpoint, Git-visible-output, and Core-first lifecycle guards. It does not bypass manifest validation, dependency resolution, source or target containment, collision checks, link and hardlink protection, index validation, local-block preservation, or rollback. Core managed, scoped-framework, and generated-index writes are preflighted and rolled back together on failure. Use `--pro` when combining diffs is an intentional expert decision, not as a generic force flag.

Core reinstall also reads and validates `open-forge.extensions.json` when managed extensions are present. It refuses a Core plan that would modify or remove a receipt-owned file, remove an occupied augmentation slot, or alter a retained owned block. Update or remove the owning extension explicitly; `--pro` does not cross this ownership boundary.

Running it will:

- create or update `AGENTS.md`
- append the Open Forge block if missing
- replace only the Open Forge block if it already exists
- overwrite Open Forge managed files with the same name
- update `scoped framework route` `entrypoints` recognized by path shape
- rebuild generated index regions
- leave user-added files outside managed paths alone

There is no wizard. The command installs the current release payload. If you want a different local shape, install first, then edit or add files. The framework is plain markdown for exactly this reason.

The current CLI indexes any explicit route chain whose folders have `entrypoints`. It does not scaffold `scope routes` from templates yet. Create the scope folders and `entrypoints` yourself for now, then run `open-forge index`.

When `scoped framework routes` already exist, `install` updates their framework `entrypoints` from the current framework wording. It identifies them by concrete path shape and canonical `entrypoint` filename, not by hidden version metadata. It does not create missing scope `entrypoints` yet.

Examples of `scoped framework routes` recognized by the current implementation:

```text
.agents/memory/[scope]/crystallized/_crystallized.md
.agents/memory/[scope]/crystallized/decisions/_decisions.md
.agents/memory/[scope]/crystallized/documents/_documents.md
.agents/memory/[scope]/crystallized/[scope]/documents/_documents.md
.agents/workflows/implementation/directives/_directives.md
```

`[scope]` means a concrete `slug` folder with its own `entrypoint`, not a literal folder name. These examples assume each intermediate scope folder is already visible through its own `entrypoint`.

These match `framework route` shapes such as:

```text
memory/.../crystallized/_crystallized.md
memory/.../crystallized/.../decisions/_decisions.md
memory/.../crystallized/.../documents/_documents.md
.../directives/_directives.md
```

A local `scope route` such as `.agents/patterns/[scope]/react/_react.md` is not updated as a `scoped framework route` unless it matches a known `framework route` shape.

Manual edits to framework files are visible in git diffs after install. Prefer sibling files, child routes, or `.overwrite.md` companions for durable local customization.

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
open-forge extend --ids {bundled-extension-id},{bundled-extension-id} {target-folder}
open-forge extend --dry-run {extension-source-or-id} {target-folder}
open-forge extend --ids {bundled-extension-id},{bundled-extension-id} --dry-run {target-folder}
open-forge extend --remove {installed-extension-id}
open-forge extend --remove {installed-extension-id},{installed-extension-id} {target-folder} --dry-run
open-forge extend {bundled-extension-id} {target-folder} --pro
```

`extend` resolves an extension and its bundled dependencies, plans the complete overlay, installs it into the target, and rebuilds generated index regions. Extension is a content-agnostic installation unit: the payload may be one skill, one workflow, directives, other routed material, support files, or any mix. Dependency resolution and installation are offline; the CLI reads only local folders and first-party extensions already shipped in the installed package.

The extension can come from:

- a local folder shaped like the files it should add to the workspace
- a local extension package folder that contains `payload/`
- a bundled first-party Open Forge extension shipped with the CLI package

Local overlay example:

```text
my-extension/
  .agents/
    patterns/
      react/
        _react.md
        components.md
```

Running `open-forge extend my-extension {target-folder}` copies those files into `{target-folder}`. Markdown files preserve matching marked local blocks when the target file already exists. Other files are copied over directly.

An idless direct overlay is unmanaged. The CLI copies its files but records no ownership. If the source declares a stable `id` in `extension.json`, it deliberately opts into receipt-managed ownership even without `payload/`; direct-overlay describes source layout, not lifecycle. The manifest itself is read for planning and is not copied into the target.

Local package example:

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

Running `open-forge extend my-extension {target-folder}` copies only `payload/` into `{target-folder}`.

`extension.json` is optional only for an idless plain payload or overlay with no dependencies or augmentations. It is required when a local source wants managed identity or declares any dependency or augmentation. The current manifest fields are:

```json
{
  "id": "my-extension",
  "name": "My Extension",
  "description": "One line shown by list and selection views",
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

- `id` is an optional lowercase stable extension id for bundled metadata and plain local overlays. It opts a local source into receipt-managed install, update, and removal. Bundled packages already have their catalogue id; a declared manifest id must match it. Any local source that declares dependencies or augmentations requires stable identity.
- `name`, `description`, and `version` are optional non-empty strings. `version` is descriptive; the CLI does not perform version selection or compatibility solving.
- `dependencies` is an optional duplicate-free array of lowercase bundled extension ids.
- `augmentations` is an optional array of unique target-slot declarations. Each item has exactly one portable base-Markdown `target`, lowercase `slot`, and package-local Markdown `source`. Target and source use slash-separated relative paths; literal backslashes are invalid.
- A dependency id may refer to any bundled extension shape, including a skill-only capability or a dependency-only convenience pack.
- A local package or overlay may depend on bundled ids, but not on another arbitrary local path.
- Invalid JSON, unknown fields, invalid field types or ids, duplicate dependencies or augmentations, missing bundled dependencies, dependency cycles, invalid paths, and reserved augmentation markers stop the command before files are written.

The manifest is CLI metadata only. It is not copied from a package-shaped extension, is not required by agents, and is never a hidden routing source. Installed payload files and materialized augmentation blocks remain runtime truth.

### Augmentations And Precedence

An augmentation target must already contain an explicit empty slot owned by that target:

```md
<!-- open-forge-augment.workflow-selection:start -->
<!-- open-forge-augment.workflow-selection:end -->
```

For each declaring extension, the CLI inserts one owned block and sorts all blocks by extension id:

```md
<!-- open-forge-extension.my-extension:start -->
- Prefer this workflow when its Goal matches the requested transition.
<!-- open-forge-extension.my-extension:end -->
```

The source fragment is trimmed and materialized into the target; it is not installed or routed separately. A declared source is excluded from payload copying even when it lives below `payload/`. Slots may contain only whitespace and owned blocks. The target must be an existing base Markdown file, not an `.overwrite.md` file or `open-forge.extensions.json`. The installer never guesses a heading or merges unmarked prose.

A managed payload may ship empty augmentation slots in base Markdown files. It may not ship pre-owned extension blocks or malformed, nested, duplicate, or non-empty slots; those states are rejected before target mutation.

Managed extensions cannot own `.overwrite.md` files as a shared-file mutation strategy. The base file, including materialized augmentation blocks, loads first; a workspace-owned overwrite companion loads afterward and remains the final local-precedence layer.

Payload-bearing bundled first-party extensions live in the CLI package under this source shape:

```text
src/extensions/{extension-id}/
  extension.json
  payload/
    .agents/
      ...
```

A dependency-only or augmentation-only package may omit `payload/` when its manifest declares at least one dependency or augmentation. A dependency-only pack installs that closure and writes no package file of its own; an augmentation-only package writes only its owned block into an existing target. This works by bundled id or from a copied local directory containing only `extension.json`, optional `README.md`, and declared fragment files; the local copy must declare a stable `id`. A bundled package with no payload, dependencies, or augmentations is not catalogued and is rejected if addressed directly. A local directory with another root file and no `payload/` retains direct-overlay layout; an idless plain overlay stays unmanaged only while it declares neither dependencies nor augmentations.

Use `open-forge extend --list` to show the complete bundled catalogue available in the installed CLI package. Each line includes direct dependencies and a `contents` summary derived from payload paths and augmentation declarations, such as `skill`, `workflow`, `directive`, `guidance`, `pattern`, `workspace`, `memory`, `augmentation`, `other`, or `pack`. The summary is display metadata, not agent runtime truth. Use `open-forge extend {extension-id}` to install one.

Use `open-forge extend` or `open-forge extend --select {target-folder}` to select bundled extensions in an interactive TTY. `[direct]` marks what the user selected and `[required]` marks automatically selected transitive dependencies. Space toggles a direct selection, but a required-only dependency is locked while another selection needs it. Removing the last dependent releases an orphaned requirement; a dependency selected directly remains direct. Enter installs and `q` cancels.

Use `open-forge extend --ids {id},{id} {target-folder}` for unattended bundled extension installs. The CLI resolves transitive dependencies from the bundled first-party extensions, orders dependencies before dependents, deduplicates repeated packages, and runs index generation once after installing the complete plan.

Normal extension installation requires recognizable Open Forge Core anchors (`AGENTS.md` with the managed Open Forge block and `.agents/loader.md` with the Open Forge loader contract). Inside Git, both anchors must be tracked, the target scope must be clean, and planned output—including the ownership receipt and derived indexes—must remain Git-visible. Outside Git, an interactive terminal recommends `git init` and requires explicit approval; a non-interactive write stops without mutation. One invocation installs one selected dependency closure as one review unit. After success, the CLI asks the user to review and commit that unit before another mutation. To keep independent extensions in independently reviewable diffs, install them in separate commands; `--ids` and multi-select deliberately combine their requested roots and automatically required dependencies into one unit.

Read-only catalogue listing and `--dry-run` remain available before Core and do not require Git cleanliness. `--pro` intentionally bypasses the Git/Core lifecycle guard for normal writes while all installation-safety preflight remains active.

Before writing, the CLI validates strict manifest and receipt fields and reads every source file in the resolved extension set. Manifest and payload paths must be portable slash-separated relative paths; literal backslashes are rejected rather than interpreted as separators. Relative paths are Unicode-normalized and case-folded for portable composition, so case-only aliases within the plan or already in the target are collisions even on a case-sensitive host. Their segments also reject Windows-invalid characters, trailing dots or spaces, and reserved device basenames. If two extensions provide the same portable path with different bytes, installation stops; identical bytes are deduplicated within the plan. A managed package may not claim an existing unowned target, even when the bytes match, and an unmanaged overlay may not replace a receipt-owned path. Identical managed payloads may share recorded owners, but updating a shared file requires every existing owner to participate and supply the same new bytes. The plan also rejects a file that would be another planned file's parent, lexical or real-path source containment violations, linked local or direct bundled package roots, a linked target root, symbolic links or junctions below those roots, multiply linked files that may be rewritten, and Git control paths: `.git` in any path segment or `.gitignore` at any depth. Those controls could hide or mutate the transaction and must be applied as separate reviewed changes. The selected index tree is validated independently, including for an empty or outside-`.agents` payload. Existing target files are then classified as create, update, delete, or unchanged; marked local blocks in Markdown remain preserved.

Payload and augmentation application, index regeneration, and `open-forge.extensions.json` update are one in-process transaction. A handled failure restores overwritten files and blocks, removes files created by the attempt, cleans up newly created empty directories, restores indexes, and restores the previous receipt. This is process-level rollback, not a persistent journal or recovery mechanism for abrupt termination or machine failure; Git remains the durable recovery boundary.

Add `--dry-run` anywhere in an extension install or removal command to print create/update/delete/unchanged effects and every planned relative path, including the receipt change, without creating the target, copying files, changing augmentation blocks, or rebuilding indexes. Installation previews also print dependency order:

```sh
open-forge extend --dry-run workflow-essentials ./my-project
open-forge extend dev-workflow ./my-project --dry-run
open-forge extend --ids planning-workflows,quality-workflows --dry-run ./my-project
```

`--dry-run` cannot be combined with `--list`. With no explicit ids, it applies after interactive selection. It also checks existing generated-index marker/layout validity and entrypoint ambiguity that can be established without applying the payload. It does not preview generated-index body changes or include generated index writes in its file counts.

### Managed Receipt, Update, And Removal

A bundled id or local manifest `id` is the managed ownership key. The CLI records managed state at the target root in transparent, Git-visible `open-forge.extensions.json`. Its schema records explicitly requested roots, installed dependency edges and versions, each extension's owned paths and augmentation target-slots, payload SHA-256 digests, and complete file owner sets. Receipt `sha256` protects extension-authored bytes. In an owned entrypoint, the bounded generated `Entries` body belongs to the CLI and may be rebuilt by `index` or Core without invalidating ownership. This is CLI state only; agents route from the materialized Markdown and native files and never need the receipt.

Before any managed install, update, or removal, the CLI validates receipt relationships and verifies every recorded file and augmentation-block digest. It also checks every retained block against the complete planned final target, so another payload in the same transaction cannot erase a slot or disturb a retained block. A malformed receipt, a missing or modified owned file, a missing or modified block, or a destructive final plan stops the command. `--pro` does not bypass ownership checks.

Reinstalling the same managed id updates it. The new package shape is reconciled against the receipt: changed owned content updates, and dropped files or augmentation declarations are released and removed only when their recorded extension-authored bytes still match and no other owner or installed block needs them. An update that removes an owned entrypoint as a route host is blocked if retained descendants would become unreachable; the same plan must move or remove those descendants, or another owner must keep the route host.

Remove installed ids with:

```sh
open-forge extend --remove vision-workflow
open-forge extend --remove vision-workflow,vision-capability ./my-project
open-forge extend --remove vision-workflow ./my-project --dry-run
```

Removal acts on exactly the ids named. It stops when a retained extension still depends on one of them. Verified extension blocks are removed from their slots; verified payload files are deleted only after their last owner is removed and only when no remaining augmentation depends on the target. Removing an owned entrypoint is blocked when it would strand retained routed descendants; move or remove them in the same plan, or keep another owner for the route host. Dependencies that become installed orphans are not pruned automatically. Preview and remove those ids explicitly when that is the intended review unit; a future `--prune` would need its own explicit, previewable root and legacy-package policy.

A writing removal uses the same Core, clean-Git, tracked-anchor, Git-visible-output, transaction, and post-review checkpoint as installation. `--pro` bypasses only that lifecycle checkpoint; it cannot force removal of modified, shared, or still-required content.

Installation dry runs and normal installs also print a concise scope review:

- `routed` - files under `.agents/` that use normal relevance routing
- `baseline-loading` - `AGENTS.md`, `.agents/loader.md`, `.agents/loader.overwrite.md`, and direct Markdown files in `.agents/directives/`
- `skill-executable` - files in a native skill package's direct `scripts/` subtree
- `outside-.agents` - workspace files outside the routed tree

Normal installs print the four counts; dry runs add the per-file plan so baseline, executable, and workspace-wide effects can be reviewed before approval.

Extension payload files normally use #Extension plus their route type and useful scope tags. Do not use load-policy tags in extension payloads unless the extension intentionally adds baseline-loaded material.

This remains a local, offline extension command. It provides receipt-backed install, update, and explicit removal for stable-id packages, while idless plain overlays and direct/APM skill installs remain unmanaged. An idless local source cannot declare dependencies or augmentations. The CLI has no external registry or network resolution, compatibility or version solver, migration hooks, automatic orphan pruning, persistent crash-recovery journal, or remote trust policy. Route-template scaffolding is also still future work. Use `--dry-run`, mutate intentionally, then inspect and commit the Git diff before the next extension transaction.

## index

```sh
open-forge index
open-forge index {target-folder}
```

`index` rebuilds the loader category registry and the generated regions inside category `entrypoints`.

When `.agents/` exists, the CLI scans `.agents/`. Otherwise it scans the target folder.

When `loader.md` exists at the scan root, the CLI generates one loader `entry` for every direct child folder that contains one recognized category `entrypoint`. Loader descriptions and tags come from the category `entrypoint`, preferring supported metadata and falling back to its first body description and #Index.

In an installed workspace, loader paths are concrete and relative to the target folder, such as `.agents/workspace/_workspace.md`. Git and submodule boundaries do not redefine that logical root. For deterministic CLI reads and writes, the route tree must also be physically contained below the target: a symlinked or junction-mounted `.agents/` tree that resolves outside it is rejected. Plain Markdown agents may still follow an explicitly trusted external mount, but the CLI will not read or mutate through that trust boundary.

Nested categories stay behind their parent category `entrypoint`. Folders without a matching `entrypoint` do not become loader routes.

Open Forge-authored category `entrypoints` are named `_{folder-name}.md`:

```text
.agents/patterns/
  _patterns.md
  local-docs.md
```

The category `entrypoint` contains stable category meaning followed by a generated region. The generated region reads direct markdown route files and direct child category `entrypoints`:

```md
- `{file}` - {description} - #{Tag1} #{Tag2} ... #{TagN}
- `{folder/_folder.md}` - {description} - #{Tag1} #{Tag2} ... #{TagN}
```

Child folders are routed through their own `_{folder-name}.md` category `entrypoint`. Parent `entrypoints` stay at one folder boundary.

The skills route also recognizes native skill packages:

```text
.agents/skills/
  _skills.md
  implementation/
    SKILL.md
    references/
      fit-change-to-system.md
```

The generated skill `entry` points to `implementation/SKILL.md`. The skill's own `SKILL.md` owns any `references/`, `scripts/`, `assets/`, or other runtime resources inside that folder.

The same rule applies to a native skill copied directly or deployed by an external manager such as Microsoft APM: place or deploy the complete package at `.agents/skills/{skill-name}/`, then run `open-forge index` and `open-forge doctor`. Indexing updates only generated route regions; it does not rewrite the skill package.

`scope routes` use the same rule. A `scope route` is a concrete `slug` folder with its own `entrypoint`. Every folder in the visible route chain needs its own `entrypoint`:

```text
.agents/memory/
  _memory.md
  [scope]/
    _[scope].md
    crystallized/
      _crystallized.md
      decisions/
        _decisions.md
      documents/
        _documents.md

.agents/guidance/
  _guidance.md
  [scope]/
    _[scope].md
    cross-platform-apps.md
```

Open Forge does not require a folder named `projects`, `scope`, `domain`, or `team`. Use those only when they improve your local routing.

Scope placement changes meaning:

```text
.agents/memory/crystallized/[scope]/decisions/
.agents/memory/[scope]/crystallized/decisions/
```

The first means `[scope]` is inside crystallized memory. The second means `[scope]` owns its own memory states. Both are valid when every folder has an `entrypoint` and the `entrypoint` descriptions make the scope clear.

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

When a legacy category `entrypoint` or loader has a final `## Entries` section containing only generated list `entries`, the CLI adds the markers automatically. If the heading is absent, the CLI appends the complete section. Malformed or non-final markers stop generation without changing the file.

Change index output by adding, editing, moving, or removing route files and child category `entrypoints` in the indexed folder.

The index generator reads:

- direct `*.md` route files, including underscore-prefixed routed files
- direct child category `entrypoints` named `_{folder-name}.md`
- direct child skill packages under `.agents/skills/` that contain `SKILL.md`

Inside `.agents/skills/`, loose markdown files are not indexed as skill routes. Use skill folders with `SKILL.md`.

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

Runtime-native `SKILL.md` files should keep native metadata such as `name` and `description`. The CLI reads the root `description` for generated skill entries and defaults their tag to #Skill when no Open Forge tags are present.

## find

```sh
open-forge find --tag KeepInMind --bodies
open-forge find --tag Decision --tag Routing
open-forge find --tag Workflow --tag PhaseDelivery
open-forge find --route .agents/workflows/dev/_dev.md --follow-required --bodies
open-forge find --route .agents/memory/crystallized/_crystallized.md --depth 1
open-forge find --tag Workflow --json
```

`find` is deterministic routing-contract lookup, not search. It walks routed files only - the loader, category `entrypoints`, their direct route files, and native skill package entrypoints - and never guesses relevance.

Combine `Workflow` with one of `PhaseDiscovery`, `PhaseDefinition`, `PhasePlanning`, `PhaseDelivery`, or `PhaseVerification` to inspect phase candidates. Those tags provide non-waterfall wayfinding; the workflow Goal and routed current truth still decide fit.

- `--tag <Tag>` filters by effective tags (metadata tags, or the generated defaults); repeat the flag to require every tag. Matching is case-insensitive; canonical spelling still comes from the loader.
- `--route <path>` selects one file or routable folder; `--depth <n>` also follows its generated `entries` n levels.
- `--follow-required` adds every target of the selected files' `## Required Routes` sections. A required route that cannot be read fails the command - it is a blocker, not a skip.
- Output is entry lines by default; `--paths` prints paths only, `--bodies` prints file contents with `----- {route} -----` separators, `--json` prints structured output.

`open-forge find --tag KeepInMind --bodies` is the deterministic complete routed #KeepInMind lookup. Run it at task start or resume, after context restoration or compaction, at meaningful phase transitions or handoffs, and before closeout; every result remains binding follow-up context within the authority of its owning content. The command proves which routed files match, not that an agent actually read or performed their instructions, so it is not an execution receipt.

Explicit routes, generated-entry expansion, and Required Routes are lexical and physical containment boundaries. `find` rejects absolute paths, parent traversal, drive changes, and real-path link escapes from the selected target. Global routed discovery likewise rejects linked or special entries before reading them.

## chain

```sh
open-forge chain .agents/patterns/react/components.md
open-forge chain .agents/patterns/react/components.md --heading Axioms
open-forge chain .agents/workflows/dev/_dev.md --heading Constraints --json
```

`chain` explains inherited Markdown context for one routed file. It emits the loader, each visible ancestor category `entrypoint`, a native skill's `SKILL.md` when the target is inside a skill package, the target, and every existing `.overwrite.md` companion in base-then-overwrite order. With no `--heading`, it lists the route chain. With `--heading`, it also reports every matching section from every chain member as `content`, `absent`, `empty`, `declared-inherited`, or `declared-none`.

The heading is arbitrary, so the same command can inspect Axioms, Mode, Goal, Constraints, or a local category heading. A missing, empty, `inherited`, or `none` local category Axioms section contributes no local axioms; it never disables loaded ancestor axioms. `--json` provides stable structured output for tools.

Routes are resolved inside the selected logical target. Absolute paths, parent traversal, drive changes, and real-path or symlink escapes are rejected without mutation. `chain` reports the currently visible file chain; run `doctor` when route-index continuity itself must be validated.

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
- workflow recipes whose level-2, ordered Mode, Goal, Required Routes, Constraints, Steps, Loop, Outputs, and Completion contract is missing or invalid (error); category-only workflow `entrypoints` may omit the recipe contract until they declare any recipe heading
- direct directive files without exactly one substantive level-2 `Axioms` section (error)
- directive files that retain the legacy `Applies To` second applicability gate (error)
- complete workflow recipes without exactly one recognized primary phase tag (error)
- category Axioms sections that mix an inherited/none sentinel with substantive local axioms (warning)
- stale generated regions that no longer match what `index` would produce (warning; run `open-forge index`)
- retired load-policy tags in metadata (warning)
- `.overwrite.md` companions without a base file (warning)
- markdown files not reachable through generated routing (warning)

Exit code is non-zero when errors exist, so `doctor` is safe for CI. `index` remains the repair tool for generated regions; `doctor` only reports.

`doctor` uses the same target-containment boundary as `find`: an external symlink/junction route tree or linked routed entry is an error and is not consumed as workspace context.

## create

```sh
open-forge create category patterns/react/components
open-forge create category .agents/memory/crystallized/mobile-app
open-forge create extension my-patterns
```

`create category` scaffolds a route chain: every missing folder in the path gets a canonical `_{folder}.md` `entrypoint` with placeholder metadata, a type tag inherited from the nearest recognized primitive segment, an explicit inherited Axioms sentinel, and an empty generated region, then all indexes are rebuilt. This lets `workflows/frontend/patterns/` remain a Pattern route inside a workflow scope. It refuses paths that are already routable. Fill in the TODO descriptions, then run `open-forge index` again. A local category may instead omit Axioms, leave it empty, or state `none`; all four shapes mean no local additions while loaded ancestor axioms remain active.

`create extension` scaffolds a managed extension package: `extension.json` with the requested stable `id` plus starter `name`, `description`, `version`, `dependencies`, and `augmentations` fields; a README with authoring rules; and an empty `payload/.agents/` tree ready for routed files. Install it with `open-forge extend <directory-or-id>`.

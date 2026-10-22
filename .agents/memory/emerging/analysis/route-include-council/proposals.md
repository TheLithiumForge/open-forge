---
open-forge:
  description: The eight Stage 1 proposals of the route include council, kept as evidence
  tags: [Memory, Analysis, Contextual, Candidate, Framework, CLI, Council]
---

# Council 4 Proposals

The eight independent Stage 1 proposals, each under its member's perspective and Stage 2 label. Links point at the repository as it stood on the #5 branch. The [record](record.md) holds the decisions.

## The CLI Product Designer (Proposal D)

### 1. Thesis

Add `open-forge route include` to use knowledge already in the project, alongside Extensions for packaged reuse and Libraries for linked reuse. Keep the loader authoritative for what agents use, with config recording which sources the command registered. Ship that in #6, then make the main context root configurable in #7 without changing existing defaults.

### 2. The concept and its name

The one new word should be **include**. An include exposes an existing project file or catalogue to Open Forge while leaving the document where it is.

The three forms of reuse answer related questions:

- `extension install` brings in a distributable package of complete files.
- `library attach` exposes a registered source inventory through relative file links.
- `route include` uses a document already at its intended project location.

Keep that distinction honest. Today Libraries operate inside one workspace. The [Library contract](../../../../../.agents/memory/crystallized/documents/cli/contracts/library/attach/interface.md) requires both source containment and destination validation. #6 should not quietly turn Libraries into arbitrary cross-workspace sharing.

Choose a **`route` subcommand**, with `route exclude` as its inverse. Including a catalogue establishes another route into knowledge. Including a leaf establishes a route ending at that file.

Reject **`link`** because it could mean a Markdown reference, a filesystem symlink, or Library projection. “Cannot link this file” would leave the user guessing which operation failed.

Reject **`route add`** because “add” does not distinguish creating content from exposing existing content. Beside `route create`, newcomers would reasonably expect aliases.

Reject a top-level **`root`** group because users first want to use a file, not configure a filesystem model. A single file also makes “add a root” needlessly abstract.

The existing contracts leave a clear boundary. Create’s purpose is “Create one routed Markdown file below an existing root route.” Init’s purpose is “Create the missing navigation files in one exact route chain.” Include creates neither document nor catalogue. It exposes an existing one, including sources outside the main root.

### 3. The CLI surface

The proposed grammar is:

```text
open-forge route include <workspace-path>
  --description <text>
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global options]

open-forge route exclude <workspace-path>
  [--dry-run]
  [global options]

open-forge route list [<source-reference>]
  [--depth <non-negative-integer|all>]
  [--included]
  [global options]
```

Global options remain `--workspace`, `--format`, `--detail`, repeatable `--detail-filter`, `--help`, and `--version`, with their existing values and defaults.

Include and Exclude apply their checked plans directly, like Route Create and Init. They have no wizard, `--automatic`, `--force`, destination option, or package identity.

The user-facing help should say:

```text
Include an existing project file or catalogue in agent context.

Usage:
  open-forge route include <workspace-path> --description <text> [options]

Behavior:
  The source stays where it is.
  An ordinary text file is read as one document.
  A catalogue such as docs/_docs.md exposes its Entries.
  Without a loading tag, agents open it when its description fits the task.

Options:
  --description <text>  Explain when the source is useful. Required.
  --tag <tag>          Add an entry tag. Repeat for additional tags.
  --apply-to <glob>    Limit applicability to working files. Repeat as needed.
  --dry-run            Show the changes without writing.

Related commands:
  route exclude       Stop including a source, keeping its files.
  route create        Create a new routed Markdown file.
  route init          Create missing catalogue files.
```

Three examples, assuming the sources exist:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow --dry-run
open-forge route include .cursor/rules/testing.mdc --description "Testing rules" --tag LoadNow --apply-to "src/**" --dry-run
open-forge route include docs/_docs.md --description "Project design and operating knowledge" --dry-run
```

Review each preview, then repeat without `--dry-run`.

An ordinary text file is an opaque leaf. Its foreign frontmatter and activation settings do not acquire Open Forge meaning. The loader line supplies its description, tags, and conditions.

A Markdown file named `_{folder}.md` is a catalogue. Follow its Entries recursively, applying the loader’s existing ancestry and same-working-file condition rules. Preserve recognized compatibility catalogue names when unambiguous. An arbitrary Markdown file containing links does not become a catalogue.

Removal and listing are explicit:

```sh
open-forge route exclude docs/_docs.md --dry-run
open-forge route list --depth 0
open-forge route list --included --depth 0
```

Exclude removes the matching loader entry and registration. It never deletes source files or writes `removedFiles`. It reports remaining exposure through other catalogues. Repeated exclusion is a no-op, including when the source has disappeared.

Include validates an existing readable text file, workspace containment after physical resolution, valid entry metadata, and unambiguous catalogue identity. Reject URLs, fragments, globs, physical escapes, and catalogue cycles. Normalize command paths to portable workspace-relative paths.

Errors should explain the next action:

```text
Cannot include docs: this path is a directory.
Name its catalogue, such as docs/_docs.md, or one individual file.

Cannot include docs/architecture.md: the file does not exist.
Check the path. To create new routed content, use route create.

Cannot include ../team/rules.md: the target is outside this workspace.
```

Invalid arguments return the existing `invalid-input` result. Unsafe or malformed workspace boundaries return `blocked`. Unavailable evidence returns `incomplete`. A failed preflight changes nothing.

An identical repeated Include is a no-op. An existing handwritten loader entry is registered without duplication. Explicit metadata options update that selected entry. Conflicting duplicate rows require correction instead of an arbitrary winner.

The other commands consume the same source model:

- **`context`** follows included catalogue chains and returns opaque leaves under their entry metadata. Missing required sources remain errors.
- **`route list`** includes active handwritten and command-added sources. `--included` selects registrations, including those currently switched off, with path, file or catalogue form, and availability.
- **`find`** searches entry tags and supported Markdown headings without requiring foreign metadata.
- **`references`** reports loader and catalogue edges, plus supported Markdown references. Opaque foreign files do not become another tool’s interpreted syntax.
- **`status`** reports registrations and effective activation. Its startup estimate includes required sources outside `.agents`, deduplicated by physical identity, with unavailable bytes reported honestly.
- **`doctor`** diagnoses missing targets, malformed catalogues, cycles, aliases, and inconsistent registrations without treating foreign leaves as missing-frontmatter defects.
- **`index`** preserves included loader rows and validates their reachable catalogues. Included sources remain author-maintained in #6. Index does not insert metadata, create catalogues, or rewrite their Entries. Its report distinguishes checked sources from generated navigation it changed.

Handwritten links remain first-class. Registration adds CLI tracking, not stronger loading authority or automatic maintenance permission.

### 4. The config

For #6, add this to `.agents/open-forge.json`, preserving other settings:

```json
{
  "schemaVersion": 2,
  "roots": [
    "CONTRIBUTING.md",
    ".cursor/rules/testing.mdc",
    "docs/_docs.md"
  ]
}
```

`roots` records exact included source paths. A root is a retrieval starting point and may end at a leaf. Do not duplicate descriptions, tags, conditions, or enabled flags in JSON.

For #7, the same settings become:

```json
{
  "schemaVersion": 2,
  "mainRoot": "knowledge",
  "roots": [
    "CONTRIBUTING.md",
    ".cursor/rules/testing.mdc",
    "docs/_docs.md"
  ]
}
```

`mainRoot` names the context directory containing `loader.md`. Extra paths remain relative to the workspace, regardless of the main root.

Extend `schemas/v2/open-forge.schema.json` with a unique array of nonempty path strings for `roots`, defaulting to `[]`, and a nonempty directory-path string for `mainRoot`, defaulting to `.agents`. Runtime validation handles containment, existence, aliases, and reserved controls. Preserve `additionalProperties: true`.

Both additions are optional and retain existing field meanings, so keep schema version 2. This follows the [current schema’s](../../../../../schemas/v2/open-forge.schema.json) stated compatibility policy: “Unknown keys are accepted and ignored”.

Include renders its metadata into the loader:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
- [Testing rules](../.cursor/rules/testing.mdc) - #LoadNow - applies to `src/**`
- [Project design and operating knowledge](../docs/_docs.md)
```

Links are calculated relative to the actual loader location.

**The loader wins for activation and entry metadata.** The [loader](../../../../../src/open-forge/.agents/loader.md) says, “The list under `Entries` at the bottom is the whole setup.”

A registered path whose loader line was removed stays off. A loader line without registration remains an active handwritten include. Adding a config path alone does not activate it. Index and Update never reconstruct deleted loader rows from config. Re-running Include is an explicit activation request.

### 5. `help concepts` and the README guide

Add `open-forge help concepts`. Bare `open-forge help` lists topics, while unknown topics show available choices. Existing `--help` behavior stays intact.

Concept help works outside an installed workspace, never fetches content, and supports normal text and JSON output. JSON returns ordered topic sections inside the existing result envelope. Keep one Markdown source, proposed as `docs/concepts.md`, embedded in the CLI and published on the docs site.

Its full outline should be:

- **What Open Forge does** explains readable context and optional deterministic assistance.
- **Choose what agents use** explains loader membership and switching branches off.
- **Files and catalogues** explains leaves, Entries, catalogue names, and compatibility names.
- **Routes and scopes** explains ancestry, root routes, scopes, and slugs.
- **Loading and conditions** explains on-demand reads, LoadNow, KeepInMind, and same-file condition matching.
- **Authority and acceptance** separates reading, rules, accepted knowledge, and edit permission.
- **Content roles** explains Directives, Guidance, Maps, Patterns, Templates, and Skills.
- **Memory** explains its four states and optional saving policy.
- **Reuse** compares includes, Libraries, and Extensions.
- **Authoring** introduces metadata and links to the complete file-format instructions.
- **Customization** explains overwrite companions and independent Template copies.
- **Maintenance** separates authored settings, generated navigation, ownership, exclusions, and recovery.
- **Native tools** explains harness discovery and the limits of Open Forge loading.
- **Without the CLI** gives the equivalent file-editing procedure.
- **Build your own framework** encourages replacing defaults with useful local rules and knowledge.

The current README says, “Agents read it only when they write workspace files.” That makes deletion unsafe while the loader still requires `README.md#instructions`.

Move those Instructions, intact in meaning, to a shipped, non-routed `authoring.md`. Change the loader’s conditional authoring read to that file. It is a format reference, not a shipped authoring Directive or another startup read.

The rules explain relative references, useful tags, narrow placement, catalogue names, inherited Axioms, scope nesting, required routed metadata, root versus scoped frontmatter, `applyTo`, synchronized Entries, native Skill exceptions, and overwrite companions. They matter because navigation and loading depend on consistent representations. Existing opaque project files remain exempt from adopting routed frontmatter.

Remove the README’s binding Instructions, detailed vocabulary, and CLI reference material. Its new outline should be:

- **You may remove this guide** explains safe removal and recording the exclusion when updates are managed.
- **Use one document you already have** gives Include and manual-entry examples.
- **Make your own rules** shows a small project-specific Directive.
- **Keep useful knowledge** shows an accepted fact and an optional resume note.
- **Reuse what works** introduces the three reuse choices.
- **Add only what helps** explains removable defaults and dependencies.
- **Check the result** points to Context, Status, and concept help.

### 6. The configurable main root (#7)

Introduce one resolved workspace layout shared by source resolution, navigation, installation, permissions, and lifecycle operations. `mainRoot` changes context storage, not workspace selection or the fixed bootstrap settings location.

Keep `.agents/open-forge.json` and `.agents/open-forge.lock.json` discoverable at their current paths. This deliberately leaves a small `.agents` control directory when knowledge lives elsewhere. Moving those controls would be another compatibility change and should not be hidden inside “custom root”.

Fresh interactive Install asks:

> Where should Open Forge keep this project’s context?

Offer `.agents` as the default and an alternative workspace-relative folder. `--main-root <directory>` supplies the answer. Unattended omission retains `.agents`. Changing an installed workspace requires `install --configure`.

Skills retain harness-native locations. Do not move `.agents/skills`, `.claude/skills`, or other native discovery directories merely because context moved. A custom-root loader can expose their catalogue or selected Skill files without creating a Skills category inside the custom root. Native activation remains controlled by the harness.

Separate embedded package paths from installed destinations. Package content under `content/.agents/` generally maps into `mainRoot`, while native Skill payload uses its declared native destination. Extra roots grant discovery, never implicit write permission.

Migration must be explicit:

```sh
open-forge install --configure --main-root knowledge --dry-run
```

The reviewed plan inventories authored notes, managed files, overwrites, private records, links, exclusions, and receipts. Move eligible context, preserve pinned native packages and controls, rewrite proven references, update host pointers, and retain recovery evidence. Block collisions. Editing `mainRoot` manually selects a layout but does not authorize moving files.

Implement in ordered slices, freezing shared contracts before dependent work:

1. Define layout, bootstrap, permission, package-mapping, and migration contracts.
2. Add settings and the resolved layout with unchanged default behavior.
3. Separate logical package identity from physical source paths.
4. Adapt discovery, Context, Find, References, navigation, Status, and Doctor.
5. Adapt Install, host sections, native Skills, and destination permission checks.
6. Adapt Update, removal, Route mutation, Extension delivery, and Library consumers.
7. Implement reviewed migration and interruption recovery.
8. Update documentation and verify default-root parity, custom-root journeys, supported platforms, and Native AOT.

Each slice has one bounded responsibility suitable for a separate worker after its dependencies land. Estimate **15–25 engineer-days**, including integration and verification. The repeated `.agents` assumptions make this a structural change, not a constant replacement.

Older CLIs ignore `mainRoot`. They therefore cannot safely maintain a custom-root workspace. Document and pin the minimum supporting CLI release.

### 7. Big changes to CLI usage

For #6, users gain Include, Exclude, `route list --included`, and `help concepts`. Read commands accept exposed project files and catalogues outside `.agents`. Route listings and Status include those sources, and startup estimates can increase because their coverage is more complete.

Include requires an explicit description. Exclude keeps files, while existing Route Remove remains a deletion operation. Extra project sources stay outside automatic metadata completion.

The README becomes removable because authoring instructions have another shipped home. Existing handwritten loader entries continue working without registration.

For #7, users gain `install --main-root` and the installation question. Source IDs relative to the main root remain stable, while exact physical paths and loader links may change. Built-in context destinations follow the main root. Native Skills keep native destinations. External package destinations still require applicable grants.

Default-root workspaces retain their existing commands, presets, and locations.

### 8. Documents to update

For #6, update these exact public files:

`README.md`, `docs/cli.md`, `docs/extensions.md`, and new `docs/concepts.md`.

Update these site files under `src/docusaurus/`:

`sidebars-guides.ts`, `docs/cli/index.md`, `docs/cli/flows.md`, `docs/glossary.md`, `docs/getting-started/installation.md`, `docs/getting-started/ten-minute-guide.md`, `docs/getting-started/grow-your-framework.md`, `docs/concepts/index.md`, `docs/concepts/routing.md`, `docs/concepts/loading-and-tags.md`, and `docs/concepts/customizing.md`.

Update `src/open-forge/.agents/loader.md`, `src/open-forge/.agents/README.md`, and add `src/open-forge/.agents/authoring.md`. Update `schemas/v2/open-forge.schema.json`.

Update `src/open-forge/.agents/skills/open-forge-cli/SKILL.md` and all four references: `references/routes.md`, `references/discovery.md`, `references/packages.md`, and `references/common.md`. Keep their `.agents/skills/open-forge-cli/` workspace counterparts in step, along with `.agents/loader.md`, `.agents/README.md`, and the new `.agents/authoring.md`.

Under `.agents/memory/crystallized/documents/cli/`, update `command-contract-set.md` and `contracts/route/_route.md`. Add `_include.md`, `interface.md`, and `behavior.md` under `contracts/route/include/`, with corresponding `_exclude.md` files under `contracts/route/exclude/`. Add `contracts/help/_help.md` and `_concepts.md`, `interface.md`, and `behavior.md` under `contracts/help/concepts/`.

Update both `interface.md` and `behavior.md` in these exact existing contract directories:

`contracts/context/`, `contracts/find/`, `contracts/references-candidate/`, `contracts/index-candidate/`, `contracts/status/`, `contracts/doctor/`, `contracts/route/list/`, `contracts/route/inspect/`, `contracts/shared/source-references/`, and `contracts/shared/source-universe-filters/`.

For #7, revisit the same public references and add `src/open-forge/AGENTS.md`, `src/open-forge/CLAUDE.md`, their workspace counterparts, and `src/open-forge/.agents/skills/_skills.md`.

Update CLI `architecture.md`, `technical-designs/workspace-libraries.md`, and both contract files under `install/`, `update/`, `remove/`, every existing `route/` leaf, every `extension/` leaf, every `library/` leaf, and `shared/workspace-permissions/`, all beneath `contracts/`.

Update the Framework documents `framework/routing/model.md`, `framework/routing/paths.md`, `framework/routing/loading.md`, and `framework/markdown/routes.md` beneath `.agents/memory/crystallized/documents/`.

For both PRs, update maintenance `payload/agents/loader.md`, add `payload/agents/readme.md` and `payload/agents/authoring.md`, and register them in `payload/agents/_agents.md`. For #7, also update `payload/AGENTS.md`, `payload/CLAUDE.md`, and `payload/agents/skills.md`. These paths are beneath `.agents/memory/crystallized/documents/maintenance/`.

### 9. Risks

The likeliest failure is restoring a loader line because config still records it. Catch that immediately with an include, manual deletion, Index, Update, Configure, and Extension lifecycle journey that must leave it off.

The next risk is interpreting existing documents as Open Forge-owned content. Verify byte preservation for foreign frontmatter, opaque text, catalogue prose, and excluded branches.

Custom-root failures will concentrate in package mapping, permissions, host pointers, native Skills, and migration of private files. Exercise one connected custom-root journey before adapting every command.

Naming also needs evidence. Ask unfamiliar users which command uses an existing document, creates a new one, and stops reading one while preserving it. Confusion between Include, Create, and Remove is an early product failure.

This proposal is based on read-only inspection. No files were changed, and no Doctor command, build, or test suite was run.

### 10. The one idea to steal

Store the included paths in config, but keep every activation choice visible in the loader that agents actually read.

## The CLI Architect (Proposal A)

### Thesis

Add `route include` to register existing project knowledge and expose it through ordinary loader entries. Reuse the shared source catalogue, topology, loading closure and navigation planner, while preserving the loader as the authority for what is enabled. Make configurable storage a separate change built on an immutable workspace layout.

### The concept and its name

Users should learn **include**: an existing project file or catalogue made available to Open Forge where it already lives.

The three mechanisms describe different reuse arrangements. Extensions distribute complete files with installation and update management. Libraries expose a local source through projected file links. Includes use existing project knowledge directly, without copying files or creating filesystem links.

There is an important current boundary. Libraries are described as computer-local reuse, but their implementation requires a workspace-contained source root. The [Library contract](../../../../../.agents/memory/crystallized/documents/cli/contracts/library/attach/behavior.md) calls it “strictly contained by the selected workspace lexically and physically.” #6 should explain that constraint without widening it.

Choose two new `route` subcommands:

```text
open-forge route include <path> ...
open-forge route exclude <path> ...
```

“Link” is unsuitable because Library already creates actual relative file links. “Route add” leaves users guessing whether it creates content, installs a category or registers existing content. A top-level `root` group would overstate a feature that accepts individual files and does not recursively adopt arbitrary directories.

`route include` fits the existing group’s purpose of exposing files through navigation. Its rules remain distinct from `route create`, which creates ordinary Markdown beneath an existing root, and `route init`, which creates missing catalogue chains. Include requires existing content and creates neither content nor catalogues.

### The CLI surface

The proposed syntax is:

```text
open-forge route include <workspace-relative-file>
  --description <text>
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-relative-file>
  [--dry-run]
  [global flags]

open-forge route list [<source-reference>]
  [--includes]
  [--depth <nonnegative-integer|all>]
  [global flags]
```

Description is required and nonblank. Tags default to an empty list, so inclusion is on demand unless a loading tag is supplied. Include and Exclude apply directly after preflight, like Create and Init. They need no `--automatic`.

Three examples:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow

open-forge route include docs/_docs.md --description "Project architecture and decisions"

open-forge route include .cursor/rules/testing.mdc --description "Testing conventions" --tag LoadNow --apply-to "src/**"
```

Use `--dry-run` first to inspect either mutation. Removal deletes the registration and its loader exposure, while preserving the target file. Ordinary `route remove` retains its existing deletion meaning.

A normal text file is a leaf. A recognized Markdown catalogue, including `docs/_docs.md`, exposes its declared Entries recursively. Existing compatibility entrypoint names remain supported. Inclusion does not scan sibling files or manufacture a catalogue. Another tool’s frontmatter does not determine Open Forge loading.

Paths resolve from the selected workspace. Reject missing targets, directories, binary or undecodable content, workspace escapes, protected controls, overwrite companions and unsafe physical aliases. Directory errors suggest the exact catalogue filename. Targets already belonging to the main routed tree receive an explanation directing users to its existing route commands.

Reject malformed conditions, duplicate singleton options and conflicting duplicate exposures before writing. Repeating an identical request is a verified no-op. An explicit Include request may replace that target’s registration metadata and deliberately restore its missing loader row.

Included sources accept exact `./docs/...` references in reading commands. Existing automatic IDs remain unchanged. Do not invent synthetic `.agents` identities for external files.

| Command | Included-source behavior |
| --- | --- |
| `context` | Uses loader exposures, required tags and complete inherited condition chains. Missing required content remains an incomplete result. |
| `route list` | Shows enabled included roots alongside ordinary roots. `--includes` selects inclusion inventory and also reports disabled registrations. |
| `route inspect` | Explains the actual path, exposure, conditions, registration and read-only maintenance boundary. |
| `find` | Searches included text and entry metadata within its declared inventory. Discovery does not activate rules. |
| `references` | Reports inclusion edges and supported authored references, with existing scan limits and ignore-setting behavior. |
| `index` | Maintains ordinary routes and preserves included loader rows. It neither fills external frontmatter nor rewrites included catalogues. |
| `status` | Reports registered, enabled, disabled and unavailable includes. Startup estimates include actual required external reads and disclose unresolved conditions. |
| `doctor` | Diagnoses missing targets, malformed catalogues, cycles, conflicting exposures and registration drift. Disabled registrations are informational. |

Hand-written loader links remain valid without registration. Include adopts an identical existing row instead of adding another. Conflicting rows require an explicit resolution.

The implementation should promote the file-reading mechanics of `ContextLinkedEntryReader` into Framework. Today it is command-local, while `SourceReadSessionReader` inventories only `[SourceLogicalPath.AgentsRoot]`. Extend the shared catalogue with path-keyed linked observations and pass their declared exposure edges into `SourceRouteTopologyBuilder`. Retain the ordinary filesystem-derived topology for native routes.

All readers should consume this same view. Status and Doctor already aggregate through `RouteOperationalContributor`. Their route observations and `RouteContextReader` should consume the expanded inventory and shared loading closure. Index’s independent catalogue construction in `IndexProjectionReader` must use the same reader, with an explicit write policy excluding included targets.

For navigation, extend `GeneratedNavigationRegionPlanner` with typed loader-row changes. Its current `TryReadDestination` accepts descendants, so included destinations need containing-file-relative encoding that permits `..`. Merge rows by resolved destination, preserve untouched authored rows byte-for-byte, and retain `BuildGeneratedChange` for bounded edits. The current `ReadAuthoredLinks` heuristic recognizes `..` rows, which must not remain the sole provenance test.

Each new leaf still needs its normal binding, definitions, operation, models, rendering, OutputText, source-generated JSON registration, help and three-project evidence.

### The config

For #6, add this shape to `.agents/open-forge.json`, preserving existing settings:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v3/open-forge.schema.json",
  "schemaVersion": 3,
  "roots": [
    {
      "path": "docs/_docs.md",
      "description": "Project architecture and decisions",
      "tags": [],
      "applyTo": []
    }
  ]
}
```

`roots` contains exact entry files, including leaf files. It grants discovery and registration, not recursive directory ownership or write permission.

For #7, the corresponding configuration is:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v4/open-forge.schema.json",
  "schemaVersion": 4,
  "mainRoot": "knowledge",
  "roots": [
    {
      "path": "docs/_docs.md",
      "description": "Project architecture and decisions",
      "tags": [],
      "applyTo": []
    }
  ]
}
```

`mainRoot` defaults to `.agents`. Every path remains workspace-relative, regardless of the settings file’s location.

Publish v3 and v4 schemas rather than editing published v2. Validate root-record shapes, descriptions, tags and conditions. Runtime validation handles normalized duplicates, containment and physical aliases. Preserve tolerant reading, unknown keys and actionable key-specific errors.

Extend `WorkspaceSettingsDocument` and `WorkspaceSettingsCodec`. Use `WorkspaceSettingsReader` snapshots and `WorkspaceSettingsChangePlanner` to plan settings and loader edits together under existing mutation and recovery mechanics. `WorkspaceSettingsWriter` currently owns allow-list grants, so it should not become a second inclusion writer.

Include renders the first example as:

```markdown
- [Project architecture and decisions](../docs/_docs.md)
```

From `knowledge/loader.md`, the relative destination happens to be the same. Deeper main roots produce different relative paths.

**The loader wins when they disagree.** Config tracks registrations and last-requested metadata. Loader rows determine effective activation, descriptions, tags and conditions. A config-only record is disabled. Deleting its loader row never authorizes Index or Update to restore it. Doctor reports drift, and an explicit Include request reconciles it.

This preserves the shipped [loader promise](../../../../../src/open-forge/.agents/loader.md): “A removed line stays removed until the user asks to restore it.”

### `help concepts` and the README guide

Implement `help` as a direct command with a topic operand:

```text
open-forge help [<topic>] [global flags]
```

Bare `help` lists topics. Initially, `concepts` is the supported topic. Unknown topics return invalid input and list valid choices. `--help` continues to explain command syntax.

Concept help needs no installed workspace and performs no filesystem discovery. Text and JSON render the same typed topic result through the existing pipeline.

The complete concepts outline is:

- **Purpose and ownership** — Plain files contain the framework and users choose its parts.
- **Loader and Entries** — Entries expose knowledge and control activation.
- **Files and catalogues** — Leaves supply content and entrypoints expose another level.
- **Routes, scopes and slugs** — Navigation and placement narrow applicable context.
- **Loading and conditions** — Required tags, on-demand selection, same-file matching and pending conditions.
- **Authority and acceptance** — Reading, registration and ownership do not establish accepted truth.
- **Content roles** — Directives, Guidance, Patterns, Maps, Templates and native Skills.
- **Memory** — Working, Emerging, Crystallized and Archived serve different states.
- **Reuse** — Includes, Libraries and Extensions.
- **Customization** — Omissions, overwrites and independent Template copies.
- **Maintenance** — Context, discovery, Index, diagnosis and managed updates.
- **Authoring** — Where the file-format instructions live and how to build your own framework.

Remove the README’s normative `## Instructions` and detailed vocabulary reference. Retain short explanations where a recipe needs them.

Its new outline should be:

- **You may remove this guide** — It is optional and nothing operational depends on it.
- **Use one document you already have** — Equivalent command and manual-entry examples.
- **Write your own rules and notes** — Start from a real project need.
- **Add a catalogue when navigation helps** — Grow from actual use.
- **Reuse knowledge elsewhere** — Choose an include, Library or Extension.
- **Turn something off** — Remove exposure and understand maintenance exclusions.
- **Customize a shipped file** — Preview updates and use overwrites where appropriate.
- **Find the next capability** — Concepts help, CLI help and documentation.

Explain persistent guide removal using the existing `remove .agents/README.md` operation, or manual deletion plus its `removedFiles` exclusion.

Move authoring instructions to `.agents/authoring.md`, an on-demand format reference outside Directives. Change the loader’s authoring link to it and exempt it from ordinary routed-file metadata checks.

Those rules remain necessary. They define catalogue filenames, Entries syntax, metadata forms, conditions, native Skill exceptions, inherited axioms, relative links and overwrite behavior. They let agents create interoperable files without guessing. Preserve the existing link-and-tag conventions and narrow-source placement guidance too. The CLI accelerates this work, but an agent without it still follows the loader’s explicit authoring read.

### The configurable main root (#7)

Resolve one immutable `WorkspaceLayout` before source discovery. It carries the main root and derived loader, guide, authoring and ownership paths. Pass narrow layout views to consumers. Avoid mutable static configuration.

Keep `.agents` as the default and as the package asset prefix. Map packaged paths into the configured workspace layout during delivery. Preserve automatic IDs relative to the main root, so `memory/crystallized` survives relocation.

Replacing `SourceLogicalPath.AgentsRoot` alone is insufficient. `SourceIdentity` separately slices the literal `.agents/`. Other dependencies include classification, catalogue requests, destination resolution, scaffold chains, sharing, permission boundaries, ownership, lifecycle projection and native Skill traversal.

Settings discovery needs an independent anchor. For custom-root installations, use workspace-root `open-forge.json`. Continue accepting legacy `.agents/open-forge.json`. If both exist, report ambiguity without merging. The #7 JSON above is valid at the legacy location, but a reviewed relocation moves settings to the workspace-root anchor. Changing a key alone does not move files.

First interactive Install asks, “Where should Open Forge keep its context files?” Offer `.agents` as recommended and an explicit alternative. Add `--main-root <directory>` for reproducible requests. Existing installations retain their location unless explicitly configured.

Keep Skills at their existing harness-native locations. With a custom root, Skills cease to be a category physically beneath that root. A native Skills catalogue may be included when wanted. Preserve existing native discovery and do not imply that moving a `SKILL.md` beneath an arbitrary knowledge folder makes a harness discover it.

Migration uses reviewed `install --configure --main-root <directory>` planning. It inventories authored content, ownership, references, exclusions and Library projections. It updates managed `AGENTS.md` and `CLAUDE.md` handoffs, rebases supported links and preserves loader omissions. Collisions or unprovable link rewrites block migration before effects.

Implement in ordered worker-sized slices, with ownership frozen before assignment:

1. **Layout and bootstrap contracts** — Resolve default and custom layouts, settings discovery and ambiguity.
2. **Identity and inventory** — Remove runtime prefix assumptions while preserving source IDs and default behavior.
3. **Topology and reading** — Migrate destination resolution, closures, source references and operational observations.
4. **Navigation and scaffolding** — Migrate formation, projection, Index and route creation paths.
5. **Delivery and permissions** — Map payload paths, exclusions, ownership and permission checks.
6. **Skills and Libraries** — Preserve native locations and correctly recreate affected relative projections.
7. **Install and relocation** — Add the question, option, complete migration plan and recovery behavior.
8. **Documentation and qualification** — Verify representative custom-root journeys and default-root compatibility.

Estimate 15–25 engineering days including documentation and compatibility evidence. The first default-layout slice should preserve existing snapshots. Add focused alternate-layout scenarios instead of replacing thousands of expected strings mechanically.

### Big changes to CLI usage

For #6, users gain Include, Exclude, `route list --includes`, exact `./...` references for included sources and `help concepts`. Discovery and diagnostics cover included knowledge, and startup estimates can increase. Included files remain outside automatic metadata completion and catalogue rewriting.

The README becomes safely optional, while authoring guidance moves to a separate conditional read. Existing commands and manual loader editing remain available.

For #7, users gain `--main-root`, an installation question and explicit relocation through Configure. Custom-root settings use the workspace-root anchor, reported paths change, and Skills retain their native locations. Package authors retain `content/.agents/` as the distribution convention.

Older CLIs can ignore #6’s unknown registration keys and still use its rendered Markdown. Custom-root mutation requires a #7-capable CLI. Unknown-key tolerance cannot make an older executable root-aware.

### Documents to update

The following paths are relative to the repository. Brace lists identify each named file.

For #6:

- `README.md`, `docs/cli.md` and `docs/extensions.md`.
- `src/open-forge/.agents/{loader.md,README.md,authoring.md}` and the repository loader counterpart.
- Both `src/open-forge/.agents/skills/open-forge-cli/` and `.agents/skills/open-forge-cli/`, updating `SKILL.md` and `references/{common.md,discovery.md,routes.md,packages.md}`.
- New `schemas/v3/open-forge.schema.json`.
- Under `.agents/memory/crystallized/documents/cli/`, update `command-contract-set.md`, `architecture.md` and `technical-designs/generated-navigation.md`.
- Add contract triples `contracts/route/include/{_include.md,interface.md,behavior.md}`, `contracts/route/exclude/{_exclude.md,interface.md,behavior.md}` and `contracts/help/{_help.md,interface.md,behavior.md}`.
- Update `contracts/_contracts.md`, `contracts/route/_route.md` and the interface/behavior pairs under `route/{list,inspect}`, `context`, `find`, `references-candidate`, `index-candidate`, `status`, `doctor` and `shared/source-references`.
- Update `framework/routing/{model.md,paths.md,loading.md}` and `framework/markdown/{syntax.md,routes.md}` beneath `.agents/memory/crystallized/documents/`.
- Update `maintenance/payload/agents/loader.md` beneath that same base. Add guide and authoring maintenance documents there and register their catalogue entries.
- Update `src/docusaurus/docs/{intro.md,glossary.md}`, `concepts/{index.md,routing.md,loading-and-tags.md,customizing.md}`, `getting-started/{installation.md,ten-minute-guide.md,greenfield-and-brownfield.md,grow-your-framework.md}`, `cli/{index.md,flows.md}` and `extensions/index.md`.

For #7:

- Revisit the preceding affected files and add `schemas/v4/open-forge.schema.json`.
- Update `src/open-forge/{AGENTS.md,CLAUDE.md}`, their repository counterparts where applicable, and `maintenance/payload/{AGENTS.md,CLAUDE.md}`.
- Update `framework/architecture.md` and CLI technical designs `workspace-permissions.md`, `workspace-libraries.md` and `generated-navigation.md`.
- Update contract pairs for `install`, `update`, `remove`, `repair`, `cleanup`, `route/{init,create,update,move,remove}`, `shared/workspace-permissions`, every affected Extension lifecycle leaf and `library/{attach,sync,detach}`.
- Update site `concepts/core-categories.md`, `getting-started/first-task.md`, native Skill placement examples and the Library flow in `cli/flows.md`.

The contract filenames `index-candidate` and `references-candidate` are intentional current paths. Neither PR should rename them incidentally.

Also correct the shipped Skill’s stale sentence claiming Essentials is the first interactive choice. Current Install documentation and the council’s recorded maintainer decision recommend Minimal.

### Risks

The largest #6 risk is two inconsistent reading systems. Prove early that one included catalogue produces matching inventories, conditions and missing-file findings across Context, Find, Status and Doctor.

The second is restoring disabled knowledge. Remove a registered loader row, then exercise Index, Update, Configure and Extension navigation. It must remain absent.

The third is accidental external writes after catalogue expansion. Compare included targets before and after every maintenance journey.

For #7, partial root indirection can produce correct-looking reads while writes still target `.agents`. The earliest decisive journey combines custom-root install, Context, Create, Index, Update and removal, then inspects every destination.

Other early checks should cover relative Library links, native Skill discovery, conflicting settings anchors and interrupted relocation. Use the existing Unit, Integration and EndToEnd projects, followed by the required Native AOT qualification.

This proposal is based on read-only source inspection. No files were modified, and no Doctor, builds or test suites were run.

### The one idea to steal

Make registration expand what the CLI can understand, while the loader continues to decide what agents actually use.

## The Contract Guardian (Proposal F)

### Thesis

Add `route include` and `route exclude` for project knowledge that stays where it is. Config records the selected paths, while loader `Entries` remain the authority for loading. Ship configurable content roots separately, with explicit migration and unchanged defaults.

### The concept and its name

The new word is **inclusion**. Extensions distribute reusable files, Libraries project local files through filesystem links, and inclusions expose existing project knowledge directly.

There is one important qualification. Today, a Library’s source and destination must both remain inside the selected workspace. “Same computer” describes the desired reuse layer, but the current contract does not admit arbitrary machine paths. [Library Attach contract](../../../../../.agents/memory/crystallized/documents/cli/contracts/library/attach/interface.md)

Use two new leaves in the existing `route` group:

```text
open-forge route include <workspace-file> --description <text> [options]
open-forge route exclude <workspace-file> [--dry-run] [global flags]
```

“Inclusion” describes the relationship without claiming ownership. Reject **link**, which already describes Markdown references and Library filesystem links. Reject **route add**, which conceals whether the operation creates content, creates routing structure, or exposes existing content. Reject a new **root** group because a file inclusion is not necessarily a directory root.

These leaves avoid changing three existing operations:

- `route create` creates an ordinary Markdown file below an existing routed root. An external path, entrypoint, or differing existing file is ineligible.
- `route init` creates missing entrypoints. Its metadata flags are invalid when the final entrypoint already exists.
- `route update` patches metadata inside an existing routed Markdown source. Its contract says, “Omitted metadata fields remain unchanged.”

Adding registration modes to these commands would change their subjects, effects, validation and results. The existing `route` group is the correct shared subject, but each operation needs its own contract.

### The CLI surface

The complete new grammar is:

```text
open-forge route include <workspace-file>
  --description <text>
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-file>
  [--dry-run]
  [global flags]
```

Both accept the existing global options: `--workspace`, `--format`, `--detail`, `--detail-filter`, `--help` and `--version`.

The operand is one literal workspace-relative file path, optionally prefixed with `./`. It is a separate path operand, not a source ID. Singleton repetition is invalid. Tags preserve argument order and reject duplicates. Conditions use existing glob parsing, normalization and deduplication.

Three examples:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow --dry-run
open-forge route include .cursor/rules/testing.mdc --description "Testing conventions" --tag LoadNow --apply-to "src/**" --dry-run
open-forge route include docs/_docs.md --description "Project documentation" --dry-run
```

After reviewing a preview, remove `--dry-run` to apply it. These leaves apply directly like Route Create and Index. They have no confirmation mode, `--automatic`, `--force` or destination option.

An ordinary file is an opaque text leaf. A canonical `_{folder}.md` is a catalogue whose `Entries` are followed. Existing recognized compatibility entrypoints remain accepted when unambiguous. An arbitrary underscore filename is not automatically a catalogue. Ordinary documents do not become catalogues merely because their bodies contain an `Entries` heading.

Include writes one loader row and adds its canonical path to config. It never changes the target. Repeating the identical request verifies a no-op. Repeating it with different description, tags or conditions explicitly replaces that loader row’s complete policy. Omitted tags and conditions mean empty lists.

Exclude removes the registration and matching direct loader row, preserving the source and ordinary references to it. It also works when the target has disappeared. A conflicting or ambiguous set of matching loader rows blocks rather than choosing one.

`route list --depth 0` lists active roots, including inclusions. `status --detail standard` additionally lists registered paths whose loader row is absent. No registration-list mode is hidden inside Route List.

Validation rejects directories, globs, absolute paths, workspace escapes, binary or invalid UTF-8 input, Git metadata and manager controls. Include rejects a missing target as `invalid-input`. Unreadable required evidence is `incomplete`. Unsafe paths, ambiguous catalogues, competing catalogue parents and cycles are `blocked`. Errors identify the path, cause and corrective command. An already routed target is reported without creating a second ancestry.

Command behavior follows these boundaries:

- **`context`** follows active loader rows with existing loading tags and inherited conditions. Foreign frontmatter never supplies activation policy.
- **`route list`** includes active external leaves and catalogue ancestry. Their metadata comes from authored entry rows, with that origin visible.
- **`find`** includes these sources in its bounded inventory, using entry metadata and Markdown headings where applicable.
- **`references`** recognizes them as targets and scans supported Markdown sources within the declared universe. It does not invent parsers for foreign rule formats.
- **`index`** preserves inclusion rows and validates their relationships. Extra-root content remains read-only, including its metadata and catalogue membership.
- **`status`** counts the actual startup closure across roots, deduplicating physical files. Missing measurements remain unavailable rather than zero.
- **`doctor`** diagnoses missing targets, malformed catalogues, unsafe ancestry and registration discrepancies. Deliberately inactive registrations are informational.

Hand-written loader rows remain valid without config registration. Include can register an identical existing row without duplicating it. Differing existing policy requires an explicit replacement shown in the preview.

Both new leaves use the existing schema-3 envelope, with machine identities `route include` and `route exclude`. Their command-local `data` is:

```text
{
  mode: "apply" | "dry-run",
  target: { path, kind: "file" | "entrypoint" } | null,
  registered: boolean | null,
  listedIn: string | null,
  changed: boolean | null
}
```

Preview reports intended registration state. Effects report planned or verified writes separately. Unavailable facts use `null` with a limitation. Full detail adds `entryBefore` and `entryAfter`. Bound semantic failures retain this object.

Preserve the shared envelope’s exact field order and vocabulary. Exits remain `completed=0`, `completed-with-warnings=2`, `incomplete=3`, `invalid-input=4`, `blocked=5`, `failed=1` and `cancelled=130`. JSON semantic results always use stdout. Parser failures and terminal help retain their existing treatment. [Shared result contract](../../../../../.agents/memory/crystallized/documents/cli/contracts/shared/result-coordinates/interface.md)

Settings and loader changes form one recoverable, revalidated plan. Failure after either write reports actual partial effects. No source ownership receipt is created.

### The config

For #6, newly authored settings use:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v3/open-forge.schema.json",
  "schemaVersion": 3,
  "roots": [
    "CONTRIBUTING.md",
    ".cursor/rules/testing.mdc",
    "docs/_docs.md"
  ]
}
```

For #7:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v4/open-forge.schema.json",
  "schemaVersion": 4,
  "mainRoot": ".context",
  "roots": [
    "CONTRIBUTING.md",
    ".cursor/rules/testing.mdc",
    "docs/_docs.md"
  ]
}
```

`roots` contains entry file paths, not directory scan requests. Missing means `[]`. `mainRoot` defaults to `.agents` and names the writable Framework content directory.

Each schema keeps `additionalProperties: true`. Root entries must be nonempty canonical workspace-relative paths. Runtime validation additionally rejects duplicate physical identities, unsafe controls and invalid catalogue topology. `mainRoot` cannot be `.`, Git metadata, an escaping path or a linked directory.

Settings versions and result-envelope versions are independent. Add v3 and v4 schemas without rewriting older schema files. New files declare the current settings version. Existing settings retain their authored declarations unless explicitly migrated, matching the current writer’s preservation policy.

Readers continue consuming understood keys from unknown versions. The current schema explicitly promises that an unrecognized version is “reported, never refused.” A settings-version increase cannot serve as a downgrade safety barrier. [Current settings schema](../../../../../schemas/v2/open-forge.schema.json)

Config records membership only. Descriptions, tags, conditions and ordering remain in loader rows. Include renders a selected path relative to the loader, for example:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

**The loader wins for effective loading.** A config registration without a loader row is inactive. Index, Update, Configure and Extension operations never recreate that row. A loader row without registration works normally. Changing only config adds no reading requirement.

Keeping policy out of config avoids two competing descriptions of when agents should read a document.

### `help concepts` and the README guide

Implement `help` as a direct command with a topic operand:

```text
open-forge help [<topic>] [global flags]
```

Bare `help` lists available topics. Initially, `concepts` is the sole topic. This avoids a ceremonial group with one child.

Unlike terminal `--help`, topic retrieval is a read-only operation supporting JSON. Its machine command is `help`, its workspace is `null`, and its data is:

```text
{
  topic: string | null,
  title: string,
  sections: [{ id, title, body }]
}
```

It requires no workspace. Unknown topics return `invalid-input`, exit 4. `help --help` retains terminal text behavior. Use one authored `docs/concepts.md` source embedded into the executable and published by the docs site.

The complete concepts outline is:

- Purpose — use existing knowledge and add structure when useful.
- Files and ownership — plain files carry meaning, while settings and receipts support maintenance.
- Routes and catalogues — entrypoints expose destinations and their selection descriptions.
- Loading — distinguish required reads, on-demand selection and rereads.
- Conditions and scope — explain inherited conditions and the same-working-file rule.
- Authority — reading, linking and tagging do not establish acceptance or edit permission.
- Optional categories — explain each role and the Memory states.
- Reuse — distinguish Extensions, Libraries and inclusions.
- Roots — distinguish the main content root from additional entry files.
- Authoring — point to the local format specification.
- Customization — explain overwrites and update behavior.
- Removal — distinguish disabling, releasing management and deleting.
- Native tools — explain harness discovery and Skill ownership.
- CLI assistance — explain discovery, previews, results and manual fallback.

Remove the glossary, general Framework explanation and binding authoring section from `.agents/README.md`. Its replacement outline is:

- **You may remove this guide** — it is optional, and managed removal must be recorded to survive updates.
- Use one document you already have.
- Write your own rule.
- Keep one accepted fact.
- Save unfinished work when useful.
- Create your own categories and reusable methods.
- Turn a part off.
- Customize a shipped file.
- Find further help.

Move authoring rules into shipped `.agents/authoring.md`, referenced conditionally by the loader before authoring. It is a format reference, not a Directive and not startup context. Exclude it from routed inventory just as the human guide is excluded today.

These rules explain catalogue naming, relative links, descriptions and tags, frontmatter forms, condition syntax, inherited Axioms, narrow placement and overwrite companions. They matter because tools and agents must interpret the same files consistently. External inclusions need only their entry-row metadata. Native Skills retain native metadata. The complete local reference must work without the CLI and remain independent of the removable guide.

### The configurable main root (#7)

Keep `.agents/open-forge.json` and `.agents/open-forge.lock.json` at fixed bootstrap locations. Move the content root, not the control-file discovery mechanism. This leaves a small `.agents` control directory, an explicit tradeoff that avoids upward searching or competing configs.

Fresh Install gains `--main-root <workspace-directory>`. The wizard asks, “Where should Open Forge keep its context files?” with `.agents` preselected. Explicit input skips the question. Unattended omission preserves today’s default. Existing installations do not move because someone changes a preference during ordinary configuration.

Native Skills stay at harness-recognized locations such as `.agents/skills`. A custom root does not create a replacement Skills category inside itself. When Skills are selected, the loader can expose their native catalogue as an inclusion. `AGENTS.md` and `CLAUDE.md` remain at their native locations and point to the configured loader. Other harness rules remain under their harness’s discovery rules.

Migration deserves one explicit operation:

```text
open-forge relocate <workspace-directory> [--dry-run] [global flags]
```

It moves the main content tree while retaining bootstrap controls and native Skills, rebases supported references and loader rows, updates managed host sections, remaps ownership destinations and path settings, then publishes `mainRoot`. Library sources remain untouched. Collisions, nested repositories, unsupported references or incomplete inventories block before writes.

Relocate uses schema 3 and the same status mapping. Its data is `{mode, from, to, moves:[{from,to}], changed}` with nullable unresolved coordinates. Applying the same completed relocation verifies a no-op. Directly editing `mainRoot` is documented for manually maintained workspaces, not as an automated migration.

Ordered implementation slices, each with one bounded owner:

1. Define root identity, bootstrap locations, package mapping, migration effects and contracts.
2. Add root-aware workspace services while retaining `.agents` defaults and unchanged ordinary output.
3. Convert source identity, loading, discovery, references and navigation consumers.
4. Convert lifecycle destinations, exclusions, permissions, privacy rules and ownership validation.
5. Add fresh Install selection and native Skill handling.
6. Implement Relocate with complete planning, recovery, interruption and verification.
7. Complete docs, focused cross-platform journeys and release qualification.

Keep canonical package asset coordinates stable and map `.agents/**` payload destinations through one declared projection. Do not replace every string mechanically.

Estimate #6 at 7–10 engineering days and #7 at 15–25, including contracts, docs and verification. These are planning estimates. Root-sensitive ownership and migration evidence are the main uncertainty.

### Big changes to CLI usage

For #6, users gain Include, Exclude and topic help. Existing source operands retain their interpretation. Add `./<workspace-path>` as an explicit exact-path form for included sources in reading commands. Bare `docs/file.md` remains an ID, avoiding filesystem-dependent parsing.

Active inclusions become visible across discovery commands. Their entry metadata replaces any requirement for Open Forge frontmatter. Status estimates increase when startup includes outside files. Index does not acquire write access to those files.

For #7, fresh Install gains a root option and question, and existing workspaces gain Relocate. Source IDs below the main root stay stable while reported physical paths change. Native Skill locations remain stable. Permission boundaries follow the configured main root, while extra roots remain read-only.

Older CLIs ignore unknown keys and cannot safely manage custom-root workspaces. Document the minimum CLI release for mutation and keep manual Markdown use available. Default-root users require no migration.

### Documents to update

Let `C` mean `.agents/memory/crystallized/documents/cli`. Every contract leaf named below means its exact `interface.md` and `behavior.md` files.

For #6:

- Add `C/contracts/route/include/{_include.md,interface.md,behavior.md}`, `C/contracts/route/exclude/{_exclude.md,interface.md,behavior.md}` and `C/contracts/help/{_help.md,interface.md,behavior.md}`.
- Update `C/command-contract-set.md`, `C/contracts/_contracts.md`, `C/contracts/route/_route.md` and `C/shared-operation-contract.md`.
- Update leaves `shared/source-references`, `context`, `find`, `references-candidate`, `index-candidate`, `status`, `doctor` and `route/{list,inspect}`.
- Update loader-preservation boundaries in `install`, `update`, `extension/{install,update,remove}` and the relevant removal contracts.
- Add `C/technical-designs/source-roots.md`. Keep Include and Exclude mechanics there only where genuinely shared.
- Update `.agents/memory/crystallized/documents/framework/routing/{model,paths,loading}.md` and `framework/markdown/{syntax,routes,compatibility}.md`.
- Add `schemas/v3/open-forge.schema.json` and `docs/concepts.md`.
- Update `README.md`, `docs/cli.md`, `docs/extensions.md`, `src/open-forge/.agents/{loader.md,README.md}` and add `src/open-forge/.agents/authoring.md`.
- Update `src/open-forge/.agents/skills/open-forge-cli/SKILL.md` and all four references: `references/{common,discovery,routes,packages}.md`. Synchronize the repository’s corresponding `.agents/skills/open-forge-cli/` files where applicable.
- Update `src/docusaurus/docs/{intro,glossary}.md`, `getting-started/{installation,ten-minute-guide,greenfield-and-brownfield,grow-your-framework}.md`, `concepts/{index,routing,loading-and-tags,customizing}.md`, `cli/{index,flows}.md` and `extensions/{index,reading-installed-files}.md`.

Library guidance currently lives in `docs/cli.md` and the site’s `cli/flows.md`. There is no separate Library page to update.

For #7, revisit that same root-sensitive set and:

- Add `schemas/v4/open-forge.schema.json` and `C/contracts/relocate/{_relocate.md,interface.md,behavior.md}`.
- Update all Route, Extension and Library leaf contracts, plus `install`, `update`, `remove`, `repair` and `cleanup`.
- Update `C/architecture.md`, `C/layers/{framework,operations,shell,presentation}.md` and technical designs `generated-navigation.md`, `workspace-permissions.md`, `workspace-libraries.md`, `lifecycle-provenance.md` and `embedded-payload.md`.
- Update `src/open-forge/{AGENTS.md,CLAUDE.md}`, native Skill examples and root-selection/migration instructions throughout the listed guides.

Register every new contract in the contract set. Existing contracts remain authoritative for their own operation. [Contract-set rules](../../../../../.agents/memory/crystallized/documents/cli/command-contract-set.md)

### Risks

The likeliest failure is silent reactivation. The decisive early scenario is Include, remove its loader row manually, then run Index, Update, Configure and Extension operations. It must remain off.

The next risk is broadening reads into writes. Compare external target bytes before and after every maintenance command, including catalogue metadata and Entries.

Graph behavior needs early attention. `ContextLinkedEntryReader` currently assigns external sources no ID and “keeps the first catalogue that linked it as its parent.” Registration must not preserve that accidental ancestry policy. Test competing parents, overlapping roots and inherited conditions before propagating the new graph.

Contract discrepancies already exist. Route List documents different catalogue and merged outcomes for unreadable metadata and malformed loaders. Resolve those explicitly before adding inclusion cases. Doctor’s error findings normally produce exit 2, while incomplete diagnostic coverage produces exit 3. Do not introduce a new cross-command interpretation accidentally.

Root migration can leave valid-looking stale receipts, privacy paths or harness pointers. Verify one mixed workspace containing authored notes, edited managed files, an Extension, a Library and private Working Memory before scaling the conversion.

README deletion can remove required instructions unless the authoring reference is independent. Verify a no-CLI authoring task after deleting the guide.

This proposal is based on read-only inspection. No files were changed, and no Doctor, builds or test suites were run.

### The one idea to steal

Store where knowledge lives in config, but keep the decision to read it visible in the loader.

## The Brownfield Team Lead (Proposal G)

### Thesis

Adoption should expose existing knowledge through a small, reversible edit to the loader and settings. Ship `route include` in #6 while preserving the loader’s `Entries` as the activation switch. Treat moving the main context root as a separate migration feature in #7.

### The concept and its name

Extensions distribute reusable files between projects. Libraries reuse a registered source through relative file symlinks. Includes expose knowledge already present in this workspace, without copying it or creating filesystem links.

That stratification describes intent, but the current Library implementation has a narrower boundary than “anywhere on this computer”. Its source must be “strictly contained by the selected workspace lexically and physically”. #6 should preserve that boundary. [Library contract](../../../../../.agents/memory/crystallized/documents/cli/contracts/library/attach/interface.md)

The one word to learn is **include**:

```text
open-forge route include <path>...
open-forge route exclude <path>...
```

I would guess `route include` when looking for a way to add existing documents to agent context. It belongs under `route` because it establishes a path through the loader.

“Link” suggests a filesystem operation, especially beside Library commands that actually create symlinks. “Route add” leaves me guessing whether it creates content, initializes catalogues, or exposes existing files. A top-level `root` group makes a single contribution guide sound like a directory configuration problem.

The operations remain distinct. `route create` writes a new ordinary Markdown file. `route init` creates missing entrypoints. `route include` requires existing content and writes only its registration and loader entry.

### The CLI surface

Proposed syntax:

```text
open-forge route include <workspace-file>...
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--clear-apply-to]
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-file>...
  [--dry-run]
  [global flags]

open-forge route list [<source-reference>]
  [--depth <non-negative-integer|all>]
  [global flags]
```

Global flags retain their existing meanings: `--workspace`, `--format`, `--detail`, `--detail-filter`, `--help`, and `--version`.

Paths resolve from the selected workspace. Repeated identical paths collapse in first-occurrence order. `--description` requires one operand. Tags and conditions apply to every operand, which makes a batch of related rules useful. Tags retain their supplied order. Conditions use the existing glob grammar. `--clear-apply-to` conflicts with `--apply-to`.

A new entry defaults to its workspace-relative path as its description, no tags, and no conditions. It therefore loads on demand. Explicit metadata updates the corresponding loader fields on an existing registered entry. Omitted fields stay unchanged.

Here is the session I would run. These are proposed commands, not commands executed against this snapshot.

First, establish Minimal and inspect its managed changes:

```sh
open-forge install --preset minimal --dry-run
open-forge install --preset minimal --automatic
git status --short
git diff
```

I would also open the new untracked files, which `git diff` does not show. Install may add managed sections to existing `AGENTS.md` and `CLAUDE.md`, so their surrounding content must remain intact.

Example one exposes orientation and one existing ADR:

```sh
open-forge route include README.md CLAUDE.md docs/adr/0001-storage.md --dry-run
open-forge route include README.md CLAUDE.md docs/adr/0001-storage.md
```

Example two makes the project’s three rule sources required:

```sh
open-forge route include CONTRIBUTING.md .cursor/rules/testing.mdc .github/copilot-instructions.md --tag LoadNow --dry-run
open-forge route include CONTRIBUTING.md .cursor/rules/testing.mdc .github/copilot-instructions.md --tag LoadNow
```

Those are two inclusion requests after Minimal. Neither requires rewriting the existing files.

Example three is an alternative when an existing ADR catalogue already lists the decisions:

```sh
open-forge route include docs/adr/_adr.md --description "Accepted architecture decisions" --tag Architecture --apply-to "src/**" --dry-run
open-forge route include docs/adr/_adr.md --description "Accepted architecture decisions" --tag Architecture --apply-to "src/**"
```

Before using that alternative, exclude any separately registered ADR that the catalogue would also parent.

An ordinary file is a leaf. A Markdown file named `_{folder}.md` is a catalogue whose explicit `Entries` are followed. Preserve recognized compatibility catalogue names when unambiguous. A directory operand never expands recursively, and a file beginning with an unrelated underscore remains a leaf.

Other assistants’ files remain opaque leaves. Their frontmatter and native activation syntax do not become Open Forge loading policy. `CLAUDE.md` continues to serve its native harness independently.

Every preview must display the exact settings change, exact loader lines, and retained source paths. Include and Exclude apply directly, like existing route creation commands. They need no confirmation flag.

For inspection and undo:

```sh
open-forge route list --depth 0 --detail full
open-forge context --for src/Orders.cs
open-forge route exclude CONTRIBUTING.md --dry-run
open-forge route exclude CONTRIBUTING.md
```

Exclude removes that registration and its loader row. It never deletes the contribution guide or its incoming ordinary references. Repeating it is a verified no-op. `route remove` must refuse included sources and their descendants, directing users to Exclude.

Validation rejects missing files, unsupported text, URLs, globs, directories, reserved control files, physical workspace escapes, cycles, duplicate physical identities, and competing catalogue parents. One invalid target prevents the whole batch from applying. Errors name the operand, cause, and correction. Existing result statuses and exit codes apply.

Command behavior should be explicit:

- **`index`** preserves inclusion rows and never writes imported files, their frontmatter, or their catalogues. It never restores a missing loader row from settings.
- **`status`** reports registered paths, enabled paths, and registrations switched off in the loader. Extend the startup estimate to count included required text, deduplicated by physical identity. Report unavailable measurements honestly.
- **`doctor`** checks targets, catalogue structure, containment, ancestry, and references. A registration intentionally switched off is informational.
- **`context`** follows the same loading tags and inherited conditions as handwritten entries. One working file must satisfy the complete condition chain.
- **`route list`** includes enabled external leaves and catalogues. Full detail identifies registered versus handwritten entries and separately lists registrations currently off.
- **`find`** searches entry tags and Markdown headings without requiring foreign frontmatter. Search results still require their ancestor context before use.
- **`references`** reports supported Markdown relationships involving included sources. Non-Markdown text does not acquire invented Markdown structure.

Readers accept exact `./<workspace-path>` references for included sources. Existing source IDs retain their meaning.

Handwritten loader entries remain first-class. If an unregistered handwritten row already targets the requested file, Include reports it unchanged and does not silently adopt it. Exclude operates only on registered entries.

### The config

Keep metadata in the loader, where agents without the CLI already read it. Settings track registration only.

For #6, add this shape to `.agents/open-forge.json`, preserving existing settings:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v3/open-forge.schema.json",
  "schemaVersion": 3,
  "roots": [
    "CONTRIBUTING.md",
    "docs/adr/_adr.md"
  ]
}
```

`roots` means additional graph entry files, including individual leaves. Values are unique, canonical workspace-relative paths using forward slashes.

An explicit Include renders a loader row using that path and the requested metadata:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

The config does not contain a second description, tag list, or condition.

For #7, the same settings file gains:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v4/open-forge.schema.json",
  "schemaVersion": 4,
  "mainRoot": "docs/agent-context",
  "roots": [
    "CONTRIBUTING.md",
    "docs/adr/_adr.md"
  ]
}
```

`mainRoot` defaults to `.agents`. It selects the framework context directory. Settings and ownership controls retain their fixed `.agents/` location, which provides deterministic discovery without searching for configuration.

From the custom loader, the contribution row becomes:

```markdown
- [Contribution rules](../../CONTRIBUTING.md) - #LoadNow
```

The loader wins disagreements about activation and loading metadata. A config-only path is registered but off. A loader-only path is handwritten and active according to its row. Removing a registration alone leaves its loader row as handwritten content.

This preserves the existing promise: “A removed line stays removed until the user asks to restore it.” [Loader](../../../../../src/open-forge/.agents/loader.md)

Publish new v3 and v4 schemas rather than changing published v2. Missing properties retain defaults. Readers preserve unknown keys, tolerate unknown versions, and validate understood fields with file-and-key diagnostics. Older CLIs ignore these additions, so custom-root workspaces must require a supporting CLI version.

### `help concepts` and the README guide

Add two read-only help leaves:

```text
open-forge help concepts [global flags]
open-forge help authoring [global flags]
```

Bare `help` lists topics. Existing `--help` continues to explain command syntax. Topics come from bundled sources, need no installed workspace, and support text and structured output.

The full concepts outline is:

- **Start with existing knowledge** explains Minimal and inclusion before optional categories.
- **Files, catalogues, and roots** explains leaves, entrypoints, root routes, and explicit membership.
- **Choose useful roles** introduces Directives, Guidance, Maps, Patterns, Templates, Skills, and Memory.
- **Keep different knowledge states distinct** explains accepted knowledge, candidates, active work, and history.
- **Select relevant context** explains descriptions, working files, scopes, and conditions.
- **Understand required reads** explains LoadNow, KeepInMind, ancestor chains, and pending conditions.
- **Understand authority** separates reading, acceptance, requirements, and edit permission.
- **Reuse knowledge** compares includes, Libraries, and Extensions with their actual boundaries.
- **Work with other assistants** explains opaque files and independent native discovery.
- **Change your setup** distinguishes disabling, unregistering, stopping management, and deleting.
- **Maintain files safely** explains previews, Index, diagnosis, overwrites, and update behavior.
- **Work without the CLI** points to the complete plain-file procedure and authoring reference.

Move the taxonomy, vocabulary, loading tutorial, and CLI inventory out of `.agents/README.md`. Move its binding authoring instructions into a new, always-installed `.agents/authoring.md`, referenced by the loader before authoring. `help authoring` renders that same bundled source. This introduces no authoring Directive and no second copy of the rules.

The authoring rules are concrete. Use relative links and useful tags. Keep detail in its narrowest source. Name catalogues consistently. Write descriptions and tags on newly authored framework sources. Respect the chosen metadata form. Keep entry descriptions, paths, tags, and conditions aligned. Preserve native Skill metadata. Use overwrite companions for durable changes to shipped content.

These rules matter because malformed metadata or stale navigation changes what agents can discover and when they read it. They do not require adding frontmatter to included foreign documents.

The new README starts with the literal header `# Open Forge guide, you may remove this file`. Its outline is:

- Connect documents you already maintain.
- Create your first project rule.
- Save one useful accepted fact.
- Add only the categories or packages that help.
- Reuse your knowledge elsewhere.
- Inspect changes and undo an inclusion.
- Remove this guide and find further help.

Explain removal through the existing root Remove command so the omission survives updates. Deleting this guide must leave every loader-required reference valid.

### The configurable main root (#7)

I would put new Open Forge knowledge in `docs/agent-context`. That keeps it beside our existing documentation while leaving ADRs, CONTRIBUTING, and assistant rules at their established paths.

The install question becomes “Where should Open Forge keep its context files?” Offer `.agents` as the default and a custom workspace-relative directory. Scripts use:

```text
open-forge install --preset minimal --root docs/agent-context
open-forge install --configure --root docs/agent-context --dry-run
```

The second form plans migration from the currently configured root. Editing `mainRoot` manually does not move files. A missing loader produces a clear diagnosis rather than a guessed fallback.

This design deliberately retains `.agents/open-forge.json` and `.agents/open-forge.lock.json`. It relocates the context root, not every file bearing the Open Forge name.

Skills remain in `.agents/skills` or their established harness-native locations. Do not create `docs/agent-context/skills` and imply native discovery will follow it. When selected, an ordinary catalogue reference exposes native Skills to Open Forge. Reading that catalogue does not activate capabilities.

Update managed handoffs in root `AGENTS.md` and `CLAUDE.md` to the chosen loader path while preserving surrounding bytes. Existing harness configuration remains independent.

Existing workspaces stay at `.agents` until explicitly migrated. The migration preview inventories context categories, user records, private records, overwrite companions, links, omissions, ownership receipts, and ignore rules. It preserves disabled branches as disabled. It leaves native Skills, controls, and unrelated occupants in place. Destination collisions block before writes.

Implement this in ordered slices that can each have a separate worker:

1. **Contract and layout model, 2 days.** Define context, control, native Skill, logical identity, and physical path boundaries.
2. **Settings and discovery, 2 days.** Add v4 parsing and one resolved layout per invocation.
3. **Read-only routing, 3 days.** Convert source resolution, catalogue traversal, Context, Find, References, and Route readers.
4. **Navigation and diagnosis, 3 days.** Convert Index, Status, and Doctor while preserving borrowed-source protection.
5. **Framework lifecycle, 3 days.** Make fresh Install and Update use the resolved layout.
6. **Packages and permissions, 4 days.** Map package context destinations to the main root, retain native Skill destinations, and keep literal external destinations permission-gated.
7. **Migration, 4 days.** Add bounded moves, link rebasing, receipt and omission updates, interruption reporting, and reversal evidence.
8. **Delivery and documentation, 3 days.** Verify default compatibility, custom-root journeys, managed and Native AOT execution, and published examples.

The implementation should separate stable logical identities from resolved physical paths. Replacing every `.agents` string would conflate context, control, and native discovery.

Allow roughly four to six engineering weeks for #7, including correction time. #6 is approximately two engineering weeks. These are planning estimates, not measured implementation results.

### Big changes to CLI usage

For #6, users gain Include, Exclude, the two help topics, registered-entry reporting, and exact external source-path operands. Startup estimates may increase because they finally count required external documents.

Include grants no source-file ownership. Index, Repair, route mutation, and package lifecycle operations must preserve included source content. Disabling a loader line still works by hand.

The README becomes removable without breaking authoring instructions. It is replaced by practical examples and discoverable help.

For #7, users gain `install --root` and an explicit migration path. Default installations retain their paths and existing source IDs. Custom installations show their actual paths in reports, previews, and suggested commands.

Skills retain native locations. Extension context destinations can relocate, while harness-specific destinations retain their declared locations and permission requirements. Older CLI versions must not maintain a custom-root workspace.

No command is renamed. Library boundaries, package IDs, presets, loading tags, and the distinction between reading and acceptance remain intact.

### Documents to update

Use `.agents/memory/crystallized/documents/cli/contracts/` as the prefix for contract paths below. Brace notation names an exact file set.

For #6:

- Add `route/include/{_include,interface,behavior}.md`, `route/exclude/{_exclude,interface,behavior}.md`, `help/_help.md`, `help/concepts/{_concepts,interface,behavior}.md`, and `help/authoring/{_authoring,interface,behavior}.md`.
- Update `_contracts.md`, `route/_route.md`, and the parent `command-contract-set.md`.
- Update `interface.md` and `behavior.md` under `route/list`, `route/inspect`, `route/update`, `route/move`, `route/remove`, `context`, `find`, `references-candidate`, `index-candidate`, `status`, `doctor`, `repair`, and `shared/source-references`.
- Update `docs/cli.md`, `docs/extensions.md`, and repository `README.md`.
- Update `src/open-forge/.agents/loader.md` and `README.md`, and add `authoring.md`.
- Update the shipped `skills/open-forge-cli/SKILL.md` and `references/{common,discovery,routes,packages}.md`. Apply the corresponding changes to `.agents/skills/open-forge-cli/`.
- Add `schemas/v3/open-forge.schema.json`.
- Update `src/docusaurus/docs/getting-started/{installation,greenfield-and-brownfield,ten-minute-guide,grow-your-framework}.md`, `concepts/{index,routing,loading-and-tags,customizing,core-categories}.md`, `cli/{index,flows}.md`, `extensions/index.md`, and `glossary.md`.
- Update maintenance `payload/agents/loader.md`, add `readme.md` and `authoring.md`, and register them in `_agents.md`.

For #7, revise those files for physical paths and migration, and additionally:

- Add `schemas/v4/open-forge.schema.json`.
- Update contract `interface.md` and `behavior.md` under `install`, `update`, `remove`, `cleanup`, `route/create`, `route/init`, every existing Extension leaf, every Library leaf, and `shared/workspace-permissions`.
- Update `.agents/memory/crystallized/documents/cli/architecture.md` and `technical-designs/{generated-navigation,workspace-libraries,lifecycle-provenance,mutation-and-recovery}.md`.
- Update framework `architecture.md`, `routing/{model,paths}.md`, `markdown/routes.md`, and `primitives/skills.md`.
- Update shipped `src/open-forge/{AGENTS,CLAUDE}.md` and their maintenance counterparts.
- Update `docs/development.md`, `src/docusaurus/docs/concepts/{routing,customizing,core-categories}.md`, and the CLI Skill’s package and source-path examples.

Fix the existing stale Essentials-default sentences in `docs/cli.md` and the shipped `references/packages.md`. Both currently also describe the newer Minimal recommendation elsewhere.

### Risks

The likeliest regression is restoring an entry somebody disabled. Catch it first with a fixture containing an empty loader and config-only registrations, then run every navigation and lifecycle planner against it.

The next risk is modifying borrowed documents. Hash the existing README, contribution guide, ADRs, and assistant files through Include, Index, Update, Exclude, and failed removal attempts.

Competing catalogue parents can change inherited rules. Test a catalogue plus a separately included child before expanding the implementation.

Startup reporting can undercount external text or double-count aliases. Compare its required-file set with Context’s closure, including pending conditions and missing targets.

Custom roots can break package links, permissions, native Skills, and omission paths. Establish one connected custom-root installation and migration journey before converting the remaining commands.

Finally, validate the name with unfamiliar users. Ask them to expose CONTRIBUTING, identify every written file, and undo the inclusion without coaching. Failure indicates an interface problem.

This proposal is based on read-only inspection. No files were modified, and no Doctor, build, or test suite was run.

### The one idea to steal

Make the first adoption promise testable: two inclusion commands expose existing knowledge, every written line is visible, and exclusion leaves the source documents untouched.

## The Unix Minimalist (Proposal B)

### 1. Thesis

Existing project knowledge should become another **route**, using `route include` and `route exclude`. Configuration records which sources the CLI knows about, while the loader’s `Entries` remain the loading authority. Keep #6 about reuse in place and reserve root relocation for #7.

### 2. The concept and its name

The three layers answer different reuse needs. Extensions distribute complete packages. Libraries project locally shared files through relative file links. An included route uses a document where it already lives.

The distinction concerns delivery, not authority. The current Library implementation also requires contained workspace paths, so “share on this computer” must not imply arbitrary filesystem access. The [Library contract](../../../../../.agents/memory/crystallized/documents/cli/contracts/library/_library.md) describes a “contained source root” and individual relative file links.

The word users should learn is **route**, which they already encounter. The shipped README defines it as “a path through catalogues and Entries.” Including an existing document creates exactly that relationship.

Use two leaves in the existing group:

```text
open-forge route include <workspace-file> [options]
open-forge route exclude <workspace-file> [options]
```

Reject `link` because it suggests filesystem links, which Libraries already create. Reject `route add` because it leaves creation and inclusion ambiguous. Reject a new `root` group because a selected file need not represent a directory or require a separate lifecycle.

`route create` creates an ordinary Markdown file. `route init` creates missing entrypoints or restores Framework categories. Their [documented boundaries](../../../../../docs/cli.md) should remain intact. Include requires an existing file and changes only its registration and loader entry.

Exclude earns a second leaf because `route remove` deletes content. Reusing that destructive command to mean “stop reading this document” would save syntax at the cost of predictability.

### 3. The CLI surface

Exact proposed syntax:

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-file>
  [--dry-run]
  [global flags]

open-forge route list [<source-reference>]
  [--depth <nonnegative-integer|all>]
  [global flags]
```

Description is required when creating a new loader row. Tags default to empty, making the route on demand. Conditions use the existing glob grammar. Description is a singleton, tags retain supplied order, and conditions use existing normalization.

Three inclusion examples:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow --dry-run
```

```sh
open-forge route include .cursor/rules/testing.mdc --description "Testing conventions" --tag LoadNow --apply-to "src/**" --dry-run
```

```sh
open-forge route include docs/team/_team.md --description "Team knowledge" --dry-run
```

Apply each reviewed request without `--dry-run`. Like Route Create and Index, these small editing operations apply directly and have no wizard or `--automatic`.

An ordinary text file is a leaf. Its exposing row supplies description, tags and conditions. Foreign frontmatter does not control activation. A recognized Markdown entrypoint, including `_{folder}.md` and existing compatibility names, exposes its `Entries` recursively. An arbitrary underscore filename remains an ordinary file unless it matches entrypoint recognition.

Include accepts one existing workspace-relative file. It rejects directories, globs, URLs, fragments, binary content, invalid encoding, reserved controls and lexical or physical workspace escapes. A directory error suggests its exact entrypoint path without selecting one automatically. Cycles terminate through physical identity tracking, while conflicting entrypoint identities block inclusion.

An existing matching loader row is preserved and registered. Supplied metadata must agree with it. Conflicting metadata requires an explicit row edit, avoiding an accidental change to loading policy during registration. Repeating an unchanged request is a verified no-op.

Removal and listing remain simple:

```sh
open-forge route exclude docs/team/_team.md --dry-run
open-forge route list --depth all
```

Exclude removes the exact direct loader row and registration, preserving destination files and unrelated incoming links. It works when the destination is missing. Ambiguous duplicate rows require correction before either operation writes.

The other commands behave as follows:

- **Index** preserves included loader rows and uses the common graph to understand their destinations. Included leaves and catalogues remain read-only. It does not add frontmatter, discover unlisted neighbours or regenerate an imported catalogue.
- **Status** reports registered, active, inactive and missing inclusions. Current startup measurements include the actual required closure outside `.agents`, counting each physical file once. Pending conditions remain incomplete measurements.
- **Doctor** diagnoses missing targets, malformed catalogues, escapes, identity conflicts and registration differences. Registration without a loader row is informational and never an automatic repair.
- **Context** follows active entries using existing loading tags, conditions and ancestor rules. Configuration alone supplies no loading reason.
- **Route List** shows active hand-written and command-added roots. At the root listing, registered inactive sources appear as labelled rows without traversed children.
- **Find** searches active included leaves and catalogue descendants using exposing metadata and supported Markdown content. It never scans the surrounding directory merely because one file was included.
- **References** includes supported Markdown relationships from included sources, with honest coverage for opaque text formats. Existing exclusions retain their command-specific meanings.

Outside the main root, exact source operands use `./workspace/path`, such as `context ./CONTRIBUTING.md`. Do not invent short aliases. Existing main-root source IDs remain unchanged.

Hand-written loader links are first-class entries. They need no registration to work, and maintenance preserves them. Include can register one without rewriting it.

### 4. The config

For #6, add only `roots`. This is a registry of entry files, including ordinary leaves, rather than directory search roots.

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v3/open-forge.schema.json",
  "schemaVersion": 3,
  "roots": [
    "CONTRIBUTING.md",
    ".cursor/rules/testing.mdc",
    "docs/team/_team.md"
  ]
}
```

Existing settings remain alongside these keys. An absent `roots` means an empty registry.

For #7:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v4/open-forge.schema.json",
  "schemaVersion": 4,
  "mainRoot": "docs/forge",
  "roots": [
    "CONTRIBUTING.md",
    ".cursor/rules/testing.mdc",
    "docs/team/_team.md"
  ]
}
```

`mainRoot` defaults to `.agents`. It names one contained directory, excluding the workspace root, Git metadata and conflicting control locations.

Keep settings and ownership controls at `.agents/open-forge.json` and `.agents/open-forge.lock.json`. This fixed bootstrap location avoids searching for the configuration that tells the CLI where to search. A custom knowledge root therefore does not promise elimination of the `.agents` control directory.

The schemas define `roots` as unique, nonempty canonical workspace-relative path strings. Version 4 adds the validated `mainRoot` string. Publish new version directories and leave v2 unchanged. The [configuration Directive](../../../../../.agents/directives/software/versioned-configuration-schema.md) says, “Once a version ships, its schema document stops changing.”

Continue reading absent, older and newer versions, reporting differences and acting on understood keys. Preserve unknown properties during writes.

Include renders its supplied metadata into the loader:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
- [Testing conventions](../.cursor/rules/testing.mdc) - #LoadNow - applies to `src/**`
- [Team knowledge](../docs/team/_team.md)
```

Under `docs/forge/loader.md`, those destinations become `../../CONTRIBUTING.md`, `../../.cursor/rules/testing.mdc` and `../team/_team.md`.

**The loader wins for activation, ordering and loading metadata.** Configuration wins only for registration and main-root location. Deleting a loader row leaves an inactive registration. Deleting a registration while retaining the row leaves an active manual route. Neither Index nor Update reconciles those differences by reactivating content.

Do not add IDs, priorities, ownership claims, duplicated metadata or an `enabled` flag.

### 5. `help concepts` and the README guide

Use one read-only command:

```text
open-forge help [<topic>] [global flags]
```

Bare `help` lists available topics. Initially, `concepts` is the only topic. Unknown topics produce an actionable invalid-input result. Existing `--help` remains command reference. Concepts works without an installed workspace or network and supports the ordinary text and JSON result envelope.

The complete concepts outline is:

- **Purpose:** Plain files route relevant knowledge and preserve continuity, with an optional CLI.
- **Routes:** Loader entries, catalogues, entrypoints, leaves, scopes and concrete folder names.
- **Selection:** Descriptions expose choices, while unselected branches stay closed.
- **Loading:** `LoadNow`, `KeepInMind`, refresh boundaries and parent-chain requirements.
- **Conditions:** Working files, `applyTo`, complete inherited matches and pending conditions.
- **Authority:** Accepted requirements, evidence, conflicts and why reading grants no editing permission.
- **Roles:** Directives, Guidance, Patterns, Templates, Maps and native Skills.
- **Memory:** Self-growing Working, Emerging, Crystallized and Archived knowledge.
- **Metadata:** Descriptions, tags, responsibility, root and scoped forms, and foreign leaves.
- **Reuse:** Include existing project files, project Library files, or distribute Extensions.
- **Maintenance:** Entries, ownership receipts, exclusions, overwrites, previews and recovery.
- **Build your own:** Add only useful conventions and retain full operation without the CLI.

Remove the glossary, category survey and CLI explanation from the installed README. Move its complete authoring instructions into a small shipped `authoring.md`, and change the loader’s authoring prerequisite to that file.

The new README outline is:

- **Open Forge guide, you may remove this file**
- **Use a document you already have**
- **Write one rule you keep repeating**
- **Keep specialized knowledge on demand**
- **Stop using something**
- **Share useful work when it earns reuse**

Keep it around 250 words, with one inclusion example and links to authoring and concepts. Explain that permanent CLI-managed removal must record the README in exclusions.

Authoring rules matter because catalogues and metadata are interfaces. Authors must preserve relative links, useful descriptions, consistent tags, recognized entrypoint names, valid Axioms, scoped placement, the selected metadata form, conditions and current Entries. Native Skills retain native metadata. Overwrites remain adjacent companions.

These rules already have a home in [README Instructions](../../../../../src/open-forge/.agents/README.md). Move them without weakening them. Keep category-specific requirements in their catalogues. No shipped authoring Directive is needed, and deleting the optional README must not break authoring guidance.

### 6. The configurable main root (#7)

Introduce one resolved workspace layout passed to consumers. Distinguish workspace coordinates, the main knowledge root, fixed control paths and native discovery paths. The current [SourceLogicalPath](../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Identity/SourceLogicalPath.cs) uses `.agents` both as a constant and as a canonical-path requirement, so replacing literals alone is insufficient.

Add `install --root <workspace-directory>`. Fresh interactive installation asks:

> Where should Open Forge keep its knowledge files?

Offer `.agents` first and a custom directory second. An explicit flag skips the question. Existing configuration supplies the current value. Unattended installation defaults to `.agents`.

The managed `AGENTS.md` handoff and `CLAUDE.md` import point at the resolved loader. Instructions alone do not relocate harness-native discovery.

Keep native Skills at `.agents/skills`. With a custom main root, expose their existing catalogue through a loader entry. Do not create another Skill package tree or duplicate packages. Other harness-native files retain their required locations.

Package source coordinates remain stable. Framework assets and Extension content under logical `.agents` map to the selected main root, except native Skills and fixed controls. Other explicit workspace destinations retain their locations and permission requirements. Rebase supported Markdown links when mapping separates their endpoints. Arbitrary script strings and foreign formats require author adaptation, not speculative rewriting.

Library mappings remain explicit. Changing the main root does not silently retarget an attached Library.

Existing workspaces stay unchanged until explicitly configured. Support migration through:

```text
open-forge install --configure --root docs/forge --dry-run
```

Its reviewed plan moves the known knowledge tree, preserves authored notes and overwrites, rewrites proven local references and managed handoffs, updates affected receipts and exclusions, and retains controls and native Skills. Collisions, uncertain ownership and unsupported references block migration with named paths. Publish `mainRoot` only after destination verification. Preserve recovery evidence and report partial effects honestly.

Implement in ordered slices, each suitable for one owner:

1. Freeze layout, package mapping, source identity and migration contracts. Establish default-root fixtures.
2. Add settings decoding and the resolved layout value, initially retaining default behavior.
3. Convert source identity, inventory, catalogue traversal and reference resolution.
4. Convert Context, Find, References, Route List, Inspect, Status and Doctor.
5. Convert navigation and route mutations, preserving read-only included sources.
6. Convert Framework delivery, Extension mapping, permissions, exclusions and receipts.
7. Separate native Skills and update harness handoffs, then prove one complete custom-root installation.
8. Implement reviewed migration, then qualify update, removal, recovery and documentation together.

After slice 7, inspect the complete journey before expanding migration cases.

Estimate 15–25 engineer-days including qualification and documentation. The migration and package-reference boundaries dominate uncertainty. Refuse arbitrary external roots, automatic root discovery, generic mount machinery, Skill duplication and blind string replacement in #7.

### 7. Big changes to CLI usage

For #6, users gain Include, Exclude and Help Concepts. Existing exact-path selectors expand beyond `.agents` using `./` paths. Route listings expose registered inactive entries. Read commands cover included content, and startup estimates can increase because they include previously omitted required files. Inclusion grants no content-management permission.

For #7, users gain `install --root` and an installation question. Existing source IDs remain stable, while exact paths, generated destinations, handoffs and reported paths follow the chosen root. Skills and controls retain fixed locations. Extension plans show mapped destinations. Migration requires explicit configuration.

Older CLIs ignore unknown settings. That preserves ordinary compatibility for #6, but an older CLI cannot manage a custom-root workspace correctly. Document the minimum CLI version and warn against older mutating commands there.

Keep current presets, default root, command names and omission preservation. README removal becomes safe because authoring no longer depends on it.

### 8. Documents to update

The following paths are exact. Braces enumerate individual filenames.

For #6:

- `README.md`
- `docs/cli.md`
- `docs/extensions.md`
- `schemas/v3/open-forge.schema.json`
- `src/open-forge/.agents/{loader.md,README.md,authoring.md}`
- `src/open-forge/.agents/skills/open-forge-cli/SKILL.md`
- `src/open-forge/.agents/skills/open-forge-cli/references/{common.md,discovery.md,routes.md,packages.md}`
- The corresponding dogfood files under `.agents/`.
- `src/docusaurus/docs/cli/{index.md,flows.md}`
- `src/docusaurus/docs/concepts/{index.md,routing.md,loading-and-tags.md,customizing.md}`
- `src/docusaurus/docs/getting-started/{installation.md,grow-your-framework.md,ten-minute-guide.md}`
- `src/docusaurus/docs/{glossary.md,limits.md}`
- `.agents/memory/crystallized/documents/cli/command-contract-set.md`
- `.agents/memory/crystallized/documents/cli/contracts/{_contracts.md,route/_route.md}`
- New contract files `route/include/{_include.md,interface.md,behavior.md}`, `route/exclude/{_exclude.md,interface.md,behavior.md}` and `help/{_help.md,interface.md,behavior.md}` beneath that contracts directory.
- Both `interface.md` and `behavior.md` in existing contract directories `context`, `find`, `references-candidate`, `index-candidate`, `status`, `doctor`, `route/list`, `route/inspect` and `shared/source-references`.
- `.agents/memory/crystallized/documents/framework/{architecture.md,markdown/routes.md,markdown/syntax.md,routing/model.md,routing/loading.md,routing/paths.md}`
- `.agents/memory/crystallized/documents/maintenance/payload/agents/loader.md`
- New `.agents/memory/crystallized/documents/maintenance/payload/agents/authoring.md`.

For #7, revisit those affected paths and add:

- `schemas/v4/open-forge.schema.json`
- `src/open-forge/{AGENTS.md,CLAUDE.md}` and dogfood counterparts.
- `.agents/memory/crystallized/documents/cli/architecture.md`
- `.agents/memory/crystallized/documents/cli/technical-designs/{workspace-permissions.md,workspace-libraries.md}`
- Both contract files under `install`, `update`, `remove`, `repair`, `cleanup`, `route/{create,init,update,move,remove}`, `shared/workspace-permissions`, `extension/{list,inspect,create,install,update,remove}` and `library/{list,inspect,attach,sync,detach}`.
- `.agents/memory/crystallized/documents/{architecture.md,extensions/architecture.md,framework/primitives/skills.md,maintenance/payload/agents/skills.md}`
- `src/docusaurus/docs/concepts/core-categories.md`
- `src/docusaurus/docs/extensions/reading-installed-files.md`

Update affected catalogue Entries when adding these files. The documentation site already publishes the CLI and Extension guides, so edit their sources rather than creating competing references.

### 9. Risks

The largest #6 risk is accidentally making configuration reactivate deleted loader rows. Detect it first with a registered source whose row is removed, then exercise Index, Install, Configure, Update and Extension delivery. It must stay off.

The next risk is treating inclusion as ownership. Use fixtures containing foreign frontmatter, authored catalogue rules and malformed metadata. Reading and diagnosis must not convert them.

Graph traversal can lose conditions, duplicate physical files or follow cycles. Check overlapping branches, aliases and nested conditions through both CLI Context and plain-file reading.

Root migration can separate receipts from files, weaken path permissions or break native Skills. Prove a custom-root workspace containing an Extension, a Library, private Working records and user overwrites before widening implementation.

Documentation already shows drift. The loader maintenance contract still prescribes old `Terms` and CLI sections absent from the shipped loader. Reconcile it against current source rather than copying its structure forward.

Partial two-file inclusion writes also matter. Publish registration before activation, and remove activation before registration. Revalidation, existing recovery mechanisms and rerun convergence must prevent a false success.

This proposal is based on read-only inspection. No builds, test suites or Doctor runs were performed.

### 10. The one idea to steal

Treat configuration as a registry and the loader as the switch, so CLI convenience can never silently turn knowledge back on.

## The Model-Behavior Realist (Proposal C)

### Thesis

Add `route include` to expose existing project knowledge through ordinary loader entries, with config tracking for maintenance. Keep the loader authoritative for reading, and give authoring instructions a permanent home before making the README removable. Ship configurable storage separately, with explicit settings discovery and migration.

### The concept and its name

Extensions distribute reusable files as packages. Libraries reuse a source through projected file links. The new thing is an **inclusion**, which exposes knowledge already at its intended project location.

That expresses the maintainer’s stratification without promising capabilities Libraries do not yet have. The current [CLI reference](../../../../../docs/cli.md) says Libraries register “a contained source root” and project it into another workspace location. General sharing across arbitrary folders on one computer remains a separate expansion.

Use `route include` and `route exclude`. They belong under `route` because they change which existing sources the loader exposes. They need no new top-level group.

Reject `link`. Libraries already create filesystem links, while inclusion creates a Markdown entry. Reject `route add`, another floated candidate, because it leaves unclear whether the command creates content, initializes catalogues, or exposes an existing file.

Keep `route create` and `route init` unchanged in #6. Create writes a new ordinary Markdown file beneath an existing root. Init creates missing entrypoints. Include requires an existing target and changes only its registration and loader entry. This distinction makes their rules disjoint.

### The CLI surface

Proposed syntax:

```text
open-forge route include <workspace-path>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--clear-tags]
  [--clear-apply-to]
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-path> [--dry-run] [global flags]

open-forge route list [<source-reference>]
  [--depth <nonnegative-integer|all>] [global flags]

open-forge route list --included
  [--depth <nonnegative-integer|all>] [global flags]

open-forge context [<source-reference>...]
  [--authoring] [existing options] [global flags]
```

Three inclusion examples:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow --dry-run
open-forge route include docs/_docs.md --description "Project knowledge" --dry-run
open-forge route include .cursor/rules/testing.mdc --description "Testing rules" --tag LoadNow --apply-to "src/**" --dry-run
```

Apply the reviewed request without `--dry-run`. No confirmation or `--automatic` is needed, matching existing non-destructive route maintenance.

A new inclusion requires a description. Tags default to empty, which means on demand. Repeating Include preserves unspecified existing fields and replaces explicitly supplied fields. Repeated tags form one ordered list. Clear flags conflict with their corresponding value flags. An identical request is a verified no-op.

A readable text file is a leaf. Its entry supplies loading metadata, and foreign frontmatter does not activate anything. A recognized Markdown entrypoint exposes its authored `Entries` recursively. Recognize `_{folder}.md` and the existing compatibility names. An arbitrary underscore filename is not automatically a catalogue.

Reject directories with an example of the required entrypoint path. Reject URLs, globs, fragments, binary files, workspace escapes, unsafe physical aliases, cycles, and competing catalogue ancestry. Missing targets and invalid arguments return `invalid-input`. Unsafe or ambiguous existing state returns `blocked`, without writes. Exclude can remove a registration whose target has disappeared.

Removal is:

```sh
open-forge route exclude docs/_docs.md --dry-run
```

It removes the loader entry and registration, preserving every target file. Existing `route remove` must refuse an included source and direct users to Exclude rather than delete project documentation.

Command behavior follows one graph:

- `context` follows enabled loader entries in order, including handwritten entries. Required tags, pending conditions, inherited conditions, overwrites, and explicit required-read instructions retain their meanings.
- `route list` shows enabled routes with exact paths and registered or handwritten origin. `--included` lists registrations, including disabled, missing, and divergent ones. It cannot take a source operand.
- `find` searches enabled included sources using entry metadata and Markdown headings where applicable. Discovering a result does not activate its rules.
- `references` includes authored links in included Markdown and incoming links to included targets. Loader `Entries` remain navigation, which the current reference command excludes from reference counts. Foreign text formats gain no invented link parser.
- `index` preserves inclusion membership and authored external entries. In #6 it reads and validates external catalogues but does not rewrite their metadata, discover unlisted siblings, or generate catalogues outside the main root.
- `status` reports registration state and counts external required text in startup estimates. Unknown conditions and unreadable files make coverage explicit.
- `doctor` diagnoses missing targets, malformed catalogues, cycles, ancestry collisions, and config drift. It never enables or repairs an inclusion automatically.

Use `./docs/architecture.md` as an unambiguous exact workspace source reference. Existing IDs and `.agents/...` references remain valid.

Handwritten links need no registration. Include adopts an existing matching loader entry without duplicating it or replacing unspecified metadata.

### The config

For #6, add `roots` to `.agents/open-forge.json`, retaining existing settings:

```json
{
  "schemaVersion": 2,
  "roots": [
    {
      "path": "CONTRIBUTING.md",
      "description": "Contribution rules",
      "tags": ["LoadNow"]
    }
  ]
}
```

Here, roots are exact starting sources, including files. They are not recursive directory scan permissions. Each object requires `path`, `description`, and `tags`, with optional `applyTo` as an array of patterns. Store paths relative to the workspace with forward slashes.

Include renders that example into `.agents/loader.md` as:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

The [shipped loader](../../../../../src/open-forge/.agents/loader.md) already says, “The list under `Entries` at the bottom is the whole setup.” Preserve that contract.

The loader wins on membership, order, description, tags, and conditions. Config records what Include manages. If someone removes the loader line, the registration becomes disabled. Index, Update, Configure, and package operations must not restore it. If someone edits its metadata, commands use the loader’s values and report drift. Explicit Include reconciles the selected registration. Adding config alone never enables a source.

Publish inclusion config before its loader line. Exclude removes the loader line before removing config. An interrupted operation therefore leaves a visible discrepancy without creating hidden activation. Reuse existing locking, expected-state checks, recovery, and verification.

Extend the v2 schema additively. Keep unknown properties accepted and preserve them during edits. Older CLIs can ignore `roots` while reading the ordinary loader entries.

The proposed #7 shape is:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v3/open-forge.schema.json",
  "schemaVersion": 3,
  "mainRoot": ".forge",
  "roots": [
    {
      "path": "CONTRIBUTING.md",
      "description": "Contribution rules",
      "tags": ["LoadNow"]
    }
  ]
}
```

`mainRoot` defaults to `.agents`. Extra paths remain workspace-relative when it changes.

Custom-root settings move to workspace-level `open-forge.json`. Existing default workspaces retain `.agents/open-forge.json`. Discovery checks those two fixed locations and blocks if both exist. It never searches arbitrary directories. The new schema version documents this layout boundary, and custom-root workspaces require the newer CLI.

### `help concepts` and the README guide

Use `open-forge help [<topic>]`. With no topic it lists topics. Initially `concepts` is the only topic. An unknown topic is invalid input. Existing `--help` remains command-interface help.

The complete concepts outline is:

- **Start here:** Read the workspace loader before work, through files or Context.
- **What is switched on:** Loader membership, ordering, missing sources, and valid empty lists.
- **Choose context:** Required reads, on-demand selection, explicit reading instructions, and search recovery.
- **Conditions:** Exact glob semantics, same-file ancestry matching, and pending unknown working files.
- **Rules and acceptance:** Axioms, Instructions, additive scope, conflicts, and the distinction between reading and acceptance.
- **Loading tags:** LoadNow, KeepInMind refresh points, and descriptive tags.
- **Content roles:** Directives, Guidance, Maps, Patterns, Templates, Skills, and optional Memory states.
- **Reuse knowledge:** Extensions, Libraries, inclusions, handwritten entries, and management boundaries.
- **Author valid files:** Complete metadata, catalogue, Directive, and loader-entry examples.
- **Customize safely:** Overwrites, exclusions, disabling, stopping management, and deletion.
- **Maintain navigation:** Index’s writable boundaries and external authored catalogues.
- **Workspace layout:** Main root, extra sources, native discovery, and migration.
- **Build your own:** Create only useful rules and structures, then share them when reuse warrants it.

Move README `Instructions` into `.agents/authoring.md`, an ordinary format reference outside the routed catalogue system. Do not ship an authoring Directive.

The current [README](../../../../../src/open-forge/.agents/README.md) says agents read it “only when they write workspace files.” Deleting it today would remove a required dependency. Replace that dependency before advertising removal.

The authoring reference preserves these concrete rules:

Use relative relationships and consistent useful tags. Put detail in its narrowest defining source. Give each routed folder one recognized catalogue. Put catalogue rules under `## Axioms` and Directive obligations under non-empty `## Instructions`. Empty Axioms add nothing, and the inherited marker cannot mix with local rules. Scopes narrow following content without requiring copied defaults.

New framework-owned routed Markdown carries `description` and `tags`, with optional `responsibility` and `applyTo`. Follow root or scoped metadata settings, keeping fields together and tags without `#`. Explain accepted condition forms, conflicting declarations, entry syntax, compatible catalogue names, empty Entries, and overwrite naming. Included existing files need no added frontmatter. Native Skills retain native metadata.

These are interoperability rules. They let agents produce files that both catalogue readers and the CLI interpret consistently.

The loader must still state the complete reading procedure, authority boundaries, loading and condition rules, off-switch behavior, missing-file errors, overwrite behavior, and this authoring trigger in plain words:

“Before writing workspace context, read `authoring.md`. With the CLI, `context --authoring` returns that same file.”

`context --authoring` adds the local reference and its overwrite companion to ordinary context without enabling another branch. Concepts help includes the complete authoring text, using the local version when a workspace is present and the bundled version otherwise. A missing local reference is reported rather than silently replaced. Help also directs readers through the current loader before workspace edits.

The removable README outline becomes:

- **You may remove this guide:** No loading or authoring rule depends on it. Explain how to record its removal against restoration.
- **Use one document you already have:** Include and handwritten-entry examples.
- **Write your first project rule:** Encourage user-authored Directives.
- **Keep useful knowledge:** Start small and preserve accepted outcomes.
- **Turn something off:** Explain the distinct effects.
- **Reuse your framework elsewhere:** Explain when inclusion, Library, or Extension fits.
- **Find the details:** Loader, authoring reference, concepts help, and website.

Definitions and mandatory authoring mechanics leave the README.

### The configurable main root (#7)

Resolve one workspace layout before constructing source identity, settings, ownership, navigation, or operations. `.agents` remains the default and a legacy payload coordinate. It stops being the universal physical destination.

The settings reader currently starts from `.agents/open-forge.json`, while `SourceLogicalPath` combines validation with that prefix. Those are distinct responsibilities. Introduce layout resolution and general contained workspace paths before adapting consumers. A global string replacement would confuse default examples, native paths, package coordinates, and physical destinations.

Fresh installation asks: “Where should Open Forge keep its loader and knowledge?” Offer `.agents` first and a custom workspace folder second. `install --main-root <directory>` supplies the choice without prompting. Existing workspaces change through `install --configure --main-root <directory>`, with preview and explicit application.

Custom roots contain the loader, authoring reference, ordinary framework categories, ownership, and recovery state. Settings stay at the fixed discovery location described above.

Skills retain harness-native destinations. For the existing `.agents/skills` layout, keep packages there and expose their catalogue through an inclusion from the custom loader. Do not create `.forge/skills` and imply that harnesses discover it. Other tools’ discovery files retain their native locations. Reading a Skill never guarantees runtime activation.

Migration must preserve user files, private records, omissions, loader membership, overwrite companions, and management claims. Rebase links by resolved destination. Update managed `AGENTS.md` and `CLAUDE.md` handoffs. An occupied destination, ambiguous configuration, or unsupported reference rewrite blocks the complete plan. Editing `mainRoot` alone never moves files.

Implement ordered slices with separate ownership:

1. **Layout contract and discovery:** Own schema, settings precedence, defaults, validation, and resolved layout.
2. **Path and source identity:** Own contained paths, root-relative IDs, exact references, collisions, and legacy compatibility.
3. **Reading and navigation:** Own graph construction, Context, Find, References, route reports, Index, Status, and Doctor.
4. **Framework delivery:** Own Install, Configure, Update, payload projection, omissions, and managed handoffs.
5. **Packages and permissions:** Own Extension destination mapping, native Skill exceptions, Library boundaries, and actual write admission. Library source layouts remain literal.
6. **Migration:** Own reviewed relocation, ownership-path changes, link rebasing, interruption recovery, and settings publication.
7. **Documentation and verification:** Own examples, contracts, default/custom-root journeys, packaging, and supported Native AOT evidence.

Freeze shared contracts before assigning dependent slices. Classify each hard-coded occurrence rather than replacing all of them. Keep default-layout snapshots and add focused custom-layout evidence.

Estimate #6 at six to ten engineering days and #7 at fifteen to twenty-five, including documentation and verification. Migration and package destination mapping carry the largest uncertainty.

### Big changes to CLI usage

#6 adds Include, Exclude, inclusion listing, exact `./` workspace references, `help concepts`, and `context --authoring`. Startup estimates include external required text. Discovery commands expose included sources. External catalogues remain authored and read-only to Index. Existing loader links require no conversion.

Exclude disables exposure without deleting knowledge. Existing deletion commands explicitly refuse inclusion targets. The README becomes removable after its required authoring content moves.

#7 adds `--main-root` and an installation question. Custom-root settings use workspace-level `open-forge.json`, and managed handoffs name the selected loader. Exact paths, ownership locations, permission descriptions, and migration previews reflect the selected root. Ordinary route IDs remain stable.

Skills stay at native discovery locations. Custom-root workspaces require a compatible CLI. Existing workspaces are not relocated automatically, and default `.agents` commands remain valid.

### Documents to update

For #6, update `README.md`, `docs/cli.md`, `docs/extensions.md`, `schemas/v2/open-forge.schema.json`, and the shipped and dogfood `loader.md` and `README.md`. Add shipped and dogfood `authoring.md`.

Update shipped and dogfood `skills/open-forge-cli/SKILL.md` and all four references: `common.md`, `discovery.md`, `routes.md`, and `packages.md`.

Under `.agents/memory/crystallized/documents/cli/`, update `command-contract-set.md`, `contracts/_contracts.md`, and `contracts/route/_route.md`. Add:

```text
contracts/route/include/_include.md
contracts/route/include/interface.md
contracts/route/include/behavior.md
contracts/route/exclude/_exclude.md
contracts/route/exclude/interface.md
contracts/route/exclude/behavior.md
contracts/help/_help.md
contracts/help/interface.md
contracts/help/behavior.md
```

Update both `interface.md` and `behavior.md` in these existing contract directories: `context`, `find`, `references-candidate`, `index-candidate`, `status`, `doctor`, `route/list`, `route/inspect`, `route/remove`, `shared/source-references`, and `shared/source-universe-filters`. Update `context/technical-design.md`, `index-candidate/technical-design.md`, and `technical-designs/generated-navigation.md`.

Under `src/docusaurus/docs/`, update `intro.md`, `glossary.md`, `concepts/index.md`, `concepts/routing.md`, `concepts/loading-and-tags.md`, `concepts/customizing.md`, `getting-started/installation.md`, `getting-started/ten-minute-guide.md`, `getting-started/greenfield-and-brownfield.md`, `getting-started/grow-your-framework.md`, `cli/index.md`, and `cli/flows.md`. The Library flow needs the accurate reuse distinction. Add `concepts/reusing-project-knowledge.md` and register it in `src/docusaurus/sidebars.ts`.

For #7, revise those layout-sensitive documents, add `schemas/v3/open-forge.schema.json`, and update `docs/development.md`, shipped `AGENTS.md` and `CLAUDE.md`, plus their dogfood managed sections.

Update both contract files for `install`, `update`, `remove`, `repair`, `cleanup`, every existing `route` operation, all six `extension` operations, all five `library` operations, and `shared/workspace-permissions`. Add `technical-designs/workspace-layout.md`. Revise `architecture.md`, `technical-designs/embedded-payload.md`, `technical-designs/lifecycle-provenance.md`, `technical-designs/workspace-libraries.md`, and `technical-designs/mutation-and-recovery.md`.

Across both PRs, update the corresponding accepted framework documents: `framework/architecture.md`, `framework/markdown/syntax.md`, `framework/markdown/routes.md`, `framework/routing/model.md`, `framework/routing/paths.md`, `framework/routing/loading.md`, and `framework/primitives/skills.md`.

Update maintenance contracts `maintenance/payload/agents/loader.md`, `maintenance/payload/agents/skills.md`, `maintenance/payload/AGENTS.md`, and `maintenance/payload/CLAUDE.md`. Add maintenance contracts for the guide and authoring reference. Preserve earlier council records as historical proposals.

### Risks

The likeliest failure is divergent reading caused by config, filesystem scanning, or metadata interpretation. Build one fixture containing a direct foreign file, nested conditional catalogue, disabled registration, handwritten entry, and overwrite. Compare expected ordered reads across loader-only use, Context-only use, and concepts-help-led use.

The second failure is losing authoring rules when the README disappears. Delete it in that fixture and ask each agent path to write a valid Directive and catalogue. Inspect the resulting files and actual read traces.

The third failure is converting inclusion into ownership. Verify that every #6 maintenance command leaves external target bytes unchanged and that Exclude never deletes them.

For #7, watch for split settings, unintended restoration, broken native discovery, stale ownership paths, and partial migration. Detect these with occupied-target and interrupted-migration cases before widening implementation.

Finally, source consistency cannot prove agent compliance. Require a small observed behavior evaluation alongside mechanical checks. This proposal is based on read-only inspection, with no builds, diagnostics, or test suites run.

### The one idea to steal

Require loader-only reading, Context-only reading, and concepts-help-led reading to pass the same ordered-read and authoring fixtures before either PR ships.

## The Migration And Safety Engineer (Proposal H)

### 1. Thesis

Add `route include` to expose existing project knowledge without moving it or taking ownership of it. Keep the loader authoritative for activation, with configuration recording the command’s registrations. Ship configurable content roots separately, through an explicit migration that preserves private files, native capabilities, ownership and recovery history.

### 2. The concept and its name

Extensions distribute reusable content as packages. Libraries expose reusable local content through relative file links. An **include** exposes knowledge already present in the project, directly at its existing path.

This describes a useful progression in distribution, without changing authority. The current Library implementation is narrower than “anywhere on the same computer”. Its [technical design](../../../../../.agents/memory/crystallized/documents/cli/technical-designs/workspace-libraries.md) requires a workspace-contained source root. Expanding that boundary belongs in separate work.

Use a new `route` subcommand:

```text
open-forge route include <workspace-relative-file> [options]
```

“Link” is misleading because Libraries already create filesystem links, while this operation creates a Markdown entry. “Route add” blurs the distinction between exposing existing content and creating content. A top-level `root` group would imply directory-only inputs and prematurely combine this feature with configurable storage.

The [current contracts](../../../../../.agents/memory/crystallized/documents/cli/command-contract-set.md) distinguish the operations cleanly. `route create` creates an ordinary routed Markdown file. `route init` creates missing entrypoints or restores Framework categories. Include requires an existing file and changes its exposure, so neither existing command needs another mode.

### 3. The CLI surface

```text
open-forge route include <path>
  --description <text>
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]

open-forge route exclude <path> [--dry-run] [global flags]
open-forge route list --includes [global flags]
```

These bounded Route mutations apply directly, like Route Create and Index. They have no confirmation wizard or `--automatic`.

Three examples:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow --dry-run
open-forge route include docs/_docs.md --description "Project documentation" --tag Project
open-forge route include .cursor/rules/testing.mdc --description "Testing conventions" --tag LoadNow --apply-to "src/**"
```

A normal file is an opaque leaf whose description, tags and conditions come from its loader line. Another tool’s frontmatter does not control Open Forge loading. A recognized Markdown entrypoint, including `docs/_docs.md`, exposes its catalogue chain. An underscore alone is insufficient. Existing compatibility names remain supported.

Require an existing readable text file inside the workspace after physical resolution. Reject directories, globs, URLs, binary files, escaping paths, reserved control files, malformed catalogues and cycles. Preserve existing safe-link containment rules. Diagnose ambiguous physical aliases rather than guessing.

Repeated identical inclusion is a no-op. An existing matching handwritten loader line can be registered without rewriting it. Conflicting supplied metadata blocks with an instruction to edit that line. Malformed loader structure blocks mutation.

Exclude unregisters the path and removes its direct loader exposure. It never deletes the source or removes references from other catalogues. Report remaining exposure when another branch still reaches it.

The other commands behave as follows:

- `context` follows active includes with normal loading tags, inherited conditions and pending-condition reporting.
- `route list` shows active included sources and catalogue descendants. `--includes` reports registered paths as active, disabled or missing.
- `find` searches included Markdown and entry metadata. Opaque text leaves participate through their entry tags, without interpreting foreign metadata.
- `references` includes authored Markdown references across these sources. Entries remain navigation and are excluded from authored-reference counts, as [documented today](../../../../../docs/cli.md).
- `status` counts the complete active startup closure, including files outside `.agents`, and reports unavailable estimates honestly.
- `doctor` diagnoses missing active targets, cycles, malformed entries and registration disagreement.
- `index` preserves loader exposure and uses included catalogues when observing topology. In #6 it does not complete metadata, create catalogues or rewrite Entries inside included project content. Those catalogues remain authored in place.

Handwritten and command-added entries use the same loading semantics. Registration grants neither source ownership nor write permission.

### 4. The config

For #6, add this optional member to `.agents/open-forge.json`:

```json
{
  "schemaVersion": 2,
  "roots": [
    { "path": "CONTRIBUTING.md" },
    { "path": "docs/_docs.md" }
  ]
}
```

For #7:

```json
{
  "schemaVersion": 2,
  "mainRoot": "docs/forge",
  "roots": [
    { "path": "CONTRIBUTING.md" },
    { "path": "docs/_docs.md" }
  ]
}
```

These are additions to existing settings, not replacement documents. `roots` records extra file entrypoints, despite the property name. `mainRoot` identifies the managed content directory and defaults to `.agents`.

Paths are canonical workspace-relative paths using `/`. Registration order is retained, duplicate canonical paths are rejected, and runtime validation checks physical identity and containment. Extend `schemas/v2/open-forge.schema.json` additively. Missing keys preserve existing behavior. Preserve unknown members on writes.

Keep schema version 2 because these are optional additions. The [schema](../../../../../schemas/v2/open-forge.schema.json) explicitly says that unknown keys are accepted and ignored. A new number would not prevent an older CLI from writing.

Include writes configuration and one ordinary loader line in the same checked plan:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

Descriptions, tags, conditions and effective order live only in the loader. Configuration records registration identity, avoiding another editable copy of loading policy.

**The loader wins on activation.** A registered path absent from Entries is disabled. Index, Update and Configure must not recreate it. A loader line absent from configuration remains active handwritten content. A configuration-only addition is reported as disabled until explicitly included.

This preserves the [loader’s existing promise](../../../../../src/open-forge/.agents/loader.md): “A removed line stays removed until the user asks to restore it.”

### 5. `help concepts` and the README guide

Add `open-forge help` as a topic inventory and `open-forge help concepts` as a workspace-independent, read-only guide. Keep existing `--help` behavior. Unknown topics return invalid input with available topics.

The concepts outline should contain these sections, in this order:

1. Purpose — use existing project knowledge and grow only what helps.
2. Files and authority — reading, acceptance, ownership and edit permission are separate.
3. Loader and Entries — listed means enabled, with empty lists valid.
4. Routes and catalogues — entrypoints expose deeper context progressively.
5. Loading tags — LoadNow, KeepInMind and on-demand selection.
6. File conditions — workspace-relative globs filter selected branches.
7. Content roles — Directives, Guidance, Maps, Memory, Patterns, Templates and Skills.
8. Reuse — includes, Libraries and Extensions, with their actual boundaries.
9. Authoring — metadata, catalogue names and navigation conventions.
10. Customization — overwrites, omissions and persistent exclusions.
11. Maintenance — Index, Doctor, Repair, ownership and recovery.
12. Layout — main content root, extra entrypoints and fixed host discovery locations.

Move framework vocabulary and command explanations out of `.agents/README.md`. Move its mandatory authoring instructions to a shipped, non-routed `authoring.md`, referenced by the loader before authoring. Embed that same source in the CLI’s concepts material. Do not introduce an authoring Directive.

The new README outline is:

1. **You may remove this guide** — it is optional and does not control loading.
2. Use one document you already have.
3. Write your first project rule.
4. Save knowledge worth reusing.
5. Add a scope or capability when needed.
6. Share through an Extension or Library.
7. Preview changes and follow recovery instructions.

Explain removal through the CLI’s persistent exclusion mechanism, or its manual equivalent, so Update respects the choice.

Keep the authoring rules. They tell authors to use one recognized catalogue per folder, meaningful descriptions and tags, consistent frontmatter, valid conditions, relative links, current Entries and adjacent overwrite companions. Catalogue Axioms and rule-file Instructions have distinct roles. These conventions make discovery and loading predictable. They are needed by agents without the CLI, but only when authoring.

### 6. The configurable main root (#7)

Treat `mainRoot` as **content layout**, separate from workspace identity and maintenance storage. Initially retain `.agents/open-forge.json` and `.agents/open-forge.lock.json` at their stable locations. A custom content root therefore does not promise an entirely absent `.agents` directory. This bounded compromise avoids adding configuration discovery and control-store migration to the same PR.

Replace global root assumptions with one resolved workspace layout passed to consumers. Separate shipped logical paths, installed workspace paths and physical identities. Existing source IDs such as `memory/crystallized` remain stable. Exact paths remain literal paths and never silently redirect.

Fresh Install accepts `--main-root <directory>`. Interactive Install asks “Where should Open Forge keep its context files?” with `.agents` selected by default and a custom-folder choice. Preview, JSON and unattended execution never ask. Existing workspaces change through:

```sh
open-forge install --configure --main-root docs/forge --dry-run
open-forge install --configure --main-root docs/forge --automatic
```

A hand edit to `mainRoot` moves nothing. If settings disagree with recorded layout, report incomplete layout and block writing commands until an explicit configuration plan resolves it. Never interpret a missing custom loader as permission to install a second Framework.

Migration must inventory actual filesystem content, including ignored and untracked files. Git alone cannot preserve private Working notes. Include overwrite companions, binary resources and user-owned descendants. Leave unrelated files in the old directory untouched.

Preflight requires ordinary contained directories, an unambiguous destination and a complete path map. Block destination collisions, unsupported nested repositories, unsafe links and unresolved recovery operations touching selected paths. Preserve old and new locations in the recovery evidence before any replacement or deletion. Verify copied bytes before retiring originals. Do not imply automatic rollback, which the current mutation architecture does not provide.

**Git-ignored routes need their own migration evidence.** Translate Install-owned ignore rules and lock sharing records together, including the actual compatible entrypoint filename. Preserve authored ignore rules byte-for-byte. If a broad authored rule would expose private content at the new location, block with a concrete correction. Never stage or untrack files. Compare shareable Entries before and after migration so private filenames cannot leak during regeneration.

**Keep the mutation lock identity unchanged.** [WorkspaceLockPathIdentity](../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Mutation/Locking/WorkspaceLockPathIdentity.cs) keys it from the physical workspace. Migration and ordinary commands must take that same lease, regardless of content root.

**Keep recovery history addressable.** [RecoveryBundlePathIdentity](../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs) also uses physical workspace identity. Historical manifests keep their original workspace-relative target paths. Never reinterpret them through today’s `mainRoot`. Completed bundles remain inspectable and cleanable. Interrupted migration reports its verified stage and explicit continuation. It must not trigger a fresh installation.

**Migrate ownership through verified effects.** Record layout provenance in the ownership document, update moved file and generated-region claims, and retain owner IDs and unaffected claims. Preserve settings exclusions and link-ignore lists by remapping only paths proven to move. Preserve category exclusions as category identities. Path grants remain literal destinations and must not broaden implicitly.

**Keep Skills at host-native locations.** A catalogue under a custom root cannot make a host discover capabilities there. Retain existing native packages, including the CLI Skill, and expose them from the custom loader where selected. Do not create a competing custom-root Skills category or install duplicate copies. The [shipped Skills contract](../../../../../src/open-forge/.agents/skills/_skills.md) states: “Your agent runtime controls activation, invocation, installation and execution.”

Keep `AGENTS.md` and `CLAUDE.md` at workspace root. Render their managed pointers to the selected loader, including Claude’s direct import. Preserve all bytes outside managed spans. Invalid or ambiguous boundaries block migration before writes.

**Map packages deliberately.** Extension package source layout remains portable. Map its logical `.agents` context destinations through the layout resolver, while native capability and tool-specific destinations retain their declared locations and permission checks. Record actual installed destinations so subsequent Update and Remove use the same mapping.

Libraries retain explicit source roots. Change projected destinations only when they belong to the selected relocation, then recompute relative link targets, owned ignore rules and ownership records. Never move Library sources merely because they sit nearby. Block a source tree inside the selected relocation until the user resolves that conflict.

Implementation slices should be ordered and independently owned:

1. Freeze layout, migration, ownership and recovery contracts. Introduce the resolver with unchanged default behavior.
2. Convert source identity, discovery and loading to the resolver. Prove stable IDs and default output.
3. Convert generated navigation, privacy filtering and root-relative authoring paths.
4. Convert Route operations, Framework lifecycle, exclusions and permission evaluation.
5. Convert Extension destination mapping and ownership publication.
6. Convert Library projection, relative links, ignore ownership and recovery attribution.
7. Implement Install migration and managed host-pointer rendering after those consumers converge.
8. Complete help, documentation and published-process platform journeys.

After slice 1, separate workers can own the source-reading, package and Library areas. One owner retains shared layout and ownership contracts. Integrate before implementing migration, with no overlapping edits to shared serializers.

Required evidence includes extensions to existing `SourceLogicalPathTests`, `LoaderMembershipTests`, settings codec tests, `InstallOperationIntegrationTests` and `UpdateManagedHostIntegrationTests`. Add proposed `RootMigrationIntegrationTests` covering ignored notes, overwrites, collisions, exclusions, sharing, Library links, interrupted publication and repeated no-op configuration. Add published-process tests for custom-root install, migration, Update, Remove, Doctor, Cleanup and offline agent startup.

Run the managed population and Native AOT integration gates on the supported Windows, macOS and Linux x64 and ARM64 targets. Include a real host-discovery smoke test for each claimed integration.

Estimate #7 at 15–25 engineering days, including platform proof and documentation. This is a cross-command migration, not a constant replacement. Budget #6 separately at roughly 5–8 days.

### 7. Big changes to CLI usage

#6 adds Include, Exclude, registered-include listing and `help concepts`. Existing source consumers additionally accept `./<workspace-relative-path>` for exposed project files, avoiding ambiguity with existing IDs. Startup estimates can increase because they finally include enabled project content outside `.agents`.

#7 adds `install --main-root` and permits it as explicit Configure input. Logical source IDs remain stable, while exact installed paths change after migration. Skills can remain physically outside the content root. Settings and ownership retain their stable control locations.

Custom roots require the new CLI. Older binaries deliberately ignore unfamiliar settings and lock fields, so a schema bump cannot make downgrade safe. Document unsupported downgrade clearly and test the actual older executable before making compatibility claims. Default-root workspaces retain their existing usage.

### 8. Documents to update

For #6, update these public files:

- `README.md`, `docs/cli.md`, `docs/extensions.md`.
- `src/docusaurus/docs/concepts/index.md`, `routing.md`, `loading-and-tags.md`, `customizing.md`.
- `src/docusaurus/docs/getting-started/installation.md`, `grow-your-framework.md`, `greenfield-and-brownfield.md`.
- `src/docusaurus/docs/cli/index.md`, `flows.md`, and `src/docusaurus/docs/glossary.md`.

Update `src/open-forge/.agents/loader.md` and `README.md`, add `authoring.md`, and align their repository counterparts. Update `schemas/v2/open-forge.schema.json`.

Update `src/open-forge/.agents/skills/open-forge-cli/SKILL.md` and all four references, `common.md`, `discovery.md`, `routes.md`, `packages.md`, plus the matching `.agents/skills/open-forge-cli/` files.

Under `.agents/memory/crystallized/documents/cli/`, update `command-contract-set.md` and add the Include, Exclude and Help Concepts contract sets. Each new leaf needs `interface.md`, `behavior.md` and its local catalogue. Update `contracts/_contracts.md`, `contracts/route/_route.md`, and the affected contracts for Route List, Context, Find, References, Index, Status and Doctor. Retain the actual `index-candidate` and `references-candidate` directory names.

For #7, update those same surfaces with layout and migration behavior. Also update:

- `src/open-forge/AGENTS.md`, `CLAUDE.md` and their managed repository counterparts.
- `schemas/v1/open-forge.lock.schema.json`.
- CLI `architecture.md`, `technical-designs/mutation-and-recovery.md`, `workspace-libraries.md`, and a new `workspace-layout.md`.
- Install, Update, Remove, Repair, Cleanup, all Route, Extension and Library contract pairs.
- Shared source-reference and workspace-permission contracts.
- Framework `architecture.md`, `routing/model.md` and `markdown/syntax.md`.
- Maintenance contracts under `maintenance/payload/`, especially `AGENTS.md`, `CLAUDE.md`, `agents/loader.md` and `agents/skills.md`.

Update affected payload inventory descriptions and generated catalogue Entries with each PR.

### 9. Risks

The likeliest #6 failure is stale registration resurrecting a removed loader line. Test removal followed by every regeneration and lifecycle operation first.

The likeliest #7 failures are lost ignored notes, private navigation becoming shared, stale ownership paths, broken relative Library links and disappearing native Skills. Detect them with small real-workspace fixtures before broad snapshot updates.

A split cutover can leave pointers, configuration and ownership disagreeing. Inject interruption at each publication stage and verify an explicit, repeatable continuation.

Older CLI use is a real compatibility limit. Record it through executable evidence, not schema assumptions.

This proposal rests on read-only source inspection. No files were changed and no builds, Doctor runs or test suites were executed.

### 10. The one idea to steal

Changing where knowledge lives must never silently change what loads, who owns it, what stays private or how it can be recovered.

## The Contrarian (Proposal E)

### 1. Thesis

Ship #6 as checked editing of the loader’s existing capabilities, with consistent discovery across commands. Defer #7 until users demonstrate a need that including existing files cannot satisfy, since the loader already accepts “any text file in the workspace.” The failure to prevent is turning a convenience command into a second configuration language that contradicts the loader. [Source](../../../../../src/open-forge/.agents/loader.md)

### 2. The concept and its name

The three layers describe how knowledge reaches a project, not three kinds of authority. Extensions distribute complete files through package installation. Libraries reuse source files through local projections. **Includes** expose knowledge already present at its existing path.

That distinction needs one factual qualification. Libraries currently require a source root contained in the selected workspace and project its files through relative symlinks. “On the same computer” describes the maintainer’s aspiration more broadly than today’s contract. #6 should not silently expand Library containment. [Library contract](../../../../../.agents/memory/crystallized/documents/cli/contracts/library/attach/interface.md)

Use **include** as the new word and add `route include` and `route exclude`.

“Link” is overloaded by ordinary Markdown links, relative filesystem links and the Library implementation. A new top-level `root` group would exaggerate the feature into another subsystem. `route add` is less precise because callers cannot tell whether it creates content or exposes existing content.

`route include` fits the existing routing group. It adds an existing file to the loader without creating or managing that file. `route create` creates an ordinary Markdown file beneath an existing root. `route init` creates missing entrypoints. Those effects remain distinct, with no existence-dependent change of operation. [Current route contracts](../../../../../.agents/memory/crystallized/documents/cli/contracts/route/_route.md)

The likely failure is mistaking inclusion for installation. Every plan and receipt should say that the target remains independently maintained.

### 3. The CLI surface

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global options]

open-forge route exclude <workspace-file> [--dry-run] [global options]

open-forge route list [<source-reference>] [--depth <integer|all>]
open-forge route list --includes
```

Global options retain their existing grammar, including `--workspace`, `--format`, `--detail`, `--detail-filter`, `--help` and `--version`. Include and Exclude apply directly like Route Create, with `--dry-run` for preview and no new confirmation flag.

Three examples:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow --dry-run
```

```sh
open-forge route include docs/_docs.md --description "Project documentation"
```

```sh
open-forge route include .cursor/rules/testing.mdc --description "Testing conventions" --tag LoadNow --apply-to "src/**"
```

Include writes a loader row and records its path in settings. Description defaults to the workspace-relative path. Tags default to empty, making the include on demand. Conditions use the existing workspace-relative glob grammar.

An ordinary file is a leaf. A canonical `_{folder}.md` entrypoint exposes its explicit `Entries`, recursively. An underscore alone does not make an arbitrary filename an entrypoint. Existing compatibility entrypoint names remain supported. Directories and globs are rejected with guidance to name an entrypoint or individual files.

Validate existence, strict UTF-8, Markdown row representability, conditions and lexical and physical workspace containment. Reject URLs, fragments, control files, escaping aliases and ambiguous ancestry. Existing routed Framework sources use their existing route operations.

Identical requests converge without duplicate rows. An existing handwritten row can be registered without changing its bytes. Explicit metadata that conflicts with that row produces a blocked result identifying the line to edit. Include does not acquire a second metadata-update mode.

Exclude removes the direct loader row and registration while preserving the target. It also works for an unregistered handwritten include. `route remove` retains its deletion meaning and directs external-include callers to Exclude. Deliberate root `remove <path>` deletion also drops affected registrations.

Normal Route List shows active includes and their reachable descendants. `--includes` reports direct includes with path, origin and active, inactive or missing state. Combining that projection with a source operand or depth is invalid.

Read commands accept an exact `./<workspace-file>` selector for exposed external sources. Existing source IDs retain their meaning. Listing prints a reusable exact selector rather than inventing competing IDs.

Command behavior must be consistent:

- `context` follows loader membership, loading tags and conditions, including catalogue chains. Linking grants neither acceptance nor editing authority.
- `find` searches exposed sources. Include metadata comes from Entries, with Markdown headings available for Markdown targets.
- `references` includes their incoming and outgoing supported Markdown links. Non-Markdown targets remain opaque and report that coverage limit.
- `index` preserves include rows and validates selected targets. It does not add metadata, create catalogues or regenerate Entries outside the main root.
- `status` reports include health and adds an effective startup estimate covering linked required files. Existing Framework-only measurements remain separately labelled.
- `doctor` diagnoses missing targets, invalid catalogues, unsafe paths, cycles and competing ancestry. A registered but unlisted path is inactive, not automatically broken.
- `route list` and `route inspect` expose the same graph and loading explanations used by Context.

Handwritten links and command-added rows have identical reading semantics. Registration must never become a prerequisite for discovery. Proven rename repairs maintain the corresponding registration when repairing its loader destination.

Use existing result coordinates. Invalid syntax returns exit 4, unsafe or conflicting state returns 5, incomplete inspection returns 3 and verified success returns 0.

The likely failure is accidental mutation of existing documentation. #6 therefore keeps included targets read-only for Index and route maintenance, regardless of `allowInstallPaths`.

### 4. The config

Add this to `.agents/open-forge.json` in #6:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v2/open-forge.schema.json",
  "schemaVersion": 2,
  "roots": [
    "CONTRIBUTING.md",
    "docs/_docs.md",
    ".cursor/rules/testing.mdc"
  ]
}
```

Here, `roots` means registered extra entry files, including leaf files. It does not mean recursively scanned directories. The name is less precise than `includes`, but retaining it across #6 and #7 avoids a needless configuration migration.

Paths are workspace-relative, use forward slashes and identify exact files. The schema adds a unique string array, with runtime validation for containment, physical aliases and path collisions. Missing `roots` means an empty registration list.

Keep version 2 for this additive change. The current schema explicitly allows unknown keys and says older CLIs ignore them. [Schema](../../../../../schemas/v2/open-forge.schema.json)

Conditional #7 configuration:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v3/open-forge.schema.json",
  "schemaVersion": 3,
  "mainRoot": ".forge",
  "roots": [
    "CONTRIBUTING.md",
    "docs/_docs.md",
    ".cursor/rules/testing.mdc",
    ".agents/skills/_skills.md"
  ]
}
```

`mainRoot` defaults to `.agents`. Version 3 documents the changed layout model. Preserve the v2 schema URL for existing files.

There is no duplicated loading metadata in JSON. Include computes a loader-relative destination and writes, for example:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

**The loader is authoritative for activation, ordering, descriptions, tags and conditions.** Settings track registration and, in #7, the main location.

If settings name a file that Entries omit, it stays inactive. Index and Update never restore its row. If Entries name an unregistered file, it remains active as handwritten configuration. Adding a path to JSON alone never enables it.

This follows the shipped rule that “a removed line stays removed until the user asks to restore it.” [Loader](../../../../../src/open-forge/.agents/loader.md)

The likely failure is treating config as a generator that repairs deliberate loader edits. Keeping metadata in one place prevents that conflict.

### 5. `help concepts` and the README guide

Add a root leaf command:

```text
open-forge help [<topic>] [global options]
open-forge help concepts
```

Bare Help lists available topics. `--help` continues to describe command syntax. Topic reading needs no installed workspace. JSON uses the existing result envelope with ordered topic sections. Unknown topics return a useful error and the available names.

The full Concepts outline is:

- **Start here** — Connect one useful existing document before adopting more structure.
- **Loader and Entries** — Entries choose active branches and their reading order.
- **Files and catalogues** — Leaves provide content and entrypoints expose further choices.
- **Loading and conditions** — Explain LoadNow, KeepInMind, on-demand selection and pending conditions.
- **Rules and authority** — Reading, tagging and package ownership do not establish acceptance.
- **Scopes and source references** — Explain inherited scope, stable IDs and exact workspace selectors.
- **Optional roles** — Describe Directives, Guidance, Maps, Patterns, Templates, Skills and Memory.
- **Includes, Libraries and Extensions** — Explain reuse in place, local projection and distribution.
- **Maintenance** — Distinguish indexing, updating, disabling, excluding and deleting.
- **Customization** — Explain overwrite companions and persistent omissions.
- **Without the CLI** — Follow the installed loader and edit ordinary files.
- **Build your own** — Add only the conventions and reusable knowledge your project needs.

Keep the topic’s canonical prose in a new `docs/concepts.md`, embedded into the CLI. Installed files continue to define runtime behavior.

Move terminology and CLI reference material out of `.agents/README.md`. Remove its mandatory authoring role before making it removable. Its new outline is:

- **You may remove this guide** — No runtime dependency points here, and recorded removal survives updates.
- Connect an existing document.
- Write your first project rule.
- Keep accepted knowledge where it already belongs.
- Add resumable notes when useful.
- Turn a branch off.
- Reuse local knowledge or distribute a package.
- Make your own framework.

Move the current `## Instructions` into a small, on-demand `.agents/authoring.md`. The loader requires that file only before authoring context. It is a format reference, not a shipped authoring Directive, and is excluded from routed-source indexing.

Those rules explain relative links, narrow ownership of detail, catalogue names, Axioms and Instructions, metadata forms, conditions, Entries and overwrites. They matter because malformed navigation can silently hide a required rule. Linked existing documents still need no frontmatter. Native Skills retain their own format. The shipped README already contains these distinctions. [README](../../../../../src/open-forge/.agents/README.md)

The likely failure is deleting the only offline specification. Moving the required reference first makes README removal honest.

### 6. The configurable main root (#7)

Do not make #7 a prerequisite for the first stable release. Includes already solve the principal brownfield problem without relocating content. Require evidence from users whose needs remain unsatisfied before funding a broad layout migration.

If built, keep one main writable Framework root and any number of read-only includes. Do not create several interchangeable installation roots.

Keep `.agents/open-forge.json` and `.agents/open-forge.lock.json` at fixed control paths. Otherwise the CLI must discover the file that tells it where discovery begins. This means configuring the knowledge root does not eliminate `.agents` entirely, which must be stated plainly.

The install question is:

> Where should Open Forge keep its context files?

Offer `.agents` first as recommended, followed by a custom workspace directory. Add `--main-root <workspace-directory>` for explicit setup. Unattended installation retains `.agents`.

Keep Skills at their existing native location, `.agents/skills`. A custom main loader can expose their catalogue through an include. Preserve built-in source IDs, including Skills, through the layout mapping. Do not promise that a harness discovers Skills beneath an arbitrary directory.

Generate the selected loader path into the managed `AGENTS.md` section and Claude imports. Other harnesses retain their documented entry mechanism.

Existing workspaces remain unchanged. Migration requires:

```text
open-forge install --configure --main-root <directory> --dry-run
```

The eventual apply must enumerate moved routes and assets, preserve private files and overwrites, rewrite affected supported links, and update ownership, exclusions, Library mappings and managed host sections. Leave unclassified files untouched and report them. Refuse destination collisions, nested repositories and uncertain mappings. Editing `mainRoot` alone is not migration.

Implement in ordered slices with separate ownership:

1. **Contracts and bootstrap** — Settings location, layout semantics, permissions and compatibility.
2. **Framework layout mapping** — Separate packaged logical paths from physical workspace destinations.
3. **Reading commands** — Context, discovery, references and health reporting at the new location.
4. **Navigation and route mutations** — Index, Repair and route operations with unchanged default behavior.
5. **Delivery** — Framework and Extension rendering, fixed control paths and native Skills.
6. **Migration** — Explicit move planning, revalidation, recovery and interruption reporting.
7. **Hosts, docs and release proof** — Managed handoffs, cross-platform journeys and published examples.

Each slice follows the preceding contract boundary. Separate workers can own them sequentially without sharing unresolved architecture.

Budget roughly four to six engineer-weeks, including migration and platform verification. This is an estimate, not measured implementation work. The central `SourceLogicalPath` currently validates `.agents` as part of logical identity, so changing one constant cannot solve physical layout. [Implementation](../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Identity/SourceLogicalPath.cs)

The likely failure is older writers ignoring `mainRoot`. A higher schema version cannot prevent that under today’s forgiving reader. Non-default roots require a documented minimum CLI version and version checks in automation.

### 7. Big changes to CLI usage

For #6, current users would notice:

- New `route include`, `route exclude`, `route list --includes` and `help concepts`.
- Exact `./<workspace-file>` selectors for exposed project sources.
- Included sources appearing consistently in discovery and relationship results.
- Additional include health and effective startup measurements.
- Diagnostics for external catalogue ambiguity and missing targets.
- Index explicitly preserving external files rather than completing their metadata.
- A removable README and a separate authoring reference.

Handwritten Entries remain supported. Existing package commands, presets and source IDs retain their meanings. Include defaults to on-demand reading.

For #7, opt-in users would additionally notice the install location question and flag, changed physical paths in plans and results, explicit migration, updated host imports and native Skills remaining under `.agents/skills`. Control files remain under `.agents`.

The likely failure is marketing these as entirely invisible additions. Expanded result inventories and startup reporting are observable changes, especially for JSON consumers.

### 8. Documents to update

Use these exact repository paths. Directory prefixes below identify their children without changing contract locations.

For #6:

- `README.md`, `docs/cli.md`, `docs/extensions.md` and new `docs/concepts.md`.
- `schemas/v2/open-forge.schema.json`.
- `src/open-forge/.agents/loader.md`, `README.md` and new `authoring.md`.
- Both `.agents/skills/open-forge-cli/` and `src/open-forge/.agents/skills/open-forge-cli/`, updating `SKILL.md` and `references/common.md`, `discovery.md`, `routes.md`, `packages.md`.
- Under `.agents/memory/crystallized/documents/cli/`, update `command-contract-set.md`, `shared-operation-contract.md`, `contracts/_contracts.md` and `contracts/route/_route.md`.
- Add contract folders `contracts/route/include/`, `contracts/route/exclude/` and `contracts/help/`, each with its matching underscore entrypoint, `interface.md` and `behavior.md`.
- Update `interface.md` and `behavior.md` in existing contract folders `context/`, `find/`, `references-candidate/`, `index-candidate/`, `status/`, `doctor/`, `repair/`, `remove/`, `route/list/`, `route/inspect/`, `route/remove/` and `shared/source-references/`.
- Update `technical-designs/generated-navigation.md` and the existing Context, Find and Index local technical designs where their source inventory changes.
- Under `.agents/memory/crystallized/documents/framework/`, update `architecture.md`, `routing/model.md`, `routing/paths.md`, `routing/loading.md`, `markdown/syntax.md` and `markdown/routes.md`.
- Under `.agents/memory/crystallized/documents/maintenance/`, update `helpers/dictionary.md` and `payload/agents/loader.md`, and add maintenance records for the README and authoring reference.
- Under `src/docusaurus/docs/`, update `cli/index.md`, `cli/flows.md`, `concepts/index.md`, `concepts/routing.md`, `concepts/loading-and-tags.md`, `concepts/customizing.md`, `getting-started/installation.md`, `getting-started/ten-minute-guide.md`, `getting-started/grow-your-framework.md`, `extensions/index.md`, `glossary.md` and `limits.md`.

For conditional #7, revisit that pack and additionally:

- Add `schemas/v3/open-forge.schema.json`.
- Update `src/open-forge/AGENTS.md`, `src/open-forge/CLAUDE.md` and their maintenance counterparts under `maintenance/payload/`.
- Update `cli/architecture.md`, `technical-designs/workspace-permissions.md`, `technical-designs/workspace-libraries.md`, `technical-designs/lifecycle-provenance.md` and `technical-designs/mutation-and-recovery.md` under the crystallized documents tree.
- Update affected Interface and Behavior contracts for `install`, `update`, every route operation, Extension install/update/remove and all Library operations.
- Update `framework/primitives/skills.md`, `framework/routing/scope.md`, `maintenance/payload/agents/skills.md` and `src/docusaurus/docs/concepts/core-categories.md`.

Refresh enclosing catalogue Entries whenever adding records. Preserve council records as historical evidence.

The likely failure is updating introductions while leaving command guidance stale. The shipped `references/packages.md` already describes Minimal as recommended but later says the first interactive choice is Essentials. Check runnable examples and default claims together.

### 9. Risks

The most probable regression is restoring a deleted loader row from config. Detect it with a single journey that includes a file, removes its row manually, then exercises Index, Configure, Update and Extension installation.

Next is accidental editing of included documents. Compare complete target bytes before and after maintenance, including foreign frontmatter and non-Markdown text.

Shared sources can lose inherited conditions when traversal chooses the first parent. The current linked-entry reader explicitly keeps “the first catalogue that linked it as its parent.” Reject new ambiguous inclusions and diagnose existing ambiguity before generalizing that reader. [Reader](../../../../../src/cli/operations/OpenForge.Cli.Operations/Commands/Context/Shared/Graph/ContextLinkedEntryReader.cs)

Startup figures can understate required reading. Compare Status’s effective measurement with Context’s actual required-file closure, retaining pending conditions as incomplete information.

Configurable roots can split package destinations, ownership and runtime discovery. Prove one custom-root journey through installation, inclusion, indexing, Extension update and removal before multiplying implementation changes.

Finally, the feature may save less effort than it costs. The earlier council already questioned whether a command justified three to five days of work. #6 now spans more commands, so budget roughly one to two engineer-weeks and validate that unfamiliar users benefit. [Council record](../../../../../.agents/memory/emerging/analysis/lossless-council/record.md)

These are proposed verification requirements. This investigation changed no files and ran no builds, tests or Doctor.

### 10. The one idea to steal

Config may remember an include, but only its loader row may switch it on, so no maintenance command can resurrect a choice the user removed.

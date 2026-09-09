# Open Forge

Adaptive Context Engineering (ACE) is the deliberate design of a workspace's information, relationships, authority, and retrieval paths so the right context is available at the right time and the environment evolves through use.

Open Forge is a user-owned, human-readable, file-native operating layer for Adaptive Context Engineering. It makes workspaces understandable, routes the right context and authority, enables confident autonomy, preserves continuity, and deliberately evolves how work gets done. It does this without imposing a universal methodology or requiring a proprietary runtime.

> Markdown makes Open Forge complete. The CLI makes it exceptional.

## Why Open Forge

Capable AI agents can reason well and still produce inconsistent work when important context is missing, stale, duplicated, too broad, or expensive to retrieve. Every new session may need to rediscover the same architecture, preferences, constraints, and unfinished work. The person directing the project then spends attention repeating settled direction and reviewing mistakes that should never have happened.

Many agent frameworks answer this with a large predefined methodology: more roles, prompts, documents, workflows, and required ceremonies. That can replace missing context with permanent process overhead.

Open Forge takes a smaller and more adaptable approach:

- Keep important meaning in human-readable files you control
- Give each detailed concept one authoritative source
- Connect related material with ordinary Markdown links, `descriptions`, anchors, and tags
- Load context by relevance instead of loading the whole workspace
- Preserve active work, candidate learning, accepted knowledge, and history without mixing their authority
- Let real decisions and recurring needs shape the local way of working
- Use deterministic tools to make correct navigation and maintenance cheaper

You begin with a useful foundation. What grows from it is yours.

## Start With A Conversation

Users do not need to learn Open Forge roles before benefiting from them. Describe the goal naturally, with as much or as little technical detail as you have.

The agent builds the best current model from the request and accepted workspace context. Unless deep analysis is requested, it begins conversationally with the current understanding, one recommendation, and at most one material decision frontier. Deeper rationale, alternatives, architecture, and plans remain available when wanted or needed. Technical users can provide detailed constraints without being forced through discovery again. Less technical users can decide through understandable outcomes and tradeoffs without being asked to design the implementation.

When direction becomes clear, useful results can become current documents, Decisions, Patterns, Guidance, Directives, Observations, or another appropriate source. Those files are outcomes of the conversation, not forms the user must learn to complete.

## Quick Start

The native CLI implements all 28 commands but is not released. These examples use a locally built CLI exposed as the global `open-forge` command. Follow [the development guide](docs/development.md#link-the-native-cli-locally) to build and link it locally, or [copy the Framework files](#working-without-the-cli).

Start from a Git repository and install Open Forge into the current directory:

```sh
git init
open-forge install
```

Review the installed files before trusting them:

```sh
git status
git diff
```

Commit the foundation as its own reviewable change:

```sh
git add AGENTS.md CLAUDE.md .agents
git commit -m "Install Open Forge"
```

That is enough to use Open Forge. `AGENTS.md` directs compatible agents to the workspace loader, and the loader exposes the available context `routes`.

When the CLI is available, an agent can obtain the complete effective startup context with one command:

```sh
open-forge context
```

The same contract remains readable and usable without the CLI.

## What You Receive

The base installation provides two cooperating areas:

| Area   | Responsibility                                                               |
| ------ | ---------------------------------------------------------------------------- |
| Core   | Entry, routing, authority, loading, and reusable agent-facing content roles  |
| Memory | Self-growing state for continuity, candidates, accepted records, and history |

Core and Memory ship together as the standard Framework. Extensions remain optional packages that add whole files through those same `routes`.

The installed shape is intentionally small:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/
  guidance/
  patterns/
  skills/
  templates/
  workflows/
  maps/
  memory/
    working/
    emerging/
    crystallized/
    archived/
```

These are useful defaults, not an untouchable taxonomy. Every file can be inspected, edited, scoped, replaced, or removed. Normal installation and update behavior must not silently restore defaults you deliberately removed.

## How It Works

### Enter Once, Route From There

`AGENTS.md` is the canonical workspace entry. Provider-specific files such as `CLAUDE.md` remain small bridges to canonical Framework entries instead of becoming competing policy documents. The Claude Code bridge imports both `AGENTS.md` and the loader so required baseline context enters the session without another agent-decided read step.

The loader exposes direct `routes`. Each `route` provides:

- A natural `description` that helps an agent select or skip it
- A relative Markdown link to the next file or `route`
- Tags for compact loading, type, scope, and search signals

Selection proceeds from general context to relevant detail. Unselected sibling `routes` stay outside active context.

Agents scan visible descriptions, tags, route paths, ancestor meaning, and links; recursively select the scopes that materially apply; compose their selected chains; and reevaluate after a material task change. They do not load route bodies merely to discover more selection surfaces.

### Describe Routed Files In Frontmatter

Entrypoints and indexed Markdown files use a small `open-forge` frontmatter block:

```yaml
---
open-forge:
  description: Explain when a reader should select or skip this file
  responsibility: State what this file defines when that boundary is useful
  tags: [Memory, Document]
---
```

- `description` is required for entrypoints and indexed Markdown files. It is the short selection text shown in generated `Entries`. A file loaded directly and never indexed may omit Open Forge frontmatter unless another contract requires it.
- `responsibility` is optional. Use it when one sentence adds a useful boundary beyond the description and route. Omit it when it would only repeat the file's meaning.
- `tags` classify loading, role, state, scope, or useful topics. Reserved tags keep the meanings defined by the Loader.

Frontmatter helps agents and deterministic tools navigate the file. It does not create authority by itself. See the [canonical Markdown syntax](.agents/memory/crystallized/documents/framework/markdown/syntax.md#frontmatter) for the complete current format.

### Use Source IDs Or Exact Paths

The native CLI gives every supported file under `.agents` an automatic source ID derived from its workspace-relative path. Commands accept either that ID or the exact `.agents` path. IDs are convenient; exact paths disambiguate collisions without guessing. See the [CLI documentation](docs/cli.md#source-references) for the full behavior.

```text
ID:   memory/crystallized/documents/architecture
Path: .agents/memory/crystallized/documents/architecture.md
```

The standard root `entrypoints` and their compact maps pay a small baseline cost so an agent knows which Framework roles exist. Keep that cost flat as the workspace grows:

- Put specialized material in the narrowest useful scope
- Use workspace-wide placement only for material that genuinely applies across the workspace
- Let each scoped `entrypoint` choose its own loading: on demand by default, #LoadNow when omission costs more than loading, or #KeepInMind when continuity must be revisited
- Use #KeepInMind on an ordinary record sparingly because those records are discovered across the workspace even when their ancestor branch is inactive

Scopes do not inherit #LoadNow merely because the standard roots use it. A deeply specialized scope can remain available without entering ordinary active context.

### Keep Authority Explicit

Loading makes information visible. It does not make that information authoritative by itself.

Current documents explain accepted concepts as they work now. Decisions preserve why consequential choices were accepted. Directives and `Axioms` state binding behavior. External systems remain authoritative when the workspace explicitly points to them.

The relationships stay visible and correctable because they are ordinary files and links.

### Preserve Continuity Without Premature Truth

Memory is self-growing Markdown state. It grows through useful records and routed scopes without forcing the whole store into active context. Recording remains deliberate rather than automatic.

Memory distinguishes four semantic states:

| State        | Purpose                                                              |
| ------------ | -------------------------------------------------------------------- |
| Working      | Temporary context needed to continue or resume active work           |
| Emerging     | Useful analysis, ideas, and observations that are not accepted truth |
| Crystallized | Accepted durable knowledge within its declared scope                 |
| Archived     | Useful history that no longer governs current work                   |

These states are not a mandatory pipeline. Information moves when its meaning, usefulness, and accepted authority change.

### Reuse Native Agent Capability

Open Forge assumes contemporary agents can reason, inspect files, navigate links, follow scoped authority, and use tools. It does not restate ordinary intelligence through exhaustive prompts.

Additional procedure earns its place when it makes work more reliable, safer, cheaper to reason about, available at the right time, or specific to the workspace.

### Make Deterministic Work Cheap

The CLI is a deterministic reasoning accelerator. It can:

- Load routed context in the correct order
- Find files by `route` or tag
- Show inherited `Axioms`
- Rebuild generated `entries` for `routes`
- Validate Framework structure
- Scaffold categories
- Install and remove optional Extensions through reviewable plans

Deterministic tools reduce the cost of obtaining and applying context. They do not privately own its meaning.

## Grow Your Own Framework

Open Forge is designed to evolve through use.

A recurring correction may become Guidance or a Directive. A stable shape may become a Pattern. A reusable capability may become a Skill. A repeated multi-step goal may become a Workflow. Important local or external destinations can be exposed through Map `routes`. Useful starting content can become a Template.

The shared Framework stays small while local scopes can become highly specialized.

Open Forge can organize:

- One project
- A monorepo
- Several related repositories
- A shared multi-project source of truth
- Product, engineering, design, research, planning, or operational knowledge
- Personal, team, or organization-specific ways of working

Each `route` exposed directly by the loader is a `root route` and is not scopable. Every `route` below a `root route` is scopable at any depth: routed `slugs` narrow everything that follows them. Relationships across `root routes` stay explicit through ordinary links.

The precise scaling claim is:

> Open Forge has no fixed structural expansion ceiling. Active context grows primarily with selected `route` depth, scopes, and relationships, not with the total size of the workspace.

Two projects may coexist in the same routed environment without ordinary work in one loading the other. Work integrating both projects can deliberately select both.

In less restrained language: grow your own framework and scale it toward infinity. The `route` still determines what enters the context window.

## Examples

### A Feature In One Repository

An agent working on authentication may receive:

- The accepted product direction
- The relevant architecture section
- Security directives for that scope
- The established API and test Patterns
- Current task state and prior handoff

Unrelated deployment history and another feature's analysis remain unselected.

### Several Interacting Projects

A shared Open Forge environment may route separately to a backend, web application, mobile application, and infrastructure repository.

Ordinary mobile work selects only the mobile scope and shared contracts. An integration task selects the mobile and backend scopes together. Keeping the other projects available adds negligible active-context cost until their `routes` are selected.

### Learning From Repeated Work

An agent notices a surprising failure with plausible future value and records an Emerging observation with its evidence and scope. A later agent finds the same observation and adds another occurrence.

Repeated evidence can justify a proposed Pattern, Guidance entry, Directive, current-document update, or another suitable destination. Candidate learning remains visibly candidate until accepted.

### Continuing Across Agents

Working Checkpoints keep the goal, current state, current step, unresolved questions, evidence, next steps, and durable-source links available across a context break or handoff. Create and seal a concise Handoff only when an actual transfer or explicitly planned resumption needs a stable boundary snapshot while the Checkpoint may continue to change.

The new agent resumes from explicit workspace state instead of reconstructing the task from private chat history.

## Core Content Roles

Open Forge uses inherited `Axioms` for mandatory Framework behavior and seven routed primitives for reusable content. The roles stay distinct because they have different authority and lifecycle semantics:

| Role      | What it contributes                                                 |
| --------- | ------------------------------------------------------------------- |
| `Axiom`   | Mandatory Framework behavior inherited through a loaded `route`     |
| Directive | Independently routed binding behavior                               |
| Guidance  | Adaptable judgment for recurring situations and tradeoffs           |
| Pattern   | A reusable shape that continues to guide related results            |
| Skill     | A specialized capability expressed through `SKILL.md`               |
| Template  | Copy-ready starting content whose ownership transfers to the result |
| Workflow  | A repeatable Markdown recipe for reaching a defined goal            |
| Map route | Concise navigation to important local or external sources           |

A workspace can use only the roles that provide value. Optional Extensions can add specialized content without making it part of the universal foundation.

## Extensions

Extensions install optional whole files into the ordinary route tree. Installed files remain complete runtime meaning; package manifests and ownership receipts are lifecycle metadata rather than hidden agent context.

Explore the bundled catalogue:

```sh
open-forge extension list --available
```

Inspect the toolkit:

```sh
open-forge extension inspect development-toolkit
```

Preview an installation:

```sh
open-forge extension install development-toolkit --dry-run
```

Install it:

```sh
open-forge extension install development-toolkit
```

The deliberately small current catalogue contains one `development-toolkit` package. It adds six lean Workflows, one native Experience Design Skill, and nine copy-ready Templates without changing the Framework's runtime model.

See [Extension documentation](docs/extensions.md) for package shapes, dependencies, receipts, update and removal behavior, safety boundaries, and sharing.

## CLI

Use `open-forge --help` to discover the current commands. The [CLI guide](docs/cli.md) explains their arguments, results, and safety boundaries.

```sh
open-forge context
open-forge find --tag Architecture
open-forge route inspect .agents/memory/crystallized/documents/_documents.md
open-forge doctor
```

The frozen TypeScript MVP remains in `src/cli-mvp/` as historical reference. Its commands and state formats do not define the native CLI.

## Working Without The CLI

Open Forge remains complete as human-readable Markdown.

A manual installation can copy the contents of `src/open-forge/` into a workspace. The `routes` can be followed through ordinary Markdown links, and generated `Entries` can be maintained manually when needed.

The CLI is the safer and cheaper path for repeated mechanical work because it validates routes, limits generated edits, previews lifecycle changes, and preserves review boundaries.

## Customization

The installed Framework belongs to the workspace.

You can:

- Add ordinary files beneath an existing route
- Create routed scopes for projects, products, disciplines, or repositories
- Nest any number of scopes anywhere below a `root route`
- Add `root routes` through the loader
- Remove or replace installed `root routes` that do not fit the workspace
- Add local Templates, Patterns, Guidance, Directives, Skills, and Workflows
- Use `{name}.overwrite.md` for a small local adjustment to a mostly suitable base
- Edit or replace a base file when the combined base and overwrite would become confusing

Scoping uses ordinary Markdown `routes`. Every scope `slug` has an `entrypoint` that states its local subject. Place it immediately before the first `route` segment it should narrow.

A folder named `skills` beneath Workflows does not recreate the Skills `root route` merely because the `slug` is familiar. Workflows use Skills and other Core primitives through explicit links.

`open-forge route init <route-target>` scaffolds missing entrypoints in one route chain. Complete the draft wording before relying on it.

After changing routed files, rebuild and validate navigation:

```sh
open-forge index
open-forge doctor
```

Prefer one detailed authoritative source plus visible links over maintaining several competing copies.

## Design Boundaries

Open Forge is not:

- A universal development, product, design, or organizational methodology
- A large catalogue of mandatory prompts, roles, workflows, or ceremonies
- A substitute for the judgment and responsibility of the person directing the work
- A hidden knowledge database or agent runtime
- A provider-specific orchestrator
- An automatic recorder of every conversation
- A mechanical guarantee that a nondeterministic agent will behave correctly

Explicit context and deterministic validation improve the probability of correct behavior. Consequential work still deserves proportionate review.

## Project Status

Open Forge is under active development and dogfooding.

The file-native Framework and its ACE direction are the accepted foundation. The native CLI implements all 28 commands, including Extension and Workspace Library operations, but is not released. The six-target package graph is implemented. Native execution and package invocation are proven locally on Linux x64; the other five matching-host receipts and final delivery acceptance remain incomplete. Interfaces and installed defaults may still evolve before a stable release.

Review installation and update diffs. Keep Git as the recovery and inspection boundary.

## Documentation

- [CLI](docs/cli.md)
- [Extensions](docs/extensions.md)
- [Developing Open Forge](docs/development.md)
- [Vision](.agents/memory/crystallized/documents/vision.md)
- [Principles](.agents/memory/crystallized/documents/principles.md)
- [Architecture](.agents/memory/crystallized/documents/architecture.md)
- [Framework Architecture](.agents/memory/crystallized/documents/framework/architecture.md)

## Who Uses Open Forge

Open Forge dogfoods itself: its own vision, architecture, decisions, migration state, maintenance contracts, Templates, and work continuity are managed through Open Forge.

Additional projects or organizations will be listed only when their usage is confirmed and they want to be named.

## License

[MIT](LICENSE)

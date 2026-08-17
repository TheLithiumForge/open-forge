---
open-forge:
  description: Historical frozen Open Forge CLI MVP role, command surface, deterministic state, safety model, verification boundary, proven properties, and liabilities
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, Architecture, CLI, MVP, Tooling, Legacy]
---

# Open Forge CLI MVP Architecture

## Status And Scope

The frozen Open Forge CLI MVP is the previously shipped implementation and an
explicitly invoked historical reference during replacement work. This document
defines the coherent view of:

- The CLI's role in Open Forge
- Its current commands and internal responsibilities
- Deterministic state and safety boundaries
- Proven behavior worth preserving
- MVP liabilities that should not become permanent design

The [top architecture](../architecture.md) defines the CLI's relationship to the
complete Open Forge system. The [Framework Architecture](../framework/architecture.md)
defines the human-readable route, authority, Memory, and primitive contracts that
the CLI consumes. The [Extensions MVP Architecture](../extensions/architecture.md)
defines extension package semantics and composition. This document defines how the
frozen MVP reads, validates, plans, and changes those surfaces.

The current command names, flags, file layout, and internal modules are
descriptive MVP state rather than automatically accepted replacement design. The
accepted [replacement CLI Architecture](architecture.md) and [Command Contract
Set](command-contract-set.md) govern the replacement without retroactively
describing frozen behavior.

## Role

The CLI is an optional `deterministic reasoning accelerator` and safety tool.

It makes operations over the human-readable Open Forge contract cheap, repeatable, inspectable, and mechanically verifiable. It does not interpret product intent, decide which candidate is true, execute agent reasoning, or serve as a private source for workspace meaning.

Its primary responsibilities are:

- Assemble explicit context in declared order
- Traverse and query routed relationships
- Explain inherited context
- Validate deterministic Framework contracts
- Maintain derived indexes
- Plan and apply bounded file changes
- Install and preserve the shipped Framework
- Compose and manage extension payloads
- Expose reviewable effects and actionable failures

The governing invariant is:

> Deterministic tools reduce the cost of obtaining and applying context; they are not the source of its meaning.

## Frozen Runtime And Distribution

`src/cli-mvp/` contains the frozen TypeScript source for `open-forge-old`, at
[`src/cli-mvp/cli.ts`](../../../../../src/cli-mvp/cli.ts). It remains directly
runnable through `bun run cli:old` during replacement development. The
transitional package build uses this source and the helpers under
`src/cli-mvp/build/`. Neither `src/cli-mvp/` nor `open-forge-old` is replacement
implementation or contract authority. The accepted replacement source tree is
the not-yet-present C# tree under `src/open-forge-cli/OpenForge.Cli`.

The frozen MVP's build and package facts remain historical MVP context. They do
not describe the replacement executable, solution, Native AOT output, or
distribution boundary.

The frozen source retains relative Framework and Extension payload resolution
for direct and historical invocation. The helpers under `src/cli-mvp/build/`
support the package build. An environment variable may replace the Extension
root for controlled MVP testing. The frozen repository build does not distribute
those adjacent payloads. Superseded standalone source, manifest, archive, and
checksum products are neither frozen-MVP requirements nor outputs of that
frozen build.

The implementation exposes selected pure internals only for fast regression tests. They are not a supported library API.

## Current Command Surface

The MVP has eight command families:

| Command   | Current responsibility                                                                                            |
| --------- | ----------------------------------------------------------------------------------------------------------------- |
| `install` | Install or reapply the shipped Framework and patch managed root entry blocks                                      |
| `extend`  | List, select, install, reconcile, preview, or remove extension packages                                           |
| `index`   | Rebuild the loader registry and generated `Entries` regions                                                       |
| `load`    | Emit baseline and continuity context in plain, path, or JSON form                                                 |
| `find`    | Query routed files by explicit tags or routes and optionally follow the frozen MVP's former Workflow dependencies |
| `chain`   | Explain loader-to-target inheritance and optionally extract one heading                                           |
| `doctor`  | Validate deterministic route, document-shape, and dependency contracts                                            |
| `create`  | Scaffold route categories or local extension packages                                                             |

The frozen source and the regression tests listed under [Verification](#verification)
remain the evidence for exact MVP behavior. The separate
[`docs/cli.md`](../../../../../docs/cli.md) describes the accepted non-shipping
replacement interface.

### Context Operations

`load` batches the loader, visible transitive `#LoadNow` closure, the complete broad `#KeepInMind` audit set, and adjacent overwrites. It does not enter an on-demand parent merely because an unseen descendant is tagged for immediate loading. This frozen broad audit does not implement the Framework's target-sensitive #KeepInMind contract.

`find` performs deterministic route lookup rather than semantic search. It can filter effective tags, select a route, expand generated entries to a bounded depth, and, as frozen legacy behavior, follow the former Workflow `Required Routes` section.

`chain` emits the loader, visible ancestor `entrypoints`, relevant `SKILL.md` boundary, target, and overwrite companions in inheritance order. Heading extraction makes inherited `Axioms` or another declared contract cheap to inspect.

The [overwrite contract](../framework/routing/overwrites.md) defines companion meaning and precedence. Context commands reproduce its base-then-overwrite order without making the companion independently selectable.

These operations accelerate explicit Framework routing. They do not infer accepted relevance or make the CLI's output more authoritative than its source files.

### Maintenance Operations

`index` derives `entries` for `routes` from the filesystem, `entrypoint` metadata, Skill packages, and direct-child relationships. It defines only bounded generated regions.

`doctor` detects deterministic defects without writing. The current implementation checks `entrypoint` ambiguity, generated-region integrity, broken `entries` and legacy `Required Routes`, stale indexes, `route` containment, the minimal Workflow shape, direct Directive `Instructions` shape, inherited sentinels for entrypoints, retired tags, orphan overwrites, and unreachable Markdown. An orphan overwrite is reported as a warning because it has no base from which to inherit a `route`; this does not yet enforce the Framework's fail-closed target.

`create category` scaffolds concrete route chains and rebuilds indexes. `create extension` scaffolds a local managed package.

The current Framework Workflow contract requires `Goal`, `Steps`, and `Completion`. The frozen MVP validator also accepts and validates the removed `Required Routes` section as legacy behavior. The replacement CLI will not implement that compatibility surface. Recipe-specific headings remain outside CLI schema.

### Change Operations

`install` currently copies the shipped Framework on first installation, patches bounded `AGENTS.md` and provider-bridge blocks, reconciles `entrypoint` files whose `managed route` shapes are recognized through scopes, and rebuilds indexes.

`extend` currently combines catalogue presentation, interactive selection, dependency resolution, payload installation, reconciliation, dry-run, ownership tracking, and removal.

Normal writes use target-scoped Git checkpoints. `--pro` bypasses selected Git and Core lifecycle guards while retaining path, dependency, ownership, collision, and transaction safety.

## Current Internal Responsibilities

The single implementation module currently contains all of these architectural areas:

1. Process entry and argument parsing
2. Human help and terminal interaction
3. Framework source and package discovery
4. Git repository inspection and review checkpoints
5. Filesystem identity, containment, and portability checks
6. Markdown frontmatter, link, heading, and generated-region parsing
7. Route discovery, tag resolution, and inheritance
8. Context assembly and query output
9. Framework and extension validation
10. Category and package scaffolding
11. Framework installation planning and application
12. Extension catalogue and manifest parsing
13. Dependency selection and closure resolution
14. Extension ownership receipts
15. File, index, and receipt transactions with rollback

This concentration made the MVP fast to evolve and allowed safety behavior to share one implementation. It now makes unrelated changes expensive to reason about, encourages command-specific special cases, and prevents the architecture from expressing stable domain boundaries.

## Deterministic State

The CLI interacts with three kinds of state:

### Authored State

Authored Markdown, extension payloads, root harness content outside managed markers, and declared external sources are authoritative for their meaning.

The CLI may preserve, copy, validate, or patch explicitly managed boundaries. It does not replace authored meaning with an internal representation.

### Derived State

Generated `Entries`, packaged manifests, checksums, and caches are reconstructable from authoritative authored sources. Their loss may reduce convenience but does not change the semantic contract.

### Managed Lifecycle State

`open-forge.extensions.json` records extension identities, dependencies, owned paths, digests, and shared owner sets. It is transparent install metadata required for safe managed update and removal.

The receipt does not participate in agent routing or runtime meaning. Deleting it loses lifecycle knowledge but does not change what installed files say.

## Safety Model

The current MVP has a comparatively mature safety boundary that should survive the redesign.

### Plan Before Mutation

Framework and extension writes are computed and preflighted before application. Extension dry-run exposes planned create, update, delete, and unchanged effects, dependency order, and route-scope classifications.

Core installation also plans its writes, but the MVP does not expose an equivalent complete public preview surface.

### Containment And Identity

Deterministic operations reject:

- Absolute or parent-traversing route arguments
- Drive changes and lexical escapes
- Symlink or junction escapes
- Linked source or target roots
- Unsafe hard-linked managed targets
- Portable case, Unicode, Windows-device, and path-segment aliases
- File and parent-directory collisions
- Extension writes to Git control paths, receipts, or workspace-owned overwrites

The CLI distinguishes lexical path identity, physical target identity, portable cross-platform identity, and route identity because no single representation is sufficient for safe mutation.

### Ownership

Managed extension writes require a stable owner and verified prior bytes. A managed package cannot silently adopt an unowned file even when its bytes match. Shared files require compatible owner sets and identical content.

Core installation cannot overwrite a receipt-owned path. Removal affects only explicitly requested extension identities and refuses to strand retained dependencies or routed descendants.

### Review And Recovery

Normal installation requires a clean, Git-visible target scope and ends with a review checkpoint. Read-only catalogue and preview operations remain available without Git.

Payload, generated-index, and receipt writes roll back together after handled in-process failures. Git is the durable recovery boundary.

The current MVP has no persistent crash journal. An abrupt process or machine failure may require Git-backed recovery.

## Output And Automation

Human-readable output is the default.

`load`, `find`, `chain`, and `doctor` expose JSON modes. Write commands expose textual plans and results but do not share one stable machine-readable plan schema.

Commands use non-zero exit status for errors. Failures generally identify the violated boundary through a direct message, but error codes and structured remediation are not yet a stable public interface.

Interactive extension selection is optional and requires a terminal. Deterministic non-interactive commands remain available for scripts and automation.

## Verification

The current test architecture has two tiers:

- Fast unit tests exercise pure selection, receipt, path, Markdown, and validation contracts
- Closure tests execute real CLI subprocesses against OS-temporary workspaces and real Git repositories

Closure coverage includes indexing, installation, extension composition, dependency graphs, collisions, containment, ownership, rollback, packaged layouts, context loading, route queries, inheritance, validation, scaffolding, and first-party catalogue integration.

The primary current test sources are:

- [`cli.unit.test.ts`](../../../../../src/cli-mvp/cli.unit.test.ts)
- [`cli.closure.test.ts`](../../../../../src/cli-mvp/cli.closure.test.ts)
- [`extension-safety.closure.test.ts`](../../../../../src/cli-mvp/extension-safety.closure.test.ts)
- [`extensions.integration.closure.test.ts`](../../../../../src/cli-mvp/extensions.integration.closure.test.ts)
- [`packaged-layouts.closure.test.ts`](../../../../../src/cli-mvp/packaged-layouts.closure.test.ts)

Tests demonstrate mechanical behavior. They do not prove that agents will interpret or follow the context emitted by the CLI.

## Proven Properties

The current MVP demonstrates these properties:

- Human-readable files remain semantically complete
- Route lookup and context assembly remain deterministic and provenance-visible
- Plain-file manual operations remain possible
- Every mutation has a bounded target
- Planning and validation happen before writes
- User-owned content is preserved outside explicit managed boundaries
- Managed ownership is transparent and Git-visible
- Portable collisions and physical escapes fail closed
- Git diffs remain the durable review and recovery surface
- Derived indexes remain rebuildable
- Machine-readable output remains available
- Fast pure tests and real closure tests remain separate

## MVP Liabilities

The current implementation should not be extended indefinitely in its present form.

### Monolithic Implementation

One roughly 4,500-line module contains every command, parser, policy, adapter, transaction, and extension concern. This makes the blast radius of a small change difficult to predict and encourages tests to reach through a frozen internal export rather than a designed application boundary.

### Conflated Lifecycle Intent

`install` currently covers first installation, idempotent updates to managed files that remain present, bounded `AGENTS.md` and provider-bridge patching, and managed `entrypoint` reconciliation through scopes. It preserves deliberately absent shipped files during an ordinary reinstall.

Those intents have different preservation and authority semantics:

- First installation introduces the Framework
- Completion adds deliberately missing material
- Upgrade applies accepted distribution changes
- Restoration intentionally returns selected files to distribution defaults
- Replacement discards local divergence

The current MVP has no explicit completion, restoration, or replacement operation. The future interface must distinguish those intents before mutation. Git recoverability is not permission to overwrite customized files.

### Overloaded Extension Command

`extend` combines catalogue listing, interactive selection, direct source installation, dependency installation, update, removal, preview, and expert bypass. This was convenient for an MVP but obscures the operation being planned.

### Uneven Preview And Machine Contracts

Extension dry-run does not currently include generated-index body diffs in its reported plan. Core installation lacks a matching public dry-run. JSON support and output schemas vary by command.

### Hardcoded Migrating Contracts

The CLI embeds:

- The minimum Workflow headings and the MVP's former optional dependency section
- Directive-shape rules
- Root harness filenames and managed markers
- A source-derived `managed route` catalogue plus path-based recognition through scopes
- Retired tag knowledge

Some deterministic schema is necessary. It must derive from accepted versioned Framework contracts or a small explicit compatibility layer, not from scattered constants that accidentally freeze old governance.

### Metadata And Overwrite Integrity

The Framework requires semantically complete authored frontmatter and generated Entries, and valid visible base-overwrite pairs that fail closed when their meaning cannot be established. The frozen MVP does not yet enforce that target: it fabricates `No description` and default tags when metadata is absent, and it reports orphan overwrites as warnings. These are temporary MVP liabilities, not compatibility guarantees or weaker Framework semantics.

### Partial Recursive Support

The router and indexer support arbitrary nested `entrypoints`. Managed reconciliation derives its `route` catalogue from the shipped source tree, requires an `entrypoint` in every folder of the candidate chain, keeps matches beneath the same `root route`, and accepts consecutive scope `slugs` between source-defined `route` segments. Descendants after the final managed segment remain user-owned `routes`.

Recognition of a `managed route` is still path-based and requires the canonical
`_{folder-name}.md` `entrypoint`. It cannot distinguish a neutral scope whose
`slug` matches a shipped `route`, a deliberately renamed `route`, or another
`route` that intentionally adopts the same contract. Generic validation,
scaffolding, and native Skill indexing use explicit `entrypoint` metadata instead
of granting behavior from a familiar root `slug` alone. The
[frozen source](../../../../../src/cli-mvp/cli.ts) defines that historical
behavior. The replacement needs an explicit human-readable way to distinguish
manager-recognized `route` segments from ordinary scope `slugs` instead of
adding more path-shape exceptions.

### Command Vocabulary

Names such as `find`, `extend`, and `--pro` emerged incrementally. The overhaul should choose verbs and flags from clear user intent rather than preserve familiarity with an unpublished MVP.

### Recovery And Concurrency

Rollback is in-process and no persistent journal coordinates abrupt recovery. The MVP also has no explicit workspace mutation lock or concurrent-plan conflict model.

### Authoring Help Is Not Exposed

The [Open Forge Markdown scope](../framework/markdown/_markdown.md) now separates canonical syntax, shared routed representation, and compatibility input. The MVP CLI does not yet expose those contracts through dedicated syntax help or derive every parser constant from a versioned schema. It accepts selected legacy equivalents internally, while the current public help does not explain the boundary between canonical, compatibility, and unsupported syntax.

### Limited Artifact Scaffolding

`create` scaffolds categories and local extension packages but not ordinary routed documents, decisions, ideas, status records, or other template-based artifacts. Agents and users must currently assemble metadata, placement, and route maintenance themselves.

## Replacement Relationship

This MVP remains a frozen reference while the replacement is implemented
directly. The accepted [replacement Architecture](architecture.md) defines the
replacement structure. The current [Command Contract Set](command-contract-set.md)
defines the replacement command-contract roles and links to its detailed
Interface, Behavior, and optional Technical Design contracts. Those sources do
not retroactively describe the shipped MVP.

## Boundaries And Non-Goals

The CLI is not:

- An agent runtime or scheduler
- A replacement for native agent reasoning
- A semantic search authority
- A private database of workspace truth
- An automatic approver of candidate Memory
- A workflow execution engine
- A substitute for Git or ordinary file inspection
- The authoritative source of extension runtime meaning

Semantic relevance, remote registries, provider orchestration, and Rune integration remain separate capabilities. If introduced, they consume the same explicit Framework sources.

## Related Current Views

- [Open Forge architecture](../architecture.md)
- [Framework Architecture](../framework/architecture.md)
- [Extensions MVP Architecture](../extensions/architecture.md)
- [Replacement CLI Architecture](architecture.md)
- [Replacement Command Contract Set](command-contract-set.md)
- [Replacement Shared Operation Contract](shared-operation-contract.md)
- [Replacement detailed contracts](contracts/_contracts.md)
- [Replacement CLI public contract](../../../../../docs/cli.md)
- [Frozen MVP source](../../../../../src/cli-mvp/cli.ts)

## Historical Context

These archived records preserve earlier direction and implementation rationale. They may inform future redesign, but they do not govern the current CLI architecture:

- [Earlier scalability and CLI experience exploration](../../../archived/ideas/2026-06-22_scalability-and-cli-experience.md)
- [Earlier CLI design exploration](../../../archived/cli-v2/ideas/cli-design.md)

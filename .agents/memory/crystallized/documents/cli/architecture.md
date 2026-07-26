---
open-forge:
    description: Current Open Forge CLI MVP architecture, retained invariants, liabilities, overhaul requirements, and verification boundary
    tags: [Memory, Document, CurrentTruth, Evergreen, Architecture, CLI, MVP, Tooling, ACE]
---

# Open Forge CLI MVP Architecture

## Status And Scope

The Open Forge CLI is a dogfooded MVP scheduled for an architectural overhaul. This document owns the coherent current view of:

- The CLI's role in Open Forge
- Its current commands and internal responsibilities
- Deterministic state and safety boundaries
- Proven behavior worth preserving
- MVP liabilities that should not become permanent design
- Requirements and conceptual boundaries for the overhaul

The [top architecture](../architecture.md) owns the CLI's relationship to the complete Open Forge system. The [Framework Architecture](../framework/architecture.md) owns the human-readable route, authority, Memory, and primitive contracts that the CLI consumes. The [Extensions MVP Architecture](../extensions/architecture.md) owns extension package semantics and composition. This document owns how the current CLI reads, validates, plans, and changes those surfaces.

The current command names, flags, file layout, and internal modules are descriptive MVP state rather than automatically accepted final design.

## Role

The CLI is an optional `deterministic reasoning accelerator` and safety tool.

It makes operations over the human-readable Open Forge contract cheap, repeatable, inspectable, and mechanically verifiable. It does not interpret product intent, decide which candidate is true, execute agent reasoning, or privately own workspace meaning.

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

> Deterministic tools reduce the cost of obtaining and applying context; they do not privately own its meaning.

## Current Runtime And Distribution

The current implementation is one TypeScript module at [`src/cli/cli.ts`](../../../../../src/cli/cli.ts). Bun builds it as an ECMAScript module targeting Node.js, and the package exposes it as the `open-forge` binary. Node.js 18 or later is the distributed runtime.

[`build.ts`](../../../../../build.ts) currently produces:

- `dist/cli.mjs`
- A standalone copy of `src/open-forge/`
- A bundled extension catalogue
- A source manifest with file hashes
- A compressed source archive and checksum

The CLI locates the Framework payload and bundled extension catalogue relative to its source or built location. An environment variable may replace the bundled extension root for controlled testing or packaging.

The implementation exposes selected pure internals only for fast regression tests. They are not a supported library API.

## Current Command Surface

The MVP has eight command families:

| Command   | Current responsibility                                                                                                                                                                                                              |
| --------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `install` | Install or reapply the shipped Framework and patch managed root entry blocks                                                                                                                                                        |
| `extend`  | List, select, install, reconcile, preview, or remove extension packages                                                                                                                                                             |
| `index`   | Rebuild the loader registry and generated `Entries` regions                                                                                                                                                                         |
| `load`    | Emit baseline and continuity context in plain, path, or JSON form                                                                                                                                                                   |
| `find`    | Query routed files by explicit tags or routes and optionally follow required routes                                                                                                                                                 |
| `chain`   | Explain loader-to-target inheritance and optionally extract one heading                                                                                                                                                             |
| `doctor`  | Validate deterministic route, document-shape, and dependency contracts                                                                                                                                                              |
| `create`  | Scaffold route categories or local extension packages - this should be extended to normal files as well like scaffold --crystalized/decision name.md aka, easy way to add the frontmatter, or maybe just create path.md we will see |

The full current user contract is documented in [`docs/cli.md`](../../../../../docs/cli.md). Exact behavior remains grounded in the implementation and its tests.

### Context Operations

`load` batches the loader, visible transitive `#LoadNow` closure, complete `#KeepInMind` set, and adjacent overwrites. It does not enter an on-demand parent merely because an unseen descendant is tagged for immediate loading.

`find` performs deterministic route lookup rather than semantic search. It can filter effective tags, select a route, expand generated entries to a bounded depth, and follow explicit Required Routes.

`chain` emits the loader, visible ancestor entrypoints, relevant `SKILL.md` boundary, target, and overwrite companions in inheritance order. Heading extraction makes inherited Axioms or another declared contract cheap to inspect.

These operations accelerate explicit Framework routing. They do not infer accepted relevance or make the CLI's output more authoritative than its source files.

### Maintenance Operations

`index` derives route entries from the filesystem, entrypoint metadata, skill packages, and direct child relationships. It owns only bounded generated regions.

`doctor` detects deterministic defects without writing. The current implementation checks entrypoint ambiguity, generated-region integrity, broken entries and Required Routes, stale indexes, route containment, workflow shape, directive shape, inherited sentinels, retired tags, orphan overwrites, and unreachable Markdown.

`create category` scaffolds concrete route chains and rebuilds indexes. `create extension` scaffolds a local managed package.

Some validations encode schemas currently under migration, particularly the exact workflow section order and fixed phase vocabulary. Those checks describe the MVP contract and must follow the accepted Framework source during the overhaul rather than preserving old schemas by inertia.

### Change Operations

`install` currently copies the shipped Framework, patches bounded `AGENTS.md` and provider-bridge blocks, updates recognized scoped Framework entrypoints, and rebuilds indexes.

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

Authored Markdown, extension payloads, root harness content outside managed markers, and declared external sources own meaning.

The CLI may preserve, copy, validate, or patch explicitly managed boundaries. It does not replace authored meaning with an internal representation.

### Derived State

Generated `Entries`, packaged manifests, checksums, and caches are reconstructable from authored owners. Their loss may reduce convenience but does not change the semantic contract.

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

The primary current test owners are:

- [`cli.unit.test.ts`](../../../../../src/cli/cli.unit.test.ts)
- [`cli.closure.test.ts`](../../../../../src/cli/cli.closure.test.ts)
- [`extension-safety.closure.test.ts`](../../../../../src/cli/extension-safety.closure.test.ts)
- [`extensions.integration.closure.test.ts`](../../../../../src/cli/extensions.integration.closure.test.ts)
- [`packaged-layouts.closure.test.ts`](../../../../../src/cli/packaged-layouts.closure.test.ts)

Tests demonstrate mechanical behavior. They do not prove that agents will interpret or follow the context emitted by the CLI.

## Strengths To Preserve

The overhaul should preserve these proven properties unless stronger evidence supports a replacement:

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

One 4,464-line module owns every command, parser, policy, adapter, transaction, and extension concern. This makes the blast radius of a small change difficult to predict and encourages tests to reach through a frozen internal export rather than a designed application boundary.

### Conflated Lifecycle Intent

`install` currently covers first installation, idempotent reapplication, Framework replacement, managed root patching, and scoped Framework updates.

Those intents have different preservation and authority semantics:

- First installation introduces the Framework
- Completion adds deliberately missing material
- Upgrade applies accepted distribution changes
- Restoration intentionally returns selected files to distribution defaults
- Replacement discards local divergence

The future interface must distinguish them before mutation. Git recoverability is not permission to overwrite customized files.

### Overloaded Extension Command

`extend` combines catalogue listing, interactive selection, direct source installation, dependency installation, update, removal, preview, and expert bypass. This was convenient for an MVP but obscures the operation being planned.

### Uneven Preview And Machine Contracts

Extension dry-run does not currently include generated-index body diffs in its reported plan. Core installation lacks a matching public dry-run. JSON support and output schemas vary by command.

### Hardcoded Migrating Contracts

The CLI embeds:

- Exact workflow headings and phase tags
- Directive-shape rules
- Root harness filenames and managed markers
- Known Framework route templates
- Selected scoped-Framework path shapes
- Retired tag knowledge

Some deterministic schema is necessary. It must derive from accepted versioned Framework contracts or a small explicit compatibility layer, not from scattered constants that accidentally freeze old governance.

### Partial Recursive Support

The router and indexer support arbitrary nested entrypoints, but scoped Framework updating recognizes a limited set of hardcoded shapes. This does not yet fulfill the Framework's recursive composition promise.

### Command Vocabulary

Names such as `find`, `extend`, and `--pro` emerged incrementally. The overhaul should choose verbs and flags from clear user intent rather than preserve familiarity with an unpublished MVP.

### Recovery And Concurrency

Rollback is in-process and no persistent journal coordinates abrupt recovery. The MVP also has no explicit workspace mutation lock or concurrent-plan conflict model.

## Overhaul Architecture

The overhaul should be organized around one deterministic planning core used by thin command and presentation surfaces.

Conceptually:

```text
Command and machine interfaces
  -> application operations
    -> plans and reviewed effects
      -> Framework and extension domain services
        -> filesystem, Git, package, and terminal adapters
```

### Interface Layer

The interface layer parses user intent, chooses human or structured presentation, and maps stable operation results to exit status. It contains no route, ownership, or transaction policy.

Interactive flows compose the same application operations exposed non-interactively. A wizard cannot become a separate behavior path.

### Application Operations

Each operation represents one clear intent, such as:

- Inspect effective context
- Query explicit routes and relationships
- Validate a workspace
- Rebuild derived navigation
- Initialize the Framework
- Complete missing selected defaults
- Preview and apply an upgrade
- Restore selected distribution files
- Create a scope or routed artifact
- Install, update, or remove extensions

Final command names remain a later interface decision. The semantic distinction among these operations is architectural.

### Domain Services

Pure or mostly pure services should own:

- Markdown metadata and structural parsing
- Route graph construction and resolution
- Context loading and inheritance
- Framework contract validation
- Portable path and ownership identity
- Distribution comparison
- Extension dependency resolution
- Change planning

These services return values and findings without printing, prompting, or mutating the filesystem.

### Plans

Every write operation should produce one inspectable plan before application.

A common plan identifies:

- Operation and target
- Source and declared owner
- Preconditions
- Create, update, delete, preserve, and unchanged effects
- Authored, managed, and derived boundaries
- Reasons for each effect
- Conflicts and required decisions
- Safety and review requirements
- Expected post-application verification

The same plan drives preview, human presentation, JSON output, application, and tests.

### Application And Transactions

The application layer revalidates plan assumptions immediately before mutation, applies effects in a recoverable order, and verifies the resulting state.

Git remains the preferred durable checkpoint. A future persistent journal or lock is justified only if it closes a demonstrated recovery or concurrency gap without becoming hidden semantic state.

### Adapters

Filesystem, Git, distribution discovery, terminal interaction, clocks, hashing, and packaging belong behind narrow adapters. This keeps platform and process behavior testable without contaminating domain rules.

## Preservation-First Lifecycle

The overhaul must assume that existing files may be intentionally customized.

Default behavior should:

- Preserve existing authored files
- Report divergence from distribution
- Add missing material only under an explicit completion intent
- Preview upgrades before replacement
- Require explicit scope for restoration
- Preserve user-owned overwrites and local routes
- Keep generated regions separate from authored content

Forceful behavior must describe exactly which preservation boundary it crosses. A generic expert flag must not stand in for several materially different actions.

## Structured Interface Contract

Every read and write operation should have a stable structured result suitable for scripts and future tools.

A versioned structured response should separate:

- Data
- Findings
- Warnings
- Conflicts
- Planned or applied effects
- Provenance
- Suggested next actions

Human output should be rendered from the same result rather than maintained as an independent implementation.

This does not require a long-lived daemon or public network API. A deterministic command process remains sufficient.

## Boundaries And Non-Goals

The CLI is not:

- An agent runtime or scheduler
- A replacement for native agent reasoning
- A semantic search authority
- A private database of workspace truth
- An automatic approver of candidate Memory
- A workflow execution engine
- A substitute for Git or ordinary file inspection
- The owner of extension runtime meaning

Semantic relevance, remote registries, provider orchestration, and Rune integration remain separate capabilities. If introduced, they consume the same explicit Framework owners.

## Migration Approach

The overhaul should begin only after the Framework and extension contracts are accepted.

Migration should:

1. Classify current tests as invariant protection, MVP compatibility, or obsolete-schema coverage
2. Extract pure domain behavior from the monolith without changing public behavior unnecessarily
3. Establish the common plan and structured result contracts
4. Separate read-only context operations from mutation operations
5. Split installation intents before changing overwrite behavior
6. Move extension-specific semantics behind the Extensions boundary
7. Replace scattered Framework constants with explicit owned contracts
8. Preserve closure tests around real filesystem, Git, package, and process boundaries
9. Remove MVP commands or flags only through an intentional migration surface

Compatibility is valuable only when it preserves a desired user contract. An unpublished MVP shape should not constrain the final architecture.

## Related Current Views

- [Open Forge architecture](../architecture.md)
- [Framework Architecture](../framework/architecture.md)
- [Extensions MVP Architecture](../extensions/architecture.md)
- [Current CLI user contract](../../../../../docs/cli.md)
- [Current CLI implementation](../../../../../src/cli/cli.ts)

## Migration Inputs

These files preserve earlier direction or implementation rationale and remain subject to reconciliation:

- [CLI overhaul candidate](../../../emerging/ideas/cli-overhaul.md)
- [Earlier extensions and CLI decision](../../decisions/extensions-and-cli.md)
- [Earlier scalability and CLI experience exploration](../../../archived/ideas/2026-06-22_scalability-and-cli-experience.md)
- [Earlier CLI design exploration](../../../archived/ideas/cli-design.md)

---
open-forge:
  description: Historical replacement CLI exploration containing interim choices later superseded or extracted into accepted current sources
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Overhaul

Archived after its durable outcomes moved to the current [CLI
Architecture](../../crystallized/documents/cli/architecture.md), [CLI
Interface](../../crystallized/documents/cli/interface.md), focused contracts,
and Decisions. This record preserves historical design context and does not
govern implementation.

## Historical Snapshot Boundary

Statements below that call Clerc, wrapper dispatch, or another intermediate
choice "accepted" describe the state of this exploration when written. They
were later superseded by the accepted Commander command boundary and direct
replacement development model. Current Crystallized sources govern every
implementation choice; this snapshot is not a second specification.

The current CLI is a dogfooded MVP and a source of behavioral evidence, not the final product architecture. Reassess and recreate it after the vision, architecture, and installable source contracts converge instead of extending its present structure indefinitely.

The [CLI MVP Architecture](../../crystallized/documents/cli/mvp-architecture.md) owns proven shipped behavior, safety boundaries, and liabilities.

## Accepted Objectives

- Treat any capable agent as the primary CLI user while keeping people first-class users for less frequent but higher-risk workspace lifecycle operations. Scripts and integrations consume the same application contracts.
- Optimize framework operations for lower agent tool-call, token, latency, retry, and avoidable-error cost without making the CLI provider-specific.
- Make correct framework behavior the cheapest path while keeping human-readable Markdown semantically complete without the CLI.
- Treat the CLI as a first-class `deterministic reasoning accelerator`: resolve explicit routes and inheritance, return ordered context with provenance, validate structure, and perform safe mechanical changes without privately owning meaning.
- Keep routing and helper behavior effective across deeply nested scopes, projects, repositories, and shared sources of truth, subject only to unavoidable context and resource costs.
- Preserve proven safety properties such as reviewable diffs, containment, collision detection, ownership checks, preview, and rollback without treating the current command or implementation shape as permanent.
- Replace the current monolithic MVP structure with an implementation whose command boundaries, contracts, and tests remain easy to evolve.
- Make every mutation start from one inspectable plan used by preview, application, structured output, and verification.
- Keep interactive wizards as presentations over the same deterministic operations available non-interactively.

The [agent-first CLI product contract](../../crystallized/decisions/cli/cli-agent-first-product-contract.md) is authoritative for the accepted audience, purpose, interface consequences, and rationale. This file is historical context only.

## Candidate Architecture

The overhaul should center on one deterministic planning core used by thin human and machine interfaces:

```text
Human and machine interfaces
  -> application operations
    -> inspectable plans and results
      -> Framework and Extensions domain services
        -> behavior-earned external capabilities
```

### Interfaces

Interfaces parse explicit intent, choose human or structured presentation, and map operation results to exit status. They do not own route, lifecycle, ownership, or transaction policy.

Interactive flows call the same application operations as scripts. A wizard must not become a second implementation path with different safety or lifecycle behavior.

The accepted [guided-operation Pattern](../../../patterns/open-forge/cli/commands/guided-operation.md) keeps interactive input, documented defaults, `--yes`, planning, application, verification, cancellation, and JSON automation over the same typed request and result contracts.

The formerly accepted [parallel entrypoint migration](../analysis/2026-07-31_cli-parallel-entrypoint-migration.md) placed the frozen old main and replacement main behind one executable wrapper. That direction is historical.

### Command And Result Libraries

The accepted [command-framework decision](../../crystallized/decisions/cli/cli-command-framework.md) selects [`@clerc/core`](https://github.com/clercjs/clerc) plus its focused completion plugin. It is ESM-only, strongly typed, modular, and supports command, flag, parameter, and dynamic-value completion for Bash, Zsh, Fish, and PowerShell.

A local spike ran the same command and completion protocol under Node, Bun, and Deno. Gunshi remains the strongest runner-up because its declarative command definition, separate runner, composition, and lazy-loading model fit well, but its completion plugin currently excludes Deno. Stricli is no longer preferred because its first-party completion installer is Bash-only and its runtime contract is Node-oriented.

Installation waits for replacement implementation rather than changing the frozen MVP during discussion. Implementation must verify precise cross-file type inference, wrapper and distribution compatibility, and accepted output and exit semantics. Lazy loading is not required at the expected command count; a focused handler can dynamically import its implementation later if startup measurements establish a real need.

The command library would own only command routing, argument parsing, help metadata, completion metadata, and invocation. It would not own application operations, domain results, filesystem access, rendering policy, or transactions.

Clerc validates flags and positional parameters and passes inferred values to independently exported handlers. Do not revalidate those inputs with Zod. The accepted [runtime boundary validation decision](../../crystallized/decisions/cli/cli-runtime-boundary-validation.md) starts other untrusted inputs with focused parsers or validators and introduces a schema library only when representative code proves that it materially improves readability, duplication, or structured-contract support.

During migration, the one executable wrapper delegates to the frozen old or replacement main. Each replacement command definition routes to a focused exported handler that can be called directly in tests. One handler file per command is the default shape, while final module boundaries remain subject to the later architecture discussion. After cutover, the entrypoint calls only the replacement main and the wrapper disappears.

### Source Locality And Dependency Direction

The accepted [CLI source-locality decision](../../crystallized/decisions/cli/cli-source-locality.md) organizes the replacement by behavior and scope instead of reproducing every technical layer as a distant top-level tree.

`src/cli/cli.ts` remains the executable entrypoint. It delegates to `main.ts` or `cli-old/main.ts` during migration. Replacement commands live under `commands/{command}/{command}.ts` with their direct tests and focused support. A simple command module exports `commandName`, its Clerc definition, and a named handler. It splits further inside the same folder only when responsibility or file size warrants it.

Commands depend on application and domain capabilities, never on sibling command internals. Feature-specific operations, parsers, renderers, ports, adapters, fixtures, and tests remain local. Reused code moves only to the nearest common ancestor of its actual consumers. CLI-wide plans, results, transaction machinery, and platform services earn broader placement because several capabilities use them.

The dependency direction remains:

```text
cli.ts -> replacement or frozen main
replacement main -> commands + concrete runtime services
commands -> application operations + renderers
application operations -> domain behavior + narrow ports
concrete adapters -> port contracts
```

This is a dependency rule, not a requirement for matching global layer folders. Pure behavior remains functional. Stateful lifecycle and resource boundaries may use classes where that shape is clearer. The replacement uses direct composition in `main.ts` rather than a dependency-injection framework.

### Application Operations

The accepted [CLI core job model](../../crystallized/decisions/cli/cli-core-job-model.md) defines five product jobs: orient, retrieve and explain, create and maintain, validate and repair, and manage lifecycle. Planning, structured results, provenance, verification, transactions, rollback, and recovery remain cross-cutting contracts.

Each operation represents one clear intent within those jobs. Candidate operations include:

- Inspect effective context
- Query explicit routes and relationships
- Validate a workspace
- Rebuild derived navigation
- Initialize the Framework
- Complete selected missing defaults
- Preview and apply an upgrade
- Restore selected distribution files
- Create a scope or routed artifact
- Install, update, or remove Extensions

Final command names and flags remain interface decisions. The semantic distinctions among operations are architectural.

### Domain Services

Pure or mostly pure services should own:

- Markdown metadata and structural parsing
- Route graph construction and resolution
- A complete versioned catalogue of the `routes` shipped to users, their manager-recognized shapes through scopes, and the files each lifecycle operation may change
- Recursive recognition of shipped `routes` through an explicit human-readable distinction between manager-recognized `route` segments and ordinary scope `slugs` rather than familiar names or selected hardcoded path shapes
- Generic indexing, navigation, inheritance, and structural validation for roots and scopes without granting behavior from familiar names
- Context loading and inheritance
- Framework contract validation
- Portable path and ownership identity
- Distribution comparison
- Extension dependency resolution
- Change planning

These services return values and findings without printing, prompting, or mutating the filesystem.

### Plans, Application, And Adapters

Every write operation should produce a common plan that identifies:

- Operation, target, source, and declared lifecycle authority
- Preconditions and expected post-application verification
- Create, update, delete, preserve, and unchanged effects
- Authored, managed, and derived boundaries
- Reasons, provenance, conflicts, and required decisions
- Safety and review requirements

Application revalidates assumptions immediately before mutation, applies effects in a recoverable order, and verifies the result. Git remains the preferred durable checkpoint. A persistent journal or workspace lock is justified only if evidence shows that it closes a real recovery or concurrency gap.

Introduce a narrow external capability only when representative behavior needs explicit ownership, failure translation, lifecycle, or replacement. Filesystem behavior is demonstrated. Git may earn a focused checkpoint capability, and a process runner may support it. Do not pre-create package, clock, terminal, or other adapters merely because the environment contains those concepts.

## Main Architecture Work Still In Scope

Testing architecture is a cross-cutting constraint on the replacement, not a substitute for the main CLI architecture. Before implementation, the design still needs coherent decisions for:

- Executable topology, old/new dispatch, startup cost, and legacy retirement
- Interface parsing, human rendering, structured results, exit semantics, and versioning
- Application-operation boundaries, orchestration, planning, verification, and failure containment
- Domain modules for workspaces, routing, references, context, health, repair, lifecycle, Extensions, and ownership
- Dependency direction and the behavior-earned ports through which domain and application code reach real external capabilities
- State, caching, concurrency, locking, journals, and invalidation only where measured needs justify them
- Runtime and package layout, Node compatibility, Bun development use, cross-platform behavior, and distribution discovery
- Performance budgets, observability, testing, migration slices, and eventual legacy deletion

The current sequence is to establish the testing model early enough that it can shape public seams and failure injection, then return to module and operation boundaries before building the first replacement slice.

## Candidate First Useful Replacement Boundary

Build the replacement as a vertical slice behind the accepted single-entrypoint wrapper. Do not publish a second binary or wait for a big-bang rewrite. Route each proven operation to the new application core only after its contract and verification evidence are ready.

The first useful slice should let an agent work confidently inside an already-installed workspace:

- Discover the workspace, installation state, applicable scope, and Framework version or provenance that can be established mechanically
- Parse canonical Framework metadata and construct the route graph
- Assemble baseline and continuity context in declared order
- Query explicit routes and relationships
- Explain inheritance, dependencies, and provenance
- Run complete read-only health diagnostics
- Repair at least stale derived navigation through the explicit repair path
- Return every operation through one versioned structured result with human and machine renderers

The derived-navigation repair is the smallest safe mutation that proves the shared plan, precondition, transaction, verification, result, and rollback contracts without introducing unsettled lifecycle authority.

The first slice deliberately excludes semantic search, general artifact scaffolding, Framework installation, upgrade, restoration, and Extension lifecycle. The existing implementation continues to serve those operations until their replacement boundaries are accepted and verified.

Cutover requires:

- Direct command and operation tests against the accepted replacement contract
- End-to-end tests over real temporary workspaces, filesystem identities, Git state, process execution, and packaged layouts where applicable
- Stable structured-result fixtures and human rendering checks
- Evidence that failure, interruption, and verification do not leave a partially applied repair
- Measured latency and output size for representative agent context operations
- Explicit classification of old tests as independently desired invariants, reusable scenarios, or legacy-only behavior

This boundary is candidate direction. Its expected high agent value follows from current dogfood use, but operation frequency and token-cost claims still need measurement.

## Dispatch Direction To Design

The new implementation should receive first opportunity to handle an invocation. It should return an explicit handled or unhandled dispatch result before execution.

An unhandled result may delegate to the frozen old main. A command recognized by the new implementation must not fall back merely because parsing, validation, planning, execution, or verification failed. Failure fallback would mask replacement defects and could execute legacy behavior after the user authorized a materially different new operation.

The formerly accepted [parallel entrypoint migration](../analysis/2026-07-31_cli-parallel-entrypoint-migration.md) kept compatibility outside replacement code. That wrapper direction is historical; direct replacement development now governs current implementation.

The final design still needs to settle:

- Whether users and tests receive explicit old and new selection paths
- Whether dispatch uses command ownership, complete invocation signatures, or another stable key
- How global help presents parallel availability without exposing migration complexity as permanent product structure
- How old behavior is retired and how stale automation detects the cutover
- Whether the old implementation is loaded lazily so new operations do not pay its startup and module-evaluation cost

## Health And Repair

The accepted [doctor and repair contract](../../crystallized/decisions/cli/cli-doctor-repair-contract.md) is authoritative for the health and repair experience.

Bare `doctor` remains strictly read-only. An explicit repair mode aggregates domain-owned diagnostics, classifies findings by fixability, plans every mechanically safe repair, applies the plan through the shared transaction boundary, reruns diagnostics, and reports repaired, still-invalid, decision-required, and blocked findings.

Doctor does not absorb installation, upgrade, restoration, extension lifecycle, or semantic decisions merely because those operations could change a reported condition. The exact repair command or flag remains part of later interface design.

## Link Integrity And Reference Suggestions To Design

The current doctor validates generated `entries` and the MVP's former Workflow dependency section, but it does not validate arbitrary Markdown links or suggest likely replacements. The replacement should treat references as one shared domain capability rather than add unrelated link walkers to individual commands.

A candidate reference graph should inventory the selected workspace or scope, assign each file a canonical physical identity, parse each relevant document once, and resolve its outgoing references without recursively reopening targets. This makes ordinary cycles such as A linking to B and B linking to A finite by construction. A visited set remains necessary for operations that traverse a selected closure. Cycles should be reported only where a relationship contract is semantically acyclic; an ordinary hyperlink cycle is not itself a defect.

Two consumers can then serve different costs and jobs:

- Full read-only doctor validates references across the finite inventoried workspace or selected scope, independent of which chain happened to be loaded.
- Context inspection can validate or expose reference findings only for the selected and loaded closure, keeping routine agent retrieval bounded and cheap.

The local-reference boundary should distinguish Framework graph references, ordinary relative file links, directory links, fragments, and workspace-local destinations outside `.agents`. A source document inside `.agents` may point to another file anywhere inside the selected workspace; doctor should resolve and check that target locally without automatically treating the destination as a routed Open Forge document or recursively validating all of its outgoing links. Candidate deterministic findings include missing targets, containment violations, case or Unicode mismatches, malformed escapes, missing Markdown extensions, and missing heading fragments.

Suggestions must remain evidence-bearing candidates rather than hidden semantic guesses. Safe sources include a unique case-correct path, a unique same-basename move, a unique nearby path under the same routed scope, or a unique close heading slug in the resolved document. Ambiguous candidates remain decision-required. Automatic repair is justified only when the correction is unique, meaning-preserving, within declared authority, and represented in the common repair plan.

Network URL validation is outside the Framework and replacement CLI scope. Doctor and repair perform no HTTP(S) request, redirect following, status probing, network suggestion, or remote-content inspection. Links that leave `.agents` while remaining inside the selected local workspace still receive the ordinary local check.

## Workflow Relationships

The current Workflow contract has no dedicated dependency section. Workflows use ordinary links and explicit instructions in Steps when another source must be read or a capability invoked. The local link checker validates those destinations without inferring an unconditional preload relation.

If repeated use later proves that deterministic typed relationships materially reduce agent cost across Workflows and other primitives, design one generic visible relationship mechanism from that evidence instead of restoring a Workflow-only construct.

## Lifecycle Direction To Design

- Separate first installation, non-destructive completion of missing framework material, intentional upgrade, and forceful restoration instead of treating every reinstall as the same operation.
- Preserve user-customized existing files by default. Require an explicit, previewable operation before replacing them with distribution defaults.
- Keep generated regions, explicitly managed blocks, user-owned files, `entrypoints` for `managed routes` through scopes, extension ownership, and overwrite companions distinct during planning.
- Define how users receive framework fixes without silently destroying customization; Git recoverability is a safeguard, not permission to overwrite.
- Explore an update wizard that presents what changed and the exact diff before mutation, then offers at least apply all changes, review and select individual changes, keep local content, or cancel. The same interaction may cover managed Framework files, installed Templates, and other managed content while preserving their different ownership rules.
- Do not treat a Template update as an automatic update to artifacts previously instantiated from it. Instance propagation would require separate provenance, ownership, conflict, and opt-in semantics.

## Canonical Markdown Syntax Help

The CLI should provide `open-forge help syntax` as the easy entry to Open Forge's canonical Markdown authoring contract. The final command vocabulary may change during interface design, but a dedicated syntax reference is part of the intended product.

The help should make one preferred form obvious for every Markdown construct Open Forge generates, parses semantically, or asks users and agents to author. At minimum, it should cover:

- Scoped YAML frontmatter with `description`, optional `responsibility`, and tags
- ATX headings
- Paragraph and blank-line separation
- Hyphen unordered lists and the canonical ordered-list form
- Fenced code blocks
- Inline relative Markdown links
- Bare established tags
- Route `Entries` and any accepted explicit relationship syntax
- Generated-region markers
- Category `entrypoints` and concrete scope `slugs`
- Overwrite companions
- Primitive-specific document shapes that deterministic validation recognizes

The command must surface the accepted [Open Forge Markdown scope](../../crystallized/documents/framework/markdown/_markdown.md) and the installed runtime sources it represents rather than make help text privately authoritative for syntax. It should distinguish:

- Canonical authoring that Open Forge emits, documents, and expects
- Unsupported equivalents that Open Forge does not promise to interpret

This boundary should apply most strongly where syntax has Framework meaning. Ordinary prose remains ordinary Markdown unless Open Forge needs to parse it. `doctor` may reject ambiguous or noncanonical Framework structures and warn about selected authoring conventions, but it should not police unrelated prose merely to enforce personal style.

Scaffolding should use the same contract to create routed artifacts with canonical metadata and structure. Templates may supply starting content, while the CLI makes placement, frontmatter, placeholder replacement, and route maintenance cheap.

## Knowledge Role Help

The CLI should also make knowledge-role selection cheap through a command such as `open-forge help roles`. Users and agents should not need to memorize a governance table before ordinary placement feels natural.

The help should expose:

- The distinct current-document, Memory, and Core roles
- The primary question, authority, and lifecycle of each role
- The difference between current state and supporting rationale
- Natural selection examples and links to applicable Templates
- The authoritative route or source file for the human-readable definition

The command must derive from or point to human-readable authoritative Framework sources rather than make CLI output a private ontology. It may offer interactive placement or scaffolding assistance, but suggestions remain transparent and correctable.

The repository-only [knowledge-role helper](../../crystallized/documents/maintenance/helpers/knowledge-roles.md) can inform this design. CLI help should expose useful selection guidance without making the internal helper a prerequisite for users.

## Structured Interface Direction

Every read and write operation should return one versioned structured result suitable for scripts and future tools. It should distinguish:

- Data
- Findings
- Warnings
- Conflicts
- Planned or applied effects
- Provenance
- Suggested next actions

Human output should be rendered from the same result. This does not require a daemon or public network API.

The accepted [CLI mutation execution contract](../../crystallized/documents/cli/contracts/mutation-execution.md) defines the shared request, plan, preflight, effect, application, verification, recovery, and evidence semantics.

The accepted [result and display decision](../../crystallized/decisions/cli/cli-result-display-boundary.md) and [Pattern](../../../patterns/open-forge/cli/commands/result-display-boundary.md) establish that handlers return typed values without terminal or process effects, the CLI selects a human or structured display adapter, non-success results contain legible coded messages, and semantic statuses map to process exit values through named objects rather than scattered literals.

The accepted [CLI Interface](../../crystallized/documents/cli/interface.md) and [command-surface decision](../../crystallized/decisions/cli/cli-command-surface.md) now own the replacement vocabulary. The historical [command-surface exploration](../analysis/2026-07-31_cli-command-surface.md) preserves the larger candidate, including its rejected `plan` prefix, typed creation, and expanded lifecycle families.

## Migration Direction

The overhaul should:

1. Classify current tests as independently desired invariant evidence, reusable scenario input, or legacy-only behavior
2. Design replacement domain behavior from accepted contracts rather than MVP compatibility
3. Establish common plan and structured-result contracts
4. Separate read-only context operations from mutation operations
5. Split installation intents before changing overwrite behavior
6. Move extension-specific semantics behind the Extensions boundary
7. Replace scattered Framework constants with explicit owned contracts
8. Preserve focused evidence around real filesystem, Git, package, and process boundaries
9. Retire MVP commands through wrapper cutover instead of compatibility code in the replacement

An unpublished MVP shape does not constrain the replacement architecture. Independently desirable behavior must be justified by the accepted Framework and product contracts.

## Testing Architecture To Design

The accepted [CLI testing architecture](../../crystallized/decisions/cli/cli-testing-architecture.md) establishes the gradual evidence model.

Current evidence on 2026-07-30 is sharply imbalanced: the runner recognizes only `unit` and `closure`; one 11 KB unit file contains the fast tier; four closure files contain most behavior; and the main closure file alone is approximately 153 KB. On the current Windows workspace, a warm local run took about 0.29 seconds for the fast tier and 95.52 seconds for closure. These timings are local evidence, not universal budgets.

The replacement organizes ordinary tests into four increasing depths:

1. Unit tests call particular exported functions or cohesive pure modules.
2. Command tests call independently importable command implementations or application handlers with typed inputs and a controlled context.
3. Integration tests cross one selected real adapter boundary without automatically exercising the complete executable.
4. End-to-end tests invoke the built CLI in an isolated operating-system temporary workspace and snapshot compact normalized public output and resulting state.

Most behavior remains below the spawned-process boundary. End-to-end coverage proves only complete critical journeys and boundaries that exist only in the distributed executable.

Tests prefer real functions, collaborators, filesystems, Git repositories, processes, and observable state. Mocks, spies, and behavior-stubbed fake adapters are not the ordinary isolation mechanism. Shared test support provides focused capability modules for declarative workspaces, process execution, Git setup, command contexts, snapshot normalization, and platform detection.

Dogfood/source alignment and Framework conformance remain visible evidence even when they reuse the same runner. Nonfunctional evidence separately measures startup, representative operation latency, output and token size, memory, large-workspace scaling, and parallel behavior. Budgets should become hard gates only after stable measurement environments and acceptable thresholds exist.

The accepted [CLI development toolchain](../../crystallized/decisions/cli/cli-development-toolchain.md) uses Bun for dependency management, scripts, tests, snapshots, and builds. Strict TypeScript checking remains a separate required step launched through Bun.

End-to-end tests use Bun's `toMatchSnapshot()` and explicit `bun test --update-snapshots` workflow. They snapshot normalized public results and workspace state. Bun also hosts artifact-portability tests that spawn the built ESM executable under Node.js, Bun, and Deno.

`Bun.build` emits the replacement from `src/cli/cli.ts` with the Node target and ESM format. Production source and the bundle remain free of Bun-only runtime APIs. Do not use a compiled or Bun-targeted artifact.

The candidate naming keeps depth visible without separating tests from behavior:

```text
commands/help/help.test.ts
commands/help/help.integration.test.ts
e2e/help.e2e.test.ts
e2e/__snapshots__/help.e2e.test.ts.snap
```

`help.test.ts` may contain focused function and direct handler evidence because the tested boundary is obvious from locality and test names. An integration suffix means the file crosses one selected real boundary. End-to-end files and their normalized snapshots live at CLI scope because they exercise the built executable.

The remaining testing design must settle:

- Acceptance or revision of the candidate specialized suffixes
- Whether property testing earns a dependency after representative pure modules exist
- The normalized snapshot fields for each end-to-end journey
- CI grouping, platform matrix, coverage use, and performance budgets
- Classification of existing cases without copying the current suite structure

## Promotion Conditions

A replacement current CLI architecture is justified when the Framework contracts are accepted and the following choices are coherent enough to govern implementation:

- Command and structured interface responsibilities
- Canonical Markdown syntax and input-version policy
- Lifecycle intents and preservation boundaries
- Shared plan, transaction, and verification contracts
- Framework and Extensions domain boundaries
- Recovery, concurrency, packaging, and performance expectations

## Deferred

- Semantic or inferred relevance remains outside the current critical path. Explicit routes, descriptions, relative links, tags, and deterministic helpers are authoritative first.
- Rune or another future retrieval layer may exploit the same visible relationship surface without becoming Open Forge runtime truth.

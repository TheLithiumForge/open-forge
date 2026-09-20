---
open-forge:
  description: "Historical CLI-v2 source: Accepted architecture, boundaries, transition, source organization, execution flow, safety, and verification for the replacement Open Forge CLI"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Open Forge CLI Architecture

## Status And Scope

This document owns the accepted architecture of the replacement Open Forge
CLI. Implementation has resumed under accepted authority, and `src/cli/` now
contains an implemented, buildable Foundation candidate. The candidate proves
the replacement composition, parsing, result, presentation, final-output,
embedded-build, test-tier, and cross-runtime boundaries. It does not yet
implement the complete nineteen-leaf command surface and is not shipped,
published, or ready for cutover.

Accepted architecture and the complete public command surface govern the
remaining incremental slices. The [MVP architecture](mvp-architecture.md),
[public MVP contract](../../../../../docs/cli.md), and frozen `src/cli-mvp/`
source preserve the previously shipped behavior as explicitly invoked
reference evidence. Lifecycle details explicitly left open by the interface
and representative module names remain Emerging or Working until decided.

The [top architecture](../architecture.md) owns the CLI's place in Open Forge. The [Framework Architecture](../framework/architecture.md) owns the human-readable contracts the CLI consumes. The [CLI interface](interface.md) owns the accepted nineteen-leaf public surface and interaction behavior. This document owns replacement component boundaries, dependency direction, execution, state, safety, migration, and verification.

## Product Role

The CLI is an optional deterministic reasoning accelerator and workspace safety tool. Open Forge deliberately treats any capable agent as its primary CLI user while keeping people first-class users and consequential decision authorities.

The CLI makes evidence and mechanical action cheap. It can retrieve, order, explain, validate, plan, scaffold, and apply deterministic effects over visible Framework sources. It does not infer product intent, accept candidate truth, perform semantic reasoning, or privately own workspace meaning.

Five stable user jobs define its capability boundary:

1. **Orient** to the workspace, applicable scope, state, baseline, and continuity context.
2. **Retrieve and explain** explicit routes, relationships, inheritance, provenance, syntax, and applicable context.
3. **Create and maintain** canonical artifacts, scopes, placement, and derived navigation while leaving semantic authorship in ordinary files.
4. **Validate and repair** through complete read-only diagnosis and explicit mechanically safe repair.
5. **Manage lifecycle** through one whole-Framework install operation and
   distinct preservation-aware Extension operations.

Planning, structured results, provenance, formatting, verification, rollback,
and recovery support these jobs. They are shared contracts rather than
separate product jobs.

The governing invariant is:

> Deterministic tools reduce the cost of obtaining and applying context; they do not privately own its meaning.

## Boundaries And Non-Goals

The CLI is not:

- An agent runtime, scheduler, or workflow executor
- A semantic search or recommendation authority
- A private database of Framework truth
- An automatic approver of Emerging Memory
- A substitute for Git or ordinary file inspection
- An authority over installed Extension runtime meaning
- A network link validator or general-purpose web safety tool

Local references outside `.agents` remain ordinary filesystem references and may be checked locally. HTTP and HTTPS validation is outside Framework scope.

## Executable And Transition

There is one package binary and one executable entrypoint:

```text
src/cli/cli.ts
  -> src/cli/main.ts
       -> replacement command registration and execution
```

The package distributes one `dist/cli.mjs` artifact with the standard Node.js
shebang. A conventional installation exposes the plain `open-forge` command;
Bun and Deno execute or install the same artifact through their explicit
runtime launchers. The [runtime compatibility contract](contracts/runtime-compatibility.md)
defines the accepted floors, parity promise, capability gates, and release
matrix. Open Forge does not ship runtime-specific bundles or a polyglot
shebang.

The existing implementation and its tests are frozen under `src/cli-mvp/`. It identifies itself as `open-forge-old` and is explicitly runnable through `bun run cli:old` for comparison and retained evidence. It is not imported by the replacement, built into the package, or reachable through replacement command dispatch.

There is no compatibility router, implementation selector, fallback, or staged command-root ownership. The replacement accepts only its intentional interface. An unimplemented or invalid command remains an ordinary replacement parse result.

Port one coherent command or shared capability slice at a time through the final source structure. A port is complete only when its accepted behavior, presentation, and relevant integration boundaries are verified. After the complete replacement and its promoted evidence are accepted, remove the frozen MVP source and remaining MVP-only tests.

## Command Boundary

The accepted replacement boundary uses `commander` for command routing, arguments, options, help metadata, command-input validation, and action invocation. Source imports `Command` and related APIs from the matching `@commander-js/extra-typings` release so chained definitions infer action and option values.

A focused adapter maps Commander metadata to `@bomb.sh/tab` while preserving the accepted public Completion family. It may register one hidden `complete` callback but does not add another visible generator command. The callback is an internal shell protocol endpoint rather than a public leaf or domain operation.

The [Completion protocol contract](contracts/completion-protocol.md) limits
that endpoint to static metadata and bounded read-only providers for finite
local identities. It delegates paths to the shell, performs no executable or
unbounded discovery, and returns no dynamic candidates when a provider cannot
complete safely within its limits.

Each same-named leaf module keeps readonly command metadata, an independently callable named handler, and its `register<Command>()` function together. Registration builds and attaches the Commander definition and action. The action maps inferred values into ordinary typed input; application and domain code do not depend on Commander types.

The [CLI Interface](interface.md#command-map) fixes the leaf inventory, arguments, global flags, repeated command flags, and mutation preview spelling. Command metadata and registration implement that contract rather than rediscovering it from legacy source.

The composition root disables Commander's implicit public help command, configures the accepted help and version aliases, preserves global options before or after subcommand words, intercepts parser output and exits, and uses `parseAsync()`. A group registration module may configure local help and the typed missing-operation result for a bare group, but it contains no domain handler.

The build generates one temporary typed module and bundles it into
`dist/cli.mjs`. That module exports `BuildIdentity` and `EmbeddedAssets` as
readonly data: the CLI version, exact Framework payload, first-party Extension
catalogue, per-file checksums, and aggregate fingerprints. It is never
committed, loaded from an adjacent runtime path, or discovered from the current
workspace. `main()` receives these values explicitly at the composition root;
direct tests inject small ordinary fixtures through the same interfaces.

Initial embedded payload files are validated UTF-8 text and become string
values in the generated module. Canonical workspace-relative paths remain
separate keys rather than being inferred from import names. The aggregate
Framework fingerprint is SHA-256 over the UTF-8 encoding of one whitespace-free
JSON array containing a named fingerprint-domain value and path/checksum pairs
sorted by canonical UTF-8 path spelling. Each checksum is `sha256:` plus the
lowercase SHA-256 of the exact encoded file bytes. Production owns the domain
value and schema through readonly named constants so framing cannot drift
between the build, Node.js, Bun, Deno, status, and tests.

Commander validation is sufficient for options and positional arguments. Other untrusted inputs, such as manifests, `.agents/open-forge.json`, formatter commands, or serialized operation data, receive focused validation at their boundary. A general schema library is introduced only when a real non-command boundary benefits from it; Zod is not mandatory.

Frontmatter is one accepted boundary-specific exception. The bundled `yaml`
package parses strict, bounded YAML syntax and positions behind
`MarkdownDocumentFacts`; Open Forge validation still owns the exact three-field
metadata schema and rejects unsupported nodes and semantics. Focused Markdown
readers continue to own headings, generated regions, references, ignored
ranges, and concealed source without a general Markdown AST dependency.

Lazy command loading is unnecessary at the expected command count. If measured startup cost later warrants it, a registered handler may dynamically import its implementation without changing the public command contract.

## Source Organization

Replacement TypeScript follows locality of behavior and scope:

```text
src/cli/
  cli.ts
  main.ts
  flags/
    global-flags.ts
    dry-run.ts
  commands/
    status/
      status.ts
      status.test.ts
    install/
      install.ts
      __tests__/
        install.test.ts
        install-workspace.integration.test.ts
    route/
      route.ts
      list/
        list.ts
        list.test.ts
```

Global flag definitions live together at CLI scope and are registered once by
the composition root. One typed map keyed by operation id declares which
global concerns apply to each leaf. The parser rejects an inapplicable global
before workspace selection or handler execution. The shared `--dry-run`
definition also lives at CLI scope but is explicitly reused only by mutating
leaves. An operation-specific flag remains with its command.

A direct leaf receives one same-named folder. A grouped leaf mirrors its one-level public family path so related commands share a natural family scope. The group module creates the Commander group and calls directly imported child registration functions; it contains no domain handler. A leaf's metadata, named handler, and registration function remain together in its same-named module. Request resolution, planning, presentation, helpers, fixtures, and direct tests split locally when cohesion or the source-size limit requires it, while the handler stays a readable coordinator.

Shared behavior moves only to the nearest common ancestor of demonstrated consumers. Shared folders express scope; each file names one capability. There is no generic `utils.ts`, speculative service catalogue, or dependency container.

One production subject expected to remain a one-test subject keeps that test
adjacent. A subject that has or is expected to need multiple test files starts
with the nearest `__tests__/` directory, which also contains its test-only
support. Filenames identify the production behavior and retain the accepted
tier suffix; separate `unit/` and `integration/` directories are redundant and
are not used. Whole-executable journeys remain under `src/cli/e2e/` because
they belong to the CLI process rather than one production module.

Pure functions fit stateless deterministic transformations. Classes fit cohesive state, lifecycle, resource ownership, or side effects when they make those responsibilities clearer. Readability, not paradigm loyalty, decides.

Commands may consume shared application or domain capabilities but do not import sibling commands' private modules. Dependency direction remains visible through direct imports and focused constructor or function parameters.

## Execution And Presentation

Every invocation follows one direction:

```text
argv
  -> replacement main
  -> Commander command parsing and validation
  -> command-local complete request resolution
  -> named handler
  -> direct typed prerequisite inspection
  -> application operation
  -> typed result
  -> applicable typed exposure projection
  -> selected human or JSON display adapter
  -> stdout, stderr, and exit status
```

Handlers return typed values. They do not call `console`, write streams, select presentation, or set process state. Human and JSON adapters consume the same result and never rerun application behavior.

Status applies its focused exposure projection after the handler result and
before either display adapter. `--redact` never reaches the handler and does
not change inspection, semantic status, suggestions, messages, or process
completion. The projector exhaustively reconstructs every public status field
and coded message; it is not a logger, middleware filter, or unstructured
string replacement. Keep this boundary status-local until another real
operation requires the identical exposure contract.

The [operation prerequisite contract](contracts/operation-prerequisites.md)
defines the exact evidence required by each leaf. Handlers call focused typed
inspectors directly and exhaustively branch on their named results. The CLI
does not flatten Framework presence into a universal permission switch or
resolve behavior through a string-keyed requirement registry, middleware
chain, or service locator.

The [request construction contract](contracts/request-construction.md) keeps
Commander-validated input, semantic request, immutable execution context,
application policy, and interaction authority distinct. Guided and explicit
input produce the same complete request before planning. Raw `--json`,
`--workspace`, `--yes`, `--redact`, `--dry-run`, `--overwrite`, TTY, and process state do
not leak into semantic requests or domain planners.

The root creates one shared completion function and passes it to every command registration function. Each Commander action maps inferred input, awaits its named handler, and passes the result plus normalized presentation selection to that function. The root uses `parseAsync()` and intercepts Commander output and exit behavior so the shared function alone selects the display adapter, writes final application streams, and applies process completion.

Structured mode emits one five-field versioned JSON document containing schema version, operation identifier, semantic status, stable coded messages, and operation-specific data. Human output remains concise, legible, and operation-aware. The executable boundary alone writes final streams and maps semantic status through the accepted `0`, `1`, `2`, `3`, `4`, and `130` exit contract.

Expected invalid, blocked, cancelled, attention, and failed outcomes are typed results. Unexpected failures cross one top-level safety boundary. Schema versions, operation ids, statuses, message levels, diagnostic codes, exit values, mutation states, effect kinds, purposes, resource states, and target kinds come from enums or readonly `as const` objects rather than scattered string or numeric literals.

## Planning And Mutation

Every mutation has one typed request, one inspectable plan, one application path, and one typed result. Interactive collection, non-interactive arguments, JSON presentation, and `--dry-run` cannot create alternate mutation implementations.

The [mutation execution contract](contracts/mutation-execution.md) owns the shared semantic request, plan, preflight, effect, application, verification, recovery, and evidence guarantees. Production TypeScript will own their exact declarations. Maintained documentation will not duplicate those declarations after implementation.

A plan contains every operation-owned persistent external effect before the first
write. This includes derived navigation, managed lifecycle state,
marker-owned shell integration, Gitless backups, and any other state the
selected operation intends to retain. Discovery, planning, and preflight do
not mutate external state. Temporary files used inside an atomic write are
executor mechanics rather than domain effects, but the executor must remove
them or report residual state.

Each effect names its target, intended transition, preconditions, protected boundaries, verification, and recovery expectations. Preflight validates the complete plan and every effect. If any effect cannot pass, no effect is applied. Application accepts only a completely preflighted plan, revalidates volatile assumptions immediately before each effect, records applied state, and verifies both individual effects and the completed operation.

The internal plan retains physical targets, complete before and after bytes, file identities, rollback material, and executor-temporary paths when the effect requires them. After a complete plan exists, focused operation data composes one shared mutation projection containing mode, preflight, application, verification, recovery, and one ordered effect array. Each effect exposes its logical target, before and after evidence, and one final state. Human counts derive from that array. Results never serialize full file contents, physical paths, temporary paths, or private recovery material.

Generated or dependent effects are projected during planning rather than
discovered through mutation. Application does not append a hidden index,
lifecycle, or cleanup write after preflight. Extension add, update,
and remove therefore project affected `Entries` and final
`.agents/open-forge.json` replacement from the assembled destination. Shared
mechanical effects remain file creation, file replacement, file deletion, and
directory creation. Operation-owned purposes and focused evidence preserve
domain meaning without duplicating executors.

The CLI introduces narrow external capability boundaries only when behavior
demonstrates a product-significant ownership, lifecycle, failure-translation,
or replacement need. Filesystem containment, atomic application, Git
readiness, and formatter execution are demonstrated boundaries. Package,
clock, terminal, and process abstractions do not exist merely because those
nouns exist.

## Doctor And Repair

Bare `doctor` is always read-only. It inspects the complete applicable Open
Forge surface, not every workspace file. Discovery starts from the exact
workspace entry and `./.agents/loader.md`, authored route topology, embedded
Framework targets, `.agents/open-forge.json`, adjacent backups, and visible
residual evidence. It then
visits their known files and contained local-reference closure through bounded
traversal. Canonical physical real paths prevent reference cycles. Runtime file
identifiers enrich alias evidence only after a capability probe proves their
precision.

The [route inventory contract](contracts/route-inventory.md) supplies one
authored topology, natural identity, metadata, parent-chain, overwrite, and
generated-navigation view to context, find, Route commands, doctor, and
rebuild. Generated Entries remain a verified derived projection and cannot
hide or invent authored routes in deterministic command output.

The [diagnosis and repair contract](contracts/diagnosis-and-repair.md) composes
workspace, recovery, route, reference, Framework, and Extension domains through
visible direct calls. Domain codes are public evidence rather than dispatch
keys. Each safe-repair finding is created beside its internal typed proposal;
the repair coordinator calls domain planners directly.

Repair requires an explicit mode. It:

1. Collects and classifies all findings.
2. Plans every mechanically safe fix.
3. Revalidates assumptions.
4. Applies the plan through the shared recoverable mutation path.
5. Reruns diagnosis.
6. Reports repaired, remaining, manual-decision, and blocked-repair findings.

Repair never guesses authored meaning, lifecycle intent, ownership, or a potentially destructive choice. Suggestions are limited to deterministic local evidence; the CLI does not become a general recommendation engine.

## State And Authority

The CLI distinguishes:

- **Authored state:** human-readable sources whose content remains authoritative for meaning.
- **Derived state:** indexes, manifests, checksums, caches, and other reconstructable projections.
- **Managed lifecycle state:** transparent Framework paths and exclusions,
  Extension ownership and source classification, and advisory checksums needed
  for install, update, and removal.

Derived state never becomes a second semantic authority. Managed lifecycle
state records operational facts but does not participate in agent routing or
define installed content's meaning. Framework selection, flat Extension
ownership, formatter configuration, source classification, and advisory
checksums use the one committed `.agents/open-forge.json` defined by the
[managed lifecycle contract](contracts/managed-lifecycle.md). Framework
presence remains independent from that file.

External Extension source receives the separate
[source-review contract](contracts/source-review.md) after subject selection
and before mutation planning. The review authority is bound to the exact
inspected fingerprints and is never persisted. Selection, source trust,
collision authority, deletion, executable configuration, and Git policy remain
independent transitions.

The CLI may preserve, validate, copy, scaffold, or change files only within an explicit operation and authority boundary. Direct file authoring remains valid and complete.

## Safety And Recovery

### Shared Safety Baseline

The replacement preserves the MVP's proven safety properties:

- Bounded targets and visible effects
- Lexical, physical, portable, and route identity checks
- Containment across symlinks, junctions, hard links, aliases, and control paths
- Preflight before mutation
- Ownership-aware managed changes
- No partial success hidden by output
- Verification after application
- Git-first review and hard-stop recovery, with an explicit Gitless backup path

### File Application And Capability Evidence

Mechanical write safety does not depend on Git. A file replacement revalidates
the expected original, writes planned bytes to a contained sibling, promotes a
complete file, verifies the result, and cleans temporary material. Recovery
restores the original only while the target still matches the applied identity.
An unexpected concurrent edit is preserved and reported as residual state.
Git supplies durable review and hard-stop recovery; Gitless operations use
explicit adjacent `.bak` files for replacements and deletions.

The initial [filesystem mutation spike](contracts/filesystem-effects.md#concurrency-boundary)
proves the preservation-first direction on Windows NTFS and explains why
ordinary rename is insufficient after revalidation. Its journal experiments
remain archived evidence rather than production architecture. The later
identity-precision probe accepts Node.js and Bun on this host but finds exact
identity unavailable through Deno on the tested Windows NTFS combination.
Production integration, supported-filesystem coverage, preserved metadata,
packaged execution, Git and backup journeys, and hard-stop residual detection
remain implementation requirements. One unavailable capability blocks the
affected operation rather than changing its ABI or falling back to in-place
overwrite.

### Derived Navigation Recovery

`route rebuild` uses in-process reverse-order recovery for handled failures and
no persistent crash journal. Its planner owns generated-region meaning. Each
promoted result remains a complete valid file, and a later rebuild recomputes
the complete selected closure after a hard process stop. Optional formatting
runs only after that mutation has completed under the post-processing boundary
below.

### Managed Workspace Recovery

Authored, manager-owned, and destructive workspace plans use the
[Git-first recovery contract](contracts/workspace-recovery.md). Relevant dirty
paths block unless `--skip-git-check` is explicit. Gitless or bypassed plans
preflight adjacent backups where needed. The CLI creates no persistent
transaction directory or cooperative mutation lock.

### Completion Recovery

User-level Completion mutation follows its separate [Completion lifecycle
contract](contracts/completion-lifecycle.md). It writes an owned generated asset
before adding activation and removes activation before deleting that asset.
Every hard-stop boundary therefore leaves dormant, usable, or disabled state
that deterministic rerun can finish. Completion uses no workspace Git,
workspace lifecycle entry, user-home lockfile, or persistent journal.

### Post-operation Formatting

Workspace formatting follows the [formatting contract](contracts/workspace-formatting.md):
explicit configuration and workspace evidence select one strategy per file,
exact paths constrain execution, manual formatting remains supported as
post-operation guidance, and a
resolved invocation is classified as `SAFE`, `RUN`, or `BLOCK` from its actual
configuration and execution boundary. Executable configuration, plugins,
analyzers, project code, and custom commands require authority outside generic
`--yes`; downloads, installation, restore, network access, and unbounded file
effects remain blocked. Formatting is not an effect inside the immutable domain
plan and never causes completed primary work to roll back. Successful automatic
formatting is revalidated and may refresh advisory checksums through a separate
atomic finalization; failure or manual follow-up returns attention while the
primary mutation remains applied.

### Unaccepted Boundaries

Concurrent-plan isolation beyond revalidation, exact-external recovery, and
forceful lifecycle intents remain unaccepted.

## Testing And Toolchain

Bun orchestrates replacement development while strict TypeScript and
type-aware ESLint remain separate read-only gates. Production and test ambient
types stay distinct. `package.json`, the lockfile, configuration, scripts, and
focused source are authoritative for exact executable values. The [CLI
Development Toolchain](development-toolchain.md) defines their relationships
and the quality, portability, and compatibility obligations they preserve.

Evidence progresses through:

1. Direct unit tests for pure functions and focused modules.
2. Direct command tests for named handlers with typed inputs and explicit real dependencies.
3. Focused integration tests crossing one selected real boundary.
4. A small end-to-end suite that spawns the built CLI in OS-temporary workspaces.

Tests prefer real functions, filesystems, Git repositories, processes, and
resulting state. Test doubles are exceptional boundary tools, not the default
architecture.

Snapshots retain one focused value or stable public process projection.
Volatile setup and safety-critical invariants remain outside snapshot approval.
The [CLI Tiered Test Slice](../../../../patterns/open-forge/cli/bun/tiered-test-slice.md)
owns the reusable evidence placement and snapshot shape.

The distributed ESM artifact must execute under the accepted Node.js, Bun, and Deno versions. Production code does not depend on the Bun global or `bun:` modules. Cross-runtime portability is proved at the built-artifact boundary.

## Required Implementation Evidence

The accepted structure is expressed by the sections above and its linked
focused contracts. Implementation still requires:

- Port completion evidence for each replacement command and shared capability slice
- Commander adoption evidence for inferred cross-file registration, readable aliases, completion protocol, and one Node/Bun/Deno artifact
- Representative domain boundaries and earned external ports
- Representative implementation evidence for the accepted managed lifecycle
  preservation and recovery semantics
- Supported operating-system and filesystem evidence, richer metadata
  capabilities, caching, and measured Git and Gitless recovery performance

## Patterns

- [CLI command slice](../../../../patterns/open-forge/cli/commands/command-slice.md)
- [Predictable command surface](../../../../patterns/open-forge/cli/commands/predictable-command-surface.md)
- [Guided operation](../../../../patterns/open-forge/cli/commands/guided-operation.md)
- [Planned mutation](../../../../patterns/open-forge/cli/filesystem/planned-mutation.md)
- [Managed reconciliation](../../../../patterns/open-forge/cli/filesystem/managed-reconciliation.md)
- [Workspace formatter strategy](../../../../patterns/open-forge/cli/workspace/workspace-formatter-strategy.md)
- [Reviewed source boundary](../../../../patterns/open-forge/cli/commands/reviewed-source-boundary.md)
- [Diagnostic domain slice](../../../../patterns/open-forge/cli/diagnostics/diagnostic-domain-slice.md)
- [Contained filesystem target](../../../../patterns/open-forge/cli/filesystem/contained-filesystem-target.md)
- [Markdown document facts](../../../../patterns/open-forge/cli/markdown/markdown-document-facts.md)
- [Local reference inventory](../../../../patterns/open-forge/cli/markdown/local-reference-inventory.md)
- [Route inventory projection](../../../../patterns/open-forge/cli/markdown/route-inventory-projection.md)
- [Result and display boundary](../../../../patterns/open-forge/cli/commands/result-display-boundary.md)
- [Template instantiation boundary](../../../../patterns/open-forge/cli/commands/template-backed-creation.md)
- [Tiered test slice](../../../../patterns/open-forge/cli/bun/tiered-test-slice.md)
- [Focused TypeScript module](../../../../patterns/open-forge/typescript/focused-module.md)
- [Named TypeScript values](../../../../patterns/open-forge/typescript/named-values.md)
- [Nearest shared scope](../../../../patterns/software/source-locality/nearest-shared-scope.md)
- [TypeScript contract ownership](../../../../patterns/open-forge/typescript/contract-ownership.md)

## Decisions And Rationale

- [Agent-first product contract](../../decisions/cli/cli-agent-first-product-contract.md)
- [Core job model](../../decisions/cli/cli-core-job-model.md)
- [Command surface](../../decisions/cli/cli-command-surface.md)
- [Doctor and repair contract](../../decisions/cli/cli-doctor-repair-contract.md)
- [Explicit diagnostic composition](../../decisions/cli/cli-explicit-diagnostic-composition.md)
- [Direct replacement development](../../decisions/cli/cli-direct-replacement-development.md)
- [Command framework](../../decisions/cli/cli-command-framework.md)
- [Runtime boundary validation](../../decisions/cli/cli-runtime-boundary-validation.md)
- [Source locality](../../decisions/cli/cli-source-locality.md)
- [Result and display boundary](../../decisions/cli/cli-result-display-boundary.md)
- [Testing architecture](../../decisions/cli/cli-testing-architecture.md)
- [Development toolchain](../../decisions/cli/cli-development-toolchain.md)
- [Git-first recovery policy](../../decisions/cli/cli-git-first-recovery-policy.md)
- [Preservation-first file mutations](../../decisions/cli/cli-preservation-first-file-mutations.md)
- [Contained filesystem identity](../../decisions/cli/cli-contained-filesystem-identity.md)
- [Local reference integrity](../../decisions/cli/cli-local-reference-integrity.md)
- [Authored route inventory](../../decisions/cli/cli-authored-route-inventory.md)
- [Frontmatter YAML boundary](../../decisions/cli/cli-frontmatter-yaml-boundary.md)

## Related Views

- [CLI interface](interface.md)
- [Shipped MVP architecture](mvp-architecture.md)
- [Open Forge architecture](../architecture.md)
- [Framework Architecture](../framework/architecture.md)
- [Extensions MVP Architecture](../extensions/architecture.md)

---
open-forge:
  description: Historical gates, tasks, dependencies, validation, and stop states from the removed first C# CLI implementation program
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, Program, Gate, Plan]
---

# CLI Release Plan

This Plan was superseded by the greenfield reset on 2026-08-21. It preserves the
former sequence and evidence expectations as historical input. It is not an
active Plan and does not define current implementation structure or state.

## Authority and use

The maintainer's accepted direction and the current authoritative sources define
the replacement CLI's meaning. This file records program status, execution order,
validation, and stop states. It does not define command behavior or make the
replacement shipped.

The current authority set is the accepted [CLI Architecture](../../crystallized/documents/cli/architecture.md),
the consolidated [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md),
the [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md),
and the detailed contracts under [`contracts/`](../../crystallized/documents/cli/contracts/_contracts.md).
The scoped [CLI Directives](../../../directives/open-forge/cli/_cli.md) and [CLI
Patterns](../../../patterns/open-forge/cli/_cli.md) govern applicable work.
Mutable Working retains only the program, Decision Agenda, Release Plan, active
Task, and Checkpoint; sealed handoffs are transfer snapshots, not command
authority.
The [archived Gate 1 records](../../archived/cli-release/gate-1-audit.md),
[migration ledger](../../archived/cli-release/contract-migration-ledger.md), and
[review history](../../archived/cli-release/review/gate-2-review-overview.md)
preserve history and evidence only. They never replace current authority.

## Current status

Gates 1, 2, 3, and 4 are complete. Gate 2 remains non-shipping. Gate 3
Architecture was accepted by the maintainer with all revised decisions. Gate 4
crystallized one accepted source set, finalized the `contracts/index/` and
`contracts/references/` paths, archived history, reconciled the applicable
Directives, Patterns, Templates, maps, and public documents, and completed its
review and bounded validation. The known legacy Doctor limitation remains
preserved Gate 5 evidence and is not claimed clean.

Gate 5 is authorized and active through bounded Tasks. The [Foundation And Native
AOT Spike Task](task-foundation-aot-spike.md) is complete. The active route-list
feature branch now contains the `.slnx`, production project, exactly three
independently runnable projects, one physical source-only shared test-support
folder, the shared CLI shell, and the first retained `route list` command with
local `win-x64` Native AOT evidence. The replacement remains non-shipping, and
Task acceptance plus local integration remain pending. No accepted shipping
executable, lifecycle file, workspace lock, package, release artifact, or release
exists. No staging or candidate path is current.

The accepted foundation is integrated into local `develop` at `b62bc2c`. The
[Route List Task](task-route-list.md) is active from that exact baseline on
`feature/cli-route-list` and is the first command in the durable sequence below.
Before another retained command begins, this Task also completes the shared CLI
shell architecture that should immediately follow the runtime/AOT foundation:
global invocation state and signals, typed shell messages and stages, explicit
compile-time command and renderer plug-in boundaries, and independently runnable
unit evidence for each shared stage.

| Gate | Purpose                                                           | State                             | Exit authority                                  |
| ---- | ----------------------------------------------------------------- | --------------------------------- | ----------------------------------------------- |
| 1    | Evidence and current-truth audit                                  | Complete; non-shipping            | Maintainer approval                             |
| 2    | Product and command contract decisions                            | Complete; non-shipping            | Maintainer acceptance                           |
| 3    | Architecture and foundation design                                | Complete; accepted                | Maintainer acceptance                           |
| 4    | Crystallization, final indexing, review, and readiness validation | Complete; accepted/current        | Maintainer acceptance                           |
| 5    | Complete implementation sequence and release proof                | Active; `route list` Task started | Reproducible evidence and maintainer acceptance |
| 6    | Post-implementation documentation, history, and release closeout  | Blocked until Gate 5              | Maintainer release and closeout acceptance      |

## Completed Gate 1 and Gate 2 foundations

- The replacement is an optional agent-first Framework accelerator with a
  predictable human maintenance surface. Operations are stateless and
  deterministic. Plain Markdown remains complete without the CLI.
- The retained delivery surface is all current contracted commands. Completion
  is rejected, and no partial `context` plus `find` publication or retained-later
  release slice exists.
- `src/cli-mvp/` contains frozen source for `open-forge-old`; neither
  `src/cli-mvp/` nor `open-forge-old` is replacement implementation or contract
  authority. `open-forge-old` remains the separate frozen executable. Rune
  remains outside this program.
- The split contract migration is complete. The old mixed sources were removed
  after lossless reconciliation, and the final shared and command-local contract
  topology is now under the Crystallized CLI route.
- No command review is active. Settled, rejected, and superseded review material
  remains archived historical context.

## Current source set

The accepted Crystallized source set is:

- [CLI Architecture](../../crystallized/documents/cli/architecture.md), which defines the accepted implementation and
  release boundaries.
- [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md), which provides the concise overview of command-contract roles, topology, and authority boundaries.
- [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md), which defines cross-command conventions.
- [Detailed command contracts](../../crystallized/documents/cli/contracts/_contracts.md), which define command-local behavior and interfaces.

These sources are current authority without competing Working copies. The final
contract paths for the `index` and `references` commands are `contracts/index/`
and `contracts/references/`. Physical-identity regressions are retained as Gate 5
implementation evidence rather than as a current staging obligation.

## Gate 3 — Accepted Architecture result

Gate 3 accepted the Architecture and all revised decisions. The result is
summarized here; the [Architecture](../../crystallized/documents/cli/architecture.md) defines the exact detail.

- The accepted design specifies `OpenForge.slnx` with one production project,
  `OpenForge.Cli`, and one future production executable. The source project is
  physically rooted at `src/open-forge-cli/OpenForge.Cli`. The accepted managed
  test topology has exactly three independently runnable projects under
  `tests/open-forge-cli`: `OpenForge.Cli.UnitTests`,
  `OpenForge.Cli.IntegrationTests`, and `OpenForge.Cli.EndToEndTests`, separated
  by their evidence boundaries. `OpenForge.Cli.TestSupport` is one shared source
  folder, not a fourth project. The active feature branch now has this target
  topology. Solution folders cannot hide physical folders.
- The composition root explicitly builds the command tree and composes each
  command through the shared typed CLI shell: centralized typed definitions,
  parser-owned values, normalized invocation state, a complete command-local
  request, one closed operation delegate, one cached command-local renderer set,
  rendered output, explicit process writers, and process completion. Each
  immutable shell stage is directly testable. Direct construction and pure
  capability functions come first. Dependency injection is prohibited for shell
  registration, dispatch, stage composition, renderer selection, and writer
  selection. A separately justified command-local lifecycle or resource may earn
  a source-generated, Native-AOT-safe path, but a general container is not a
  default boundary.
- The accepted dependency versions are System.CommandLine 2.0.11, Markdig 1.3.2,
  and YamlDotNet 18.1.0. The accepted target removes the executable Markdig
  reference while no retained Markdown consumer exists; a later retained consumer
  may add back that exact version through one fixed CommonMark pipeline. The
  accepted target uses one CLI-root source-generated YamlDotNet context for the
  three accepted semantic models and source-generated System.Text.Json metadata
  with reflection disabled. The conservative `open-forge-markdown-v1` fingerprint
  is the accepted future semantic identity. Unsupported equivalence fails closed.
- The only replacement lifecycle document is
  `.agents/open-forge.lifecycle.json`, schema version 1, with isolated
  `framework` and `extensions` sections in one common envelope. The replacement
  does not read, recognize, migrate, alias, or fall back to old lifecycle or
  Extension files. Existing old-format files remain ordinary untouched content
  outside replacement authority.
- The BCL, especially real `System.IO`, is the first filesystem boundary. Tests
  use real isolated operating-system temporary resources. Operations that mutate
  the selected workspace acquire the actual OS-level lock at
  `.agents/open-forge.lock`; `extension create` has no workspace subject and is
  the only current no-workspace mutation exception. File existence is not lock
  ownership. No fake or virtual filesystem is part of the design.
- Every operation forms one concrete typed result. The structured envelope is
  schema version 1 with `schemaVersion`, `command`, `status`, `workspace`,
  `result`, and `next`. Workspace-aware results contain the exact selected
  workspace and method; accepted no-workspace results retain `workspace: null`.
  The seven semantic statuses have fixed exits, and
  recovery provenance binds workspace, operation, target, artifact, expected
  identity, and recovery state. Gate 5 must prove these boundaries.
- Tests use xUnit v3 through Microsoft Testing Platform. Every test has an
  explicit `DisplayName`, one durable `Feature` trait, and exactly one
  `Evidence` trait from `Unit`, `Integration`, `EndToEnd`, or
  `PackageEndToEnd`. Shared-shell Unit evidence covers every directly callable
  stage, terminal validation and no-op modes, workspace-aware and no-workspace
  state including `workspace: null`, preserved boundary statuses, unknown finite
  values, operation-once and cancellation, renderer selection, explicit writers,
  and process completion.
- The release publishes exactly six RIDs: `win-x64`, `win-arm64`, `linux-x64`,
  `linux-arm64`, `osx-x64`, and `osx-arm64`. The package graph uses the launcher
  `@thelithiumforge/open-forge` and the six matching
  `@thelithiumforge` platform packages. Packages contain no download,
  postinstall, compilation, or behavioral wrapper. Checksums, signatures, SBOM,
  provenance, OIDC attestation, support-floor execution, and main-only
  publication are release requirements.

Architecture acceptance does not provide source, Native AOT, package, CI, or
release evidence. Those are Gate 5 proof obligations.

## Gate 4 — Completed closeout

Gate 4 is complete. It established one accepted Crystallized source set for the
replacement CLI:

- The CLI Architecture, consolidated Command Contract Set, Shared CLI Operation
  Contract, and detailed `contracts/**` are the current command source set.
- The final physical contract paths are `contracts/index/` and
  `contracts/references/`. No staging or candidate path is current.
- Gate 1 audits, migration records, and review records were archived as
  historical evidence. Directives, Patterns, Templates, maps, and public
  documents were reconciled with the accepted source set.
- Generated `Entries` were regenerated through the repository's navigation
  process. Their interiors remain generated navigation, not authored authority.

The final validation evidence is:

- Fresh targeted semantic and writing review findings were corrected, and
  focused rereviews passed.
- The targeted legacy `open-forge-old index` check succeeded during temporary
  compatibility staging; the final paths were restored afterward.
- `open-forge-old load --bodies` succeeded during staging.
- The final legacy Doctor remains unable to traverse the final `_index.md` and
  `_references.md` physical-identity compatibility names. This preserved
  new-CLI regression is not claimed clean.
- Custom final validation covered 142 scoped Markdown files, 2,276 local links,
  and 277 anchor references with zero broken.
- Candidate paths: 0. Temporary Find/Index IDs: 0. Old Working contract paths: 0. Current old lifecycle claims: 0. Bad Crystallized frontmatter tags: 0.
- Authored current files were formatted, `git diff --check` was clean, and
  sealed earlier handoffs were unchanged.

Gate 4 did not implement a command, create a package, run a release, or accept
Gate 5. The replacement remains non-shipping.

## Gate 5 — Active implementation plan

Gate 5 is authorized and active. The first bounded Task established the stable
.NET 10 foundation and local Windows `win-x64` Native AOT evidence without
implementing a retained command. It prepared the six-RID workflow; WSL, macOS,
the native runner jobs, and support-floor execution remain later Gate 5 evidence.
Packaging, publication, and release remain outside this Task, and the replacement
remains non-shipping.

The active route-list Task continues under the accepted Development Workflow. It
first discards the unaccepted post-Blue experiment and deliberately rebuilds the
shared CLI foundation without retained command behavior: one centralized typed
syntax/definition graph, `System.CommandLine`-owned parse facts, only a bounded
delimiter guard, global invocation/workspace/status/next-action shapes, directly
testable typed stages, explicit output, and one CLI-root source-generated YAML
context. That increment removes Foundation probes, route implementation/evidence,
and the unused executable Markdig reference. After focused review and commit, the
Task composes only required `route list` behavior as its first command plug-in,
while route source/depth, topology, finite status conditions, payload, and
renderers remain command-local. No custom operators, user-defined conversions,
duplicated option identities, second raw parser, universal domain engine, or
generic recovery mechanism is accepted; future command plans, effects, and
recovery remain local. The workspace-wide C# Directive applies whenever C# source or tests are authored or
reviewed. Its Blue pass is a production-only creative structure review, and its
Purple pass owns the three-project test transition and justified shared real-OS
temporary-workspace support without changing expectations. Dedicated phase
specialists are optional. Gray, Red, and Green remain the truthful inseparable
`33015e1` commit because the phase-commit rule arrived after their worktree states
were combined. For Blue, global foundation, route reintroduction, and Purple, the
Mastermind inspects the actual result, runs required verification/review, updates
the authoritative Task in the same coherent commit, and commits each mutating
increment before the next mutates files. A no-change pass carries its evidence
into the next coherent or acceptance commit. The maintainer-selected
[Experimental Development Workflow](../../../skills/use-workflow/references/open-forge/experimental-development.md)
now uses brilliant implementation for callable contracts and unresolved shared
foundations, then the normal max-reasoning Luna `implementer` for closed execution
after the Mastermind records a solid structure and complete packet. The
Mastermind owns inspection, the combined correctness and improvement spotter
pass, evidence, commits, and acceptance; helpers never stage, commit, merge, push,
or accept their own work.

`cb32bf4` commits the global foundation, and `cca0001` commits corrected route
reintroduction after its 158-case managed suite, 80 focused route cases,
sequential local `win-x64` production/SystemTests Native AOT publication, 19
published tests, audit, diff checks, and scoped rereviews passed. `8a755da`
commits Purple, which
preserves the 117 existing test declarations across exactly UnitTests,
IntegrationTests, and EndToEndTests, with seven focused tests for the shared
source-only workspace primitive. The managed projects report 117, 49, and 18
passing cases; sequential local `win-x64` production and test-executable Native
AOT publication plus direct 49-case and 18-case execution pass. The production
diff is limited to accepted test-access identities. Its first public scenario and
full local gate passed.
That evidence predates the accepted refinement and is superseded until repeated
against the corrected commit.

A later maintainer-supplied independent static review found that route-list is a
viable base to continue but is not accepted. The active Task now owns one complete
maintainer-directed refinement for diagnostics, help, selection, Loader,
component-wise physical containment, cancellation, typed read causes, human
parity, and missing evidence. The same refinement adopts explicit narrow-owner
`Shared/` source hierarchy, direct SDK-style `.slnx` project membership, default
authored-source globs, and centralized SDK artifacts under `dist/dotnet/`, while
retaining one managed BCL-first Native AOT project and no custom native or
platform source. Route-list remains phase 1 and blocks phase 2 until its repeated
public/full gate, targeted reviews, and Acceptance pass.

The refinement's Gray callable-production-contract increment now adds unwired
typed diagnostics, canonical selection factories, typed read causes, and
structured physical-containment results. Warning-free Release build, all three
existing managed evidence projects, and targeted correctness and improvement
reviews pass. Complete affected Red starts from that exact Gray commit; no runtime
behavior or public output is accepted from Gray itself.

Complete affected refinement Red now contributes 49 declarations and 85 focused
cases across the exact three test projects. Forty Gray/regression cases pass and
45 fail only on accepted missing behavior; all historical 117 Unit, 49
Integration, and 18 EndToEnd cases remain passing, with zero target-Windows skips.
Formatting, warning-free Release build, canonical traits, real-OS fixture
ownership, built-process environment, and diff checks pass. Green starts from the
coherent Red commit with production and expectations frozen and uses the normal
max-reasoning Luna implementer under the experimental Workflow.

### Durable implementation sequence

The sequence below is the default command-by-command plan. It orders cheap
read-only identity and content evidence before mutation, normally introduces each
shared capability through a real consumer, and records the accepted route-free
global-shell increment as the explicit prerequisite to the first retained
command. It keeps aggregate diagnosis after its facts exist and leaves deletion
cleanup until every artifact producer is known. A
Task may move only when its read-only Preflight records concrete dependency
evidence and preserves the same safety and no-partial-publication boundaries.

| Order | Task or command     | Dependency and benefit                                                                                              |
| ----: | ------------------- | ------------------------------------------------------------------------------------------------------------------- |
|     0 | Foundation          | Stable .NET 10, exact direct dependencies, typed/AOT foundations, and Windows evidence                              |
|     1 | Global CLI shell    | Parser-owned global state, centralized definitions, global YAML, typed/testable stages, and explicit process output |
|     2 | `route list`        | First closed command/renderer plug-in, authored topology, source identity, and no-write process evidence            |
|     3 | `route inspect`     | Loading, inheritance, overwrite, and one-route provenance                                                           |
|     4 | `find`              | Complete source inventory, metadata/content parsing, coordinates, and predicates                                    |
|     5 | `references`        | Direct link parsing, containment, target identity, and incoming/outgoing coverage                                   |
|     6 | `context`           | Invocation-local closure graph, loading, links, and exact content projection                                        |
|     7 | `extension list`    | Read-only catalogue and installed/available lifecycle facts                                                         |
|     8 | `extension inspect` | Exact package identity, dependency closure, trust, and three-way comparison                                         |
|     9 | `index`             | First mutation: generated projection, lock, plan, dry-run, patch, and recovery                                      |
|    10 | `route init`        | Bounded route-chain creation using proven projection and mutation safety                                            |
|    11 | `route create`      | One routed source, metadata, Template isolation, and parent projection                                              |
|    12 | `route update`      | Lossless metadata patching and coalesced generated effects                                                          |
|    13 | `route move`        | Complete unmanaged structural inventory, reference safety, and guarded recovery                                     |
|    14 | `route remove`      | Complete absence, incoming-reference detachment, and bounded deletion                                               |
|    15 | `extension create`  | Distinct no-workspace catalogue mutation and destination recovery                                                   |
|    16 | `install`           | Framework ownership establishment, embedded payload identity, and lifecycle writing                                 |
|    17 | `update`            | Trusted Framework reconciliation with force/prune and preservation                                                  |
|    18 | `extension install` | Dependency-first Extension ownership and lifecycle publication                                                      |
|    19 | `extension update`  | Trusted Extension reconciliation, shared owners, force/prune, and recovery                                          |
|    20 | `extension remove`  | Ownership release, dependency protection, retention, and same-request prune                                         |
|    21 | `status`            | Aggregate workspace, lifecycle, Extension, recovery, and availability facts                                         |
|    22 | `doctor`            | Complete cross-domain diagnosis after every fact provider exists                                                    |
|    23 | `repair`            | Fresh-diagnosis-gated correction using proven mutation and recovery capabilities                                    |
|    24 | `cleanup`           | Final provenance-guarded deletion after every recovery-artifact producer exists                                     |

Before order 10 is finalized, resolve deferred CLI-D105: whether managed scoped
Framework route instances extend `route init` through an explicit
`route init <concrete-route> --framework` mode with sparse canonical-chain
alignment and later lifecycle ownership. That decision does not change the
current read-only order or authorize implementation early.

After command 24:

1. **Thin npm packages:** package the canonical executable through the launcher
   and six platform packages. Wrappers never implement command behavior.
2. **CI and main-only release proof:** prove build, test, Native AOT, package,
   support-floor, supply-chain, and release journeys. Publication runs only from
   `main`.

Return to Architecture if the foundation or implementation materially fails an
accepted assumption. The replacement cannot ship until the complete retained
command surface and its complete release proof are accepted. There is no
partial publication.

## Historical validation

The archived [Gate 1 audit and registers](../../archived/cli-release/gate-1-audit.md), [contract migration ledger](../../archived/cli-release/contract-migration-ledger.md), and [settled review records](../../archived/cli-release/review/gate-2-review-overview.md) retain prior validation detail. Historical checks established prose, routing, links, generated-navigation, body-loading, and diff consistency for their scopes. They did not establish implementation, dependency, parser, filesystem, Native AOT, package, CI, or release evidence.

## Stop conditions

Stop for maintainer judgment when work encounters an unaccepted product or
Framework-semantic boundary, a Rune expansion, a safety bypass, a new package
channel, a platform promise outside the Architecture, evidence that invalidates
an accepted decision, generated changes outside scope, or an inseparable
pre-existing change.

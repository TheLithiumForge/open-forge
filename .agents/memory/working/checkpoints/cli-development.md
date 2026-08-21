---
open-forge:
  description: Current state and next action for the greenfield replacement CLI development program
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Architecture, Plan, Task, Contextual]
---

# CLI Development Checkpoint

## Goal

Build the complete replacement CLI from an accepted top-down architecture and a
detailed hierarchical Task set, then deliver every retained command and release
boundary without restoring the removed local architecture.

## Current State

- The final removed implementation is preserved at Git commit `4b873de`.
- Architecture and delegation governance is committed at `aa7d178`.
- The old production tree and root C# workspace were removed at `40ba03e`.
- Candidate tests are quarantined under
  `src/cli/tests/preserved/route-list-v1/` and are not active projects.
- The complete greenfield CLI Architecture, active Plan, and 57-file hierarchical
  Task set are authored.
- The scoped .NET 10 workspace, six-project graph, exact package graph, and
  artifacts routing exist entirely below `src/cli/`; restore and topology checks
  pass.
- The command-free Core and thin root host are committed at `2662f50`; active
  Foundation evidence and six-RID CI scaffolding are committed at `7a601cc`.
- Foundation acceptance passed with 33 Unit, 20 Integration, and 4 EndToEnd cases,
  current `win-x64` Native AOT root and test executable runs, a clean package
  audit, on-disk boundary audits, and a final independent review with no findings.
- Route-list command-local definitions, binding surface, request, selection, row,
  provenance, finding, coverage, result, operation call surface, preserved-test
  disposition, and complete evidence matrix are frozen. The earlier route-list
  contract increment passed a warning-free build with 46 Unit cases; independent
  code and writing reviews passed.
- The completed route-list selection/Loader increment accepts source-reference,
  exact ID/path, catalogue identity, adjacent overwrite, strict Loader
  region/destination, Loader-root selection, and selection-level physical-alias
  behavior. The complete warning-free Release build, 152 Unit cases, and 59
  Integration cases pass; final correctness and local improvement reviews pass.
- The completed route-list filesystem increment inventories only proven-contained
  sources through deterministic real-OS traversal, strict UTF-8 and generated-YAML
  metadata reads, typed physical/read findings, finite aliases, active cycles, and
  cancellation retention. The complete warning-free Release build, 214 Unit cases,
  and 69 Integration cases pass; final correctness and local improvement reviews
  pass.
- The completed route-list topology increment forms immutable authored route
  relationships, selected finite or complete depth, canonical rows, coverage,
  status, findings, and next actions without filesystem access. The warning-free
  Release build, 243 Unit cases, and 80 Integration cases pass.
- The completed route-list presentation increment adds route-list request binding,
  typed invalid depth/workspace results, one inventory/selection/topology/result
  operation coordinator, compact and expanded human output, a dedicated
  source-generated JSON DTO projection, bounded verbose diagnostics, route/group/list
  help, root composition, and managed plus published-process evidence. JSON follows
  the contract with a top-level envelope and result fields `selection`,
  `requestedDepth`, `effectiveDepth`, `coverage`, `findings`, and `rows`; finite
  depths are numbers and `all` is a string.
- The route-list slice is accepted for local development. Its presentation
  implementation and local acceptance evidence are complete. The accepted state
  is included in the coherent closeout commit; no commit or squash hash is claimed
  here. Verification includes the warning-free Release solution build; 259 Unit,
  84 Integration, and 7 EndToEnd cases, all passed with no skip; the
  package/vulnerability audit, formatting, scoped-restore, and `git diff --check`
  checks are clean; the published
  `win-x64` Native AOT root passing the managed EndToEnd suite; native `win-x64`
  Integration 84; native `win-x64` EndToEnd 7; and no-write workspace-hash
  evidence. All executable and code acceptance gates are green. No shared route
  facts were promoted before route inspect.

## Current Step

Execute [Route Inspect](../cli-development/tasks/route-discovery/route-inspect.md) Task decomposition and promotion comparison before production code. Split and close its child Tasks beginning with contracts/evidence and explicit promotion decisions.

## Route-List Closeout Decision

The maintainer's closeout/waiver decision authorized commit/squash integration
after the local route-list evidence. The two legacy-router errors are routed to [CLI-EDGE-001 — Legacy
routing-tool duplicate-entrypoint reports](../cli-development/edge-cases.md#cli-edge-001--legacy-routing-tool-duplicate-entrypoint-reports)
and are not route-list blockers. The accepted state is included in the coherent
closeout commit; no commit or squash hash is claimed here.

## Accepted Decisions

- All replacement-specific C# material is scoped below `src/cli/`.
- The initial physical boundaries are `root/`, `core/`, and `tests/`.
- The project graph contains one executable, one Core library, three runnable test
  projects, and one test-support library.
- The Mastermind authors architecture, cross-cutting callable contracts, and the
  actual route-free foundation.
- Smaller implementers receive only closed Tasks after the relevant foundation is
  accepted.
- Tests remain under `src/cli/tests/`; preserved tests are candidate evidence,
  not architecture authority.

## Blockers

- None for starting route-inspect Task decomposition.
- `CLI-EDGE-002` through `CLI-EDGE-005` are required route-inspect inputs, not
  blockers for decomposition.
- The deferred `route init` Framework-shape blocker remains a later,
  non-blocking decision and does not affect route discovery.

## Resume

Read:

1. [CLI Architecture](../../crystallized/documents/cli/architecture.md)
2. [CLI Development Plan](../cli-development/plan.md)
3. [Route Inspect](../cli-development/tasks/route-discovery/route-inspect.md)
4. [Replacement CLI Edge-Case Ledger](../cli-development/edge-cases.md)
5. [Program Architecture Directive](../../../directives/program-architecture.md)
6. [Architectural Perspectives](../../../guidance/architectural-perspectives.md)
7. The selected parent and active route-inspect Tasks

Then split and close the route-inspect child Tasks before production code,
starting with contracts/evidence and explicit promotion comparisons.

---
open-forge:
  description: Run route-list managed, process, Native AOT, public-scenario, no-write, and architecture acceptance
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Route, List, Acceptance, NativeAOT, Complete]
---

# Accept Route List

## Task State

- State: Complete.
- Implementer: Mastermind.
- Responsible role: Mastermind.
- Parent: [Route Discovery](../_route-discovery.md).

## Integrated Review

Inspect the exact route-list diff from the accepted Foundation, its direct Shell
and Framework consumers, and every adopted test. Confirm command-root locality,
private support paths, no global route policy, no old source copy, no constructor
bypass, and no hidden filesystem or rendering side effect.

## Required Evidence

1. Contract-to-evidence matrix has no missing or unowned guarantee.
2. Focused Unit, Integration, and EndToEnd route-list tests pass independently.
3. Full active test projects pass without warning or required skip.
4. Published root, Integration, and EndToEnd `win-x64` AOT executables pass.
5. Public scenarios cover default roots, exact ID, exact path, depth boundaries,
   JSON, human views, help, verbose diagnostics, failures, cancellation, and exits.
6. Real-workspace scenarios cover every alias and read state, including the
   external-then-contained link chain.
7. Before/after hashes prove route list writes no workspace byte or metadata under
   its control.
8. Package audit, artifact layout, namespace, dependency, old-reference, and diff
   checks pass.

## Spotter Review

Perform a top-down correctness and local-improvement pass. A fresh reviewer may
check the bounded route-list commit if it adds decision value, but Mastermind owns
architecture acceptance. Group all accepted corrections before commit; do not
defer known debt into route inspect.

## Local Acceptance Record

All executable and code acceptance gates are green. The local route-list
acceptance evidence is:

- The Release build is warning-free. 259 Unit tests, 84 Integration tests, and 7
  EndToEnd tests all passed with no skip.
- Format verification and `git diff --check` are clean. Scoped restore is clean.
  `dotnet package list --project OpenForge.Cli.slnx --vulnerable --include-transitive --format json --no-restore`
  reported all six active projects and no vulnerable package.
- The published `win-x64` Native AOT root passed the managed EndToEnd suite. The
  published native `win-x64` Integration executable passed all 84 tests, and the
  published native `win-x64` EndToEnd executable passed all 7 tests.
- Public EndToEnd route-list journeys now cover root, group, and leaf help;
  operand-free Loader roots, including workspace-defined roots; exact ID and
  exact path; depth 0, default 1, and all; compact and expanded human views; the
  JSON view no-op; routed native source; overwrite identity; detached route;
  verbose stream isolation; complete, attention, incomplete, and invalid
  results; attached-empty depth; process cancellation; fixed exits; and no-write
  hashes.
- Unit evidence renders all seven statuses and freezes the version-1 JSON field
  graph and order. Integration covers real Loader, selection, filesystem, and
  topology states; aliases, cycles, and external-then-reentry; exact finite and
  all depths; empty roots; metadata and read failures; and real mid-read
  interrupted result presentation.
- Acceptance corrections were found and verified: JSON/human `tags` and fixed
  JSON order align to the Interface; non-complete findings and next precede rows;
  pipeline cancellation preserves an already-formed typed result while still
  rejecting pre-cancelled operation start; and command binding receives bounded
  raw argument evidence so `--depth= --json` remains a typed JSON invalid-depth
  result instead of consuming `--json` as the depth value.
- Architecture and diff audits found route behavior local under
  `Commands/Route/List`, rendering free of filesystem access, source-generated
  serialization with reflection disabled, no active old namespace or path
  references, the root limited to host and composition, no project-local
  `bin/obj`, intended project references, and no route fact promotion yet. Route
  inspect has not proved shared ownership.

## Pass Condition

Commit one complete accepted route-list slice and update parent Task, Plan,
Checkpoint, Architecture promotion notes, and dogfooding evidence. Only then may
route inspect start. No shared route facts were promoted before route inspect.

## Closeout Decision

The maintainer authorized commit/squash integration. The two legacy-router
errors are routed to [CLI-EDGE-001 — Legacy routing-tool duplicate-entrypoint
reports](../../../edge-cases.md#cli-edge-001--legacy-routing-tool-duplicate-entrypoint-reports)
in the active edge-case ledger. No route-list acceptance blocker remains. This
accepted state is included in the coherent closeout commit; no commit or squash
hash is claimed here.

## Failure Routing

Return a failure to the earliest invalidated model, selection, filesystem,
topology, presentation, shared foundation, or contract. Do not patch a later
renderer or test to hide an earlier fact defect.

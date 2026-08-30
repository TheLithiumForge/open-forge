---
open-forge:
  description: Active Overseer continuity for the replacement CLI task graph, decisions, agents, worktrees, and observations
  tags: [Memory, Working, Contextual, Active, KeepInMind, CLI, Overseer, Orchestration, Decision, Evidence]
---

# CLI Overseer Memory

## Purpose

Keep the compact project-level state needed to resume and coordinate replacement
CLI development without retaining complete child transcripts. Accepted product
meaning remains in Crystallized contracts and architecture. The Plan, Tasks, and
Checkpoint remain authoritative for execution state and evidence; this record
retains only the live orchestration graph, decision frontier, and observations
that the Overseer must carry across parallel lanes.

## Maintainer Authority

- The maintainer alone accepts architectural decisions and feature additions or
  removals. Agents may identify contradictions, alternatives, risks, and
  recommendations, but must stop before changing accepted product meaning.
- Use proportionate native C# and existing Framework capabilities. A workaround,
  general engine, speculative abstraction, or materially stronger safety model
  is a stop condition for discussion.
- Do not push. Keep implementation in isolated feature branches and worktrees;
  local integration occurs only after review and accepted evidence.
- Questions and status discussions do not pause the program. Continue every
  unaffected lane until the maintainer explicitly requests a halt; pause only
  the exact boundary that requires an unresolved maintainer decision.
- Create every new worktree in the designated `open-forge-worktree` directory
  and record its feature-branch name. Existing registered worktrees retain
  their current paths unless a separate safe migration is deliberately
  accepted.

## Current Horizon

The accepted dependency order is:

1. The D0 contract freeze and all four shared foundations are integrated and
   accepted at the current local baseline.
2. Continue Extension Create and root Install on their isolated command paths.
   Route Inspect interactive selection is complete in reviewed integration
   candidate `d9f1f350`; integrate it locally before dependent use. Keep remaining
   shared root composition, serialization, help, and executable evidence
   sequential.
3. Implement Framework-aware Route Init after root Install establishes the
   trusted base lifecycle, then complete the remaining Route Mutation M2 leaves.
4. Implement root Update M3 only after full M2 is complete. Preparation that is
   independent of unfinished behavior may proceed earlier in isolated lanes:
   scope discovery, contract and ownership audits, callable-surface analysis,
   Gray/Red readiness, and worktree preparation. Parallelize those processes;
   do not implement dependent command behavior out of order.

Root Install owns the closed base Framework installation. Route Init owns
concrete scoped route initialization and reuses the neutral embedded payload and
topology capability. It does not become `install --route`, a blueprint engine,
or a general template/scaffold system.

## Active Lanes

| Lane | Responsibility | State |
| --- | --- | --- |
| D0 | Contract, architecture, Plan, Task, checkpoint, and public-doc freeze | Integrated at `38e1498` |
| F1 | Native Shell question/answer transport and invocation capability | Integrated at `e782090` after review |
| F2 | Embedded Framework payload reader and deterministic inventory | Integrated at `680915a` after review |
| F3 | Framework lifecycle `sourceAssetPath` provenance | Integrated at `0989356` after review |
| F4 | Shared planned directory-creation mutation effect | Integrated at `33913df` after review |
| C1 | Extension Create | Active on branch `codex/extension-create` |
| C2 | Root Install | Ready for command-local work on branch `codex/root-install`; its overlapping root-composition, serialization, help, and process seams are integration-owned and sequential |
| C3 | Route Inspect interactive correction | Complete in reviewed `codex/c3-integration` candidate `d9f1f350`; pending local `develop` integration |
| C4 | Generic and Framework-aware Route Init | Waiting for integrated Install |
| M2 preparation | Init/Create/Update/Move/Remove readiness | Complete on clean no-op branches from `33913dfe`; leaf Tasks retain accepted preparation decisions and remaining authority gates; no implementation commit exists |

Each mutating lane owns a distinct worktree and feature branch. Shared root
composition, serializer registration, public help, process evidence, Plan, and
Checkpoint are integration-owned unless a Task packet explicitly says otherwise.

## Accepted Observations

- The D0 contract freeze is integrated at `38e1498`. F1, F2, F3, and F4 are
  integrated at `e782090`, `680915a`, `0989356`, and `33913df`, respectively.
  The combined reviewed baseline has a Release build with `0` warnings and `0`
  errors; managed Unit `1284/1284`, Integration `500/500`, and EndToEnd
  `125/125`; Native AOT Integration `500/500` and EndToEnd `125/125`; and zero
  skips in every stated run.
- C3 Route Inspect interaction is complete at reviewed integration candidate
  `d9f1f350` over `c3642df1`. Root owns Console and redirection facts; Core owns
  the interaction transport and Route Inspect policy. Focused Unit
  `131/131` plus Shell interaction `8/8`, Integration `82/82`, published
  EndToEnd `32/32`, full managed `1284/511/125`, and local `linux-x64` Native
  AOT `511/125` pass with zero skips. Independent review and docs-only rebase
  recheck pass. Direct composition proves interactive behavior; published
  redirected-human and JSON flows prove no prompt. Actual PTY process proof is
  explicitly outside the accepted evidence scope, and no workaround was added.
- The accepted lock bootstrap outcome is nullable. `null` means no bootstrap
  directory outcome was successfully observed; `Existing` means the existing
  `.agents` directory was validated; `Materialized` means its absence was
  observed, ordinary BCL creation ran, and the resulting directory was
  validated. A reached outcome is retained across acquired, failed, and
  cancelled results, and acquisition requires a non-null outcome. This makes
  no hostile same-user creator-identity claim.

- A switch that names every declared enum member is not closed over unnamed
  runtime numeric values. The direct modern-C# pattern is a clear switch
  expression with every named arm plus a discard arm that throws
  `ArgumentOutOfRangeException`, with evidence for every named mapping and one
  undefined runtime value. Do not add analyzer, generator, union, reflection, or
  warning-suppression machinery merely to claim stronger exhaustiveness.
- Exact-name BCL manifest-resource access is compatible with the project's
  reflection-free behavior boundary. Reflective type discovery, assembly-wide
  behavioral scanning, and reflective serialization remain prohibited.
- An inventory fingerprint cannot identify which historical embedded asset
  produced one concrete scoped target after that asset is removed, renamed, or
  moved. Required nullable per-target `sourceAssetPath` is the smallest complete
  provenance addition; it does not grant Route Remove lifecycle-release
  authority.
- Current Route Remove remains positive-unmanaged-only. A managed scoped target
  blocks it until the maintainer separately accepts release-unit, preservation,
  shared-region, publication-order, and repeat semantics.

## Open Decision Frontier

No currently surfaced decision blocks the shared foundations. C2 command-local
non-wire work is ready, but the exact public Root Install JSON result schema,
including residual values `none`, `retained`, and `unknown`, remains a current
maintainer decision frontier before its protected serialization integration.
M2 preparation also surfaced command-local result/callable frontiers retained in
the Route Mutation Tasks. The maintainer accepted the recorded Create correction,
Init/Update freeze timing, Move lifecycle and neutral-resolver corrections, Init
neutral Framework-layer reuse, and Remove parser projection. Remaining exact
wire/proportionality/ownership/effect choices stay open and must close at the
recorded sequential boundary. The Route Create contract correction is integrated
at `cd01b8a71cec399a17409428835d18f589335623`; its behavior still waits for
Route Init.
New architecture or product questions must still be returned to the maintainer
before changing accepted meaning.

## Accepted Interaction Placement

Keep `CliInvocation` and semantic requests free of streams and context objects.
Root composition injects `CliInteractiveSession` only into prompt-capable
operations or their factories. Each relevant binder records only an explicit
command-local policy fact such as `AllowInteractiveSourceSelection`. Generic
binding, unrelated operations, and unrelated requests remain unchanged.

## Accepted Interaction And Creation Policy

- Route Inspect prompts only when standard input and its prompt stream are both
  terminal-capable. One answer may be a one-based candidate number or the exact
  displayed path. Invalid input or end-of-input retains the existing blocked
  collision result; cancellation is interrupted. Use ordinary .NET redirection
  facts only, with no terminal framework.
- Root Install prompts once only for a prompt-capable human application that
  would write, after full preflight and before lock/effects. Dry-run, exact
  no-op, `--automatic`, and JSON never prompt. Refusal, end-of-input, and
  cancellation are no-write interrupted results. A non-prompt-capable human
  application that would write requires `--automatic`; omission is invalid with
  direct rerun guidance.
- A shared directory effect plans every exact missing directory, revalidates the
  missing target and physical parent under the existing workspace lease, calls
  ordinary `Directory.CreateDirectory`, and verifies the exact result. Created
  directories remain after later failure; there is no rollback, compensation,
  recovery bundle, P/Invoke, or hostile same-user creator-identity guarantee.
  Keep the effect separate from byte-bearing file changes.
- `.agents` is the single explicit lock-bootstrap directory. An applicable
  Install or generic Route Init plan includes it visibly; `WorkspaceLockManager`
  creates and verifies it immediately before acquiring
  `.agents/open-forge.lock`; shared directory effects create only descendants
  after the lease. Retain and report `.agents` as residual state after a later
  failure. Keep Local Application Data for recovery bundles, not workspace lock
  identity; do not move or duplicate the lock.
- `WorkspaceLockResult.BootstrapOutcome` is nullable across every result state.
  `null` means no directory outcome was successfully observed; `Existing` means
  the pre-existing `.agents` directory was validated; `Materialized` means the
  manager observed absence, attempted ordinary BCL creation, and validated the
  resulting directory. Preserve any reached outcome across acquired, failed,
  and cancelled results; acquired requires a non-null outcome. `Materialized`
  makes no hostile-process creator-identity claim.
- When two consumers require the same semantics and evidence, promote the
  smallest honest shared capability at their nearest common scope. Similarity
  alone does not justify a generic engine; duplicated identical ownership does
  justify a shared step, pipeline stage, function, or module.
- Extension Create derives its default name by splitting the stable ID on `-`,
  uppercasing the first ASCII letter of each segment, and joining with spaces.
  Its default description is `Open Forge Extension package <stable-id>.`.
  Any existing safely resolved catalogue directory is valid, including an empty
  one; unrelated siblings are ignored, and only `<catalogue>/<id>` participates
  in collision and idempotence.
- Extension Create does not promise an exact question count. Its interactive
  wizard asks for every missing required fact, currently the stable ID and
  catalogue path, with concise guidance. Invalid or blank input can be corrected
  locally while input remains available; end-of-input is a no-write invalid
  result and cancellation is interrupted. Optional metadata uses accepted
  defaults unless explicit flags override it. Keep this flow command-local; do
  not create a general retry framework or arbitrary attempt limit.

## Reporting And Closeout

For every direct child and reported descendant, retain its task name, role,
model, reasoning level, owned worktree/branch, result, review, and commit or
blocker in the relevant Task or checkpoint before integration. Prefer compact
outcome-first updates: important current change, exact evidence, decision or
blocker, and next dependency. Do not discard sound work because an agent omitted
a requested self-identification when the actual model and reasoning can be
verified independently.

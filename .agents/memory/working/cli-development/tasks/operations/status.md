---
open-forge:
  description: Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts
  tags: [Memory, Working, CLI, Task, Status, Observation, Contextual]
---

# Task 15: Status

## Task State

- State: Queued after Task 14 “Extension Install”.
- Permanent mapping: Task 15 “Status” in the
  [project control ledger](../../project-control.md).
- Planned progress horizon: phase 0 of 5, milestone 0 of 8. The streamlined
  phases are Preflight, explicit Gray/Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped improvement pass, and acceptance.
- Incremental completeness: the first accepted horizon covers every observable
  producer present in its exact frozen baseline and records that complete typed
  contributor inventory. It does not call absent future commands healthy,
  supported, or silently omitted. Every later producer must extend the explicit
  contributor inventory and pass affected Status evidence before that producer
  is accepted. Final release requires a complete revalidated inventory for all
  retained producers.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/status/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/status/behavior.md).

## Read-Only Preparation Receipt

Read-only Sol/xhigh preparation completed against clean `develop` commit
`328599a006ef206fd82004e778296c2cac2bc10c`, tree
`56a24a7a50550702eae13bcbfaed9e9ead2a19f3`, without edits, builds, tests,
artifacts, activation, or consumption of Task 14's mutable worktree. It verified
that no Status command or operational contributor catalogue exists yet and
inventoried the current workspace, context/source, embedded Framework,
lifecycle/Extension, generated-navigation, and recovery producers and evidence.

The smallest neutral forecast is `Framework/OperationalContributors/**` for one
immutable application-scoped catalogue and its narrow typed view contracts,
with each concrete contributor remaining beside its producer and Status-only
joining, ordering, availability, findings, and rendering remaining under
`Commands/Status/**`. This forecast does not freeze signatures or authorize
source changes.

After Task 14 integrates, Preflight must refresh the producer inventory and
resolve three Gray authority gaps before Red: the exact command-local JSON
result graph, the generated-state representation, and confirmation that
"current target" means lifecycle-managed current/changed/missing facts rather
than recovery-derived target inspection. Task 15 remains queued at phase 0/5,
milestone 0/8 until that revalidation and activation occur.

## Expected Outcome

`status` reports complete typed current facts for workspace, Framework and
Extension lifecycle, managed content, generated navigation, and external
recovery bundles without diagnosis, recommendation, repair, cleanup, or
mutation. It does not inspect or report repository state.

## Architecture

- Keep definitions, binding, request, operation, result, rows, findings,
  renderers, and help at `Commands/Status/`.
- Consume the same readers and fact models used by real producers. Do not copy
  lifecycle, generated, recovery, or ownership parsing.
- Create local `Shared/{Aggregation,Rendering}/` support for status-specific
  joining, ordering, availability, and projections.
- Preserve domain provenance so Doctor can later consume typed facts without
  parsing status output.

## Requirements

Implement accepted workspace/no-workspace behavior, lifecycle sections,
installed identities, drift/missing/changed states, generated state, recovery
bundle and current-target states, availability, deterministic order,
compact/expanded/JSON, diagnostics, help, status precedence, and next actions.
Distinguish zero, absent, unavailable, unmanaged, not applicable, incomplete,
and unsafe. Recognized bundles use the strict external schema and static
prior/intended fingerprints; Status never extracts, restores, or rebinds them.

## Evidence

Unit fixed-fact aggregation and renderer tests; Integration fixtures produced by
actual install/update/extension/index/mutation paths; malformed and unknown
bundle state; read failures; unchanged snapshots; complete process scenarios;
AOT; and regressions for every producer.

## Stop Conditions

Stop before diagnosing causes, assigning health grades, recommending changes,
planning effects, or maintaining a duplicate workspace database.

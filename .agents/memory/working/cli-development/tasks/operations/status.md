---
open-forge:
  description: Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts
  tags: [Memory, Working, CLI, Task, Status, Observation, Contextual]
---

# Implement Status

## Task State

- State: Planned after every lifecycle and mutation producer.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/status/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/status/behavior.md).

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

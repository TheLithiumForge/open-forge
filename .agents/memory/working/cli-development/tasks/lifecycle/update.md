---
open-forge:
  description: Implement root Framework update from accepted lifecycle identity
  tags: [Memory, Working, CLI, Task, Update, Framework, Lifecycle, Contextual]
---

# Task 6: Root Update

## Task State

- State: Planned after the complete Route Mutation M2 lane, which itself follows
  Install. Independent scope, contract, ownership, Gray/Red-readiness, and
  worktree preparation may occur earlier; dependent command behavior may not.
- Permanent mapping: Task 6 “Root Update” in the
  [project control ledger](../../project-control.md).
- Status/Doctor obligation: extend the explicit typed contributor inventory and
  affected evidence before Task acceptance.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/update/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/update/behavior.md).

## Expected Outcome

Root `update` reconciles only lifecycle-managed Framework files and regions from
their recorded identity to the accepted embedded payload while preserving
workspace-owned content and producing one verified external recovery bundle
covering every existing target it replaces or deletes.

## Architecture

- Share payload identity, managed-file facts, lifecycle parsing, and ownership
  comparison with Install under the nearest root lifecycle capability.
- Use each target's concrete path, nullable `sourceAssetPath`, region/generated
  identity, and baseline fingerprint. Accept structurally valid historical source
  paths so current inventory absence can prove retirement; validate newly
  published non-null paths against the current inventory.
- Keep update selection, drift classification, conflict policy, plan, ordering,
  findings, result, and renderers local.
- Separate observed current bytes, expected prior managed identity, intended new
  bytes, user divergence, and application receipts.

## Evidence

Cover current/no-op, version change, managed drift, user-owned surrounding text,
missing managed file, unexpected replacement, lifecycle missing/malformed/unknown,
historical retired source assets, generated-region `null`, legacy ordinary files,
dry run, lock race, bundle preparation and retention on
partial failure/cancellation, generated navigation, idempotence, process,
packaged payload, and AOT.

## Stop Conditions

Stop before overwriting unrecognized divergence, guessing prior identity,
migrating legacy state, applying without recorded lifecycle, or treating embedded
payload version alone as proof of current workspace bytes.

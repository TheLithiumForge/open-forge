---
open-forge:
  description: Implement root Framework update from accepted lifecycle identity
  tags: [Memory, Working, CLI, Task, Update, Framework, Lifecycle, Contextual]
---

# Implement Root Update

## Task State

- State: Planned after Install.
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
- Keep update selection, drift classification, conflict policy, plan, ordering,
  findings, result, and renderers local.
- Separate observed current bytes, expected prior managed identity, intended new
  bytes, user divergence, and application receipts.

## Evidence

Cover current/no-op, version change, managed drift, user-owned surrounding text,
missing managed file, unexpected replacement, lifecycle missing/malformed/unknown,
legacy ordinary files, dry run, lock race, bundle preparation and retention on
partial failure/cancellation, generated navigation, idempotence, process,
packaged payload, and AOT.

## Stop Conditions

Stop before overwriting unrecognized divergence, guessing prior identity,
migrating legacy state, applying without recorded lifecycle, or treating embedded
payload version alone as proof of current workspace bytes.

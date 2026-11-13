---
open-forge:
  description: Define cheap Run checks and complete Status hashing
  tags: [Memory, Document, CLI, Contract, Tool, Run, CurrentTruth]
---

# tool run behavior

`tool run` is a read and dispatch operation that starts only a verified declared tool pin.

A copied workspace declaration does not establish locally installed bytes.

Status: V1 contract decided. Implementation pending.

## Flow

The operation follows these steps:

1. Resolve the workspace and validate the tool ID.
2. Require matching settings, lock and account-local store records for its exact pin.
3. Check recorded file sizes and timestamps without creating a store, journal or lease. Changed or missing facts block execution. Tool Status performs full re-hashing.
4. Resolve the recorded executable and fixed launcher inputs from the verified store. Never search PATH for the tool or accept another executable.
5. Forward the remaining argument array and process streams with no shell and no successful-run output.

A resolved launch keeps the selected workspace alongside its dispatch request. A start failure or cancellation before process creation carries that workspace into its typed failure. An Install offer uses the workspace from the refusal. It never repeats workspace discovery during consent.

## Missing tools

Tool Status, workspace Status, Doctor and Run report declared missing tools with `open-forge tool install`. Interactive Run can offer that installation, but applies the same artifact, component, destination and final consent checks.

The [interface](interface.md) defines caller behavior. The [dated decisions](../../../technical-designs/external-tools.md#decisions-2026-11-13) keep the source history.

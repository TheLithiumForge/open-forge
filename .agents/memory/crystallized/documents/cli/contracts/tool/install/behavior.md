---
open-forge:
  description: Define Tool install checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install behavior

Tool install behavior is the flow that installs tools and runs their native setup.
A teammate runs bare Install and receives the exact CodeGraph version in the lock. Named Install selects latest support.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Flow

Preflight every requested tool before any effects. Choose a usable channel and manager, resolve the exact version and show the plan. Obtain confirmation or --yes. Install missing or replacement bytes without creating a second install. Verify the version, run native setup, add configuration and publish exact lock receipts. Re-running runs native reconfiguration and checks each recorded addition. Stop at the first failure after effects start.
Use the [common Behavior](../behavior.md) for detection, channels, native setup and additive effects.
Keep syntax and reports in the [Interface](interface.md).

## Results

Use the common status precedence and shared schema-3 exits.
Keep actual effects, conflicts and unstarted work separate.
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- Exact team restore, fresh latest, range and exact specs, client selection, --yes consent, missing TTY, native setup, additive no-op and partial completion.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.

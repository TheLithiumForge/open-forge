---
open-forge:
  description: Define Tool status checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool status behavior

Tool status behavior is the flow that checks versions and additive configuration.
A missing declared CodeGraph produces bare Install as the next step without creating Tool state.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Flow

Use bounded native probes. CodeGraph's version probe sets CODEGRAPH_NO_DOWNLOAD=1. Compare additions with the lock and native previous/applied state with machine records. Missing declared tools offer `open-forge tool install`. A supported self-update offers named Install to reapply. An unsupported one warns without downgrading.
Use the [common Behavior](../behavior.md) for detection, channels, native setup and additive effects.
Keep syntax and reports in the [Interface](interface.md).

## Results

Use the common status precedence and shared schema-3 exits.
Keep actual effects, conflicts and unstarted work separate.
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- Self-updates inside and outside support, missing additions, changed managed files, missing native machine facts and no Tool state writes.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.

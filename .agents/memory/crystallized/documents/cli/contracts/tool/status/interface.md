---
open-forge:
  description: Check tool versions, configuration and launch readiness
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool status interface

Tool status is a command that checks versions and additive configuration.
A missing declared CodeGraph produces bare Install as the next step without creating Tool state.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Syntax

Proposed:

```text
open-forge tool status [<id>] [global flags]
```

With an ID, check that known tool. Otherwise check every declared tool in ordinal order. An empty declaration set is a no-op.
The help purpose is "Check tool versions, configuration and launch readiness."

## Report

Report declaration, exact lock version, detected version, channel, bin and manager-global-bin readiness, runtime and tool paths, additive config drift and native-setting state. Use distinct missing, unsupported, changed, current and unavailable facts.
The [common Interface](../interface.md) defines consent, repetition and result coordinates.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool status
```

A missing declared CodeGraph produces bare Install as the next step without creating Tool state.

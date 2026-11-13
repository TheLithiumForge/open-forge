---
open-forge:
  description: Check tool versions, configuration and launch readiness
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool status interface

Tool status is a command that checks tool versions, configuration and launch readiness.
A teammate with a declared but missing CodeGraph gets bare Install as the next step, with no files written.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Syntax

Proposed:

```text
open-forge tool status [<id>] [global flags]
```

With an ID, check that known tool. Without an ID, check every declared tool. An empty declaration set is a successful no-op.
The help purpose is "Check tool versions, configuration and launch readiness."

## Report

Show declaration, supported range, lock baseline, detected exact version, channel, bin, PATH readiness, runtime and tool paths, config drift and Skill and managed-line state. Include separate missing, unsupported, changed, current and unavailable facts.
The [common Interface](../interface.md) defines report structure, repetition, result meanings and consent.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool status
```

A teammate with a declared but missing CodeGraph gets bare Install as the next step, with no files written.

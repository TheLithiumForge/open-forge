---
open-forge:
  description: List catalogue tools and project declarations
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list interface

Tool list is a command that lists catalogue tools and declarations.
CodeGraph and Codebase Memory appear in ID order. Rune has no V1 row.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Syntax

Proposed:

```text
open-forge tool list [global flags]
```

No operands or local flags. It can show the catalogue without an installed workspace.
The help purpose is "List catalogue tools and project declarations."

## Report

Report ID, name, origin, upstream, licence, channels, supported profiles and declaration state.
The [common Interface](../interface.md) defines consent, repetition and result coordinates.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool list
```

CodeGraph and Codebase Memory appear in ID order. Rune has no V1 row.

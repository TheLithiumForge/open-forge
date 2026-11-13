---
open-forge:
  description: List catalogue tools and project declarations
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list interface

Tool list is a command that lists catalogue tools and project declarations.
List returns CodeGraph and Codebase Memory in ID order. Rune has no V1 row.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Syntax

Proposed:

```text
open-forge tool list [global flags]
```

List has no operands or local flags. It can show the catalogue without an installed workspace.
The help purpose is "List catalogue tools and project declarations."

## Report

Show ID, name, first-party or third-party origin, upstream, licence, channels, supported ranges and whether the workspace declares the tool.
The [common Interface](../interface.md) defines report structure, repetition, result meanings and consent.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool list
```

List returns CodeGraph and Codebase Memory in ID order. Rune has no V1 row.

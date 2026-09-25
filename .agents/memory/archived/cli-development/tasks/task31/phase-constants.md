---
open-forge:
  description: Completed Task 31 M4 constant-placement subtask and its reusable ownership rule
  tags: [Memory, CLI, Task, Subtask, Contextual, Complete, Archived, Historical]
---

# Task 31 — M4 Constant Placement

## Status

Complete. The behavior-preserving result is retained as an active rule for the
remaining Task 31 work.

## Rule

- A constant used across the CLI belongs in a global constants type.
- A constant owned by one command or module belongs in that command's constants
  type.
- A constant used by one class belongs at the top of that class.

## Measured application

The completed slice composed 88 command-line literals from `CommandIdentity`.
The seven `DiagnosticValueLimit` copies were resolved with the M3 escaper
decision. The report uses one shared schema-3 envelope; schema versioning is no
longer command-local.

The A4 snapshot receipts later in this file retain their original schema and
flag vocabulary as historical evidence. They predate the completed G4 report
migration and do not describe current CLI behavior.

The source evidence is the sealed [Implementation Duplication analysis](../../../../emerging/analysis/cli-experience-audit/implementation-duplication.md);
the current update rule is in [Task 31](../../../../working/cli-development/tasks/task31-implementation-duplication.md).

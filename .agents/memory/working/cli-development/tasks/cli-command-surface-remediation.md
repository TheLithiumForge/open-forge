---
open-forge:
  description: Apply only the complete CLI command-surface audit findings accepted by the maintainer
  tags: [Memory, Working, Contextual, Planned, CLI, Task, Remediation, Architecture, Refactoring, Testing]
---

# Task 21: CLI Command Surface Remediation

## Task State

- State: Conditional after Task 10 “CLI Command Surface Audit”.
- Permanent mapping: Task 21 “CLI Command Surface Remediation” in the
  [project control ledger](../project-control.md).
- Phase and milestone horizon: Deferred until Task 10 closes and the maintainer
  accepts an exact finding set. No finding, implementation scope, or progress is
  inferred before then.
- Planned profile: A dedicated Task Mastermind turns only accepted findings into
  one dependency-aware remediation capsule. The profile, implementation waves,
  review budget, and evidence are selected from the actual accepted findings.

## Expected Outcome

Apply only the Task 10 findings accepted by the maintainer while preserving
accepted CLI behavior, contracts, public schemas, ownership boundaries, and
delivery semantics. Findings remain independently traceable through correction
and verification. Rejected, duplicate, preference-only, and deferred findings
do not enter implementation.

## Boundaries

- Task 10 remains read-only and does not quietly implement its findings.
- This Task receives a fresh exact base after every retained command is accepted.
- Every C# author and reviewer independently reads the complete current C#
  Directive, Design, and Style files and reports fresh SHA-256 fingerprints.
- Tests cover Open Forge-owned behavior only. Third-party libraries, runtimes,
  operating systems, and package managers may be boundaries but are never test
  subjects.
- Shared promotion requires proved responsibility and real consumers. Similar
  files or large file counts alone do not justify a generic framework.

## Activation Boundary

Do not activate before Task 10 records its complete immutable audit, the
maintainer accepts an exact finding set, and the Overseer freezes dependencies,
protected meaning, implementation ownership, and proportional evidence.

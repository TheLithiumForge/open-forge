---
open-forge:
  description: Prepare the behavior-preserving six-command Extension refactor until functional Extension and Library destination work is settled
  tags: [Memory, Working, CLI, Task, Extension, Refactoring, Testing, Contextual, Queued]
---

# Task 26: Extension Internal Consolidation

## Task State

- State: Queued after Tasks 24 and 25 are accepted or explicitly
  declined.
- Permanent mapping: Task 26 “Extension Internal Consolidation” in the
  [project control ledger](../project-control.md).
- Prior input: read-only preparation tip
  `8a153f23dabb05019eae2c15fae51331b9e88335`, tree
  `bafd3d1e3173ad9342bd25a6086330dcfbe5ee16`.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Phase and milestone: not assigned.

## Candidate Boundary

This task owns only the later internal consolidation of Extension Create,
Inspect, Install, List, Update, and Remove after all command and destination
behavior is final. It may introduce one neutral shared Extension foundation and
move detailed public cases to Unit or Integration evidence while retaining
exactly three simple public EndToEnd journeys per command.

The work is differential-locked: public syntax, help, text and JSON bytes,
result shapes, package and lifecycle schemas, ownership, recovery, and command
semantics do not change. Known source-reader and Update behavior candidates
belong to Task 24 review rather than this refactor.

## Activation Conditions

Activation requires completed command work, a fresh accepted post-Task-25
baseline, exact before/after behavior oracles, a shared-fixture ownership plan,
and an Overseer-frozen execution plan under the user’s existing authorization.
Shared foundation, composition, artifacts, and final
convergence remain serialized; command-private evidence and leaf adaptations
may run in parallel after those boundaries freeze.

No behavior change, new command, compatibility machinery, runtime registry,
JavaScript path, remote action, destructive external action, or publication is
authorized.

## Task 24 Acceptance Input

Task 24 completed the source-enumeration correction, current `content/` naming,
consumer permission behavior and eighteen public Extension journeys. Its fresh
review also aligned external Inspect identities and closed selected destination
ancestry/control-file gaps. These are accepted behavior, not remaining refactor
work. Retain their lower-tier regressions and public count. Re-evaluate the
older source-universe/manifest/graph preparation against the final Task 25
baseline before freezing any shared foundation; do not repeat completed work.

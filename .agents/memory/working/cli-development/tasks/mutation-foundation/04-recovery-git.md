---
open-forge:
  description: Implement isolated Git checkpoints, recovery artifacts, interruption boundaries, and crash-oriented evidence
  tags: [Memory, Working, CLI, Task, Mutation, Recovery, Git, Contextual]
---

# Implement Git And Recovery Boundaries

## Task State

- State: Planned after atomic application.
- Parent: [Mutation Foundation](_mutation-foundation.md).

## Expected Outcome

Mutating commands can establish accepted Git checkpoints when applicable, record
exact recovery provenance when changes cannot complete cleanly, and expose typed
recovery state to Status, Doctor, Repair, and Cleanup.

## Components

- `GitRepositoryInspector` discovers repository/root/state through direct process
  execution with argument arrays, explicit working directory, bounded streams,
  cancellation, and typed results.
- `GitCheckpointService` creates only contract-authorized checkpoints and records
  exact commit or stash identity. It never changes global Git configuration.
- `RecoveryArtifactStore` writes owned schema-versioned recovery files through
  atomic primitives and validates their provenance before read or removal.
- `RecoveryStateReader` reports absent, valid, incomplete, mismatched, malformed,
  and unsupported state without applying recovery.

## Rules

- No shell command string. No ambient working directory. No credential capture.
- Dirty-state policy belongs to the command contract and plan.
- Recovery artifacts never replace lifecycle state or imply automatic reversal.
- Interruption after an effect retains receipts and writes only recovery facts that
  can be proven from completed work.
- Tests use isolated Git repositories and local author configuration only.

## Evidence

Integration covers non-repository, clean/dirty states, checkpoint success/failure,
detached and nested repositories where contracted, cancellation, process failure,
recovery absent/valid/mismatched/malformed, partial receipt provenance, and owned
artifact cleanup. Crash-oriented tests terminate an owned process only at explicit
test hooks that cannot ship as product behavior.

## Stop Conditions

Stop before adding a shell, changing user Git config, storing credentials, guessing
completed effects, or creating a generic rollback engine.

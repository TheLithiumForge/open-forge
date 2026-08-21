---
open-forge:
  description: Implement bounded cleanup of recognized disposable and recovery artifacts
  tags: [Memory, Working, CLI, Task, Cleanup, Mutation, Contextual]
---

# Implement Cleanup

## Task State

- State: Planned after Repair and every artifact producer.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/cleanup/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/cleanup/behavior.md).

## Expected Outcome

`cleanup` identifies and removes only contract-recognized disposable, stale, or
resolved recovery artifacts after proving ownership, retention, dependency, and
safety. It preserves authored, managed-current, active-recovery, and unknown files.

## Architecture

- Consume typed artifact identities from every producer. Do not scan by broad
  filename pattern or age alone.
- `CleanupObservation` classifies recognized artifact, provenance, active use,
  retention, and safety.
- `CleanupPlan` lists exact targets, expectations, order, dry-run projection, and
  verification.
- Reuse lock, physical identity, atomic deletion, Git/recovery, and receipts.

## Evidence

Cover no-op, each recognized artifact class, active and unresolved recovery,
unknown lookalikes, symlinks/aliases, retention rules, dry run, lock race,
partial failure/recovery, idempotence, preservation hashes, Doctor/Status after
cleanup, process, and AOT.

## Stop Conditions

Stop before glob-based broad deletion, age-only ownership, following an artifact
link outside containment, deleting unresolved recovery, or cleaning an artifact
whose producer identity is unavailable.

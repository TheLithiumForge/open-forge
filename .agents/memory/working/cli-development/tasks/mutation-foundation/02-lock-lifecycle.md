---
open-forge:
  description: Implement real lock ownership, lifecycle reads and writes, and expected-state revalidation
  tags: [Memory, Working, CLI, Task, Mutation, Lock, Lifecycle, Contextual]
---

# Implement Locking, Lifecycle, And Revalidation

## Task State

- State: Planned after mutation contracts.
- Parent: [Mutation Foundation](_mutation-foundation.md).

## Expected Outcome

Workspace mutations acquire positive OS lock ownership, read and validate exact
lifecycle state, and revalidate every planned precondition after locking and
before the first effect.

## Components

- `WorkspaceLockManager` acquires `.agents/open-forge.lock` with a real exclusive
  OS handle, bounded metadata, cancellation, and deterministic busy/failure
  states. Disposal releases the handle; stale file existence is not ownership.
- `LifecycleStore` strictly reads and source-generates schema version 1, rejects
  malformed/unknown/legacy content, and writes only through atomic application.
- `FileExpectationValidator` checks existence, kind, physical identity, and hash
  from accepted BCL facts.
- `MutationPreflight` validates a command-local plan before lock without effects.
- `MutationRevalidator` repeats only volatile expected-state checks under the lock
  and returns typed mismatches.

## Required Behavior

No workspace effect occurs before lock ownership and successful revalidation.
Cancellation before effects leaves bytes unchanged. Lock contention never deletes
or replaces another actor's lock file. Workspace-free `extension create` cannot
call the workspace lock manager.

Lifecycle writes preserve unrelated accepted sections, reject unknown schema
versions, use exact source/package identities, and never infer from legacy files.

## Evidence

Integration uses separate processes or handles for contention, cancellation,
stale path, marker mismatch, lifecycle absent/valid/malformed/unknown-version,
precondition change between planning and lock, and unchanged snapshots. Published
AOT Integration runs the same lock and lifecycle path.

## Stop Conditions

Stop if lock ownership is inferred from existence, if revalidation can occur after
the first effect, if lifecycle parsing uses reflection, or if a command bypass is
needed.

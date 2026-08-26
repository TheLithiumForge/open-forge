---
open-forge:
  description: Freeze workspace-lock, lifecycle-envelope, mutation-precondition, receipt, and recovery contracts
  tags: [Memory, Working, CLI, Task, Mutation, Contract, Lifecycle, Recovery, Contextual]
---

# Freeze Mutation And Lifecycle Contracts

## Task State

- State: Ready after accepted read-only facts, Extension Inspect, and Generated
  Navigation. These contracts must be accepted before public Index application.
- Responsible role: Mastermind.
- Parent: [Mutation Foundation](_mutation-foundation.md).

## Expected Outcome

Every mutating command can depend on one accepted set of immutable callable
contracts for observation, preconditions, lock ownership, planned changes,
application receipts, lifecycle state, and recovery provenance without delegating
product policy to a generic engine.

## Required Models

Create cohesive contracts under `Core/Framework/`:

- `WorkspaceLockRequest`, `WorkspaceLockLease`, and `WorkspaceLockResult` under
  `Mutation/Locking/`. A lease is positive OS ownership, not file existence.
- `FileExpectation` captures path, expected kind, existence, identity, and content
  hash needed for revalidation.
- `PlannedFileChange` captures create, replace, delete, or generated-region change
  plus expected state and intended bytes. It contains no command-specific reason.
- `FileChangeReceipt` records exact observed before/after identity, bytes/hash,
  effect state, and verification.
- `LifecycleEnvelopeV1`, `FrameworkLifecycleState`, and
  `ExtensionLifecycleState` model only accepted persisted schema.
- `RecoveryProvenance` binds workspace, command, operation ID, target, artifact,
  expected identity, and recovery state.
- `GitCheckpointRequest` and typed result model exact repository state and
  checkpoint outcome without shell text parsing in callers.

## Invariants

- Fact and effect records are immutable and state-valid by factory.
- No record carries parser, renderer, writer, service, or broad command context.
- Commands form their own plan type from these primitives.
- Lifecycle version is explicit; no legacy shape, fallback, or migration field.
- Recovery never promises automatic rollback. It records what is recoverable and
  what actually happened.
- Lock, expectation, receipt, lifecycle, and recovery identities use normalized
  and physically proven workspace paths from Foundation.

## Evidence

Unit tests cover every valid and invalid construction, schema serialization,
unknown versions, missing provenance, impossible receipt states, and workspace-null
boundaries. Integration source-generation tests construct the lifecycle context in
managed and Native AOT execution without writing files yet.

## Stop Conditions

Stop if contracts require one universal command plan/result, hide command ordering,
assume every effect is reversible, or recognize a legacy lifecycle format.

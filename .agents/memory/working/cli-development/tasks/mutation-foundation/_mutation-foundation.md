---
open-forge:
  description: Build and accept locking, lifecycle, planning, application, recovery, and Git foundations before mutations
  tags: [Memory, Working, CLI, Task, Mutation, Lifecycle, Recovery, Git, Contextual]
---

# Mutation Foundation

## Task State

- State: Planned.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisites: Accepted read-only facts and `index`.

## Expected Outcome

Commands can form local immutable plans, acquire the real workspace lock,
revalidate expected state, apply bounded atomic changes, verify resulting identity
and bytes, use accepted Git or recovery mechanisms, and write schema-versioned
lifecycle state without a universal mutation engine.

## Invariants

- Planning performs no mutation.
- Lock acquisition precedes final revalidation and every workspace effect.
- `extension create` remains the only accepted no-workspace mutation exception.
- Shared primitives expose preconditions, changes, receipts, locking, atomic
  replacement, lifecycle storage, and recovery facts. Commands retain policy and
  ordering.
- Failures never claim rollback that did not occur. Recovery provenance binds the
  workspace, command, target, expected identity, artifact, and state.

## Child Tasks

- [ ] [Freeze workspace-lock, lifecycle-envelope, mutation-precondition, receipt, and recovery contracts](01-contracts.md) — Planned — Implementer: Not assigned
- [ ] [Implement real lock ownership, lifecycle reads and writes, and expected-state revalidation](02-lock-lifecycle.md) — Planned — Implementer: Not assigned
- [ ] [Implement atomic file operations, verified application receipts, and command-local plan execution primitives](03-apply.md) — Planned — Implementer: Not assigned
- [ ] [Implement isolated Git checkpoints, recovery artifacts, interruption boundaries, and crash-oriented evidence](04-recovery-git.md) — Planned — Implementer: Not assigned
- [ ] [Integrate and accept the mutation foundation before any mutating command](05-acceptance.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->

- [Freeze workspace-lock, lifecycle-envelope, mutation-precondition, receipt, and recovery contracts](01-contracts.md) - #Memory #Working #CLI #Task #Mutation #Contract #Lifecycle #Recovery #Contextual
- [Implement real lock ownership, lifecycle reads and writes, and expected-state revalidation](02-lock-lifecycle.md) - #Memory #Working #CLI #Task #Mutation #Lock #Lifecycle #Contextual
- [Implement atomic file operations, verified application receipts, and command-local plan execution primitives](03-apply.md) - #Memory #Working #CLI #Task #Mutation #Filesystem #Atomic #Contextual
- [Implement isolated Git checkpoints, recovery artifacts, interruption boundaries, and crash-oriented evidence](04-recovery-git.md) - #Memory #Working #CLI #Task #Mutation #Recovery #Git #Contextual
- [Integrate and accept the mutation foundation before any mutating command](05-acceptance.md) - #Memory #Working #CLI #Task #Mutation #Acceptance #Integration #Contextual

<!-- open-forge:generated-index:end -->

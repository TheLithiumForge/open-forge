---
open-forge:
  description: Implement lease-validated cleanup of recognized recovery bundles and drafts
  tags: [Memory, Working, CLI, Task, Cleanup, Mutation, Contextual]
---

# Task 20: Cleanup

## Task State

- State: Task-locally Active at phase 4 of 5, milestone 3 of 8. Preflight, Gray,
  and Red are accepted. Green is current but held until Task 19 is accepted and
  integrated and the producer/callable boundary is freshly refrozen. In the
  project queue, Task 20 remains Queued after active Task 19.
- Permanent mapping: Task 20 “Cleanup” in the
  [project control ledger](../../project-control.md).
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/cleanup/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/cleanup/behavior.md).

The accepted task-local record tip is
`96ed0aa35b7e092aa78e5bb7249ce71c5598b2c9`, tree
`be13c85b4cb26271c0770b880a094f125c0a7f7c`. Accepted Gray is
`0263ae1e6f63b85b89457c35c28aee29eef1afb3`, tree
`03f0f7c05f7b2d6b935313fd64de71169f7be7e7`; accepted Red is
`e2b7fdc0ef49adaa9fc101e5c9c4e4a603b9807a`, tree
`7922db08761f53c681937144f94ecef9d886679a`. This state claims no Green,
review, correction, or final acceptance.

## Expected Outcome

`cleanup` forms an operand-free, default-all catalogue of positively recognized
recovery bundles and drafts associated with the selected workspace. Before lease
acquisition, candidates are unclassified for activity. An empty catalogue is a
verified no-lease no-op. Any deletion requires holding the same-workspace
persistent `WorkspaceLockLease` through `FileShare.None`, then repeating final
catalogue and expected-state revalidation under that lease. Lease contention
causes no deletion. Authored, managed-current, malformed, mismatched, differently
keyed, unknown, and unsafe files remain untouched.

## Architecture

- Consume typed artifact identities from every producer. Do not scan by broad
  filename pattern or age alone.
- Catalogue facts establish strict bundle or draft recognition, selected-workspace
  association, provenance, expected state, and safety without an activity
  classifier.
- `CleanupPlan` lists exact candidate targets, expectations, order, contingent
  dry-run projection, required lease boundary, and verification.
- Application may return an empty no-op without acquiring a lease. Otherwise it
  acquires the persistent same-workspace `WorkspaceLockLease` through
  `FileShare.None`, rebuilds the complete catalogue, and revalidates all expected
  state while holding that lease before deleting anything. Contention prevents
  every deletion.
- Reuse physical identity, strict recovery-bundle recognition, bounded deletion,
  and typed deletion-result facts. Cleanup's explicit support-artifact exception
  is nonrecursive; it never extracts, restores, rolls back, compensates, or
  creates a replacement bundle. It writes no activity classifier, marker, PID,
  journal, or lock metadata.

## Evidence

Cover empty no-lease no-op, recognized selected-workspace bundles and drafts,
pre-lease activity-unclassified reporting, same-workspace lease acquisition,
contention with no deletion, final under-lease catalogue and expected-state
revalidation, unknown/malformed/mismatched lookalikes, symlinks/aliases, dry run,
partial failure, idempotence, preservation hashes, no activity metadata,
Doctor/Status after cleanup, process, and AOT.

## Stop Conditions

Stop before glob-based broad deletion, age-only ownership, following an artifact
link outside containment, adding an independent activity classifier or activity
metadata, deleting without the held same-workspace lease and final revalidation,
or cleaning an artifact whose producer identity is unavailable.

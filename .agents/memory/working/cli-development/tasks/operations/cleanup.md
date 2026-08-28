---
open-forge:
  description: Implement lease-validated cleanup of recognized recovery bundles and drafts
  tags: [Memory, Working, CLI, Task, Cleanup, Mutation, Contextual]
---

# Implement Cleanup

## Task State

- State: Planned after Repair and every artifact producer.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/cleanup/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/cleanup/behavior.md).

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

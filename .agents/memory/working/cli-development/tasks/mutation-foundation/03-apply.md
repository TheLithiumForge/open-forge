---
open-forge:
  description: Implement atomic file operations, verified application receipts, and command-local plan execution primitives
  tags: [Memory, Working, CLI, Task, Mutation, Filesystem, Atomic, Contextual]
---

# Implement Atomic Application Primitives

## Task State

- State: Planned after lock and revalidation.
- Parent: [Mutation Foundation](_mutation-foundation.md).

## Expected Outcome

Commands can apply an already-validated ordered list of bounded file changes and
receive exact verified receipts. Shared code performs mechanics only; command
plans retain policy, grouping, and sequence.

## Components

- `AtomicFileWriter` writes owned temporary content in the target directory,
  flushes as required, verifies intended bytes, and replaces or creates through
  accepted BCL operations.
- `FileDeletionApplier` deletes only an expected physically proven ordinary file
  or accepted empty directory under command policy.
- `GeneratedRegionApplier` applies an already-formed exact region change without
  recomputing route meaning.
- `FileChangeApplier` dispatches one finite primitive change and returns one
  `FileChangeReceipt`.
- `MutationReceiptVerifier` rereads affected identity and bytes after application.

## Rules

- The caller supplies an ordered command-local plan after lock/revalidation.
- Every primitive rechecks its exact target expectation immediately before effect.
- Temporary artifacts have unpredictable owned names and never collide with
  authored content.
- A failure returns receipts for completed changes and exact uncompleted state. It
  does not claim rollback.
- Permissions, line endings, encoding, timestamps, and replacement semantics
  follow command contracts and platform evidence; no broad preservation promise is
  invented.

## Evidence

Integration covers create, replace, delete, generated region, collision,
precondition mismatch, denied access, I/O failure, cancellation boundaries,
verification mismatch, cleanup of owned temporary files, and preservation of
unrelated bytes. Native AOT executes all accepted primitive paths.

## Stop Conditions

Stop if atomicity differs materially by platform, if safe replacement requires
native interop, or if the applier must understand a command's product policy.

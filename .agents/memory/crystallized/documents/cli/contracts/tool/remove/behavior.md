---
open-forge:
  description: Define receipt-driven tool-owned uninstall and final-reference deletion
  tags: [Memory, Document, CLI, Contract, Tool, Remove, Behavior, CurrentTruth]
---

# tool remove behavior
`tool remove` is a write operation that runs the stored cleanup and uninstall commands before releasing a workspace registration.

A complete plan precedes consent and effects. Preview uses that same plan without transfers, processes, grants or writes.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate one ID and read settings, stored definitions, lock receipts and account-local registration evidence.
2. Distinguish verified prior removal from incomplete or unavailable registration data.
3. Check command output scope, missing content, grants, exclusions, stopped sessions and complete references.
4. Form one complete removal plan with index-cleanup and uninstall commands, ignore changes and paired record release.
5. Resolve grants and final confirmation before effects.
6. Acquire user-store then workspace leases and revalidate the plan.
7. Run the stored index-cleanup command, then uninstall commands in declared scope. A failed or interrupted command stops subsequent commands and keeps registration and execution evidence.
8. Use Git recovery or sibling backups for managed files, remove unused Tool ignore rules and release the registration and roles after successful command verification.
9. Recheck all shared references before deleting recognized unused install or archive bytes. Keep other worktrees usable.
10. Verify leftovers and report tool-owned effects, kept shared artifacts and unfinished work separately.

## Repetition

A verified unchanged request is a no-op without new leases, backups, transfers or child processes. Missing receipts and foreign files don't become a verified installation through adoption. After a partial command failure, keep execution evidence and every unfinished part. A new invocation plans from current facts. It doesn't replay a saved plan or guess an inverse command.

The [interface](interface.md) defines caller input and output.

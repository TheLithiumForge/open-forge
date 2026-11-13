---
open-forge:
  description: Define receipt-driven complete tool removal with checked members and final-reference deletion
  tags: [Memory, Document, CLI, Contract, Tool, Remove, Behavior, CurrentTruth]
---

# tool remove behavior
`tool remove` is a write operation that releases one registration after its recognized artifacts are removed and verified.

It inventories all selected members and runtime output before destructive effects. Unknown content prevents a complete destructive plan.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate one ID and read settings, stored definitions, lock receipts and account-local registration evidence.
2. Distinguish verified prior removal from incomplete or unavailable registration data.
3. Read all selected members, generated files, native settings and shared references without following links.
4. Check stopped sessions, exact generated values, directory membership, grants and recovery space.
5. Form one complete removal plan, then resolve grants and final confirmation.
6. Acquire user-store then workspace leases and revalidate every required fact.
7. Prepare and verify sibling backups before changing or deleting existing files.
8. Remove recognized role members, guidance, worktree state, native settings and managed ignore rules.
9. Verify selected removal, then release the workspace registration and paired records.
10. Recheck all references. Delete recognized shared files only with complete empty references, then delete covered empty directories from children to parents.
11. Verify leftovers and report kept other-registration artifacts and recovery separately.

## Failure

A new child or changed member stops new effects. Restore changed existing files from their sibling backups. Keep a backup if restoration fails and report every unfinished requested part. Delete directories only after their covered children are gone.

The [interface](interface.md) defines caller input and output.

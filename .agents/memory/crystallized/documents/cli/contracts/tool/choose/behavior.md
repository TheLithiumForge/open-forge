---
open-forge:
  description: Define checked provider switches and role clearing without fallback or index changes
  tags: [Memory, Document, CLI, Contract, Tool, Choose, Behavior, CurrentTruth]
---

# tool choose behavior
`tool choose` is a write operation that assigns or clears one role in the workspace settings without editing tool-owned files.

A complete plan precedes consent and effects. Preview uses that same plan without transfers, processes, grants or writes.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate one role and an exact installed tool ID or `none`.
2. Read all role assignments and affected providers' settings and receipts.
3. Validate the target capability, verified pin, root and required process consent.
4. Form the complete paired settings and lock plan. Keep tool-owned host files, guidance, installs and indexes unchanged.
5. Resolve destination grants and final confirmation.
6. Acquire the workspace lease and revalidate the current records.
7. Use Git recovery or sibling backups for the paired records, then publish the single-provider mapping.
8. Verify the role mapping and report the outcome. Choice never invokes a tool installer or restores removed content.

## Repetition

A verified unchanged request is a no-op without new leases, backups, transfers or child processes. Missing receipts and foreign files don't become a verified installation through adoption. After a partial command failure, keep execution evidence and every unfinished part. A new invocation plans from current facts. It doesn't replay a saved plan or guess an inverse command.

The [interface](interface.md) defines caller input and output.

---
open-forge:
  description: Define exact pin replacement with stage-first verification and publish-before-release references
  tags: [Memory, Document, CLI, Contract, Tool, Update, Behavior, CurrentTruth]
---

# tool update behavior
`tool update` is a write operation that verifies a supported replacement and invokes its installer before publishing the new pin.

A complete plan precedes consent and effects. Preview uses that same plan without transfers, processes, grants or writes.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate one registered ID and resolve the chosen exact source pin.
2. Read both definitions, receipts, settings, exclusions, bindings, references and consent.
3. Form one plan for replacement artifacts, installer commands, ignore entries, references and paired records.
4. Resolve separate transfer consent, process consent, grants and final confirmation.
5. Stage and verify approved immutable bytes outside writing leases.
6. Acquire user-store then workspace leases and revalidate all required facts.
7. Run the new pin's declared installer with the recorded hosts and root. Record each result.
8. Use Git recovery or sibling backups for managed files, then publish and verify the new reference and paired records.
9. Release the old immutable reference. Recheck complete references before deleting unused bytes. Don't run an old uninstaller against the new configuration.
10. Verify child shutdown and report rebuild instructions when extraction or version changes invalidate the index.

## Repetition

A verified unchanged request is a no-op without new leases, backups, transfers or child processes. Missing receipts and foreign files don't become a verified installation through adoption. After a partial command failure, keep execution evidence and every unfinished part. A new invocation plans from current facts. It doesn't replay a saved plan or guess an inverse command.

The [interface](interface.md) defines caller input and output.

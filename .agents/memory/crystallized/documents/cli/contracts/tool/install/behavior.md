---
open-forge:
  description: Define staged tool verification, registration and offline reuse without role selection
  tags: [Memory, Document, CLI, Contract, Tool, Install, Behavior, CurrentTruth]
---

# tool install behavior
`tool install` is a write operation that verifies a supported artifact set and runs its pinned installer before registering the workspace.

A complete plan precedes consent and effects. Preview uses that same plan without transfers, processes, grants or writes.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate the complete request and resolve its exact source and pin.
2. Read settings, lock receipts, store bindings, references and process consent.
3. Validate release admission, native target, contained roots, declared command outputs, grants and exclusions.
4. Form all transfers, installer invocations, managed ignore rules, references and control-file changes in one ordered plan.
5. Resolve separate artifact consent, daemon consent, destination grants and final confirmation.
6. For apply, stage and hash approved artifacts outside writing leases. Reject unsafe archive members before extraction or execution.
7. Acquire the user-store lease, then the workspace lease, and revalidate every required fact.
8. Run the declared install command for its reviewed scope. Record each started command and exit result.
9. Prepare managed-file recovery using Git or sibling backups, apply ignore changes and publish matching settings and lock records.
10. Verify registration and child shutdown, then report external index instructions. New registrations have no chosen roles.

## Repetition

A verified unchanged request is a no-op without new leases, backups, transfers or child processes. Missing receipts and foreign files don't become a verified installation through adoption. After a partial command failure, keep execution evidence and every unfinished part. A new invocation plans from current facts. It doesn't replay a saved plan or guess an inverse command.

The [interface](interface.md) defines caller input and output.

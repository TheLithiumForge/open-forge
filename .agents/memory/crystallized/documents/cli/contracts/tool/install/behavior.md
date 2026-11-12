---
open-forge:
  description: Define staged tool verification, registration and offline reuse without role selection
  tags: [Memory, Document, CLI, Contract, Tool, Install, Behavior, CurrentTruth]
---

# tool install behavior
`tool install` is a write operation that verifies one supported artifact set before registering its workspace use.

A complete plan precedes consent and effects. Preview uses that same plan without transfers, processes, grants or writes.

Status: V1 core contract decided. Writer implementation requires recovery v2 confirmation.

## Flow

The operation follows these steps:

1. Validate the complete request and resolve its exact source and pin.
2. Read workspace settings, lock receipts, store bindings, references and process consent.
3. Validate the supported tuple, qualified target, contained roots, exclusions, member claims and free recovery space.
4. Form all transfers, checks, settings actions, guidance, references and control-file changes in one ordered plan.
5. Resolve separate artifact consent, daemon consent, destination grants and final confirmation.
6. For apply, stage and hash approved artifacts outside held writing leases. Reject unsafe archive members before extraction or execution.
7. Acquire the user-store lease, then the workspace lease, and revalidate every required fact.
8. Prepare and read back one immutable recovery v2 bundle before covered effects.
9. Apply bounded configuration and verified shared content, then publish receipts and the settings entry.
10. Verify registration, declared outputs and child shutdown, then report external index instructions and recovery disposition.

## Repetition

A matching verified registration and unchanged configuration is a no-op. It creates no lease or recovery bundle. Existing foreign bytes, mismatched receipts and hand-edited settings never become a verified no-op through adoption.

The [interface](interface.md) defines caller input and output.

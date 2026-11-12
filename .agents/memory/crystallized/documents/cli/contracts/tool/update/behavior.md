---
open-forge:
  description: Define exact pin replacement with stage-first verification and publish-before-release references
  tags: [Memory, Document, CLI, Contract, Tool, Update, Behavior, CurrentTruth]
---

# tool update behavior
`tool update` is a write operation that stages a supported replacement before changing a registered reference.

The old install stays usable until the new reference is published and verified. Shared deletion requires a complete current reference set.

Status: V1 core contract decided. Writer implementation requires recovery v2 confirmation.

## Flow

The operation follows these steps:

1. Validate one registered ID and resolve the chosen exact source pin.
2. Read both definitions, receipts, settings, exclusions, bindings, references and consent.
3. Form one plan for artifacts, compatible settings, active members, required guidance, references and retirement.
4. Resolve separate transfer consent, process consent, grants and final confirmation.
5. Stage and verify approved immutable bytes outside held writing leases.
6. Acquire user-store then workspace leases and revalidate all required facts.
7. Prepare and verify recovery v2 before covered changes.
8. Apply the replacement settings and content, then publish and verify the new reference and paired workspace records.
9. Release the old reference. Recheck the complete reference set before deleting recognized unreferenced bytes.
10. Verify remaining artifacts and child shutdown, then report rebuild instructions and recovery disposition.

## Failure

Stop new effects after a failure. Keep verified changes and the immutable recovery bundle. A new invocation plans from current files. It doesn't resume a saved plan or roll back completed effects.

The [interface](interface.md) defines caller input and output.

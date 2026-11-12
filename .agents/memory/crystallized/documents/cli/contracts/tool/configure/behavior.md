---
open-forge:
  description: Define stored-definition configuration, guidance checks and managed host reconciliation
  tags: [Memory, Document, CLI, Contract, Tool, Configure, Behavior, CurrentTruth]
---

# tool configure behavior
`tool configure` is a write operation that plans changes from the registered pin and its recorded generated values.

Host members, native settings and required guidance remain separate checked effects. A role assignment doesn't override a covering removal exclusion.

Status: V1 core contract decided. Writer implementation requires recovery v2 confirmation.

## Flow

The operation follows these steps:

1. Validate one ID and resolve the recorded pin, hosts and root.
2. Read the stored definition, receipts, grants, exclusions, bindings and process scope.
3. Resolve the complete desired host set and contained code root.
4. Compare managed semantic values and required guidance with their receipts.
5. Plan active host and guidance reconciliation, removed-host release, settings changes and index invalidation facts.
6. Resolve process consent, grants and final confirmation using the complete plan.
7. Under user-store then workspace leases, revalidate files, members, references and process scope.
8. Prepare verified recovery v2, apply each checked effect and publish matching settings and lock records.
9. Verify declared outputs and shutdown, then report external maintenance and host restarts.

## Limits

Configure doesn't resolve another pin, download bytes, choose a role, index or delete an index. A verified unchanged configuration is a no-op without runtime state.

The [interface](interface.md) defines caller input and output.

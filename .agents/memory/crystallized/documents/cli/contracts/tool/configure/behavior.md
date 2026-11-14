---
open-forge:
  description: Define stored-definition installer configuration without downloads
  tags: [Memory, Document, CLI, Contract, Tool, Configure, Behavior, CurrentTruth]
---

# tool configure behavior

`tool configure` is a write operation that runs the registered pin's declared installer with its desired configuration.

A complete plan precedes consent and effects. Preview uses that same plan without transfers, processes, grants or writes.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate one ID and resolve the recorded pin, hosts and root.
2. Read the stored definition, command receipts, grants, exclusions, bindings and process scope.
3. Resolve the complete desired host set and contained code root.
4. Check declared output permissions and exclusions before starting any command.
5. Plan removed-host uninstall commands, desired-host installation, ignore changes and paired records. Workspace-scoped profiles replace their complete host set.
6. Resolve process consent, grants and final confirmation using that complete plan.
7. Under user-store then workspace leases, revalidate files, references and process scope.
8. Run declared configuration commands and record results. Stop after a failed or interrupted command.
9. Use Git recovery or sibling backups for managed files, publish matching settings and lock records and report external rebuild instructions when the root changes.

## Repetition

A verified unchanged request is a no-op without new leases, backups, transfers or child processes. Missing receipts and foreign files don't become a verified installation through adoption. After a partial command failure, keep execution evidence and every unfinished part. A new invocation plans from current facts. It doesn't replay a saved plan or guess an inverse command.

The [interface](interface.md) defines caller input and output.

## Entry changes

Resolve optional-component choices before consent. Keep saved choices when omitted on Configure or Update. A new non-interactive prompt-hook choice requires `--prompt-hook yes|no`. After the tool's installer runs, check its declared originals, remove only declined entries and apply the remaining declared command rewrites. Verify the resulting entries before publishing success. Record expected rewritten values for passive drift checks. The [manifest behavior](../../shared/tool-manifest/behavior.md#entry-rewrites) defines the format limits.

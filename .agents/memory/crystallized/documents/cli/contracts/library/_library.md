---
open-forge:
  description: Route the accepted current Workspace Library list, inspect, attach, sync, and detach contracts
  responsibility: Provide Library group help and route child contracts without defining operation behavior
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Workspace, CurrentTruth]
---

# library Command Group

## Status And Authority

This is the accepted current Crystallized routing entrypoint for the
`open-forge library` command group. The group exposes one stable command order
and does not perform a library operation in its bare form. The child contract
sets define the operation meaning; this entrypoint defines only group routing,
help order, and the shared boundary.

The exact command and help order is:

1. `open-forge library list [global flags]`
2. `open-forge library inspect <library-id> [global flags]`
3. `open-forge library attach <library-id> <source-root> [--to <workspace-relative-directory>] [--dry-run] [global flags]`
4. `open-forge library sync <library-id> [--dry-run] [global flags]`
5. `open-forge library detach <library-id> [--dry-run] [global flags]`

The five child contract sets are routed by this group in the order above.
`list` and `inspect` are read-only; `attach`, `sync`, and `detach` have their
own command-local contracts and mutation boundaries. No additional Library
command or alias is implied by this group.

## Shared Library Boundary

Workspace Libraries register a contained source root for one consumer workspace
and recursively expose eligible source files below a recorded destination root
through individual relative file links. No specially named source child is
required. Optional Attach `--to` defaults to the workspace root (`.`). The source root and the consumer workspace retain separate identities.
The consumer-local record is `.agents/open-forge.libraries.json`.

The record is a strict schema-v1 document. Its top-level members are
`schemaVersion` and `libraries`; each library record has only `id`, `sourceRoot`,
`destinationRoot`, and `paths`. It is separate from `.agents/open-forge.lifecycle.json`. Library
IDs use the lowercase ASCII stable-ID grammar and occupy a namespace separate
from automatic source IDs. A library ID is management identity, never a
source-reference operand.

The [Workspace Libraries Technical Design](../../technical-designs/workspace-libraries.md)
is the shared realization source for this group. It is linked here by its
accepted relative path and is not authored by this contract pack.

Both routed read-only commands use the shared [Global CLI Flags](../shared/global-flags/interface.md),
the shared [Result Coordinates](../shared/result-coordinates/interface.md), and
the shared source-ID derivation in [CLI Source References](../shared/source-references/interface.md).
They do not acquire a lock, create recovery state, or mutate a workspace,
record, source, or link.

## Child Contract Sets

- [`list/`](list/_list.md) reports bounded consumer-record and registered-link
  observations without a complete source inventory.
- [`inspect/`](inspect/_inspect.md) inventories one source root completely and
  compares its eligible projection with the exact registered paths and links.
- [`attach/`](attach/_attach.md) registers one contained source root and creates
  its declared projections under its own mutation contract.
- [`sync/`](sync/_sync.md) reconciles one registered library with a complete
  current source inventory under its own mutation contract.
- [`detach/`](detach/_detach.md) removes one registered library projection
  under its own mutation contract without touching its source.

The group entrypoint does not duplicate any child Interface or Behavior
Contract, and it does not define the mutation commands' plans or effects.

## Axioms

- inherited - No local axioms; loaded ancestor entrypoints remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Route the accepted read-only Library list contracts for bounded record and link observations](list/_list.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #List #Interface #Behavior #ReadOnly #CurrentTruth
- [Route the accepted read-only Library inspect contracts for one exact library ID](inspect/_inspect.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Inspect #Interface #Behavior #ReadOnly #CurrentTruth
- [Route the accepted Library attach contracts for one contained source root](attach/_attach.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Attach #Mutation #Recovery #Safety #CurrentTruth
- [Route the accepted Library sync contracts for one registered source root](sync/_sync.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Sync #Mutation #Recovery #Safety #CurrentTruth
- [Route the accepted Library detach contracts for one registered source root](detach/_detach.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Detach #Mutation #Recovery #Safety #CurrentTruth
<!-- open-forge:generated-index:end -->

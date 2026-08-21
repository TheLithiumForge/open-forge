---
open-forge:
  description: Implement read-only route discovery, beginning with route list and then route inspect
  tags: [Memory, Working, CLI, Task, Route, Discovery, ReadOnly, Contextual, Active]
---

# Route Discovery

## Task State

- State: Active.
- Implementer: Mastermind.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisite: Accepted [Foundation](../foundation/_foundation.md).

## Outcome And Architecture

`route list` and `route inspect` become the first accepted commands. List proves
safe route inventory and deterministic projection. Inspect becomes the second
consumer and promotes only identical source, route, identity, loading, overwrite,
and topology facts to `Commands/Route/Shared/` or `Framework/Routing/`.

Command bindings, requests, results, findings, depth or profile semantics,
renderers, help, and evidence remain local. Neither command imports the other's
private support.

## Common Acceptance

- Complete Interface and Behavior contracts are mapped to evidence.
- Real filesystem identity, Loader roots, aliases, cycles, encoding, cancellation,
  no-write behavior, JSON, human output, help, diagnostics, streams, and exits are
  covered at their correct tiers.
- Each command passes a published Native AOT public scenario before the next
  command begins.

## Child Tasks

- [x] [Freeze route-list contracts, local models, preserved-test disposition, and complete evidence matrix](done/route-list-contracts.md) — Complete — Implementer: Mastermind
- [x] [Implement route-list binding, source identity, exact-path selection, ID selection, and strict Loader destination resolution](done/route-list-selection.md) — Complete — Implementer: Mastermind
- [x] [Implement route-list inventory over shared physical safety with typed reads, aliases, findings, and cancellation retention](done/route-list-filesystem.md) — Complete — Implementer: Mastermind
- [x] [Implement route-list topology, ordering, depth, coverage, status, and next-action formation](done/route-list-topology.md) — Complete — Implementer: Mastermind
- [x] [Implement route-list human, JSON, diagnostic, and binding-derived help projections](done/route-list-presentation.md) — Complete — Implementer: Mastermind
- [x] [Run route-list managed, process, Native AOT, public-scenario, no-write, and architecture acceptance](done/route-list-acceptance.md) — Complete — Implementer: Mastermind
- [ ] [Implement and accept route inspect, then promote only route facts proved identical by both commands](route-inspect.md) — Active — Implementer: Mastermind

## Entries

<!-- open-forge:generated-index:start -->

- [Routes completed leaf Tasks for Route Discovery without changing their historical Task meaning](done/_done.md) - #Memory #Working #CLI #Task #Route #Discovery #Complete #Contextual
- [Run route-list managed, process, Native AOT, public-scenario, no-write, and architecture acceptance](done/route-list-acceptance.md) - #Memory #Working #CLI #Task #Route #List #Acceptance #NativeAOT #Contextual #Complete
- [Implement and accept route inspect, then promote only route facts proved identical by both commands](route-inspect.md) - #Memory #Working #CLI #Task #Route #Inspect #Promotion #Contextual #Active

<!-- open-forge:generated-index:end -->

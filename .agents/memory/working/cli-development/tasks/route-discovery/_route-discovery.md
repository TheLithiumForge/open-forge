---
open-forge:
  description: Implement read-only route discovery, beginning with route list and then route inspect
  tags: [Memory, Working, CLI, Task, Route, Discovery, ReadOnly, Contextual, Complete]
---

# Route Discovery

## Task State

- State: Complete after accepted Route List and Route Inspect slices.
- Post-completion correction: Route Inspect's accepted interactive collision
  selection is Complete and squash-integrated at `fa3db1ee` with exact tree
  equality to final reviewed candidate `37c9360`; completed discovery meaning
  remains otherwise closed.
- Implementer: Mastermind.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisite: Accepted [Foundation](../foundation/_foundation.md).

## Outcome And Architecture

`route list` and `route inspect` are complete. `Framework/Sources/` is the sole
neutral authority for source identity, inventory, selected reading, Loader facts,
topology, and routing/route facts. Route List and Route Inspect retain
command-local projections and policy; neither command creates a competing neutral
authority.

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
- [x] [Implement and accept route inspect, then promote only route facts proved identical by both commands](route-inspect.md) — Complete — Implementer: Mastermind
  - [x] [Freeze route-inspect contracts and evidence](route-inspect-contracts.md) — Complete — Implementer: Mastermind
  - [x] [Resolve route-inspect inputs and audit promotion](done/route-inspect-resolution-promotion.md) — Complete — Implementer: Mastermind
  - [x] [Form the route-inspect profile](route-inspect-profile.md) — Complete at `c407e24`; link-safe routing deferred — Implementer: Mastermind
  - [x] [Present route inspect through the CLI](route-inspect-presentation.md) — Complete at production commit `51c0960` plus final managed acceptance — Implementer: Mastermind
  - [x] [Accept route inspect and close Route Discovery](route-inspect-acceptance.md) — Complete — integrated managed/native and closeout gates — Implementer: Mastermind
- [x] [Complete the accepted Route Inspect interactive source-collision selection](route-inspect-interaction-correction.md) — Complete and squash-integrated at `fa3db1ee` with exact tree equality to final reviewed candidate `37c9360`

## Closeout And Routing Waiver

Route List is integrated at `edca509`; Route Inspect is accepted from final
correction `9c690b4` with complete managed and local `win-x64` Native AOT evidence.
The later native-interaction conformance correction is complete at final reviewed
`linux-x64` integration candidate `37c9360`; its protected Root integration
commit is `d9f1f350`. Focused managed/process, complete managed/Native AOT,
no-write dogfood, and independent review evidence pass. The final candidate is
squash-integrated locally at `fa3db1ee` with exact tree equality.
Actual terminal-process PTY proof is explicitly outside this accepted evidence
scope; no PTY, native, or platform-specific harness was added.
`CLI-EDGE-001` remains the generated-index/routing refresh blocker: its legacy
duplicate-entrypoint failure prevents a clean link-safe move and generated-index
refresh. Completed Inspect records remain in place, and generated `Entries` below
retain their pre-closeout projection rather than being hand-edited. Manual
navigation and status text outside the generated region is current under this
waiver. No routing-tool success or generated-index refresh is claimed.

## Entries

<!-- open-forge:generated-index:start -->
- [Routes completed leaf Tasks for Route Discovery without changing their historical Task meaning](done/_done.md) - #Memory #Working #CLI #Task #Route #Discovery #Complete #Contextual
- [Implement and accept route inspect, then promote only route facts proved identical by both commands](route-inspect.md) - #Memory #Working #CLI #Task #Route #Inspect #Promotion #Contextual #Complete
- [Run integrated route-inspect acceptance, promotion closeout, edge dispositions, and delivery-boundary audits](route-inspect-acceptance.md) - #Memory #Working #CLI #Task #Route #Inspect #Acceptance #NativeAOT #Contextual #Complete
- [Freeze route-inspect callable contracts and map every guarantee to evidence before behavior](route-inspect-contracts.md) - #Memory #Working #CLI #Task #Route #Inspect #Contract #Evidence #Contextual #Complete
- [Complete the already-accepted Route Inspect interactive source-collision selection](route-inspect-interaction-correction.md) - #Memory #Working #CLI #Task #Route #Inspect #Interaction #Correction #Contextual
- [Bind, execute, and present route inspect once through compact, expanded, JSON, diagnostics, and help surfaces](route-inspect-presentation.md) - #Memory #Working #CLI #Task #Route #Inspect #Presentation #Contextual #Complete
- [Form the inspect-local route profile from one accepted graph and fact set without adding diagnosis or mutation](route-inspect-profile.md) - #Memory #Working #CLI #Task #Route #Inspect #Profile #Contextual #Complete
<!-- open-forge:generated-index:end -->

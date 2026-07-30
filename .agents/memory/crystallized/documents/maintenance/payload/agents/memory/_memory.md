---
open-forge:
  description: Current maintenance contracts for the installable Memory root and its Working, Emerging, Crystallized, and Archived `routes`
  responsibility: Preserve the installed Memory authority boundary, standard state `routes`, source alignment, and complete operational contract
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, MemoryModel]
---

# Memory Runtime Maintenance

## Source

[`src/open-forge/.agents/memory/_memory.md`](../../../../../../../../src/open-forge/.agents/memory/_memory.md) is the canonical installed Memory `entrypoint`. The repository [Memory `entrypoint`](../../../../../../_memory.md) dogfoods the same authored contract and adds repository-local generated `Entries`.

The [Memory architecture](../../../../framework/memory/_memory.md), [Memory model](../../../../framework/memory/model.md), four routed state contracts, [transition contract](../../../../framework/memory/transitions.md), and [accepted-state contract](../../../../framework/truth.md) define coherent Framework meaning beyond runtime instructions.

## Contract

- Frontmatter keeps #LoadNow, #Memory, and #OrganicGrowth so the root Memory contract enters baseline context with its Framework classification and growth role visible
- The root `entrypoint` keeps Memory below applicable authority while allowing it to record any subject without activating behavior it describes
- Durable communication, coordination, direction, uncertainty, and learning are preserved when they must survive current context, while ordinary conversation does not become Memory automatically
- Clear user direction is accepted within its stated scope and updates its appropriate current destination. Tentative, exploratory, inferred, or materially ambiguous conclusions remain Working or Emerging until accepted.
- The Memory `entrypoint` relies on the loader's universal scoping and management contract instead of restating it
- Working, Emerging, Crystallized, and Archived remain the four standard state `routes`, each with its own `entrypoint` and positive state semantics
- Movement follows changes in recorded-state meaning and links to the appropriate #Core `route` or external authoritative system when Memory should not own the accepted result
- A new top-level Memory state requires explicit user direction because it changes the shared state model. Ordinary routed scopes remain recursively customizable.
- The authored source stays compact and complete for runtime use without requiring repository Maintenance or conceptual documents

## Standard Loading And Content Shape

| Standard `route` | Loading contract |
|---|---|
| Memory | #LoadNow baseline `entry` |
| Working and Crystallized | #LoadNow navigation for active and accepted records |
| Emerging and Observations | #KeepInMind continuity review |
| Handoffs, Sessions, Analysis, Ideas, Decisions, and Documents | #LoadNow after their parent `entrypoint` loads |
| Archived | On-demand historical navigation without #LoadNow or #KeepInMind |

Canonical leaf `entrypoints` ship with empty generated `Entries`. The repository dogfood tree may add local records through regenerated indexes without changing authored source.

## Verification

- Core installation closure tests verify the complete Memory `route` set, loading policy, root authority wording, `managed route` reconciliation through scopes, and generated navigation
- Compare every canonical Memory source with its dogfood authored content outside generated `Entries`
- Run `open-forge index` and `open-forge doctor` against both the repository and `src/open-forge/`

## Entries

<!-- open-forge:generated-index:start -->
- [Current maintenance contract for the installable Archived Memory entrypoint](archived.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Archived #Historical
- [Current maintenance contracts for the installable Crystallized Memory entrypoint and its Decisions and Documents routes](crystallized/_crystallized.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Crystallized
- [Current maintenance contracts for the installable Emerging Memory entrypoint and its Analysis, Ideas, and Observations routes](emerging/_emerging.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Emerging #Candidate
- [Current maintenance contracts for the installable Working Memory entrypoint and its Handoffs and Sessions routes](working/_working.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Working #Contextual
<!-- open-forge:generated-index:end -->

---
open-forge:
  description: Current maintenance contracts for the installable Memory root and its Working, Emerging, Crystallized, and Archived routes
  responsibility: Preserve the installed Memory authority boundary, standard state routes, source alignment, and complete operational contract
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, MemoryModel]
---

# Memory Runtime Maintenance

## Source

[`src/open-forge/.agents/memory/_memory.md`](../../../../../../../../src/open-forge/.agents/memory/_memory.md) is the canonical installed Memory entrypoint. The repository [Memory entrypoint](../../../../../../_memory.md) dogfoods the same authored contract and adds repository-local generated entries.

The [Memory architecture](../../../../framework/memory/_memory.md), [Memory model](../../../../framework/memory/model.md), four routed state contracts, [transition contract](../../../../framework/memory/transitions.md), and [accepted-state contract](../../../../framework/truth.md) own coherent Framework meaning beyond runtime instructions.

## Contract

- Frontmatter keeps #LoadNow, #Memory, and #OrganicGrowth so the root Memory contract enters baseline context with its layer and growth role visible
- The root entrypoint keeps Memory below applicable authority while allowing it to record any subject without activating behavior it describes
- Durable communication, coordination, direction, uncertainty, and learning are preserved when they must survive current context, while ordinary conversation does not become Memory automatically
- Working, Emerging, Crystallized, and Archived remain the four shipped state routes, each with its own entrypoint and positive state semantics
- Movement follows changes in recorded-state meaning and links to the appropriate #Core route or external authoritative system when Memory should not own the accepted result
- A new root state requires explicit user direction because it changes the shared state model; ordinary child scopes remain recursively customizable
- The authored source stays compact and complete for runtime use without requiring repository Maintenance or conceptual documents

## Verification

- Core installation closure tests verify the complete Memory route set, root authority wording, scoped Framework route updates, and generated navigation
- Compare canonical source and dogfood authored content outside generated `Entries`
- Run `open-forge index` and `open-forge doctor` against both the repository and `src/open-forge/`

## Entries

<!-- open-forge:generated-index:start -->
- [Current maintenance contract for the installable Archived Memory entrypoint](archived.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Archived #Historical
- [Current maintenance contracts for the installable Crystallized Memory entrypoint and its Decisions and Documents routes](crystallized/_crystallized.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Crystallized
- [Current maintenance contracts for the installable Emerging Memory entrypoint and its Analysis, Ideas, and Observations routes](emerging/_emerging.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Emerging #Candidate
- [Current maintenance contracts for the installable Working Memory entrypoint and its Handoffs and Sessions routes](working/_working.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Working #Contextual
<!-- open-forge:generated-index:end -->

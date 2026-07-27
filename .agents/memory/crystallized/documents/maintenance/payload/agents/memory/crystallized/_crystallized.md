---
open-forge:
  description: Current maintenance contracts for the installable Crystallized Memory entrypoint and its Decisions and Documents routes
  responsibility: Preserve accepted durable state, consolidation, supersession, shipped child routes, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Crystallized]
---

# Crystallized Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/crystallized/_crystallized.md`](../../../../../../../../../src/open-forge/.agents/memory/crystallized/_crystallized.md) is the canonical installed Crystallized Memory entrypoint. The repository [Crystallized entrypoint](../../../../../../../crystallized/_crystallized.md) dogfoods the same authored contract and adds repository-local generated entries.

The [Crystallized state contract](../../../../../framework/memory/crystallized.md) defines accepted durable Memory. The [accepted-state contract](../../../../../framework/truth.md) owns Framework-wide acceptance and synchronization.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Crystallized, and #CurrentTruth classification
- The entrypoint keeps one coherent current representation for each distinct question and scope by updating, splitting, merging, or reshaping existing material
- Superseded material is archived or linked with enough context to understand the change
- Decisions and Documents remain the two shipped child routes and enter through ordinary generated navigation
- Recursive scopes may organize accepted material without changing Crystallized meaning or forcing all authoritative state into Memory
- The installed parent contains no project-specific accepted records

## Verification

- Installation and scoped-route closure tests verify Crystallized, Decisions, and Documents creation, updating, indexing, and classification
- Compare canonical source and dogfood authored content outside generated `Entries`

## Entries

<!-- open-forge:generated-index:start -->
- [Current maintenance contract for the installable Decisions Memory entrypoint](decisions.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Decision #Rationale
- [Current maintenance contract for the installable Documents Memory entrypoint](documents.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Record
<!-- open-forge:generated-index:end -->

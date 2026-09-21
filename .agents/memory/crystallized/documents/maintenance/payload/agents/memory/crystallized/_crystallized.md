---
open-forge:
  description: Current maintenance contract for the Core Crystallized Memory entrypoint and its optional Extension-provided Decisions and Documents routes
  responsibility: Preserve accepted durable state, consolidation, replacement history, standard nested roles, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Crystallized]
---

# Crystallized Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/crystallized/_crystallized.md`](../../../../../../../../../src/open-forge/.agents/memory/crystallized/_crystallized.md) is the canonical installed Crystallized Memory `entrypoint`. The repository [Crystallized `entrypoint`](../../../../../../_crystallized.md) dogfoods the same authored contract and adds repository-local generated `Entries`.

The [Crystallized state contract](../../../../../framework/memory/crystallized.md) defines accepted durable Memory. The [accepted-state contract](../../../../../framework/truth.md) owns Framework-wide acceptance and synchronization.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Crystallized, and #CurrentTruth classification
- Clear user direction, delegated authority, a requested action that clearly requires the choice, or a declared external authority establishes acceptance within scope. Tags, repetition, and agent confidence do not establish it.
- The entrypoint keeps one current answer for each distinct question and scope by updating, splitting, merging, or reshaping existing material
- Material that is no longer current is archived or linked with enough context to understand the change
- Decisions and Documents retain their distinct roles within Crystallized Memory. Their optional Extension-provided routes remain linked through the maintenance `Entries`.
- Recursive scope follows the Loader rules and does not force all accepted state into Memory
- The installed parent contains no project-specific accepted records

## Verification

- Core installation closure tests verify the Crystallized route's creation, updating, indexing, and classification. Selected Extension installation tests verify the optional Decisions and Documents routes and their source classification.

## Entries

- [Current maintenance contract for the installable Decisions Memory entrypoint](decisions.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Decision #Rationale
- [Current maintenance contract for the installable Documents Memory entrypoint](documents.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Record

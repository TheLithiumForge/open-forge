---
open-forge:
  description: Current maintenance contract for the installable Archived Memory entrypoint
  responsibility: Preserve historical authority boundaries, extraction, provenance, restoration, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Archived, Historical]
---

# Archived Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/archived/_archived.md`](../../../../../../../../src/open-forge/.agents/memory/archived/_archived.md) is the canonical installed Archived Memory entrypoint. The repository [Archived entrypoint](../../../../../../archived/_archived.md) dogfoods the same authored contract and may add local generated entries.

The [Archived state contract](../../../../framework/memory/archived.md) defines historical meaning and authority. The [transition contract](../../../../framework/memory/transitions.md#replacement-and-archival) owns extraction and restoration across state boundaries.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Archived, #Contextual, and #Historical classification
- Archived material remains historical context without current authority
- Archived preserves historical Memory records and uses linked records for relevant history about artifacts that remain elsewhere
- The entrypoint preserves origin, archival reason, and replacement relationships when a replacement exists
- Useful current meaning is extracted before archival to the route or system that owns it
- Restoration targets an explicit current destination and validates historical material against current conditions
- The installed `route` begins empty and supports Memory scopes before or after the Archived state without requiring a mirrored archive taxonomy

## Verification

- Installation and route tests verify the Archived entrypoint, classification, generated-region integrity, and recursive child routing
- Compare canonical source and dogfood authored content outside generated `Entries`

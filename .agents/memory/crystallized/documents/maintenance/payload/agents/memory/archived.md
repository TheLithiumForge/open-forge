---
open-forge:
  description: Current maintenance contract for the installable Archived Memory entrypoint
  responsibility: Preserve historical authority boundaries, extraction, provenance, restoration, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Archived, Historical]
---

# Archived Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/archived/_archived.md`](../../../../../../../../src/open-forge/.agents/memory/archived/_archived.md) is the canonical installed Archived Memory `entrypoint`. The repository [Archived `entrypoint`](../../../../../../archived/_archived.md) dogfoods the same authored contract and may add local generated `Entries`.

The [Archived state contract](../../../../framework/memory/archived.md) defines historical meaning and authority. The [transition contract](../../../../framework/memory/transitions.md#replacement-and-archival) owns extraction and restoration across state boundaries.

## Contract

- Frontmatter preserves #Memory, #Archived, #Contextual, and #Historical classification without #LoadNow or #KeepInMind
- Archived material remains historical context without current authority
- Archived keeps historical Memory records and links to retained sources when the history belongs elsewhere
- The entrypoint records origin, archival reason, and replacement when one exists
- Useful current meaning moves before archival to the route or system that should define it
- Restoration targets an explicit current destination and validates historical material against current conditions
- The installed `route` begins empty and supports Memory scopes before or after the Archived state without requiring a mirrored archive taxonomy

## Verification

- Installation and `route` tests verify the Archived `entrypoint` remains on demand while preserving classification, generated-region integrity, and recursive child routing

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

- The title is followed by `## What useful history should remain available without governing current work?` and a short paragraph describing the folder.
- Frontmatter preserves #Memory, #Archived, #Contextual and #Historical without a loading tag. Its Entries are checked when history may help current work.
- Archived material remains historical context without current authority
- Archived keeps historical Memory records and links to retained sources when the history belongs elsewhere
- The entrypoint requires retained records to identify their origin, archival reason and replacement when one exists.
- Still-current content is extracted before archival into the sources that define it. Retain the useful remainder without requiring lossless preservation
- Metadata that would activate a former role or claim current authority is removed, including loading tags. Restoration follows the destination's rules.
- Consolidation and transformation preserve useful history. Deletion follows user direction or accepted retention preferences
- Restoration validates material against current conditions and uses an explicit destination. Current knowledge requires acceptance within its scope
- The installed `route` begins empty and supports Memory scopes before or after the Archived state without requiring a mirrored archive taxonomy

## Verification

- Use the [payload integrity checks](../../_payload.md#verification) for shipped bytes and metadata rendering.
- Compare source and repository rules outside Entries. Check the shipped Memory row leaves Archived on demand.
- Manually review extraction, historical metadata and explicit restoration. Packaging checks do not prove retention decisions or acceptance.

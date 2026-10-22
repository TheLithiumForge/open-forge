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

- The title is followed by `## What accepted knowledge should remain current within this scope?` and a short paragraph describing the folder.
- Frontmatter preserves #LoadNow, #Memory, #Crystallized and #CurrentTruth.
- Entries are checked before deciding no accepted knowledge applies. Only accepted current knowledge belongs here, within its scope and under the loader's acceptance rule. Tags, repetition and confidence do not establish acceptance.
- Changed accepted direction updates, splits, merges or reshapes existing records rather than adding a competing current version.
- Superseded material is moved or linked with enough context to understand the change.
- Decisions and Documents are distinct optional Extension roles. Core does not ship their routes or force all accepted knowledge into Memory.
- The installed state catalogue contains no project records and starts with empty Entries.

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for the Core state, optional Extension bytes and metadata rendering.
- Compare shared source and repository rules outside Entries. Review acceptance and replacement behavior manually rather than claiming byte parity proves it.
- Confirm optional Decisions and Documents remain Extension contributions and preserve their separate roles.

## Entries

- [Current maintenance contract for the installable Decisions Memory entrypoint](decisions.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Decision #Rationale
- [Current maintenance contract for the installable Documents Memory entrypoint](documents.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Record

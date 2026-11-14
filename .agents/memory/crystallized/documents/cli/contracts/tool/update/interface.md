---
open-forge:
  description: Update tools to a supported version
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool update interface

Tool update is a command that updates tools through their published channel.
Update replaces the active version and rewrites absolute launchers while recording the new exact version.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Syntax

Proposed:

```text
open-forge tool update [<id[@version]>] [--channel <channel>] [--yes] [--allow-path <path>...] [--dry-run] [global flags]
```

Use the same named version-spec grammar as Install. A bare name selects latest support. An explicit spec selects that supported version or minor range. Without a spec, update every declared tool. A named Update requires a declared or locked tool. A channel override requires a named spec.
The help purpose is "Update tools to a supported version."

## Report

Report old and new exact versions and profiles, channel and client, account-wide replacement, requirements, rewritten launchers, native setup, additive migration and exact lock changes.
The [common Interface](../interface.md) defines consent, repetition and result coordinates.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool update
```

Update replaces the active version and rewrites absolute launchers while recording the new exact version.

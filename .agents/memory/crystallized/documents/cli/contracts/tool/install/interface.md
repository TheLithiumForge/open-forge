---
open-forge:
  description: Install tools and add configuration
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install interface

Tool install is a command that installs tools and runs their native setup.
A teammate runs bare Install and receives the exact CodeGraph version in the lock. Named Install selects latest support.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Syntax

Proposed:

```text
open-forge tool install [<id[@version]>] [--channel <channel>] [--yes] [--allow-path <path>...] [--dry-run] [global flags]
```

A bare name selects latest support, even when an older version is recorded. A spec such as codegraph@1.6 or codegraph@1.6.2 selects that supported range or exact version. Bare Install selects declarations and restores exact lock versions. A declared tool without a lock selects latest within its declared range. A channel override requires a named spec.
The help purpose is "Install tools and add configuration."

## Report

Report requested spec, resolved exact version, channel, npm client, account-wide effects, native commands and outputs, additive changes, telemetry, downloads, PATH instructions and record changes.
The [common Interface](../interface.md) defines consent, repetition and result coordinates.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool install
```

A teammate runs bare Install and receives the exact CodeGraph version in the lock. Named Install selects latest support.

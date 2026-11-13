---
open-forge:
  description: Install tools and add their project configuration
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install interface

Tool install is a command that installs tools and adds their project configuration.
With CodeGraph 1.6.x declared, a teammate runs bare Install and receives a working direct `codegraph` command plus project configuration.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Syntax

Proposed:

```text
open-forge tool install [<id>] [--version <range>] [--channel <channel>] [--allow-download] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

A named ID uses its declared range when present, otherwise the newest supported range. `--version` selects an earlier supported range. Bare Install selects every declared tool. Version and channel overrides require a named ID.
The help purpose is "Install tools and add their project configuration."

## Report

Show the exact selected patch before approval, existing installation reuse, channel and prerequisites, account-wide effects, setup outputs, telemetry, config changes, guidance, local state and portable record changes.
The [common Interface](../interface.md) defines report structure, repetition, result meanings and consent.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool install
```

With CodeGraph 1.6.x declared, a teammate runs bare Install and receives a working direct `codegraph` command plus project configuration.

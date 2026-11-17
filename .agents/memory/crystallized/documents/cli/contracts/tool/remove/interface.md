---
open-forge:
  description: Remove Open Forge's changes for a tool and offer its own uninstall
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove interface

Tool remove interface is the syntax and report of `tool remove`.
For example, Codebase Memory's question says its uninstall removes it and its entries from every coding agent on the person's account.

Status: Lean design decided on 17 November 2026.

## Syntax

```text
open-forge tool remove <id> [--yes] [--dry-run] [global flags]
```

The help purpose is "Remove Open Forge's changes for a tool and offer its own uninstall."

## Question

After Open Forge's changes are removed, and only when the tool is installed, Remove shows the tool's one-sentence uninstall description and its commands and asks `Run it too?`.
The default answer is no. `--yes` takes the default.
Without a live terminal, or with `--format json`, Remove asks nothing and shows the commands to run by hand.

## Report

The report lists Open Forge's changes with their outcome, then the uninstall commands with their outcome or as not run.
The [common Interface](../interface.md) defines result coordinates.

## Example

```text
open-forge tool remove codegraph
```

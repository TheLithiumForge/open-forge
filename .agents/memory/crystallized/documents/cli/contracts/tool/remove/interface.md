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


## Findings catalogue

| Code | Meaning |
| --- | --- |
| `tool-remove.invalid-input` | Correct the named operand or option |
| `tool-remove.unknown-tool` | Use an ID from `tool list` |
| `tool-remove.settings-unavailable` | Fix `.agents/open-forge.json`, which can't be read or parsed |
| `tool-remove.integration-conflict` | Fix the named tool file. Open Forge left it unchanged |
| `tool-remove.command-failed` | Read the failed command's exit code and output, fix the problem and run again |
| `tool-remove.target-changed-during-apply` | A file changed while Open Forge was writing it. Run the command again |
| `tool-remove.write-failed` | Open Forge couldn't write its changes. The cause names the reason |
| `tool-remove.interrupted` | The command was cancelled. Nothing more ran or changed |

## Example

```text
open-forge tool remove codegraph
```

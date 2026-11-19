---
open-forge:
  description: Install a tool and configure the project, or install configured tools that are missing
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install interface

Tool install interface is the syntax and plan shape of `tool install`.
For example, `tool install codegraph` shows the npm command, CodeGraph's setup command and Open Forge's two file changes, then asks.

Status: Lean design decided on 17 November 2026.

## Syntax

```text
open-forge tool install [<id>] [--yes] [--dry-run] [global flags]
```

With an ID, Install installs that tool when it's missing, runs its project setup and adds Open Forge's changes.
Without an ID, Install installs the project's configured tools that are missing on this machine.
The help purpose is "Install a tool and set it up for this project, or install this project's missing tools."
Help says that updating means running the tool's own update or installing it again.

## Plan

The plan uses the active voice and this shape:

```text
Open Forge will run these commands to install CodeGraph:
  npm i -g @colbymchenry/codegraph@1.6.2
  codegraph install --yes --location local
                              Adds CodeGraph to the coding agents it finds in this project
                              CodeGraph keeps its default telemetry. Turn it off with: codegraph telemetry off
Then Open Forge will make these changes:
  codegraph.json              Keep archived Open Forge memory out of the graph
  .agents/open-forge.json     Add codegraph to this project's tools
Continue? [y/N]
```

When the tool is already installed, the install command is replaced by a line that names the detected version.
Bare Install lists each missing tool with its install command.
Changes that are already present aren't listed.

## Report

After the run, the same lines carry their outcome.
A failed command shows its exit code, the tail of its output and a plain next step.
The [common Interface](../interface.md) defines consent and result coordinates.


## Findings catalogue

| Code | Meaning |
| --- | --- |
| `tool-install.invalid-input` | Correct the named operand or option |
| `tool-install.unknown-tool` | Use an ID from `tool list` |
| `tool-install.settings-unavailable` | Fix `.agents/open-forge.json`, which can't be read or parsed |
| `tool-install.open-forge-missing` | Run `open-forge install` first. Install records the tool in `.agents/open-forge.json` |
| `tool-install.integration-conflict` | Fix the named tool file. Open Forge left it unchanged |
| `tool-install.requirement-missing` | Install the named package manager or runner. Nothing ran |
| `tool-install.confirmation-required` | Run in a terminal or pass `--yes`. Nothing ran |
| `tool-install.command-failed` | Read the failed command's exit code and output, fix the problem and run again |
| `tool-install.target-changed-during-apply` | A file changed while Open Forge was writing it. Run the command again |
| `tool-install.write-failed` | Open Forge couldn't write its changes. The cause names the reason |
| `tool-install.interrupted` | The command was cancelled. Nothing more ran or changed |

## Example

```text
open-forge tool install codegraph
```

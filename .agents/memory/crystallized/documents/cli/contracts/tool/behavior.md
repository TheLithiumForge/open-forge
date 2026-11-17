---
open-forge:
  description: Define the tool catalogue, detection, command execution and Open Forge's own changes
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool behavior

Tool behavior is the common flow that detects a tool, runs its own install and setup commands and adds Open Forge's small integration changes.
For example, Install runs `npm i -g @colbymchenry/codegraph@1.6.2`, then CodeGraph's own project setup, then adds archived Open Forge memory to `codegraph.json`'s `exclude` array.

Status: Lean design decided on 17 November 2026.

## Goals

The tool commands aim for usefulness, automatic configuration and safety.
Open Forge uses each tool's documented install method and its own setup.
It doesn't manage the tool's settings, runtimes, PATH or downloads.
When something doesn't work, it explains in plain words what the person needs to do.

## Catalogue

The catalogue is typed C# data owned by the Framework. Each entry holds:

- an ID, a name and a one-line purpose
- a category, such as Code search. A new category needs only a new definition.
- the upstream URL and licence
- the install method, which is either a package-manager command or the tool's own install script, with the tested version pinned where the installer allows it
- the required package manager or runner behind the install method: `npm`, or `script`
- the tool's own project setup command, if it has one, with one plain sentence about what it does and an optional plain note
- a `--version` detection command
- Open Forge's integration changes
- one plain sentence about what the tool's own uninstall does, and its uninstall commands

Commands can differ by platform. Windows and other platforms each have their own command text where the upstream documentation does.
Every command and argument is a fixed catalogue constant and never comes from user input.

V1 has two entries, both in the Code search category:

| Tool            | Install method | Project setup                                                 | Integration change                                  |
| --------------- | -------------- | ------------------------------------------------------------- | --------------------------------------------------- |
| CodeGraph       | `npm`          | `codegraph install --yes --location local --target claude,codex` | Union archived memory into `codegraph.json` `exclude` |
| Codebase Memory | `script`       | None. Its installer configures the agents it finds            | Marked block in `.cbmignore`                        |

The [Tool setup design](../../technical-designs/tool-setup.md) records the exact commands and their upstream evidence.

## Detection

Detection runs the entry's `--version` command through the bounded process host and reads the first version number in its output.
A command that can't start or exits with an error means "not installed".
Detection runs nothing else and writes nothing.

A tool's configured state comes from the `tools` list in the project settings, `.agents/open-forge.json`.
Open Forge's integration state comes from the files themselves. Each change is self-identifying, so no record or lock is needed.

## Requirements

Before it runs any command, Install checks the package manager or runner each selected tool needs.
`npm` needs `npm`. `script` needs `curl` and `bash` on Linux and macOS, and Windows PowerShell on Windows.
If one is missing, Install runs nothing and says in plain words what to install. For example: CodeGraph needs npm, which isn't installed. Install Node.js from https://nodejs.org (npm comes with it), then run `open-forge tool install` again.

## Commands

Open Forge runs each command the way a person would type it.
Ordinary commands run through the platform shell, `cmd.exe` on Windows and `/bin/sh` elsewhere, so package-manager shims such as `npm.cmd` resolve normally.
PowerShell commands run through `powershell -NoProfile -NonInteractive`.
Commands run in the workspace root with the person's own environment and no input.
Each command has a time limit, and cancellation stops its whole process tree.
Open Forge keeps the tail of each command's output. A failed command reports its exit code, that tail and a plain next step.

## Setup

Run the tool's own project setup unchanged and say in one sentence what it does.
CodeGraph's setup adds CodeGraph to Claude Code and Codex in the project. With `--yes`, CodeGraph keeps its default telemetry, so the plan adds: `Turn it off with: codegraph telemetry off`.
Codebase Memory has no separate setup. Its installer configures every coding agent it finds for the person's account and adds its folder to PATH.
Open Forge writes no MCP entries, hooks, permissions, instruction blocks or Skills of its own.

## Integration changes

V1 has one kind of integration: keeping archived Open Forge memory out of each tool's index.
The archived path comes from the project's main root, such as `.agents/memory/archived`.

- **JSON union.** Add the archive pattern to a JSON array, creating the file or member when missing. Removal deletes only that value. A file left with nothing but an empty array is deleted.
- **Marked block.** Add a block between two fixed marker lines. Removal deletes only that block. A file left empty is deleted.

A malformed file, a malformed block or a value of the wrong type is a conflict. Open Forge leaves the file unchanged and says what to fix.
Changes go through the shared write path, with expected-state checks and the `target-changed-during-apply` failure, and with the shared recovery bundle for the operation.
Adding the tool ID to the settings `tools` list uses the same path.

## Safety

- Nothing runs without consent, and the exact commands are always shown first.
- Open Forge never edits PATH, never changes a tool's own settings and never installs runtimes. A tool's own installer may do these things, and the plan says so.
- Open Forge's file edits are additive and exactly reversible.
- Open Forge keeps no records beyond the settings `tools` list.

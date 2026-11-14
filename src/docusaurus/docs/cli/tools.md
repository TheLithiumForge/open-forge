---
title: Tool setup
---

# Tool setup

Tool setup is the CLI command group that installs published developer tools and runs their native setup for your project.

For example, you can install CodeGraph, build its project index and use the same direct command from your terminal and agent app.
The project lock lets a teammate restore the exact version later.

## Requirements

Use Open Forge `0.9.0-beta.19` or newer in your project directory.
Run `open-forge tool list` to see the catalogue, then read the entry before installing it:

```sh
open-forge tool inspect codegraph
```

Inspect shows supported versions, channels, machine requirements, native writes and Open Forge additions.
CodeGraph starts at `1.6.2` in `1.6.x`.
Codebase Memory starts at `0.12.1` in `0.12.x`.
Review native effects, telemetry changes and watcher defaults in the [command reference](/guides/cli#tool-native-effects) first.

## Install

Preview the exact version and channel, then apply the plan:

```sh
open-forge tool install codegraph@1.6.2 --channel github-release --dry-run
open-forge tool install codegraph@1.6.2 --channel github-release --yes
open-forge tool status codegraph
```

The install produces CodeGraph `1.6.2` and Status reports the same locked and detected version.
The native installer configures local Claude and Codex integration.
Open Forge turns account telemetry off, excludes archived memory and checks Gitignore coverage.

`--yes` accepts downloads and the shown setup plan.
PATH changes and shared deletion need separate live choices.
If bin isn't on PATH, follow the printed instructions, restart your terminal and agent apps, then run Status again.
A dry run can use the network to resolve latest and reports that use.

## Use the tool

From your project, build the index and read it:

```sh
codegraph --version
codegraph init
codegraph query greeting
```

`--version` reports `1.6.2`.
`init` indexes your project so a query can find a symbol such as `greeting`.
Restart Claude and Codex after setup.
Codex needs to trust your project before it enables project configuration.
The native MCP entries, hooks, permissions and instructions stay under the upstream installer.

For Codebase Memory, the catalogue ID is `codebase-memory` and the direct command is `codebase-memory-mcp`.
Its setup uses default account-wide client discovery.
Read its entry to check the account files and watcher defaults before applying it.

To install that version and build an index from your project directory:

```sh
open-forge tool install codebase-memory@0.12.1 --channel github-release --yes
codebase-memory-mcp --version
codebase-memory-mcp cli --quiet index_repository --repo-path .
```

The direct command reports `codebase-memory-mcp 0.12.1`.
Indexing returns the project name for later graph queries.
The native agent setup uses the same installed executable for MCP.

## Versions and teams

On named Install and Update, a bare ID chooses latest support.
Use `codegraph@1.6` for a supported minor or `codegraph@1.6.2` for an exact stable version.
`--channel` applies to a named Install or Update.
For channel choice and valid syntax, read the [selector rules](/guides/cli#tool-selectors).

Commit `.agents/open-forge.json` and `.agents/open-forge.lock.json`.
These paths stay fixed when you use a custom context root.
A teammate restores the exact locked versions with this sequence:

```sh
open-forge tool install --dry-run
open-forge tool install --yes
```

To choose newest support for declared tools, preview Update, then apply it:

```sh
open-forge tool update codegraph --dry-run
open-forge tool update codegraph --yes
```

In this `1.6.2` example, Update reuses the verified installation because it is already current.
An account-wide replacement can affect other projects.

## Remove

Preview removal, then remove the project setup:

```sh
open-forge tool remove codegraph --dry-run
open-forge tool remove codegraph --yes
```

CodeGraph's native project setup and Open Forge's recorded project additions are removed.
The installed CLI and index stay available.
Account telemetry stays off and the result prints its manual change command.

For Codebase Memory, `open-forge tool remove codebase-memory --yes` removes only Open Forge project additions and prints native uninstall instructions.
The Windows `0.12.1` journey completes this project-only removal with the account setup and index kept.
Native uninstall removes the account binary and all agent entries, so it needs a separate live confirmation naming those effects.
Read [removal and backups](/guides/cli#tool-removal) before changing account setup shared by other projects.

## Command results

The six commands are `list`, `inspect`, `status`, `install`, `update` and `remove` under `open-forge tool`.
Use `open-forge tool <command> --help` for options and examples.

For scripts, use `--format json` and check `status` and exit code.
Tool results use schema 3 and `data.tools`.
For actual Status fields, read the [JSON example](/guides/cli#tool-json).

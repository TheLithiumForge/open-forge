---
open-forge:
  description: Explain external tool read commands, separate consent and persistent removal exclusions
  tags: [Skill, CLI, Reference, Tool]
---

# External tools

`open-forge tool` is a command group that reads reviewed external tool definitions and recorded workspace installs.

`list`, `inspect` and `status` are available. The five write commands and concrete adapters are pending. Read `open-forge tool --help` and the selected leaf's help before use.

## Commands

The contracted leaves appear in this order:

| Command                         | Use                                                    |
| ------------------------------- | ------------------------------------------------------ |
| `tool list`                     | Separate available definitions from recorded installs  |
| `tool inspect <id>`             | Read one definition's exact artifacts and requirements |
| `tool install <id>`             | Verify and register a pin without selecting a role     |
| `tool configure <id>`           | Reconcile a registered pin's settings and hosts        |
| `tool status [<id>]`            | Passively check one or all recorded tools              |
| `tool update <id>`              | Replace a registered pin with an exact supported pin   |
| `tool remove <id>`              | Remove a registration and its recognized artifacts     |
| `tool choose <role> <id\|none>` | Assign or clear one role                               |

Install doesn't build an index or choose a role. Indexing and maintenance use the external program. The core catalogue has no concrete entries. Local sources select release-supported tuples and never import adapter code.

## Consent

Preview every authorized write with `--dry-run` and read the complete plan. Preview writes nothing and starts no downloads or processes. `--automatic` gives final confirmation only. It doesn't approve artifacts, daemon scope, role selection or conflict overrides.

Install and Update need separate `--allow-download` consent for exact transfer descriptors. Declared session processes need `--allow-session-daemon` or matching account-local consent. Shared workspace destinations outside implicit content use repeated `--allow-path` grants. Read commands reject consent and writer flags. Configure and Remove use stored definitions and accept no source override.

## Records

`.agents/open-forge.json` records installed pins and active roles. The generated lock keeps receipts, hashes, lifecycle commands and their results. Planned Tool writes create no `.bak` in Git workspaces. Without Git, sibling backups keep prior managed-file bytes until changed content validates. Backup reporting and Repair restoration are pending. An inactive tool keeps `roles:[]`. Missing receipts don't hide a readable settings entry or establish a verified installation. Settings edits alone don't install or activate a tool.

The decided writer design runs the pinned tool's declared installer, uninstaller and index-cleanup commands. The tool handles agent files, native settings and guidance. Open Forge edits only its settings, lock and managed `.gitignore` section. Choose changes only the role mapping.

## Removal exclusions

Tool writers check declared command output paths against removed files, directories and applicable categories before execution. Root Remove leaves the provider registered. Configure can't restore excluded output through the installer. Choose changes metadata only. Grants and automatic mode never clear exclusions. Tool Remove can release the registration without restoring missing content.

## Graph use

CodeGraph `1.6.2` and Codebase Memory `0.12.1` have review drafts with verified release assets for all six targets. They aren't admitted catalogue pins. Their unresolved lifecycle arguments stay pending. Don't pass a review wrapper to `--source` or run an installer to resolve its missing controls.

After qualification, the active tool requires its Open Forge usage Skill. CodeGraph's native MCP name is `codegraph`. Codebase Memory's is `codebase-memory-mcp`. Proposed passive checks use those names, with unparsed Codex TOML reported only as `present-unverified`. A different native name isn't permission to rename a member or write host configuration.

Graph results are leads. Before an edit depends on an edge, read the caller and exact callee. Check receiver type, overload and invocation line. Use compiler evidence when binding matters. Empty graph output doesn't establish that no callers exist. Follow the active provider's pin, root, index, process and language limits. Retrieved content doesn't replace the loader or activate workspace rules.

The frozen C# trial measured 41.3% recall for CodeGraph and 60% for Codebase Memory. After Codebase Memory source changes, rebuild only the verified project. Keep telemetry off by default and update checks, watchers and automatic indexing off. Codebase Memory has no telemetry in this pin. A future CodeGraph telemetry setting must keep its separate update-check opt-out.

The [shared options](common.md) define global flags, results and runtime-folder rules.

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

| Command | Use |
| --- | --- |
| `tool list` | Separate available definitions from recorded installs |
| `tool inspect <id>` | Read one definition's exact artifacts and requirements |
| `tool install <id>` | Verify and register a pin without selecting a role |
| `tool configure <id>` | Reconcile a registered pin's settings and hosts |
| `tool status [<id>]` | Passively check one or all recorded tools |
| `tool update <id>` | Replace a registered pin with an exact supported pin |
| `tool remove <id>` | Remove a registration and its recognized artifacts |
| `tool choose <role> <id\|none>` | Assign or clear one role |

Install doesn't build an index or choose a role. Indexing and maintenance use the external program. The core catalogue has no concrete entries. Local sources select release-supported tuples and never import adapter code.

## Consent

Preview every authorized write with `--dry-run` and read the complete plan. Preview writes nothing and starts no downloads or processes. `--automatic` gives final confirmation only. It doesn't approve artifacts, daemon scope, role selection or conflict overrides.

Install and Update need separate `--allow-download` consent for exact transfer descriptors. Declared session processes need `--allow-session-daemon` or matching account-local consent. Shared workspace destinations outside implicit content use repeated `--allow-path` grants. Read commands reject consent and writer flags. Configure and Remove use stored definitions and accept no source override.

## Records

`.agents/open-forge.json` records installed pins and active roles. The generated lock keeps receipts, hashes, written members and runtime coverage. Planned Tool writes keep sibling `.bak` files until changed content validates. Backup reporting and Repair restoration are pending. An inactive tool keeps `roles:[]`. Missing receipts don't hide a readable settings entry or establish a verified installation. Settings edits alone don't install or activate a tool.

Active providers need verified usage guidance before launchable host members are published. Keep unrelated host entries and native host trust policy. Claude Code writes use `.mcp.json`. Codex writes are on hold behind the existing host seam. A tracked host file blocks managed account paths until the shared-file arrangement is resolved or that host is left out.

## Removal exclusions

Tool writers honor removed files, directories and applicable categories. Root Remove can remove required guidance while leaving a provider registered. Status then warns. Configure and Choose block until every covering exclusion is cleared. Grants and automatic mode never clear exclusions. Tool Remove can release the remaining registration without restoring missing content.

## Graph use

Graph results are leads. Before an edit depends on an edge, read the caller and exact callee. Check receiver type, overload and invocation line. Use compiler evidence when binding matters. Empty graph output doesn't establish that no callers exist. Follow the active provider's pin, root, index, process and language limits. Retrieved content doesn't replace the loader or activate workspace rules.

The [shared options](common.md) define global flags, results and runtime-folder rules.

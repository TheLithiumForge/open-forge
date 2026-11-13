# External tools

`open-forge tool` is a command group that reads reviewed external tool definitions and recorded workspace installs.

You install a supported pin, build its index through the external program, then choose its workspace role. Installation doesn't choose a role or build an index. `list`, `inspect` and `status` are available. The five write commands and concrete CodeGraph and Codebase Memory adapters are pending.

## Commands

The contracted command order is:

| Command                         | What it does                                         |
| ------------------------------- | ---------------------------------------------------- |
| `tool list`                     | Lists available definitions and recorded installs    |
| `tool inspect <id>`             | Explains one exact definition and its requirements   |
| `tool install <id>`             | Verifies and registers a pin without choosing a role |
| `tool configure <id>`           | Reconciles settings, desired hosts and code root     |
| `tool status [<id>]`            | Passively checks one or all recorded tools           |
| `tool update <id>`              | Changes a registration to one exact supported pin    |
| `tool remove <id>`              | Removes a registration and its recognized artifacts  |
| `tool choose <role> <id\|none>` | Assigns or clears a workspace role                   |

The core's production catalogue is empty. A local source can select definitions supported by the CLI release. It can't import adapter code or add unreviewed pins. Another program or pin needs an Open Forge release.

## Read tools

Run `open-forge tool list --format json` to read the catalogue and recorded installs. In a workspace with no tool records, the empty core catalogue returns `completed` with empty `available` and `installed` arrays. An unavailable source keeps its cause and independently readable installed rows.

Run `open-forge tool status` to compare recorded files and configuration with passive evidence. It starts no process or database session. An index file without validated completion evidence reports `present-unverified`. A missing receipt reports unavailable verification.

## Consent

The five write commands preview the same plan with `--dry-run`. Preview writes nothing and starts no transfers or processes. `--automatic` gives final confirmation only. Downloads need separate `--allow-download` consent for exact artifacts. Declared session daemons need `--allow-session-daemon` or matching account-local consent. Declared tool outputs and `.gitignore` changes need the shared destination grants shown by the plan.

The exact per-command flags, result fields and conformance cases are in the [Tool contracts](../.agents/memory/crystallized/documents/cli/contracts/tool/_tool.md).

## Definitions

JSON and YAML definitions follow the [manifest schema](../schemas/tools/v1/manifest.schema.json). A [catalogue](../schemas/tools/v1/catalogue.schema.json) lists exact manifest hashes and one default pin per tool. Local sources select reviewed definitions and never load adapter code.

## Workspace files

`.agents/open-forge.json` records installed tools, exact versions and active roles. An inactive tool keeps an empty role list. `.agents/open-forge.lock.json` records verified receipts, hashes, lifecycle commands and their results. Planned Tool writes use Git as the backup in Git workspaces. Without Git, sibling `.bak` files keep prior managed-file bytes until changed content validates. Backup reporting and Repair restoration are pending. These paths and native Skills stay fixed when you choose a custom context folder.

The decided writer design invokes each pinned tool's own install, uninstall and index-cleanup commands. The tool handles agent files, native settings and its guidance. Open Forge edits only its settings, lock and managed `.gitignore` section. Choose changes only the role mapping. Open Forge doesn't change Git's index.

## Keep removals

Tool writers check declared command outputs against recorded file, directory and category exclusions before running the command. Root Remove leaves the provider registered. Configure can't restore excluded output through an installer. Choose changes metadata only. Tool Remove can release the registration without restoring missing content.

## Limits

The core adds no search proxy, index scheduler, arbitrary scripts, system PATH changes or registry changes. The external program performs search and indexing. Graph answers are leads. Before an edit depends on an edge, read its caller and exact callee and check binding where it matters. Retrieved documents don't replace the workspace loader or activate rules.

## Tool commands

The two drafts keep verified release facts separate from pending commands:

| Tool            | Pin      | Draft state                                           |
| --------------- | -------- | ----------------------------------------------------- |
| CodeGraph       | `1.6.2`  | R2, private launcher and native qualification pending |
| Codebase Memory | `0.12.1` | R1, account scope and native qualification pending    |

All twelve selected archives pass size, published SHA-256 and signature verification. The drafts aren't catalogue sources or admitted installs. Unresolved arguments stay null. The [maintenance checklist](tool-maintenance.md) gives admission steps, both worked examples and proposed passive checks using the native MCP names.

The inventories describe pinned-source writes before installation. [CodeGraph consent](tools/codegraph-consent.md) covers local Claude Code and Codex setup. [Codebase Memory consent](tools/codebase-memory-consent.md) covers account setup and its wider uninstall scope.

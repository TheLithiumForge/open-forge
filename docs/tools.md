# External tools
`open-forge tool` is a command group that reads reviewed external tool definitions and recorded workspace installs.

You install a supported pin, build its index through the external program, then choose its workspace role. Installation doesn't choose a role or build an index. `list`, `inspect` and `status` are available. The five write commands and concrete CodeGraph and Codebase Memory adapters are pending.

## Commands

The contracted command order is:

| Command | What it does |
| --- | --- |
| `tool list` | Lists available definitions and recorded installs |
| `tool inspect <id>` | Explains one exact definition and its requirements |
| `tool install <id>` | Verifies and registers a pin without choosing a role |
| `tool configure <id>` | Reconciles settings, desired hosts and code root |
| `tool status [<id>]` | Passively checks one or all recorded tools |
| `tool update <id>` | Changes a registration to one exact supported pin |
| `tool remove <id>` | Removes a registration and its recognized artifacts |
| `tool choose <role> <id\|none>` | Assigns or clears a workspace role |

The core's production catalogue is empty. A local source can select definitions supported by the CLI release. It can't import adapter code or add unreviewed pins. Another program or pin needs an Open Forge release.

## Read tools

Run `open-forge tool list --format json` to read the catalogue and recorded installs. In a workspace with no tool records, the empty core catalogue returns `completed` with empty `available` and `installed` arrays. An unavailable source keeps its cause and independently readable installed rows.

Run `open-forge tool status` to compare recorded files and configuration with passive evidence. It starts no process or database session. An index file without validated completion evidence reports `present-unverified`. A missing receipt reports unavailable verification.

## Consent

The five write commands preview the same plan with `--dry-run`. Preview writes nothing and starts no transfers or processes. `--automatic` gives final confirmation only. Downloads need separate `--allow-download` consent for exact artifacts. Declared session daemons need `--allow-session-daemon` or matching account-local consent. Host files and other destinations outside context and Skill folders need shared `--allow-path` grants.

The exact per-command flags, result fields and conformance cases are in the [Tool contracts](../.agents/memory/crystallized/documents/cli/contracts/tool/_tool.md).

## Definitions

JSON and YAML definitions follow the [manifest schema](../schemas/tools/v1/manifest.schema.json). A [catalogue](../schemas/tools/v1/catalogue.schema.json) lists exact manifest hashes and one default pin per tool. Local sources select reviewed definitions and never load adapter code.

## Workspace files

`.agents/open-forge.json` records installed tools, exact versions and active roles. An inactive tool keeps an empty role list. `.agents/open-forge.lock.json` records verified receipts, hashes, generated member values and recovery bundle records. Prior bytes stay in recovery bundles. These paths and native Skills stay fixed when you choose a custom context folder.

Only active providers receive managed MCP members in `.mcp.json` and `.codex/config.toml`, plus their required usage Skill. Member edits keep unrelated entries and comments. Tracked host files block account-specific managed paths until you resolve the shared-file arrangement or leave that host out of managed setup. Open Forge doesn't change Git's index.

## Keep removals

Tool writers honor recorded file, directory and category exclusions. Removing required guidance through root Remove leaves the provider registered but incomplete. Configure and Choose block until you clear every covering exclusion. Tool Remove can release the remaining registration without restoring missing content.

## Limits

The core adds no search proxy, index scheduler, arbitrary scripts, system PATH changes or registry changes. The external program performs search and indexing. Graph answers are leads. Before an edit depends on an edge, read its caller and exact callee and check binding where it matters. Retrieved documents don't replace the workspace loader or activate rules.

## Not decided yet

- Confirm recovery v2 coordinates, native settings snapshots and the simultaneous Status, Doctor and Cleanup reader rollout before tool writer implementation.
- Confirm the provisional Tomlyn 2.10.2 dependency behind the host-config reader.

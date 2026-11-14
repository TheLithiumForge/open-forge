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

## Supply

Open Forge distributes recipes and never ships, mirrors or rehosts tool bytes. Native tools download exact official GitHub release assets with pinned version, URL, size, SHA-256, source commit and build-attestation evidence where published. Node tools use a private official portable runtime pinned from signed `SHASUMS256.txt`. The bundled npm runs `npm ci` from the definition's complete pinned registry lock with integrity hashes and package scripts disabled. Every command and registration names absolute private Node and package paths. Never substitute the user's Node. musl/Alpine has no official Node build.

Missing pins or closure, unsafe archives, size or hash mismatches, failed required signature or attestation checks and missing consent block installation. Upstream license conditions still apply. Tools never self-update. Telemetry defaults off with a visible per-tool opt-in and is shown in Inspect.

## Records

`.agents/open-forge.json` records installed pins and active roles. The generated lock keeps receipts, hashes, lifecycle commands and their results. Planned Tool writes create no `.bak` in Git workspaces. Without Git, sibling backups keep prior managed-file bytes until changed content validates. Backup reporting and Repair restoration are pending. An inactive tool keeps `roles:[]`. Missing receipts don't hide a readable settings entry or establish a verified installation. Settings edits alone don't install or activate a tool.

The decided writer design runs the pinned tool's declared installer, uninstaller and index-cleanup commands. The tool handles agent files, native settings and guidance. Open Forge edits only its settings, lock and managed `.gitignore` section. Choose's agent integration policy needs the maintainer's decision.

Each workspace pins one version per tool. User stores keep each tool and private Node version while a workspace references it. Update selects reviewed catalogue pins only, including exact older pins for downgrades. The catalogue keeps previously reviewed pins. Publish the replacement reference before releasing the old one. Missing references never permit shared deletion. Inspect shows resolved install, runtime, cache, state and workspace paths or an unavailable cause.

Running Install again re-verifies and repairs bytes and reruns the pinned installer. Verified local artifacts can avoid another transfer. Use Status for diagnosis. There is no separate Tool repair or open-path command.

## Removal exclusions

Tool writers check declared command output paths against removed files, directories and applicable categories before execution. Root Remove leaves the provider registered. Configure can't restore excluded output through the installer. Choose never restores excluded output. Grants and automatic mode never clear exclusions. Tool Remove can release the registration without restoring missing content.

Remove runs stored index-cleanup and uninstall commands before releasing the registration, unused Tool ignore rules and store references. The last reference permits version release. Git workspaces create no `.bak`. Without Git, keep sibling backups through validation and restore on failure.

Each active tool needs a Skill describing its purpose, commands, limits and trust notes. Its installer registers MCP and supplies that Skill.

## Proposed execution

Proposed for maintainer review: `open-forge tool exec <tool> -- <args>` runs the pinned binary and private runtime from the current directory. It adds no PATH entry or symlink. It isn't implemented.

## Graph use

Graph results are leads. Before an edit depends on an edge, read the caller and exact callee. Check receiver type, overload and invocation line. Use compiler evidence when binding matters. Empty graph output doesn't establish that no callers exist. Follow the active provider's pin, root, index, process and language limits. Retrieved content doesn't replace the loader or activate workspace rules.

The [shared options](common.md) define global flags, results and runtime-folder rules.

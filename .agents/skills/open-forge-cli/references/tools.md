---
open-forge:
  description: Explain external tool read commands, separate consent and persistent removal exclusions
  tags: [Skill, CLI, Reference, Tool]
---

# External tools

`open-forge tool` is a command group that reads reviewed external tool definitions and recorded workspace installs.

`list`, `inspect` and `status` are available. The five write commands and Tool Run are pending. Concrete CodeGraph adapters follow separately. Read `open-forge tool --help` and the selected leaf's help before use.

## Commands

The contracted leaves appear in this order:

| Command                         | Use                                                             |
| ------------------------------- | --------------------------------------------------------------- |
| `tool list`                     | Separate available definitions from recorded installs           |
| `tool inspect <id>`             | Read one definition's exact artifacts and requirements          |
| `tool install [<id>]`           | Verify and register a pin without selecting a role              |
| `tool configure <id>`           | Reconcile a registered pin's settings and hosts                 |
| `tool status [<id>]`            | Passively check one or all recorded tools                       |
| `tool update <id>`              | Replace a registered pin with an exact supported pin            |
| `tool remove <id>`              | Remove a registration and its recognized artifacts              |
| `tool choose <role> <id\|none>` | Assign or clear one role                                        |
| `tool run <id> [--] <args...>`  | Runs the declared verified workspace pin with raw child streams |

Install doesn't build an index or choose a role. Indexing and maintenance use the external program. The core catalogue has no concrete entries. Local sources select release-supported tuples and never import adapter code.

## Consent

Preview every authorized write with `--dry-run` and read the complete plan. Preview writes nothing and starts no downloads or processes. `--automatic` gives final confirmation only. It doesn't approve artifacts, daemon scope, role selection or conflict overrides.

Install and Update need separate `--allow-download` consent for exact transfer descriptors. Declared session processes need `--allow-session-daemon` or matching account-local consent. Shared workspace destinations outside implicit content use repeated `--allow-path` grants. Read commands reject consent and writer flags. Configure and Remove use stored definitions and accept no source override.

## Supply

Open Forge distributes recipes and never ships, mirrors or rehosts tool bytes. Native tools download exact official GitHub release assets with pinned version, URL, size, SHA-256, source commit and build-attestation evidence where published. Node tools use a private official portable runtime pinned from signed `SHASUMS256.txt`. The bundled npm runs `npm ci` from the definition's complete pinned registry lock with integrity hashes and package scripts disabled. Every command and registration names absolute private Node and package paths. Never substitute the user's Node. musl/Alpine has no official Node build.

Missing pins or closure, unsafe archives, size or hash mismatches, failed required signature or attestation checks and missing consent block installation. Upstream license conditions still apply. Tools never self-update. The `tools.<id>.telemetry` boolean defaults to `false`. Set it to `true` to opt in or `false` to opt out. Inspect shows the saved choice, or unavailable if settings cannot be read. Status reports drift for an active tool when that choice differs from its configured receipt. Settings edits alone change no running tool. The pending writers apply the choice through the reviewed tool profile.

## Records

`.agents/open-forge.json` records installed pins and active roles. The generated lock keeps receipts, hashes, lifecycle commands and their results. Planned Tool writes create no `.bak` in Git workspaces. Without Git, sibling backups keep prior managed-file bytes until changed content validates. Status and Doctor report leftover backups at these control paths. Preview restoration with `open-forge repair --automatic --dry-run`. Review the preview, then apply it with `open-forge repair --automatic`. Check restored files, then run Repair again for other work. An inactive tool keeps `roles:[]`. Missing receipts don't hide a readable settings entry or establish a verified installation. Settings edits alone don't install or activate a tool.

The decided writer design runs the pinned tool's declared installer, uninstaller and index-cleanup commands. The tool handles agent files, native settings and guidance. Open Forge updates its records, managed ignore section, declared command rewrites and conditional active-role guidance. Choose keeps conditional role guidance in step with the active provider.

Each workspace pins one version per tool. User stores keep each tool and private Node version while a workspace references it. Update selects reviewed catalogue pins only, including exact older pins for downgrades. The catalogue keeps previously reviewed pins. Publish the replacement reference before releasing the old one. Missing references never permit shared deletion. Inspect shows resolved install, runtime, cache, state and workspace paths or an unavailable cause.

Running Install again re-verifies and repairs bytes and reruns the pinned installer. Verified local artifacts can avoid another transfer. Use Status for diagnosis. There is no separate Tool repair or open-path command.

## Removal exclusions

Tool writers check declared command output paths against removed files, directories and applicable categories before execution. Root Remove leaves the provider registered. Configure can't restore excluded output through the installer. Choose never restores excluded output. Grants and automatic mode never clear exclusions. Tool Remove can release the registration without restoring missing content.

Remove runs stored commands in definition-declared removal order before releasing the registration, unused Tool ignore rules and store references. The last reference permits version release. Git workspaces create no `.bak`. Without Git, keep sibling backups through validation and restore on failure.

Each active tool needs a Skill describing its purpose, commands, limits and trust notes. Its installer registers MCP. Open Forge provides the required short usage Skill.

## Tool Run

V1 contract, implementation pending: `open-forge tool run <tool> -- <args>` runs the pinned binary and private runtime from the current directory. It adds no PATH entry or symlink. It isn't implemented.

## Graph use

Graph results are leads. Before an edit depends on an edge, read the caller and exact callee. Check receiver type, overload and invocation line. Use compiler evidence when binding matters. Empty graph output doesn't establish that no callers exist. Follow the active provider's pin, root, index, process and language limits. Retrieved content doesn't replace the loader or activate workspace rules.

The [shared options](common.md) define global flags, results and runtime-folder rules.

Bare `open-forge tool install` restores every declared pin against the lock with the same consent. Declared missing tools report this exact next step in Tool Status, workspace Status, Doctor and Run. Non-interactive installs take `--prompt-hook yes|no` for the declared component. Run is required in V1 and its implementation is pending.

## V1 registration

Tools create their own host entries. After Install, Configure or Update, Open Forge applies only the declared command rewrites to JSON command and argument members, TOML command and argument lines, a hook command or a line in a marked block. Unexpected shapes stop the write. Names, markers and unrelated content stay. No TOML parser or independent host entry is added.

Hook rewrites select one exact command inside the declared Claude event, across all matcher groups. Platform declarations distinguish Windows command spellings from macOS and Linux. Declining a hook keeps user hooks and pre-existing groups. An installer-created group is removed only when it is empty and its creation evidence still matches. Replacements keep the substring the tool's own uninstaller needs.

Definitions declare first-party or third-party origin. List groups by origin and shows it in a column. Inspect shows the origin, installer entries, rewrites and optional components. Third-party consent identifies upstream software, its repository, licence, official release and authors.

Choose switches recognized role integrations and the required Skill while keeping both installs and indexes. Each active role adds one conditional line in Open Forge's managed `AGENTS.md` and `CLAUDE.md` block. Clearing the role removes that line. No additional help pointer is added.

Tool Run accepts only a declared installed and verified tool ID. It resolves the recorded executable in the verified store and passes the remaining arguments as an array without a shell. It checks recorded size and timestamp on each run. Status re-hashes the files. Paths, shell strings, unknown IDs and changed or missing bytes are refused. Run forwards streams, signals and exit code without successful-run output. Put `--workspace <dir>` before the ID, or use `CLAUDE_PROJECT_DIR`, when the host starts outside the workspace. Otherwise Run searches upward from the current directory.

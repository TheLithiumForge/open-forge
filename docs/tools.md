# External tools

`open-forge tool` is a command group that reads reviewed external tool definitions and recorded workspace installs.

You install a supported pin, build its index through the external program, then choose its workspace role. Installation doesn't choose a role or build an index. `list`, `inspect` and `status` are available. The five write commands and Tool Run are pending. Concrete CodeGraph adapters follow separately.

## Commands

The contracted command order is:

| Command                         | What it does                                                    |
| ------------------------------- | --------------------------------------------------------------- |
| `tool list`                     | Lists available definitions and recorded installs               |
| `tool inspect <id>`             | Explains one exact definition and its requirements              |
| `tool install [<id>]`           | Verifies and registers a pin without choosing a role            |
| `tool configure <id>`           | Reconciles settings, desired hosts and code root                |
| `tool status [<id>]`            | Passively checks one or all recorded tools                      |
| `tool update <id>`              | Changes a registration to one exact supported pin               |
| `tool remove <id>`              | Removes a registration and its recognized artifacts             |
| `tool choose <role> <id\|none>` | Assigns or clears a workspace role                              |
| `tool run <id> [--] <args...>`  | Runs the declared verified workspace pin with raw child streams |

The core's production catalogue is empty. A local source can select definitions supported by the CLI release. It can't import adapter code or add unreviewed pins. Another program or pin needs an Open Forge release.

## Read tools

Run `open-forge tool list --format json` to read the catalogue and recorded installs. In a workspace with no tool records, the empty core catalogue returns `completed` with empty `available` and `installed` arrays. An unavailable source keeps its cause and independently readable installed rows.

Run `open-forge tool status` to compare recorded files and configuration with passive evidence. It starts no process or database session. An index file without validated completion evidence reports `present-unverified`. A missing receipt reports unavailable verification.

## Consent

The five write commands preview the same plan with `--dry-run`. Preview writes nothing and starts no transfers or processes. `--automatic` gives final confirmation only. Downloads need separate `--allow-download` consent for exact artifacts. Declared session daemons need `--allow-session-daemon` or matching account-local consent. Declared tool outputs and `.gitignore` changes need the shared destination grants shown by the plan.

The exact per-command flags, result fields and conformance cases are in the [Tool contracts](../.agents/memory/crystallized/documents/cli/contracts/tool/_tool.md).

## Supply

Open Forge ships reviewed recipes containing versions, official URLs, hashes and commands. It never ships, mirrors or rehosts tool bytes. Installation downloads from upstream. The upstream license and notices still apply.

Native tools use the exact asset from the project's own GitHub release, pinned by version, URL, size, SHA-256 and source commit. Published build attestations are checked against the reviewed source and build identity. A failed required check blocks installation.

Node tools use a private pinned portable archive from `https://nodejs.org/dist/v<version>/`. Its pinned hash comes from the signed `SHASUMS256.txt`. The runtime stays in the per-user versioned store. The bundled npm runs `npm ci` from the definition's pinned package manifest and complete lockfile, using npm registry URLs and integrity hashes. It resolves no new dependency and runs no package install scripts. Every registration and command uses absolute paths to that Node and package. Your own Node and PATH stay unchanged. musl/Alpine has no official Node build and is unsupported.

Missing pins, incomplete locks, unsafe archives, size or digest mismatches, failed required signature or attestation checks and missing consent block installation. The [admission checklist](../.agents/memory/crystallized/documents/maintenance/tools.md) explains how new recipes and pins are reviewed.

Tools never self-update. Update checks stay disabled. Telemetry defaults off. Set `tools.<id>.telemetry` in `.agents/open-forge.json` to `true` to opt in or `false` to opt out. Omission means `false`. Inspect shows the saved choice, or unavailable when settings cannot be read. Status reports drift for an active tool when the choice differs from its configured receipt. Settings edits alone change no running tool. The pending writers apply the choice through the reviewed tool profile.

JSON and YAML definitions follow the [manifest schema](../schemas/tools/v1/manifest.schema.json). A [catalogue](../schemas/tools/v1/catalogue.schema.json) lists exact manifest hashes and one default pin per tool. Local sources select reviewed definitions and never load adapter code.

## Stores and records

`.agents/open-forge.json` records installed tools, exact versions and active roles. An inactive tool keeps an empty role list. `.agents/open-forge.lock.json` records verified receipts, hashes, lifecycle commands and their results. Planned Tool writes use Git as the backup in Git workspaces. Without Git, sibling `.bak` files keep prior managed-file bytes until changed content validates. Status and Doctor report leftover backups at these control paths. Preview restoration with `open-forge repair --automatic --dry-run`. Review the preview, then apply it with `open-forge repair --automatic`. Check restored files, then run Repair again for other work. These paths and native Skills stay fixed when you choose a custom context folder.

The decided writer design invokes each pinned tool's own install, uninstall and index-cleanup commands. The tool handles agent files, native settings and its guidance. Open Forge updates its records, managed ignore section, declared command rewrites and conditional active-role guidance. Choose keeps the conditional role guidance in step with the active provider. Open Forge doesn't change Git's index.

Each active tool requires a Skill describing its purpose, commands, limits and trust notes. Its installer registers MCP. Open Forge provides the required short usage Skill.

Each workspace pins one version per tool. User stores keep tool and private Node versions while any workspace references them. The last update or removal releases the old version after checking all references. Missing or unreadable references never permit shared deletion.

Default user-store locations are:

| OS      | Tools                                               | Archives                                       | State                                                |
| ------- | --------------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------- |
| Windows | `%LOCALAPPDATA%/OpenForge/tools`                    | `%LOCALAPPDATA%/OpenForge/tool-cache`          | `%LOCALAPPDATA%/OpenForge/tool-state`                |
| macOS   | `~/Library/Application Support/OpenForge/tools`     | `~/Library/Caches/OpenForge/tools`             | `~/Library/Application Support/OpenForge/tool-state` |
| Linux   | `${XDG_DATA_HOME:-~/.local/share}/open-forge/tools` | `${XDG_CACHE_HOME:-~/.cache}/open-forge/tools` | `${XDG_STATE_HOME:-~/.local/state}/open-forge/tools` |

Install keys contain the tool ID, version, target and artifact-set hash. Node runtimes have their own version, target and archive hash. Workspace locks keep recipe snapshots, hashes, files, references and command results. Physical store bindings and process consent stay account-local. Inspect shows resolved paths or why a path is unavailable. There is no separate open-path command.

## Reinstall and update

Running `tool install` again on the same pin re-verifies its bytes, restores missing or changed immutable files and reruns the tool's install command. Verified local artifacts can avoid another download. There is no separate Tool repair command. Use Status to diagnose a problem.

Update selects only pins reviewed in this Open Forge release's catalogue. The catalogue keeps every previously reviewed pin. Select an exact older reviewed pin to downgrade, subject to its install checks. Update publishes the new reference before releasing the old one.

## Keep removals

Tool Remove runs the stored commands in definition-declared removal order, removes unused Tool ignore rules and releases store references. Other workspaces keep their shared versions. Concrete command vectors and native testing belong to the next branch.

Tool writers check declared command outputs against recorded file, directory and category exclusions before running the command. Root Remove leaves the provider registered. Configure can't restore excluded output through an installer. Choose never restores excluded output. Tool Remove can release the registration without restoring missing content.

## Limits

The core adds no search proxy, index scheduler, arbitrary scripts, system PATH changes or registry changes. The external program performs search and indexing. Graph answers are leads. Before an edit depends on an edge, read its caller and exact callee and check binding where it matters. Retrieved documents don't replace the workspace loader or activate rules.

V1 contract, implementation pending: `open-forge tool run <tool> -- <args>` runs the pinned binary with its pinned runtime from your current directory. It adds no PATH entry or symlink. This branch doesn't implement it.

Bare `open-forge tool install` restores every declared pin against the lock with the same consent. Declared missing tools report this exact next step in Tool Status, workspace Status, Doctor and Run. Non-interactive installs take `--prompt-hook yes|no` for the declared component. Run is required in V1 and its implementation is pending.

## V1 registration

Tools create their own host entries. After Install, Configure or Update, Open Forge applies only the declared command rewrites to JSON command and argument members, TOML command and argument lines, a hook command or a line in a marked block. Unexpected shapes stop the write. Names, markers and unrelated content stay. No TOML parser or independent host entry is added.

Hook rewrites select one exact command inside the declared Claude event, across all matcher groups. Platform declarations distinguish Windows command spellings from macOS and Linux. Declining a hook keeps user hooks and pre-existing groups. An installer-created group is removed only when it is empty and its creation evidence still matches. Replacements keep the substring the tool's own uninstaller needs.

Definitions declare first-party or third-party origin. List groups by origin and shows it in a column. Inspect shows the origin, installer entries, rewrites and optional components. Third-party consent identifies upstream software, its repository, licence, official release and authors.

Choose switches recognized role integrations and the required Skill while keeping both installs and indexes. Each active role adds one conditional line in Open Forge's managed `AGENTS.md` and `CLAUDE.md` block. Clearing the role removes that line. No additional help pointer is added.

Tool Run accepts only a declared installed and verified tool ID. It resolves the recorded executable in the verified store and passes the remaining arguments as an array without a shell. It checks recorded size and timestamp on each run. Status re-hashes the files. Paths, shell strings, unknown IDs and changed or missing bytes are refused. Run forwards streams, signals and exit code without successful-run output. Put `--workspace <dir>` before the ID, or use `CLAUDE_PROJECT_DIR`, when the host starts outside the workspace. Otherwise Run searches upward from the current directory.

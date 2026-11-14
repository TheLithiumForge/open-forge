---
open-forge:
  description: Place external tool definitions, stores, host capabilities and finite process profiles in the CLI architecture
  tags: [Memory, Document, CLI, TechnicalDesign, Tool, CurrentTruth]
---

# External tools
External tool support is a CLI capability that downloads verified upstream programs and runs their declared workspace installation commands.

The core handles contracts, catalogue selection, records, declared commands and bounded lifecycle effects. Concrete CodeGraph and Codebase Memory definitions and adapters belong to the next stacked branch. This core ships an empty production catalogue.

Status: V1 core design decided.

## Supply

Open Forge ships a reviewed recipe containing versions, official URLs, hashes and commands. It never ships, mirrors or rehosts a tool's bytes. Installation downloads directly from the official source so upstream serves the download. The tool's upstream license and notices still apply.

Native tools use the exact asset from their project's GitHub release. Pin its version, URL, size, SHA-256 and source commit. Verify a build attestation where upstream publishes one, against the reviewed repository, commit and build identity. A required verification failure blocks execution.

Node tools use an exact official portable archive from `https://nodejs.org/dist/v<version>/`. Pin the archive hash from the release's signed `SHASUMS256.txt` and keep signer evidence in the reviewed recipe. Extract the runtime into the versioned user store. Every npm and tool invocation names the absolute private Node path. The user's Node and PATH stay unchanged. musl/Alpine has no official build and is unsupported.

The package and its complete dependency closure come from the npm registry. The definition pins the package manifest, lockfile and integrity values. The private Node runs its bundled npm's `npm ci` with the reviewed options, package scripts disabled and no dependency resolution or lockfile changes. A missing closure blocks installation. Native tools need no Node runtime.

The [tool admission checklist](../../maintenance/tools.md) defines source review, checksum and attestation evidence, licensing checks, native tests and release admission. A local source can't widen that reviewed recipe.

## Composition

Root composition directly registers the eight Tool leaves in contract order. Compiled adapters are selected by finite typed IDs and revisions. There is no reflective discovery, imported adapter code, service locator or runtime plugin registry. Local sources select exact reviewed tuples through data only.

Framework Tools handles definition facts, catalogue and allow-list checks, template values, store identity, reference records and passive host observations. Framework Settings handles the visible tool settings map. Framework Lifecycle handles verified tool receipts. Framework Tools handles sibling backup mechanics. Framework Recovery keeps existing v1 bundle mechanics. Operations Tool keeps command requests, policy, plans, findings, effects and results. Rendering Tool projects command models. OutputText contains human wording. Namespaces mirror physical paths and local support stays under `Shared/<Capability>/`.

The host provides bounded network and process capabilities through typed requests. Download requests name exact HTTPS descriptors and approved redirect hosts. Process requests name reviewed finite actions, verified executable, separate arguments, complete declared environment, physical CWD, timeout and cancellation. Definitions declare pinned lifecycle argument vectors. They never choose a shell, executable from PATH or imported code. Capabilities return typed facts and don't render or decide domain status.

All JSON graphs use static source generation. YAML uses the existing standard parser and static model context. Native AOT evidence covers every added graph and embedded resource. The core uses no TOML dependency or agent-config editing seam.

## Stores

Default account stores are:

| OS | Installs | Archives | Runtime state |
| --- | --- | --- | --- |
| Windows | `%LOCALAPPDATA%/OpenForge/tools` | `%LOCALAPPDATA%/OpenForge/tool-cache` | `%LOCALAPPDATA%/OpenForge/tool-state` |
| macOS | `~/Library/Application Support/OpenForge/tools` | `~/Library/Caches/OpenForge/tools` | `~/Library/Application Support/OpenForge/tool-state` |
| Linux | `${XDG_DATA_HOME:-~/.local/share}/open-forge/tools` | `${XDG_CACHE_HOME:-~/.cache}/open-forge/tools` | `${XDG_STATE_HOME:-~/.local/state}/open-forge/tools` |

Use the platform's native known-folder resolution. A product-level store override gives tests an owned temporary root without changing OS configuration. Every test redirects all product account stores through one shared test-support seam before use. Tests never download real artifacts or touch real account stores.

`OPENFORGE_TOOL_HOME` selects an absolute test root with `tools`, `tool-cache` and `tool-state` children. Normal runs use the platform locations above. The shared test data-home helper sets this override before tests run.

The account-local `tool-state/registry.json` keeps root identities, bounded units, file hashes, workspace references and both sides of dependency references. Pending units stay separate from verified bytes. An existing store without its complete registry is unavailable coverage. Initialization publishes a private directory through an atomic rename. Later registry changes require the store file lease. Large transfers happen outside that lease. Byte replacement shares the Tool backup mechanism and rechecks expected content and the live lease before publication.

Private staging uses `.staging/<operation-id>` inside the destination store area. The pending unit records `stagingPath`, expected member hashes and whether that unit was previously verified. Save that record before moving files. Recheck destination bytes before each move. Verify all members and Unix executable bits before marking the unit verified. An interruption keeps the pending record and any unmoved staged members. A retry uses fresh staged bytes and checks the saved members before removing the old stage. Unrecognized members block the retry. Changed registered members block a unit that has never been verified.

Reusing a verified unit rechecks its complete dependency closure before adding a workspace reference. Releasing a nonfinal reference keeps the shared bytes. The final release checks complete references, hashes and directory coverage before deleting known members and empty directories. Foreign or changed members block deletion. Dependency references are released only after the parent unit is removed.

Install keys are `<id>/<version>/<target>/<artifact-set-sha256>`. For one archive, the set digest is its SHA-256. For several artifacts, hash ordinally sorted UTF-8 `id=integrity` lines, each ending in LF. Archive keys use artifact digests. Private Node runtimes use their own version, target and archive digest keys and share references by the same rules. Account-local records bind exact physical units and keep shared references and local process consent outside Git. Custom account roots and unmanaged adoption are deferred.

Store bindings cover one install, archive, runtime-state or declared native-cache unit. Their hashes cover kind and resolved physical root. They never grant a whole home or global store parent. Runtime state uses physical worktree, code-root and execution-environment identities. Branch names and shared Git history aren't identities. Immutable bytes can be shared. Mutable indexes can't be shared without a tested native content-key contract.

## Workspace records

Settings v6 adds the direct tool-ID map to `.agents/open-forge.json`. Lock v2 adds detailed tool receipts to `.agents/open-forge.lock.json`. These fixed paths and `.agents/skills` don't move with `mainRoot`. The [manifest interface](../contracts/shared/tool-manifest/interface.md#workspace-settings) defines settings, lock meaning and paired-write rules.

Account records supplement the lock with physical bindings, shared references and local process consent. An unreadable reference set is unavailable coverage. It never becomes an empty set that permits final shared deletion. Keep disappeared-worktree references until authorized cleanup resolves them.

Paired record publication checks Tool pins, hosts, roots and execution summaries against the detailed lock before changing files. It rechecks both expected records under the workspace lease, then writes and validates the pair through the Tool backup flow. After failure or cancellation, restore prior bytes only while the target still matches this operation's writes. Keep concurrent changes and report any unfinished restoration or kept backup. A first publication creates only the fixed control parent and removes its unchanged new files after failure.

Command planners form the ordered process requests before consent. Execution starts after consent and permission checks pass. Execution saves the complete proposed command list before launch. The process host reports a start after creating the child. That notification saves started evidence. Exit, failure and interruption update the command record before another command can start. A failed command or failed evidence write stops the sequence. In-memory results keep any outcome that couldn't be saved. Record recovery never guesses an inverse tool command.

Each workspace records one version per tool. Update selects only pins reviewed in this Open Forge release's catalogue, which keeps every previously reviewed pin. An exact older pin permits a downgrade. Publish the replacement reference before releasing the old one. Keep tool bytes and private runtimes while any workspace references them. Release an unreferenced version after the last update or removal and a complete reference check.

## Reinstall

Running Install again on the same pin re-verifies its bytes, restores missing or changed immutable files and reruns the pinned install command. Verified local artifacts can avoid another transfer, but they don't skip the installer. Foreign bytes aren't adopted. There is no separate Tool repair command. Status diagnoses and Inspect shows resolved install, runtime, cache, state and workspace paths with a cause for any unavailable path.

## Process settings

Tools never self-update. Every supplied launch disables update checks according to its reviewed profile. Telemetry defaults off and Inspect shows the saved choice. If settings cannot be read, the choice is unavailable. The visible `tools.<id>.telemetry` boolean lets you opt in and turn it off again. Omission means `false`. The receipt records the applied choice, so active-tool Status can report drift after a settings edit. The installer and later launch instructions use that choice. A recipe can't silently force telemetry on or silently replace an opt-in with off. Watching and index policy keep their separate controls.

## Host members

Tool-owned installers register their MCP server with agent hosts and handle native settings, helpers and usage guidance. Each active tool requires a Skill that explains its purpose, commands, limits and trust notes. Open Forge edits only its settings, lock and managed ignore section. It doesn't parse or write Codex TOML or write Claude JSON. Passive host-entry observations are information and aren't generated-member receipts.

Definitions declare pinned install, uninstall and index-cleanup argument vectors, scope, timeout and output paths. The verified target entry runs with separate arguments, declared environment and physical workspace CWD. Exact release admission covers these declarations. The [shared Tool rules](../shared-operation-contract.md#tool-commands) define command ordering and execution evidence.

## Ignore editing

Extract only neutral byte-preserving section mechanics from the existing Library ignore editor when Tool becomes its second consumer. Keep marker policy, desired paths, receipts, grants and deletion decisions with each family. Tool markers and exclusion rules live in the shared operation contract. Keep surrounding bytes, BOM and original line endings.

## Adapter profiles

Adapters provide finite configuration, passive checks and external maintenance instructions. They don't provide a query proxy, scheduler or index command inside Open Forge. Required guidance names the pin, root, state, commands, process limits and language trust notes. Retrieval doesn't activate workspace instructions.

CodeGraph's future profile validates a single-segment in-project index directory. It defaults telemetry off and applies the visible telemetry choice while keeping daemon and watcher opt-outs. Every supplied launch replaces hook-related child environment with an empty `GIT_CONFIG_PARAMETERS`, count one, key `core.hooksPath` and a verified private sink. Configuration and external recipes check Git's resolved hooks path with that exact environment before proceeding. File-level Git configuration and real hooks stay unchanged. The reviewed bootstrap helper creates schema only through pinned bundled APIs. The next branch chooses the pinned installer, uninstaller and index-cleanup arguments. The core contains no CodeGraph-specific invocation.

Codebase Memory's future profile accepts only code root `.` and physical workspace CWD. Verify native project identity and index binding for both hosts. Use one exact build and resolved account cache for all active clients. Session consent covers the build, cache, process scope and final-client shutdown. Native settings turn automatic indexing, watching and UI off while keeping unrelated settings. Preflight ancestor permissions. A rejected dedicated cache requires a separate default-cache plan, never a silent switch or permission change.

After changed CBM sources, print an exact-project full-rebuild recipe. It deletes the selected native project, verifies that project's database absent, then indexes the exact workspace. It never deletes the account cache. CodeGraph uses external manual sync. Neither profile treats database presence as fresh completion evidence.

## Native tests

Definition support and installation testing are separate from call accuracy. Concrete adapters need both host journeys on each matching native target. Cross-builds and emulation don't replace native evidence. Check pristine first index, Unicode and spaced paths, conflicting inherited hooks, default-off telemetry with opt-in, no watchers, final-client exit, offline restart and complete removal while other worktrees stay usable.

The core uses minimal synthetic test-only definitions and local hashed artifacts. They aren't production catalogue entries or public adapters. Cover parser, records and effect mechanics at Unit and real-filesystem Integration boundaries, then small published-command journeys at EndToEnd. Trigger complete managed and host Native AOT gates for composition, serialization, stores and mutation changes.

## Not decided yet

- Concrete installer, uninstaller and index-cleanup vectors need native tests on the next branch. They don't change this core's generic command contract.
- Proposed: `open-forge tool exec <tool> -- <args>` runs the workspace's pinned binary with its private runtime from the current directory. It passes arguments separately, uses absolute program paths and creates no PATH entry or symlink. This is a specification proposal only. No Exec command is implemented.
- Whether Choose switches agent integrations through declared commands or changes only the role map needs the maintainer's decision.
- Unix Node archive preparation needs a decision on unused launcher links. The upstream [packaging code](https://github.com/nodejs/node/blob/main/tools/install.py) creates npm and npx symlinks. The current archive rule rejects links. Proposed: a reviewed recipe can name exact unused launcher links to omit, while creating no links and rejecting every other link. Don't admit a dependent recipe before this is decided.

The [CLI Architecture](../architecture.md) defines the parent layer and dependency rules.

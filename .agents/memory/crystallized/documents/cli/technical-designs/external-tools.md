---
open-forge:
  description: Place external tool definitions, stores, host capabilities and finite process profiles in the CLI architecture
  tags: [Memory, Document, CLI, TechnicalDesign, Tool, CurrentTruth]
---

# External tools
External tool support is a CLI capability that installs reviewed program bytes and reconciles their workspace configuration.

The core handles contracts, catalogue selection, records, declared commands and bounded lifecycle effects. Concrete CodeGraph and Codebase Memory definitions and adapters belong to the next stacked branch. This core ships an empty production catalogue.

Status: V1 core design decided.

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

Install keys are `<id>/<version>/<target>/<artifact-set-sha256>`. For one archive, the set digest is its SHA-256. For several artifacts, hash ordinally sorted UTF-8 `id=integrity` lines, each ending in LF. Archive keys use artifact digests. Account-local records bind exact physical units and keep shared references and local process consent outside Git. Custom account roots and unmanaged adoption are deferred.

Store bindings cover one install, archive, runtime-state or declared native-cache unit. Their hashes cover kind and resolved physical root. They never grant a whole home or global store parent. Runtime state uses physical worktree, code-root and execution-environment identities. Branch names and shared Git history aren't identities. Immutable bytes can be shared. Mutable indexes can't be shared without a tested native content-key contract.

## Workspace records

Settings v6 adds the direct tool-ID map to `.agents/open-forge.json`. Lock v2 adds detailed tool receipts to `.agents/open-forge.lock.json`. These fixed paths and `.agents/skills` don't move with `mainRoot`. The [manifest interface](../contracts/shared/tool-manifest/interface.md#workspace-settings) defines settings, lock meaning and paired-write rules.

Account records supplement the lock with physical bindings, shared references and local process consent. An unreadable reference set is unavailable coverage. It never becomes an empty set that permits final shared deletion. Keep disappeared-worktree references until authorized cleanup resolves them.

## Host members

Tool-owned installers handle agent configuration, native settings, helpers and usage guidance. Open Forge edits only its settings, lock and managed ignore section. It doesn't parse or write Codex TOML or write Claude JSON. Passive host-entry observations are information and aren't generated-member receipts.

Definitions declare pinned install, uninstall and index-cleanup argument vectors, scope, timeout and output paths. The verified target entry runs with separate arguments, declared environment and physical workspace CWD. Exact release admission covers these declarations. The [shared Tool rules](../shared-operation-contract.md#tool-commands) define command ordering and execution evidence.

## Ignore editing

Extract only neutral byte-preserving section mechanics from the existing Library ignore editor when Tool becomes its second consumer. Keep marker policy, desired paths, receipts, grants and deletion decisions with each family. Tool markers and exclusion rules live in the shared operation contract. Keep surrounding bytes, BOM and original line endings.

## Adapter profiles

Adapters provide finite configuration, passive checks and external maintenance instructions. They don't provide a query proxy, scheduler or index command inside Open Forge. Required guidance names the pin, root, state, commands, process limits and language trust notes. Retrieval doesn't activate workspace instructions.

CodeGraph's future profile validates a single-segment in-project index directory. It sets telemetry, daemon and watcher opt-outs together. Every supplied launch replaces hook-related child environment with an empty `GIT_CONFIG_PARAMETERS`, count one, key `core.hooksPath` and a verified private sink. Configuration and external recipes check Git's resolved hooks path with that exact environment before proceeding. File-level Git configuration and real hooks stay unchanged. The reviewed bootstrap helper creates schema only through pinned bundled APIs. The next branch chooses the pinned installer, uninstaller and index-cleanup arguments. The core contains no CodeGraph-specific invocation.

Codebase Memory's future profile accepts only code root `.` and physical workspace CWD. Verify native project identity and index binding for both hosts. Use one exact build and resolved account cache for all active clients. Session consent covers the build, cache, process scope and final-client shutdown. Native settings turn automatic indexing, watching and UI off while keeping unrelated settings. Preflight ancestor permissions. A rejected dedicated cache requires a separate default-cache plan, never a silent switch or permission change.

After changed CBM sources, print an exact-project full-rebuild recipe. It deletes the selected native project, verifies that project's database absent, then indexes the exact workspace. It never deletes the account cache. CodeGraph uses external manual sync. Neither profile treats database presence as fresh completion evidence.

## Native tests

Definition support and installation testing are separate from call accuracy. Concrete adapters need both host journeys on each matching native target. Cross-builds and emulation don't replace native evidence. Check pristine first index, Unicode and spaced paths, conflicting inherited hooks, no telemetry or watchers, final-client exit, offline restart and complete removal while other worktrees stay usable.

The core uses minimal synthetic test-only definitions and local hashed artifacts. They aren't production catalogue entries or public adapters. Cover parser, records and effect mechanics at Unit and real-filesystem Integration boundaries, then small published-command journeys at EndToEnd. Trigger complete managed and host Native AOT gates for composition, serialization, stores and mutation changes.

## Not decided yet

Concrete installer, uninstaller and index-cleanup vectors need native tests on the next branch. They don't change this core's generic command contract.

The [CLI Architecture](../architecture.md) defines the parent layer and dependency rules.

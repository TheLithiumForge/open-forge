---
open-forge:
  description: Check catalogue entries before adding supported tool versions
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool entry review

Tool entry review is a process that checks a tool's published channels and additive configuration recipe.
For example, CodeGraph's supported range can be declared while its 1.6.2 release archive still waits for its attestation finding to be resolved.

Status: V1 catalogue specified. Entries are implemented and reviewed in lane E after step 0 review.

## Source

The [Tool setup design](../cli/technical-designs/tool-setup.md) defines the catalogue and installation flow.
The [Tool records](../cli/contracts/shared/tool-records/interface.md) define portable and local facts.
The typed catalogue will live under `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Catalogue/`.

## Contract

Before admitting a profile, check each item:

1. Record ID, name, origin, upstream and licence. Confirm the package and release are published by that upstream.
2. Name every advertised channel and its runtime, minimum manager, OS, CPU and Linux libc requirements. Mark untested or unsupported targets honestly.
3. Check the bounded version probe and parser against real output. The probe mustn't invoke installation, setup or indexing.
4. Define non-overlapping major.minor ranges, exact tested versions and the minimum Open Forge version.
5. Resolve every direct-download asset to a pinned SHA-256 or verified attestation. Check expected repository, workflow and source commit. Block mismatches.
6. For private Node, verify signed SHASUMS256 using upstream release keys and pin the official portable archive hash. Use that runtime's npm and test native addons on the matching target.
7. Disclose package-manager integrity and install-script behavior. Describe it as manager-verified, weaker than a reviewed dependency closure.
8. Record the setup argument vector and every declared project or account effect. Keep tool hooks, permissions, watchers and other behavior. Disclose telemetry off.
9. Define each config change, precondition, read-back and removal action. Check no-op, migration, drift, prior absence and unrelated-byte retention.
10. Verify absolute-path launchers, runtime pinning, direct command resolution and updates. Verify Windows MCP `cmd /c <tool>` for `.cmd` launchers in native tests.
11. Test PATH choices, chosen shell files including a custom file, restart instructions, lost-state marks and confirmed final removal. Never change system PATH.
12. Add the direct-command Skill and conditional managed-block line. Confirm setup completion, update, failure and removal on each reviewed native target.

Open Forge ships recipes and guidance, not upstream tool bytes.
Existing source and schema versions stay unchanged when adding a profile.
Any new config kind, channel adapter or dependency requires its own design review.

### CodeGraph

The entry specification is:

| Field              | Value                                                                     |
| ------------------ | ------------------------------------------------------------------------- |
| ID and name        | `codegraph`, CodeGraph                                                    |
| Origin and licence | `third-party`, MIT                                                        |
| Upstream           | `https://github.com/colbymchenry/codegraph`                               |
| Profile            | `1.6.x`, examined version `1.6.2`                                         |
| Channels           | npm `@colbymchenry/codegraph` and GitHub release                          |
| Detection          | `codegraph --version`, exact stable SemVer parser                         |
| Setup              | `["install", "--yes", "--location", "local", "--target", "claude,codex"]` |
| Added config       | Allow-listed `["telemetry", "off"]`, JSON Pointer `/exclude` union        |
| Archive exclusion  | `<mainRoot>/memory/archived/**`                                           |
| Guidance           | Proposed Skill `codegraph-usage`, direct command `codegraph`              |

The release bundles Node. The npm wrapper uses a host Node to locate that bundle.
The [npm wrapper](https://github.com/colbymchenry/codegraph/blob/v1.6.2/scripts/npm-shim.js) can download a missing platform package on first launch.
Disclose that upstream fallback. Don't disable it.
The [source package](https://github.com/colbymchenry/codegraph/blob/v1.6.2/package.json) declares Node `>=20.0.0 <25.0.0`.
That isn't the published wrapper's tested minimum.
Lane E must verify the npm-channel requirement and minimum manager before entry review.

Release assets use these names:

| Target        | Asset                           |
| ------------- | ------------------------------- |
| Windows x64   | `codegraph-win32-x64.zip`       |
| Windows ARM64 | `codegraph-win32-arm64.zip`     |
| macOS x64     | `codegraph-darwin-x64.tar.gz`   |
| macOS ARM64   | `codegraph-darwin-arm64.tar.gz` |
| Linux x64     | `codegraph-linux-x64.tar.gz`    |
| Linux ARM64   | `codegraph-linux-arm64.tar.gz`  |

The URL is `https://github.com/colbymchenry/codegraph/releases/download/v<version>/<asset>`.
Per-asset hashes are copy candidates from `origin/feature/tool-code-graphs:src/tools/drafts/codegraph-1.6.2.json`.
They need fresh review. The 1.6.2 attested workflow commit differs from the tag commit in Council 1.
Keep that archive blocked until the finding is resolved. Don't reuse its old entry review flag.

The local installer writes these project files:

| Target        | File                    | Effect                                     |
| ------------- | ----------------------- | ------------------------------------------ |
| Claude        | `.mcp.json`             | CodeGraph MCP entry                        |
| Claude        | `.claude/settings.json` | Prompt hook and permissions                |
| Claude        | `.claude/CLAUDE.md`     | Native instruction block                   |
| Codex         | `.codex/config.toml`    | Native MCP section                         |
| Codex         | `AGENTS.md`             | Native instruction block                   |
| Claude legacy | `.claude.json`          | Migrate an existing legacy local MCP entry |

The upstream [Claude target](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/installer/targets/claude.ts) and [Codex target](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/installer/targets/codex.ts) declare those local paths.
Back up the actual declared destinations before invoking it.
Keep all native blocks, hook commands and permissions.
The [installer source](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/installer/index.ts) is the source for the final written-file inventory.
Telemetry off affects the account.
The [telemetry commands](https://github.com/colbymchenry/codegraph/blob/v1.6.2/TELEMETRY.md#turning-it-off) provide `telemetry status`, `telemetry off` and `telemetry on`.
The off command removes the local identity and unsent queues. Turning it on creates a new identity.
Disclose those effects. Guarded removal restores the setting, not the deleted telemetry history.
The read-back parser records the setting without copying the machine ID into project records.

The proposed Skill resource is `src/tools/skills/codegraph-usage/SKILL.md`.
Its proposed conditional managed-block line is "Use `codegraph` for code navigation and check returned relationships in source."
The line appears only while the tool is active.

CodeGraph [creates an internal ignore file](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/directory.ts#L831-L900) when it creates its index directory.
It ignores transient contents and keeps its own ignore file.
Its internal ignore therefore doesn't ignore the whole directory.
Check whether `.codegraph/` is already ignored at setup time.
Add an Open Forge marked root `.gitignore` block only if that directory isn't already ignored.
Don't create an index just to establish ignore coverage.

### Codebase Memory

The entry specification is:

| Field              | Value                                                                           |
| ------------------ | ------------------------------------------------------------------------------- |
| ID and name        | `codebase-memory`, Codebase Memory                                              |
| Origin and licence | `third-party`, MIT                                                              |
| Upstream           | `https://github.com/DeusData/codebase-memory-mcp`                               |
| Profile            | `0.12.x`, examined version `0.12.1`                                             |
| Channel            | GitHub release                                                                  |
| Detection          | `codebase-memory-mcp --version`, exact stable SemVer parser                     |
| Placement          | Native binary in `~/.open-forge/bin/`                                           |
| Setup              | Absent. Skip account-wide setup                                                 |
| Project config     | JSON Pointer MCP entry, Codex TOML marked section and `.cbmignore` marked block |
| Guidance           | Proposed Skill `codebase-memory-usage`, direct command `codebase-memory-mcp`    |

Its release URL is `https://github.com/DeusData/codebase-memory-mcp/releases/download/v<version>/<asset>`.
Windows assets use `codebase-memory-mcp-windows-{amd64|arm64}.zip`.
macOS assets use `codebase-memory-mcp-darwin-{amd64|arm64}.tar.gz`.
Linux assets use `codebase-memory-mcp-linux-{amd64|arm64}-portable.tar.gz`.
Per-asset hash candidates are in `origin/feature/tool-codebase-memory:src/tools/drafts/codebase-memory-0.12.1.json`.
Verify their hashes, OS minimums, CPU and libc support before entry review.

The proposed JSON Pointer is `/mcpServers/codebase-memory` in `.mcp.json`.
Its value has direct command `codebase-memory-mcp`, empty args and project CWD.
The Codex section is `[mcp_servers.codebase-memory]` inside unique Open Forge markers.
The `.cbmignore` block excludes `/<mainRoot>/memory/archived/`.
Host formats and an environment root binding need native verification. Exact key names are review proposals.
Portable config mustn't embed a user's home path.

The proposed Skill resource is `src/tools/skills/codebase-memory-usage/SKILL.md`.
Its proposed conditional managed-block line is "Use `codebase-memory-mcp` for code navigation and check returned relationships in source."
The line appears only while the tool is active.

Its [configuration reference](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/docs/CONFIGURATION.md) describes account-wide native settings.
Defaults are `auto_index=false`, `auto_watch=true` and `watcher_enabled=true`.
Disclose those defaults. Keep them.
Removal never invokes account-wide setup or deletes every project's indexes.

## Verification

Use small synthetic entries and owned temporary homes for generic tests.
Check each reviewed tool natively on Windows, macOS and Linux, each on x64 and ARM64 where its requirements allow it.
A cross-build isn't native execution evidence.
Use focused semantic assertions and reviewed output snapshots.
CI supplies the full managed and Native AOT gates.
The implementation lanes add profile, codec and lifecycle tests. Step 0 only compiles shared shapes.

## Not decided yet

- Which exact minimum Open Forge release should each first profile declare?
- What npm runtime and manager minimum does the published CodeGraph wrapper support?
- What minimum OS, CPU and libc versions are reviewed for each release asset?
- How is the CodeGraph 1.6.2 attestation and tag mismatch resolved?
- How should the telemetry parser distinguish the stored value from a value forced by the caller's environment?

---
open-forge:
  description: Review native installer recipes and additive configuration every few months
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool entry review

Tool entry review is a process that checks published installation channels and additive configuration.
For example, a CodeGraph release uses pinned asset hashes while its native installer keeps its own hook and permissions.

Status: Round-2 rules decided on 14 November 2026. Catalogue entries and native journeys follow implementation.

## Source

The [Tool setup design](../cli/technical-designs/tool-setup.md) defines the catalogue.
The [record contracts](../cli/contracts/shared/tool-records/_tool-records.md) define exact versions and additive receipts.
The current releases checked on 14 November 2026 are [CodeGraph 1.6.2](https://github.com/colbymchenry/codegraph/releases/tag/v1.6.2) and [Codebase Memory 0.12.1](https://github.com/DeusData/codebase-memory-mcp/releases/tag/v0.12.1).
The detailed round-2 decision source is `of-tool-setup-decisions-2.md`.

## Contract

Before adding or updating a profile, complete these checks:

1. Record ID, name, origin, upstream and licence.
2. Check the published channel's runtime, manager, OS, CPU and libc requirements.
3. Test bounded version detection and environment overlays. CodeGraph uses `CODEGRAPH_NO_DOWNLOAD=1` for probes.
4. Declare the major.minor profile, minimum reviewed patch, tested versions and minimum Open Forge build.
5. Pin each direct asset's SHA-256 or verified attestation. CodeGraph uses published SHA256SUMS. Reject digest mismatches.
6. For private Node, verify signed SHASUMS256, skip archive link entries before the link-rejecting extractor and use Node's own npm CLI script.
7. Detect and test npm-family clients, project-lockfile preference, Yarn Modern exclusion and manager-global-bin readiness.
8. Record native setup and uninstall commands, dry-run support and project or account effects. Keep native MCP names, hooks, permissions, blocks and Skills.
9. Check JSON union and absent-only set, marked blocks, absent-only files and native account settings against their additive removal rules.
10. Verify exact lock versions, channels and added items, keys, markers and hashes. Keep native previous values in machine state.
11. Test absolute launchers, private runtime compatibility, PATH choices and unattended instructions. Never edit real user PATH or shell profiles in journeys.
12. Run real direct-command and MCP journeys, update and native-first removal. Keep full suites in CI.

Open Forge adds no per-tool Skill or managed-block line.
It doesn't rewrite upstream integrations.
The normal process runner uses argument vectors, not shell strings.
A native uninstall that requires PATH or shared-binary effects needs live confirmation.

### Review interval

Review the catalogue every three months and after an upstream compatibility or integrity report.
Check current release, recipe commands, native effects, hashes, requirements and tests.
Keep older supported profiles and exact pins that existing locks use.
Update the review date and source links with every entry change.
Use the release that first ships an entry as its minimum Open Forge version.
A profile starts at the current reviewed tool patch and doesn't silently raise that minimum on a later periodic review.

### CodeGraph

The V1 entry has these values:

| Field                   | Value                                                                                     |
| ----------------------- | ----------------------------------------------------------------------------------------- |
| ID and name             | `codegraph`, CodeGraph                                                                    |
| Origin and licence      | `third-party`, MIT                                                                        |
| Upstream                | `https://github.com/colbymchenry/codegraph`                                               |
| Profile and first patch | `1.6.x`, `1.6.2`                                                                          |
| Channels                | npm `@colbymchenry/codegraph` and GitHub release                                          |
| Setup                   | `["install", "--yes", "--location", "local", "--target", "claude,codex"]`                 |
| Uninstall               | `["uninstall", "--yes", "--location", "local", "--target", "claude,codex", "--keep-cli"]` |
| Added config            | `telemetry off`, `codegraph.json` `/exclude` union with `<mainRoot>/memory/archived/**`   |
| Probe                   | `--version`, with `CODEGRAPH_NO_DOWNLOAD=1`                                               |

The release bundles Node.
Its published npm wrapper uses the host Node to locate that bundle.
The entry's initial supported wrapper runtime is Node 20 or newer, checked separately from the selected client's own runtime requirements.
This is an Open Forge support floor, not a claim that upstream publishes an engines minimum.
Keep its normal missing-platform download fallback outside probes.

Release assets are:

| Target        | Asset                           |
| ------------- | ------------------------------- |
| Windows x64   | `codegraph-win32-x64.zip`       |
| Windows ARM64 | `codegraph-win32-arm64.zip`     |
| macOS x64     | `codegraph-darwin-x64.tar.gz`   |
| macOS ARM64   | `codegraph-darwin-arm64.tar.gz` |
| Linux x64     | `codegraph-linux-x64.tar.gz`    |
| Linux ARM64   | `codegraph-linux-arm64.tar.gz`  |

Use `https://github.com/colbymchenry/codegraph/releases/download/v<version>/<asset>`.
Pin hashes from that release's published `SHA256SUMS`.
The older draft's URLs and hashes are copy candidates, not a replacement for checking the published file.
Council 1's attested workflow commit and tag commit mismatch stays a note.
It doesn't block this SHA-256 channel.

Native local setup writes Claude MCP, settings, hook, permissions and instruction block, plus Codex config and root AGENTS.md.
It can migrate an older local `.claude.json` entry.
Back up those declared files, keep the installer output unchanged and record actual effects in E's journey.
Don't add an Open Forge Skill or line.

Telemetry off is account-wide and deletes the upstream identity and unsent queues.
Read the stored setting through the native status command with telemetry overrides removed for that probe.
Keep the caller's normal environment and report any override separately.
Machine state records only the previous setting, not telemetry identity or queues.

CodeGraph creates an internal ignore file when its index directory is created.
Check effective `.codegraph/` coverage.
Add an Open Forge marked root `.gitignore` block only when the tool's own output and existing rules don't already cover it.
Don't create an index just to get ignore coverage.

### Codebase Memory

The V1 entry has these values:

| Field                   | Value                                                          |
| ----------------------- | -------------------------------------------------------------- |
| ID and name             | `codebase-memory`, Codebase Memory                             |
| Origin and licence      | `third-party`, MIT                                             |
| Upstream                | `https://github.com/DeusData/codebase-memory-mcp`              |
| Profile and first patch | `0.12.x`, `0.12.1`                                             |
| Channel                 | GitHub release, native binary in `~/.open-forge/bin/`          |
| Setup                   | `["install", "--yes", "--skip-binary", "--dir", "<bin>"]`      |
| Native preview          | Add `--dry-run` to that setup vector                           |
| Uninstall               | `["uninstall", "--yes", "--dir", "<bin>"]`                     |
| Added config            | A marked `.cbmignore` block for `/<mainRoot>/memory/archived/` |

The documented `--skip-binary` mode configures agents while keeping the already placed binary and PATH unchanged.
`--dir` identifies the real Open Forge bin for native registrations.
Leave native client discovery at its default and disclose account scope.
The [upstream CLI](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c) supplies these flags and its account-wide uninstall.
Open Forge writes no MCP entry, Codex TOML section, hook, instruction block or usage Skill.
Keep the native watcher defaults.

Native uninstall includes binary removal and, on Windows, persistent user PATH cleanup.
Without live confirmation, print its command and leave removal pending.
Never pass `--delete-indexes`.
The native command keeps indexes by default.

Use `https://github.com/DeusData/codebase-memory-mcp/releases/download/v<version>/<asset>`.
Windows names use `codebase-memory-mcp-windows-{amd64|arm64}.zip`.
macOS names use `codebase-memory-mcp-darwin-{amd64|arm64}.tar.gz`.
Linux names use `codebase-memory-mcp-linux-{amd64|arm64}-portable.tar.gz`.
Verify exact hashes and runtime target requirements before compiling the entry.

## Verification

Generic tests use synthetic entries, local artifacts and owned temporary homes.
E runs real Windows install, status, direct command, MCP, update and remove journeys for both tools.
It records exactly what native setup wrote and what Open Forge added or removed.
Verify that temporary native commands can't edit real shell profiles or real persistent user PATH.
CI supplies other native targets and full suites.
Profile review never redistributes tool bytes.

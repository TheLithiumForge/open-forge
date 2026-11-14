---
open-forge:
  description: Checked upstream recipes, asset pins and isolated Windows journey prerequisites for Task 90
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Upstream recipes

Upstream recipes are the checked release facts and native command plans for compiling the two V1 tool entries.
For example, Codebase Memory configures discovered account clients after Open Forge places its verified binary.

Status: Catalogue candidate accepted for staged integration. The commit continuation uses base `6d007254d5ca2937f4874ba5a93161f585814d8c`. Native journeys and final user docs follow Root composition.

## Contents

- [Baseline](#baseline)
- [Releases](#releases)
- [Native recipes](#native-recipes)
- [Added configuration](#added-configuration)
- [Model mapping](#model-mapping)
- [Windows journeys](#windows-journeys)
- [Verification](#verification)
- [Not decided yet](#not-decided-yet)

## Baseline

The clean execution base is `2e517348ef19e34afa6297c7efa7d0db5451b45c`.
The [lane packet](../packets/lane-e.md) records the research runs and the coordinator's catalogue transfer. This continuation also changes the Catalogue files and their focused Unit tests.
The [entry checklist](../../../../crystallized/documents/maintenance/tools.md) controls the later entry review.
The [decided design](../../../../crystallized/documents/cli/technical-designs/tool-setup.md) supplies C1 through C7, including Node 20 for the CodeGraph wrapper, interactive account uninstall and kept account settings.

Required context was read, including the complete C# design and style files, CLI architecture, selected Task 90, Tool contracts and record contracts.
Council 2 `tools.md`, `node.md` and `reuse.md` were read from `D:/Repositories/open-forge-worktrees/reviews/council-tool-redesign/`.
Council 1's synthesis and relevant advocate findings were read from the sibling `council-tool-isolation/` folder.
Their earlier suggestions to remove hooks, skip native CBM setup, write MCP entries or add Open Forge Skills are superseded.

Old facts were read with these exact local Git object paths:

- `origin/feature/tool-code-graphs:src/tools/drafts/codegraph-1.6.2.json`
- `origin/feature/tool-codebase-memory:src/tools/drafts/codebase-memory-0.12.1.json`

The first research run changed research Markdown only.
Git can recover it, and the normal BCL, existing Markdown checker and Prettier cover verification.
Exceptional machinery: none.
The catalogue continuation adds a managed build and focused Unit checks for its new source. The first-run documentation receipts stay below.
Full suites, Native AOT, real installation and payload snapshot regeneration belong to later integration.

## Releases

The official [CodeGraph release](https://github.com/colbymchenry/codegraph/releases/tag/v1.6.2) was published at `2026-10-03T06:41:20Z`.
The official [Codebase Memory release](https://github.com/DeusData/codebase-memory-mcp/releases/tag/v0.12.1) was published at `2026-10-16T23:35:48Z`.
Both release tags, GitHub asset metadata, checksum files, tagged source and npm metadata were checked on 14 November 2026.
Both projects publish an MIT licence in their tagged `LICENSE` file.
Open Forge distributes recipes and downloads upstream assets when authorized.

Publication facts are:

| Field                 | CodeGraph                            | Codebase Memory                                     |
| --------------------- | ------------------------------------ | --------------------------------------------------- |
| ID, name, origin      | `codegraph`, CodeGraph, `ThirdParty` | `codebase-memory`, Codebase Memory, `ThirdParty`    |
| Repository            | `colbymchenry/codegraph`             | `DeusData/codebase-memory-mcp`                      |
| Profile, first patch  | `1.6.x`, `1.6.2`                     | `0.12.x`, `0.12.1`                                  |
| V1 channels           | `npm`, `github-release`              | `github-release`                                    |
| Published npm package | `@colbymchenry/codegraph@1.6.2`      | `codebase-memory-mcp@0.12.1`, outside this V1 entry |
| npm publication       | `2026-10-03T06:45:46.734Z`           | `2026-10-16T23:37:27.444Z`                          |
| npm bin               | `codegraph` -> `npm-shim.js`         | `codebase-memory-mcp` -> `bin.js`                   |
| Wrapper requirement   | Node >=20 by C6                      | Published npm `engines.node` is `>=18`              |
| Native runtime        | Bundle includes Node `24.16.0`       | Native executable, no Node requirement              |

The [CodeGraph registry metadata](https://registry.npmjs.org/@colbymchenry/codegraph/1.6.2) has six exact `1.6.2` optional platform packages and no `engines` field.
The source app manifest's `>=20.0.0 <25.0.0` is separate from the published wrapper manifest.
The [wrapper source](https://github.com/colbymchenry/codegraph/blob/v1.6.2/scripts/npm-shim.js) starts the bundled runtime and can download a missing platform bundle into `CODEGRAPH_INSTALL_DIR` or `~/.codegraph`.
That fallback needs system `tar`.
Version probes set `CODEGRAPH_NO_DOWNLOAD=1`.
Normal invocations keep upstream fallback behavior.
The [CBM registry metadata](https://registry.npmjs.org/codebase-memory-mcp/0.12.1) declares a `postinstall` downloader.
Its existence doesn't add an npm channel to the decided CBM entry.

No upstream requirement for a particular npm-family client or a per-client minimum was found.
Use an empty `MinimumClientVersions` map and no `RequiredClient` for CodeGraph.
Client runtime compatibility and global-bin readiness still need A's checks.
Don't copy npm 11 or another manager minimum from this repository into either entry.

The [CodeGraph bundle script](https://github.com/colbymchenry/codegraph/blob/v1.6.2/scripts/build-bundle.sh) uses official Node archives.
The [Node 24.16.0 platform table](https://github.com/nodejs/node/blob/v24.16.0/BUILDING.md#platform-list) gives Windows 10, macOS 13.5 and Linux kernel 4.18 with glibc 2.28 for these x64 and ARM64 binaries.
Linux also needs libstdc++ 6.0.25 (`GLIBCXX_3.4.25`).
The glibc CodeGraph archive isn't a musl archive.
The wrapper support floor doesn't remove the bundled runtime's OS requirements.

The [CBM README](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/README.md#installation) describes a self-contained executable.
Its [portable build workflow](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/.github/workflows/_build.yml#L234-L272) builds and checks fully static Linux binaries on x64 and ARM64.
Use those `-portable` assets for Linux, with `ToolLinuxLibc.Static`.
Upstream doesn't specify a numeric Windows, macOS or Linux-kernel floor for this release in the checked installation documentation.
Keep these values unspecified and require target execution evidence before claiming a new floor.
No additional CPU-feature floor was established.

The following exact pins come from [CodeGraph SHA256SUMS](https://github.com/colbymchenry/codegraph/releases/download/v1.6.2/SHA256SUMS).
Every hash also matches the GitHub API asset digest:

| Asset                           | Bytes    | SHA-256                                                            |
| ------------------------------- | -------- | ------------------------------------------------------------------ |
| `codegraph-win32-x64.zip`       | 54260850 | `7455e817ee8032666867a1badeb799b8a884c5f4fea9b444e088a02069b6deea` |
| `codegraph-win32-arm64.zip`     | 50258514 | `308f7bf92e7886f4d5f767a4755d703e92d5cae78ac042b8907f1e99db0fc071` |
| `codegraph-darwin-x64.tar.gz`   | 59393903 | `53d1a4d1a9af31d6cec11b346df2ae792087081e13f623f583e27783c1e7f9bc` |
| `codegraph-darwin-arm64.tar.gz` | 58280485 | `d74d1bfb4060db63ec3c2b72c4e17f76c31978af3bad6ace4370d1501ac0662e` |
| `codegraph-linux-x64.tar.gz`    | 63433974 | `ef0af416092128fb1ccc723786000b7edf4a6971fe604acd08bf966e4f37b828` |
| `codegraph-linux-arm64.tar.gz`  | 63017097 | `c8c6be292be21d00dea26bad8b28d434731cf612fb8cc475dc10f5b3038b5b64` |

CodeGraph URLs use `https://github.com/colbymchenry/codegraph/releases/download/v1.6.2/<asset>`.
`SHA256SUMS` itself hashes to `4e6fc8b6b0478c76c3a10ff48323c715b030b8faecb3826210c50a5863549cb5`.
CodeGraph archives have one `codegraph-<target>/` folder, so `StripComponents=1`.
Keep the whole bundle: `node[.exe]`, `lib/` and `bin/`.
The native Windows launcher is `bin/codegraph.cmd` and the Unix launcher is `bin/codegraph`.
Open Forge setup and probes need the real executable and prefix below.
Do not flatten the bundle or expose Node in Open Forge bin.

### Bundled invocation

The tagged bundle script copies the runtime and writes the launchers at [lines 99 through 150](https://github.com/colbymchenry/codegraph/blob/v1.6.2/scripts/build-bundle.sh#L99-L150).
These are source-defined paths relative to the extracted bundle after `StripComponents=1`:

| GitHub asset                    | Node executable | JS entry                    |
| ------------------------------- | --------------- | --------------------------- |
| `codegraph-win32-x64.zip`       | `node.exe`      | `lib/dist/bin/codegraph.js` |
| `codegraph-win32-arm64.zip`     | `node.exe`      | `lib/dist/bin/codegraph.js` |
| `codegraph-darwin-x64.tar.gz`   | `node`          | `lib/dist/bin/codegraph.js` |
| `codegraph-darwin-arm64.tar.gz` | `node`          | `lib/dist/bin/codegraph.js` |
| `codegraph-linux-x64.tar.gz`    | `node`          | `lib/dist/bin/codegraph.js` |
| `codegraph-linux-arm64.tar.gz`  | `node`          | `lib/dist/bin/codegraph.js` |

Every row uses the exact ordered prefix `["--liftoff-only", "--disable-warning=ExperimentalWarning", "<bundle>/lib/dist/bin/codegraph.js"]`.
Resolve `<bundle>` to the verified absolute extraction root, then append the operation's argument vector.
For example, the Windows version probe runs absolute `<bundle>/node.exe` with that prefix followed by `["--version"]`.
The two flags precede the JS entry.
`--liftoff-only` avoids the documented V8 WASM compiler failure and the second flag suppresses experimental warnings.
There is no additional `--experimental-sqlite` argument in these launchers.
The [npm shim's Windows path](https://github.com/colbymchenry/codegraph/blob/v1.6.2/scripts/npm-shim.js#L63-L107) uses the same executable, entry and prefix because Node refuses to spawn `.cmd` directly.
This gives A a declared invocation without a shell or filename search.

For long-running MCP use, keep the inherited nonempty `CODEGRAPH_HOST_PPID`.
The Unix release launcher otherwise exports its parent PID and the [npm shim](https://github.com/colbymchenry/codegraph/blob/v1.6.2/scripts/npm-shim.js#L44-L55) supplies its parent PID on both platforms.
Direct invocation must carry the corresponding native host PID through A's process host when replacing those launch paths.
This environment value is separate from the runtime argument prefix.
These facts come from tagged packaging source. No archive was extracted or executed to verify its members in this run.

Council 1 and the old draft record attested workflow source `0f0af7bf22a5083a47e34fcb1e1bf67355174ef8` and tag commit `6560052a6f856855d3f71eee838fd66ccfa4285d`.
The release page still points to tag commit `6560052`.
This run doesn't assert source equivalence or a fresh attestation verification.
The mismatch is a note. C6 permits the checked SHA-256 channel.

The following exact pins come from [CBM checksums.txt](https://github.com/DeusData/codebase-memory-mcp/releases/download/v0.12.1/checksums.txt).
Every hash also matches the GitHub API asset digest:

| Asset                                             | Bytes    | SHA-256                                                            |
| ------------------------------------------------- | -------- | ------------------------------------------------------------------ |
| `codebase-memory-mcp-windows-amd64.zip`           | 48508123 | `4e4e0f16b45233af5c757a1fcbe3c3b16e9056375b1fbe2ce020b6de75dedcc0` |
| `codebase-memory-mcp-windows-arm64.zip`           | 47934918 | `934ab6bd67a76c6ebd9e110d87b7090ffd629919bedaeddabe5a19175038b063` |
| `codebase-memory-mcp-darwin-amd64.tar.gz`         | 50555159 | `ca6e64397a8ae80cec898a8205a6aa16750ac798acc05a0c3ed983837944a581` |
| `codebase-memory-mcp-darwin-arm64.tar.gz`         | 49875423 | `9bfcbe38414b47969a3e1d1c1999f5357409387c86fa3d24221b5458267660d2` |
| `codebase-memory-mcp-linux-amd64-portable.tar.gz` | 48920994 | `8c3d0341064301e78ecc84cf7348d4e2c6562a0f57448243ea560aaa34de36c1` |
| `codebase-memory-mcp-linux-arm64-portable.tar.gz` | 48466763 | `ed0f1f548cd79b121c401c2e9f7841c189a001af26d3768368088c24f6a53533` |

CBM URLs use `https://github.com/DeusData/codebase-memory-mcp/releases/download/v0.12.1/<asset>`.
`checksums.txt` itself hashes to `103ddf75ccbe4e4de9faa57f074ee5f16b88edab6fd2f7dfd34e50d147dddffe`.
The native binary is `codebase-memory-mcp.exe` on Windows and `codebase-memory-mcp` elsewhere.
The tagged [packaging script](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/scripts/package-release.sh#L242-L265) puts that binary, `LICENSE`, `THIRD_PARTY_NOTICES.md` and `install.ps1` or `install.sh` at the archive root. Use `StripComponents=0`.
Keep the licence and notice files. Open Forge invokes the verified binary, not the bundled shell installer.
No archive bytes were downloaded in this run.
The per-asset `.bundle` signatures are available, but this recipe uses the checked SHA-256 pins.

## Native recipes

These vectors run through the resolved executable and prefix arguments, with the project as working directory.
Angle-bracket values below are substitutions, not literal arguments.
The [CodeGraph CLI](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/bin/codegraph.ts#L2640-L2835) and [CBM CLI](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L11239-L11420) define the flags:

| Tool, operation       | Argument vector                                                                           | Scope and result                                                                 |
| --------------------- | ----------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| CodeGraph probe       | `["--version"]`                                                                           | Bounded probe, `CODEGRAPH_NO_DOWNLOAD=1`                                         |
| CodeGraph setup       | `["install", "--yes", "--location", "local", "--target", "claude,codex"]`                 | Project Claude and Codex setup. Keeps hook and permissions                       |
| CodeGraph uninstall   | `["uninstall", "--yes", "--location", "local", "--target", "claude,codex", "--keep-cli"]` | Removes native project integrations. Keeps CLI and index                         |
| CodeGraph preview     | No native dry-run flag                                                                    | Describe the same vectors and files. `--print-config` is only a config snippet   |
| CBM probe             | `["--version"]`                                                                           | Stateless version mode                                                           |
| CBM setup             | `["install", "--yes", "--skip-binary", "--dir", "<bin>"]`                                 | Account client discovery and configuration after verified placement              |
| CBM native preview    | `["install", "--yes", "--skip-binary", "--dir", "<bin>", "--dry-run"]`                    | Native description without configuration changes                                 |
| CBM file inventory    | `["install", "--yes", "--skip-binary", "--dir", "<bin>", "--plan"]`                       | `agent.install.plan.v1` JSON, no writes or network                               |
| CBM uninstall         | `["uninstall", "--yes", "--dir", "<bin>"]`                                                | Account-wide binary and all agent entries. Live Open Forge confirmation required |
| CBM uninstall preview | Append `--dry-run` to uninstall                                                           | Describes account removal. Never add `--delete-indexes`                          |

`<bin>` is the resolved Open Forge bin for release placement.
CBM's registrations must point to a verified binary at that path before setup.
The [CBM installer](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L11340-L11400) honors `--skip-binary` for placement and PATH on Windows.
Its macOS branch can still sign the existing target. Declare that platform effect before claiming unchanged binary bytes there.
Default discovery stays enabled. Do not pass `--clients` to narrow the production recipe.

The native CodeGraph file set is defined by its [Claude target](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/installer/targets/claude.ts) and [Codex target](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/installer/targets/codex.ts):

| File                                       | Native effect                                                                                    |
| ------------------------------------------ | ------------------------------------------------------------------------------------------------ |
| `.mcp.json`                                | `mcpServers.codegraph`, command `codegraph`, arguments `serve --mcp`, Claude `alwaysLoad=true`   |
| `.claude/settings.json`                    | `mcp__codegraph__*` permissions, `UserPromptSubmit` hook and cleanup of old CodeGraph sync hooks |
| `.claude/CLAUDE.md`                        | Native `CODEGRAPH_START` and `CODEGRAPH_END` instruction block                                   |
| `.codex/config.toml`                       | Native `[mcp_servers.codegraph]` table                                                           |
| `AGENTS.md`                                | Native CodeGraph instruction block. Keeps other content                                          |
| `.claude.json`                             | Migrates or removes a legacy project MCP entry. Can delete an otherwise empty file               |
| `<home>/.codegraph/telemetry.json`         | Native telemetry state, identity and consent fields                                              |
| `<home>/.codegraph/telemetry-queue*.jsonl` | Disabling telemetry removes pending and sending queues                                           |

The native [JSON writer](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/installer/targets/shared.ts#L80-L126) can also write `<file>.backup` before replacing malformed JSON, plus transient `<file>.tmp.<pid>` files for atomic replacement. Include existing `.mcp.json.backup`, `.claude/settings.json.backup` and `.claude.json.backup` destinations in preflight when those native writes apply.
Windows prompt hooks use `codegraph.cmd prompt-hook`.
Setup doesn't index unless `--init` is passed, which this recipe omits.
Normal MCP execution keeps the daemon and watcher.
Codex's native local configuration requires project trust. A written file alone doesn't show host activation.

CBM has 26 built-in client branches and 19 [registry profiles](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/agent_clients.c#L30-L79) in the checked source.
They can write shared account files, not only Claude and Codex files.

### Plan receipt

The [inventory builder](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L10980-L11098) calls the real client dispatch with recording enabled and writes disabled.
The [CLI plan branch](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L11298-L11347) prints one pretty-printed JSON object followed by a newline, then returns exit 0 before the install banner, placement, activation or network work.
It builds the target path from `--dir` without requiring a binary there.
An executable CBM is still needed to run the command itself.
`--plan` takes this branch before `--clients` selection is applied.
Keep default discovery and pass neither `--clients` nor `--skip-config`.

The complete source-defined field shape below uses empty arrays. This is not captured command output.

Illustrative:

```json
{
  "type": "agent.install.plan.v1",
  "agents_detected": [],
  "config_files_planned": [],
  "instruction_files_planned": [],
  "skill_files_planned": [],
  "agent_files_planned": [],
  "prompt_files_planned": [],
  "hooks_planned": [],
  "writes_started": false,
  "network_after_install": false,
  "next_safe_command": "codebase-memory-mcp install -y"
}
```

All six non-hook arrays contain strings.
`agents_detected` contains built-in IDs such as `claude-code` and registry `stable_id` values such as `gitlab-duo`.
The five file arrays contain resolved path strings.
Each `hooks_planned` item is exactly an object with string members `agent` and `path`.
Its `agent` is a display label such as `Claude Code`, not necessarily an `agents_detected` ID.
Skills, agent profiles and prompts also appear in `instruction_files_planned`.
One file can appear in several arrays or hooks, so backup planning needs their deduplicated union.
The recorder appends entries without deduplicating them.

Proposed: parser ID `cbm-install-plan-v1` requires this `type`, typed arrays, `writes_started=false`, `network_after_install=false`, exit 0 and bounded complete stdout.
Collect every file array plus every hook `path` before native setup.
Keep stderr separate so a native planning error isn't mistaken for a complete inventory.
`next_safe_command` is upstream advice, not the approved invocation. It omits our `--skip-binary` and `--dir` arguments.
The JSON has no binary-placement inventory or complete deletion inventory.
Plan mode skips the legacy and OpenHands profile edits described under Inventory supplements.

When CBM is absent, Inspect and dry-run describe the declared potential paths and account effects below without installing CBM just to obtain a plan.
Label those paths as potential and the actual discovered client set as unavailable.
After authorized verified placement, obtain a fresh native plan with the same home, environment, working directory and bin as setup.
Resolve the declared supplements and take backups before invoking setup.
The coordinator needs to freeze that ordering in the shared contract.

### Client outputs

For backup planning, `<home>` means CBM's resolved home, `<claude>` its Claude directory and `<codex>` its Codex directory.
The exact Claude and Codex set is:

| Client        | Files and directories                                                                                                                                                                                                          |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Claude        | `<claude-user-root>/.claude.json`, `<claude>/settings.json`, `skills/codebase-memory/SKILL.md`, tiered `agents/*.md`, `hooks/cbm-code-discovery-gate.cmd`, `hooks/cbm-session-reminder.cmd`, `hooks/cbm-subagent-reminder.cmd` |
| Codex         | `<codex>/config.toml`, `AGENTS.md`, existing `hooks.json` when present, `skills/codebase-memory/SKILL.md` and tiered `agents/*.toml`                                                                                           |
| Legacy Claude | `<claude>/.mcp.json`, empty old Skill directories and extensionless versions of the three hook scripts                                                                                                                         |

Tiered profile basenames are `codebase-memory-scout`, `codebase-memory` and `codebase-memory-auditor`.
Keep each client's suffix, such as `.md`, `.toml`, `.json` or `.agent.md`.
The [profile renderer](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/agent_profiles.c#L149-L164) defines those names.
Claude keeps its native search, read-coverage, SessionStart and SubagentStart hooks.
Codex keeps the native activation pointer and lifecycle hooks, using existing `hooks.json` or the TOML representation.

The remaining built-in account outputs come from the same CLI's `install_cli_agent_configs`, `install_editor_agent_configs` and `install_additional_agent_configs` functions.
Paths in this table are relative to `<home>` unless a resolved root is named.
`skills/` means the native `codebase-memory/SKILL.md` beneath it, and `agents/` means all three tiered profiles:

| Client        | Native output set                                                                                                                                                                         |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Gemini        | `.gemini/settings.json`, `.gemini/GEMINI.md`, tiered `.gemini/agents/*.md` and hooks in settings. Windows keeps BeforeTool and SessionStart, with AfterTool coverage withheld by upstream |
| OpenCode      | Resolved `opencode.json` or `opencode.jsonc`, `.config/opencode/AGENTS.md`, `skills/`, `agents/`, `plugins/cbm-augment.ts`                                                                |
| Antigravity   | `.gemini/config/mcp_config.json`, `.gemini/GEMINI.md`, cleanup in existing `.gemini/antigravity-cli/settings.json`                                                                        |
| Aider         | `.aider.conf.yml`, `CONVENTIONS.md`                                                                                                                                                       |
| Zed           | Resolved `Zed/settings.json`, Windows `AppData/Roaming/Zed/AGENTS.md` or Unix `.config/zed/AGENTS.md` and `.agents/skills/`                                                               |
| Kilo Code     | `.config/kilo/kilo.jsonc`, `rules/codebase-memory-mcp.md`, `agents/`                                                                                                                      |
| VS Code       | Resolved `Code/User/mcp.json`, existing `Code/User/profiles/*/mcp.json`, plus Copilot durable context                                                                                     |
| Cursor        | `.cursor/mcp.json`, `.cursor/skills/`, `.cursor/agents/`                                                                                                                                  |
| Windsurf      | `.codeium/windsurf/mcp_config.json`, `memories/global_rules.md`                                                                                                                           |
| OpenClaw      | Resolved config, resolved workspace `AGENTS.md` and `TOOLS.md`, compaction settings                                                                                                       |
| Kiro          | Resolved Kiro home `settings/mcp.json`, `steering/codebase-memory.md`, `skills/`, `agents/*.json`                                                                                         |
| Junie         | `.junie/mcp/mcp.json`, `.junie/skills/`, `.junie/agents/`                                                                                                                                 |
| Hermes        | Resolved Hermes home `config.yaml`, `skills/`, native context hook                                                                                                                        |
| OpenHands     | `.openhands/mcp.json`, `settings.json`, existing `agent-profiles/*.json` references and `.agents/skills/`                                                                                 |
| Augment       | `.augment/settings.json`, `rules/codebase-memory.md`, `agents/`, `hooks/` native session and coverage scripts                                                                             |
| Cline         | Resolved CLI root `mcp.json`, `rules/codebase-memory-mcp.md`, `skills/`, cleanup of legacy context hooks and resolved IDE data `settings/cline_mcp_settings.json`                         |
| Warp          | `.agents/skills/`                                                                                                                                                                         |
| Qwen          | Resolved Qwen home `settings.json`, `QWEN.md`, `skills/`, `agents/`, lifecycle hooks                                                                                                      |
| Copilot CLI   | Resolved Copilot home `mcp-config.json`, `copilot-instructions.md`, `hooks/codebase-memory-mcp.json`, `skills/`, `agents/*.agent.md`                                                      |
| Factory Droid | `.factory/mcp.json`, `AGENTS.md`, `hooks.json`, `skills/`, tiered `droids/*.md`                                                                                                           |
| Crush         | Resolved config, `.config/crush/codebase-memory.md` and context reference. Uninstall also checks the legacy file described below                                                          |
| Goose         | Resolved config `config.yaml`, `.config/goose/.goosehints`                                                                                                                                |
| Mistral Vibe  | Resolved Vibe home `config.toml`, `AGENTS.md`, `skills/`, `agents/*.toml`, tiered `prompts/*.md`                                                                                          |
| Grok Build    | Resolved Grok home `config.toml`, `rules/codebase-memory.md`, `skills/`, `agents/`, lifecycle hooks                                                                                       |

The [registry path resolver](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/agent_clients.c#L416-L610) adds these account outputs.
Use its chosen existing filename when several alternatives exist:

| Client           | MCP path and additional outputs                                                                                   |
| ---------------- | ----------------------------------------------------------------------------------------------------------------- |
| Qoder            | `.qoder/settings.json`, `.qoder/skills/` and tiered `.qoder/agents/`. Native context hook is withheld on Windows  |
| Kimi             | Resolved Kimi home `mcp.json`, `config.toml`, `AGENTS.md`, `skills/`                                              |
| GitLab Duo       | Resolved platform config `GitLab/duo/mcp.json` on Windows, sibling `hooks.json`                                   |
| Rovo Dev         | `.rovodev/mcp.json` or existing `mcp_config.json`, `AGENTS.md`, `skills/`, tiered `subagents/`                    |
| Amp              | `.config/agents/skills/codebase-memory/mcp.json`, `.config/amp/AGENTS.md`, shared Skills                          |
| Devin            | Resolved platform `devin/config.json`, sibling `AGENTS.md`, `skills/`. Native context hook is withheld on Windows |
| Tabnine          | `.tabnine/mcp_servers.json`                                                                                       |
| Continue         | Resolved explicit or existing default YAML config                                                                 |
| Visual Studio    | `<home>/.mcp.json`                                                                                                |
| TRAE             | Resolved existing client config                                                                                   |
| Roo Code         | Resolved existing client config                                                                                   |
| Amazon Q         | Chosen `.aws/amazonq/default.json`, `agents/default.json` or `mcp.json`                                           |
| CodeBuddy        | Chosen `.codebuddy/.mcp.json`, `mcp.json` or legacy `.codebuddy.json`, `CODEBUDDY.md`, `skills/`, `agents/`       |
| IBM Bob IDE      | `.bob/mcp.json`, `.bob/rules/codebase-memory.md`, `.bob/skills/`                                                  |
| IBM Bob Shell    | `.bob/mcp_settings.json`, `.bob/rules/codebase-memory.md`                                                         |
| Pochi            | `.pochi/config.jsonc`, `README.pochi.md`, `skills/`, `agents/`                                                    |
| Pi               | `.pi/agent/AGENTS.md`, `skills/`, `extensions/cbmem.ts`. No MCP registration                                      |
| Sourcegraph Cody | Explicit config only. Native opt-in profile                                                                       |
| Oh My Pi         | Resolved `PI_CODING_AGENT_DIR` or selected `.omp/` profile `mcp.json`, `skills/`, `agents/`                       |

For the conditional client paths, resolve through native plan output before mutation instead of guessing from this table.
An exact inventory for a real account is therefore a prerequisite, not a fixed Claude/Codex list.
Native setup outputs aren't Open Forge additive receipts.

### Inventory supplements

The following declarations supplement the plan. When the binary is absent, they also describe potential outputs without claiming client detection.
Apply the client conditions to the complete native discovery set, including registry clients:

| Condition                   | Exact extra path or expansion                                                                                                                                                                                                                                                                                                                                                | Native effect and source                                                                                                                                                                                                                                                                                                                                                                                               |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Claude detected             | `<claude>/.mcp.json`                                                                                                                                                                                                                                                                                                                                                         | Remove the owned legacy MCP entry. The plan returns before this [cleanup](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L8550-L8614)                                                                                                                                                                                                                                                      |
| Claude detected on Windows  | `<claude>/hooks/cbm-code-discovery-gate`, `<claude>/hooks/cbm-session-reminder`, `<claude>/hooks/cbm-subagent-reminder`                                                                                                                                                                                                                                                      | Remove only recognized extensionless scripts while writing `.cmd` replacements. The three migration sites are [gate](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L5807-L5874), [session](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L5883-L5922) and [subagent](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L6118-L6157) |
| A native Skill root is used | `<skills>/codebase-memory-exploring`, `<skills>/codebase-memory-tracing`, `<skills>/codebase-memory-quality`, `<skills>/codebase-memory-reference`                                                                                                                                                                                                                           | The [Skill installer](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L1597-L1671) removes only empty directories. It keeps historical files and nonempty directories                                                                                                                                                                                                                       |
| Claude detected             | `<claude>/skills/codebase-memory-mcp`                                                                                                                                                                                                                                                                                                                                        | The [old monolithic path cleanup](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L1752-L1759) also removes only an empty directory. It doesn't delete an old `SKILL.md`                                                                                                                                                                                                                    |
| Antigravity detected        | `<home>/.gemini/antigravity-cli/settings.json` when present                                                                                                                                                                                                                                                                                                                  | Remove legacy Gemini session hooks. This [branch](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L9845-L9871) isn't recorded by the plan                                                                                                                                                                                                                                                   |
| Kilo detected               | Windows `<home>/AppData/Roaming/Code/User/globalStorage/kilocode.kilo-code/settings/mcp_settings.json`, macOS `<home>/Library/Application Support/Code/User/globalStorage/kilocode.kilo-code/settings/mcp_settings.json`, Linux `<home>/.config/Code/User/globalStorage/kilocode.kilo-code/settings/mcp_settings.json`, plus `<home>/.kilocode/rules/codebase-memory-mcp.md` | Clean only owned fragments in existing legacy files. The [branch](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L10029-L10065) uses these home-based paths even when the current platform config root differs                                                                                                                                                                             |
| OpenHands detected          | Existing regular files immediately under `<home>/.openhands/agent-profiles/` whose names end in `.json`                                                                                                                                                                                                                                                                      | Add `codebase-memory-mcp` to `mcp_server_refs`. The [profile updater](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L9962-L9995) skips a missing directory and the [install branch](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L10256-L10284) doesn't record its members                                                                                  |
| VS Code detected            | `<code-user>/profiles/<existing-directory>/mcp.json` for each immediate existing profile directory                                                                                                                                                                                                                                                                           | The [profile installer](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L9898-L9925) already includes these in a live plan. A description without CBM must declare the expansion, including a missing `mcp.json` inside an existing profile directory                                                                                                                                       |

`<skills>` means each native Skill directory in the client tables, including shared `.agents/skills` or registry-specific roots.
These four empty-directory checks apply wherever `cbm_install_skills` runs, not only Claude.
Treat directory removals as declared directory effects instead of backing up or deleting arbitrary directory contents.
Existing nonempty old Skill trees stay unchanged.
Normal tiered profile files are already planned. A binary-absent description must retain all three exact basenames and each client's suffix from Client outputs.
Codex's chosen `hooks.json` or `config.toml` hook target is also already planned. Declare both possibilities without inventing `hooks.json` when it doesn't exist.

Cline v0.12.1 withholds new context hooks. Native setup and uninstall can remove recognized existing `TaskStart`, `TaskResume`, `UserPromptSubmit` and `PreCompact` adapters from `<home>/.cline/hooks/`. Windows uses `.ps1` files. Other platforms use extensionless names. The [native hook rules](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L5613-L5630) define the filenames, and the plan omits this cleanup. The catalogue declares these existing files for backup.

Install-plan output isn't an uninstall plan.
For example, native Crush uninstall also checks existing `<home>/.config/crush/CRUSH.md` for [legacy context removal](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L12739-L12749).
Keep uninstall declarations separate from install supplements.
Resolve conditional roots with the native documented overrides and existing-path choices.
OpenClaw workspace paths can come from its config and need the same physical-target checks.
Do not restrict production discovery to make an inventory smaller.

CBM's account cache contains `_config.db`, `config.json`, indexes and daemon logs.
Install and uninstall also use upstream activation files and logs.
These are native effects, not Open Forge journal files.
The [runtime configuration](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/docs/CONFIGURATION.md) defaults to `auto_index=false`, `auto_watch=true` and `watcher_enabled=true`.
Keep those defaults.
Project `.codebase-memory/graph.db.zst` is opt-in through `persistence:true`, which the journey omits.

## Added configuration

Only the following Open Forge changes belong in the initial profiles:

| Tool            | Addition                                                                                 | Removal                                                                        |
| --------------- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| CodeGraph       | Native `telemetry off`, account scope                                                    | Keep stored setting, report it and print the previous setting's manual command |
| CodeGraph       | Union `codegraph.json` `/exclude` with `<mainRoot>/memory/archived/**`                   | Remove only recorded added items still present                                 |
| CodeGraph       | Root `.gitignore` marked block for `.codegraph/`, only when effective coverage is absent | Remove the recorded unique block                                               |
| Codebase Memory | Root `.cbmignore` marked archive block containing `/<mainRoot>/memory/archived/`         | Remove the recorded unique block                                               |

The CodeGraph native setting uses `ApplyArguments=["telemetry","off"]`.
Its previous value maps to manual `["telemetry","on"]` or `["telemetry","off"]` instructions.
Stored reads and inherited overrides have separate rules below.
`telemetry off` clears identity and unsent queues, and a later `on` can't recover that history.
C7 keeps the account setting on project removal. It doesn't create a cross-project registry.

### Stored telemetry

The tagged [telemetry design](https://github.com/colbymchenry/codegraph/blob/v1.6.2/docs/design/telemetry.md#L112-L150) documents `~/.codegraph/telemetry.json`, its Boolean `enabled` member and default-on behavior.
The [constructor](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/telemetry/index.ts#L211-L224) resolves that path with `path.join(os.homedir(), '.codegraph', 'telemetry.json')` in normal production use.
`TelemetryOptions.dir` is a constructor injection, not a CLI home override.
`CODEGRAPH_INSTALL_DIR` moves the wrapper's download cache, not this telemetry file.

The bundled runtime's [Node 24.16.0 home rules](https://github.com/nodejs/node/blob/v24.16.0/doc/api/os.md#L206-L220) use `USERPROFILE` on Windows when defined, otherwise the current user's profile directory.
On POSIX they use `HOME` when defined, otherwise the effective UID's home.
Windows `HOME`, cwd, the project directory and `.NET` known folders aren't interchangeable with that rule.
Resolve the same native home through the injected host before a pre-install read.
If that host can't determine the native fallback, report it as unavailable instead of guessing a path.

The [tagged reader](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/telemetry/index.ts#L412-L426) accepts `enabled` only as a Boolean.
It also requires `machine_id` to be a string, or null when `enabled=false`.
It doesn't validate UUID syntax, `consent_source`, `updated_at` or `first_run_notice_shown` for acceptance.
With both telemetry environment overrides removed, [status resolution](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/telemetry/index.ts#L233-L255) gives these source-defined results:

| Stored file                                                  | Native enabled value | Native reason |
| ------------------------------------------------------------ | -------------------- | ------------- |
| File absent                                                  | `true`               | `default`     |
| Valid object with `enabled` absent                           | `true`               | `default`     |
| Malformed JSON, JSON null, wrong field types or a read error | `true`               | `default`     |
| `enabled=true` and string `machine_id`                       | `true`               | `config`      |
| `enabled=false` and string or null `machine_id`              | `false`              | `config`      |
| Boolean `enabled` with missing or rejected `machine_id`      | `true`               | `default`     |

For example, `{"enabled":false,"machine_id":null}` is stored off.
`{"enabled":false}` is rejected by the native reader and resolves to default on.
The direct probe must not claim native parity by reading only `/enabled` on the second object.

Proposed: add a declared read-only stored probe for this native setting before an invocation exists.
It reads the resolved file through the normal JSON reader without starting CodeGraph, creating directories, writing consent or touching queues.
An absent file or an object without `enabled` yields the known upstream default `on`.
For an accepted stored object, return `on` or `off` from its Boolean member using the tagged acceptance condition above.
Malformed JSON, non-object roots and non-absence I/O errors block this Open Forge change.
If `enabled` is present, a wrong type or a rejected `machine_id` shape also blocks it.
Other fields don't affect this setting read.
This last rule is an Open Forge preflight proposal. Native CodeGraph silently falls back to on for those errors.
Keep that difference declared, including the rejected `machine_id` case.
Read only enough identity shape to validate the stored setting. Never return, persist or print its identity value.
The proposal needs coordinator acceptance before implementation.

### Status parser

When CodeGraph exists, use `ReadArguments=["--no-color","telemetry","status"]` and `ReadEnvironment={"DO_NOT_TRACK":null,"CODEGRAPH_TELEMETRY":null,"CODEGRAPH_NO_DOWNLOAD":"1"}`.
The global `--no-color` option is declared in the [CLI](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/bin/codegraph.ts#L228-L233).
Null values remove variables from that child only.
The [status action](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/bin/codegraph.ts#L2792-L2830) prints a leading blank line and exactly `Telemetry: enabled (<reason>)` or `Telemetry: disabled (<reason>)`.
The reasons are `your saved choice`, `default`, `DO_NOT_TRACK environment variable` and `CODEGRAPH_TELEMETRY environment variable`.
It then prints a `Machine ID:` line, a `Config:` line with five spaces after the colon and a blank-separated documentation line.
That documentation line uses `https://github.com/colbymchenry/codegraph/blob/main/TELEMETRY.md`, even in this tagged release.
Colors wrap the setting and reason when color is enabled. No JSON status mode is declared.

Proposed: `ValueParserId="codegraph-telemetry-status-v1"` accepts one complete setting line after normal line-ending and ANSI handling.
Map `enabled` to native value `on` and `disabled` to native value `off`, with `DesiredValue="off"`.
After removing overrides, accept `your saved choice` or `default` as the reason.
An override reason, missing or duplicate setting line, unknown value, failed exit or truncated output makes the stored read unavailable and blocks apply.
Ignore the other lines. Don't parse or save their Machine ID, config path or documentation text as a setting.
This is a parser contract proposal, not a parser implementation or captured runtime output.

The source makes `telemetry status` read-only for this use.
The [preAction hook](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/bin/codegraph.ts#L235-L250) excludes telemetry commands from usage recording and flush.
`getStatus()` can call `clearPending()`, but that [method](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/telemetry/index.ts#L402-L405) clears only in-memory counts and events.
It doesn't remove disk queues. Disk removal belongs to `setEnabled(false)`.
This corrects the earlier report's ambiguous status-queue warning.

### Inherited overrides

The native precedence is `DO_NOT_TRACK`, then `CODEGRAPH_TELEMETRY`, then accepted stored config, then default on.
A nonempty `DO_NOT_TRACK` other than `0` or case-insensitive `false` forces effective off.
Otherwise a nonempty `CODEGRAPH_TELEMETRY` forces off for `0` or case-insensitive `false` and on for any other value.
There is no whitespace trim. Empty values don't override stored state.
Report the deciding variable and effective value separately from the stored previous value.
Keep the caller's environment for normal invocation and native apply.
For example, inherited `CODEGRAPH_TELEMETRY=1` can keep effective telemetry on after the stored value is changed to off.
The stored-state probe removes overrides only for its own read. It doesn't make that inherited override disappear.

The [CodeGraph directory code](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/directory.ts#L875-L951) creates an internal `.codegraph/.gitignore` with `*` and `!.gitignore` when initializing its index directory.
Native setup alone doesn't create that directory.
Check effective data coverage after native output using Git's ignore rules.
Don't initialize an index just to obtain ignore coverage.
Existing root rules or an existing effective internal ignore can make the Open Forge block unnecessary.

The [CBM ignore rules](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/docs/cbmignore.md) read only root `.cbmignore` and apply on the next index.
Use comment markers, for example proposed `# Open Forge codebase-memory archives start` and `# Open Forge codebase-memory archives end`.
These mark an ignore block, not tool guidance.
Don't add recovery-state exclusions, MCP entries, Codex sections, Skills or managed guidance lines.

## Model mapping

The frozen model folder is `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/`.
The mapping below reuses those records and introduces no substitute model:

| Model field                 | Recipe fact                                                                                                                  |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `ToolIdentity`              | IDs, names, `ThirdParty`, official repository URI and `MIT`                                                                  |
| `ToolNpmChannel`            | CodeGraph package, no required client, empty evidenced minimum map, Node wrapper >=20                                        |
| `ToolGitHubReleaseChannel`  | Official repository, the six exact asset records above                                                                       |
| `ToolReleaseAsset`          | Exact version, target, URL, checked SHA-256, archive kind, launcher path, strip count, byte cap and proposed real invocation |
| `ToolTarget`                | OS, x64/ARM64, glibc 2.28 for CodeGraph Linux and Static for CBM portable Linux                                              |
| `ToolDetectionSpec`         | Direct command name, `--version`, `StableSemVer`, proposed 15-second timeout and 16 KiB cap                                  |
| `ToolVersionProfile`        | Range `(1,6)` or `(0,12)`, minimum patch, reviewed baseline and first shipping Open Forge build                              |
| `ToolSetupSpec`             | Separate setup/uninstall vectors, CBM dry-run vector, declared native files, account setting/PATH/binary effects             |
| `ToolNativeConfigChange`    | Proposed file probe before CLI availability, native status parser, off command and manual previous-setting arguments         |
| `ToolJsonValueChange`       | `Union`, `/exclude`, array containing the resolved archive pattern                                                           |
| `ToolMarkedBlockChange`     | Conditional Git ignore block and CBM archive block                                                                           |
| `ToolLockRecord`            | Actual exact version/channel, optional profile digest and only actual additions                                              |
| `ToolWorkspaceMachineState` | Previous/applied account telemetry values and manual commands, keyed by physical workspace SHA-256                           |

`TestedVersions` must describe completed execution evidence.
This source review supports the baseline selection but doesn't claim either baseline passed a native journey.
Rune stays outside the V1 catalogue.

The following historical `CONTRACT_REQUEST` proposals came from the first research run and supplementary run `r_fec5e0e39de0`. Shared contract `09bcb9e` now defines the runtime ABI. The implemented entries use that committed ABI, including `ToolBundledRuntimeSpec` instead of the proposed invocation record. The first shipping version is provisional and the documentation test source-list edit stays with the integrator:

1. `contract(tool): define recipe substitutions`. Define one shared token grammar and resolver for `ToolSetupSpec.Arguments`, `DryRunArguments`, `ToolDeclaredEffect.PathTemplate`, `ToolJsonValueChange.Value`, `ToolMarkedBlockChange.ContentTemplate` and the new probe and invocation declarations below. Required values are resolved Open Forge bin, native home, client profile roots, workspace, verified asset root and slash-form mainRoot. Include null environment values as removals and fail unknown tokens. `ToolSetupRequest` currently has no home/bin input, and `ToolConfigRequest` has mainRoot but no rule for substitution inside JSON string values. Provide one resolved setup context instead of reading ambient homes in D. Also define how CBM reuse outside Open Forge bin satisfies `--dir` without creating a second install or registering a missing bin target. Consumers: A launchers, B changes/backups, C Inspect/Status, D writers and E entries.
2. `contract(tool): resolve native output inventories`. Add an optional typed inventory declaration to `ToolSetupSpec` with arguments, parser ID `cbm-install-plan-v1`, timeout and output cap. The exact argument vector is the CBM file-inventory row above. Add typed resolved file and directory effects to the planned setup result before backups. The parser consumes the Plan receipt shape and merges every file array and hook path. Declare the Inventory supplements with their client, platform, existing-file, empty-directory and immediate-profile expansion conditions. Include the declaration in the fixed profile digest projection. Account absolute paths stay machine-local. When no binary exists, allow a description of potential declared outputs with discovery unavailable. After verified placement, rerun the real plan and complete supplement resolution and backups before setup. Current `Effects.PathTemplate` doesn't define these expansions or a plan parser, and `ToolSetupResult` has no resolved inventory. Consumers: B backups, C reports, D preflight/setup and E recipes. Keep all native client discovery and overrides.
3. `contract(tool): declare conditional ignore additions`. Give the marked-block recipe an optional typed effective-ignore condition, naming `.codegraph/`, evaluated after native output and again before applying. `ToolMarkedBlockChange` only expresses an unconditional block today. Consumers: B config planning, C observations, D sequencing and E CodeGraph entry.
4. `contract(tool): declare bundled platform requirements`. Define the existing `ToolTarget.MinimumOperatingSystemVersion` as a kernel minimum for Linux, and add an optional typed required-system-library list or an equivalent shared representation. CodeGraph's Node 24 bundle needs Linux 4.18 and libstdc++ 6.0.25 in addition to the existing glibc field. Keep the host wrapper runtime separate from the bundled runtime. Consumers: A target preflight, C Inspect, D plans and E assets. Numeric CBM OS floors stay unspecified unless further evidence establishes them.
5. `contract(tool): assign catalogue shipping build`. The maintainer has been asked for the minimum shipping Open Forge version. The answer is pending. Set `MinimumOpenForgeVersion` to that first shipping release once supplied. The current repository version is `0.9.0-beta.18`, which shipped before Tool setup. Copying it would misstate support. Consumers: version/profile checks and both entries.
6. `contract(tool): include recipe research links`. Extend `ToolSetupDocumentationTests.PlannedDocumentsHaveValidLinks` with this report's repository-relative path when the integrator transfers that test boundary. This run keeps the protected test source unchanged. Consumers: focused Markdown evidence and the final entry review.
7. `contract(tool): declare pre-install stored telemetry`. Add an optional typed stored-file probe to `ToolNativeConfigChange` that works with a null `ToolConfigRequest.Invocation`. Declare native-home resolution, `.codegraph/telemetry.json`, JSON Pointer `/enabled`, default native value `on` and parser ID `codegraph-telemetry-file-v1`. Freeze the Stored telemetry proposal's absence, missing-member, accepted-object and error rules, including its declared difference from native malformed-file fallback. Keep the existing native-command change kind. Add no generic JSON mutation recipe for this account file. Freeze `AllowListId="codegraph-telemetry"`, native status `ValueParserId="codegraph-telemetry-status-v1"`, `DesiredValue="off"`, the Status parser read vector/environment and previous `on`/`off` manual command mapping. Include the file-probe declaration in the fixed profile digest projection. Return inherited override facts separately and keep identity out of results and state. Consumers: B native settings, C read commands, D all-tool preflight and E CodeGraph profile. Update the shared record behavior and maintenance checklist's native-status-only wording when accepting this addition.
8. `contract(tool): declare release executable prefixes`. Add optional typed `ToolReleaseAsset.Invocation` data with `ExecutableRelativePath` and ordered `PrefixArguments`. Keep the existing `EntryRelativePath` as the native launcher path. Resolve the new data only inside the verified extraction root and carry the resulting absolute executable and prefix as `ToolCommandTarget`. For each CodeGraph asset, use the Bundled invocation table, with the two literal runtime flags followed by the absolute JS entry. The shared substitution contract must name its asset-root token. Keep the native host-PID forwarding rule in the process environment when bypassing upstream launchers. No `.cmd` execution, shell wrapping, extension swap or directory search supplies missing data. Consumers: A channel resolution and launchers, B native probes/apply, C detection, D native setup and E release entries.

The existing `ToolDeclaredEffectKind.PathChange` and `SharedFileDeletion` can describe CBM uninstall's gated effects.
The existing native setting and portable record shapes already support C2/C7 removal behavior.
No schema or record change is requested for restoring telemetry on Remove.

### Catalogue factory

F calls `ToolCatalogueFactory.Create(nodeRuntimes)` from the [factory source](../../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Catalogue/ToolCatalogueFactory.cs).
Its parameter is `IReadOnlyList<ToolPrivateNodeSpec>` from A's approved official runtime catalogue.
The result is the shared `ToolCatalogue`, with an ordinal frozen entry dictionary and a copied immutable runtime inventory.
The factory creates neither private Node pins nor machine paths and performs no I/O.
Pass A's complete reviewed list. An empty list is supported when testing catalogue data alone.

The entry factories and their current facts are:

| Entry           | Source                                                                                                                                                | Profile facts                                                                                                     |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| CodeGraph       | [CodeGraphCatalogueEntry](../../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Catalogue/CodeGraphCatalogueEntry.cs)           | `1.6.x` from `1.6.2`, Node native home policy, npm and GitHub channels, six reviewed pins, bundled Node `24.16.0` |
| Codebase Memory | [CodebaseMemoryCatalogueEntry](../../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Catalogue/CodebaseMemoryCatalogueEntry.cs) | `0.12.x` from `0.12.1`, HomeThenUserProfile policy, GitHub only, six reviewed pins, no Node runtime               |

Both use provisional `MinimumOpenForgeVersion=0.9.0-beta.19` as directed by the coordinator.
This doesn't bump any package version.
Both keep `TestedVersions` empty until the later native journeys supply execution evidence.
The reviewed minimum patch and direct asset pins already select the intended baseline.
The constructors keep the native vectors and empty normal environment overlays.
CodeGraph's version and optional status reads carry `CODEGRAPH_NO_DOWNLOAD=1`.
Its stored telemetry reader is preferred before installation and for later verification.
Only its three decided Open Forge additions and CBM's archive block are authored.
All MCP, Skills, agents and hooks in possible-output declarations belong to the native installers.

### Client bindings

The [CBM output declarations](../../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Catalogue/Shared/CodebaseMemory/CodebaseMemoryNativeOutputs.cs) define the keys F needs in the injected recipe context.
They don't select or install a client.
`ClientId` matches native plan IDs, including `claude-code` and `augment-auggie`.
Root resolves the same environment overrides and existing-path choices reviewed in Client outputs before B runs native effects.
Bind expected paths independently of an arbitrary returned inventory path, then check the native plan against them.

The key forms have these meanings:

| Key form                                                                                         | Binding                                                                                                  |
| ------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------- |
| `<id>-config`                                                                                    | Exact native-selected MCP config file for that client, including an arbitrary documented config override |
| `claude-code`                                                                                    | Native Claude directory from `CLAUDE_CONFIG_DIR` or the native home                                      |
| `claude-user`                                                                                    | Directory containing native `.claude.json`                                                               |
| `codex`, `kimi`, `kiro`, `hermes`, `qwen`, `copilot-cli`, `mistral-vibe`, `grok`, `devin`, `omp` | Resolved native client directory                                                                         |
| `vscode`                                                                                         | The platform's resolved Code/User directory, including its immediate profiles                            |
| `shared-skills`                                                                                  | Native shared `.agents/skills` directory                                                                 |
| `cline-data`                                                                                     | Native Cline IDE data directory                                                                          |
| `openclaw-workspace`                                                                             | Native workspace resolved from its account config and overrides                                          |
| `gitlab-duo-hooks`                                                                               | Exact resolved native GitLab hook config file                                                            |
| `codebase-memory-cache`, `codebase-memory-runtime`                                               | Resolved native cache and daemon runtime directories, reported without reading their contents            |

The exact `-config` client set and all path suffixes are in the linked declarations.
F provides file-valued config bindings as absolute paths as well as directory-valued roots.
VS Code also needs the native `copilot-cli` directory binding because its installer writes the shared Copilot context.
Install uses CBM's native plan. Uninstall has its own declarations and descriptive dry run.
CodeGraph's native uninstall stays project-scoped while reporting native account telemetry effects. Telemetry alone doesn't select CBM's account-uninstall confirmation case.
Existing-profile patterns retain an exact final filename where the native installer can create it.
Legacy Skill directory cleanup is report-only. It doesn't request backups of directory trees.
No new shared ABI or private substitute model was added in this pass.

## Windows journeys

Run later only after A through D integrate, E receives the catalogue/test paths and the required contracts are integrated.
Create one owned root under `D:/` with separate `repo/`, `home/`, `temp/`, cache and runtime directories per tool.
Create Git repos containing a small caller/callee fixture and a matching archived fixture with a distinctive symbol.
All mutable client files must start inside that root.
Use a child-process environment, not persistent shell settings.

The source checks establish these home and platform requirements:

| Component                 | Resolution and required journey control                                                                                                                                                                                                                                     |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CodeGraph home            | `os.homedir()` for telemetry and fallback cache. Set child `USERPROFILE` on Windows and `HOME` on POSIX, then verify the actual Node runtime's home before upstream execution. Windows falls back to the current user's profile, POSIX to the effective UID's home          |
| CodeGraph other paths     | Set child `TEMP`, `TMP`, `TMPDIR`, `NODE_COMPILE_CACHE` and `CODEGRAPH_INSTALL_DIR` to owned directories. Local targets stay anchored to cwd                                                                                                                                |
| Claude and Codex          | Set child `CLAUDE_CONFIG_DIR` and `CODEX_HOME` to owned roots. CodeGraph local target paths still use the project                                                                                                                                                           |
| CBM home                  | `cbm_get_home_dir()` checks `HOME`, then `USERPROFILE`, as shown by the tagged [platform code](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/foundation/platform.c#L499-L569)                                                                            |
| CBM config                | `APPDATA` and `LOCALAPPDATA` are honored by generic app-path helpers. Some clients instead build `home/AppData/...`, so keep both within the owned root                                                                                                                     |
| CBM cache                 | Set `CBM_CACHE_DIR` once per isolated account journey. It moves cache, not daemon rendezvous                                                                                                                                                                                |
| CBM rendezvous            | Set production `CBM_RUNTIME_DIR` to an existing owned directory. [Bootstrap](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/daemon/bootstrap.c#L227-L301) applies it to clients, daemon, workers and activation. It keeps native private-directory checks |
| CBM known folder          | Without that override, [Windows IPC](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/daemon/ipc.c#L5537-L5565) calls `SHGetFolderPathW(CSIDL_LOCAL_APPDATA)`. Environment-only redirection doesn't cover it                                                |
| Client discovery          | Keep native default discovery. Use an owned child environment and seed owned Claude/Codex markers for the first journey. Capture `--plan` and supplements to check every discovered client's targets. The fixture doesn't narrow the production recipe                      |
| Other client overrides    | Clear or redirect `KIRO_HOME`, `HERMES_HOME`, `QWEN_HOME`, `CLINE_DATA_DIR`, `OPENCODE_CONFIG`, `OPENCODE_CONFIG_DIR`, `COPILOT_HOME`, `CRUSH_GLOBAL_CONFIG`, `VIBE_HOME` and `GROK_HOME`. Cline CLI stays at `<home>/.cline`                                               |
| Registry-client overrides | Clear or redirect `GLAB_CONFIG_DIR`, `KIMI_CODE_HOME`, `CBM_CONTINUE_CONFIG_PATH`, `CBM_TRAE_CONFIG_PATH`, `CBM_ROO_CONFIG_PATH`, `CBM_CODY_CONFIG_PATH`, `PI_CODING_AGENT_DIR` and `OMP_PROFILE`                                                                           |
| OpenClaw overrides        | Clear or redirect `OPENCLAW_HOME`, `OPENCLAW_STATE_DIR`, `OPENCLAW_CONFIG_PATH`, `OPENCLAW_WORKSPACE_DIR` and `OPENCLAW_PROFILE`. Check workspace paths read from its config too                                                                                            |
| Git settings              | Use owned `GIT_CONFIG_GLOBAL`, disable system config for the child and keep `XDG_CONFIG_HOME` owned. CBM discovery reads Git global excludes                                                                                                                                |
| Open Forge home           | Exercise the integrated root's injected home/known-folder host and fake PATH host. Don't assume changing `USERPROFILE` moves .NET known folders                                                                                                                             |

The full environment-name inventory still needs a final source comparison when the journey harness is written.
An absolute client override or an inherited PATH-discovered client must not lead to real account files.
Run no command when its resolved targets leave the owned roots.

Complete the later journey in this order:

1. Verify owned paths, actual runtime home, cache, daemon namespace and all resolved native plan paths. Refuse external links and absolute targets outside the owned roots. Keep normal watcher defaults.
2. Run Inspect, Status and exact-version dry-run before install. Confirm no Tool records or config writes and no CodeGraph download fallback. Record exact process bounds and results.
3. Install the exact baseline through the integrated CLI with the explicit GitHub channel first. Decline or fake PATH edits. For CodeGraph, later repeat the npm path in an owned manager prefix without changing real global settings.
4. Check actual lock version/channel and additive receipts. Compare the whole owned tree before/after native setup, including instruction blocks, Skills, hooks, permissions, legacy changes and account cache. Verify unrelated sentinel bytes stay unchanged.
5. Use direct CodeGraph `init --yes`, `query <fixture-symbol>` and `explore <fixture-symbol>`. For CBM, use `cli index_repository --repo-path <repo>`, `cli --quiet list_projects --format json`, then `cli search_graph --project <returned-project> --name-pattern <fixture-symbol>`. Use returned project identity, not a guessed project key.
6. Launch the exact native-installed MCP command from its installed config. Send `initialize`, `notifications/initialized`, `tools/list` and a real `tools/call` using the returned input schema. CodeGraph calls `codegraph_explore`. CBM calls `index_repository`, `index_status` and `search_graph`. Verify the fixture result and absence of the archived fixture. Record negotiated protocol, server version, result and shutdown. A successful file write isn't MCP evidence.
7. On Windows, prove that both Claude and Codex can launch their native command, including the `.cmd` case. Keep their entries as upstream wrote them. If a host fails, report that exact incompatibility. Don't rewrite the entry to `cmd /c` or Open Forge dispatch.
8. Reapply named Install and test bare Install against the exact lock. Run real Update with the exact baseline and supported minor selector to record update/no-change behavior. Cover version transitions with synthetic entries through the production Update operation. This is sufficient for the assigned journey. Don't add an older unsupported release to manufacture a real transition.
9. Run real Windows CodeGraph Remove with `--yes` so its native project uninstall runs and shared binary/runtime deletion stays unselected. Verify native-first order and removal of only recorded project additions. Keep stored telemetry off and show its manual command.
10. Run real Windows CBM Remove with `--yes`. Verify its account-wide native uninstall doesn't execute, project archive additions disappear, account binary/client entries/indexes stay and the exact uninstall command is printed. Intentional keeping doesn't make project removal incomplete. Cover no-terminal and declined confirmation through production operation tests with the injected hosts.
11. Prove confirmed CBM account removal through the production Remove operation with an injected process/PATH host. The plan and live-terminal confirmation must name account-wide binary removal, all agent entries and persistent user PATH cleanup. Assert the exact native executable, prefix, uninstall vector and environment passed to the process host, invocation before project cleanup and absence of `--delete-indexes`. Keep owned index sentinels unchanged. Record this as synthetic operation evidence, not real native-uninstall evidence.
12. Close owned MCP/CLI processes, then stop only the daemon in the owned `CBM_RUNTIME_DIR` or owned CodeGraph project. Verify owned process exit before deleting temporary files. Never stop an ambient account daemon.

The [native uninstall path](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L12898-L12932) calls persistent Windows PATH removal without a skip flag.
The [registry helper](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L6745-L6810) opens `HKCU/Environment` independently of home.
The maintainer's continuation resolves the journey choice with the injected production-operation path above.
Real Windows CBM removal therefore uses `--yes` project-only cleanup.
The account-confirmation test doesn't launch the upstream uninstaller or write a registry key.
Keep upstream bytes unchanged and create no extra registry test keys.
No upstream no-PATH flag, older release or expanded execution permission is needed for this assigned evidence split.

## Verification

The managed Release solution build passed with zero warnings and errors.
`npm ci` completed without changing tracked dependencies. npm reported 9 moderate and 1 high advisory.
No dependency update was made.

Checks, counts and artifact receipts are recorded in the packet's Evidence section.
This report adds one routed file for the integrator's later Index pass.
The research runs changed no catalogue, schema, model, central composition, shared contract or parent Task file. This continuation adds the two catalogue entries and their tests. Protected shared types, schemas, composition and parent state stay unchanged.
No binary was downloaded or installed. No native setup, uninstall or telemetry command ran.
No payload-driven fixture or snapshot was regenerated.

This continuation rechecked tagged CodeGraph telemetry design, reader, CLI, bundle script and npm shim, Node 24.16.0 home documentation, and tagged CBM plan, legacy cleanup, profile expansion and registry-client source.
The exact source links and line ranges sit beside each fact above.
The supplementary run reported 20 registry clients. This pass rechecked the published initializer and counted 19 records. The array-bound enum token is excluded. The 26 built-in client branches are separate. The full plan shape, overlapping arrays, skipped legacy effects and empty-directory-only Skill cleanup stay as recorded above.
No native status or plan output was captured. Their documented output descriptions are source evidence only.
The earlier build and 12-test receipt are research-run evidence. The packet separately records the catalogue build and focused Unit results. No native setup or journey result is claimed.

## Not decided yet

- Final first shipping Open Forge build. The catalogue uses provisional `0.9.0-beta.19` until the maintainer answers.
- Integrator addition of this report to the focused documentation test source list. The runtime ABI requests are integrated at `09bcb9e`.

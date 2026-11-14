---
open-forge:
  description: Record owned Windows Tool journeys, grouped corrections and final source checks
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence, Windows]
---

# Windows tool journeys

Windows tool journeys are the checks that run the production Tool graph and upstream tools in owned Windows homes and Git repositories.

For example, a successful native install must produce a direct command and a working MCP request before its exact version enters `TestedVersions`.
Both real Windows journeys complete. Confirmed CBM account removal uses the production operation with injected native effects.

Status: Checked candidate. Two C account-choice captures need coordinator review after the E-J4 correction.

## Inputs

The branch is `feature/tool-setup-e-final` on core `885b17db3e56ceb6839ba23609e85a4380c75018`.
The previous branch stays at `3a6469a8e1e7119281b842640f069c6d0456ad44`.
The complete original 31-file candidate matches its prior manifest and is copied under `artifacts/task90-lane-e-grouped/prior-candidate/`.
All prior logs and checks are kept under `prior-evidence/` in that same artifact folder.
The original candidate source checkpoint is `15f9fb7692e57512b2847f1a1272a93c54e1ce3dcc9ef19849f3c8f83fc7cb8c`.

The first input set has 51 files.
Its patch SHA-256 is `d17391264d0044123887ea0a11b8d1f1a7a868180599171e13fc1a4584230707`.
Its C v1 manifest SHA-256 is `422e0d7db326a8f07f13d0a91232c2dafa51a8c82e6ea09c7020c48e47bbe5a9`.
The complete preparation record is `artifacts/task90-lane-e-final/input-manifest.json`.

The grouped dispatch supplies these checked replacements:

| Input         | SHA-256                                                            | Check                                                                                            |
| ------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------ |
| B patch       | `27b3c7a09eb374d8e61f95b852cef21ff6315d0dafada8a727371c18b2c4cabc` | `git apply --check` passes. All seven output hashes match                                        |
| C v2 manifest | `7a10709564a7c5108dd579b78e77b5f222a397cc7662e5a29d0de8fa66a37da6` | Exactly four rows differ from v1. Each target first matches v1, then receives the exact v2 bytes |

The C replacements are the synthetic Catalogue fixture, its two reviewed account-choice baselines and its receipt.
No production Root file changes in that replacement.
The B seven-file and C four-file records are in `artifacts/task90-lane-e-grouped/input-corrections.json`.
B's 66-case, build and formatter evidence is consumed without rerunning its classes.
All supplied B and C files stay protected and outside E staging.
The grouped return has no F-P1 input. C supplies that correction in the later v3 continuation.
The grouped input manifest covers 58 files, including 53 protected files and five assigned packet or wording overlaps.
The E-only manifest covers 33 paths, including the one deleted unused outline.

The later C v3 continuation adds the accepted F-P1 correction.
Its manifest SHA-256 is `56bfd6530bead94bc852d5b46f28c437923f4760733172a9cb0de1fb34ca703f`.
Exactly five changed paths are copied after checking v2, core or absent-target bytes.
Its other 31 rows aren't copied. The input set then has 60 paths.
The checked C v3 receipt is `artifacts/task90-lane-e-c-v3/receipt.json`.

The final correction preserves that complete 33-path candidate and every earlier evidence directory under `artifacts/task90-lane-e-remove-final/`.
Only D's `src/cli/**` patch is checked and applied.
The patch SHA-256 is `eeb35af0156d5b8e0615cdc1896baa41ba216ae0d02d706276c64f4f888dae2c`.
Its 12-file manifest SHA-256 is `015bab6f4103ead84dc07242ae3aafb5120904dc8a0c8c277cd3c46bf62914c0`.
All 12 C# output hashes match. D's records aren't copied.
All earlier 60 input paths and 33 E paths match immediately after applying the patch.
The final input set has 72 paths, including 67 protected paths and five assigned overlaps.
The E-only set has 35 paths after adding the CBM tested-version entry and its matching test change.

The maintainer approved I1 and minimum Open Forge `0.9.0-beta.19`.
Builds use the normal `OpenForgeCliVersion` property.
The actual Root executable and manual host report that version.
Release files and dependencies are unchanged.

## Isolation

The CodeGraph fixture stays at `D:/ww-temp/task90-e-final/journey-e731c00b94/codegraph`.
The first CBM retry there reaches verified placement, then native activation refuses the shared `D:/ww-temp` ancestor's DACL.
No shared ancestor ACL is changed.

The successful CBM fixture is `D:/of-task90-e-final/journey-8c5e12856e/codebase-memory`.
The new task-only `D:/of-task90-e-final` has a protected DACL granting FullControl only to the current process identity, LocalSystem and Administrators.
Children inherit those rules.
The tagged [native directory walk](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/daemon/ipc.c#L5291-L5389) checks each component below the drive root.
The existing native guard passes this new ancestry.
The fixture record is `private-fixture.json` under the grouped artifact folder.

The tracked manual host injects native home, Open Forge home, client roots and known folders into the production graph.
Installation uses the real production process and download hosts.
The base child environment is an allow-list.
HOME, USERPROFILE, APPDATA, LOCALAPPDATA, XDG paths, Git and npm configuration, TEMP and TMP all point inside the owned fixture.
`CBM_CACHE_DIR` and the existing `CBM_RUNTIME_DIR` are owned paths too.
The PATH host rejects persistent writes.
No production test flag, public option or storage setting is added.

CBM install uses `--skip-binary` after verified placement.
Tagged `cli.c:11188-11195` returns before native Windows PATH editing in that mode.
The real host rejects every CBM uninstall vector before process execution.
Even its dry run opens HKCU, as E-J4 records below.

## CodeGraph journey

The first real journey completes these steps:

| Step            | Result                                                                               | Artifact under `artifacts/task90-lane-e-final/`                   |
| --------------- | ------------------------------------------------------------------------------------ | ----------------------------------------------------------------- |
| Install         | Exit 0, `1.6.2`, `github-release`                                                    | `codegraph-install.json`                                          |
| Status          | Exit 0, locked and detected `1.6.2`, current                                         | `codegraph-status.json`                                           |
| Direct launcher | Python CreateProcess, ordinary shell and BCL ProcessStartInfo return `1.6.2`, exit 0 | `codegraph-direct-createprocess.txt`, `codegraph-bcl-direct.json` |
| Index and query | One Python file indexed. Query finds `greeting`                                      | `codegraph-init.stdout.txt`, `codegraph-direct-query.stdout.txt`  |
| MCP             | Initialize, tools/list and `codegraph_explore` return current greeting source        | `codegraph-mcp-transcript.json`                                   |
| Update          | Exit 0, verified `1.6.2` reused without download                                     | `codegraph-update.json`                                           |
| Remove          | Exit 0, native project cleanup before Open Forge cleanup                             | `codegraph-remove.json`                                           |

The [official Windows x64 archive](https://github.com/colbymchenry/codegraph/releases/download/v1.6.2/codegraph-win32-x64.zip) matches SHA-256 `7455e817ee8032666867a1badeb799b8a884c5f4fea9b444e088a02069b6deea`.
Native setup uses `install --yes --location local --target claude,codex` unchanged.
It writes project `.mcp.json`, `.claude/settings.json`, `.claude/CLAUDE.md`, `.codex/config.toml` and `AGENTS.md`.
The hook, permissions, MCP names and instruction blocks stay native.
Codex's project trust requirement stays unchanged.
No Claude or Codex app session is run.

Open Forge turns stored telemetry off, unions `.agents/memory/archived/**` with the existing exclude and adds conditional Gitignore coverage.
Read-only version probes add `CODEGRAPH_NO_DOWNLOAD=1`.
The owned account has no inherited telemetry override.
Native `init` separately writes `.codegraph/.gitignore` and the index.

Remove clears native local setup and removes the recorded archive union and marked ignore block.
Original `AGENTS.md`, `.gitignore`, `codegraph.json`, `.cbmignore` and Python source return byte-for-byte.
The CLI and index stay available.
Stored account telemetry stays off, and the result prints `telemetry on` as the manual native change.
CodeGraph `TestedVersions` contains `1.6.2`.

## CBM journey

The real command is `tool install codebase-memory@0.12.1 --channel github-release --yes --format json`.
It runs through the composed Root graph in the private fixture and returns exit 0, `completed`.
The [official Windows amd64 archive](https://github.com/DeusData/codebase-memory-mcp/releases/download/v0.12.1/codebase-memory-mcp-windows-amd64.zip) matches SHA-256 `4e4e0f16b45233af5c757a1fcbe3c3b16e9056375b1fbe2ce020b6de75dedcc0`.
Reviewed pins and tagged source facts are in the [recipe report](upstream-recipes.md).

The real results are:

| Step                 | Result                                                                                      | Artifact under `artifacts/task90-lane-e-grouped/`                 |
| -------------------- | ------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| Install              | Exit 0, exact `0.12.1`, verified GitHub archive                                             | `cbm-private-install.json`                                        |
| Status               | Exit 0, current, locked and detected `0.12.1`                                               | `cbm-status.json`                                                 |
| Direct version       | Exit 0, `codebase-memory-mcp 0.12.1`                                                        | `cbm-direct-version.stdout.txt`                                   |
| Direct index         | Exit 0, 57 nodes, 55 edges, archive directory excluded                                      | `cbm-direct-index.stdout.txt`                                     |
| Direct search        | Exit 0, greeting Function, File and Module results                                          | `cbm-direct-query.stdout.txt`                                     |
| MCP                  | Protocol `2024-11-05`, server version `0.12.1`, 17 tools, successful index and search calls | `cbm-mcp-index-transcript.json`, `cbm-mcp-search-transcript.json` |
| Update preview       | Exit 0, completed                                                                           | `cbm-update-preview.json`                                         |
| Update               | Exit 0, verified `0.12.1` reused, zero downloads                                            | `cbm-update.json`                                                 |
| First `--yes` Remove | Exit 5 before effects. E-J4, corrected in the final recheck                                 | `cbm-remove.json`                                                 |

Native installation runs `install --yes --skip-binary --dir <owned Open Forge bin>` unchanged.
Native `--plan` detects Codex from the seeded owned configuration.
There is no `--clients` restriction or replacement discovery.
Native setup rewrites `.codex/config.toml` and adds these account files:

- `.codex/AGENTS.md`
- `.codex/skills/codebase-memory/SKILL.md`
- `.codex/agents/codebase-memory-scout.toml`
- `.codex/agents/codebase-memory.toml`
- `.codex/agents/codebase-memory-auditor.toml`

The config keeps native MCP name `codebase-memory-mcp`, its direct executable, forwarded native cache/runtime variables and native SessionStart/SubagentStart hooks.
Open Forge adds only its marked archive block in project `.cbmignore` and its settings, lock and machine receipts.
No Open Forge MCP entry, usage Skill or guidance line is added.

The direct native `config get` calls confirm `auto_index=false`, `auto_watch=true` and `watcher_enabled=true`.
Both MCP index and direct index report `.agents/memory/archived` excluded.
The useful search returns the actual greeting source symbols.
Native cache/runtime lock files refuse content reads while held. File manifests mark those entries as exclusive native locks instead of inventing hashes.
Project preservation checks use the readable project files.
The first refused Remove leaves the project byte-identical, with its recorded archive block, native binary and client entries still present.
After the checked D correction, real project-only Remove succeeds as the final recheck records below.
CBM `TestedVersions` then contains `0.12.1`.

The real composed calls use the tracked manual host with the private root and `codebase-memory` before these CLI arguments:

```text
tool install codebase-memory@0.12.1 --channel github-release --yes --format json
tool status codebase-memory --format json
tool update codebase-memory --dry-run --format json
tool update codebase-memory --yes --format json
tool remove codebase-memory --yes --format json
```

The direct calls run the installed native executable with the owned environment and project working directory.
The actual direct index vector is `cli --quiet index_repository --repo-path .`.
The search vector is `cli --quiet --json search_graph --project <returned project> --name-pattern greeting --format json`.
The returned project is `D-of-task90-e-final-journey-8c5e12856e-codebase-memory-project`.
The read-only config vectors are `config get auto_index`, `config get auto_watch` and `config get watcher_enabled`.
The MCP command is that same executable with no arguments.
Its `index_repository` call uses the owned absolute repository path, then `search_graph` uses the returned project and `name_pattern=greeting`.
Each proxy exits 0 after closing input.
All three recorded CodeGraph and CBM MCP PIDs are absent in `owned-mcp-process-check.json`.
The final owned native `daemon status`, `daemon stop` and `daemon status` calls report no running daemon.
Their exits are 1, 0 and 1 in `cbm-daemon-commands.json`.
The tagged [daemon control code](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/main.c#L2458-L2492) returns failure for an inactive status and success when there is nothing to stop.

## Injected account removal

The separate injected proof uses the actual Root operation and catalogue with an inert executable in another owned D: fixture.
All native processes and transfers are injected. PATH uses the production host with an environment that rejects persistent writes.
Inert optional client bindings are supplied only in this synthetic proof, never in the real native journey.
The injected version probe isn't native compatibility evidence.

The production line prompt receives live plan acceptance and account choice `2`.
It names binary removal, all agent entries and native PATH effects.
The native callback receives the exact recipe vector `uninstall --yes --dir <owned bin>`, an empty environment overlay and the owned project working directory.
It runs before the project archive block is removed.
The injected callback removes only its inert binary and account entry and records the native PATH effect as simulated.
No real native uninstaller, registry or persistent PATH operation runs.

The operation returns exit 0.
The project ignore file returns byte-for-byte, the owned index sentinel stays intact and exactly one uninstall call is recorded.
The vector contains no all-index deletion option.
The raw transcript and `account-removal-receipt.json` are under the grouped artifact folder.
This is production operation evidence with injected native effects.
The grouped proof runs at `D:/of-task90-e-final/proof-ab15bebde5`.
Its receipt records the injected version probe separately from native compatibility.
The final D-integrated graph repeats that proof at `D:/of-task90-e-final/proof-a3f7b72b63` with exit 0.
Its refreshed receipt and prompt transcript are under `artifacts/task90-lane-e-remove-final/`.

## Revalidated findings

E-J1 is fixed for first installation by the checked B patch.
Unavailable optional paths are deferred before verified placement.
After placement, the real `install --plan` supplies detected clients, and required independent bindings are checked before setup.
Private-fixture installation succeeds without seeding optional clients.
The earlier ACL refusal is a fixture ancestry requirement, resolved only in the newly owned private directory.

E-J2 is fixed by the checked B patch.
After native initialization, real Git confirms its all-data ignore rule.
The new `codegraph-native-ignore-preview.json` reports `index-ignore: NoChange`.
The native ignore file and all native setup behavior stay unchanged.

E-J3 is fixed in E's two assigned binding files.
Inspect's parser operand and all three writer parser operands now use `ZeroOrOne`.
Both existing request binders still require exact IDs for normal Inspect and Remove execution.
Ten focused cases pass, including all eight prior semantic help cases and two missing-ID cases.
The invalid executions create no homes or operation effects and call no external hosts.

## Project-only Remove

E-J4 is fixed by the checked D correction and real recheck.
The earlier protected planner prepared account-wide native uninstall before its separate choice.
The real safety host refused its native dry run before process execution.
A diagnostic that injected only the preview then reached the unavailable optional output binding.
Those two failed results stay under `artifacts/task90-lane-e-grouped/`.

The tagged [native path helper](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L6750-L6767) uses `RegCreateKeyExW` for `HKCU/Environment`.
The [dry-run path](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L6916-L7014) opens that key before its dry-run check.
Home, cache and runtime redirection can't relocate HKCU.
The real host still rejects every native account uninstall and uninstall dry run.

D now describes unchosen account removal without native preview or output binding.
The full original request is prepared only after live account confirmation.
CodeGraph's project uninstall with incidental account telemetry stays on its native-first path.
The real recheck uses the unchanged guarded manual host and private fixture.
Its exact CLI arguments are `tool remove codebase-memory --yes --format json`.
It returns exit 0 and `status=completed`.

The only file-content changes are project `.cbmignore`, `.agents/open-forge.json` and `.agents/open-forge.lock.json`.
The marked archive block is removed and `.cbmignore` matches its original bytes.
The project declaration and lock entry are removed.
All native Codex files, both installed executable copies, native settings and the project index match their before hashes.
The inventory has the same 67 non-log files before and after.
The two existing native cache logs predate Remove and use the earlier log exclusion policy.
No native file is deleted.

The result has `nativePreview=Description`, `native=skipped`, completed project config and completed records.
All 329 possible output templates stay conditional.
It emits exactly one `manual-native-uninstall` command with the owned executable, arguments `uninstall --yes --dir <owned bin>` and an empty environment overlay.
Only native `--version` and eight Git backup queries run.
No native uninstall or uninstall dry run executes.
Raw result, command, process logs, before/after hashes, inventory and verification are under `artifacts/task90-lane-e-remove-final/cbm-remove*`.

Before the recheck, the executable, client files, project, settings and index match the earlier Update receipt.
Only native activity and rendezvous data differ after the earlier daemon control calls.
D changes Remove preparation, while the earlier native setup and direct/MCP inputs stay unchanged.
The exact successful Install, Status, direct, MCP and Update receipts are kept without replaying those steps.
After real Remove and the injected confirmed-account proof pass, CBM `0.12.1` enters `TestedVersions`.
The matching catalogue assertion uses `CodebaseMemoryDefinitions.Version`.

## Coordinator review

I-R1 is closed in source.
`CommandHelpIntegrationTests` keeps semantic exit, empty error stream, no-write and numeric width checks.
Exact headings, prose, order, example prefixes and automation assertions are removed.
The width exception uses `CliSyntaxDefinitions.ExecutableName`.
The existing Imprint call checks exact public bytes after the coordinator's final acceptance.
No expected prose constants or renderer-derived expectations replace the literals.

All 43 captures wait for one coordinator acceptance pass:

| Family          | Captures                                                                                                                                    |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Root and groups | `root`, `route`, `extension`, `library`, `tool`                                                                                             |
| Tool            | `tool-list`, `tool-inspect`, `tool-status`, `tool-install`, `tool-update`, `tool-remove`                                                    |
| Root leaves     | `install`, `update`, `remove`, `status`, `context`, `find`, `references`, `index`, `doctor`, `repair`, `cleanup`, `help`                    |
| Route           | `route-list`, `route-inspect`, `route-init`, `route-create`, `route-include`, `route-exclude`, `route-update`, `route-move`, `route-remove` |
| Extension       | `extension-list`, `extension-inspect`, `extension-create`, `extension-install`, `extension-update`, `extension-remove`                      |
| Library         | `library-list`, `library-inspect`, `library-attach`, `library-sync`, `library-detach`                                                       |

The machine-readable list is `pending-help-captures.json`.
The class is compiled. Its payload-driven captures aren't run or accepted in this continuation.

I-R2 is closed in both byte-identical CLI Skill Tool references and corresponding user docs.
The intro now restores the exact lock version, then runs CodeGraph native setup.
Latest selection is scoped to a bare ID on named Install and Update.
Settings and lock paths are fixed at `.agents/open-forge.json` and `.agents/open-forge.lock.json`, including custom context roots.
The site also gives the checked CBM install, direct version and direct index commands.

The coordinator also handles CLI Skill payload parity, affected catalogue read captures and the two known WorkspaceShape cleanup inventories.
Only the supplied two C v2 reviewed baselines are copied.
E regenerates or accepts no payload baseline.

The D correction changes two existing C account-choice prompt captures.
`ToolCompositionInteractionIntegrationTests` runs seven cases. Five pass and two fail only at Imprint.
The account approval, decline, no-host-effect and native-first assertions before the capture pass.
The received captures keep the output template unresolved, omit the pre-confirmation backup target and label the command `manual-native-uninstall`.
Their protected expected files stay byte-identical.
The received `approved.json` and `declined.json` files are copied under `artifacts/task90-lane-e-remove-final/pending-c-account-choice/` for coordinator review.
This is the only remaining focused-test failure.

## Checks

The final managed Release build passes with zero warnings and errors in `00:02:31.12`.
The actual Root executable reports `0.9.0-beta.19`.
The final manual launcher build and injected account proof return exit 0.
The solution command is:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:OpenForgeCliVersion=0.9.0-beta.19 -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The final focused commands are:

```powershell
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteOperationTests --minimum-expected-tests 36 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteSafetyTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Planning.ToolWriteRemovePlanningTests --minimum-expected-tests 9 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Writing.Application.ToolWriteDeferredRemovalTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.ToolCompositionInteractionIntegrationTests --minimum-expected-tests 7 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.ToolWorkspaceCompositionIntegrationTests --minimum-expected-tests 18 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Observation.ToolWorkspaceObservationReaderTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.ToolHelpIntegrationTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolJourneyTransitionTests --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.CodebaseMemoryCatalogueTests --minimum-expected-tests 12 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.ToolDocumentationIntegrationTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off
```

The final population has 134 cases on managed .NET 10, Windows x64.
There are 132 passes, two pending C Imprint mismatches and zero skips.
Artifacts under `artifacts/task90-lane-e-remove-final/` give these results:

| Class group              | Cases | Passed | Failed | Exit | Log                 |
| ------------------------ | ----- | ------ | ------ | ---- | ------------------- |
| D operation              | 36    | 36     | 0      | 0    | `d-operation.log`   |
| D safety                 | 15    | 15     | 0      | 0    | `d-safety.log`      |
| D planning               | 9     | 9      | 0      | 0    | `d-planning.log`    |
| D owned-file application | 11    | 11     | 0      | 0    | `d-application.log` |
| C interaction            | 7     | 5      | 2      | 2    | `c-interaction.log` |
| C workspace Root         | 18    | 18     | 0      | 0    | `c-workspace.log`   |
| C reader                 | 11    | 11     | 0      | 0    | `c-reader.log`      |
| E help                   | 10    | 10     | 0      | 0    | `e-help.log`        |
| E transition             | 2     | 2      | 0      | 0    | `e-transition.log`  |
| E CBM catalogue          | 12    | 12     | 0      | 0    | `e-catalogue.log`   |
| E documentation          | 3     | 3      | 0      | 0    | `e-docs.log`        |

The first CBM run used an incorrect minimum of 15.
All 12 actual cases passed, but that command returned policy exit 9.
The corrected minimum is 12 and the repeated class returns exit 0.
The earlier 15-case catalogue subtotal comprised 12 CBM cases and three catalogue-factory cases.
No assertion or source change reduces the test population.
The initial policy result stays in `e-catalogue-initial-minimum.log` with its exit receipt.
The final population count excludes that repeated execution.

Both mandatory checks finish on final C# source:

| Command                           | Exit | Artifacts                               |
| --------------------------------- | ---- | --------------------------------------- |
| `npm run check:dotnet:whitespace` | 0    | `whitespace.log`, `whitespace.exit.txt` |
| `npm run check:dotnet:style`      | 0    | `style.log`, `style.exit.txt`           |

The nine-file Prettier check, production Markdown local-link and Skill parity checks, LF checks and `git diff --check` pass.
The site command is `npm run build` in `src/docusaurus`.
It returns 0, prints Node's localStorage experimental warning and generates static files.
Root and site dependency inputs are unchanged from the earlier successful `npm ci` runs.
The formatting command is `npx --no-install prettier --check` with the nine paths in `markdown-files.json`.
The tracked packet changes only Progress and Evidence.
No native suite, AOT build, unrelated class, baseline acceptance or GitHub action runs.

The final `input-manifest.json` records all 72 inputs.
The `e-file-manifest.json` records the 35-path E set, including one deletion.
The `source-manifest.json` and `final-receipt.json` bind those files to commands, counts, exits and raw artifacts.
Earlier grouped and C v3 receipts and all native logs stay in their original directories and in this continuation's `prior-evidence/` copies.
Only the Windows report needs the coordinator's final routed-file Index pass.
The index is empty and nothing is committed.
Current progress is in the [lane packet](../packets/lane-e.md).

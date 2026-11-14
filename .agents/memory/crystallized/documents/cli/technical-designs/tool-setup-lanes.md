---
open-forge:
  description: Freeze round-2 interfaces and strict Worker Watch file assignments
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool setup lanes

Tool setup lanes are the file assignments for implementing native tool setup and additive configuration.
For example, lane D builds against the frozen A, B and C interfaces while their implementations run in separate worktrees.

Status: Phase 3 integration is in progress. Phase 1 is accepted and Phase 2 started the isolated Worker Watch lanes.

## Source and phases

The detailed source is the maintainer's `of-tool-setup-decisions-2.md`, dated 14 November 2026.
The [design decision record](tool-setup.md#decisions) records R2-01 through R2-16.
They override step 0.
The maintainer's Phase 1 acceptance and C1 through C7 answers in the task conversation override earlier removal rules.
The [Task 90 record](../../../../working/cli-development/tasks/task90-tool-setup.md) keeps phase progress and runs.

The three phases are:

1. Fold in decisions, verify and commit. Phase 1 is accepted.
2. Write final packets, create worker worktrees and start the swarm. Report the phase boundary.
3. Integrate lanes, run E's journeys and final docs, regenerate payload snapshots once and complete one fresh adherence review. Request CI through the overseer and stop.

## Frozen interfaces

Shared models live under `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/`.
`Services/ToolServiceContracts.cs` fixes the version, channel, config, record, process, download and PATH interfaces.
The namespace prefix is `OpenForge.Cli.Core.Framework.Tools.Models`.
These shapes contain no command implementation.
The models include `id@version` selection, `--yes` consent, npm-client facts and additive lock records.
They include no Open Forge tool guidance shape or workspace edit state.

Workers don't silently change these files or schemas.
A required change goes in a separate `contract(tool): ...` commit, with the reason, affected consumers and no implementation based on an unintegrated contract.
The mastermind checks and integrates that contract before dependent work.
Workers use agreed fakes at their typed interfaces until implementations merge.
Channel requests carry their workspace and install entry, so registry config and package identity stay with the plan.
Config requests carry the resolved recipe context and planned invocation for native writes.
The [shared planning design](tool-setup.md#shared-planning) defines the shared inputs.
A implements channels and hosts. B implements config, templates, setup planning and backups. C implements native inventory parsing, codecs and observations. D sequences writers and completes backups. After checked core integration, C handles central Root wiring while F completes the private home and native config provider.
After this contract lands, D corrects its selector so bare Install pins only the lock version under RD1.
B implements the frozen backup interface and profile digest method for D.
C returns file SHA-256 expectations, with null for absence, for its own expected-state writes.
Each worker reads the complete C# catalogue, design and style Directives independently.

## File assignments

The following paths are relative to their project unless a full repository prefix is shown.
Each assignment includes its matching Unit, Integration or EndToEnd paths.
Leaf help and output text initially belong to the same lane as that leaf.

| Lane       | Exclusive writable files                                                                                                                                                                                                                                                                                | Entry point                                                                    |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| A          | Framework `Framework/Tools/Shared/{Supply,PackageManagers,Runtimes,Launchers,Paths,Versions}/**`; root `Composition/Shared/Tools/**`                                                                                                                                                                    | Channel service and root process, network and PATH hosts                       |
| B          | Framework `Framework/Tools/Shared/{ConfigChanges,Backups,Filesystem,NativeSettings}/**`                                                                                                                                                                                                                 | Config service, additive receipt formation, profile digest and sibling backups |
| C          | Framework `Framework/Tools/Shared/{Records,Observation,Serialization}/**`, `Framework/Settings/**` and `Framework/Ownership/**`; Operations `Commands/Tool/{List,Inspect,Status}/**`, `Commands/Tool/Shared/Observation/**`, `Commands/Tool/Models/Read/**`; workspace Status and Doctor tool appenders | Record service, codecs and read operations                                     |
| D          | Operations `Commands/Tool/{Install,Update,Remove}/**`, `Commands/Tool/Shared/Writing/**`, `Commands/Tool/Models/Writing/**`                                                                                                                                                                             | Writer operations, plan consent, native setup and failure ordering             |
| E          | Framework `Framework/Tools/Catalogue/**`; tool catalogue tests and real Windows journey evidence; then final `docs/cli.md`, site CLI pages, both CLI Skill copies and Tool help wording                                                                                                                 | Real entries, direct-command and MCP journeys and user documentation           |
| F          | Root `Composition/Shared/ToolHomes/**` and matching Integration tests, plus F's packet and reports                                                                                                                                                                                                      | Native home, client paths and the private native config reader                 |
| Integrator | The mastermind keeps frozen models, schemas, shared contracts, actual workspace controls, payload-dependent snapshots and acceptance. After checked core integration, C receives central command registration, `CliToolComposer`, shared rendering wiring and matching Integration tests                | Cross-lane wiring and acceptance                                               |

Framework paths start at `src/cli/framework/OpenForge.Cli.Framework/`.
Operations paths start at `src/cli/operations/OpenForge.Cli.Operations/`.
A alone edits root `Composition/Shared/Tools/**` host capabilities.
A also supplies the reviewed private Node version pins and command resolution.
After E's catalogue integrates, F can build the independent native-home and client-path provider before the command graph.
F resolves the declared directory and file bindings from supplied environment and known-folder inputs.
Native inventory still discovers clients. F doesn't select or restrict them.
C alone edits Settings and Ownership codecs. B returns typed addition records and never writes their JSON files.
D uses C's record service and B's config service.
D applies one planned removal or config change at a time and publishes the complete remaining receipt set through C before starting the next change.
A, B and C return pending backup completions to D.
D completes them only after final verification and publication, or keeps them after failure.
The assigned central integrator alone edits command registration.
The mastermind alone edits shared models.
After C's records commit, C can receive the central integration task on the checked combined base.
F keeps `ToolHomes/**` while C uses its stable provider API.
The [native config reader](tool-setup.md#native-config-reader) changes only that private provider.
This split lets the two disjoint tasks proceed together without two writers on one file.
No lane implements a substitute for a frozen interface.

For C and D, matching Rendering and OutputText leaf files belong to that leaf's lane.
C also handles workspace Status and Doctor's Tool presentation.
E's final wording transfer happens only after C and D stop editing those files.
During E's final pass, route wording corrections through E and behavior corrections through the original lane.
Don't start two writers on one file.

## Start order

In Phase 2, create A through D from the Phase 1 tip and run them in parallel.
E can research its exact upstream recipes separately, but final entry integration, docs and journeys follow A through D.
Use at most six concurrent lanes.
Every writer has a dedicated branch and worktree.
The user requests `gpt-6.1-sol` at `xhigh`, through Worker Watch in PowerShell.
Set TEMP and TMP to the user's chosen Worker Watch temp root before agent creation.
Do not use the broken Bash shim.
Check daemon state, exact agent IDs and run IDs before submission.
Use a unique stable request ID per packet.

Each packet gives its exact base commit, file assignment, protected files, source references, focused checks and stop conditions.
It says the worker isn't alone and must keep others' changes.
It requires a local conventional commit and a report.
No worker pushes, merges a PR or changes GitHub.
The mastermind performs the user-authorized local no-fast-forward merges.
This overrides the older lane file's blanket no-merge rule for integration only.

## Copy sources

Copy individual files with `git show <ref>:<path>` into the assigned destination, then adapt them.
Copy the exact files or focused methods. Don't cherry-pick old commits.
Each source below is an exact existing Git path.

| Lane | Ref                                   | Exact source path                                                                                                        | Adaptation                                                         |
| ---- | ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------ |
| A    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Supply/ToolArtifactIntegrity.cs`                       | New asset shape                                                    |
| A    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Supply/ToolArtifactSource.cs`                          | Official source and redirect checks                                |
| A    | `origin/feature/tool-command`         | `src/cli/root/OpenForge.Cli/Composition/Shared/Tools/ToolHttpTransfer.cs`                                                | Bounded transfer and progress                                      |
| A    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Supply/ToolArchiveExtractor.cs`                        | New archive input                                                  |
| A    | `origin/feature/tool-command`         | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Tools/Shared/Supply/ToolArtifactIntegrityTests.cs`            | Focused integrity cases                                            |
| A    | `origin/feature/tool-command`         | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Supply/ToolArchiveExtractorTests.cs`    | Owned archive fixtures                                             |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupPaths.cs`                            | Local backup targets                                               |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupReader.cs`                           | New catalogue targets                                              |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupRestorer.cs`                         | Guarded explicit repair                                            |
| B    | `c2e7d119d`                           | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupSession.cs`                          | Back up dirty tracked bytes                                        |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupSession.Restoration.cs`              | Keep concurrent edits                                              |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolGitRecoveryReader.cs`                      | Git covers saved bytes only                                        |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Filesystem/ToolFileReplacement.cs`                     | Existing replacement mechanics                                     |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolJsonEntries.cs`                       | Byte-preserving JSON Pointer editor                                |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolRegistrationBytes.cs`                 | Encoding and byte spans                                            |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolTextEntries.cs`                       | Copy unique-marker region mechanics only. Drop TOML key rewriting  |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolRoleGuidanceRenderer.cs`              | Copy marker-span renderer only. Drop every role or guidance branch |
| B    | `origin/feature/tool-command`         | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Backups/ToolBackupReaderTests.cs`       | New target inventory                                               |
| B    | `origin/feature/tool-command`         | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Backups/ToolBackupSessionTests.cs`      | Dirty tracked and untracked bytes                                  |
| C    | `origin/feature/tool-run`             | `src/cli/operations/OpenForge.Cli.Operations/Commands/Status/Shared/Aggregation/StatusToolFindingAppender.cs`            | Missing declared tools                                             |
| C    | `origin/feature/tool-run`             | `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/DoctorToolFindingAppender.cs`                | Missing declared tools                                             |
| C    | `origin/feature/tool-run`             | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Status/Shared/Aggregation/StatusToolFindingAppenderTests.cs`   | New records and direct commands                                    |
| C    | `origin/feature/tool-run`             | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Domains/DoctorToolFindingAppenderTests.cs`       | New records and direct commands                                    |
| E    | `origin/feature/tool-code-graphs`     | `src/tools/drafts/codegraph-1.6.2.json`                                                                                  | Facts only. Recheck entry review                                   |
| E    | `origin/feature/tool-codebase-memory` | `src/tools/drafts/codebase-memory-0.12.1.json`                                                                           | Facts only. Keep native account setup                              |
| B    | `c2e7d119d`                           | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Backups/ToolBackupContinuationTests.cs` | Keep the original backup across sequential edits                   |

The writers-backup ref `origin/backup/tool-command-writers-20261113` retains commit `c2e7d119d`.
Check its same-path backup fixes before copying.
Never copy old manifests, registry, leases, role models, guidance delivery, command rewrites, Run or payload snapshots.
Adapt backup helpers to run without WorkspaceLockLease and without the workspace recovery mechanism.
Native package-manager verification replaces the old reviewed-closure promise.
The [entry review checklist](../../maintenance/tools.md) controls entry evidence.

## Phase gates

Run a build and only tests touched or created by the lane.
Run both `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` before every commit.
A local NU1903 override stays on the invocation.
Use reviewed Imprint snapshots for literal output.
Keep LF line endings and separate test tiers.
Run docs formatting, local links and the site build, then index changed routes.
Never run Doctor in this repository workspace.
The overseer handles pushes and hosted full suites.

E runs both tools in temporary Git repositories under D: with owned temporary homes.
Check install, status, direct CLI use, MCP, update and remove.
Record every native installer write and each Open Forge addition or removal.
Use redirected test homes and test profile files for PATH testing.
Never change real persistent user PATH or real shell profiles.
Before native execution, check whether upstream account APIs honor the test redirection.
If not, stop that journey and report the exact missing seam.

After integration, regenerate payload-driven snapshots and fixtures once.
Review received contents before acceptance.
Then start one fresh Sol xhigh adherence reviewer in a detached worktree.
It follows `of-review-adherence.md`, compares every requirement with the local diff and round-2 source and writes a requirement matrix plus stable findings.
Use local Git and supplied CI evidence. It doesn't mutate GitHub.
Route each gap to its lane and resume the same review thread until there are none.
A completed worker run doesn't replace acceptance.

## Decision mapping

Every decision below is dated 14 November 2026:

| Decision | Lane consequence                                                                                                          |
| -------- | ------------------------------------------------------------------------------------------------------------------------- |
| R2-01    | B records additions, C persists them in the committed lock, A/C keep machine facts. No workspace Tool state or extra lock |
| R2-02    | D restores exact lock versions in bare Install                                                                            |
| R2-03    | C writes exact version, channel, changes and optional digest                                                              |
| R2-04    | D uses `--yes`; A never changes PATH or shared files unattended                                                           |
| R2-05    | C/D parse the frozen `id@version` shape                                                                                   |
| R2-06    | A/D disclose latest lookups and use native dry run where declared                                                         |
| R2-07    | D runs chosen native uninstall first. C2 skips Codebase Memory account uninstall with `--yes` or no terminal              |
| R2-08    | A skips Node links before extraction and invokes npm CLI through Node                                                     |
| R2-09    | E declares both native installers unchanged and their account effects                                                     |
| R2-10    | E delivers no Open Forge usage Skill or managed-block line                                                                |
| R2-11    | C/D use the frozen schema-3 result convention                                                                             |
| R2-12    | E starts at current profiles and reviews every three months. SHA256SUMS is sufficient for CodeGraph                       |
| R2-13    | A/E put `CODEGRAPH_NO_DOWNLOAD=1` on version probes                                                                       |
| R2-14    | D preflights the whole batch and stops at the first effect failure                                                        |
| R2-15    | A detects all four npm-family clients and manager-global-bin readiness                                                    |
| R2-16    | A keeps round-1 home, launcher, private Node and chosen PATH rules                                                        |

## Recorded choices

The maintainer decided C1 through C7 on 14 November 2026:

| ID  | Decision                                                                                                                                                                                                | Lane consequence                                                                             |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| C1  | CLI version selectors are major.minor or exact stable SemVer. Bare ID selects latest. No tags or full npm ranges                                                                                        | A parses, D accepts specs. Settings still use minor.x                                        |
| C2  | Codebase Memory account uninstall needs live interactive confirmation naming its binary and all agent entries. `--yes`, no terminal or a decline skips it and removes only Open Forge project additions | D chooses the native step, B removes project additions and E tests both paths                |
| C3  | Keep optional profileDigest with the fixed JSON projection                                                                                                                                              | B computes it and C serializes it                                                            |
| C4  | Keep the machine-state filenames and physical-path SHA-256 workspace keys                                                                                                                               | C writes them                                                                                |
| C5  | Keep `--channel` on named Install and Update and schema-3 `data.tools` results                                                                                                                          | C/D form results and the integrator wires output                                             |
| C6  | CodeGraph starts at 1.6.2 with Node 20, Codebase Memory at 0.12.1 and reviews run every three months                                                                                                    | A checks requirements and E declares profiles                                                |
| C7  | Project Remove keeps account-wide native settings, names them and prints manual change commands. No per-project restoration or cross-project registry                                                   | B returns kept native facts and manual arguments. D reports them without executing a restore |

The former Q1 through Q14 are resolved by R2-01 through R2-16 and C1 through C7.
No C1 through C7 question stays open.

---
open-forge:
  description: Assign tool setup files and exact copy sources after maintainer review
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool setup lanes

Tool setup lanes are the file assignments that turn the reviewed spec into five implementation slices.
For example, lane D consumes the channel and config-change shapes without choosing their record formats.

Status: Step 0 ready for maintainer review after its recorded checks. No implementation lane starts before approval.

## Shared types

Step 0 adds compile-only models under `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/`.
The folders are `Catalogue`, `Channels`, `Configuration`, `Launchers`, `Records` and `Services`.
The namespace prefix follows the existing Framework convention, `OpenForge.Cli.Core.Framework.Tools.Models`.
These records aren't command requests, wire serializers or implementations.
They carry facts across the accepted lane boundaries.
The integrator handles any reviewed shared-model changes.

## File assignments

Each assignment includes tests at the matching path below the existing Unit, Integration or EndToEnd project.
Each command leaf gets its own OutputText file.

| Lane       | Files and responsibility                                                                                                                                                                                                                                         | Depends on                               |
| ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- |
| A          | `Framework/Tools/Shared/{Supply,PackageManagers,Runtimes,Launchers,Path}/**` and root `Composition/Shared/Tools/**`. Published channels, private Node, generated launchers and platform PATH effects                                                             | Reviewed models                          |
| B          | `Framework/Tools/Shared/{ConfigChanges,Backups,Filesystem}/**`. Four changes, read-back, drift and Git-aware backups                                                                                                                                             | Reviewed models                          |
| C          | `Framework/Tools/Shared/{Records,Observation,Serialization}/**`, existing Settings and Ownership codecs, `Commands/Tool/{List,Inspect,Status}/**`, corresponding Rendering and OutputText, workspace Status and Doctor appenders, schemas v6 and v2 codec parity | A detection shapes, B observation shapes |
| D          | `Commands/Tool/{Install,Update,Remove,Shared/Writing}/**`, corresponding Rendering and OutputText, Repair tool-backup integration                                                                                                                                | A, B and C contracts                     |
| E          | `Framework/Tools/Catalogue/**`, tool usage Skills, both CLI Skill copies, `docs/tools.md`, site tool pages and final help wording                                                                                                                                | Catalogue shapes and admission review    |
| Integrator | `CliToolComposer`, root composition wiring, central changed models, shared contracts and payload-dependent snapshots                                                                                                                                             | All five lane results                    |

`Framework` paths in this table are relative to `src/cli/framework/OpenForge.Cli.Framework/`.
`Commands` paths are relative to `src/cli/operations/OpenForge.Cli.Operations/`.
No two lanes edit the same central codec or shared model.
Lane C handles Settings and Ownership files as one assignment.
A, B, C and E can proceed after review. D can use agreed fakes before wiring.
The integrator wires the final graph, checks cross-lane behavior and reviews the complete diff.
The current user request starts none of these lanes.

## Copy sources

Copy individual files with `git show <ref>:<path>` into the assigned destination, then adapt them.
Don't merge or cherry-pick the old design.
Each source below is an exact existing Git path.

| Lane | Ref                                   | Exact source path                                                                                                      | Adaptation                                                        |
| ---- | ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| A    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Supply/ToolArtifactIntegrity.cs`                     | New asset shape                                                   |
| A    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Supply/ToolArtifactSource.cs`                        | Official source and redirect checks                               |
| A    | `origin/feature/tool-command`         | `src/cli/root/OpenForge.Cli/Composition/Shared/Tools/ToolHttpTransfer.cs`                                              | Bounded transfer and progress                                     |
| A    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Supply/ToolArchiveExtractor.cs`                      | New archive input                                                 |
| A    | `origin/feature/tool-command`         | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Tools/Shared/Supply/ToolArtifactIntegrityTests.cs`          | Focused integrity cases                                           |
| A    | `origin/feature/tool-command`         | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Supply/ToolArchiveExtractorTests.cs`  | Owned archive fixtures                                            |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupPaths.cs`                          | Local backup targets                                              |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupReader.cs`                         | New catalogue targets                                             |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupRestorer.cs`                       | Guarded explicit repair                                           |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupSession.cs`                        | Back up dirty tracked bytes                                       |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolBackupSession.Restoration.cs`            | Keep concurrent edits                                             |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Backups/ToolGitRecoveryReader.cs`                    | Git covers saved bytes only                                       |
| B    | `origin/feature/tool-command`         | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Filesystem/ToolFileReplacement.cs`                   | Existing replacement mechanics                                    |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolJsonEntries.cs`                     | Byte-preserving JSON Pointer editor                               |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolRegistrationBytes.cs`               | Encoding and byte spans                                           |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolTextEntries.cs`                     | Copy unique-marker region mechanics only. Drop TOML key rewriting |
| B    | `origin/feature/tool-registration`    | `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Registration/ToolRoleGuidanceRenderer.cs`            | Per-tool line and marked-block rendering. Drop roles              |
| B    | `origin/feature/tool-command`         | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Backups/ToolBackupReaderTests.cs`     | New target inventory                                              |
| B    | `origin/feature/tool-command`         | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Backups/ToolBackupSessionTests.cs`    | Dirty tracked and untracked bytes                                 |
| C    | `origin/feature/tool-run`             | `src/cli/operations/OpenForge.Cli.Operations/Commands/Status/Shared/Aggregation/StatusToolFindingAppender.cs`          | Missing declared tools                                            |
| C    | `origin/feature/tool-run`             | `src/cli/operations/OpenForge.Cli.Operations/Commands/Doctor/Shared/Domains/DoctorToolFindingAppender.cs`              | Missing declared tools                                            |
| C    | `origin/feature/tool-run`             | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Status/Shared/Aggregation/StatusToolFindingAppenderTests.cs` | New records and direct commands                                   |
| C    | `origin/feature/tool-run`             | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Domains/DoctorToolFindingAppenderTests.cs`     | New records and direct commands                                   |
| E    | `origin/feature/tool-code-graphs`     | `src/tools/drafts/codegraph-1.6.2.json`                                                                                | Facts only. Recheck admission                                     |
| E    | `origin/feature/tool-codebase-memory` | `src/tools/drafts/codebase-memory-0.12.1.json`                                                                         | Facts only. Skip account setup                                    |

The writers-backup ref `origin/backup/tool-command-writers-20261113` retains commit `c2e7d119d`.
Check its same-path backup fixes before copying.
Never copy old manifests, registry, leases, role models, command rewrites, Run or old payload snapshots.
Native package-manager verification replaces the old reviewed-closure promise.
The [admission checklist](../../maintenance/tools.md) controls entry evidence.

## Checks

For each lane, build and run only tests it touches or creates.
Run `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` before every commit.
Run formatting, Markdown and link checks, then the site build for docs changes.
Index changed routes after checking their navigation plan.
The overseer handles pushes and hosted CI. CI is the full gate.
A local NU1903 override is allowed only on the invocation.
No full managed or native suite runs locally.

## Not decided yet

These are proposals to review before dependent implementation:

- Q1 - Should missing team installations use the committed exact tested baseline, while compatible existing patches are reused?
- Q2 - Are `version` and `profileDigest` the lock field names, and should the digest cover setup, config templates and guidance through a fixed ordered UTF-8 encoding?
- Q3 - Are `.open-forge-state/tools/<id>.json` and the three machine-state filenames in Tool records the chosen locations?
- Q4 - Should `--allow-download` remain separate from `--automatic`, and should non-interactive PATH setup and shared-file deletion require a future dedicated flag while defaulting to manual PATH instructions and retained files?
- Q5 - Should Update accept `--version` and `--channel`, and should the local value-taking `--version` shadow the global version bypass on Tool Install and Update?
- Q6 - Should dry-run remain network-free and report unresolved exact versions, then resolve and disclose them again before apply consent?
- Q7 - Should Remove restore recorded unchanged project setup additions directly, or should a reviewed local upstream uninstall also run?
- Q8 - What serialization protects cooperating machine-wide tool writers, and how should approved Node archives' internal npm links be extracted through the existing link-rejecting extractor?
- Q9 - Are the proposed MCP key `codebase-memory`, Skill IDs and direct conditional-line wording the desired names?
- Q10 - Should all six leaves use `data.tools`, with writers adding `preview`, `plan` and `outcomes`, before their full wire payloads are fixed by the implementation contracts?
- Q11 - What minimum CLI, npm, OS and libc versions and telemetry read-back commands admit the first profiles, and how should CodeGraph's attestation mismatch be closed?
- Q12 - How should read commands check CodeGraph's wrapper readiness before a version probe, so its first-launch download fallback stays available without making Status or dry-run download?
- Q13 - Should bare writers preflight every selected tool and perform no effects if one is blocked, then stop on the first failed tool after effects start? Is cancellation, invalid input, blocked, failed, incomplete, warnings and completion the chosen aggregate precedence?

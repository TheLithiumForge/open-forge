---
open-forge:
  description: Define published tool installation and additive project configuration
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool setup

Tool setup is a CLI capability that installs tools through published channels and adds project configuration.
For example, a teammate installs a declared CodeGraph range and uses `codegraph` directly after setup.

Status: V1 design decided. Shared types are compile-only. The maintainer reviews this step before implementation lanes start.

## Catalogue

Framework Tools has one typed C# dictionary indexed by tool ID.
A `ToolCatalogueEntry` contains identity, channels, detection and version profiles.
Identity has ID, name, `first-party` or `third-party` origin, upstream and licence.
Each channel declares its package or release source and runtime, minimum manager, OS, CPU and Linux libc requirements.
V1 adapters cover npm and GitHub release.
The same channel contract can later accept PyPI through pipx or uv, RubyGems, `dotnet tool`, Homebrew, winget and scoop.
Those adapters aren't V1 implementations.

A profile is one non-overlapping major.minor range, its tested exact versions and minimum Open Forge version.
For example, `1.6.x` means `>=1.6.0 <1.7.0`, excluding prereleases.
Each profile gives an optional setup argument vector, declared effects, config changes, a Skill resource and one conditional managed-block line.
A bounded version probe and a typed parser ID detect the installed version.
No manifest decoder or external catalogue source is added.

## Channels

Use the [common channel order](../contracts/tool/behavior.md#channel-choice).
An override selects an advertised channel and still runs its requirement checks.
Resolve and show the exact version before approval.
Package-manager installs use the manager's integrity checks and record the exact version.
That is weaker than a reviewed dependency closure.
GitHub release downloads require a pinned per-asset SHA-256 or a verified attestation binding the asset to the expected repository, build and source.
Mismatch blocks installation.
Only official upstream recipes ship. Open Forge doesn't redistribute tool bytes.

## Launchers

The per-user home is `~/.open-forge/`, or `%USERPROFILE%\.open-forge\` on Windows.
Its folders are:

| Folder                     | Content                                 |
| -------------------------- | --------------------------------------- |
| `bin/`                     | Generated launchers and native binaries |
| `runtimes/node/<version>/` | Official private Node runtimes          |
| `tools/<id>/<version>/`    | Tool files installed by Open Forge      |
| `state/`                   | Machine facts and PATH history          |

Generate small `.cmd` files and executable `sh` scripts with absolute paths baked in at install time.
A launcher starts the tool entry with its pinned runtime or forwards to the tool's release launcher.
A real `.exe` can be placed directly in bin.
Never put `node` in bin.
Update rewrites launchers.
Open Forge takes no part when the tool runs.

For native Windows MCP hosts that can't start `.cmd`, use `cmd /c <tool>`.
This host launch form is marked "verify in native tests".
It doesn't turn setup or native config argument vectors into shell strings.

## Private Node

When an npm tool lacks a suitable Node, download an official portable archive from `nodejs.org/dist`.
Pin its SHA-256 from signed SHASUMS256 and keep signature evidence with the recipe.
Use that Node's own npm to install into `tools/<id>/<version>/`.
Native-addon prebuilds then match the runtime used by the launcher.
Tools needing the same Node version share it.
Tools can also require different Node versions.
Remove a runtime only when no launcher uses it, with the shared-file confirmation.
The user's Node and PATH entries remain unchanged.
There is no Node proxy.

Illustrative future Rune flow:

```text
published Rune npm package + no suitable Node
  -> verified private Node
  -> that Node's npm installs Rune
  -> absolute-path Rune launcher
  -> direct rune command
```

Rune isn't published and has no V1 catalogue entry.

## PATH

Status and Inspect always show bin and current PATH membership.
If bin is missing, offer these choices:

- "Add it for me" - on Windows, use `Environment.SetEnvironmentVariable(..., EnvironmentVariableTarget.User)`. Never use `setx` or system PATH.
- "Add it for me" - on macOS or Linux, append a self-identifying marked block to shell startup files you choose.
- "I'll do it myself" - print the exact line.

Offer detected `.zshrc`, `.bashrc`, `.profile` and fish config files, plus a custom file such as a shared profile.
Render POSIX and fish syntax separately.
Remember every added entry and block per machine in state.
The marks identify their purpose even if that state is lost.
When the last tool is removed, offer PATH removal and ask for confirmation.
Say that terminals and agent apps need a restart.
Current-process PATH membership doesn't prove a restarted agent can launch the tool.

## Configuration

Use the four kinds and drift rules in [Tool record behavior](../contracts/shared/tool-records/behavior.md).
The design keeps recorded prior values separate from applied values.
CodeGraph's exclude uses JSON Pointer `/exclude` with array union.
Text changes use unique markers, including Codex TOML sections.
Native settings use allow-listed commands with read-back and restore vectors.
Telemetry off is the one accepted default change.
It is always disclosed, including its account-wide scope for CodeGraph.
Keep tool hooks, Claude permissions, watchers and other native behavior.

Git covers tracked unchanged bytes. A sibling `.bak` covers other existing bytes.
Git can't recover uncommitted bytes.
Failures keep actual effects and guarded recovery facts.

## Records

[Tool record interface](../contracts/shared/tool-records/interface.md) defines the four locations and their schemas.
Settings declare the supported range.
The committed lock records the exact tested version and applied profile digest.
Workspace-local ignored state keeps prior and applied values.
Machine state keeps channels, paths, launchers and PATH history.
The exact tested baseline isn't a promise of isolated per-project executable versions.
There is one active version per tool per machine.

## Flows

Install detects, selects a channel, resolves an exact version and shows the plan.
After consent, it installs missing bytes, verifies the direct command, runs setup, adds config and guidance, then saves verified records.
Bare Install selects every declared tool.
Repeating Install reconfigures with drift checks.

Update checks drift, resolves the newest supported range, discloses account-wide replacement, updates bytes and launchers, then migrates config and records.
Only known support in a newer catalogue justifies "update Open Forge first".
Status detects a user's own update and offers reapply inside support or warns outside it.

Remove restores this project's unchanged Open Forge additions.
It prints the native package-manager uninstall command.
It asks before deleting Open Forge-placed tool files or runtimes because other projects can use them.
It separately offers confirmed PATH removal after the last tool.

## Composition

Framework `Framework/Tools/Models/` holds only catalogue facts, records and service request and result shapes in step 0.
Operations keeps command meaning, planning, consent, effects and result status.
Root hosts bounded processes, network transfer and platform PATH effects.
Rendering and OutputText keep presentation separate.
Source-generated codecs arrive in lane C.
There are no command registrations, adapters, process runners or mutations in this step.

Existing workspace locking, permission and expected-state rules apply to project changes.
The supported failure limits cover malformed input, interrupted operations, dependency failure and cooperating writers.
They don't defend against a malicious process already controlling the same account.
Machine-wide writer serialization and archive-link handling need the review answers in the [lane plan](tool-setup-lanes.md#not-decided-yet).

## Decisions

Every row links to the issue comment for its decided direction.
The 14 November task brief supplies the more exact details named in the last column:

| Date       | Decision                                         | Issue comment                                                                                      | Task-brief detail                                 |
| ---------- | ------------------------------------------------ | -------------------------------------------------------------------------------------------------- | ------------------------------------------------- |
| 2026-11-13 | Published channels and additive setup            | [Redesign](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)        | Keep hooks and permissions                        |
| 2026-11-14 | Typed catalogue and major.minor profiles         | [Catalogue](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)       | Tested versions and minimum CLI                   |
| 2026-11-14 | Existing install, manager, private Node, release | [Channel order](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)   | Override through `--channel`                      |
| 2026-11-14 | User home and absolute launchers                 | [Launchers](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)       | No Node in bin, Windows MCP test                  |
| 2026-11-14 | Official verified private Node                   | [Private runtime](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6850220404) | Shared by version, own npm                        |
| 2026-11-14 | Chosen user PATH setup                           | [PATH](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)            | User .NET API, chosen shell files and custom file |
| 2026-11-14 | Four change kinds and local preimages            | [Config changes](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)  | Drift and guarded removal                         |
| 2026-11-14 | Git and sibling backups                          | [Backup basis](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6849450008)    | Dirty bytes need backup                           |
| 2026-11-14 | One active machine version                       | [Versions](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)        | Earlier install range, newest update              |
| 2026-11-14 | Six commands and team Install                    | [Commands](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)        | Remove Run, Choose, Configure and roles           |
| 2026-11-14 | Four record locations                            | [Records](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)         | Fresh schemas from v5 and v1                      |
| 2026-11-14 | Manager integrity or verified download           | [Supply](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)          | Honest dependency-closure limit                   |
| 2026-11-14 | CodeGraph and Codebase Memory in V1              | [Entries](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)         | Rune example only                                 |
| 2026-11-14 | Direct commands and per-tool guidance            | [Agents](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832)          | No help pointer                                   |

The old #26 and #30 design is superseded. Its branches provide copy candidates only.

---
open-forge:
  description: Define the lean tool catalogue, command execution, integration edits and settings list behind the Tool commands
  tags: [Memory, Document, CLI, Tool, TechnicalDesign, CurrentTruth]
---

# Tool setup design

Tool setup design is the implementation model behind `tool list`, `tool install` and `tool remove`.
For example, the CodeGraph catalogue entry holds its npm install command, its own setup command and one JSON union in `codegraph.json`.

Status: Lean design decided on 17 November 2026. It replaces the round-2 design, which is parked in draft PR #32.

## Placement

| Layer       | Owns                                                                                                   |
| ----------- | ------------------------------------------------------------------------------------------------------ |
| Framework   | `Framework/Tools/`: catalogue entries and definitions, the process host contract, integration editing |
| Framework   | `Framework/Settings/`: the `tools` list read and its add and remove edits                              |
| Operations  | `Commands/Tool/`: bindings, requests, planning, consent, application and results for the three leaves  |
| Rendering   | `Presentation/Tool/`: help, report selection, text rows and plan lines, JSON contexts                  |
| OutputText  | `Tool/`: authored wording                                                                              |
| Root        | `Composition/Shared/Tools/`: the real process host and the Tool composition                            |

Composition takes an optional process host from `CliCompositionInputs`. Integration tests pass a fake host. Production uses the real one.

## Catalogue

Each entry is one typed C# record. Commands are fixed strings with a shell kind: `System` runs through `cmd.exe /d /s /c` on Windows and `/bin/sh -c` elsewhere, and `PowerShell` runs through `powershell -NoProfile -NonInteractive -EncodedCommand`. An entry can give Windows and other platforms different command lists.

### CodeGraph

| Field        | Value                                                                                                              |
| ------------ | ------------------------------------------------------------------------------------------------------------------ |
| ID, category | `codegraph`, Code search                                                                                           |
| Upstream     | https://github.com/colbymchenry/codegraph, MIT                                                                    |
| Method       | `npm`, tested version 1.6.2                                                                                        |
| Install      | `npm i -g @colbymchenry/codegraph@1.6.2`                                                                           |
| Detection    | `codegraph --version`                                                                                              |
| Setup        | `codegraph install --yes --location local`                                                   |
| Uninstall    | `codegraph uninstall --yes --location local --keep-cli`, then `npm uninstall -g @colbymchenry/codegraph` |
| Integration  | JSON union of `<mainRoot>/memory/archived/**` into `codegraph.json` at `/exclude`                                 |

Evidence, checked on 17 November 2026 at tag v1.6.2:

- The [README](https://github.com/colbymchenry/codegraph/blob/v1.6.2/README.md) documents `npm i -g @colbymchenry/codegraph`, `codegraph install` with `--yes`, `--location`, `--target` and `--init`, `codegraph uninstall` with `--keep-cli`, `--target` and `--yes`, `codegraph telemetry off` and `codegraph.json` with an `exclude` array.
- The [CLI source](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/bin/codegraph.ts) defines `install --yes` as "Non-interactive: defaults to --location=global --target=auto, auto-allow on" and `uninstall --yes` as "Non-interactive: defaults to --location=global --target=all". Both prompt for `--target` and `--location` only without `--yes`.
- So `--yes` alone runs without prompts but writes account-wide agent files. Open Forge adds only `--location local`, which keeps setup and uninstall in the project, and leaves the targets to CodeGraph's own defaults: the agents it detects on install and all of them on uninstall.
- `install` wires up agents only. It doesn't index, place a binary or change PATH. npm puts `codegraph` in its own global bin.
- The installer asks about telemetry up front. `--yes` skips that question and keeps the default, so the plan names `codegraph telemetry off`.

### Codebase Memory

| Field        | Value                                                                                                     |
| ------------ | --------------------------------------------------------------------------------------------------------- |
| ID, category | `codebase-memory`, Code search                                                                            |
| Upstream     | https://github.com/DeusData/codebase-memory-mcp, MIT                                                     |
| Method       | `script`, tested version 0.12.1                                                                           |
| Install      | Linux and macOS: `curl -fsSL https://raw.githubusercontent.com/DeusData/codebase-memory-mcp/main/install.sh \| bash` |
|              | Windows PowerShell: `irm https://raw.githubusercontent.com/DeusData/codebase-memory-mcp/main/install.ps1 \| iex` |
| Detection    | `codebase-memory-mcp --version`                                                                           |
| Setup        | None. The installer configures agents                                                                     |
| Uninstall    | Linux and macOS: `codebase-memory-mcp uninstall --yes`                                                    |
|              | Windows PowerShell: `codebase-memory-mcp uninstall --yes --dir "$env:LOCALAPPDATA\Programs\codebase-memory-mcp"` |
| Integration  | Marked block with `/<mainRoot>/memory/archived/` in `.cbmignore`                                          |

Evidence, checked on 17 November 2026 at tag v0.12.1:

- The [README](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/README.md) documents the `curl … install.sh | bash` one-line installer and `codebase-memory-mcp uninstall`, and says `install` configures every coding agent it detects.
- [`install.sh`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/install.sh) downloads the latest release into `~/.local/bin` and runs `codebase-memory-mcp install -y --force --dir=…`. It accepts no version option.
- [`install.ps1`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/install.ps1) downloads the latest release into `%LOCALAPPDATA%\Programs\codebase-memory-mcp` and runs the same `install`. It reads no parameters through `iex` and doesn't prompt.
- The README's Windows one-liner uses `scripts/setup-windows.ps1`, which prompts before it edits PATH and Claude Code settings. Open Forge runs commands without input, so it uses `install.ps1`.
- The binary's `install` adds its folder to the user PATH on Windows and to a shell profile elsewhere, but only when it placed the binary itself. The plan says so.
- `uninstall` removes the binary, its PATH entry and every agent entry, and keeps indexes unless `--delete-indexes` is passed. Its default folder is `~/.local/bin`, so the Windows command passes the `install.ps1` folder.
- The installers always fetch the latest release. `CBM_DOWNLOAD_URL` could point them at a tag, but upstream documents it as a testing override, so V1 doesn't use it.

## Requirements

`npm` is present when `npm --version` exits 0. `script` is present when `curl --version` and `bash --version` exit 0 on Linux and macOS, and when PowerShell starts on Windows.

## Process host

`IToolProcessHost.RunAsync` takes a command, the workspace root, a time limit and a cancellation token. It returns whether the process started, its exit code, whether it timed out and the last 8 KiB of combined output.
Standard input is closed. The environment is inherited.
Timeout or cancellation kills the whole process tree.
Detection and requirement probes have 15 seconds. Install, setup and uninstall commands have 15 minutes.
The report shows the last 20 lines of a failed command's output.

## Integration edits

Integration edits are pure byte functions in the Framework:

- JSON union parses with `JsonNode`, adds the string when missing and writes indented JSON. It keeps other members and their order, like the settings writer. Removal deletes the string and deletes a file left as `{"exclude": []}` with no other member.
- Marked block finds exactly one start marker line and one end marker line. Adding appends the block at the end of the file. Removal deletes the block lines and deletes a file left empty.

Malformed JSON, a non-array `exclude`, or duplicated, partial or reversed markers are conflicts.

## Settings

Settings schema v6 adds `tools`, an array of unique tool IDs. Earlier schemas don't define it, and every older key keeps its meaning.
Adding and removing an ID use the settings codec's object-model edit, which keeps unknown keys and their order.

## Write path

Planning reads each target's bytes and expectation. After the commands run, Install reads the targets again and plans its changes from those bytes.
The application acquires the workspace lock as other writing commands do, prepares one recovery bundle for its targets, revalidates each expectation and applies each change through `FileChangeApplier`.
A changed target is `target-changed-during-apply`. A verified run deletes its recovery bundle. A failed run keeps it and reports its path.

## Tests

- Unit tests in the Framework project cover the integration edits and the settings `tools` edits.
- Integration snapshot tests cover `tool list`, `tool install` and `tool remove` through the composed CLI with a fake process host.
- EndToEnd covers Tool help only.

## Accepted decisions

The maintainer accepted these on 18 November 2026:

1. A detected tool, at any version, skips its install command, and the plan shows that version.
2. Codebase Memory isn't pinned. Its installers fetch the latest release, and 0.12.1 is the tested version. `CBM_DOWNLOAD_URL` stays unused because upstream calls it a testing override.
3. Codebase Memory's own installer adds its folder to PATH. Open Forge never edits PATH itself, and the plan says so.
4. On Windows, Codebase Memory installs through `install.ps1`, which takes no input, rather than the interactive `setup-windows.ps1`.
5. CodeGraph uses its own default targets with `--location local`, as the evidence above shows.
6. In the plan, a setup command's effect and notes sit on their own indented lines under the command.
7. Remove applies Open Forge's own changes without a plan question and asks only about the tool's uninstall. `--dry-run` previews both.
8. Install stops in a folder without `.agents`, because the tools list has nowhere to go.

## Parked ideas

Draft PR #32 keeps the fuller round-2 design and its records. These ideas moved out of V1:

- Verified downloads, SHA-256 pins, private Node runtimes, launchers and PATH setup.
- Tool settings, such as turning CodeGraph telemetry off, and their previous values.
- Agent client root tables and the native plan reader.
- The `inspect`, `status` and `update` commands, version profiles and the profile digest.
- The project tool lock and machine state.
- pnpm, Yarn and bun.

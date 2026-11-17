---
open-forge:
  description: Trace the published Status and Doctor consumer's unredirected Tool machine-state reads
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Published Tool isolation

Published Tool isolation is the missing process-entry connection that lets an owned EndToEnd child select its Open Forge Tool home.
The current published Status and Doctor path reaches the Windows user's Tool state even when the workspace declares no tools.

Status: Source diagnosis only. F-P1 is a high-severity test-isolation gap before the affected published checks run.

## Read trace

The checked source is C's accepted Root candidate on `885b17db3e56ceb6839ba23609e85a4380c75018`, read at `D:/Repositories/open-forge-worktrees/tool-setup-c`.
The Framework reader is also present in Main at that core commit.
C's `evidence/command-composition.md:202-214` establishes typed isolation for in-process captures. It doesn't establish published-child isolation.

The actual read chain is:

1. `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs:15-20` invokes workspace Status and Doctor in `LegacyFilesAreNeitherReadNorDeleted` after Framework Install and Update.
2. `Shared/PublishedProcess/PublishedWorkspaceLockStore.cs:26-30` provides `TestDataHome.EnvironmentVariables(temporary.Path)`. The test-support method at `src/cli/tests/support/OpenForge.Cli.TestSupport/Isolation/TestDataHome.cs:107-123` sets `OPENFORGE_DATA_HOME`, `XDG_DATA_HOME`, `LOCALAPPDATA` and `HOME`. These select the legacy data store, not the Tool home.
3. `Shared/PublishedProcess/PublishedProcessTestSupport.cs:22-31` passes that map to `ProcessRunner`. `ProcessRunner.cs:187-211` overlays it on the child environment. It passes no typed Root inputs.
4. The real executable's `src/cli/root/OpenForge.Cli/Program.cs:15` calls the normal `CliHost.RunAsync`. `Hosting/CliHost.cs:24-43` creates `CliCompositionInputs` without `Tools`.
5. `Composition/CliCompositionRoot.cs:85-91` connects the deferred Tool reader to workspace commands. `StatusOperation.cs:63-70` and `DoctorOperation.cs:29-36`, below `src/cli/operations/OpenForge.Cli.Operations/Commands/`, invoke it without an empty-tool-declaration guard.
6. `Composition/Shared/ToolComposition/ToolCompositionFactory.cs:32-35` creates the graph and obtains `graph.Homes.HomePath`. `ToolCompositionGraph.cs:36-42` supplies `Environment.GetFolderPath` as its known-folder capability.
7. `Composition/Shared/ToolHomes/ToolHomeProvider.cs:58-60` uses the OS UserProfile known folder, then appends `.open-forge`. Only its typed `HomePath` input overrides this choice. `ToolHomePaths.Resolve` checks actual path components and directories. This isn't just a constructed path string.
8. `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/Observation/ToolWorkspaceObservationReader.cs:11-12` calls `records.ReadAsync` before its declared-tool loop at line 17.
9. `Shared/Observation/ToolRecordReader.cs:44-73` inventories Tool and runtime records, independently of settings declarations. Lines 74-84 read the workspace and PATH records. `Inventory`, lines 136-143, calls `Directory.Exists` and `Directory.EnumerateFiles`. `ReadAsync`, lines 118-125, calls `File.Exists`, `Directory.Exists` and, for existing files, `File.ReadAllBytesAsync`.

The final reader paths are defined by `Framework/Tools/Shared/Records/ToolRecordPaths.cs:17-26`:

- `<Windows UserProfile>/.open-forge/state/tools/*.json`.
- `<Windows UserProfile>/.open-forge/state/runtimes/node/*.json`.
- `<Windows UserProfile>/.open-forge/state/workspaces/<workspace-key>.json`.
- `<Windows UserProfile>/.open-forge/state/path.json`.

The reader checks catalogue-known tool/runtime filenames even when their directory inventory is empty.
It also reads additional discovered record filenames when present.
Only after those reads does the workspace reader iterate declared tools.
With no `settings.tools`, that final loop is empty, so it doesn't run per-tool native detection. The machine-store access has already happened.
Malformed or unavailable machine records can therefore also affect workspace findings with no declared tools.

This diagnosis establishes real filesystem calls directed at the per-user store.
It doesn't assert that a particular machine JSON file currently exists or was read in an executed test.
No real user store was opened to establish this finding.

## Affected selection

Within the five published classes in the [final payload plan](payload-final-pass.md#focused-selections), the current reachability is:

| Class after `OpenForge.Cli.EndToEndTests.`          | Cases | Tool machine-store path reached                                                  |
| --------------------------------------------------- | ----- | -------------------------------------------------------------------------------- |
| `PublishedInstallProcessTests`                      | 6     | Yes. Its `LegacyFilesAreNeitherReadNorDeleted` case calls both Status and Doctor |
| `PublishedEmbeddedPayloadProcessTests`              | 2     | No. Its commands are Framework Install preview and available Extension List      |
| `PublishedRouteInitRestorationProcessTests`         | 2     | No. Its commands are Framework Install, Route Init and help                      |
| `PublishedMainRootDefaultCompatibilityProcessTests` | 2     | No. Its commands are Framework Install and repeated Install                      |
| `PublishedMainRootOfflineStartupProcessTests`       | 1     | No. Its commands are Framework Install and Route Include                         |

The same shared process helper doesn't provide Tool-home isolation to any other published Status or Doctor caller.
The table limits this finding to the planned five-class selection.
Unrelated help and command factories stay lazy. This finding doesn't infer Tool reads from their registration alone.

## Missing connection

The accepted Root capability is the resolved Tool home supplied through `CliCompositionInputs.Tools.HomePath`.
C's `CliToolIsolation` supplies that capability and throwing hosts for in-process captures.
The stock published entrypoint doesn't populate it, and the EndToEnd process helper can currently supply only arguments and environment values.
No existing environment field connects the legacy `TestDataHome` override to this Tool-home input.
Changing `HOME` or `USERPROFILE` alone doesn't replace the Windows known-folder capability used here.

The coordinator and C need to close this published-entry isolation connection before running the affected case.
Use a normal supported host/home-selection input with the same meaning as the accepted typed input.
This diagnosis chooses no new option, environment name, test switch or launcher protocol.
Substituting the manual in-process host would exercise a different boundary from the published executable.

For the no-tools case, an eligibility guard would need to precede graph/home construction and the combined record read.
Returning early from the existing declared-tool loop can't prevent the reads already traced above.
Such a guard doesn't replace home selection for published commands that do declare tools.

Only this short evidence record is added by this question.
No implementation, tests, CLI/tool invocation, store access, Git mutation or persistent platform change ran.

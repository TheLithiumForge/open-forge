# Tool registration

Tool registration is a Framework service that plans declared installer-entry changes and conditional agent guidance.

For example, an installer writes a hook command. `ToolEntryPlanner` checks its exact original and returns prior bytes, replacement bytes and typed checks. Writers then apply the edit under permissions and backups.

Status: Framework services implemented. Command composition belongs to Writers.

## Integration

Use the services in this order:

1. Resolve component choices with `ToolOptionalComponents.Resolve()`. Missing choices keep their explanation, default and finite flag for Operations to present. `ParsePromptHook()` accepts `yes` and `no`.
2. Pass the chosen declarations, declined components and the host's template values to [ToolEntryPlanner](ToolEntryPlanner.cs). Bind its `Services` to `ToolEntryServices`.
3. Require every plan check to be `Original` before applying an edit. Before each write, call [ToolEntryVerifier](ToolEntryVerifier.cs) `RecheckAsync()` against the complete prior bytes. Writers handle permissions, exclusions, backups and writes.
4. After writing, call `VerifyAsync()`. Publish its rewritten-entry and component-decline receipts only when every check is `Matching` or `Declined`. A failed check stops further writes while the writer keeps completed installer evidence.
5. For passive checks, call `ObserveReceiptsAsync()` with the recorded resolved values. Map these checks to the caller's finding prefix. These checks cover the declared parts of each file.

`${openForge}` comes from the host. Pass `open-forge` when it is available on PATH. Otherwise pass the absolute running executable. Hook and marked-line templates declare their host quoting.

## Hook events

Set `MatchCommand` and use the event Pointer for a Claude hook, for example `/hooks/UserPromptSubmit`. The planner finds the exact command across all matcher groups and their hook arrays. Repeated matches stop the plan. Other hooks stay intact.

`PlatformOverrides` gives a platform's original and replacement pair. Pass the host's exact target in `Values[ToolVocabulary.TargetVariable]`. Windows, macOS and Linux overrides use `ToolHookPlatforms`. Omitted platforms use the common declaration. Receipts keep the resolved values. `RequiredReplacementSubstring` checks the literal text needed by the tool's uninstaller.

Before running the installer, capture the files in `BeforeInstallerFiles`. A key whose value is `null` means observed absence. An omitted key means the prior state is unknown. Framework compares prior groups with the installer outcome before recording a new group's empty shape in `ToolRewriteReceipt.CreatedHookGroup`.

For later removal, pass the saved receipt through `CreatedHookGroups`. Use the rewritten command in the complete expected removal entry and clear its platform overrides. `RemoveCreatedEmptyGroup` permits removal of that group only after every command in it is removed and its creation receipt still matches. Repeated group shapes stop receipt-based cleanup. Keep groups with user hooks, pre-existing groups and groups whose creation is unknown. `AbsentCommands` lists other exact spellings, including resolved replacements, that must stay absent after decline.

## Role guidance

Bind [ToolRoleGuidancePlanner](ToolRoleGuidancePlanner.cs) `Services` to `ToolRoleGuidanceServices`. Pass the complete desired role set. An empty set removes conditional lines from both managed blocks.

Each generated line has a trailing role and tool marker so the next plan can switch or clear it. The shared Markdown reader finds the Open Forge block. Authored content and tool blocks stay intact. Missing files carry prior absence. Recheck each edit before writing, then use `ObserveAsync()` to validate both files. The required Skill is a separate writer effect.

## Limits

TOML uses text checks for a simple named table. Its `command` is one double-quoted value. Its `args` is one single-line array of double-quoted values. Both must match the declared original spellings, allowing surrounding whitespace and comments. Multiline values and escaped table headers prevent an exact text selection and stop the plan. Unrelated bytes stay intact.

Declining an array entry can shift its siblings. Its receipt checks that the complete declared entry is absent from that parent array. Object-entry receipts check the exact key's absence. Hook event receipts search the declared event again, so reordered groups stay covered.

The services read real files through the existing contained file reader. They grant no permission, run no child command and write no file.

## Evidence

Unit tests use bytes in memory. Integration tests use temporary files and authored fixtures shaped like installer output. They cover exact rewrites, refusals, decline, passive drift, role switching, stale files and links. Malformed input, interruption and concurrent edits are the supported failure cases. Writers keep the existing Git or sibling-backup recovery behavior.

Run the focused managed selections with these arguments:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:OpenForgeSkipDevelopmentPublish=true -- --filter-namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Registration --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off
```

CI runs the complete managed and Native AOT gates. Local pre-commit checks include `npm run check:dotnet:whitespace` and `npm run check:dotnet:style`.

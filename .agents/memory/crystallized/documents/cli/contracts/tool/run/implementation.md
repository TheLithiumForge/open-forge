---
open-forge:
  description: Record Dispatch services, registration points and focused verification evidence
  tags: [Memory, Document, CLI, Tool, Run]
---

# Run implementation

Tool Run dispatch is a command service that resolves a declared workspace pin before handing its process to the root host.

For example, a host can start in a workspace subdirectory. Resolution finds the workspace above it and keeps that subdirectory as the child's working directory.

Status: Dispatch services complete. Root command registration pending.

## Scope

The lane changes Run source, focused tests, Run wording and Run documentation. The task's missing-tool exception also connects workspace Status and Doctor. Root command registration belongs to Writers. The [behavior contract](behavior.md) defines the required checks.

The implementation uses the existing settings, lock, store registry, contained-file reader and reviewed-definition reader. System.CommandLine parses prefixes until it assigns the tool ID. Child input never enters that parser. The process host uses an absolute executable with separate arguments. It inherits stdout and stderr. It inherits ordinary stdin and relays raw MCP input to detect parent pipe closure.

## Applicability

Run starts a local tool that can change the workspace through its own commands. Dispatch reads verification records and creates no store, lease or recovery state. The child handles its own effects. Git and the tool's own recovery cover those effects where available.

The checks cover malformed input, ordinary file edits, missing records, static links, process-start failures and pre-launch cancellation. The Architecture excludes a malicious process running as the same account. File size and UTC timestamp checks follow that limit.

Pure parsing, pin selection and consent-request tests use Unit evidence. Real workspace and store checks use Integration evidence. A self-hosted fixture process checks raw streams, argument positions, CWD, environment and child exits. CI runs the complete suites and native targets. Local checks use only changed tests, a build and both required .NET format gates.

## Registration points

Writers connects these calls:

- `ToolRunBinding.CreateSymbols` - add the Run command and its ID argument to the composed tree.
- `ToolRunRequestParser.Parse` - pass that tree, ID argument and complete process arguments before the normal report parser sees child options. Root options before the command path stay Open Forge input. A convenience overload accepts only tokens after Run. Prelude help and version return terminal input without a Run request. Nonempty `Diagnostics` gives pre-launch invalid input. Write those diagnostics to raw stderr with exit 4 before workspace resolution or consent.
- `ToolRunOperation.ResolveAsync` - resolve a verified launch or typed refusal.
- `ToolRunSession.ResolveAsync` - offer the normal Install service only for an interactive declared-missing refusal.
- `ToolRunInstallService` - accept the normal bare `ToolInstallRequest` and return its result, or null when the offer is declined. Bare Install restores exact declared pins against the lock. It doesn't replace a settings pin with a catalogue default. The current absent-service stub keeps the refusal.
- `ToolLaunchBinding.McpArguments` - record each fully resolved MCP argument vector after the Run ID during full verification. Exact forwarded-argument matches enable MCP disconnect handling. Omitted records keep ordinary CLI input behavior.
- `ToolDispatchProcessHost.RunAsync` - implement the raw `ToolDispatchProcess` capability.
- `ToolRunHost.RunAsync` - write pre-launch refusals to stderr and return the child's exit after launch.
- `ToolRunMissingReader.ReadAsync` - provide the same declared-missing fact for workspace Status and Doctor. Both commands pass their existing settings and lock observations. Their appenders add the required finding codes and Install action without repeating the control-file reads.

Two additive shared-model changes support dispatch. `ToolRunReady.Workspace` keeps the selected workspace for start-failure and pre-launch cancellation diagnostics. `ToolLaunchBinding.McpArguments` distinguishes verified MCP invocation lifetime from ordinary piped CLI input. The field is optional and omitted from JSON when absent.

New parsing records live in `Run.Models.Parsing`. New pin and workspace observation records live in `Run.Models.Resolution`. The shared request and resolution contracts keep their existing namespaces.

## Signals

The [signal contract](interface.md#signals) defines the shared-console behavior and parent-only stop policy. The root host uses managed APIs and keeps the existing console and process group. Console interrupts suppress Open Forge's default termination while it waits for the child. Parent-only termination or a matching MCP pipe closure starts the fixed one-second grace. A running child then receives `Process.Kill(entireProcessTree: true)`.

The Integration child registers interrupt handlers before reporting readiness. It records its PID and a descendant PID. Portable cases check MCP disconnect and ordinary CLI EOF. Linux and macOS add positive-PID signal delivery with the OS `kill` command. They check equal process groups, continued parent waiting after an interrupt, the child's received-signal bytes and exit, conventional parent-stop exits and descendant termination. Test guards bound broken tests without adding a product execution timeout.

## Evidence

The focused managed selection passes 45 Unit cases, with no failures or skips. It includes 41 Run cases and four Status and Doctor projection cases. The command builds the Unit project and its dependencies before execution. Two reviewed Unit snapshots show the missing-tool title, workspace-specific message and exact `open-forge tool install` action. Both complete and incomplete Doctor coverage use the same wording.

Reproduce Unit evidence with this command:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class 'OpenForge.Cli.Core.UnitTests.Commands.Tool.Run.*' '*ToolFindingAppenderTests' --minimum-expected-tests 45 --no-ansi --progress off
```

Integration passes 32 cases on Windows, with no failures or skips. A synthetic installed executable checks binary stdin, stdout and stderr, empty and quoted arguments, child `--help`, delimiters, CWD, environment and exact zero and nonzero exits. A real broken pipe checks continued parent-input reading after the child closes stdin. Process-level refusals check empty stdout, the correct exit, unchanged workspace and store bytes and absence of a child witness. Resolution cases cover workspace precedence, rejected executable paths and shell strings, undeclared IDs, missing records, mismatched pins, changed file sizes and UTC timestamps and pre-launch cancellation. Workspace Status and Doctor use the same missing-tool fact and the same control-file observation.

Reproduce Integration evidence with this command:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class 'OpenForge.Cli.IntegrationTests.Commands.Tool.Run.*' --minimum-expected-tests 32 --no-ansi --progress off
```

The third reviewed snapshot is `ReportsMissingWithoutInstall/missing-tool.stderr`. It contains only the `tool-run.declared-tool-missing` diagnostic and exact Install next step. It was accepted through one targeted serial snapshot-update run after reading the received file. The three snapshot files use LF.

The managed runs use SDK 10.0.101 and report `net10.0|x64`. The approved audit workaround was passed only to local build and test commands. Project, dependency and CI settings keep their existing values. Both `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` exit 0. Markdown formatting and `git diff --check` also pass.

Commit `f79549109` adds the selected workspace to `ToolRunReady`. Commit `d20e00410` adds MCP invocation vectors and their JSON compatibility tests. Root Tool command registration, normal Install service composition and population of verified MCP vectors stay with Writers. The registration points above are ready to connect.

## Limits

Unix signal cases are authored but aren't executed locally. Windows has no managed console-signal sender for these tests. Linux and macOS add four signal cases through the named theory data. CI owns their execution, the complete suites and native targets. No private Node runtime is added.

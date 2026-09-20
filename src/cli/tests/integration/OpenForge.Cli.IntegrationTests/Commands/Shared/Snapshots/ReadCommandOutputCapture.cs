using System.Runtime.CompilerServices;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

internal sealed class ReadCommandOutputCapture(
    string workspacePath,
    string? extensionSourcePath = null,
    string? recoveryBundlePath = null,
    string? interactiveAnswer = null)
{
    internal async Task MatchAsync(
        ReadOutputScenario scenario,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "")
    {
        foreach (var view in new[] { "compact", "expanded" })
        {
            foreach (var json in new[] { false, true })
            {
                var detail = view == "compact" ? CliPresentationDefinitions.Minimal : CliPresentationDefinitions.Standard;
                var capture = await CaptureAsync(scenario, detail, json, "read-output-locks");
                var formatName = json ? CommandOutputSnapshot.JsonContentNameSegment : ".";
                CommandOutputSnapshot.MatchSnapshot(
                    CommandOutputNormalization.Normalize(
                        capture.PrimaryContent,
                        workspacePath,
                        CliBuildVersion.InformationalVersion,
                        recoveryBundlePath,
                        extensionSourcePath,
                        normalizeTextPaths: !json),
                    $"{scenario.Situation}{formatName}{view}", sourceFile, testName);
            }
        }
    }

    internal async Task MatchDetailsAsync(
        ReadOutputScenario scenario,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "",
        IDictionary<string, string>? snapshotCollector = null)
    {
        var textPrimary = new Dictionary<CliDetail, (bool StandardOutput, string Content)>();
        var jsonPrimary = new Dictionary<CliDetail, string>();
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var detail in CommandOutputDetailVocabulary.All)
        {
            var detailName = CommandOutputDetailVocabulary.Name(detail);
            foreach (var json in new[] { false, true })
            {
                var capture = await CaptureAsync(scenario, detailName, json, "read-output-detail-locks");
                var formatName = json ? CommandOutputSnapshot.JsonContentNameSegment : ".";
                var hasSeparateDiagnostics = detail == CliDetail.Debug
                    && capture.DiagnosticContent.Length > 0;
                if (hasSeparateDiagnostics)
                {
                    snapshots.Add(
                        $"{scenario.Situation}{formatName}{detailName}.diagnostics",
                        CommandOutputNormalization.Normalize(
                            capture.DiagnosticContent,
                            workspacePath,
                            CliBuildVersion.InformationalVersion,
                            recoveryBundlePath,
                            extensionSourcePath,
                            normalizeTextPaths: !json));
                }
                else
                {
                    Assert.Empty(capture.SecondaryContent);
                }

                if (json && detail != CliDetail.Debug && scenario.DiagnosticId is { } diagnosticId)
                    Assert.Contains(diagnosticId, capture.PrimaryContent, StringComparison.Ordinal);
                if (detail == CliDetail.Debug && scenario.DiagnosticId is { } debugDiagnosticId)
                    Assert.Contains(debugDiagnosticId, capture.PrimaryContent + capture.StandardError, StringComparison.Ordinal);

                Assert.NotEmpty(capture.PrimaryContent);
                var normalized = CommandOutputNormalization.Normalize(
                    capture.PrimaryContent,
                    workspacePath,
                    CliBuildVersion.InformationalVersion,
                    recoveryBundlePath,
                    extensionSourcePath,
                    normalizeTextPaths: !json);
                var snapshotContent = json && !scenario.ShellDiagnostic
                    ? CommandOutputSnapshotFormatting.PrettyPrintJson(normalized)
                    : normalized;
                snapshots.Add($"{scenario.Situation}{formatName}{detailName}", snapshotContent);
                if (json)
                {
                    // Shell diagnostics remain plain stderr even when --format json; never parse them as JSON.
                    if (!scenario.ShellDiagnostic)
                        jsonPrimary[detail] = normalized;
                }
                else
                {
                    textPrimary[detail] = (capture.PrimaryIsStandardOutput, capture.PrimaryContent);
                }
            }
        }

        if (!scenario.ShellDiagnostic
            && textPrimary[CliDetail.Full].StandardOutput
            && textPrimary[CliDetail.Debug].StandardOutput)
        {
            Assert.Equal(textPrimary[CliDetail.Full].Content, textPrimary[CliDetail.Debug].Content);
        }

        if (!scenario.ShellDiagnostic
            && !textPrimary[CliDetail.Full].StandardOutput
            && !textPrimary[CliDetail.Debug].StandardOutput)
        {
            var fullContent = textPrimary[CliDetail.Full].Content;
            var debugContent = textPrimary[CliDetail.Debug].Content;
            Assert.True(
                debugContent.StartsWith(fullContent, StringComparison.Ordinal),
                $"Debug text changed the primary output for {scenario.Situation}.");
            var diagnostics = debugContent[fullContent.Length..].TrimEnd('\r', '\n');
            if (diagnostics.Length > 0)
            {
                snapshots[$"{scenario.Situation}.debug"] = CommandOutputNormalization.Normalize(
                    fullContent,
                    workspacePath,
                    CliBuildVersion.InformationalVersion,
                    recoveryBundlePath,
                    extensionSourcePath,
                    normalizeTextPaths: true);
                snapshots[$"{scenario.Situation}.debug.diagnostics"] = CommandOutputNormalization.Normalize(
                    diagnostics,
                    workspacePath,
                    CliBuildVersion.InformationalVersion,
                    recoveryBundlePath,
                    extensionSourcePath,
                    normalizeTextPaths: true);
                textPrimary[CliDetail.Debug] = (false, fullContent);
            }
        }

        if (!scenario.ShellDiagnostic)
        {
            CommandOutputDetailComparison.AssertFullDebugJson(jsonPrimary[CliDetail.Full], jsonPrimary[CliDetail.Debug]);
        }

        if (snapshotCollector is null)
        {
            CommandOutputSnapshot.MatchDetailSnapshot(snapshots, sourceFile, testName);
        }
        else
        {
            foreach (var snapshot in snapshots)
                snapshotCollector.Add(snapshot.Key, snapshot.Value);
        }
    }

    private async Task<CommandOutputCaptureRun> CaptureAsync(
        ReadOutputScenario scenario,
        string detail,
        bool json,
        string lockStoreName)
    {
        using var input = new StringReader(interactiveAnswer ?? string.Empty);
        using var output = new StringWriter();
        using var error = new StringWriter();
        using var prompts = new StringWriter();
        using var lockStore = WorkspaceLockTestStore.Create(lockStoreName);
        _ = lockStore.Track(new CliWorkspace(workspacePath, workspacePath, CliWorkspaceSelectionMethod.CurrentDirectory));
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", CliBuildVersion.InformationalVersion),
            new CliCompositionInputs
            {
                StandardInput = input,
                PromptOutput = prompts,
                StandardInputRedirected = interactiveAnswer is null,
                PromptOutputRedirected = interactiveAnswer is null,
                LockStoreRoot = lockStore.StoreRoot,
            });
        string[] format = json ? ["--format", "json"] : [];
        var completion = await application.RunAsync(
            [.. scenario.Arguments, "--detail", detail, .. format],
            new CliProcessEnvironment(workspacePath),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);
        Assert.Equal(scenario.ExitCode, completion.ExitCode);
        var promptOutput = prompts.ToString();
        Assert.Empty(promptOutput);

        var standardOutput = output.ToString();
        var standardError = error.ToString();
        var primaryIsStandardOutput = !scenario.ShellDiagnostic
            && (json || scenario.ExitCode is 0 or 2 or 3);

        Assert.Equal(
            primaryIsStandardOutput ? CliOutputTarget.StandardOutput : CliOutputTarget.StandardError,
            completion.PrimaryOutputTarget);
        var primaryContent = primaryIsStandardOutput ? standardOutput : standardError;
        var secondaryContent = primaryIsStandardOutput ? standardError : standardOutput;
        var hasSeparateDiagnostics = detail == CliPresentationDefinitions.Debug
            && primaryIsStandardOutput
            && standardError.Length > 0;
        if (!hasSeparateDiagnostics)
            Assert.Empty(secondaryContent);
        if (json && scenario.DiagnosticId is { } diagnosticId)
            Assert.Contains(diagnosticId, primaryContent, StringComparison.Ordinal);

        return new CommandOutputCaptureRun(
            completion,
            standardOutput,
            standardError,
            promptOutput,
            primaryIsStandardOutput,
            null,
            primaryIsStandardOutput ? standardError : null);
    }
}

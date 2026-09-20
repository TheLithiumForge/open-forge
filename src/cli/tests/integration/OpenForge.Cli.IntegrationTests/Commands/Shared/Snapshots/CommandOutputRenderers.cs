using System.Runtime.CompilerServices;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

internal sealed class CommandOutputRenderers<TResult>(
    Func<CliPresentationRequest<TResult>, CliRenderedOutput> render,
    Func<string, string>? normalizeText = null,
    Func<string, string>? normalizeJson = null)
    where TResult : ICliCommandResult
{
    internal static CommandOutputRenderers<TResult> From<TData>(
        CliReportRendering<TResult, TData> rendering,
        Func<string, string>? normalizeText = null,
        Func<string, string>? normalizeJson = null)
        where TData : class
        => new(
            request => CliRenderingStage.Render(request, rendering),
            normalizeText,
            normalizeJson);

    internal static string Render<TData>(
        CliPresentationRequest<TResult> request,
        CliReportRendering<TResult, TData> rendering,
        Func<string, string>? normalize = null)
        where TData : class
    {
        var output = CliRenderingStage.Render(request, rendering);
        ValidateOutput(request, output);
        var content = output.PrimaryContent;
        return normalize is null ? content : normalize(content);
    }

    internal void Match(
        TResult result,
        string situation,
        string? recoveryBundlePath = null,
        string? extensionSourcePath = null,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "")
    {
        foreach (var view in new[] { CliDetail.Minimal, CliDetail.Standard })
        {
            var viewName = view == CliDetail.Minimal ? "compact" : "expanded";
            var textRequest = new CliPresentationRequest<TResult>(result, new(CliFormat.Text, view, null));
            var jsonRequest = new CliPresentationRequest<TResult>(result, new(CliFormat.Json, view, null));
            var workspacePath = result.Workspace?.PhysicalRoot;
            CommandOutputSnapshot.MatchSnapshot(
                CommandOutputNormalization.Normalize(
                    RenderPrimary(textRequest, normalizeText),
                    workspacePath,
                    CliBuildVersion.InformationalVersion,
                    recoveryBundlePath,
                    extensionSourcePath,
                    normalizeTextPaths: true),
                $"{situation}.{viewName}", sourceFile, testName);
            CommandOutputSnapshot.MatchSnapshot(
                CommandOutputNormalization.Normalize(
                    RenderPrimary(jsonRequest, normalizeJson),
                    workspacePath,
                    CliBuildVersion.InformationalVersion,
                    recoveryBundlePath,
                    extensionSourcePath),
                $"{situation}{CommandOutputSnapshot.JsonContentNameSegment}{viewName}", sourceFile, testName);
        }
    }

    internal void MatchDetails(
        TResult result,
        string situation,
        string? recoveryBundlePath = null,
        string? extensionSourcePath = null,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "",
        IDictionary<string, string>? snapshotCollector = null)
    {
        var textOutputs = new Dictionary<CliDetail, CliRenderedOutput>();
        var jsonOutputs = new Dictionary<CliDetail, CliRenderedOutput>();
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var detail in CommandOutputDetailVocabulary.All)
        {
            var detailName = CommandOutputDetailVocabulary.Name(detail);
            var textRequest = new CliPresentationRequest<TResult>(result, new(CliFormat.Text, detail, null));
            var jsonRequest = new CliPresentationRequest<TResult>(result, new(CliFormat.Json, detail, null));
            var textRendered = RenderOutput(textRequest);
            var jsonRendered = RenderOutput(jsonRequest);
            var workspacePath = result.Workspace?.PhysicalRoot;

            if (detail != CliDetail.Debug)
            {
                Assert.Null(textRendered.DiagnosticContent);
                Assert.Null(jsonRendered.DiagnosticContent);
            }

            var textContent = normalizeText is null ? textRendered.PrimaryContent : normalizeText(textRendered.PrimaryContent);
            snapshots.Add(
                $"{situation}.{detailName}",
                CommandOutputNormalization.Normalize(
                    textContent,
                    workspacePath,
                    CliBuildVersion.InformationalVersion,
                    recoveryBundlePath,
                    extensionSourcePath,
                    normalizeTextPaths: true));
            if (textRendered.DiagnosticContent is { } textDiagnostics)
            {
                snapshots.Add(
                    $"{situation}.{detailName}.diagnostics",
                    CommandOutputNormalization.Normalize(
                        textDiagnostics,
                        workspacePath,
                        CliBuildVersion.InformationalVersion,
                        recoveryBundlePath,
                        extensionSourcePath,
                        normalizeTextPaths: true));
            }

            var jsonContent = normalizeJson is null ? jsonRendered.PrimaryContent : normalizeJson(jsonRendered.PrimaryContent);
            var normalizedJson = CommandOutputNormalization.Normalize(jsonContent, workspacePath, CliBuildVersion.InformationalVersion, recoveryBundlePath, extensionSourcePath);
            snapshots.Add(
                $"{situation}{CommandOutputSnapshot.JsonContentNameSegment}{detailName}",
                CommandOutputSnapshotFormatting.PrettyPrintJson(normalizedJson));
            if (jsonRendered.DiagnosticContent is { } jsonDiagnostics)
            {
                snapshots.Add(
                    $"{situation}{CommandOutputSnapshot.JsonContentNameSegment}{detailName}.diagnostics",
                    CommandOutputNormalization.Normalize(
                        jsonDiagnostics,
                        workspacePath,
                        CliBuildVersion.InformationalVersion,
                        recoveryBundlePath,
                        extensionSourcePath));
            }

            textOutputs[detail] = textRendered;
            jsonOutputs[detail] = jsonRendered;
        }

        var fullText = textOutputs[CliDetail.Full];
        var debugText = textOutputs[CliDetail.Debug];
        Assert.Equal(fullText.PrimaryTarget, debugText.PrimaryTarget);
        Assert.Equal(fullText.PrimaryContent, debugText.PrimaryContent);

        var fullJson = jsonOutputs[CliDetail.Full];
        var debugJson = jsonOutputs[CliDetail.Debug];
        CommandOutputDetailComparison.AssertFullDebugJson(fullJson.PrimaryContent, debugJson.PrimaryContent);
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

    private CliRenderedOutput RenderOutput(CliPresentationRequest<TResult> request)
    {
        var output = render(request);
        ValidateOutput(request, output);
        return output;
    }

    private string RenderPrimary(CliPresentationRequest<TResult> request, Func<string, string>? normalize)
    {
        var output = RenderOutput(request);
        var content = output.PrimaryContent;
        return normalize is null ? content : normalize(content);
    }

    private static void ValidateOutput(
        CliPresentationRequest<TResult> request,
        CliRenderedOutput output)
    {
        var expectsStdout = request.Presentation.Format == CliFormat.Json
            || request.Result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention or CliSemanticStatus.Incomplete;
        Assert.Equal(expectsStdout ? CliOutputTarget.StandardOutput : CliOutputTarget.StandardError, output.PrimaryTarget);
        Assert.Equal(request.Result.Status, output.Status);
    }
}

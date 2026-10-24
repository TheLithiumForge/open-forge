using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

[Trait("Boundary", "Output"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
public sealed class RouteListMainRootPresentationTests
{
    private const string MainRoot = "knowledge/team/context";
    private const string LoaderPath = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;

    [Fact]
    public async Task LoaderDiagnosticsNameTheConfiguredPath()
    {
        using var workspace = TemporaryWorkspace.Create(nameof(LoaderDiagnosticsNameTheConfiguredPath));
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}"}""");
        workspace.CreateDirectory(MainRoot);
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        var capture = new ReadCommandOutputCapture(workspace.Path);
        await capture.MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "missing",
            Arguments = [RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name],
            ExitCode = CliStatusDefinitions.Read(CliSemanticStatus.Incomplete).Disposition.ExitCode,
        }, snapshotCollector: captures);

        workspace.WriteText(LoaderPath, $"# Loader\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n");
        await capture.MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "malformed",
            Arguments = [RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name],
            ExitCode = CliStatusDefinitions.Read(CliSemanticStatus.Blocked).Disposition.ExitCode,
        }, snapshotCollector: captures);

        CommandOutputSnapshot.MatchDetailSnapshot(captures
            .Where(pair => !pair.Key.Contains(CommandOutputSnapshot.JsonContentNameSegment, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
    }
}

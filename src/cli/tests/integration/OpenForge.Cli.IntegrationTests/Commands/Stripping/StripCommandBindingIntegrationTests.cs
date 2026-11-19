using OpenForge.Cli.Core.Commands.Export;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Uninstall;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Stripping;

// Binds export and uninstall from process arguments through the composed CLI, so a binder change fails here. Both
// commands declare the shared selection flags.
public sealed class StripCommandBindingIntegrationTests
{
    private const string Folder = WorkspaceLayout.DefaultMainRoot + "/docs";
    private const string Moved = Folder + "/game/rules.md";
    private const string Flattened = Folder + "/rules.md";
    private const string Body = "# Rules\n";
    private const string Document = "---\nopen-forge:\n  description: Rules\n  tags: [Docs]\n---\n" + Body;
    private const string CurrentDirectoryPrefix = "./";
    private const string Inside = "copy";
    private const string Destination = "<destination>";

    [Fact(DisplayName = "Export binds its destination, comma and repeated parts, flatten and dry-run from its arguments and keeps the project"), Trait("Feature", "export"), Trait("Evidence", "Integration")]
    public async Task ExportBindsItsArguments()
    {
        using var workspace = StripIntegrationWorkspace.Create("export-binding");
        workspace.Write(Moved, Document);
        var parts = $"{StrippingDefinitions.PartName(StripPart.Metadata)}{StrippingDefinitions.PartSeparator}{StrippingDefinitions.PartName(StripPart.Links)}";
        var before = workspace.SnapshotHashes();

        var preview = await workspace.RunAsync([ExportDefinitions.CommandIdentity, workspace.Destination, StrippingDefinitions.StripOption, parts,
            StrippingDefinitions.FlattenOption, Folder, StrippingDefinitions.DryRunOption]);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Empty(workspace.ExportFiles());
        var applied = await workspace.RunAsync([ExportDefinitions.CommandIdentity, workspace.Destination,
            StrippingDefinitions.StripOption, StrippingDefinitions.PartName(StripPart.Metadata),
            StrippingDefinitions.StripOption, StrippingDefinitions.PartName(StripPart.Entrypoints),
            StrippingDefinitions.FlattenOption, CurrentDirectoryPrefix + Folder]);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal([Flattened], workspace.ExportFiles());
        Assert.Equal(Body, workspace.ReadExport(Flattened));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Export rejects an unknown part through its binder and keeps the text and JSON report"), Trait("Feature", "export"), Trait("Evidence", "Integration")]
    public async Task ExportRejectsUnknownPart()
    {
        using var workspace = StripIntegrationWorkspace.Create("export-binding-invalid");
        var unknown = StrippingDefinitions.PartName(StripPart.Links).ToUpperInvariant();

        CommandOutputSnapshot.MatchDetailSnapshot(await CaptureAsync(workspace, "unknown-part",
            [ExportDefinitions.CommandIdentity, workspace.Destination, StrippingDefinitions.StripOption, unknown]));
    }

    // A destination that is not empty is refused before anything is planned or written.
    [Fact(DisplayName = "Export refuses a destination that is not empty and keeps the text and JSON report"), Trait("Feature", "export"), Trait("Evidence", "Integration")]
    public async Task ExportRefusesDestinationThatIsNotEmpty()
    {
        using var workspace = StripIntegrationWorkspace.Create("export-destination-not-empty");
        workspace.Write(Moved, Document);
        File.WriteAllText(Path.Combine(workspace.Destination, Path.GetFileName(Moved)), Body);
        var before = workspace.SnapshotHashes();

        CommandOutputSnapshot.MatchDetailSnapshot(await CaptureAsync(workspace, "destination-not-empty",
            [ExportDefinitions.CommandIdentity, workspace.Destination]));
        Assert.Equal([Path.GetFileName(Moved)], workspace.ExportFiles());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    // The project folder itself, a folder that holds it and a file are never destinations.
    [Theory(DisplayName = "Export refuses the project, a folder that holds it and a file as its destination"), Trait("Feature", "export"), Trait("Evidence", "Integration")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ExportRefusesDestination(int refused)
    {
        using var workspace = StripIntegrationWorkspace.Create("export-destination-refused");
        workspace.Write(Moved, Document);
        string[] destinations = [workspace.Path, Path.GetDirectoryName(workspace.Path)!, workspace.Combine(Moved)];
        var before = workspace.SnapshotHashes();

        var completion = await workspace.RunAsync([ExportDefinitions.CommandIdentity, destinations[refused]]);

        Assert.Equal(CliSemanticStatus.Invalid, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    // A destination inside the project is allowed, and the copy leaves the destination itself out.
    [Fact(DisplayName = "Export writes to a new folder inside the project and leaves that folder out of the copy"), Trait("Feature", "export"), Trait("Evidence", "Integration")]
    public async Task ExportInsideTheProject()
    {
        using var workspace = StripIntegrationWorkspace.Create("export-inside");
        workspace.Write(Moved, Document);
        var before = workspace.SnapshotHashes();
        var destination = workspace.Combine(Inside);
        try
        {
            var completion = await workspace.RunAsync([ExportDefinitions.CommandIdentity, destination]);

            Assert.Equal(CliSemanticStatus.Complete, completion.Status);
            Assert.Equal(workspace.ReadBytes(Moved), File.ReadAllBytes(Path.Combine(destination, Moved)));
            Assert.False(Directory.Exists(Path.Combine(destination, Inside)));
        }
        finally
        {
            if (Directory.Exists(destination)) Directory.Delete(destination, recursive: true);
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    // The published uninstall journey also binds these flags. This Integration case binds every uninstall option in process.
    [Fact(DisplayName = "Uninstall binds repeated strip parts, a named edited file and dry-run from its arguments"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task UninstallBindsItsArguments()
    {
        using var workspace = await OpenForge.Cli.IntegrationTests.Commands.Uninstall.UninstallIntegrationTests.InstalledAsync("uninstall-binding");
        var directives = WorkspaceLayout.DefaultMainRoot + "/directives/_directives.md";
        var before = workspace.SnapshotHashes();

        var preview = await workspace.RunAsync([UninstallDefinitions.CommandIdentity,
            StrippingDefinitions.StripOption, StrippingDefinitions.PartName(StripPart.Entrypoints),
            StrippingDefinitions.StripOption, StrippingDefinitions.PartName(StripPart.Links),
            StrippingDefinitions.RemoveEditedOption, CurrentDirectoryPrefix + directives, StrippingDefinitions.DryRunOption]);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        var applied = await workspace.RunAsync([UninstallDefinitions.CommandIdentity,
            StrippingDefinitions.StripOption, StrippingDefinitions.PartName(StripPart.Metadata),
            StrippingDefinitions.RemoveEditedOption, directives, UninstallDefinitions.Automatic.Name]);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.False(workspace.Exists(directives));
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName));
    }

    // Every detail level in text and JSON, with the workspace and the destination replaced by placeholders.
    private static async Task<Dictionary<string, string>> CaptureAsync(StripIntegrationWorkspace workspace, string name, string[] arguments)
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var detail in CommandOutputDetailVocabulary.All)
        {
            var detailName = CommandOutputDetailVocabulary.Name(detail);
            var text = await workspace.RunWithOutputAsync([.. arguments, CliSyntaxDefinitions.Detail.Name, detailName]);
            var json = await workspace.RunWithOutputAsync([.. arguments, CliSyntaxDefinitions.Detail.Name, detailName,
                CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat]);
            Assert.Equal(CliSemanticStatus.Invalid, text.Completion.Status);
            Assert.Equal(CliSemanticStatus.Invalid, json.Completion.Status);
            snapshots.Add($"{name}.{detailName}", Normalize(workspace, text.Output + text.Error));
            snapshots.Add($"{name}{CommandOutputSnapshot.JsonContentNameSegment}{detailName}", Normalize(workspace, json.Output + json.Error));
        }

        return snapshots;
    }

    private static string Normalize(StripIntegrationWorkspace workspace, string output)
        => CommandOutputNormalization.Normalize(
            CommandOutputNormalization.ReplaceDelimitedPath(output, workspace.Destination, Destination),
            workspace.Path, CliBuildVersion.InformationalVersion, normalizeTextPaths: true);
}

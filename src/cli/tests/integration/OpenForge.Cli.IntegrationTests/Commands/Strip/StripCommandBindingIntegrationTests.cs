using OpenForge.Cli.Core.Commands.Strip;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Uninstall;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Strip;

// Binds strip and uninstall from process arguments through the composed CLI, so a binder change fails here.
public sealed class StripCommandBindingIntegrationTests
{
    private const string Folder = WorkspaceLayout.DefaultMainRoot + "/docs";
    private const string Moved = Folder + "/game/rules.md";
    private const string Flattened = Folder + "/rules.md";
    private const string Body = "# Rules\n";
    private const string Document = "---\nopen-forge:\n  description: Rules\n  tags: [Docs]\n---\n" + Body;
    private const string CurrentDirectoryPrefix = "./";

    [Fact(DisplayName = "Strip binds comma and repeated parts, flatten and dry-run from its arguments"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task StripBindsItsArguments()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-binding");
        workspace.Write(Moved, Document);
        var parts = $"{StrippingDefinitions.PartName(StripPart.Metadata)}{StrippingDefinitions.PartSeparator}{StrippingDefinitions.PartName(StripPart.Links)}";
        var before = workspace.SnapshotHashes();

        var preview = await workspace.RunAsync([StripDefinitions.CommandIdentity, parts, StripDefinitions.Flatten.Name, Folder, StripDefinitions.DryRun.Name]);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        var applied = await workspace.RunAsync([StripDefinitions.CommandIdentity, StrippingDefinitions.PartName(StripPart.Metadata),
            StrippingDefinitions.PartName(StripPart.Entrypoints), StripDefinitions.Flatten.Name, CurrentDirectoryPrefix + Folder, StripDefinitions.Automatic.Name]);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.False(workspace.Exists(Moved));
        Assert.Equal(Body, workspace.Read(Flattened));
    }

    [Fact(DisplayName = "Strip rejects an unknown part through its binder and keeps the text and JSON report"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task StripRejectsUnknownPart()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-binding-invalid");
        var unknown = StrippingDefinitions.PartName(StripPart.Links).ToUpperInvariant();

        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var detail in CommandOutputDetailVocabulary.All)
        {
            var detailName = CommandOutputDetailVocabulary.Name(detail);
            var text = await workspace.RunWithOutputAsync([StripDefinitions.CommandIdentity, unknown, CliSyntaxDefinitions.Detail.Name, detailName]);
            var json = await workspace.RunWithOutputAsync([StripDefinitions.CommandIdentity, unknown, CliSyntaxDefinitions.Detail.Name, detailName,
                CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat]);
            Assert.Equal(CliSemanticStatus.Invalid, text.Completion.Status);
            Assert.Equal(CliSemanticStatus.Invalid, json.Completion.Status);
            snapshots.Add($"unknown-part.{detailName}", Normalize(workspace, text.Output + text.Error));
            snapshots.Add($"unknown-part{CommandOutputSnapshot.JsonContentNameSegment}{detailName}", Normalize(workspace, json.Output + json.Error));
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    // The published uninstall journey also binds these flags. This Integration case binds every uninstall option in process.
    [Fact(DisplayName = "Uninstall binds repeated strip parts, a named edited file and dry-run from its arguments"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task UninstallBindsItsArguments()
    {
        using var workspace = await OpenForge.Cli.IntegrationTests.Commands.Uninstall.UninstallIntegrationTests.InstalledAsync("uninstall-binding");
        var directives = WorkspaceLayout.DefaultMainRoot + "/directives/_directives.md";
        var before = workspace.SnapshotHashes();

        var preview = await workspace.RunAsync([UninstallDefinitions.CommandIdentity,
            UninstallDefinitions.Strip.Name, StrippingDefinitions.PartName(StripPart.Entrypoints),
            UninstallDefinitions.Strip.Name, StrippingDefinitions.PartName(StripPart.Links),
            UninstallDefinitions.RemoveEdited.Name, CurrentDirectoryPrefix + directives, UninstallDefinitions.DryRun.Name]);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        var applied = await workspace.RunAsync([UninstallDefinitions.CommandIdentity,
            UninstallDefinitions.Strip.Name, StrippingDefinitions.PartName(StripPart.Metadata),
            UninstallDefinitions.RemoveEdited.Name, directives, UninstallDefinitions.Automatic.Name]);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.False(workspace.Exists(directives));
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName));
    }

    private static string Normalize(StripIntegrationWorkspace workspace, string output)
        => CommandOutputNormalization.Normalize(output, workspace.Path, CliBuildVersion.InformationalVersion, normalizeTextPaths: true);
}

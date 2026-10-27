using System.Text.Json;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

[Trait("Boundary", "Host"), Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
public sealed class RouteRemoveOutsideInvalidReferenceIntegrationTests
{
    public static IEnumerable<object[]> InvalidReferences()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteRemoveOutsideWorkspace.CustomRoot })
            foreach (var registered in new[] { false, true })
                foreach (var empty in new[] { false, true })
                    foreach (var rootCommand in new[] { false, true })
                        yield return [root, registered, empty, rootCommand];
    }

    [Theory, MemberData(nameof(InvalidReferences))]
    public async Task InvalidExactReferencesKeepTypedInputRefusal(string root, bool registered, bool empty, bool rootCommand)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root);
        fixture.SeedCategory();
        if (!registered) fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings(routes: []));
        var reference = SourceWorkspaceRelativePath.LiteralReferencePrefix
            + (empty ? string.Empty : RouteRemoveOutsideWorkspace.DirectoryPath + "/../old.md");
        var parsed = SourceReferenceParser.Parse(reference);
        Assert.Equal(SourceReferenceParseState.Invalid, parsed.State);
        Assert.Equal(SourceReferenceKind.SourcePath, parsed.Kind);
        var before = fixture.Inner.SnapshotHashes();
        var directories = Directories(fixture);
        var result = await RouteRemoveOperationFactory.Create(fixture.Inner.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(fixture.Inner.Workspace, reference, RouteRemoveMode.Apply, automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Empty(result.Effects);
        AssertUnchanged(fixture, before, directories);

        using var output = new StringWriter();
        using var error = new StringWriter();
        string[] command = rootCommand
            ? [RemoveDefinitions.CommandIdentity, reference, RemoveDefinitions.KindOption, RemoveDefinitions.RouteKind]
            : [RouteDefinitions.RouteGroup.Name, RouteRemoveDefinitions.RemoveCommand.Name, reference];
        var completion = await fixture.Inner.RunAsync([.. command, RouteRemoveDefinitions.Automatic.Name,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Empty(report.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
        AssertUnchanged(fixture, before, directories);
        Assert.Equal(CliSemanticStatus.Invalid, completion.Status);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteRemoveFindingCode.InvalidSource, Assert.Single(result.Findings).Code);
        Assert.Equal(RouteRemoveSourceSelection.BasePath, result.Source.SelectedBy);
        Assert.Equal(RouteRemoveDefinitions.ReadMachineName(RouteRemoveFindingCode.InvalidSource),
            Assert.Single(report.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray())
                .GetProperty(CliJsonFields.Code).GetString());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot, false)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot, true)]
    public async Task ValidExactLeafAndCategoryKeepRootRoutePreview(string root, bool category)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root, listed: false, compatibility: true);
        if (category) fixture.SeedCategory();
        var path = category ? WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.Category)
            : RouteRemoveOutsideWorkspace.Leaf;
        var before = fixture.Inner.SnapshotHashes();
        var directories = Directories(fixture);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + path, RemoveDefinitions.KindOption, RemoveDefinitions.RouteKind,
            RouteRemoveDefinitions.DryRun.Name, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        using var report = JsonDocument.Parse(output.ToString());
        Assert.NotEmpty(report.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
        Assert.Empty(report.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray());
        AssertUnchanged(fixture, before, directories);
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task PrimaryAndNativeIdsKeepRootRoutePreview(bool native)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var nativeAnchor = WorkspaceRoutePath.CanonicalEntrypointPath(WorkspaceLayout.NativeSkillsPath);
        fixture.Inner.WriteText(nativeAnchor, RouteRemoveOutsideWorkspace.Entry(MarkdownEntriesSectionReader.EmptyEntry));
        fixture.Inner.WriteText(fixture.Layout.LoaderPath, RouteRemoveOutsideWorkspace.Entry(
            $"- [Primary](guidance/_guidance.md) - #Fixture\n- [Native]({Path.GetRelativePath(fixture.Layout.MainRoot, nativeAnchor).Replace('\\', '/')}) - #Fixture\n"));
        var before = fixture.Inner.SnapshotHashes();
        var directories = Directories(fixture);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity,
            native ? WorkspaceLayoutNames.SkillsDirectoryName : RouteRemoveIntegrationWorkspace.LeafId,
            RemoveDefinitions.KindOption, RemoveDefinitions.RouteKind, RouteRemoveDefinitions.DryRun.Name,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        using var report = JsonDocument.Parse(output.ToString());
        Assert.NotEmpty(report.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
        Assert.Empty(report.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray());
        AssertUnchanged(fixture, before, directories);
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot, false)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot, true)]
    public async Task ProtectedAnchorsKeepRootRouteRefusal(string root, bool compatibility)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root, compatibility: compatibility);
        var before = fixture.Inner.SnapshotHashes();
        var directories = Directories(fixture);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor,
            RemoveDefinitions.KindOption, RemoveDefinitions.RouteKind, RouteRemoveDefinitions.Automatic.Name,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal(RouteRemoveDefinitions.ReadMachineName(RouteRemoveFindingCode.ProtectedTarget),
            Assert.Single(report.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray())
                .GetProperty(CliJsonFields.Code).GetString());
        Assert.Empty(report.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
        AssertUnchanged(fixture, before, directories);
    }

    private static string[] Directories(RouteRemoveOutsideWorkspace fixture)
        => Directory.EnumerateDirectories(fixture.Inner.Path, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(fixture.Inner.Path, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal).ToArray();

    private static void AssertUnchanged(RouteRemoveOutsideWorkspace fixture,
        IReadOnlyDictionary<string, string> before, string[] directories)
    {
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        Assert.Equal(directories, Directories(fixture));
        fixture.Inner.AssertNoLockInfrastructure();
    }
}

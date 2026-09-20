using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListEncodedWhitespaceIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "An admitted encoded-space Loader declaration preserves known Route List rows as incomplete"),
        Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EncodedSpaceDeclarationRetainsKnownRootAndMissingDestinationFinding()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        const string rootDeclaration = "- [Root](root/_root.md) - #Root";
        const string blankDeclaration = "- [Blank](%20) - #Blank";
        var loaderContents = GeneratedLoaderDocumentBuilder.Build($"{rootDeclaration}\n{blankDeclaration}");
        workspace.Write(".agents/loader.md", loaderContents);
        workspace.Write(
            ".agents/root/_root.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Known root", "Root"));
        var before = workspace.SnapshotHashes();

        var inventory = await workspace.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(RouteListInventoryState.Complete, inventory.State);
        Assert.Empty(inventory.Findings);
        Assert.Collection(
            inventory.Sources,
            source =>
            {
                Assert.Equal(RouteListSourceKind.Loader, source.Kind);
                Assert.Equal(".agents/loader.md", source.Source.CanonicalPath);
            },
            source =>
            {
                Assert.Equal(RouteListSourceKind.Entrypoint, source.Kind);
                Assert.Equal("root", source.Source.Id);
                Assert.Equal(".agents/root/_root.md", source.Source.CanonicalPath);
            });
        var loader = Assert.Single(
            inventory.ProjectionBuildResult.Projections,
            projection => projection.LogicalSource.Identity.CanonicalBasePath == ".agents/loader.md");
        var read = Assert.IsType<FileReadResult<string>>(loader.BaseRead.Read);
        Assert.Equal(FileReadState.Complete, read.State);
        Assert.Equal(loaderContents, read.Value);
        var document = new MarkdownDocumentParser().Parse(Assert.IsType<string>(read.Value));
        Assert.Equal(MarkdownGeneratedRegionState.Complete, document.GeneratedRegion.State);
        var contentSpan = Assert.IsType<MarkdownTextSpan>(document.GeneratedRegion.ContentSpan);
        var declarations = document.Source[contentSpan.Start..contentSpan.End]
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal([rootDeclaration, blankDeclaration], declarations);
        Assert.True(SourceLoaderDeclarationParser.TryParse(declarations[0], out var rootDestination, out var rootCause));
        Assert.Equal("root/_root.md", rootDestination);
        Assert.Null(rootCause);
        Assert.True(SourceLoaderDeclarationParser.TryParse(declarations[1], out var blankDestination, out var blankCause));
        Assert.Equal("%20", blankDestination);
        Assert.Null(blankCause);
        Assert.Equal(before, workspace.SnapshotHashes());

        var result = await CliHostCapture.RunAsync(["route", "list", "--format", "json"], workspace.Path);

        Assert.Equal(before, workspace.SnapshotHashes());
        using var output = JsonDocument.Parse(result.Output);
        Assert.Equal("incomplete", output.RootElement.GetProperty("status").GetString());
        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        var facts = output.RootElement.GetProperty("data");
        var row = Assert.Single(facts.GetProperty("rows").EnumerateArray());
        Assert.Equal("root", row.GetProperty("id").GetString());
        Assert.Equal(".agents/root/_root.md", row.GetProperty("path").GetString());
        var finding = Assert.Single(output.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("route-list.loader-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal("%20", finding.GetProperty("subject").GetProperty("id").GetString());
    }
}

using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;

using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions;

public sealed class ExtensionSourceIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Real Extension catalogue resolves exact manifests dependencies and stable ordering"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public async Task RealCatalogueResolvesManifestsDependenciesAndOrdering()
    {
        using var workspace = TemporaryWorkspace.Create("extension-target");
        using var source = TemporaryWorkspace.Create("extension-source");
        WriteManifest(source, "z-toolkit", "z-toolkit", ["a-base"]);
        WriteManifest(source, "a-base", "a-base", []);
        var beforeWorkspace = workspace.SnapshotHashes();
        var beforeSource = source.SnapshotHashes();
        var reader = new ExtensionSourceReader(new PhysicalPathResolver());

        var result = await reader.ReadAsync(Workspace(workspace), source.Path, CancellationToken.None);

        Assert.Equal(ExtensionSourceReadState.Complete, result.State);
        Assert.Equal(ExtensionSourceKind.Catalogue, result.Kind);
        Assert.Equal(["a-base", "z-toolkit"], result.Packages.Select(package => package.Id));
        Assert.Equal(["a-base"], result.Packages[1].Dependencies);
        Assert.Equal(beforeWorkspace, workspace.SnapshotHashes());
        Assert.Equal(beforeSource, source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Real Extension source rejects duplicate IDs incomplete closure and workspace overlap"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public async Task RealSourceRejectsUnsafeAndIncompleteUniverses()
    {
        using var workspace = TemporaryWorkspace.Create("extension-target");
        using var duplicate = TemporaryWorkspace.Create("extension-duplicate");
        WriteManifest(duplicate, "first", "same-id", []);
        WriteManifest(duplicate, "second", "same-id", []);
        using var incomplete = TemporaryWorkspace.Create("extension-incomplete");
        WriteManifest(incomplete, "toolkit", "toolkit", ["missing"]);
        var reader = new ExtensionSourceReader(new PhysicalPathResolver());

        var duplicateResult = await reader.ReadAsync(Workspace(workspace), duplicate.Path, CancellationToken.None);
        var incompleteResult = await reader.ReadAsync(Workspace(workspace), incomplete.Path, CancellationToken.None);
        var overlapResult = await reader.ReadAsync(Workspace(workspace), workspace.CreateDirectory("catalogue"), CancellationToken.None);

        Assert.Equal(ExtensionSourceReadState.Blocked, duplicateResult.State);
        Assert.Equal(ExtensionSourceReadState.Invalid, incompleteResult.State);
        Assert.Equal(ExtensionSourceReadState.Blocked, overlapResult.State);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Real Extension source blocks a physical alias of the selected workspace"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public async Task RealSourceBlocksPhysicalWorkspaceAlias()
    {
        using var workspace = TemporaryWorkspace.Create("extension-alias-target");
        using var aliasParent = TemporaryWorkspace.Create("extension-alias-parent");
        var alias = aliasParent.CreateDirectorySymbolicLink("source-alias", workspace.Path);
        var reader = new ExtensionSourceReader(new PhysicalPathResolver());

        var result = await reader.ReadAsync(Workspace(workspace), alias, CancellationToken.None);

        Assert.Equal(ExtensionSourceReadState.Blocked, result.State);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Real Extension source overlap takes precedence over missing and file classification"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    [InlineData("missing-child")]
    [InlineData("file-child")]
    public async Task RealSourceBlocksLexicalWorkspaceChildrenBeforeClassification(string scenario)
    {
        using var workspace = TemporaryWorkspace.Create("extension-overlap-precedence");
        var source = scenario switch
        {
            "missing-child" => workspace.Combine("missing-catalogue"),
            "file-child" => workspace.CreateFile("catalogue.json"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The overlap scenario is not defined."),
        };

        var result = await new ExtensionSourceReader(new PhysicalPathResolver()).ReadAsync(
            Workspace(workspace),
            source,
            TestContext.Current.CancellationToken);

        Assert.Equal(ExtensionSourceReadState.Blocked, result.State);
    }

    private static CliWorkspace Workspace(TemporaryWorkspace workspace)
        => new(
            lexicalRoot: workspace.Path,
            physicalRoot: workspace.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static void WriteManifest(
        TemporaryWorkspace source,
        string directory,
        string id,
        string[] dependencies)
    {
        source.WriteText(
            $"{directory}/extension.json",
            $$"""
            {
              "id": {{JsonString(id)}},
              "name": {{JsonString(id)}},
              "description": {{JsonString($"Package {id}.")}},
              "version": "1.0.0",
              "dependencies": [{{string.Join(", ", dependencies.Select(JsonString))}}]
            }
            """);
    }

    private static string JsonString(string value)
        => $"\"{JsonEncodedText.Encode(value)}\"";
}

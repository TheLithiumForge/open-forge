using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.References;

[Trait("Feature", "references"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class SourceLinkFolderIntegrationTests
{
    [Theory(DisplayName = "Existing contained folders resolve without byte or Markdown acquisition"),
        InlineData("../src/cli"), InlineData("../src/cli/"), InlineData("../src/cli%2F")]
    public async Task ContainedFolderResolves(string destination)
    {
        using var temporary = TemporaryWorkspace.Create("folder-destination");
        temporary.CreateDirectory(".agents");
        temporary.CreateDirectory("src/cli");
        var result = await Resolve(temporary, destination);
        Assert.Equal(SourceLinkTargetResolution.Complete, result.Target.Resolution);
        Assert.Equal("src/cli", result.Target.Path);
        Assert.Null(result.Finding);
    }

    [Theory(DisplayName = "Folder fragments have a precise malformed cause without attempting to read the folder"),
        InlineData("../src/cli#heading"), InlineData("../src/cli/#heading"), InlineData("../src/cli/#")]
    public async Task FolderFragmentIsMalformed(string destination)
    {
        using var temporary = TemporaryWorkspace.Create("folder-fragment");
        temporary.CreateDirectory(".agents");
        temporary.CreateDirectory("src/cli");
        var result = await Resolve(temporary, destination);
        Assert.Equal(SourceLinkTargetResolution.Malformed, result.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.DestinationMalformed, result.Finding?.Code);
        Assert.Equal("A folder has no headings. Remove the heading fragment.", result.Finding?.Cause);
    }

    [Fact(DisplayName = "Folder links follow contained symlinks and freshly refuse a retargeted escape")]
    public async Task FolderLinkIdentityIsFresh()
    {
        using var temporary = TemporaryWorkspace.Create("folder-link-identity");
        temporary.CreateDirectory("workspace/.agents");
        temporary.CreateDirectory("workspace/target");
        temporary.CreateDirectory("outside");
        temporary.CreateDirectorySymbolicLink("workspace/alias", "target");
        var workspace = new CliWorkspace(temporary.Combine("workspace"), temporary.Combine("workspace"), CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(workspace, TestContext.Current.CancellationToken);
        var resolver = Resolver().CreateSession(session);
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = ".agents/source.md",
            RawDestination = "../alias/",
        };
        Assert.Equal(SourceLinkTargetResolution.Complete, (await resolver.ResolveAsync(input, TestContext.Current.CancellationToken)).Target.Resolution);
        Directory.Delete(temporary.Combine("workspace/alias"));
        Directory.CreateSymbolicLink(temporary.Combine("workspace/alias"), "../outside");
        Assert.Equal(SourceLinkTargetResolution.PhysicalEscape, (await resolver.ResolveAsync(input, TestContext.Current.CancellationToken)).Target.Resolution);
    }

    private static SourceLinkDestinationResolver Resolver() => new(new PhysicalPathResolver(),
        (_, _, _) => throw new InvalidOperationException("A folder target must not be read."),
        _ => throw new InvalidOperationException("A folder target must not be parsed."));

    private static async Task<SourceLinkDestinationFacts> Resolve(TemporaryWorkspace temporary, string destination)
    {
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(workspace, TestContext.Current.CancellationToken);
        return await Resolver().ResolveAsync(new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = ".agents/source.md",
            RawDestination = destination,
        }, TestContext.Current.CancellationToken);
    }
}

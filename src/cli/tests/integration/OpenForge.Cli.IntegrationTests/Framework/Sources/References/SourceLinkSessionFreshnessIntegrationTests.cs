using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.References;

[Trait("Feature", "references"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class SourceLinkSessionFreshnessIntegrationTests
{
    [Fact(DisplayName = "One reference session observes a missing target being created and a contained link escaping")]
    public async Task PhysicalIdentityRefreshesWithinSession()
    {
        using var temporary = TemporaryWorkspace.Create("reference-session-identity");
        temporary.CreateDirectory("workspace/.agents");
        temporary.WriteText("outside.md", "# Old\n");
        var workspace = new CliWorkspace(temporary.Combine("workspace"), temporary.Combine("workspace"), CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var token = TestContext.Current.CancellationToken;
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), token);
        var resolver = new SourceLinkDestinationResolver(new PhysicalPathResolver(), StrictUtf8FileReader.ReadAsync, new MarkdownDocumentParser().Parse).CreateSession(session);
        SourceLinkDestinationInput Input(string destination) => new()
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = ".agents/source.md",
            RawDestination = destination,
        };

        Assert.Equal(SourceLinkTargetResolution.Missing, (await resolver.ResolveAsync(Input("target.md#old"), token)).Target.Resolution);
        temporary.WriteText("workspace/.agents/target.md", "# Old\n");
        Assert.Equal(SourceLinkTargetResolution.Complete, (await resolver.ResolveAsync(Input("target.md#old"), token)).Target.Resolution);
        var link = temporary.CreateFileSymbolicLink("workspace/.agents/alias.md", "target.md");
        Assert.Equal(SourceLinkTargetResolution.Complete, (await resolver.ResolveAsync(Input("alias.md#old"), token)).Target.Resolution);
        File.Delete(link);
        File.CreateSymbolicLink(link, "../../outside.md");
        Assert.Equal(SourceLinkTargetResolution.PhysicalEscape, (await resolver.ResolveAsync(Input("alias.md#old"), token)).Target.Resolution);
    }

    [Fact(DisplayName = "One reference session refreshes edited bytes, acquired locks and released locks without seeding old reads")]
    public async Task BytesAndFailuresRefreshWithinSession()
    {
        using var temporary = TemporaryWorkspace.Create("reference-session-bytes");
        temporary.WriteText(".agents/target.md", "# Old\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var token = TestContext.Current.CancellationToken;
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), token);
        var target = session.Catalogue.FindByPath(".agents/target.md") ?? throw new InvalidOperationException();
        Assert.Equal("# Old\n", (await session.DocumentReader.ReadAsync(target.Base, token)).Read?.Value);
        var parses = 0;
        var resolver = new SourceLinkDestinationResolver(new PhysicalPathResolver(), StrictUtf8FileReader.ReadAsync, text =>
        {
            parses++;
            return new MarkdownDocumentParser().Parse(text);
        }).CreateSession(session.DocumentReader.Documents);
        resolver.RetainSource(target.Base, new MarkdownDocumentParser().Parse("# Old\n"));
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = ".agents/source.md",
            RawDestination = "target.md#old",
        };
        Assert.Equal(SourceLinkTargetResolution.Complete, (await resolver.ResolveAsync(input, token)).Target.Resolution);
        temporary.ReplaceText(".agents/target.md", "# New\n");
        Assert.Equal(SourceLinkTargetResolution.FragmentMissing, (await resolver.ResolveAsync(input, token)).Target.Resolution);
        var alias = Path.GetFileName(temporary.Combine(".agents/target.md"));
        if (OperatingSystem.IsWindows()) alias = alias.ToUpperInvariant();
        Assert.Equal(SourceLinkTargetResolution.FragmentMissing, (await resolver.ResolveAsync(input with { RawDestination = alias + "#old" }, token)).Target.Resolution);
        using (var held = new FileStream(target.Base.PhysicalPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Assert.Equal(SourceLinkTargetResolution.Unreadable, (await resolver.ResolveAsync(input, token)).Target.Resolution);
        Assert.Equal(SourceLinkTargetResolution.FragmentMissing, (await resolver.ResolveAsync(input, token)).Target.Resolution);
        Assert.Equal(1, parses);
        Assert.Equal("# New\n", (await session.DocumentReader.ReadAsync(target.Base, token)).Read?.Value);
    }
}

using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Reading;

[Trait("Feature", "source-reading"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class SourceReadSessionWorkTests
{
    [Fact(DisplayName = "Catalogue document and link inspection share path observations and parsed documents within one read stage")]
    public async Task SharedAcquisitionRefreshesOnTheNextReadStage()
    {
        using var temporary = TemporaryWorkspace.Create("unified-source-read-session");
        temporary.WriteText(".agents/nested/source.md", "[Target](target.md#target)\n");
        temporary.WriteText(".agents/nested/target.md", "# Target\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var observations = new Dictionary<string, int>(StringComparer.Ordinal);
        var paths = new PhysicalPathResolver(path =>
        {
            observations[path] = observations.GetValueOrDefault(path) + 1;
            return LinkTargetReader.Read(path);
        });
        var token = TestContext.Current.CancellationToken;
        var reader = new SourceReadSessionReader(paths);
        var session = await reader.ReadAsync(workspace, token);
        var target = session.Catalogue.FindByPath(".agents/nested/target.md") ?? throw new InvalidOperationException();
        var read = await session.DocumentReader.ReadAsync(target.Base, token);
        var text = read.Read?.Value ?? throw new InvalidOperationException();
        var resolver = new SourceLinkDestinationResolver(paths, StrictUtf8FileReader.ReadAsync, new MarkdownDocumentParser().Parse).CreateSession(session);
        var document = resolver.ParseDocument(text);
        Assert.Same(document, session.DocumentReader.ParseDocument(text));
        resolver.RetainSource(target.Base, document);
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = ".agents/nested/source.md",
            RawDestination = "target.md#target",
        };
        for (var index = 0; index < 10; index++)
            Assert.Equal(SourceLinkTargetResolution.Complete, (await resolver.ResolveAsync(input, token)).Target.Resolution);
        Assert.Equal(11, session.DocumentReader.Acquisition.ReadCount);
        Assert.Equal(1, observations[temporary.Combine(".agents/nested")]);

        temporary.ReplaceText(".agents/nested/target.md", "# Changed\n");
        var next = await reader.ReadAsync(workspace, token);
        var refreshed = await next.DocumentReader.ReadAsync(target.Base, token);
        Assert.Equal("# Changed\n", refreshed.Read?.Value);
        Assert.Equal(2, observations[temporary.Combine(".agents/nested")]);
        Assert.NotSame(document, next.DocumentReader.ParseDocument(refreshed.Read?.Value ?? throw new InvalidOperationException()));
    }
}

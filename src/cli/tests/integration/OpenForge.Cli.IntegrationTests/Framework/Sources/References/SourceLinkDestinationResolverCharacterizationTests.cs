using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.References;

public sealed class SourceLinkDestinationResolverCharacterizationTests
{
    private const string SourcePath = ".agents/nested/source.md";
    private const string TargetPath = ".agents/target.md";

    [Theory(DisplayName = "Full destination resolution and canonical matching retain distinct admission policies"),
        InlineData("", SourcePath, (int)SourceLinkTargetResolution.Malformed, true, null),
        InlineData("#source", SourcePath, (int)SourceLinkTargetResolution.Complete, true, SourcePath),
        InlineData("../target.md#bad%", TargetPath, (int)SourceLinkTargetResolution.EncodingUnsupported, true, null),
        InlineData("../target.md", TargetPath, (int)SourceLinkTargetResolution.Complete, true, TargetPath),
        InlineData("../../../outside.md", TargetPath, (int)SourceLinkTargetResolution.OutsideWorkspace, false, null),
        InlineData("../target.md?q=1", TargetPath, (int)SourceLinkTargetResolution.Query, false, null),
        InlineData("../target\\name.md", TargetPath, (int)SourceLinkTargetResolution.Malformed, false, null),
        InlineData("../target%5Cname.md", TargetPath, (int)SourceLinkTargetResolution.Malformed, false, null),
        InlineData("/target.md", TargetPath, (int)SourceLinkTargetResolution.Absolute, false, null),
        InlineData("%2Ftarget.md", TargetPath, (int)SourceLinkTargetResolution.Absolute, false, null),
        InlineData("C:/target.md", TargetPath, (int)SourceLinkTargetResolution.Absolute, false, null),
        InlineData("../café.md", ".agents/café.md", (int)SourceLinkTargetResolution.Complete, true, ".agents/café.md"),
        InlineData("../caf%C3%A9.md", ".agents/café.md", (int)SourceLinkTargetResolution.Complete, true, ".agents/café.md"),
        Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task CallableAdmissionPoliciesRemainDistinct(
        string rawDestination,
        string expectedCanonicalPath,
        int expectedResolution,
        bool expectedMatch,
        string? expectedResolvedPath)
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        var before = workspace.SnapshotHashes();
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: StrictUtf8FileReader.ReadAsync,
            markdownParser: new MarkdownDocumentParser().Parse);

        var facts = await resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, rawDestination),
            TestContext.Current.CancellationToken);
        var matches = SourceLinkDestinationResolver.ResolvesToCanonicalPath(
            workspace: workspace.Workspace,
            sourceCanonicalPath: SourcePath,
            rawDestination: rawDestination,
            expectedCanonicalPath: expectedCanonicalPath);

        Assert.Equal((SourceLinkTargetResolution)expectedResolution, facts.Target.Resolution);
        Assert.Equal(expectedResolvedPath, facts.Target.Path);
        Assert.Equal(expectedMatch, matches);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Literal unpaired UTF-16 retains its exception boundary in both source-link callables"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task LiteralUnpairedSurrogateIsNotSilentlyReplaced()
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: StrictUtf8FileReader.ReadAsync,
            markdownParser: new MarkdownDocumentParser().Parse);
        const string destination = "../\uD800.md";

        await Assert.ThrowsAsync<ArgumentException>(() => resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, destination),
            TestContext.Current.CancellationToken).AsTask());
        Assert.Throws<ArgumentException>(() => SourceLinkDestinationResolver.ResolvesToCanonicalPath(
            workspace: workspace.Workspace,
            sourceCanonicalPath: SourcePath,
            rawDestination: destination,
            expectedCanonicalPath: TargetPath));
    }

    [Fact(DisplayName = "An encoded query character reaches physical resolution while canonical matching preserves it"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task EncodedQueryCharacterRemainsPartOfTheLexicalPath()
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        var attemptedPaths = new List<string>();
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: (_, lexicalPath) =>
            {
                attemptedPaths.Add(lexicalPath);
                return PhysicalPathResolution.Classified(PhysicalPathState.Missing, lexicalPath);
            },
            strictUtf8Reader: StrictUtf8FileReader.ReadAsync,
            markdownParser: new MarkdownDocumentParser().Parse);

        var facts = await resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target%3F.md"),
            TestContext.Current.CancellationToken);
        var matches = SourceLinkDestinationResolver.ResolvesToCanonicalPath(
            workspace: workspace.Workspace,
            sourceCanonicalPath: SourcePath,
            rawDestination: "../target%3F.md",
            expectedCanonicalPath: ".agents/target?.md");

        Assert.Equal([workspace.Absolute(".agents/target?.md")], attemptedPaths);
        Assert.Equal(SourceLinkTargetResolution.Missing, facts.Target.Resolution);
        Assert.Equal(".agents/target?.md", facts.Target.Path);
        Assert.True(matches);
    }

    [Theory(DisplayName = "Injected physical exceptions remain unsafe facts before read or Markdown work"),
        InlineData(false), InlineData(true), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task PhysicalExceptionsStayInsideTheirExistingBoundary(bool cancellationException)
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        var calls = new List<string>();
        Exception failure = cancellationException ? new OperationCanceledException() : new IOException("physical failure");
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: (_, _) =>
            {
                calls.Add("physical");
                throw failure;
            },
            strictUtf8Reader: (physicalPath, logicalPath, cancellationToken) =>
            {
                calls.Add("read");
                return StrictUtf8FileReader.ReadAsync(
                    physicalPath: physicalPath,
                    logicalPath: logicalPath,
                    cancellationToken: cancellationToken);
            },
            markdownParser: source =>
            {
                calls.Add("markdown");
                return new MarkdownDocumentParser().Parse(source);
            });

        var facts = await resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target.md#details"),
            TestContext.Current.CancellationToken);

        Assert.Equal(["physical"], calls);
        Assert.Equal(SourceLinkTargetResolution.PhysicalEscape, facts.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnsafe, facts.Finding?.Code);
        Assert.Equal("The local destination physical boundary could not be established.", facts.Finding?.Cause);
    }

    [Fact(DisplayName = "An injected strict-read exception propagates unchanged instead of becoming a target fact"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task ReadExceptionPropagatesUnchanged()
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        var failure = new IOException("read failure");
        var markdownCalls = 0;
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: (_, _, _) => throw failure,
            markdownParser: source =>
            {
                markdownCalls++;
                return new MarkdownDocumentParser().Parse(source);
            });

        var exception = await Assert.ThrowsAsync<IOException>(() => resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target.md#details"),
            TestContext.Current.CancellationToken).AsTask());

        Assert.Same(failure, exception);
        Assert.Equal(0, markdownCalls);
    }

    [Fact(DisplayName = "An injected strict-read cancellation propagates with its original identity"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task ReadCancellationPropagatesUnchanged()
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        using var cancellation = new CancellationTokenSource();
        var failure = new OperationCanceledException(cancellation.Token);
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: (_, _, _) => throw failure,
            markdownParser: new MarkdownDocumentParser().Parse);

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target.md#details"),
            cancellation.Token).AsTask());

        Assert.Same(failure, exception);
    }

    [Fact(DisplayName = "A cancelled read result becomes cancellation before Markdown parsing"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task CancelledReadResultPreservesTheOperationToken()
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        using var cancellation = new CancellationTokenSource();
        var markdownCalls = 0;
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: (_, logicalPath, _) => ValueTask.FromResult(FileReadResult<string>.Cancelled(logicalPath)),
            markdownParser: source =>
            {
                markdownCalls++;
                return new MarkdownDocumentParser().Parse(source);
            });

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target.md#details"),
            cancellation.Token).AsTask());

        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.Equal(0, markdownCalls);
    }

    [Theory(DisplayName = "Injected Markdown exceptions remain unreadable facts after the target read"),
        InlineData(false), InlineData(true), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task MarkdownExceptionsStayInsideTheirExistingBoundary(bool cancellationException)
    {
        using var workspace = CreateWorkspace();
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        var calls = new List<string>();
        Exception failure = cancellationException ? new OperationCanceledException() : new IOException("Markdown failure");
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: (physicalPath, logicalPath, cancellationToken) =>
            {
                calls.Add("read");
                return StrictUtf8FileReader.ReadAsync(
                    physicalPath: physicalPath,
                    logicalPath: logicalPath,
                    cancellationToken: cancellationToken);
            },
            markdownParser: _ =>
            {
                calls.Add("markdown");
                throw failure;
            });

        var facts = await resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target.md#details"),
            TestContext.Current.CancellationToken);

        Assert.Equal(["read", "markdown"], calls);
        Assert.Equal(SourceLinkTargetResolution.Unreadable, facts.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnreadable, facts.Finding?.Code);
        Assert.Equal("The target Markdown fragment facts could not be established.", facts.Finding?.Cause);
    }

    [Fact(DisplayName = "An exact target ID collision without a fragment returns before target text is read"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task IdentityCollisionWithoutFragmentDoesNotReadTargetText()
    {
        using var workspace = CreateWorkspace();
        workspace.Write(".agents/target/_target.md", "# Other target\n");
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        Assert.Equal(2, catalogue.FindAllById("target").Count);
        var reads = 0;
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: (physicalPath, logicalPath, cancellationToken) =>
            {
                reads++;
                return StrictUtf8FileReader.ReadAsync(
                    physicalPath: physicalPath,
                    logicalPath: logicalPath,
                    cancellationToken: cancellationToken);
            },
            markdownParser: new MarkdownDocumentParser().Parse);

        var facts = await resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target.md"),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, reads);
        Assert.Equal(SourceLinkTargetResolution.Complete, facts.Target.Resolution);
        Assert.Null(facts.Target.Id);
        Assert.Equal(SourceLinkDestinationFindingCode.IdentityCollision, facts.Finding?.Code);
        Assert.Equal([TargetPath, ".agents/target/_target.md"], facts.Finding?.Candidates.Select(candidate => candidate.Path));
    }

    [Theory(DisplayName = "Fragment resolution preserves precedence over an admitted target ID collision"),
        InlineData("details", (int)SourceLinkTargetResolution.Complete, (int)SourceLinkDestinationFindingCode.IdentityCollision, null),
        InlineData("DETAILS", (int)SourceLinkTargetResolution.FragmentMissing, (int)SourceLinkDestinationFindingCode.FragmentMissing, "details"),
        InlineData("absent", (int)SourceLinkTargetResolution.FragmentMissing, (int)SourceLinkDestinationFindingCode.FragmentMissing, null),
        Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task FragmentOutcomesPrecedeIdentityCollision(
        string fragment,
        int expectedResolution,
        int expectedFinding,
        string? expectedCanonicalFragment)
    {
        using var workspace = CreateWorkspace();
        workspace.Write(".agents/target/_target.md", "# Other target\n");
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        Assert.Equal(2, catalogue.FindAllById("target").Count);
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: StrictUtf8FileReader.ReadAsync,
            markdownParser: new MarkdownDocumentParser().Parse);

        var facts = await resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, $"../target.md#{fragment}"),
            TestContext.Current.CancellationToken);

        Assert.Equal((SourceLinkTargetResolution)expectedResolution, facts.Target.Resolution);
        Assert.Null(facts.Target.Id);
        Assert.Equal(fragment, facts.Fragment);
        Assert.Equal((SourceLinkDestinationFindingCode)expectedFinding, facts.Finding?.Code);
        Assert.Equal(expectedCanonicalFragment, facts.CanonicalFragment?.Canonical);
        if (expectedCanonicalFragment is not null)
        {
            Assert.Equal("DETAILS", facts.CanonicalFragment?.Authored);
        }

        if ((SourceLinkDestinationFindingCode)expectedFinding == SourceLinkDestinationFindingCode.IdentityCollision)
        {
            Assert.Equal([TargetPath, ".agents/target/_target.md"], facts.Finding?.Candidates.Select(candidate => candidate.Path));
        }
        else
        {
            Assert.Empty(Assert.IsType<SourceLinkDestinationFinding>(facts.Finding).Candidates);
        }
    }

    [Fact(DisplayName = "Unavailable canonical heading facts precede missing-fragment and target ID collision output"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task UnavailableCanonicalFragmentPrecedesMissingAndCollision()
    {
        using var workspace = SourceIntegrationWorkspace.Create("source-link-unavailable-fragment");
        workspace.Write(SourcePath, "# Source\n");
        const string targetContents = "# <span>Details</span>\n";
        workspace.Write(TargetPath, targetContents);
        workspace.Write(".agents/target/_target.md", "# Other target\n");
        var catalogue = await ReadCatalogueAsync(workspace.Workspace);
        Assert.Equal(2, catalogue.FindAllById("target").Count);
        var parser = new MarkdownDocumentParser();
        var heading = Assert.Single(parser.Parse(targetContents).Headings);
        Assert.True(heading.IsCanonical);
        Assert.Null(heading.FragmentIdentifier);
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: ResolvePhysical,
            strictUtf8Reader: StrictUtf8FileReader.ReadAsync,
            markdownParser: parser.Parse);

        var facts = await resolver.ResolveAsync(
            Input(workspace.Workspace, catalogue, "../target.md#absent"),
            TestContext.Current.CancellationToken);

        Assert.Equal(SourceLinkTargetResolution.Unreadable, facts.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnreadable, facts.Finding?.Code);
        Assert.Equal("A canonical target heading has no available fragment identifier.", facts.Finding?.Cause);
        Assert.Empty(Assert.IsType<SourceLinkDestinationFinding>(facts.Finding).Candidates);
        Assert.Null(facts.CanonicalFragment);
    }

    private static SourceIntegrationWorkspace CreateWorkspace()
    {
        var workspace = SourceIntegrationWorkspace.Create("source-link-resolution-characterization");
        workspace.Write(SourcePath, "# Source\n");
        workspace.Write(TargetPath, "# Details\n");
        workspace.Write(".agents/café.md", "# Café\n");
        return workspace;
    }

    private static async ValueTask<SourceCatalogue> ReadCatalogueAsync(CliWorkspace workspace)
    {
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace, [".agents"]),
            TestContext.Current.CancellationToken);
        Assert.False(catalogue.IsCancelled);
        Assert.NotNull(catalogue.FindByPath(SourcePath));
        Assert.NotNull(catalogue.FindByPath(TargetPath));
        return catalogue;
    }

    private static SourceLinkDestinationInput Input(
        CliWorkspace workspace,
        SourceCatalogue catalogue,
        string destination)
        => new()
        {
            Workspace = workspace,
            Catalogue = catalogue,
            SourceCanonicalPath = SourcePath,
            RawDestination = destination,
        };

    private static PhysicalPathResolution ResolvePhysical(CliWorkspace workspace, string lexicalPath)
        => new PhysicalPathResolver().ResolveCandidate(
            lexicalWorkspaceRoot: workspace.LexicalRoot,
            physicalWorkspaceRoot: workspace.PhysicalRoot,
            candidatePath: lexicalPath);
}

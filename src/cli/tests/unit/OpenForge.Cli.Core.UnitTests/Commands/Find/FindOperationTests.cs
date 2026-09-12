using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Application;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Commands.Find.Shared.Projection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find;

public sealed class FindOperationTests
{
    [Theory(DisplayName = "Find operation reads one boundary, resolves the universe before layers, and invokes explicit boundaries once"),
        InlineData("overwrite-path")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public async Task OperationReadsOneBoundaryAndFormsUniverseBeforeLayers(string selectorForm)
    {
        var fixture = CreateOperationFixture(withOverwrite: selectorForm == "overwrite-path");
        var calls = new List<string>();
        var operation = CreateOperation(
            fixture,
            calls,
            routeFacts: false,
            selectedLayerReader: (_, layer, cancellationToken) =>
            {
                calls.Add($"layer:{layer.Kind}");
                return ValueTask.FromResult(CompleteRead(layer));
            });
        var request = CreateRequest(selectorForm, content: []);

        var result = await operation.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(1, fixture.SourceBoundaryCalls);
        Assert.Equal(1, fixture.PhysicalPathCalls);
        var expectedLayerCount = selectorForm == "overwrite-path" ? 2 : 1;
        Assert.Equal(
            expectedLayerCount,
            calls.Count(value => value.StartsWith("layer:", StringComparison.Ordinal)));
        Assert.Equal(
            selectorForm == "overwrite-path"
                ? ["boundary", "physical", "layer:Base", "markdown", "frontmatter", "layer:Overwrite", "markdown", "frontmatter"]
                : ["boundary", "physical", "layer:Base", "markdown", "frontmatter"],
            calls);
        Assert.Empty(result.Findings);
    }

    [Theory(DisplayName = "Find operation requests route facts only when a matched metadata projection is effective"),
        InlineData("", 0),
        InlineData("metadata", 1)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public async Task OperationSkipsRouteFactsUnlessMatchedMetadataIsRequested(
        string content,
        int expectedRouteFactsCalls)
    {
        var fixture = CreateOperationFixture();
        var routeFactsCalls = 0;
        var operation = CreateOperation(
            fixture,
            [],
            routeFacts: true,
            routeFactsCall: () => routeFactsCalls++);
        var request = CreateRequest("id", SplitContent(content));

        var result = await operation.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expectedRouteFactsCalls, routeFactsCalls);
        Assert.Single(result.Matches);
        if (expectedRouteFactsCalls == 1)
        {
            Assert.Contains(
                result.Matches[0].Projections,
                projection => projection.Part == FindContentPartKind.Metadata);
        }
    }

    [Fact(DisplayName = "Find operation retains safe matches when cancellation is raised at a deterministic selected-layer seam")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public async Task OperationRetainsSafeMatchesAcrossDeterministicMidCancellation()
    {
        var fixture = CreateOperationFixture(withSecondSource: true);
        using var cancellation = new CancellationTokenSource();
        var selectedLayerCalls = 0;
        var operation = CreateOperation(
            fixture,
            [],
            routeFacts: false,
            selectedLayerReader: (_, layer, token) =>
            {
                selectedLayerCalls++;
                if (selectedLayerCalls == 1)
                {
                    return ValueTask.FromResult(CompleteRead(layer));
                }

                cancellation.Cancel();
                return ValueTask.FromResult(CancelledRead(layer));
            });

        var result = await operation.ExecuteAsync(
            CreateRequest("default", []),
            cancellation.Token);

        Assert.Equal(2, selectedLayerCalls);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(fixture.Source.Identity.AutomaticId, Assert.Single(result.Matches).Id);
        Assert.Contains(result.Findings, finding => finding.Code == FindFindingCode.Interrupted);
        Assert.Equal(FindCoverageState.Interrupted, result.Coverage.State);
    }

    [Fact(DisplayName = "Find operation converts unexpected boundary failures to bounded operation findings without exception identity")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public async Task OperationConvertsUnexpectedBoundaryFailureWithoutLeakingExceptionIdentity()
    {
        var fixture = CreateOperationFixture();
        const string failureMessage = "filesystem-failure-with-type-name";
        var operation = CreateOperation(
            fixture,
            [],
            routeFacts: false,
            sourceSessionRead: (_, _) =>
                throw new InvalidOperationException(failureMessage));

        var result = await operation.ExecuteAsync(
            CreateRequest("id", []),
            CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(FindFindingCode.OperationFailed, finding.Code);
        Assert.DoesNotContain(failureMessage, finding.Cause, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), finding.Cause, StringComparison.Ordinal);
        Assert.DoesNotContain("StackTrace", finding.Cause, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(result.Matches);
    }

    private static FindOperation CreateOperation(
        OperationFixture fixture,
        IList<string> calls,
        bool routeFacts,
        SourceReadSessionRead? sourceSessionRead = null,
        Action? routeFactsCall = null,
        FindSelectedLayerReader? selectedLayerReader = null)
    {
        var sourceReader = new SourceDocumentReader(fixture.Workspace);
        var sessionRead = sourceSessionRead
            ?? ((_, _) =>
            {
                calls.Add("boundary");
                fixture.SourceBoundaryCalls++;
                return ValueTask.FromResult(new SourceReadSession(
                    fixture.Catalogue,
                    sourceReader,
                    new SourceCatalogueSelectionScope(
                        ".agents",
                        Physical(fixture.Workspace.PhysicalRoot, ".agents"))));
            });
        SourcePhysicalPathResolver physicalPathResolver = (_, path) =>
        {
            calls.Add("physical");
            fixture.PhysicalPathCalls++;
            return PhysicalPathResolution.Contained(path, Physical(fixture.Workspace.PhysicalRoot, path));
        };
        var readLayer = selectedLayerReader
            ?? ((_, layer, _) =>
            {
                calls.Add($"layer:{layer.Kind}");
                return ValueTask.FromResult(CompleteRead(layer));
            });
        FindRouteFactsReader readRouteFacts = (_, _, _) =>
        {
            routeFactsCall?.Invoke();
            return ValueTask.FromResult(CreateRouteFacts(fixture.Source));
        };
        FindMarkdownDocumentReader readMarkdown = source =>
        {
            calls.Add("markdown");
            return CreateDocument(source);
        };
        FindFrontmatterFactsReader readFrontmatter = input =>
        {
            calls.Add("frontmatter");
            return new FindFrontmatterFacts(
                FindFrontmatterAvailability.Complete,
                "Description",
                [new FindFrontmatterTagOccurrence(
                    "Topic",
                    new SourceLocation(1, 1, 0, 5))]);
        };

        if (!routeFacts)
        {
            routeFactsCall = null;
        }

        return new FindOperation(
            new FindSourceResolver(
                sessionRead,
                new FindUniverseResolver(new SourceUniverseFilterResolver(
                    new SourceReferenceResolver(physicalPathResolver))),
                physicalPathResolver,
                readRouteFacts),
            new FindLayerInspector(
                readLayer,
                readMarkdown,
                readFrontmatter,
                new FindBodyTagScanner()),
            new FindMatcher(),
            new FindProjectionBuilder(),
            new FindResultBuilder());
    }

    private static FindRequest CreateRequest(
        string selectorForm,
        IEnumerable<string> content)
    {
        var workspace = CreateWorkspace();
        var selector = selectorForm switch
        {
            "id" => "docs/result",
            "overwrite-path" => ".agents/docs/result.overwrite.md",
            "default" => null,
            _ => throw new ArgumentOutOfRangeException(nameof(selectorForm), selectorForm, "The operation selector form is not defined."),
        };
        var contentParts = content.Select(ReadContentPart).ToArray();
        var predicate = new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic");
        return new FindRequest(
            workspace,
            new FindUniverseFilter(selector is null ? [] : [selector], []),
            new FindQuery(
                [predicate],
                [predicate],
                FindRequirement.All,
                new FindRegionSelection(
                    [],
                    [new FindRegion(FindRegionKind.Frontmatter, null, "frontmatter")],
                    [new FindRegion(FindRegionKind.Body, null, "body")])),
            new FindPresentationSelection(
                null,
                CliView.Expanded,
                new FindContentSelection(contentParts, contentParts)));
    }

    private static OperationFixture CreateOperationFixture(
        bool withSecondSource = false,
        bool withOverwrite = false)
    {
        var workspace = CreateWorkspace();
        var source = CreateSource(workspace, ".agents/docs/result.md", withOverwrite);
        var sources = new List<SourceLogicalSource> { source };
        var candidates = new List<SourceCandidate> { Candidate(workspace, source) };
        if (withSecondSource)
        {
            var second = CreateSource(workspace, ".agents/docs/second.md");
            sources.Add(second);
            candidates.Add(Candidate(workspace, second));
        }

        return new OperationFixture(
            workspace,
            new SourceCatalogue(workspace, candidates, sources, [], false),
            source);
    }

    private static SourceLogicalSource CreateSource(
        CliWorkspace workspace,
        string path,
        bool withOverwrite = false)
    {
        var id = SourceIdentity.DeriveId(path)
            ?? throw new InvalidOperationException("The operation source must have a derived ID.");
        var baseLayer = new SourceLayer(
            path,
            Physical(workspace.PhysicalRoot, path),
            SourceDocumentForm.Markdown,
            SourceLayerKind.Base);
        SourceLayer? overwrite = null;
        if (withOverwrite)
        {
            var overwritePath = path[..^".md".Length] + ".overwrite.md";
            overwrite = new SourceLayer(
                overwritePath,
                Physical(workspace.PhysicalRoot, overwritePath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite);
        }

        return new SourceLogicalSource(new SourceLogicalIdentity(id, path), baseLayer, overwrite);
    }

    private static SourceCandidate Candidate(
        CliWorkspace workspace,
        SourceLogicalSource source)
    {
        var physicalPath = source.Base.PhysicalPath;
        return new SourceCandidate(
            source.Base.CanonicalPath,
            source.Base.Form,
            source.Identity.AutomaticId,
            PhysicalPathState.Contained,
            physicalPath,
            Path.GetDirectoryName(physicalPath));
    }

    private static SourceDocumentReadResult CompleteRead(SourceLayer layer)
    {
        var verification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Verified,
            layer.PhysicalPath,
            null);
        return new SourceDocumentReadResult(
            layer,
            verification,
            FileReadResult<string>.Complete(layer.CanonicalPath, "Topic body"));
    }

    private static SourceDocumentReadResult CancelledRead(SourceLayer layer)
    {
        var verification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Cancelled,
            null,
            null);
        return new SourceDocumentReadResult(layer, verification, null);
    }

    private static SourceRouteFacts CreateRouteFacts(SourceLogicalSource source)
    {
        var node = new SourceRouteNode(source.Identity, SourceRouteParentState.None, [], []);
        return new SourceRouteFacts(
            new SourceRouteTopology([node], [source.Identity.CanonicalBasePath]),
            [new SourceRouteFact(source.Identity, SourceRouteState.Routed, true)],
            [],
            true,
            false);
    }

    private static MarkdownDocumentFacts CreateDocument(string source)
    {
        var documentSource = source.Length == 0 ? "Topic body" : source;
        return new MarkdownDocumentFacts(
            documentSource,
            new MarkdownDocumentStructure(
                new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Missing, null, null, 0),
                new MarkdownTextSpan(0, documentSource.Length),
                [],
                [],
                MarkdownGeneratedRegionFact.Absent()),
            new MarkdownInlineFacts([], [], [], []));
    }

    private static FindContentPart ReadContentPart(string value)
        => value switch
        {
            "metadata" => new FindContentPart(FindContentPartKind.Metadata, null, "metadata"),
            "frontmatter" => new FindContentPart(FindContentPartKind.Frontmatter, null, "frontmatter"),
            "headings" => new FindContentPart(FindContentPartKind.Headings, null, "headings"),
            "body" => new FindContentPart(FindContentPartKind.Body, null, "body"),
            _ when value.StartsWith("section:", StringComparison.Ordinal)
                => new FindContentPart(FindContentPartKind.Section, value["section:".Length..], value),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The operation content part is not defined."),
        };

    private static string[] SplitContent(string content)
        => content.Length == 0
            ? []
            : content.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static CliWorkspace CreateWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-operation-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static string Physical(string root, string logicalPath)
        => Path.GetFullPath(Path.Combine(root, logicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private sealed record OperationFixture(
        CliWorkspace Workspace,
        SourceCatalogue Catalogue,
        SourceLogicalSource Source)
    {
        internal int SourceBoundaryCalls { get; set; }

        internal int PhysicalPathCalls { get; set; }
    }
}

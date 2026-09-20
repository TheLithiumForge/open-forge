using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationProjectorTests
{
    private const string RootPath = ".agents/root/_root.md";

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation projects Loader exposure from current direct topology and authored facts")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void LoaderProjectionUsesDirectChildrenAndAuthoredMetadata()
    {
        var loader = Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var root = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var topology = new SourceRouteTopology(
            [Node(root, SourceRouteParentState.None, [], [])],
            [root.Identity.CanonicalBasePath]);
        var request = Request(
            topology,
            [loader, root],
            [Region(loader, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [Metadata(root, "Root", ["Root", "Guide"])]);

        var projection = new GeneratedNavigationProjector().Project(request);

        var region = Assert.Single(projection.Regions);
        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Null(region.UnavailableReason);
        var entry = Assert.Single(region.Entries);
        Assert.Equal(root.Identity.CanonicalBasePath, entry.CanonicalPath);
        Assert.Equal("root/_root.md", entry.Destination);
        Assert.Equal("- [Root](root/_root.md) - #Root #Guide", entry.Line);
        Assert.Equal(
            "- [Root](root/_root.md) - #Root #Guide\n",
            region.ExpectedBody);
        Assert.True(projection.IsComplete);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation emits the exact empty body for a childless entrypoint")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void EmptyEntrypointProjectionUsesCanonicalSentinel()
    {
        var root = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var topology = new SourceRouteTopology(
            [Node(root, SourceRouteParentState.None, [], [])],
            []);
        var request = Request(
            topology,
            [root],
            [Region(root, OpenForgeDocumentSeed.GeneratedEntries(entries: "- old"))],
            []);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Empty(region.Entries);
        Assert.Equal("- none - No entries - #Empty\n", region.ExpectedBody);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation percent-encodes safe containing-file-relative Unicode and space destinations")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void DestinationsUseCanonicalUtf8PathEncoding()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(
            ".agents/root/café folder.md",
            "root/café-folder");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [Metadata(child, "Café", ["Docs"])]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var entry = Assert.Single(region.Entries);

        Assert.Equal("caf%C3%A9%20folder.md", entry.Destination);
        Assert.Equal(
            "- [Café](caf%C3%A9%20folder.md) - #Docs",
            entry.Line);
        Assert.Equal(
            child.Identity.CanonicalBasePath,
            SourceGeneratedDestinationResolver.Resolve(
                parent.Identity.CanonicalBasePath,
                false,
            entry.Destination));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation encodes Markdown delimiter parentheses for parser round trips")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void DestinationsEncodeMarkdownDelimitersForAcceptedRoundTrip()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(
            ".agents/root/child(paren).md",
            "root/child(paren)");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [Metadata(child, "Parentheses", ["Docs"])]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var entry = Assert.Single(region.Entries);
        var parsed = SourceGeneratedEntriesParser.Parse(
            new MarkdownDocumentParser().Parse(
                OpenForgeDocumentSeed.GeneratedEntries(entries: entry.Line)));

        Assert.Equal("child%28paren%29.md", entry.Destination);
        Assert.Equal(
            "- [Parentheses](child%28paren%29.md) - #Docs",
            entry.Line);
        Assert.Equal(SourceGeneratedEntriesState.Complete, parsed.State);
        Assert.Equal(entry.Destination, Assert.Single(parsed.Entries).Destination);
        Assert.Equal(
            child.Identity.CanonicalBasePath,
            SourceGeneratedDestinationResolver.Resolve(
                parent.Identity.CanonicalBasePath,
                false,
                Assert.Single(parsed.Entries).Destination));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation encodes Markdown delimiter parentheses for Loader parser round trips")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void LoaderDestinationsEncodeMarkdownDelimitersForAcceptedRoundTrip()
    {
        var loader = Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var child = Source(
            ".agents/root(paren)/_root.md",
            "root(paren)",
            SourceDocumentForm.CanonicalEntrypoint);
        var topology = new SourceRouteTopology(
            [Node(child, SourceRouteParentState.None, [], [])],
            [child.Identity.CanonicalBasePath]);
        var request = Request(
            topology,
            [loader, child],
            [Region(loader, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [Metadata(child, "Root", ["Root"])]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var entry = Assert.Single(region.Entries);
        var parsed = SourceLoaderEntriesParser.Parse(
            OpenForgeDocumentSeed.GeneratedEntries(entries: entry.Line));
        var destination = Assert.Single(parsed.Destinations);

        Assert.Equal("root%28paren%29/_root.md", entry.Destination);
        Assert.Equal(
            "- [Root](root%28paren%29/_root.md) - #Root",
            entry.Line);
        Assert.Equal(SourceLoaderEntriesParseState.Valid, parsed.State);
        Assert.Equal(SourceLoaderDestinationParseState.Valid, destination.State);
        Assert.Equal(child.Identity.CanonicalBasePath, destination.CanonicalPath);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Generated navigation escapes authored backslashes into one canonical Markdown link")]
    [InlineData("Trailing backslash\\", "- [Trailing backslash\\\\](child.md) - #Docs")]
    [InlineData("Repeated backslashes \\\\", "- [Repeated backslashes \\\\\\\\](child.md) - #Docs")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void AuthoredBackslashesProduceOneCanonicalMarkdownLink(
        string description,
        string expectedLine)
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [Metadata(child, description, ["Docs"])]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        var expectedDocument = Encoding.UTF8.GetString(change.ExpectedDocumentBytes.AsSpan());
        var reparsed = new MarkdownDocumentParser().Parse(expectedDocument);
        var link = Assert.Single(reparsed.Links);

        Assert.Equal(description, Assert.Single(region.Entries).Description);
        Assert.Equal(expectedLine, Assert.Single(region.Entries).Line);
        Assert.Equal(MarkdownLinkForm.Inline, link.Form);
        Assert.Equal("child.md", link.RawDestination);
        Assert.Equal(expectedLine[2..expectedLine.IndexOf(" - #", StringComparison.Ordinal)],
            expectedDocument[link.Span.Start..link.Span.End]);
        Assert.Equal(
            SourceGeneratedEntriesState.Complete,
            SourceGeneratedEntriesParser.Parse(reparsed).State);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation orders destinations and retains one physical child across aliases")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void ProjectionDeduplicatesPhysicalAliasesDeterministically()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var firstAlias = Source(".agents/root/a.md", "a", physicalPath: Physical("shared.md"));
        var secondAlias = Source(".agents/root/z.md", "z", physicalPath: Physical("shared.md"));
        var other = Source(".agents/root/b.md", "b");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [
                    secondAlias.Identity.CanonicalBasePath,
                    other.Identity.CanonicalBasePath,
                    firstAlias.Identity.CanonicalBasePath,
                ]),
                Node(firstAlias, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
                Node(secondAlias, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
                Node(other, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, secondAlias, other, firstAlias],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [
                Metadata(secondAlias, "Alias Z", ["Two"]),
                Metadata(other, "Other", ["Three"]),
            ]);

        var projection = new GeneratedNavigationProjector().Project(request);
        var repeated = new GeneratedNavigationProjector().Project(request);
        var region = Assert.Single(projection.Regions);

        Assert.Equal(
            ["a.md", "b.md"],
            region.Entries.Select(entry => entry.Destination));
        Assert.Equal(
            [".agents/root/a.md", ".agents/root/b.md"],
            region.Entries.Select(entry => entry.CanonicalPath));
        Assert.Equal(
            projection.Regions.Select(value => value.ExpectedBody),
            repeated.Regions.Select(value => value.ExpectedBody));
        Assert.Equal(
            projection.Regions.Single().Change?.ExpectedDocumentBytes,
            repeated.Regions.Single().Change?.ExpectedDocumentBytes);
        Assert.Equal(
            projection.Regions.Single().Change?.ContentLocation,
            repeated.Regions.Single().Change?.ContentLocation);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation blocks a direct child when authored metadata is not complete")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void MissingChildMetadataIsUnavailable()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            []);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);

        Assert.Equal(GeneratedNavigationRegionState.Unavailable, region.State);
        Assert.Equal(GeneratedNavigationRegionUnavailableReason.MetadataUnavailable, region.UnavailableReason);
        Assert.Empty(region.Entries);
        Assert.Null(region.Change);
        Assert.Contains("complete authored", region.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation uses an ordinary child's automatic identity for plain missing metadata")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void PlainMissingMetadataUsesAutomaticIdentityWithoutTags()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [MissingMetadata(child)]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var entry = Assert.Single(region.Entries);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Equal(child.Identity.AutomaticId, entry.Description);
        Assert.Empty(entry.Tags);
        Assert.Equal("- [root/child](child.md)", entry.Line);
        Assert.Equal("- [root/child](child.md)\n", region.ExpectedBody);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation uses an observed description for description-only missing metadata")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void DescriptionOnlyMissingMetadataUsesObservedDescriptionWithoutTagSeparator()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [MissingMetadata(child, observedDescription: "Child description")]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var entry = Assert.Single(region.Entries);

        Assert.Equal("Child description", entry.Description);
        Assert.Empty(entry.Tags);
        Assert.Equal("- [Child description](child.md)", entry.Line);
        Assert.DoesNotContain(" - #", entry.Line, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation preserves exact observed tags for an entrypoint with missing description")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void TagsOnlyMissingEntrypointUsesObservedTagsVerbatim()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(
            ".agents/root/child/_child.md",
            "root/child",
            SourceDocumentForm.CanonicalEntrypoint);
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [MissingMetadata(child, observedTags: ["Docs", "Guide"])]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var entry = Assert.Single(region.Entries);

        Assert.Equal(child.Identity.AutomaticId, entry.Description);
        Assert.Equal(["Docs", "Guide"], entry.Tags);
        Assert.Equal("- [root/child](child/_child.md) - #Docs #Guide", entry.Line);
    }

    [Trait("Boundary", "Output")]
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation keeps one target for aliased entrypoint regions and ignores overwrite companions")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void ProjectionDeduplicatesAliasedTargetsAndUsesBaseFacts()
    {
        var firstTarget = Source(
            ".agents/one/_one.md",
            "one",
            SourceDocumentForm.CanonicalEntrypoint,
            physicalPath: Physical("shared-target.md"));
        var secondTarget = Source(
            ".agents/two/_two.md",
            "two",
            SourceDocumentForm.CanonicalEntrypoint,
            physicalPath: Physical("shared-target.md"));
        var child = Source(
            ".agents/one/child.md",
            "one/child",
            withOverwrite: true);
        var topology = new SourceRouteTopology(
            [
                Node(firstTarget, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(secondTarget, SourceRouteParentState.None, [], []),
                Node(child, SourceRouteParentState.Resolved, [firstTarget.Identity.CanonicalBasePath], []),
            ],
            []);
        var request = Request(
            topology,
            [secondTarget, child, firstTarget],
            [
                Region(
                    secondTarget,
                    OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale")),
                Region(
                    firstTarget,
                    OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale")),
            ],
            [Metadata(child, "Child", ["Child"])]);

        var projection = new GeneratedNavigationProjector().Project(request);

        var region = Assert.Single(projection.Regions);
        Assert.Equal(firstTarget.Identity.CanonicalBasePath, region.CanonicalPath);
        Assert.Equal("child.md", Assert.Single(region.Entries).Destination);
        Assert.NotNull(child.Overwrite);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation returns immutable unavailable facts for duplicate heading ownership")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void MalformedBoundaryDoesNotFormAChange()
    {
        var root = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var topology = new SourceRouteTopology(
            [Node(root, SourceRouteParentState.None, [], [])],
            []);
        var malformed = new MarkdownDocumentParser().Parse(
            "# Root\n\n## Entries\n\n## Entries\n\nstale\n");
        var request = Request(
            topology,
            [root],
            [Region(root, malformed)],
            []);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);

        Assert.Equal(GeneratedNavigationRegionState.Unavailable, region.State);
        Assert.Equal(GeneratedNavigationRegionUnavailableReason.GeneratedRegionInvalid, region.UnavailableReason);
        Assert.Empty(region.Entries);
        Assert.Null(region.Change);
        Assert.False(string.IsNullOrWhiteSpace(region.Cause));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation preserves exact CRLF bounded bytes and Unicode UTF-8 location")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void BoundedChangeRetainsPrefixSuffixAndByteCoordinates()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var source = OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = "- stale",
            LineEnding = "\r\n",
            Prefix = "# Root 😀\r\nnotes",
        });
        var document = new MarkdownDocumentParser().Parse(source);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, document)],
            [Metadata(child, "Café", ["Docs"])]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        var span = Assert.IsType<OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(
            document.GeneratedRegion.EntriesBlock?.Span);
        var expectedLocation = new Utf8SourceMap(source).Map(span.Start, span.Length);

        Assert.Equal(GeneratedNavigationChangeKind.Update, change.Kind);
        Assert.Equal("- [Café](child.md) - #Docs\r\n", change.ExpectedBody);
        Assert.Equal(expectedLocation, change.ContentLocation);
        Assert.True(Encoding.UTF8.GetByteCount(change.ExpectedBody) > change.ExpectedBody.Length);
        Assert.Equal(source[..span.Start], change.Prefix);
        Assert.Equal(source[span.End..], change.Suffix);
        Assert.True(change.PrefixPreserved);
        Assert.True(change.SuffixPreserved);
        Assert.Equal(
            Encoding.UTF8.GetBytes(source),
            change.BeforeDocumentBytes.ToArray());
        Assert.Equal(
            Encoding.UTF8.GetBytes(change.Prefix + change.ExpectedBody + change.Suffix),
            change.ExpectedDocumentBytes.ToArray());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Generated navigation preserves partial metadata changes and surrounding line endings")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void PartialMetadataRetainsExteriorBytesForLfAndCrlf(string lineEnding)
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var source = OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = "- stale",
            LineEnding = lineEnding,
            Prefix = $"# Root 😀{lineEnding}notes",
        });
        var document = new MarkdownDocumentParser().Parse(source);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, document)],
            [MissingMetadata(child, observedDescription: "Child")]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        var span = Assert.IsType<OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(
            document.GeneratedRegion.EntriesBlock?.Span);

        Assert.Equal($"- [Child](child.md){lineEnding}", change.ExpectedBody);
        Assert.Equal(source[..span.Start], change.Prefix);
        Assert.Equal(source[span.End..], change.Suffix);
        Assert.Equal(
            Encoding.UTF8.GetBytes(source),
            change.BeforeDocumentBytes.ToArray());
        Assert.Equal(
            Encoding.UTF8.GetBytes(change.Prefix + change.ExpectedBody + change.Suffix),
            change.ExpectedDocumentBytes.ToArray());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation classifies an already canonical body as an immutable no-op")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void CanonicalBodyIsUnchanged()
    {
        var parent = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(parent, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [parent.Identity.CanonicalBasePath], []),
            ],
            []);
        var source = OpenForgeDocumentSeed.GeneratedEntries(
            entries: "- [Child](child.md) - #Docs");
        var document = new MarkdownDocumentParser().Parse(source);
        var request = Request(
            topology,
            [parent, child],
            [Region(parent, document)],
            [Metadata(child, "Child", ["Docs"])]);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);

        Assert.True(change.IsUnchanged);
        Assert.False(change.RequiresUpdate);
        Assert.Equal(change.BeforeBody, change.ExpectedBody);
        Assert.Equal(
            Encoding.UTF8.GetBytes(source),
            change.ExpectedDocumentBytes.ToArray());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation keeps strict UTF-8 read failures as unavailable without a document")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void StrictUtf8FailureIsUnavailable()
    {
        var root = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var topology = new SourceRouteTopology(
            [Node(root, SourceRouteParentState.None, [], [])],
            []);
        var request = Request(
            topology,
            [root],
            [new GeneratedNavigationRegionInput(root, "The source read failed strict UTF-8 validation.")],
            []);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions);

        Assert.Equal(GeneratedNavigationRegionState.Unavailable, region.State);
        Assert.Equal(GeneratedNavigationRegionUnavailableReason.SourceDocumentUnavailable, region.UnavailableReason);
        Assert.Equal("The source read failed strict UTF-8 validation.", region.Cause);
        Assert.Null(region.Change);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation distinguishes unsupported source and unsafe topology rejections")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void SourceAndTopologyRejectionsAssignTypedReasons()
    {
        var ordinary = Source(".agents/ordinary.md", "ordinary");
        var emptyTopology = new SourceRouteTopology([], []);
        var validDocument = OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale");
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.RegionSourceUnsupported,
            ReadUnavailableReason(Request(
                emptyTopology,
                [ordinary],
                [Region(ordinary, validDocument)],
                [])));

        var root = Source(RootPath, "root", SourceDocumentForm.CanonicalEntrypoint);
        var aliasChild = Source(
            ".agents/root/alias.md",
            "root/alias",
            physicalPath: root.Base.PhysicalPath);
        var unsafeTopology = new SourceRouteTopology(
            [
                Node(root, SourceRouteParentState.None, [], [aliasChild.Identity.CanonicalBasePath]),
                Node(aliasChild, SourceRouteParentState.Resolved, [root.Identity.CanonicalBasePath], []),
            ],
            []);
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.TopologyUnsafe,
            ReadUnavailableReason(Request(
                unsafeTopology,
                [root, aliasChild],
                [Region(root, validDocument)],
                [Metadata(aliasChild, "Alias", ["Docs"])])));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation distinguishes missing, unavailable, and unsupported-line-ending regions")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void RegionDocumentRejectionsAssignTypedReasons()
    {
        var root = Source(RootPath, "root", SourceDocumentForm.CanonicalEntrypoint);
        var rootTopology = new SourceRouteTopology(
            [Node(root, SourceRouteParentState.None, [], [])],
            []);
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.GeneratedRegionMissing,
            ReadUnavailableReason(Request(rootTopology, [root], [Region(root, "# Root\n")], [])));
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.GeneratedRegionUnavailable,
            ReadUnavailableReason(Request(
                rootTopology,
                [root],
                [Region(root, "---\nopen-forge:\n  tags: [Root]\n# Root\n")],
                [])));
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported,
            ReadUnavailableReason(Request(
                rootTopology,
                [root],
                [Region(root, OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = "- stale",
                    LineEnding = "\r",
                }))],
                [])));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation distinguishes invalid metadata and unsafe destinations")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void MetadataAndDestinationRejectionsAssignTypedReasons()
    {
        var root = Source(RootPath, "root", SourceDocumentForm.CanonicalEntrypoint);
        var validDocument = OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale");
        var child = Source(".agents/root/child.md", "root/child");
        var routedTopology = new SourceRouteTopology(
            [
                Node(root, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [root.Identity.CanonicalBasePath], []),
            ],
            []);
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.MetadataInvalid,
            ReadUnavailableReason(Request(
                routedTopology,
                [root, child],
                [Region(root, validDocument)],
                [new GeneratedNavigationMetadata(
                    child,
                    SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed))])));
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable,
            ReadUnavailableReason(Request(
                routedTopology,
                [root, child],
                [Region(root, validDocument)],
                [Metadata(child, "[Unsafe]", ["Docs"])])));

        var unsafeDestination = Source(".agents/root/unsafe?.md", "root/unsafe?");
        var unsafeDestinationTopology = new SourceRouteTopology(
            [
                Node(root, SourceRouteParentState.None, [], [unsafeDestination.Identity.CanonicalBasePath]),
                Node(unsafeDestination, SourceRouteParentState.Resolved, [root.Identity.CanonicalBasePath], []),
            ],
            []);
        Assert.Equal(
            GeneratedNavigationRegionUnavailableReason.DestinationUnsafe,
            ReadUnavailableReason(Request(
                unsafeDestinationTopology,
                [root, unsafeDestination],
                [Region(root, validDocument)],
                [Metadata(unsafeDestination, "Unsafe destination", ["Docs"])])));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Generated navigation rejects an unsafe observed partial description")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void UnsafeObservedPartialDescriptionIsUnavailable()
    {
        var root = Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var topology = new SourceRouteTopology(
            [
                Node(root, SourceRouteParentState.None, [], [child.Identity.CanonicalBasePath]),
                Node(child, SourceRouteParentState.Resolved, [root.Identity.CanonicalBasePath], []),
            ],
            []);

        var region = Assert.Single(new GeneratedNavigationProjector().Project(Request(
            topology,
            [root, child],
            [Region(root, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"))],
            [MissingMetadata(child, observedDescription: "[Unsafe]")])).Regions);

        Assert.Equal(GeneratedNavigationRegionState.Unavailable, region.State);
        Assert.Equal(GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable, region.UnavailableReason);
        Assert.Empty(region.Entries);
        Assert.Null(region.Change);
    }

    private static GeneratedNavigationProjectionRequest Request(
        SourceRouteTopology topology,
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<GeneratedNavigationRegionInput> regions,
        IEnumerable<GeneratedNavigationMetadata> metadata)
    {
        return new GeneratedNavigationProjectionRequest(
            formation: GeneratedNavigationTestData.Formation(topology, sources),
            regions: regions,
            metadata: metadata);
    }

    private static GeneratedNavigationRegionUnavailableReason ReadUnavailableReason(
        GeneratedNavigationProjectionRequest request)
    {
        var reason = Assert.Single(new GeneratedNavigationProjector().Project(request).Regions).UnavailableReason;
        return Assert.IsType<GeneratedNavigationRegionUnavailableReason>(reason);
    }

    private static GeneratedNavigationRegionInput Region(
        SourceLogicalSource source,
        MarkdownDocumentFacts document)
    {
        return new GeneratedNavigationRegionInput(source, document);
    }

    private static GeneratedNavigationRegionInput Region(
        SourceLogicalSource source,
        string document)
    {
        return Region(source, new MarkdownDocumentParser().Parse(document));
    }

    private static GeneratedNavigationMetadata Metadata(
        SourceLogicalSource source,
        string description,
        IEnumerable<string> tags)
    {
        return new GeneratedNavigationMetadata(
            source,
            SourceAuthoredMetadataFacts.Complete(description, tags));
    }

    private static GeneratedNavigationMetadata MissingMetadata(
        SourceLogicalSource source,
        string? observedDescription = null,
        IEnumerable<string>? observedTags = null)
    {
        return new GeneratedNavigationMetadata(
            source,
            SourceAuthoredMetadataFacts.WithoutValues(
                SourceAuthoredMetadataState.Missing,
                observedDescription,
                observedTags));
    }

    private static SourceRouteNode Node(
        SourceLogicalSource source,
        SourceRouteParentState parentState,
        IEnumerable<string> parentPaths,
        IEnumerable<string> childPaths)
    {
        return new SourceRouteNode(
            source.Identity,
            parentState,
            parentPaths,
            childPaths);
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        string automaticId,
        SourceDocumentForm form = SourceDocumentForm.Markdown,
        string? physicalPath = null,
        bool withOverwrite = false)
    {
        var baseLayer = new SourceLayer(
            canonicalPath,
            physicalPath ?? Physical(canonicalPath),
            form,
            SourceLayerKind.Base);
        var overwrite = withOverwrite
            ? new SourceLayer(
                canonicalPath[..^".md".Length] + ".overwrite.md",
                Physical(canonicalPath[..^".md".Length] + ".overwrite.md"),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(automaticId, canonicalPath),
            baseLayer,
            overwrite);
    }

    private static string Physical(string canonicalPath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "generated-navigation-unit",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
    }
}

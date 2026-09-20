using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

public sealed class FindDocumentInspectionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Find inspection preserves YAML, Markdown, Unicode, CRLF, and overwrite-layer evidence")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    public async Task RealYamlMarkdownUnicodeCrLfAndOverwriteLayersProduceExactEvidenceAndLocations()
    {
        const string baseLf = """
            ---
            open-forge:
              description: Base description
              tags:
                - "Caf\u00E9"
                - "Base2"
            ---
            # Base 😀 Heading
            Visible text #BaseTag and [linked #Label](https://example.invalid/#destination) plus `#code`.

            Setext base
            ============

            ```text
            #hidden
            ```

            <div>#html</div>
            """;
        const string overwriteLf = """
            ---
            open-forge:
              description: Overwrite description
              tags: ["Flow", "工作", "Caf\u00E9"]
            ---
            ## Overwrite ✨ Heading
            Overwrite text #FlowTag and [label #LinkTag](https://example.invalid/#ignored).

            Setext overwrite
            -----------------
            """;

        using var workspace = TemporaryWorkspace.Create("find-document-inspection");
        workspace.CreateDirectory(".agents");
        var baseSource = WithCrLf(baseLf);
        var overwriteSource = WithCrLf(overwriteLf);
        workspace.WriteText(".agents/document.md", baseSource);
        workspace.WriteText(".agents/document.overwrite.md", overwriteSource);
        var before = workspace.SnapshotHashes();
        var cliWorkspace = CreateWorkspace(workspace);
        var cancellationToken = TestContext.Current.CancellationToken;
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(cliWorkspace, [".agents"]),
            cancellationToken);

        var source = Assert.Single(catalogue.Sources);
        var overwrite = Assert.IsType<SourceLayer>(source.Overwrite);
        Assert.Equal("document", source.Identity.AutomaticId);
        Assert.Equal(".agents/document.md", source.Identity.CanonicalBasePath);
        Assert.Equal(".agents/document.overwrite.md", overwrite.CanonicalPath);

        var reader = new SourceDocumentReader(cliWorkspace);
        var baseRead = await ReadSelectedLayer(reader, source.Base, cancellationToken);
        var overwriteRead = await ReadSelectedLayer(reader, overwrite, cancellationToken);
        Assert.Equal(SourceLayerVerificationState.Verified, baseRead.Verification.State);
        Assert.Equal(SourceLayerVerificationState.Verified, overwriteRead.Verification.State);
        Assert.Equal(baseSource, Assert.IsType<FileReadResult<string>>(baseRead.Read).Value);
        Assert.Equal(overwriteSource, Assert.IsType<FileReadResult<string>>(overwriteRead.Read).Value);
        Assert.Equal(before, workspace.SnapshotHashes());

        var inspector = CreateInspector();
        var baseFacts = await inspector.InspectAsync(
            new FindLayerInspectionInput(source, reader, source.Base),
            cancellationToken);
        var overwriteFacts = await inspector.InspectAsync(
            new FindLayerInspectionInput(source, reader, overwrite),
            cancellationToken);

        Assert.Same(source, baseFacts.Source);
        Assert.Same(source.Base, baseFacts.Layer);
        Assert.Equal("Base description", baseFacts.Description);
        Assert.Empty(baseFacts.Findings);
        Assert.Same(source, overwriteFacts.Source);
        Assert.Same(overwrite, overwriteFacts.Layer);
        Assert.Empty(overwriteFacts.Findings);

        var baseDocument = Assert.IsType<MarkdownDocumentFacts>(baseFacts.Document);
        var baseFrontmatter = Assert.IsType<FindFrontmatterFacts>(baseFacts.Frontmatter);
        var baseBodyTags = Assert.IsType<FindBodyTagFacts>(baseFacts.BodyTags);
        var overwriteDocument = Assert.IsType<MarkdownDocumentFacts>(overwriteFacts.Document);
        var overwriteFrontmatter = Assert.IsType<FindFrontmatterFacts>(overwriteFacts.Frontmatter);
        var overwriteBodyTags = Assert.IsType<FindBodyTagFacts>(overwriteFacts.BodyTags);

        AssertCompleteFrontmatterBoundary(baseDocument, baseSource);
        AssertCompleteFrontmatterBoundary(overwriteDocument, overwriteSource);
        AssertHeadingOutline(
            baseDocument,
            ["Base 😀 Heading", "Setext base"],
            [1, 1],
            [MarkdownHeadingForm.Atx, MarkdownHeadingForm.Setext],
            [true, false]);
        AssertHeadingOutline(
            overwriteDocument,
            ["Overwrite ✨ Heading", "Setext overwrite"],
            [2, 2],
            [MarkdownHeadingForm.Atx, MarkdownHeadingForm.Setext],
            [true, false]);

        var baseAtxStart = baseSource.IndexOf("# Base 😀 Heading", StringComparison.Ordinal);
        var baseSetextStart = baseSource.IndexOf("Setext base", StringComparison.Ordinal);
        var baseSetextEnd = baseSource.IndexOf("============", StringComparison.Ordinal) + "============".Length;
        Assert.Equal(
            [
                new MarkdownTextSpan(baseAtxStart, "# Base 😀 Heading".Length),
                new MarkdownTextSpan(baseSetextStart, baseSetextEnd - baseSetextStart),
            ],
            baseDocument.Headings.Select(heading => heading.Span));
        Assert.Equal(
            [
                new MarkdownTextSpan(baseAtxStart, baseSetextStart - baseAtxStart),
                new MarkdownTextSpan(baseSetextStart, baseSource.Length - baseSetextStart),
            ],
            baseDocument.Sections.Select(section => section.Span));

        var overwriteAtxStart = overwriteSource.IndexOf("## Overwrite ✨ Heading", StringComparison.Ordinal);
        var overwriteSetextStart = overwriteSource.IndexOf("Setext overwrite", StringComparison.Ordinal);
        var overwriteSetextEnd = overwriteSource.IndexOf("-----------------", StringComparison.Ordinal) + "-----------------".Length;
        Assert.Equal(
            [
                new MarkdownTextSpan(overwriteAtxStart, "## Overwrite ✨ Heading".Length),
                new MarkdownTextSpan(overwriteSetextStart, overwriteSetextEnd - overwriteSetextStart),
            ],
            overwriteDocument.Headings.Select(heading => heading.Span));
        Assert.Equal(
            [
                new MarkdownTextSpan(overwriteAtxStart, overwriteSetextStart - overwriteAtxStart),
                new MarkdownTextSpan(overwriteSetextStart, overwriteSource.Length - overwriteSetextStart),
            ],
            overwriteDocument.Sections.Select(section => section.Span));

        AssertFrontmatterFacts(
            baseFrontmatter,
            baseSource,
            "Base description",
            ["Café", "Base2"],
            ["\"Caf\\u00E9\"", "\"Base2\""]);
        AssertFrontmatterFacts(
            overwriteFrontmatter,
            overwriteSource,
            "Overwrite description",
            ["Flow", "工作", "Café"],
            ["\"Flow\"", "\"工作\"", "\"Caf\\u00E9\""]);
        AssertBodyTagFacts(baseBodyTags, baseSource, ["BaseTag", "Label"]);
        AssertBodyTagFacts(overwriteBodyTags, overwriteSource, ["FlowTag", "LinkTag"]);
        AssertVisibleContent(baseDocument, baseSource, "Visible text", "#destination", "#hidden", "#html");
        AssertVisibleContent(overwriteDocument, overwriteSource, "Overwrite text", "#ignored");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Find inspection maps invalid, changed, missing, and unavailable layers to typed incomplete facts")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    public async Task InvalidEncodingChangedMissingAndUnavailableLayersProduceTypedIncompleteFacts()
    {
        using var workspace = TemporaryWorkspace.Create("find-document-inspection-failures");
        workspace.CreateDirectory(".agents");
        workspace.WriteBytes(".agents/invalid.md", [0xC3, 0x28]);
        workspace.WriteText("changed-before.md", "before");
        workspace.WriteText("changed-after.md", "after");
        workspace.WriteText(".agents/missing.md", "missing");
        workspace.WriteText(".agents/unavailable.md", "unavailable");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/changed.md",
                workspace.Combine("changed-before.md"),
                out var changedLinkPath),
            "This integration case requires real symbolic-link support.");

        var cliWorkspace = CreateWorkspace(workspace);
        var cancellationToken = TestContext.Current.CancellationToken;
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(cliWorkspace, [".agents"]),
            cancellationToken);
        Assert.Equal(4, catalogue.Sources.Count);
        var invalidSource = FindSource(catalogue, ".agents/invalid.md");
        var changedSource = FindSource(catalogue, ".agents/changed.md");
        var missingSource = FindSource(catalogue, ".agents/missing.md");
        var unavailableSource = FindSource(catalogue, ".agents/unavailable.md");

        var linkPath = changedLinkPath
            ?? throw new InvalidOperationException("The changed fixture link was not created.");
        File.Delete(linkPath);
        File.CreateSymbolicLink(linkPath, workspace.Combine("changed-after.md"));
        File.Delete(workspace.Combine(".agents/missing.md"));
        var before = workspace.SnapshotHashes();
        var reader = new SourceDocumentReader(cliWorkspace);
        var invalidRead = await ReadSelectedLayer(reader, invalidSource.Base, cancellationToken);
        var changedRead = await ReadSelectedLayer(reader, changedSource.Base, cancellationToken);
        var missingRead = await ReadSelectedLayer(reader, missingSource.Base, cancellationToken);
        SourceDocumentReadResult unavailableRead;
        using (new FileStream(
                   workspace.Combine(".agents/unavailable.md"),
                   FileMode.Open,
                   FileAccess.ReadWrite,
                   FileShare.None))
        {
            unavailableRead = await ReadSelectedLayer(reader, unavailableSource.Base, cancellationToken);
        }

        Assert.Equal(SourceLayerVerificationState.Verified, invalidRead.Verification.State);
        Assert.Equal(
            FileReadState.InvalidEncoding,
            Assert.IsType<FileReadResult<string>>(invalidRead.Read).State);
        Assert.Equal(SourceLayerVerificationState.Changed, changedRead.Verification.State);
        Assert.Null(changedRead.Read);
        Assert.Equal(SourceLayerVerificationState.Missing, missingRead.Verification.State);
        Assert.Equal(FileReadState.Missing, Assert.IsType<FileReadResult<string>>(missingRead.Read).State);
        Assert.Equal(SourceLayerVerificationState.Verified, unavailableRead.Verification.State);
        var unavailableResult = Assert.IsType<FileReadResult<string>>(unavailableRead.Read);
        Assert.True(
            unavailableResult.State is FileReadState.AccessDenied or FileReadState.InputOutputFailure,
            $"Expected a typed unavailable read, got {unavailableResult.State}.");
        Assert.NotNull(unavailableResult.Failure);

        var inspector = CreateInspector();
        var invalidFacts = await inspector.InspectAsync(
            new FindLayerInspectionInput(invalidSource, reader, invalidSource.Base),
            cancellationToken);
        var changedFacts = await inspector.InspectAsync(
            new FindLayerInspectionInput(changedSource, reader, changedSource.Base),
            cancellationToken);
        var missingFacts = await inspector.InspectAsync(
            new FindLayerInspectionInput(missingSource, reader, missingSource.Base),
            cancellationToken);
        var unavailableFacts = await inspector.InspectAsync(
            new FindLayerInspectionInput(unavailableSource, reader, unavailableSource.Base),
            cancellationToken);

        AssertIncompleteInspection(invalidFacts, invalidSource, FindFindingCode.InvalidEncoding);
        AssertIncompleteInspection(changedFacts, changedSource, FindFindingCode.InspectionUnavailable);
        AssertIncompleteInspection(missingFacts, missingSource, FindFindingCode.InspectionUnavailable);
        AssertIncompleteInspection(unavailableFacts, unavailableSource, FindFindingCode.InspectionUnavailable);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static CliWorkspace CreateWorkspace(TemporaryWorkspace workspace)
        => new(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static ValueTask<SourceDocumentReadResult> ReadSelectedLayer(
        SourceDocumentReader reader,
        SourceLayer layer,
        CancellationToken cancellationToken)
        => reader.ReadAsync(layer, cancellationToken);

    private static FindLayerInspector CreateInspector()
        => new(
            ReadSelectedLayer,
            new MarkdownDocumentParser().Parse,
            new FindFrontmatterReader().Read,
            new FindBodyTagScanner());

    private static SourceLogicalSource FindSource(SourceCatalogue catalogue, string canonicalPath)
        => catalogue.Sources.Single(source =>
            string.Equals(source.Identity.CanonicalBasePath, canonicalPath, StringComparison.Ordinal));

    private static void AssertCompleteFrontmatterBoundary(
        MarkdownDocumentFacts document,
        string source)
    {
        var openingEnd = source.IndexOf("\r\n", StringComparison.Ordinal) + 2;
        var closingBreakStart = source.IndexOf("\r\n---\r\n", openingEnd, StringComparison.Ordinal);
        Assert.True(closingBreakStart >= 0);
        var closingStart = closingBreakStart + 2;
        var closingEnd = closingStart + 3;
        var bodyStart = closingEnd + 2;

        Assert.Equal(MarkdownFrontmatterState.Complete, document.Frontmatter.State);
        Assert.Equal(new MarkdownTextSpan(0, closingEnd), document.Frontmatter.BlockSpan);
        Assert.Equal(
            new MarkdownTextSpan(openingEnd, closingStart - openingEnd),
            document.Frontmatter.YamlSpan);
        Assert.Equal(bodyStart, document.Frontmatter.BodyStart);
        Assert.Equal(new MarkdownTextSpan(bodyStart, source.Length - bodyStart), document.BodySpan);
    }

    private static void AssertHeadingOutline(
        MarkdownDocumentFacts document,
        IReadOnlyList<string> expectedTexts,
        IReadOnlyList<int> expectedLevels,
        IReadOnlyList<MarkdownHeadingForm> expectedForms,
        IReadOnlyList<bool> expectedCanonical)
    {
        Assert.Equal(expectedTexts, document.Headings.Select(heading => heading.VisibleText));
        Assert.Equal(expectedLevels, document.Headings.Select(heading => heading.Level));
        Assert.Equal(expectedForms, document.Headings.Select(heading => heading.Form));
        Assert.Equal(expectedCanonical, document.Headings.Select(heading => heading.IsCanonical));
    }

    private static void AssertFrontmatterFacts(
        FindFrontmatterFacts facts,
        string source,
        string expectedDescription,
        IReadOnlyList<string> expectedTags,
        IReadOnlyList<string> authoredTokens)
    {
        Assert.Equal(FindFrontmatterAvailability.Complete, facts.Availability);
        Assert.Equal(expectedDescription, facts.Description);
        Assert.Equal(expectedTags, facts.Tags.Select(tag => tag.Authored));
        Assert.Equal(
            authoredTokens.Select(token => ExpectedLocation(source, token)),
            facts.Tags.Select(tag => tag.Location));
    }

    private static void AssertBodyTagFacts(
        FindBodyTagFacts facts,
        string source,
        IReadOnlyList<string> expectedTags)
    {
        Assert.Equal(FindBodyTagAvailability.Complete, facts.Availability);
        Assert.Equal(expectedTags, facts.Occurrences.Select(occurrence => occurrence.Authored));
        Assert.Equal(
            expectedTags.Select(tag => ExpectedAuthoredTagLocation(source, tag)),
            facts.Occurrences.Select(occurrence => occurrence.Location));
    }

    private static void AssertVisibleContent(
        MarkdownDocumentFacts document,
        string source,
        string visibleText,
        params string[] excludedText)
    {
        Assert.Contains(document.VisibleText, fact =>
            source[fact.Span.Start..fact.Span.End].Contains(visibleText, StringComparison.Ordinal));
        foreach (var excluded in excludedText)
        {
            Assert.DoesNotContain(document.VisibleText, fact =>
                source[fact.Span.Start..fact.Span.End].Contains(excluded, StringComparison.Ordinal));
        }
    }

    private static void AssertIncompleteInspection(
        FindLayerInspectionFacts facts,
        SourceLogicalSource source,
        FindFindingCode expectedCode)
    {
        Assert.Same(source, facts.Source);
        Assert.Same(source.Base, facts.Layer);
        Assert.Null(facts.Document);
        Assert.Null(facts.Frontmatter);
        Assert.Null(facts.BodyTags);
        Assert.Null(facts.Description);
        var finding = Assert.Single(facts.Findings);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(CliSemanticStatus.Incomplete, finding.Status);
        var findingSource = Assert.IsType<FindSourceIdentity>(finding.Source);
        Assert.Equal(source.Identity.AutomaticId, findingSource.Id);
        Assert.Equal(source.Identity.CanonicalBasePath, findingSource.Path);
        Assert.Equal(SourceLayerKind.Base, finding.Layer);
        Assert.Equal(source.Base.CanonicalPath, finding.Path);
        Assert.Null(finding.Region);
        Assert.Null(finding.Location);
        Assert.NotEmpty(finding.Cause);
        Assert.Empty(finding.Candidates);
    }

    private static SourceLocation ExpectedAuthoredTagLocation(string source, string tag)
    {
        var marker = $"#{tag}";
        var markerStart = source.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(markerStart >= 0, $"The source does not contain the body tag '{marker}'.");
        return ExpectedLocation(source, markerStart + 1, tag.Length);
    }

    private static SourceLocation ExpectedLocation(string source, string token)
    {
        var start = source.IndexOf(token, StringComparison.Ordinal);
        Assert.True(start >= 0, $"The source does not contain the expected token '{token}'.");
        return ExpectedLocation(source, start, token.Length);
    }

    private static SourceLocation ExpectedLocation(string source, int start, int length)
    {
        var line = 1;
        var column = 1;
        for (var index = 0; index < start;)
        {
            if (source[index] == '\r')
            {
                index += index + 1 < source.Length && source[index + 1] == '\n' ? 2 : 1;
                line++;
                column = 1;
                continue;
            }

            if (source[index] == '\n')
            {
                index++;
                line++;
                column = 1;
                continue;
            }

            index += char.IsHighSurrogate(source[index]) ? 2 : 1;
            column++;
        }

        return new SourceLocation(
            line,
            column,
            Encoding.UTF8.GetByteCount(source.AsSpan(0, start)),
            Encoding.UTF8.GetByteCount(source.AsSpan(start, length)));
    }

    private static string WithCrLf(string source)
        => source.Replace("\n", "\r\n", StringComparison.Ordinal);
}

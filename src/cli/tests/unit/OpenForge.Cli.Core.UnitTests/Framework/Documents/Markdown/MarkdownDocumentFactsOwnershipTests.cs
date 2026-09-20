using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class MarkdownDocumentFactsOwnershipTests
{
    private const string Source = "# H\ntext `c` [L](x) ![I](y)\n";

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown document facts isolate all six collections from input and intermediate aliases"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void DocumentFactsOwnTheirCollections()
    {
        var heading = Heading();
        var section = new MarkdownSectionFact(heading, new MarkdownTextSpan(0, 28));
        var visible = new MarkdownVisibleTextFact(new MarkdownTextSpan(4, 4));
        var opaque = new MarkdownOpaqueSpan(new MarkdownTextSpan(9, 3), isCode: true);
        var link = Link(isImage: false);
        var image = Link(isImage: true);
        MarkdownHeadingFact[] headings = [heading];
        MarkdownSectionFact[] sections = [section];
        MarkdownVisibleTextFact[] visibleText = [visible];
        MarkdownOpaqueSpan[] opaqueSpans = [opaque];
        MarkdownLinkFact[] links = [link];
        MarkdownLinkFact[] images = [image];
        var structure = Structure(headings, sections);
        var inline = new MarkdownInlineFacts(visibleText, opaqueSpans, links, images);

        headings[0] = null!;
        sections[0] = null!;
        visibleText[0] = null!;
        opaqueSpans[0] = null!;
        links[0] = null!;
        images[0] = null!;
        var facts = new MarkdownDocumentFacts(Source, structure, inline);

        ReplaceIfWritable(structure.Headings);
        ReplaceIfWritable(structure.Sections);
        ReplaceIfWritable(inline.VisibleText);
        ReplaceIfWritable(inline.OpaqueSpans);
        ReplaceIfWritable(inline.Links);
        ReplaceIfWritable(inline.Images);
        ReplaceIfWritable(facts.Headings);
        ReplaceIfWritable(facts.Sections);
        ReplaceIfWritable(facts.VisibleText);
        ReplaceIfWritable(facts.OpaqueSpans);
        ReplaceIfWritable(facts.Links);
        ReplaceIfWritable(facts.Images);

        Assert.Equal(Source, facts.Source);
        Assert.Equal(new MarkdownTextSpan(0, 28), facts.BodySpan);
        Assert.Same(heading, Assert.Single(facts.Headings));
        Assert.Same(section, Assert.Single(facts.Sections));
        Assert.Same(heading, facts.Sections[0].Heading);
        Assert.Same(visible, Assert.Single(facts.VisibleText));
        Assert.Same(opaque, Assert.Single(facts.OpaqueSpans));
        Assert.Same(link, Assert.Single(facts.Links));
        Assert.Same(image, Assert.Single(facts.Images));
        Assert.False(facts.Links[0].IsImage);
        Assert.True(facts.Images[0].IsImage);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown collection inputs are consumed at ingress once and in declared order"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void InputEnumerationRemainsAtIngress()
    {
        var trace = new List<string>();
        var heading = Heading();
        var section = new MarkdownSectionFact(heading, new MarkdownTextSpan(0, 28));
        var structure = Structure(
            Observe([heading], "headings", trace),
            Observe([section], "sections", trace));
        Assert.Equal(["headings", "sections"], trace);
        var inline = new MarkdownInlineFacts(
            Observe<MarkdownVisibleTextFact>([], "visible", trace),
            Observe<MarkdownOpaqueSpan>([], "opaque", trace),
            Observe<MarkdownLinkFact>([], "links", trace),
            Observe<MarkdownLinkFact>([], "images", trace));
        Assert.Equal(["headings", "sections", "visible", "opaque", "links", "images"], trace);

        var facts = new MarkdownDocumentFacts(Source, structure, inline);

        Assert.Same(heading, Assert.Single(facts.Headings));
        Assert.Same(section, Assert.Single(facts.Sections));
        Assert.Equal(["headings", "sections", "visible", "opaque", "links", "images"], trace);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown collection argument validation precedes input enumeration"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void MissingLaterCollectionDoesNotConsumeEarlierInput()
    {
        var trace = new List<string>();

        var structureException = Assert.Throws<ArgumentNullException>(() => Structure(
            Observe<MarkdownHeadingFact>([], "headings", trace),
            null!));
        var inlineException = Assert.Throws<ArgumentNullException>(() => new MarkdownInlineFacts(
            Observe<MarkdownVisibleTextFact>([], "visible", trace),
            [],
            [],
            null!));

        Assert.Equal("sections", structureException.ParamName);
        Assert.Equal("images", inlineException.ParamName);
        Assert.Empty(trace);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown structure stops consuming inputs when the first collection throws"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void StructureEnumerationFailureStopsLaterInput()
    {
        var trace = new List<string>();
        var failure = new InvalidOperationException("heading enumeration failed");

        var actual = Assert.Throws<InvalidOperationException>(() => Structure(
            Observe<MarkdownHeadingFact>([], "headings", trace, failure),
            Observe<MarkdownSectionFact>([], "sections", trace)));

        Assert.Same(failure, actual);
        Assert.Equal(["headings"], trace);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown inline inputs retain earlier consumption and stop after a throwing collection"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void InlineEnumerationFailureStopsLaterInput()
    {
        var trace = new List<string>();
        var failure = new InvalidOperationException("link enumeration failed");

        var actual = Assert.Throws<InvalidOperationException>(() => new MarkdownInlineFacts(
            Observe<MarkdownVisibleTextFact>([], "visible", trace),
            Observe<MarkdownOpaqueSpan>([], "opaque", trace),
            Observe<MarkdownLinkFact>([], "links", trace, failure),
            Observe<MarkdownLinkFact>([], "images", trace)));

        Assert.Same(failure, actual);
        Assert.Equal(["visible", "opaque", "links"], trace);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Markdown null members are rejected at final construction before resource or span failures"),
        InlineData("headings"), InlineData("sections"), InlineData("visible"),
        InlineData("opaque"), InlineData("links"), InlineData("images")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void NullMembersPrecedeOtherDocumentValidation(string collection)
    {
        var heading = Heading();
        var section = new MarkdownSectionFact(heading, new MarkdownTextSpan(0, 28));
        var structure = Structure(
            [collection == "headings" ? null! : heading],
            [collection == "sections" ? null! : section],
            new MarkdownTextSpan(0, 29));
        var inline = new MarkdownInlineFacts(
            [collection == "visible" ? null! : new MarkdownVisibleTextFact(new MarkdownTextSpan(4, 4))],
            [collection == "opaque" ? null! : new MarkdownOpaqueSpan(new MarkdownTextSpan(9, 3))],
            [collection == "links" ? null! : Link(isImage: true)],
            [collection == "images" ? null! : Link(isImage: false)]);

        AssertFailure(
            () => new MarkdownDocumentFacts(Source, structure, inline),
            "Markdown facts cannot contain null members.",
            parameterName: null);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Markdown resource kinds are checked before document structure"), InlineData(true), InlineData(false)]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void ResourceKindPrecedesStructureValidation(bool invalidLinks)
    {
        var structure = Structure([], [], new MarkdownTextSpan(0, 29));
        var inline = new MarkdownInlineFacts(
            [],
            [],
            invalidLinks ? [Link(isImage: true)] : [],
            invalidLinks ? [] : [Link(isImage: false)]);

        AssertFailure(
            () => new MarkdownDocumentFacts(Source, structure, inline),
            "Markdown link and image facts must retain their exact resource kind.",
            "inlineFacts");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown structure span failures precede inline span failures"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void StructureValidationPrecedesInlineValidation()
    {
        var structure = Structure([], [], new MarkdownTextSpan(0, 29));
        var inline = new MarkdownInlineFacts([new MarkdownVisibleTextFact(new MarkdownTextSpan(29, 1))], [], [], []);

        AssertFailure(
            () => new MarkdownDocumentFacts(Source, structure, inline),
            "A Markdown span must be contained by its source.",
            "structure");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown heading order is checked before section count"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void HeadingOrderPrecedesSectionValidation()
    {
        var structure = Structure([Heading(start: 4), Heading()], []);

        AssertFailure(
            () => new MarkdownDocumentFacts(Source, structure, new MarkdownInlineFacts([], [], [], [])),
            "Markdown headings must retain source order.",
            "headings");
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Markdown sections retain their heading and exact end boundary"), InlineData(true), InlineData(false)]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void SectionIdentityAndBoundaryRemainValidated(bool mismatchedHeading)
    {
        var heading = Heading();
        var section = new MarkdownSectionFact(
            mismatchedHeading ? Heading(text: "Other") : heading,
            new MarkdownTextSpan(0, mismatchedHeading ? 28 : 27));
        var structure = Structure([heading], [section]);

        AssertFailure(
            () => new MarkdownDocumentFacts(Source, structure, new MarkdownInlineFacts([], [], [], [])),
            mismatchedHeading
                ? "Markdown sections must retain heading order and identity."
                : "A Markdown section must end at its exact heading boundary.",
            "sections");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown inline validation retains its facts parameter after structural admission"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void InlineSpanFailureRetainsItsParameter()
    {
        var inline = new MarkdownInlineFacts([new MarkdownVisibleTextFact(new MarkdownTextSpan(29, 1))], [], [], []);

        AssertFailure(
            () => new MarkdownDocumentFacts(Source, Structure([], []), inline),
            "A Markdown span must be contained by its source.",
            "facts");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Markdown inline facts retain source order after valid span admission"), Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void InlineOrderRemainsValidated()
    {
        var inline = new MarkdownInlineFacts(
            [new MarkdownVisibleTextFact(new MarkdownTextSpan(8, 1)), new MarkdownVisibleTextFact(new MarkdownTextSpan(4, 1))],
            [],
            [],
            []);

        AssertFailure(
            () => new MarkdownDocumentFacts(Source, Structure([], []), inline),
            "Markdown visible text must retain source order.",
            "spans");
    }

    private static MarkdownDocumentStructure Structure(
        IEnumerable<MarkdownHeadingFact> headings,
        IEnumerable<MarkdownSectionFact> sections,
        MarkdownTextSpan? bodySpan = null)
        => new(
            frontmatter: new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Missing, null, null, bodyStart: 0),
            bodySpan: bodySpan ?? new MarkdownTextSpan(0, 28),
            headings: headings,
            sections: sections,
            generatedRegion: MarkdownGeneratedRegionFact.Absent());

    private static MarkdownHeadingFact Heading(string text = "H", int start = 0)
        => new(
            visibleText: text,
            level: 1,
            form: MarkdownHeadingForm.Atx,
            isCanonical: true,
            fragmentIdentifier: "h",
            span: new MarkdownTextSpan(start, 3));

    private static MarkdownLinkFact Link(bool isImage)
        => new(
            syntax: new MarkdownLinkSyntax(MarkdownLinkForm.Inline, isImage),
            rawDestination: isImage ? "y" : "x",
            span: isImage ? new MarkdownTextSpan(20, 7) : new MarkdownTextSpan(13, 6),
            destinationSpan: isImage ? new MarkdownTextSpan(25, 1) : new MarkdownTextSpan(17, 1),
            label: MarkdownLinkLabelFact.Supported(isImage ? "I" : "L"));

    private static void ReplaceIfWritable<T>(IReadOnlyList<T> values)
        where T : class
    {
        if (values is not IList<T> indexed)
        {
            return;
        }

        try
        {
            indexed[0] = null!;
        }
        catch (NotSupportedException)
        {
        }
    }

    private static IEnumerable<T> Observe<T>(
        IEnumerable<T> values,
        string name,
        List<string> trace,
        Exception? failure = null)
    {
        trace.Add(name);
        if (failure is not null)
        {
            throw failure;
        }

        foreach (var value in values)
        {
            yield return value;
        }
    }

    private static void AssertFailure(Action action, string message, string? parameterName)
    {
        var exception = Assert.Throws<ArgumentException>(action);

        if (parameterName is null)
        {
            Assert.Equal(message, exception.Message);
        }
        else
        {
            Assert.StartsWith(message, exception.Message, StringComparison.Ordinal);
        }
        Assert.Equal(parameterName, exception.ParamName);
    }
}

using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown.Shared.ManagedHosts;

public sealed class MarkdownManagedHostReaderTests
{
    [Theory(DisplayName = "Canonical managed hosts retain exact spans and leave the following section outside")]
    [InlineData("\n", "**"), InlineData("\r\n", "__")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void CanonicalSpanPreservesSurroundings(string newline, string delimiter)
    {
        var prefix = $"\uFEFFPréface 🛠{newline}{newline}";
        var host = $"# Open Forge{newline}{newline}Read café instructions.{newline}{newline}{delimiter}End of Open Forge managed section.{delimiter}{newline}";
        var suffix = $"{newline}## Authored section{newline}Keep this.{newline}";
        var document = new MarkdownDocumentParser().Parse($"{prefix}{host}{suffix}");

        Assert.Equal(MarkdownManagedHostState.Present, document.ManagedHost.State);
        Assert.False(document.ManagedHost.IsLegacy);
        var span = Assert.IsType<MarkdownTextSpan>(document.ManagedHost.Span);
        Assert.Equal(new MarkdownTextSpan(prefix.Length, host.Length), span);
        Assert.Equal(host, document.Source[span.Start..span.End]);
    }

    [Theory(DisplayName = "Soft-wrapped strong footers preserve exact managed spans")]
    [InlineData("\n", "**"), InlineData("\r\n", "**")]
    [InlineData("\n", "__"), InlineData("\r\n", "__")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void SoftWrappedFooterPreservesSpan(string newline, string delimiter)
    {
        var prefix = $"\uFEFFAuthored prefix{newline}{newline}";
        var host = $"# Open Forge{newline}{newline}Instructions.{newline}{newline}{delimiter}End of Open Forge managed{newline}section.{delimiter}{newline}";
        var suffix = $"{newline}## Authored section{newline}Keep this.{newline}";
        var document = new MarkdownDocumentParser().Parse($"{prefix}{host}{suffix}");

        Assert.Equal(MarkdownManagedHostState.Present, document.ManagedHost.State);
        Assert.False(document.ManagedHost.IsLegacy);
        var span = Assert.IsType<MarkdownTextSpan>(document.ManagedHost.Span);
        Assert.Equal(new MarkdownTextSpan(prefix.Length, host.Length), span);
        Assert.Equal(host, document.Source[span.Start..span.End]);
        Assert.Equal(suffix, document.Source[span.End..]);
    }

    [Theory(DisplayName = "Hard-wrapped strong footers cannot close a managed host")]
    [InlineData("\n", "  "), InlineData("\r\n", "  ")]
    [InlineData("\n", "\\"), InlineData("\r\n", "\\")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void HardWrappedFooterIsRejected(string newline, string hardBreak)
    {
        var source = $"# Open Forge{newline}{newline}**End of Open Forge managed{hardBreak}{newline}section.**{newline}";
        var fact = new MarkdownDocumentParser().Parse(source).ManagedHost;

        Assert.Equal(MarkdownManagedHostState.Invalid, fact.State);
        Assert.Null(fact.Span);
    }

    [Theory(DisplayName = "Structured inline footer text cannot close a managed host")]
    [InlineData("End of Open Forge `managed` section.")]
    [InlineData("End of Open Forge *managed* section.")]
    [InlineData("End of Open Forge <span>managed</span> section.")]
    [InlineData("End of Open Forge [managed](target.md) section.")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void StructuredFooterTextIsRejected(string footer)
    {
        var fact = new MarkdownDocumentParser().Parse($"# Open Forge\n\n**{footer}**\n").ManagedHost;

        Assert.Equal(MarkdownManagedHostState.Invalid, fact.State);
        Assert.Null(fact.Span);
    }

    [Theory(DisplayName = "Legacy comments pair with adjacent lines and formatter blank lines")]
    [InlineData(""), InlineData("\n"), InlineData("\n# Open Forge\n"), InlineData("\n\n# Open Forge\n\n")]
    [InlineData("# Open Forge\n\n# Open Forge\n\n")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void LegacyPairRetainsInternalHeading(string body)
    {
        var host = $"<!-- open-forge:start -->\n{body}<!-- open-forge:end -->\n";
        var document = new MarkdownDocumentParser().Parse($"\uFEFF{host}\n## Keep\nAuthored.");

        Assert.Equal(MarkdownManagedHostState.Present, document.ManagedHost.State);
        Assert.True(document.ManagedHost.IsLegacy);
        Assert.Equal(new MarkdownTextSpan(1, host.Length), document.ManagedHost.Span);
    }

    [Theory(DisplayName = "Malformed or ambiguous authentic managed boundaries are invalid")]
    [InlineData("# Open Forge")]
    [InlineData("**End of Open Forge managed section.**")]
    [InlineData("**End of Open Forge managed section.**\n\n# Open Forge")]
    [InlineData("# Open Forge\n\n# Open Forge\n\n**End of Open Forge managed section.**")]
    [InlineData("# Open Forge\n\n**End of Open Forge managed section.**\n\n**End of Open Forge managed section.**")]
    [InlineData("# Open Forge\n\n# Authored\n\n**End of Open Forge managed section.**")]
    [InlineData("<!-- open-forge:start -->")]
    [InlineData("<!-- open-forge:end -->")]
    [InlineData("<!-- open-forge:end -->\n<!-- open-forge:start -->")]
    [InlineData("<!-- open-forge:start -->\n<!-- open-forge:start -->\n<!-- open-forge:end -->")]
    [InlineData("<!-- open-forge:start -->\n<!-- open-forge:end -->\n<!-- open-forge:end -->")]
    [InlineData("<!-- open-forge:start -->\n# Open Forge\n\n**End of Open Forge managed section.**\n\n<!-- open-forge:end -->")]
    [InlineData("# Open Forge\n\n<!-- open-forge:start -->\n<!-- open-forge:end -->")]
    [InlineData("<!-- open-forge:start -->\n<!-- open-forge:end -->\n\n# Open Forge")]
    [InlineData("<!-- open-forge:start -->\n# Open Forge\n\n<!-- open-forge:end -->\n\n# Open Forge\n\n**End of Open Forge managed section.**")]
    [InlineData("# Open Forge\n\n**End of Open Forge managed section.** trailing text")]
    [InlineData("# Open Forge\n\n**End of Open Forge managed section.**\nMore in the same paragraph")]
    [InlineData("---\nunterminated frontmatter")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void AuthenticInvalidBoundariesAreBlocked(string source)
    {
        var fact = new MarkdownDocumentParser().Parse(source).ManagedHost;

        Assert.Equal(MarkdownManagedHostState.Invalid, fact.State);
        Assert.Null(fact.Span);
        Assert.False(string.IsNullOrWhiteSpace(fact.Cause));
    }

    [Theory(DisplayName = "Examples and nonboundary structures cannot claim a managed host")]
    [InlineData("```md\n# Open Forge\n\n**End of Open Forge managed section.**\n<!-- open-forge:start -->\n<!-- open-forge:end -->\n```\n")]
    [InlineData("> # Open Forge\n>\n> **End of Open Forge managed section.**\n>\n> <!-- open-forge:start -->\n> <!-- open-forge:end -->\n")]
    [InlineData("- # Open Forge\n\n  **End of Open Forge managed section.**\n\n  <!-- open-forge:start -->\n  <!-- open-forge:end -->\n")]
    [InlineData("Use `# Open Forge` and `**End of Open Forge managed section.**`, `<!-- open-forge:start -->`, `<!-- open-forge:end -->`.")]
    [InlineData("Text <!-- open-forge:start --> and <!-- open-forge:end -->.")]
    [InlineData("    # Open Forge\n\n    **End of Open Forge managed section.**\n    <!-- open-forge:start -->\n    <!-- open-forge:end -->")]
    [InlineData("# **Open Forge**")]
    [InlineData("Open Forge\n==========")]
    [InlineData("Text **End of Open Forge managed section.**")]
    [InlineData("**End of Open Forge managed section.** trailing text")]
    [InlineData("*End of Open Forge managed section.*")]
    [InlineData("***End of Open Forge managed section.***")]
    [InlineData("**End of Open Forge managed section.**\nMore in the same paragraph")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ExamplesAreAbsent(string source)
    {
        Assert.Equal(MarkdownManagedHostState.Absent, new MarkdownDocumentParser().Parse(source).ManagedHost.State);
    }

    [Fact(DisplayName = "Boundary examples inside a canonical host do not duplicate its real boundaries")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ExamplesWithinHostAreIgnored()
    {
        const string source = """
            # Open Forge

            ```md
            # Open Forge
            <!-- open-forge:start -->
            <!-- open-forge:end -->
            **End of Open Forge managed section.**
            ```

            **End of Open Forge managed section.**
            """;
        var fact = new MarkdownDocumentParser().Parse(source).ManagedHost;

        Assert.Equal(MarkdownManagedHostState.Present, fact.State);
        Assert.Equal(new MarkdownTextSpan(0, source.Length), fact.Span);
    }

    [Fact(DisplayName = "Existing document-fact construction defaults to an absent managed host")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ExistingConstructionDefaultsToAbsent()
    {
        var parsed = new MarkdownDocumentParser().Parse("");
        var facts = new MarkdownDocumentFacts(
            "",
            new MarkdownDocumentStructure(parsed.Frontmatter, parsed.BodySpan, parsed.Headings, parsed.Sections, parsed.GeneratedRegion),
            new MarkdownInlineFacts([], [], [], []));

        Assert.Equal(MarkdownManagedHostState.Absent, facts.ManagedHost.State);
    }
}

using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata.Shared.Completion;

[Trait("Feature", "metadata-completion"), Trait("Evidence", "Unit")]
public sealed class FrameworkMetadataCompletionTests
{
    [Fact(DisplayName = "Malformed metadata inspection performs no completion parse and agrees with completion")]
    public void MalformedInspectionDoesNotComplete()
    {
        var parses = 0;
        var completer = new FrameworkMetadataCompleter(text =>
        {
            parses++;
            return new MarkdownDocumentParser().Parse(text);
        });
        foreach (var source in new[] { "---\nlegacy: value\n---\n# Note\n", "---\ntags: []\n---\n# Note\n", "---\nunclosed\n", "# Note\n" })
        {
            var document = new MarkdownDocumentParser().Parse(source);
            var expected = new FrameworkMetadataCompleter().Complete(Input(source));
            Assert.Equal(expected.State == FrameworkMetadataCompletionState.Malformed ? expected.Cause : null,
                completer.ReadMalformedCause(document));
        }
        Assert.Equal(0, parses);
    }

    [Fact(DisplayName = "Completion parses identical fresh content once across validation passes and notices changed content")]
    public void ReusesContentParsingAcrossFreshPasses()
    {
        const string source = "---\ndescription: Existing\ntags: [Guidance]\n---\n# Note\n";
        var parses = 0;
        var completer = new FrameworkMetadataCompleter(text =>
        {
            parses++;
            return new MarkdownDocumentParser().Parse(text);
        });

        var first = completer.Complete(Input(source));
        var second = completer.Complete(Input(source, path: ".agents/guidance/other.md"));
        Assert.Equal(1, parses);
        Assert.Same(first.Document, second.Document);

        var changed = completer.Complete(Input(source.Replace("# Note", "# Changed", StringComparison.Ordinal)));
        Assert.Equal(2, parses);
        Assert.NotSame(first.Document, changed.Document);
        Assert.Equal("Changed", Assert.Single(changed.Document?.Headings ?? []).VisibleText);
    }

    [Theory(DisplayName = "Completion retains its parsed output without requiring a second caller parse")]
    [InlineData("---\ndescription: Existing\ntags: [Guidance]\n---\n# Note\n", 1)]
    [InlineData("# Note\n", 2)]
    public void RetainsParsedDocument(string source, int expectedParses)
    {
        var parses = 0;
        MarkdownDocumentFacts? lastDocument = null;
        var completer = new FrameworkMetadataCompleter(text =>
        {
            parses++;
            lastDocument = new MarkdownDocumentParser().Parse(text);
            return lastDocument;
        });

        var result = completer.Complete(Input(source));

        Assert.Equal(expectedParses, parses);
        Assert.NotNull(lastDocument);
        var retained = Assert.IsType<MarkdownDocumentFacts>(result.Document);
        Assert.Same(lastDocument, retained);
        Assert.Equal(Encoding.UTF8.GetString(result.Bytes.AsSpan()), retained.Source);
    }

    [Theory(DisplayName = "Completion derives visible root headings at every level before the filename")]
    [InlineData("# **Deploy**  [checklist](target.md) &amp; `verify`\n", "Deploy checklist & verify")]
    [InlineData("> # Quoted\n\n- ## Listed\n\n```md\n# Code\n```\n\n### First   root\n\n# Later\n", "First root")]
    [InlineData("Setext heading\n===\n", "Setext heading")]
    [InlineData("#\n\n###### Last level\n", "Last level")]
    public void DerivesHeading(string body, string expected)
        => Assert.Equal(expected, Complete(body).Description);

    [Fact(DisplayName = "Unsupported heading text falls back to the filename rather than partial visible text")]
    public void UnsupportedHeadingUsesStem()
        => Assert.Equal("Note", Complete("# Visible <br> words\n\n# Later\n").Description);

    [Theory(DisplayName = "Headingless completion derives a sentence case stem with a neutral fallback")]
    [InlineData("deploy-checklist.md", "Deploy checklist")]
    [InlineData("moneyRounding.md", "Money rounding")]
    [InlineData("HTTPServer_check LIST.md", "Http server check list")]
    [InlineData("123-check.md", "123 Check")]
    [InlineData("---__!.md", "Note")]
    public void DerivesStem(string name, string expected)
        => Assert.Equal(expected, Complete("Body", path: $".agents/guidance/{name}").Description);

    [Theory(DisplayName = "New headers preserve original bytes after the BOM using the first newline convention")]
    [InlineData("Root", "\n", false)]
    [InlineData("Root", "\r\n", true)]
    [InlineData("Scoped", "\n", true)]
    [InlineData("Scoped", "\r\n", false)]
    public void InsertsLeadingBlock(string form, string newline, bool bom)
    {
        var prefix = bom ? "\uFEFF" : "";
        var body = $"# Deploy checklist{newline}Keep café and 💰.\n";
        var fields = form == "Root"
            ? $"description: Deploy checklist{newline}tags: [Guidance]{newline}"
            : $"open-forge:{newline}  description: Deploy checklist{newline}  tags: [Guidance]{newline}";
        var result = Complete(prefix + body, form);
        Assert.Equal(FrameworkMetadataCompletionState.Completed, result.State);
        Assert.Equal(Encoding.UTF8.GetBytes($"{prefix}---{newline}{fields}---{newline}{newline}{body}"), result.Bytes.ToArray());
        Assert.Equal([FrameworkMetadataField.Description, FrameworkMetadataField.Tags], result.Fields);
        Assert.Equal(FrameworkMetadataCompletionState.NoOp, Complete(Encoding.UTF8.GetString(result.Bytes.AsSpan()), form).State);
    }

    [Theory(DisplayName = "Partial mappings preserve supplied fields comments indentation and the existing declaration form")]
    [InlineData("Root", "Scoped", "description: 'Authored words' # keep\ncustom: true\n", "tags: [Guidance]\n")]
    [InlineData("Scoped", "Root", "open-forge:\n    tags: [Guidance, Team] # keep\ncustom: true\n", "    description: Note\n")]
    [InlineData("Root", "Root", "{description: 'Authored words', custom: true} # keep\n", "tags: [Guidance], ")]
    [InlineData("Scoped", "Scoped", "open-forge: {tags: [Guidance, Team]} # keep\ncustom: true\n", "description: \"Note\", ")]
    [InlineData("Scoped", "Root", "{open-forge: {description: 'Authored words'}, custom: true}\n", "tags: [Guidance], ")]
    public void CompletesPartial(string authoredForm, string workspaceForm, string yaml, string insertion)
    {
        var before = $"---\n{yaml}---\nBody café\n";
        var result = Complete(before, workspaceForm);
        Assert.Equal(FrameworkMetadataCompletionState.Completed, result.State);
        var after = Encoding.UTF8.GetString(result.Bytes.AsSpan());
        var insertionStart = after.IndexOf(insertion, StringComparison.Ordinal);
        Assert.True(insertionStart >= 0);
        Assert.Equal(before, after.Remove(insertionStart, insertion.Length));
        Assert.Equal(Enum.Parse<FrontmatterForm>(authoredForm), result.Form);
        Assert.Single(result.Fields);
        Assert.Equal(FrameworkMetadataCompletionState.NoOp, Complete(after, workspaceForm).State);
    }

    [Theory(DisplayName = "Supplied empty null duplicate wrong type and conflicting metadata are skipped without changes")]
    [InlineData("description: ''\n")]
    [InlineData("description: null\n")]
    [InlineData("description: ~\n")]
    [InlineData("description: [wrong]\n")]
    [InlineData("description: !!int 12\n")]
    [InlineData("description: one\ndescription: two\n")]
    [InlineData("tags: []\n")]
    [InlineData("tags: null\n")]
    [InlineData("tags: Guidance\n")]
    [InlineData("tags: [Guidance, Guidance]\n")]
    [InlineData("tags: [null]\n")]
    [InlineData("open-forge: null\n")]
    [InlineData("open-forge: []\n")]
    [InlineData("open-forge: *unknown\n")]
    [InlineData("open-forge: {}\nopen-forge: {}\n")]
    [InlineData("responsibility: ''\n")]
    [InlineData("applyTo: ['src/**']\nopen-forge: {applyTo: ['test/**']}\n")]
    [InlineData("description: [unclosed\n")]
    public void SkipsMalformed(string yaml)
    {
        var before = $"---\n{yaml}---\n# Body\n";
        var result = Complete(before);
        Assert.Equal(FrameworkMetadataCompletionState.Malformed, result.State);
        Assert.Equal(Encoding.UTF8.GetBytes(before), result.Bytes.ToArray());
        Assert.Empty(result.Fields);
        Assert.NotNull(result.Cause);
    }

    [Fact(DisplayName = "Complete supplied fields produce an exact no-op")]
    public void CompleteMetadataIsUnchanged()
    {
        const string before = "---\ndescription: 'Keep me' # exact\ntags: [Team, Guidance]\n---\n# Different heading\n";
        var result = Complete(before);
        Assert.Equal(FrameworkMetadataCompletionState.NoOp, result.State);
        Assert.Equal(Encoding.UTF8.GetBytes(before), result.Bytes.ToArray());
        Assert.Null(result.Description);
        Assert.Null(result.Tags);
    }

    [Fact(DisplayName = "Valid mappings without an additive standalone boundary are unsafe")]
    public void RefusesUnsafeBoundary()
    {
        const string before = "---\n? description\n: Authored words\n---\nBody\n";
        var result = Complete(before);
        Assert.Equal(FrameworkMetadataCompletionState.Unsafe, result.State);
        Assert.Equal(Encoding.UTF8.GetBytes(before), result.Bytes.ToArray());
    }

    [Fact(DisplayName = "Invalid UTF-8 bytes have no safe insertion boundary")]
    public void RefusesInvalidEncoding()
    {
        var input = Input("Body") with { Bytes = [0xff] };
        var result = new FrameworkMetadataCompleter().Complete(input);
        Assert.Equal(FrameworkMetadataCompletionState.Unsafe, result.State);
        Assert.Equal(input.Bytes, result.Bytes);
    }

    [Theory(DisplayName = "Completion preserves dual equivalent applyTo fields and uses scoped precedence")]
    [InlineData("applyTo: ['src/**']\nopen-forge: {applyTo: ['src/**'], tags: [Team]}\n")]
    [InlineData("description: Foreign words\ntags: [Foreign]\nopen-forge: {tags: [Team]}\n")]
    [InlineData("description: Keep\napplyTo: ['src/**']\n")]
    public void PreservesApplicabilityAndPrecedence(string yaml)
    {
        var source = $"---\n{yaml}---\n# Note\n";
        var result = Complete(source);
        Assert.Equal(FrameworkMetadataCompletionState.Completed, result.State);
        var after = Encoding.UTF8.GetString(result.Bytes.AsSpan());
        Assert.Contains(yaml.Replace("open-forge: {", "open-forge: {description: \"Note\", ", StringComparison.Ordinal), after, StringComparison.Ordinal);
    }

    private static FrameworkMetadataCompletionResult Complete(string source, string form = "Scoped", string path = ".agents/guidance/note.md")
        => new FrameworkMetadataCompleter().Complete(Input(source, form, path));

    private static FrameworkMetadataCompletionInput Input(string source, string form = "Scoped", string path = ".agents/guidance/note.md")
        => new()
        {
            Bytes = [.. Encoding.UTF8.GetBytes(source)],
            CanonicalPath = path,
            Form = Enum.Parse<FrontmatterForm>(form),
            DefaultTags = ["Guidance"],
        };
}

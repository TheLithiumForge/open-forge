using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.WorkspaceAdoption;

public sealed class WorkspaceAdoptionDocumentPlannerTests
{
    private const string SkillPath = ".agents/skills/sample-skill/SKILL.md";
    private const string EntrypointPath = ".agents/workspace/_workspace.md";

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption creates native Skill metadata from heading without changing its body")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void AddsMissingNativeSkillFrontmatterFromHeading()
    {
        const string source = "# Local Skill\n\nRead [Reference](references/guide.md).\n";
        const string expected = "---\n"
            + "name: \"sample-skill\"\n"
            + "description: \"Local Skill\"\n"
            + "---\n\n"
            + "# Local Skill\n\nRead [Reference](references/guide.md).\n";

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.NotNull(plan.IntendedBytes);
        Assert.Equal(expected, Decode(plan.IntendedBytes));
        Assert.Equal([WorkspaceAdoptionAction.MetadataCompleted], plan.Actions);
        Assert.Equal(["name", "description"], plan.Fields);
        Assert.Collection(
            plan.Derivation,
            value => Assert.Equal(
                new WorkspaceAdoptionDerivation("name", WorkspaceAdoptionDerivationSource.DirectoryName),
                value),
            value => Assert.Equal(
                new WorkspaceAdoptionDerivation("description", WorkspaceAdoptionDerivationSource.Heading),
                value));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption adds only the missing native Skill description")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void CompletesPartialNativeSkillWithoutChangingExistingNameOrUnknownYaml()
    {
        const string source = "---\nname: sample-skill\nlicense: MIT\n---\n# Local skill\n";

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.NotNull(plan.IntendedBytes);
        var updated = Decode(plan.IntendedBytes);
        Assert.StartsWith("---\nname: sample-skill\nlicense: MIT\ndescription: \"Local skill\"\n---\n", updated);
        Assert.EndsWith("# Local skill\n", updated);
        Assert.Equal(["description"], plan.Fields);
        Assert.Equal(
            new WorkspaceAdoptionDerivation("description", WorkspaceAdoptionDerivationSource.Heading),
            Assert.Single(plan.Derivation));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Complete native Skill metadata returns identical bytes with unknown YAML intact")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void LeavesCompleteNativeSkillBytesUnchanged()
    {
        const string source = "---\n"
            + "name: sample-skill\n"
            + "description: &label 'Preserved description'\n"
            + "license: MIT\n"
            + "summary: *label\n"
            + "---\n# Local skill\n";
        var bytes = Encode(source);

        var plan = new WorkspaceAdoptionDocumentPlanner().Plan(
            SkillPath,
            SourceDocumentForm.Skill,
            bytes,
            ensureEntries: false);

        Assert.Equal(bytes, plan.IntendedBytes);
        Assert.Empty(plan.Actions);
        Assert.Empty(plan.Fields);
        Assert.Null(plan.Cause);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption keeps a UTF-8 BOM, CRLF endings, and the original body bytes")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void PreservesBomCrLfAndOriginalBodyBytes()
    {
        const string body = "# Local Skill\r\n\r\nRead résumé — the original body.\r\n";
        var bytes = Encode("\uFEFF" + body);

        var plan = new WorkspaceAdoptionDocumentPlanner().Plan(
            SkillPath,
            SourceDocumentForm.Skill,
            bytes,
            ensureEntries: false);

        Assert.NotNull(plan.IntendedBytes);
        var intendedBytes = Assert.IsType<byte[]>(plan.IntendedBytes);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, intendedBytes[..3]);
        var bodyBytes = Encode(body);
        Assert.Equal(bodyBytes, intendedBytes[^bodyBytes.Length..]);
        Assert.Contains("\r\nname: \"sample-skill\"\r\n", Decode(intendedBytes));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption fills a missing native Skill name and preserves its description")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void AddsMissingNameWithoutReplacingExistingDescription()
    {
        const string source = "---\ndescription: \"Keep this description\"\nlicense: MIT\n---\n# Other heading\n";

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.NotNull(plan.IntendedBytes);
        var updated = Decode(plan.IntendedBytes);
        Assert.Contains("description: \"Keep this description\"", updated);
        Assert.Contains("license: MIT", updated);
        Assert.Contains("name: \"sample-skill\"", updated);
        Assert.DoesNotContain("description: \"Other heading\"", updated);
        Assert.Equal(["name"], plan.Fields);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption repairs a blank native Skill name and preserves valid metadata")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void ReplacesBlankNativeNameWithoutChangingDescriptionLicenseOrBody()
    {
        const string source = "---\nname: \"\"\n"
            + "description: Existing description\n"
            + "license: MIT\ncustom:\n  nested: keep\n"
            + "---\n# Body remains exact\n";

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.NotNull(plan.IntendedBytes);
        var updated = Decode(plan.IntendedBytes);
        Assert.Contains("name: \"sample-skill\"", updated);
        Assert.Contains("description: Existing description", updated);
        Assert.Contains("license: MIT\ncustom:\n  nested: keep", updated);
        Assert.EndsWith("# Body remains exact\n", updated);
        Assert.Equal(["name"], plan.Fields);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption repairs a blank native Skill description with typed span edits")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void ReplacesBlankNativeDescriptionAndPreservesUnknownYaml()
    {
        const string source = "---\nname: sample-skill\ndescription: \"\"\n"
            + "license: MIT\ncustom: keep\n---\n# Suggested description\n";

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.NotNull(plan.IntendedBytes);
        var updated = Decode(plan.IntendedBytes);
        Assert.Contains("name: sample-skill\n", updated);
        Assert.Contains("description: \"Suggested description\"", updated);
        Assert.Contains("license: MIT\ncustom: keep", updated);
        Assert.EndsWith("# Suggested description\n", updated);
        Assert.Equal(["description"], plan.Fields);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Native Skill description derivation uses the first available source")]
    [InlineData("---\ntitle: Imported title\nlicense: MIT\n---\n# Body heading\n", "Imported title", nameof(WorkspaceAdoptionDerivationSource.ExistingTitle))]
    [InlineData("---\nlicense: MIT\n---\n# Body heading\n", "Body heading", nameof(WorkspaceAdoptionDerivationSource.Heading))]
    [InlineData("---\nlicense: MIT\n---\nNo heading here.\n", SkillPath, nameof(WorkspaceAdoptionDerivationSource.RelativePath))]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void DerivesNativeDescriptionByTitleThenHeadingThenPath(
        string source,
        string expectedDescription,
        string expectedSource)
    {
        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.NotNull(plan.IntendedBytes);
        Assert.Contains($"description: {Quote(expectedDescription)}", Decode(plan.IntendedBytes));
        Assert.Equal(
            Enum.Parse<WorkspaceAdoptionDerivationSource>(expectedSource),
            Assert.Single(plan.Derivation, value => value.Field == "description").Source);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Native Skill body fallback skips level-two headings and uses the first level-one heading")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void DerivesDescriptionFromFirstLevelOneHeadingAfterLevelTwo()
    {
        const string body = "## Secondary heading\n# Primary heading\n\nKeep the authored body.\n";
        const string source = "---\nname: sample-skill\nlicense: MIT\n---\n" + body;

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        var intendedBytes = Assert.IsType<byte[]>(plan.IntendedBytes);
        var updated = Decode(intendedBytes);
        Assert.Contains("description: \"Primary heading\"", updated);
        Assert.Contains("license: MIT", updated);
        var bodyBytes = Encode(body);
        Assert.Equal(bodyBytes, intendedBytes[^bodyBytes.Length..]);
        Assert.Equal(
            WorkspaceAdoptionDerivationSource.Heading,
            Assert.Single(plan.Derivation, value => value.Field == "description").Source);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Native Skill body fallback uses the relative path when no level-one heading exists")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void FallsBackToRelativePathWhenSkillBodyHasOnlyLevelTwoHeadings()
    {
        const string body = "## Secondary heading\n\nKeep the authored body.\n";
        const string source = "---\nname: sample-skill\nlicense: MIT\n---\n" + body;

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        var intendedBytes = Assert.IsType<byte[]>(plan.IntendedBytes);
        var updated = Decode(intendedBytes);
        Assert.Contains($"description: {Quote(SkillPath)}", updated);
        Assert.Contains("license: MIT", updated);
        var bodyBytes = Encode(body);
        Assert.Equal(bodyBytes, intendedBytes[^bodyBytes.Length..]);
        Assert.Equal(
            WorkspaceAdoptionDerivationSource.RelativePath,
            Assert.Single(plan.Derivation, value => value.Field == "description").Source);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Native Skill can reuse an existing Open Forge description without adding Open Forge metadata")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void ReusesExistingOpenForgeDescriptionForNativeSkill()
    {
        const string source = "---\nopen-forge:\n"
            + "  description: Imported context\n"
            + "  tags: [Workspace]\n"
            + "---\n# Heading fallback\n";

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.NotNull(plan.IntendedBytes);
        var updated = Decode(plan.IntendedBytes);
        Assert.Contains("description: \"Imported context\"", updated);
        Assert.Contains("open-forge:\n  description: Imported context\n  tags: [Workspace]", updated);
        Assert.Equal(
            WorkspaceAdoptionDerivationSource.ExistingDescription,
            plan.Derivation.Single(value => value.Field == "description").Source);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Invalid or ambiguous native Skill metadata blocks with its source field")]
    [InlineData("---\nname: sample-skill\ndescription: [unfinished\n---\n", "frontmatter")]
    [InlineData("---\nname: one\nname: two\ndescription: valid\n---\n", "name")]
    [InlineData("---\nname: sample-skill\ndescription: [not, scalar]\n---\n", "description")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void BlocksMalformedDuplicateAndWrongTypeNativeMetadata(string source, string field)
    {
        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.Null(plan.IntendedBytes);
        Assert.Contains(SkillPath, plan.Cause, StringComparison.Ordinal);
        Assert.Contains($"field '{field}'", plan.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Ordinary Markdown without authored metadata remains byte-for-byte unchanged")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void LeavesOrdinaryMarkdownWithoutMetadataUnchanged()
    {
        const string source = "# Local notes\n\nKeep this authored document.\n";
        var bytes = Encode(source);

        var plan = new WorkspaceAdoptionDocumentPlanner().Plan(
            ".agents/notes/local.md",
            SourceDocumentForm.Markdown,
            bytes,
            ensureEntries: false);

        Assert.Equal(bytes, plan.IntendedBytes);
        Assert.Empty(plan.Actions);
        Assert.Empty(plan.Fields);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Partial ordinary metadata remains unchanged without optional tag completion")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void LeavesPartialOrdinaryMetadataUnchanged()
    {
        const string source = "---\nopen-forge:\n  description: Draft guide\n---\n# Guide\n";
        var bytes = Encode(source);

        var plan = new WorkspaceAdoptionDocumentPlanner().Plan(
            ".agents/notes/guide.md",
            SourceDocumentForm.Markdown,
            bytes,
            ensureEntries: false);

        Assert.Equal(bytes, plan.IntendedBytes);
        Assert.Empty(plan.Actions);
        Assert.Empty(plan.Fields);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "A valid entrypoint with one Entries region is unchanged")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void LeavesValidEntrypointAndEntriesRegionUnchanged()
    {
        const string source = "---\nopen-forge:\n"
            + "  description: Workspace map\n"
            + "  tags: [Workspace]\n"
            + "---\n# Home\n\n## Entries\n\n- none - No entries - #Empty\n";
        var bytes = Encode(source);

        var plan = new WorkspaceAdoptionDocumentPlanner().Plan(
            EntrypointPath,
            SourceDocumentForm.CanonicalEntrypoint,
            bytes,
            ensureEntries: true);

        Assert.Equal(bytes, plan.IntendedBytes);
        Assert.Empty(plan.Actions);
        Assert.Empty(plan.Fields);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption appends a missing Entries region without changing authored body bytes")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void AppendsEntriesAfterAnUnchangedAuthoredBody()
    {
        const string body = "# Home\n\nKeep this paragraph and link [as written](guide.md).\n";

        var plan = Plan(EntrypointPath, SourceDocumentForm.CanonicalEntrypoint, body, ensureEntries: true);

        Assert.NotNull(plan.IntendedBytes);
        var updated = Decode(plan.IntendedBytes);
        Assert.StartsWith(body, updated);
        Assert.EndsWith("## Entries\n\n- none - No entries - #Empty\n", updated);
        Assert.Equal([WorkspaceAdoptionAction.EntriesSectionAdded], plan.Actions);
        Assert.Empty(plan.Fields);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption blocks duplicate Entries regions")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void BlocksDuplicateEntriesRegions()
    {
        const string source = "# Home\n\n## Entries\n\n- none - No entries - #Empty\n\n## Entries\n\n- none - No entries - #Empty\n";

        var plan = Plan(EntrypointPath, SourceDocumentForm.CanonicalEntrypoint, source, ensureEntries: true);

        Assert.Null(plan.IntendedBytes);
        Assert.Contains(EntrypointPath, plan.Cause, StringComparison.Ordinal);
        Assert.Contains("field 'Entries'", plan.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Created workspace entrypoint parses with metadata and an Entries region")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void CreatesParseableEntrypointAndPlanningItAgainIsUnchanged()
    {
        var planner = new WorkspaceAdoptionDocumentPlanner();
        var created = planner.CreateEntrypoint(EntrypointPath, "\r\n", FrontmatterForm.Scoped);

        Assert.NotNull(created.IntendedBytes);
        Assert.Equal([WorkspaceAdoptionAction.EntrypointCreated], created.Actions);
        Assert.Equal(["open-forge.description", "open-forge.tags"], created.Fields);
        Assert.Collection(
            created.Derivation,
            value => Assert.Equal(WorkspaceAdoptionDerivationSource.RelativePath, value.Source),
            value => Assert.Equal(WorkspaceAdoptionDerivationSource.RequiredTag, value.Source));
        var intendedBytes = Assert.IsType<byte[]>(created.IntendedBytes);
        Assert.Equal(new byte[] { 0x2D, 0x2D, 0x2D }, intendedBytes[..3]);
        var scaffold = Decode(intendedBytes);
        Assert.Contains("## Axioms\r\n\r\n- inherited - No local axioms; loaded ancestor axioms remain active.\r\n", scaffold);
        Assert.DoesNotContain("\n", scaffold.Replace("\r\n", string.Empty, StringComparison.Ordinal));

        var markdown = new MarkdownDocumentParser().Parse(scaffold);
        Assert.Equal(MarkdownGeneratedRegionState.Complete, markdown.GeneratedRegion.State);
        var authored = new SourceAuthoredMetadataParser().Parse(markdown, SourceDocumentForm.CanonicalEntrypoint);
        Assert.Equal(SourceAuthoredMetadataState.Complete, authored.State);
        Assert.Equal(["Workspace"], authored.Tags);

        var second = planner.Plan(
            EntrypointPath,
            SourceDocumentForm.CanonicalEntrypoint,
            intendedBytes,
            ensureEntries: true);
        Assert.Equal(intendedBytes, second.IntendedBytes);
        Assert.Empty(second.Actions);
    }

    [Theory(DisplayName = "Adoption emits canonical metadata bytes in both forms while preserving the body and requested newline")]
    [InlineData(false, "\n")]
    [InlineData(false, "\r\n")]
    [InlineData(true, "\n")]
    [InlineData(true, "\r\n")]
    [Trait("Boundary", "Input"), Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void CreatesCanonicalEntrypointBytes(bool root, string newline)
    {
        var form = root ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        var yaml = form switch
        {
            FrontmatterForm.Scoped => """
                open-forge:
                  description: Workspace entrypoint for workspace at .agents/workspace
                  tags: [Workspace]
                """,
            FrontmatterForm.Root => """
                description: Workspace entrypoint for workspace at .agents/workspace
                tags: [Workspace]
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The frontmatter form is not defined."),
        };
        var expected = $$"""
            ---
            {{yaml}}
            ---

            # workspace

            ## Axioms

            - inherited - No local axioms; loaded ancestor axioms remain active.

            ## Entries

            - none - No entries - #Empty
            """.ReplaceLineEndings(newline) + newline;

        var created = new WorkspaceAdoptionDocumentPlanner().CreateEntrypoint(EntrypointPath, newline, form);

        Assert.Equal(Encode(expected), created.IntendedBytes);
        Assert.Null(created.Cause);
    }

    [Fact(DisplayName = "Entrypoint composition accepts an explicit root form without changing its body")]
    [Trait("Boundary", "Input"), Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void CreatesRequestedRootFormWithTheSameBody()
    {
        var planner = new WorkspaceAdoptionDocumentPlanner();
        var scoped = planner.CreateEntrypoint(EntrypointPath, "\r\n", FrontmatterForm.Scoped);
        var root = planner.CreateEntrypoint(EntrypointPath, "\r\n", FrontmatterForm.Root);
        var parser = new MarkdownDocumentParser();
        var scopedDocument = parser.Parse(Decode(Assert.IsType<byte[]>(scoped.IntendedBytes)));
        var rootDocument = parser.Parse(Decode(Assert.IsType<byte[]>(root.IntendedBytes)));
        var metadata = new SourceAuthoredMetadataParser().Parse(rootDocument, SourceDocumentForm.CanonicalEntrypoint);

        Assert.Equal(SourceAuthoredMetadataState.Complete, metadata.State);
        Assert.Equal(FrontmatterForm.Root, metadata.FrameworkMetadata?.Syntax.AuthoredForm);
        var scopedBody = Assert.IsType<MarkdownTextSpan>(scopedDocument.BodySpan);
        var rootBody = Assert.IsType<MarkdownTextSpan>(rootDocument.BodySpan);
        Assert.Equal(scopedDocument.Source[scopedBody.Start..], rootDocument.Source[rootBody.Start..]);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Entrypoint scaffolding accepts only recognized entrypoint paths")]
    [InlineData(".agents/notes/local.md")]
    [InlineData(".agents/skills/sample-skill/SKILL.md")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void RejectsUnrecognizedEntrypointPaths(string path)
    {
        var plan = new WorkspaceAdoptionDocumentPlanner().CreateEntrypoint(path, "\n", FrontmatterForm.Scoped);

        Assert.Null(plan.IntendedBytes);
        Assert.Contains(path, plan.Cause, StringComparison.Ordinal);
        Assert.Contains("recognized entrypoint", plan.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Adoption blocks bytes that are not supported strict UTF-8")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void BlocksUnsupportedEncodingWithSourceAndField()
    {
        var plan = new WorkspaceAdoptionDocumentPlanner().Plan(
            SkillPath,
            SourceDocumentForm.Skill,
            new byte[] { 0xFF, 0xFE, 0x23, 0x00 },
            ensureEntries: false);

        Assert.Null(plan.IntendedBytes);
        Assert.Contains(SkillPath, plan.Cause, StringComparison.Ordinal);
        Assert.Contains("field 'encoding'", plan.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Invalid applyTo blocks a complete native Skill")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void BlocksInvalidApplyToEvenWhenNativeMetadataIsComplete()
    {
        const string source = "---\nname: sample-skill\n"
            + "description: Complete native metadata\n"
            + "applyTo: '**//*.md'\n---\n# Skill\n";

        var plan = Plan(SkillPath, SourceDocumentForm.Skill, source);

        Assert.Null(plan.IntendedBytes);
        Assert.Contains(SkillPath, plan.Cause, StringComparison.Ordinal);
        Assert.Contains("field 'applyTo'", plan.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Native Skill rejects an Entries request")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void RejectsEntriesForNativeSkill()
    {
        var plan = Plan(SkillPath, SourceDocumentForm.Skill, "# Skill\n", ensureEntries: true);

        Assert.Null(plan.IntendedBytes);
        Assert.Contains("field 'Entries'", plan.Cause, StringComparison.Ordinal);
    }

    private static WorkspaceAdoptionDocumentPlan Plan(
        string path,
        SourceDocumentForm form,
        string source,
        bool ensureEntries = false)
        => new WorkspaceAdoptionDocumentPlanner().Plan(path, form, Encode(source), ensureEntries);

    private static byte[] Encode(string source) => Encoding.UTF8.GetBytes(source);

    private static string Decode(byte[]? bytes)
        => Encoding.UTF8.GetString(bytes ?? throw new InvalidOperationException("The document plan is blocked."));

    private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
}

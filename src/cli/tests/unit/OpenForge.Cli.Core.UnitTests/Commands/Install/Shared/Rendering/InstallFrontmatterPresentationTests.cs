using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install.Shared.Help;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Install.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Rendering;

public sealed class InstallFrontmatterPresentationTests
{
    [Fact(DisplayName = "The frontmatter question shows both forms and their examples")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void QuestionShowsBothChoicesWithExamples()
    {
        var question = InstallSetupQuestions.Frontmatter(new("root"));
        Assert.Equal("How should Open Forge write file metadata?", question.Question);
        Assert.Collection(question.Choices,
            root => { Assert.Equal("root", root.Value); Assert.Equal("Root keys", root.Label); Assert.Equal("description: and tags: at the top level", root.Summary); },
            scoped => { Assert.Equal("scoped", scoped.Value); Assert.Equal("Scoped under open-forge:", scoped.Label); Assert.Equal("open-forge: holds description: and tags:", scoped.Summary); });
    }

    [Fact(DisplayName = "Fresh setup lists root first")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void FreshListsRootFirst()
        => Assert.Equal("root", InstallSetupQuestions.Frontmatter(new("root")).Choices[0].Value);

    [Theory(DisplayName = "Configure lists the current form first"), InlineData("root"), InlineData("scoped")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ConfigureListsCurrentFirst(string current)
        => Assert.Equal(current, InstallSetupQuestions.Frontmatter(new(current)).Choices[0].Value);

    [Fact(DisplayName = "An unknown initial form is rejected")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void UnknownInitialFormIsRejected()
        => Assert.Throws<ArgumentOutOfRangeException>(() => InstallSetupQuestions.Frontmatter(new("unknown")));

    [Theory(DisplayName = "Install renders the resolved form in text and JSON at every detail")]
    [InlineData("root"), InlineData("scoped")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void FormRendersAtEveryDetail(string form)
    {
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var data = Select(new(form, null, []), detail);
            var text = InstallDataTextRenderer.Render(data, new(detail, null), CliTextStyle.Plain).Content;
            Assert.StartsWith($"Frontmatter: {form}\n", text, StringComparison.Ordinal);
            using var document = JsonDocument.Parse(JsonSerializer.Serialize(data, InstallDataJsonContext.Default.InstallData));
            Assert.Equal(form, document.RootElement.GetProperty("frontmatter").GetProperty("form").GetString());
        }
    }

    [Fact(DisplayName = "Install lists kept files in JSON at every detail and in text from standard detail")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ChangeAndKeptFilesRender()
    {
        var form = new InstallFrontmatter("root", "scoped",
            [new(".agents/edited.md", InstallFrontmatterKeptReason.Edited), new(".agents/unavailable.md", InstallFrontmatterKeptReason.SourceUnavailable)]);
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var data = Select(form, detail);
            var text = InstallDataTextRenderer.Render(data, new(detail, null), CliTextStyle.Plain).Content;
            Assert.Contains("Frontmatter: scoped -> root\n", text, StringComparison.Ordinal);
            Assert.Contains("Kept 2 files in their previous form.\n", text, StringComparison.Ordinal);
            Assert.Equal("scoped", data.Frontmatter?.PreviousForm);
            using var document = JsonDocument.Parse(JsonSerializer.Serialize(data, InstallDataJsonContext.Default.InstallData));
            var kept = document.RootElement.GetProperty("frontmatter").GetProperty("kept").EnumerateArray().ToArray();
            Assert.Collection(kept,
                edited =>
                {
                    Assert.Equal(".agents/edited.md", edited.GetProperty("path").GetString());
                    Assert.Equal("edited", edited.GetProperty("reason").GetString());
                },
                unavailable =>
                {
                    Assert.Equal(".agents/unavailable.md", unavailable.GetProperty("path").GetString());
                    Assert.Equal("source-unavailable", unavailable.GetProperty("reason").GetString());
                });
            if (detail >= CliDetail.Standard)
            {
                Assert.Contains(".agents/edited.md", text, StringComparison.Ordinal);
                Assert.Contains(".agents/unavailable.md", text, StringComparison.Ordinal);
                Assert.Contains("edited", text, StringComparison.Ordinal);
                Assert.Contains("source-unavailable", text, StringComparison.Ordinal);
            }
            else
            {
                Assert.DoesNotContain(".agents/edited.md", text, StringComparison.Ordinal);
                Assert.DoesNotContain(".agents/unavailable.md", text, StringComparison.Ordinal);
                Assert.DoesNotContain("edited", text, StringComparison.Ordinal);
                Assert.DoesNotContain("source-unavailable", text, StringComparison.Ordinal);
            }
        }
    }

    [Fact(DisplayName = "JSON omits unresolved frontmatter, unchanged previous form and absent kept files")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void JsonOmitsAbsentMembers()
    {
        using var absent = JsonDocument.Parse(JsonSerializer.Serialize(Select(null, CliDetail.Full), InstallDataJsonContext.Default.InstallData));
        Assert.False(absent.RootElement.TryGetProperty("frontmatter", out _));
        using var unchanged = JsonDocument.Parse(JsonSerializer.Serialize(Select(new("scoped", "scoped", []), CliDetail.Full), InstallDataJsonContext.Default.InstallData));
        Assert.Equal(["form"], unchanged.RootElement.GetProperty("frontmatter").EnumerateObject().Select(property => property.Name));
    }

    [Fact(DisplayName = "Install help lists the frontmatter option and both usage examples")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void HelpListsOption()
    {
        var help = InstallHelpSections.Create();
        Assert.Contains("[--frontmatter <root|scoped>]", Assert.Single(help.Sections, section => section.Heading == "Syntax").Body, StringComparison.Ordinal);
        Assert.Equal("Choose where Open Forge writes file metadata: root or scoped. A fresh unattended Install uses root. Change an installed workspace with --configure.",
            Assert.Single(help.Sections, section => section.Heading == "Frontmatter").Body);
        var examples = Assert.Single(help.Sections, section => section.Heading == "Examples").Body;
        Assert.Contains("open-forge install --frontmatter scoped --automatic", examples, StringComparison.Ordinal);
        Assert.Contains("open-forge install --configure --frontmatter root --dry-run", examples, StringComparison.Ordinal);
        Assert.Equal("  Ordinary Install delivers only the Framework payload embedded in the running CLI. With a form change, --configure also converts eligible owned Extension files from their recorded sources. Install does not discover another workspace, fetch content, manipulate Git, repair markers, reconcile managed divergence, or roll back target effects.",
            Assert.Single(help.Sections, section => section.Heading == "Notes").Body);
    }

    private static InstallData Select(InstallFrontmatter? form, CliDetail detail)
    {
        var result = new InstallResult(null, new InstallBindingInput(false, false, InstallMode.DryRun), []) { Frontmatter = form };
        return InstallReportSelector.Select(result, new(detail, null)).Data;
    }
}

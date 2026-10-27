using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedInstallProcessTests
{
    private const string OutsideDirectory = "docs/team";
    private const string OutsideIndependentFile = "CONTRIBUTING.md";

    [Theory(DisplayName = "Published Install configure conversion and Update preserve independently registered outside content")]
    [InlineData(PublishedTokens.Sources.AgentsRoot), InlineData("knowledge/main")]
    [Trait("Feature", "install-outside-preservation"), Trait("Evidence", "EndToEnd")]
    public async Task OutsideLifecycleComposition(string mainRoot)
    {
        using var workspace = PublishedJourneyWorkspace.Create(nameof(OutsideLifecycleComposition));
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(PublishedTokens.Layout.GitIgnore);
        workspace.ExpectFiles(PublishedInstallWorkspace.EmbeddedPayloadPaths.Select(path =>
            path.StartsWith(PublishedTokens.Sources.AgentsRoot + "/", StringComparison.Ordinal)
                && !path.StartsWith(PublishedTokens.Layout.NativeSkillsPath + "/", StringComparison.Ordinal)
                ? mainRoot + path[PublishedTokens.Sources.AgentsRoot.Length..] : path).ToArray());
        var anchor = OutsideDirectory + "/" + PublishedTokens.Layout.IndexFile;
        var outsideText = OpenForgeDocumentSeed.Metadata("Independent outside rules", ["Authored"],
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Independent outside rules",
                Entries = string.Empty,
                LineEnding = "\r\n",
            }));
        const string independentText = "# Independent contribution rules\n";
        workspace.WriteText(anchor, outsideText);
        workspace.WriteText(OutsideIndependentFile, independentText);
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{mainRoot}}",
             "{{PublishedTokens.Settings.OutsideRoutesProperty}}":["{{OutsideDirectory}}"],
             "{{PublishedTokens.Settings.RootsProperty}}":["{{OutsideIndependentFile}}"]}
            """);
        AssertOutsideJourneySuccess(await workspace.RunAsync(PublishedTokens.Install.Command, PublishedTokens.Install.Automatic,
            PublishedTokens.Install.Frontmatter, PublishedTokens.Settings.ScopedFrontmatter, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat));
        var unlisted = await workspace.RunAsync(PublishedTokens.Route.Command, PublishedTokens.RouteInspect.Command,
            PublishedTokens.Layout.LiteralReferencePrefix + anchor, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat);
        AssertOutsideJourneySuccess(unlisted);
        using (var inspected = JsonDocument.Parse(unlisted.StandardOutput))
            Assert.Equal(OutsideDirectory, inspected.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.RouteInspectData.MaintenanceRoot).GetString());
        AssertOutsideJourneySuccess(await workspace.RunAsync(PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command,
            PublishedTokens.Layout.LiteralReferencePrefix + anchor, PublishedTokens.RouteInclude.Description, "Independent outside policy",
            PublishedTokens.RouteInclude.Tag, PublishedTokens.Metadata.LoadNow, PublishedTokens.RouteInclude.ApplyTo, "src/**",
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat));
        var loader = mainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
        var loaderBefore = File.ReadAllBytes(workspace.Combine(loader));
        string[][] stages =
        [
            [PublishedTokens.Install.Command, PublishedTokens.Install.Automatic],
            [PublishedTokens.Install.Command, PublishedTokens.Install.Configure, PublishedTokens.Install.Preset,
                PublishedTokens.Install.FullCorePreset, PublishedTokens.Install.Automatic],
            [PublishedTokens.Install.Command, PublishedTokens.Install.Configure, PublishedTokens.Install.Frontmatter,
                PublishedTokens.Settings.RootFrontmatter, PublishedTokens.Install.Automatic],
            [PublishedTokens.Update.Command, PublishedTokens.Update.Automatic],
        ];
        foreach (var stage in stages)
        {
            var dryRun = stage[0] == PublishedTokens.Update.Command ? PublishedTokens.Update.DryRun : PublishedTokens.Install.DryRun;
            var before = workspace.SnapshotState(includeRuntime: true);
            AssertOutsideJourneySuccess(await workspace.RunAsync([.. stage, dryRun,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]));
            Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
            AssertOutsideJourneySuccess(await workspace.RunAsync([.. stage, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]));
            Assert.Equal(loaderBefore, File.ReadAllBytes(workspace.Combine(loader)));
            Assert.Equal(Encoding.UTF8.GetBytes(outsideText), File.ReadAllBytes(workspace.Combine(anchor)));
            Assert.Equal(Encoding.UTF8.GetBytes(independentText), File.ReadAllBytes(workspace.Combine(OutsideIndependentFile)));
            using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
            Assert.Equal(OutsideDirectory, Assert.Single(settings.RootElement.GetProperty(PublishedTokens.Settings.OutsideRoutesProperty).EnumerateArray()).GetString());
            Assert.Contains(settings.RootElement.GetProperty(PublishedTokens.Settings.RootsProperty).EnumerateArray(), value => value.GetString() == OutsideIndependentFile);
        }
        var completed = workspace.SnapshotState();
        var repeat = await workspace.RunAsync(PublishedTokens.Update.Command, PublishedTokens.Update.Automatic,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat);
        AssertOutsideJourneySuccess(repeat);
        using var repeated = JsonDocument.Parse(repeat.StandardOutput);
        Assert.Empty(repeated.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        Assert.Equal(completed, workspace.SnapshotState());
    }

    private static void AssertOutsideJourneySuccess(ProcessRunResult run)
    {
        Assert.True(run.ExitCode == PublishedTokens.Statuses.CompleteExitCode, run.StandardOutput + run.StandardError);
        Assert.Empty(run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, document.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        Assert.DoesNotContain(document.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray(), effect =>
            effect.GetProperty(PublishedTokens.Json.Path).GetString()?.StartsWith(OutsideDirectory + "/", StringComparison.Ordinal) == true);
    }
}

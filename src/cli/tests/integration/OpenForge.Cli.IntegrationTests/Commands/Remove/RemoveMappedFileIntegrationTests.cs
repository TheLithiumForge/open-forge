using System.Text.Json;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

[Trait("Feature", "remove-root"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class RemoveMappedFileIntegrationTests
{
    private const string MainRoot = "docs/forge";
    private const string Category = "notes";
    private const string Article = "article" + SourceFormClassifier.MarkdownExtension;
    private const string FixtureTag = "Fixture";

    [Theory(DisplayName = "Top-level Remove keeps default human guide refusals and loader removal behavior in custom layouts")]
    [InlineData(WorkspaceLayoutNames.HumanGuideFileName, false, false)]
    [InlineData(WorkspaceLayoutNames.HumanGuideFileName, true, false)]
    [InlineData(WorkspaceLayoutNames.LoaderFileName, false, false)]
    [InlineData(WorkspaceLayoutNames.LoaderFileName, true, false)]
    [InlineData(WorkspaceLayoutNames.HumanGuideFileName, false, true)]
    [InlineData(WorkspaceLayoutNames.HumanGuideFileName, true, true)]
    [InlineData(WorkspaceLayoutNames.LoaderFileName, false, true)]
    [InlineData(WorkspaceLayoutNames.LoaderFileName, true, true)]
    public async Task MappedFilesKeepDefaultBehavior(string fileName, bool dotSlash, bool routeKind)
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, MainRoot })
        {
            using var workspace = RemoveRootIntegrationWorkspace.Create(nameof(MappedFilesKeepDefaultBehavior));
            var layout = Seed(workspace, root);
            var guideBytes = File.ReadAllBytes(workspace.Combine(layout.HumanGuidePath));
            var before = workspace.SnapshotHashes();
            var target = fileName == WorkspaceLayoutNames.HumanGuideFileName ? layout.HumanGuidePath : layout.LoaderPath;
            var operand = dotSlash ? SourceWorkspaceRelativePath.LiteralReferencePrefix + target : target;
            var output = new StringWriter();
            var error = new StringWriter();

            List<string> arguments = [RemoveDefinitions.CommandIdentity, operand, RemoveDefinitions.AutomaticOption,
                CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat];
            if (routeKind) arguments.AddRange([RemoveDefinitions.KindOption, RemoveDefinitions.RouteKind]);
            var completion = await workspace.RunAsync(arguments, output, error);

            Assert.Empty(error.ToString());
            using var report = JsonDocument.Parse(output.ToString());
            Assert.Equal(RemoveDefinitions.CommandIdentity, report.RootElement.GetProperty(CliJsonFields.Command).GetString());
            var findings = report.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray().ToArray();
            if (fileName == WorkspaceLayoutNames.HumanGuideFileName || routeKind)
            {
                Assert.Equal(CliSemanticStatus.Invalid, completion.Status);
                var finding = Assert.Single(findings);
                var code = finding.GetProperty(CliJsonFields.Code).GetString();
                Assert.Equal(RouteRemoveFindingCodes.InvalidSubject, code);
                Assert.Equal(target, finding.GetProperty(CliJsonFields.Subject).GetProperty(CliJsonFields.Id).GetString());
                Assert.Empty(report.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
                Assert.Equal(before, workspace.SnapshotHashes());
            }
            else
            {
                Assert.Equal(CliSemanticStatus.Complete, completion.Status);
                Assert.Empty(findings);
                Assert.False(File.Exists(workspace.Combine(target)));
                var settings = WorkspaceSettingsCodec.Read(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))).Document;
                Assert.NotNull(settings);
                Assert.Contains(target, settings.RemovedFiles);
                Assert.Equal(guideBytes, File.ReadAllBytes(workspace.Combine(layout.HumanGuidePath)));
            }
        }
    }

    [Theory(DisplayName = "Top-level Remove passes classified routed files and category entrypoints as exact paths in every layout")]
    [InlineData(false), InlineData(true)]
    public async Task ClassifiedRoutesKeepExactTargets(bool category)
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, MainRoot })
        {
            using var workspace = RemoveRootIntegrationWorkspace.Create(nameof(ClassifiedRoutesKeepExactTargets));
            var layout = Seed(workspace, root);
            var entrypointName = SourceFormClassifier.CanonicalEntrypointFileName(Category);
            var articlePath = layout.SourcePath(Category + "/" + Article);
            var categoryPath = layout.CategoryDirectory(Category);
            File.WriteAllText(workspace.Combine(layout.LoaderPath),
                OpenForgeDocumentSeed.GeneratedEntries($"- [Notes]({Category}/{entrypointName}) - #{FixtureTag}"));
            workspace.WriteText(categoryPath + "/" + entrypointName,
                OpenForgeDocumentSeed.Metadata("Fixture category", [FixtureTag],
                    OpenForgeDocumentSeed.GeneratedEntries($"- [Article]({Article}) - #{FixtureTag}")));
            workspace.WriteText(articlePath, OpenForgeDocumentSeed.Metadata("Fixture article", [FixtureTag], "# Article\n"));
            var guideBytes = File.ReadAllBytes(workspace.Combine(layout.HumanGuidePath));
            var output = new StringWriter();
            var error = new StringWriter();

            var completion = await workspace.RunAsync(
                [RemoveDefinitions.CommandIdentity, category ? categoryPath : articlePath, RemoveDefinitions.AutomaticOption,
                    CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);

            Assert.Equal(CliSemanticStatus.Complete, completion.Status);
            Assert.Empty(error.ToString());
            Assert.False(File.Exists(workspace.Combine(articlePath)));
            Assert.Equal(!category, Directory.Exists(workspace.Combine(categoryPath)));
            Assert.Equal(guideBytes, File.ReadAllBytes(workspace.Combine(layout.HumanGuidePath)));
            var settings = WorkspaceSettingsCodec.Read(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))).Document;
            Assert.NotNull(settings);
            if (category) Assert.Contains(Category, settings.RemovedCategories);
            else Assert.Contains(articlePath, settings.RemovedFiles);
        }
    }

    private static WorkspaceLayout Seed(RemoveRootIntegrationWorkspace workspace, string root)
    {
        var layout = WorkspaceLayout.Create(workspace.Workspace, root);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}
            """);
        workspace.WriteText(layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        workspace.WriteText(layout.HumanGuidePath, "# Fixture guide\n");
        return layout;
    }
}

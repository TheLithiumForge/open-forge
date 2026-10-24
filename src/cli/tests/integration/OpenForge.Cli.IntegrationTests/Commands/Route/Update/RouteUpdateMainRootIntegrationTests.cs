using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

[Trait("Feature", "route-update"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteUpdateMainRootIntegrationTests
{
    private const string MainRoot = "docs/context";
    private const string OtherRoot = "handbook/context";
    private const string Category = "notes";
    private const string LeafName = "overview";
    private const string Description = "Before overview";
    private const string UpdatedDescription = "After overview";
    private const string Tag = "Notes";

    [Fact(DisplayName = "Route Update plans from the observed custom root and rejects a changed layout")]
    public async Task CustomLayoutDrivesSelectionAndRevalidation()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(RouteUpdateMainRootIntegrationTests));
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var leaf = LeafName + SourceFormClassifier.MarkdownExtension;
        var entrypoint = SourceFormClassifier.CanonicalEntrypointFileName(Category);
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        temporary.WriteText(MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName,
            "# Loader\n\n" + OpenForgeDocumentSeed.GeneratedEntries($"- [{Category}]({Category}/{entrypoint}) - #{Tag}"));
        temporary.WriteText(MainRoot + "/" + Category + "/" + entrypoint,
            OpenForgeDocumentSeed.Metadata(Category, [Tag], body: OpenForgeDocumentSeed.GeneratedEntries($"- [{Description}]({leaf}) - #{Tag}")));
        var target = MainRoot + "/" + Category + "/" + leaf;
        temporary.WriteText(target, OpenForgeDocumentSeed.Metadata(Description, [Tag], body: string.Empty));
        var before = SnapshotBytes(temporary);
        var builder = RouteUpdateIntegrationWorkspace.CreatePlanBuilder();
        var request = new RouteUpdateRequest(workspace, Category + "/" + LeafName,
            RouteUpdateIntegrationWorkspace.DescriptionPatch(UpdatedDescription), null, RouteUpdateMode.DryRun);
        var build = await builder.BuildAsync(request, TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Equal(MainRoot, plan.Observation.Catalogue.Layout.MainRoot);
        Assert.Equal(target, plan.Observation.TargetSource.Identity.CanonicalBasePath);
        Assert.Equal(Category + "/" + LeafName, plan.Observation.TargetSource.Identity.AutomaticId);
        Assert.Contains(plan.FileChanges, change => change.LogicalPath == temporary.Combine(target));
        AssertUnchanged(before, temporary);

        temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot));
        var changed = SnapshotBytes(temporary);
        var revalidation = await new RouteUpdatePlanRevalidator(builder, new RouteUpdatePlanEquivalence())
            .RevalidateAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RouteUpdatePlanRevalidationState.Changed, revalidation.State);
        AssertUnchanged(changed, temporary);
    }

    private static Dictionary<string, byte[]> SnapshotBytes(TemporaryWorkspace workspace)
        => Directory.EnumerateFiles(workspace.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);

    private static void AssertUnchanged(Dictionary<string, byte[]> expected, TemporaryWorkspace workspace)
    {
        var actual = SnapshotBytes(workspace);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), actual.Keys.Order(StringComparer.Ordinal));
        foreach (var (path, bytes) in expected)
        {
            Assert.Equal(bytes, actual[path]);
        }
    }

    private static string Settings(string root) => $$"""
        {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}
        """;
}

using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create.Shared.Application;

[Trait("Feature", "route-create"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteCreateOutsideApplicationIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private const string FileName = "note" + SourceFormClassifier.MarkdownExtension;
    private const string TemplateBody = "\r\n# Starting {topic}\r\n\r\nIndependent copied body.\r\n";
    public enum Drift { Settings, Anchor, Target, TargetDirectory, Directory, Sibling }

    [Theory(DisplayName = "Outside Create applies one bounded intended workspace, preserves authored rows and repeats as a no-op")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteCreateOutsideWorkspace.CustomRoot, false)]
    public async Task ApplyAndRepeat(string mainRoot, bool listed)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot, listed: listed);
        const string foreign = "foreign/guide" + SourceFormClassifier.MarkdownExtension;
        fixture.Files.WriteText(foreign, RouteCreateOutsideWorkspace.Document());
        fixture.Files.ReplaceText(fixture.Anchor, RouteCreateOutsideWorkspace.Document(entries:
            $"- [Authored foreign policy](../../{foreign}) - #{RouteCreateOutsideWorkspace.Tag} - applies to `src/**`"));
        var path = RouteCreateOutsideWorkspace.Directory + "/scope/topic/" + FileName;
        var loaderBefore = File.ReadAllBytes(fixture.Files.Combine(fixture.Layout.LoaderPath));
        var settingsBefore = File.ReadAllBytes(fixture.Files.Combine(WorkspaceSettingsDefinitions.RelativePath));
        var foreignBefore = File.ReadAllBytes(fixture.Files.Combine(foreign));
        var anchorBefore = new MarkdownDocumentParser().Parse(File.ReadAllText(fixture.Files.Combine(fixture.Anchor)));
        var build = await fixture.Build(path);
        var plan = Assert.IsType<RouteCreatePlan>(build.Plan);
        var retainedRows = plan.NavigationFormation.Catalogue.IncludedRows.ToArray();
        Assert.Contains(retainedRows, row => row.Path == foreign);
        fixture.Own(plan);
        var applied = await new RouteCreateApplicationOperation().ExecuteAsync(plan, Token);
        Assert.Equal(RouteCreateVerificationState.Verified, applied.Verification);
        Assert.Empty(applied.Findings);
        Assert.All(applied.Effects, effect => Assert.Equal(RouteCreateEffectOutcome.Verified, effect.Outcome));
        foreach (var change in plan.FileChanges) Assert.Equal(change.IntendedBytes.ToArray(), File.ReadAllBytes(change.LogicalPath));
        Assert.Equal(loaderBefore, File.ReadAllBytes(fixture.Files.Combine(fixture.Layout.LoaderPath)));
        Assert.Equal(settingsBefore, File.ReadAllBytes(fixture.Files.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(foreignBefore, File.ReadAllBytes(fixture.Files.Combine(foreign)));
        var anchorAfter = new MarkdownDocumentParser().Parse(File.ReadAllText(fixture.Files.Combine(fixture.Anchor)));
        var beforeSpan = anchorBefore.GeneratedRegion.EntriesBlock!.Span;
        var afterSpan = anchorAfter.GeneratedRegion.EntriesBlock!.Span;
        Assert.Equal(Encoding.UTF8.GetBytes(anchorBefore.Source[..beforeSpan.Start]), Encoding.UTF8.GetBytes(anchorAfter.Source[..afterSpan.Start]));
        Assert.Equal(Encoding.UTF8.GetBytes(anchorBefore.Source[beforeSpan.End..]), Encoding.UTF8.GetBytes(anchorAfter.Source[afterSpan.End..]));
        var foreignRow = Assert.Single(SourceGeneratedEntriesParser.Parse(anchorAfter).Entries, row => row.Destination == "../../" + foreign);
        var originalRow = Assert.Single(retainedRows, row => row.Path == foreign).Entry;
        Assert.Equal(Encoding.UTF8.GetBytes(anchorBefore.Source.Substring(originalRow.Span.Start, originalRow.Span.Length)),
            Encoding.UTF8.GetBytes(anchorAfter.Source.Substring(foreignRow.Span.Start, foreignRow.Span.Length)));
        var beforeRepeat = fixture.Files.SnapshotHashes();
        var repeat = Assert.IsType<RouteCreatePlan>((await fixture.Build(path)).Plan);
        Assert.True(repeat.IsNoOp);
        Assert.Equal(beforeRepeat, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Outside Create retains optional destination metadata and copies only an explicit Template body")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteCreateOutsideWorkspace.CustomRoot)]
    public async Task TemplateAndOptionalMetadata(string mainRoot)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var template = RouteCreateOutsideWorkspace.Directory + "/template" + SourceFormClassifier.MarkdownExtension;
        fixture.Files.WriteText(template, OpenForgeDocumentSeed.Metadata("Independent template metadata",
            RoutedMetadataDefaults.Tags(RoutedMetadataCategory.Templates), TemplateBody));
        var path = RouteCreateOutsideWorkspace.Directory + "/" + FileName;
        var plan = Assert.IsType<RouteCreatePlan>((await fixture.Build(path, optional: true,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + template)).Plan);
        Assert.Contains(plan.Preview.Findings, finding => finding.Code == RouteCreateFindingCode.OptionalMetadata);
        Assert.Null(plan.Preview.Metadata.Description);
        Assert.Empty(plan.Preview.Metadata.Tags);
        var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(plan.IntendedTargetBytes.AsSpan()));
        var body = document.BodySpan ?? throw new InvalidOperationException();
        Assert.Equal(Encoding.UTF8.GetBytes(TemplateBody), Encoding.UTF8.GetBytes(document.Source.Substring(body.Start, body.Length)));
        fixture.Own(plan);
        var applied = await new RouteCreateApplicationOperation().ExecuteAsync(plan, Token);
        Assert.Equal(RouteCreateVerificationState.Verified, applied.Verification);
        Assert.Equal(RouteCreateFindingCode.OptionalMetadata, Assert.Single(applied.Findings).Code);
        Assert.True(Assert.IsType<RouteCreatePlan>((await fixture.Build(path, optional: true,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + template)).Plan).IsNoOp);
    }

    [Theory(DisplayName = "Outside Create revalidates retained settings, directories, anchor, target and readonly sibling bytes under its lease")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Settings), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Drift.Settings)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Anchor), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Drift.Anchor)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Target), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Drift.Target)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.TargetDirectory), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Drift.TargetDirectory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Directory), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Drift.Directory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Sibling), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Drift.Sibling)]
    public async Task RetainedStateDrift(string mainRoot, Drift drift)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var sibling = RouteCreateOutsideWorkspace.Directory + "/sibling" + SourceFormClassifier.MarkdownExtension;
        fixture.Files.WriteText(sibling, RouteCreateOutsideWorkspace.Document());
        var path = RouteCreateOutsideWorkspace.Directory + "/scope/" + FileName;
        var plan = Assert.IsType<RouteCreatePlan>((await fixture.Build(path)).Plan);
        switch (drift)
        {
            case Drift.Settings: fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings([])); break;
            case Drift.Anchor: fixture.Files.ReplaceText(fixture.Anchor, RouteCreateOutsideWorkspace.Document("# Concurrent anchor edit\n")); break;
            case Drift.Target: fixture.Files.WriteText(path, RouteCreateOutsideWorkspace.Document()); break;
            case Drift.TargetDirectory: fixture.Files.CreateDirectory(path); break;
            case Drift.Directory: fixture.Files.CreateDirectory(RouteCreateOutsideWorkspace.Directory + "/scope"); break;
            case Drift.Sibling: fixture.Files.ReplaceText(sibling, RouteCreateOutsideWorkspace.Document("# Concurrent sibling body\n")); break;
            default: throw new ArgumentOutOfRangeException(nameof(drift));
        }
        var before = fixture.Files.SnapshotHashes();
        var applied = await new RouteCreateApplicationOperation().ExecuteAsync(plan, Token);
        Assert.Equal(RouteCreateFindingCode.TargetChanged, Assert.Single(applied.Findings).Code);
        Assert.All(applied.Effects, effect => Assert.Equal(RouteCreateEffectOutcome.NotStarted, effect.Outcome));
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Outside and primary Create can read managed sibling metadata without changing the managed file")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteCreateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteCreateOutsideWorkspace.CustomRoot, true)]
    public async Task ManagedSiblingComparison(string mainRoot, bool managedEntrypoint)
    {
        foreach (var outside in new[] { false, true })
        {
            var directory = outside ? RouteCreateOutsideWorkspace.Directory : mainRoot + "/" + WorkspaceCategoryNames.Guidance;
            using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
            var anchor = fixture.Anchor;
            if (!outside)
            {
                anchor = WorkspaceRoutePath.CanonicalEntrypointPath(directory);
                fixture.Files.WriteText(anchor, RouteCreateOutsideWorkspace.Document());
                fixture.Files.ReplaceText(fixture.Layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(
                    $"- [Primary fixture]({Path.GetRelativePath(mainRoot, anchor).Replace(Path.DirectorySeparatorChar, '/')}) - #{RouteCreateOutsideWorkspace.Tag}"));
            }
            var sibling = directory + "/managed" + SourceFormClassifier.MarkdownExtension;
            fixture.Files.WriteText(sibling, RouteCreateOutsideWorkspace.Document());
            var ownership = WorkspaceOwnershipDocument.Empty with
            { Extensions = [new("fixture-owner", null, null, [], [managedEntrypoint ? anchor : sibling], [])] };
            fixture.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(ownership));
            var before = File.ReadAllBytes(fixture.Files.Combine(sibling));
            var path = directory + "/" + FileName;
            var plan = Assert.IsType<RouteCreatePlan>((await fixture.Build(path)).Plan);
            fixture.Own(plan);
            var result = await new RouteCreateApplicationOperation().ExecuteAsync(plan, Token);
            Assert.Equal(RouteCreateVerificationState.Verified, result.Verification);
            Assert.Empty(result.Findings);
            Assert.Equal(before, File.ReadAllBytes(fixture.Files.Combine(sibling)));
            Assert.Equal(anchor, plan.ParentSource.Identity.CanonicalBasePath);
        }
    }

    [Fact(DisplayName = "Outside Create retains verified file effects and recovery when the later anchor replacement fails")]
    public async Task PartialFailurePreservesTargetAndRecovery()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var fixture = new RouteCreateOutsideWorkspace(RouteCreateOutsideWorkspace.CustomRoot);
        var path = RouteCreateOutsideWorkspace.Directory + "/" + FileName;
        var before = File.ReadAllBytes(fixture.Files.Combine(fixture.Anchor));
        var plan = Assert.IsType<RouteCreatePlan>((await fixture.Build(path)).Plan);
        fixture.Own(plan);
        using (File.Open(fixture.Files.Combine(fixture.Anchor), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var result = await new RouteCreateApplicationOperation().ExecuteAsync(plan, Token);
            Assert.Contains(result.Findings, finding => finding.Code == RouteCreateFindingCode.WriteFailed);
            Assert.Equal(plan.IntendedTargetBytes.ToArray(), File.ReadAllBytes(fixture.Files.Combine(path)));
            Assert.Equal(before, File.ReadAllBytes(fixture.Files.Combine(fixture.Anchor)));
            Assert.NotNull(result.Recovery.ResidualPath);
            Assert.True(File.Exists(result.Recovery.ResidualPath));
        }
    }
}

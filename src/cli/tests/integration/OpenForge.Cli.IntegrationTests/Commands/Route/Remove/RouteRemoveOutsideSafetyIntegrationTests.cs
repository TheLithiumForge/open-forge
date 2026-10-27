using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

[Trait("Boundary", "Host"), Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
public sealed class RouteRemoveOutsideSafetyIntegrationTests
{
    [Fact]
    public async Task LateOwnershipChangeStopsBeforeTheFirstPersistentEffect()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var plan = await fixture.Plan();
        var operationId = Guid.NewGuid();
        await using var lease = await fixture.Inner.AcquireLeaseAsync(operationId);
        var held = new RouteRemoveHeldApplication(plan, operationId, lease);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(held, TestContext.Current.CancellationToken);
        Assert.NotNull(prepared.Preparation);
        fixture.Inner.SeedFrameworkClaim(RouteRemoveOutsideWorkspace.Leaf);
        var before = fixture.Inner.SnapshotHashes();
        var result = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(new RouteRemoveEffectApplicationInput
        {
            Plan = plan,
            Lease = lease,
            RecoveryPreparation = prepared,
        }, TestContext.Current.CancellationToken);
        Assert.NotEmpty(result.Findings);
        Assert.Equal(RouteRemoveRecoveryState.Retained, result.Recovery.State);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        Assert.True(File.Exists(prepared.Preparation.BundlePath));
    }

    [Fact]
    public async Task SurvivingRowsKeepBomSpacingAndLineEndingsByteExact()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var authored = "\uFEFF" + RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.RemovedRow
            + RouteRemoveOutsideWorkspace.SurvivingRows).Replace("Preserve introduction and row policy.", "Preserve introduction and row policy.  \t", StringComparison.Ordinal);
        Assert.Equal(SourceGeneratedEntriesState.Complete, SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(authored)).State);
        fixture.Inner.WriteBytes(fixture.Anchor, Encoding.UTF8.GetBytes(authored));
        var result = await fixture.Execute();
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(Encoding.UTF8.GetBytes(authored.Replace(RouteRemoveOutsideWorkspace.RemovedRow, string.Empty, StringComparison.Ordinal)), fixture.Inner.ReadBytes(fixture.Anchor));
    }

    [Fact]
    public async Task UnrelatedUnavailableRegistrationAndProtectedReadonlySiblingDoNotGrantOrBlockEffects()
    {
        using var fixture = new RouteRemoveOutsideWorkspace(listed: false);
        fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings(routes: [RouteRemoveOutsideWorkspace.DirectoryPath, "unavailable/other"]));
        fixture.Inner.SeedFrameworkClaim(RouteRemoveOutsideWorkspace.Survivor, region: true);
        var sibling = fixture.Inner.ReadBytes(RouteRemoveOutsideWorkspace.Survivor);
        var ownership = fixture.Inner.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath);
        var physical = fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Survivor);
        var attributes = File.GetAttributes(physical);
        File.SetAttributes(physical, attributes | FileAttributes.ReadOnly);
        try
        {
            var result = await fixture.Execute();
            Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        }
        finally
        {
            File.SetAttributes(physical, attributes);
        }
        Assert.Equal(sibling, fixture.Inner.ReadBytes(RouteRemoveOutsideWorkspace.Survivor));
        Assert.Equal(ownership, fixture.Inner.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath));
    }

    [Fact]
    public async Task OriginalIgnoredTargetScopeSurvivesCurrentOperationProof()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        const string incoming = "Before [kept label](docs/team/old.md) after.\n";
        fixture.Inner.WriteText("incoming.md", incoming);
        fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath,
            fixture.Settings($",\"{WorkspaceSettingsDefinitions.IgnoreLinksToProperty}\":[\"{RouteRemoveOutsideWorkspace.Leaf}\"],\"{WorkspaceSettingsDefinitions.RootsProperty}\":[\"independent.md\"]"));
        fixture.Inner.WriteText("independent.md", "Independent registered file\n");
        var result = await fixture.Execute();
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(incoming, fixture.Inner.ReadText("incoming.md"));
        Assert.Empty(fixture.Inner.ReadSettings().IgnoreLinksTo);
        Assert.Equal(["independent.md"], fixture.Inner.ReadSettings().Roots);
    }

    [Theory]
    [InlineData("leaf-link")]
    [InlineData("parent-link")]
    [InlineData("registration-link")]
    public async Task PhysicalAliasesNeverAcquireOutsideMutationAuthority(string kind)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        fixture.Inner.WriteText("foreign/file.md", "Foreign bytes\n");
        string link;
        if (kind == "leaf-link")
        {
            fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Leaf);
            link = fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Leaf);
            File.CreateSymbolicLink(link, "../../foreign/file.md");
        }
        else if (kind == "parent-link")
        {
            fixture.Inner.DeleteFile(fixture.Anchor);
            link = fixture.Inner.Combine(fixture.Anchor);
            File.CreateSymbolicLink(link, "../../foreign/file.md");
        }
        else
        {
            fixture.Inner.DeleteFile(fixture.Anchor);
            fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Leaf);
            fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Overwrite);
            fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Survivor);
            Directory.Delete(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.DirectoryPath));
            link = fixture.Inner.Combine(RouteRemoveOutsideWorkspace.DirectoryPath);
            Directory.CreateSymbolicLink(link, "../foreign");
        }
        try
        {
            var before = fixture.Inner.ReadBytes("foreign/file.md");
            var result = await fixture.Execute();
            Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
            Assert.NotEmpty(result.Findings);
            Assert.Empty(result.Effects);
            Assert.Equal(before, fixture.Inner.ReadBytes("foreign/file.md"));
            fixture.Inner.AssertNoLockInfrastructure();
        }
        finally
        {
            if (kind == "registration-link") Directory.Delete(link);
            else File.Delete(link);
        }
    }

    [Fact]
    public async Task IncludedOrdinaryCatalogueRowsUseOneBoundedSnapshot()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        const string path = RouteRemoveOutsideWorkspace.DirectoryPath + "/reading.md";
        var original = RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.RemovedRow + RouteRemoveOutsideWorkspace.SurvivingRows);
        fixture.Inner.WriteText(path, original);
        fixture.Inner.WriteText(fixture.Anchor, fixture.Inner.ReadText(fixture.Anchor) + "- [Reading](reading.md) - #Catalogue\r\n");
        var result = await fixture.Execute();
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(original.Replace(RouteRemoveOutsideWorkspace.RemovedRow, string.Empty, StringComparison.Ordinal), fixture.Inner.ReadText(path));
    }

    [Theory]
    [InlineData("settings")]
    [InlineData("readonly")]
    [InlineData("leaf")]
    [InlineData("row-policy")]
    public async Task LeaseRevalidationRejectsChangedInputs(string changed)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var plan = await fixture.Plan();
        await using var lease = await fixture.Inner.AcquireLeaseAsync(Guid.NewGuid());
        switch (changed)
        {
            case "settings": fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings(routes: [RouteRemoveOutsideWorkspace.DirectoryPath, "elsewhere"])); break;
            case "readonly": fixture.Inner.WriteText(RouteRemoveOutsideWorkspace.Survivor, "Changed supporting bytes\n"); break;
            case "leaf": fixture.Inner.WriteText(RouteRemoveOutsideWorkspace.Leaf, "Changed target bytes\n"); break;
            case "row-policy": fixture.Inner.WriteText(fixture.Anchor, fixture.Inner.ReadText(fixture.Anchor).Replace("#First", "#Changed", StringComparison.Ordinal)); break;
        }
        var before = fixture.Inner.SnapshotHashes();
        var result = await RouteRemoveOperationFactory.CreatePlanRevalidator().RevalidateAsync(plan, lease, TestContext.Current.CancellationToken);
        Assert.Equal(RouteRemovePlanRevalidationState.Changed, result.State);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Fact]
    public async Task OneSnapshotComposesProseAndRowsWithRecoveryOnce()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var original = fixture.Inner.ReadText(fixture.Anchor).Replace("# Independent parent", "Before [visible old](old.md) after.\r\n\r\n# Independent parent", StringComparison.Ordinal)
            + "\r\nTrailing [visible old](old.md) prose.\r\n";
        fixture.Inner.WriteText(fixture.Anchor, original);
        var plan = await fixture.Plan();
        Assert.Single(plan.Projection.FileChanges, change => change.LogicalPath == fixture.Inner.Combine(fixture.Anchor));
        var recovery = Assert.Single(plan.Projection.RecoveryTargets, target => target.Change.LogicalPath == fixture.Inner.Combine(fixture.Anchor));
        Assert.Equal(fixture.Inner.ReadBytes(fixture.Anchor), recovery.Before.Bytes.ToArray());
        var result = await fixture.Execute();
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(original.Replace("[visible old](old.md)", "visible old", StringComparison.Ordinal)
            .Replace(RouteRemoveOutsideWorkspace.RemovedRow, string.Empty, StringComparison.Ordinal), fixture.Inner.ReadText(fixture.Anchor));
    }

    [Theory]
    [InlineData("missing-exclusion")]
    [InlineData("orphan-overwrite")]
    [InlineData("retained-row")]
    public async Task IncompleteAbsenceCannotBecomeNoOp(string scenario)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Leaf);
        if (scenario != "orphan-overwrite") fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Overwrite);
        if (scenario != "retained-row") fixture.Inner.WriteText(fixture.Anchor, RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.SurvivingRows));
        if (scenario != "missing-exclusion") fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath,
            fixture.Settings($",\"{WorkspaceSettingsDefinitions.RemovedFilesProperty}\":[\"{RouteRemoveOutsideWorkspace.Leaf}\",\"{RouteRemoveOutsideWorkspace.Overwrite}\"]"));
        var before = fixture.Inner.SnapshotHashes();
        var result = await fixture.Execute();
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.NotEmpty(result.Findings);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }

    [Fact]
    public async Task OutsideSkillIsOrdinaryAndReadonlyForeignInputsRemainUntouched()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        const string skill = RouteRemoveOutsideWorkspace.DirectoryPath + "/" + SourceFormClassifier.NativeSkillFileName;
        fixture.Inner.WriteText(skill, "---\nname: independent\ndescription: independent skill\n---\n# Independent\n");
        var foreignBefore = fixture.Inner.ReadBytes("notes.md");
        var loaderBefore = fixture.Inner.ReadBytes(fixture.Layout.LoaderPath);
        var result = await fixture.Execute(skill);
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(RouteRemoveSubjectKind.Leaf, result.Subject.Kind);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + skill, result.Source.Id);
        Assert.False(File.Exists(fixture.Inner.Combine(skill)));
        Assert.Equal(foreignBefore, fixture.Inner.ReadBytes("notes.md"));
        Assert.Equal(loaderBefore, fixture.Inner.ReadBytes(fixture.Layout.LoaderPath));
    }

    [Fact]
    public async Task LateLeafRaceRetainsSettingsAndRecoveryWithoutRollback()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var plan = await fixture.Plan();
        var operationId = Guid.NewGuid();
        await using var lease = await fixture.Inner.AcquireLeaseAsync(operationId);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(new RouteRemoveHeldApplication(plan, operationId, lease), TestContext.Current.CancellationToken);
        Assert.NotNull(prepared.Preparation);
        fixture.Inner.WriteText(RouteRemoveOutsideWorkspace.Leaf, "Concurrent independent bytes\n");
        var result = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(new RouteRemoveEffectApplicationInput
        {
            Plan = plan,
            Lease = lease,
            RecoveryPreparation = prepared,
        }, TestContext.Current.CancellationToken);
        Assert.Equal(RouteRemoveRecoveryState.Retained, result.Recovery.State);
        Assert.Contains(result.Findings, finding => finding.Code == RouteRemoveFindingCode.TargetChangedDuringApply);
        Assert.Contains(RouteRemoveOutsideWorkspace.Leaf, fixture.Inner.ReadSettings().RemovedFiles);
        Assert.Equal("Concurrent independent bytes\n", fixture.Inner.ReadText(RouteRemoveOutsideWorkspace.Leaf));
        Assert.Equal(RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.SurvivingRows), fixture.Inner.ReadText(fixture.Anchor));
        Assert.Contains(result.Receipts.OfType<RouteRemoveFileChangeReceipt>(), receipt => receipt.Receipt.EffectState == FilesystemEffectState.NotStarted);
        Assert.True(File.Exists(prepared.Preparation.BundlePath));
    }

    [Fact]
    public async Task LateReadonlyInputChangeFailsFinalVerificationAndRetainsRecovery()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var plan = await fixture.Plan();
        var operationId = Guid.NewGuid();
        await using var lease = await fixture.Inner.AcquireLeaseAsync(operationId);
        var held = new RouteRemoveHeldApplication(plan, operationId, lease);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(held, TestContext.Current.CancellationToken);
        Assert.NotNull(prepared.Preparation);
        var progress = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(new RouteRemoveEffectApplicationInput
        {
            Plan = plan,
            Lease = lease,
            RecoveryPreparation = prepared,
        }, TestContext.Current.CancellationToken);
        Assert.Empty(progress.Findings);
        fixture.Inner.WriteText(RouteRemoveOutsideWorkspace.Survivor, "Concurrent readonly edit\n");
        var result = await RouteRemoveOperationFactory.CreateApplicationCompletion().CompleteAsync(held, prepared, progress, TestContext.Current.CancellationToken);
        Assert.Equal(RouteRemoveVerificationState.Failed, result.Verification);
        Assert.Equal(RouteRemoveRecoveryState.Retained, result.Recovery.State);
        Assert.True(File.Exists(prepared.Preparation.BundlePath));
        Assert.Equal("Concurrent readonly edit\n", fixture.Inner.ReadText(RouteRemoveOutsideWorkspace.Survivor));
    }
}

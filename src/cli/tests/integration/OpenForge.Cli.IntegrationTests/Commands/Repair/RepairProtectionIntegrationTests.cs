using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
public sealed class RepairProtectionIntegrationTests
{
    [Fact(DisplayName = "Managed generated Entries remain navigation and do not become protected authored-link findings")]
    public async Task GeneratedEntriesAreNotProtectedAuthoredLinks()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", "# Notes\n\nRead [Design](cli-design.md).\n");
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create().ExecuteAsync(
            new(fixture.Workspace, [], OpenForge.Cli.Core.Commands.Index.Models.Request.IndexMode.Apply), TestContext.Current.CancellationToken);
        Protect(fixture, "managed", ".agents/guidance/_guidance.md", ".agents/guidance/command-design.md");
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var result = await new RepairOperation(Components(fixture)).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(result.ReportFindings, finding => finding.Protection && finding.SourceCanonicalPath == ".agents/guidance/_guidance.md");
        Assert.Equal("open-forge repair --automatic", RepairReportSelector.Select(result, new(CliDetail.Standard)).Next?.Command);
    }

    [Theory]
    [InlineData("managed", false), InlineData("managed", true)]
    [InlineData("excluded", false), InlineData("excluded", true)]
    [InlineData("private", false), InlineData("private", true)]
    [InlineData("overwrite", false), InlineData("overwrite", true)]
    [InlineData("managed-target", false), InlineData("managed-target", true)]
    [InlineData("managed-missing-target", false), InlineData("managed-missing-target", true)]
    public async Task EveryProtectedBoundaryPreservesSource(string protection, bool explicitRelink)
    {
        using var fixture = new RepairRenameGitWorkspace();
        var source = protection == "overwrite" ? ".agents/guidance/notes.overwrite.md" : ".agents/guidance/notes.md";
        const string old = ".agents/guidance/rounding.md";
        const string successor = ".agents/guidance/money-rounding.md";
        const string text = "# Notes\n\nRead [Rounding](rounding.md).\n";
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [source] = text,
            [old] = "# Rounding\n\nKeep minor units.\n",
        });
        if (protection == "overwrite") fixture.Write(".agents/guidance/notes.md", "# Base\n");
        fixture.Move(old, successor);
        Protect(fixture, protection, source, successor);
        var components = Components(fixture);
        var request = new RepairRequest(fixture.Workspace, RepairMode.Apply, automatic: !explicitRelink,
            relinks: explicitRelink ? [new(source, 3, 17, "rounding.md", successor, null)] : [], allowInteraction: false);
        var result = await new RepairOperation(components).ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(text, File.ReadAllText(fixture.PathFor(source)));
        Assert.DoesNotContain(result.Plan?.Effects ?? [], effect => effect.SourceCanonicalPath == source);
    }

    [Theory]
    [InlineData("managed"), InlineData("excluded"), InlineData("private"), InlineData("managed-target")]
    public async Task ProtectionAddedAfterPlanningBlocksUnderLease(string protection)
    {
        using var fixture = new RepairRenameGitWorkspace();
        const string source = ".agents/guidance/notes.md";
        const string old = ".agents/guidance/rounding.md";
        const string successor = ".agents/guidance/money-rounding.md";
        const string text = "# Notes\n\nRead [Rounding](rounding.md).\n";
        await fixture.InitializeAsync(new Dictionary<string, string> { [source] = text, [old] = "# Rounding\n\nKeep minor units.\n" });
        fixture.Move(old, successor);
        var components = Components(fixture);
        var preview = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        Assert.Single(preview.Plan!.Selection.Selected);
        var original = preview.Plan;
        var plan = new OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan(new(fixture.Workspace, RepairMode.Apply, true, [], false),
            original.Selection, original.Steps, original.Conflicts, original.LibrarySteps)
        { Maintenance = original.Maintenance, RenameEvidence = original.RenameEvidence };
        Protect(fixture, protection, source, successor);
        var outcome = await components.Application.ExecuteAsync(plan, [], TestContext.Current.CancellationToken);
        Assert.Equal(OpenForge.Cli.Core.Commands.Repair.Models.Result.RepairPreflightState.Blocked, outcome.Preflight.State);
        Assert.Equal(text, File.ReadAllText(fixture.PathFor(source)));
    }

    private static void Protect(RepairRenameGitWorkspace fixture, string protection, string source, string successor)
    {
        if (protection == "excluded")
        {
            fixture.Write(".agents/open-forge.json", $"{{\"schemaVersion\":1,\"removedFiles\":[\"{source}\"]}}");
            return;
        }
        var ownership = WorkspaceOwnershipDocument.Empty;
        if (protection is "managed" or "managed-target" or "managed-missing-target")
        {
            var protectedPath = protection switch { "managed" => source, "managed-missing-target" => ".agents/guidance/rounding.md", _ => successor };
            ownership = ownership with { Framework = new(new("open-forge", null), [protectedPath], []) };
        }
        if (protection == "private")
            ownership = ownership with { Framework = new(new("open-forge", null), [], []) { GitIgnoredRoutes = [new(".agents/guidance", ".agents/guidance/_guidance.md")] } };
        File.WriteAllBytes(fixture.PathFor(".agents/open-forge.lock.json"), WorkspaceOwnershipCodec.Write(ownership));
    }

    private static RepairOperationComponents Components(RepairRenameGitWorkspace fixture)
    {
        var evidence = new RoutedMaintenanceRenameEvidenceAdapter(new RenameEvidenceReader(new PhysicalPathResolver(),
            new GitRenameEvidenceReader(configureProcess: fixture.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync));
        return RepairOperationFactory.CreateDefaultComponents(new(evidence.ReadEvidenceAsync, evidence));
    }

    [Theory(DisplayName = "Automatic and explicit Repair preserve a managed Skill and repair a user-owned Skill")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task ProtectedSkillSource(bool managed, bool explicitRelink)
    {
        using var fixture = new RepairRenameGitWorkspace();
        const string source = ".agents/skills/checks/SKILL.md";
        const string old = ".agents/guidance/rounding.md";
        const string successor = ".agents/guidance/money-rounding.md";
        const string destination = "../../guidance/rounding.md";
        var skill = "---\nname: checks\ndescription: Check workspace rules\n---\n\n# Checks\n\nRead [Rounding](../../guidance/rounding.md).\n";
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [source] = skill,
            [old] = "---\nopen-forge:\n  description: Monetary rounding policy\n  tags: [Guidance]\n---\n\n# Rounding\n\nKeep minor units.\n",
        });
        fixture.Write(".agents/skills/_skills.md", "---\nopen-forge:\n  description: Skills\n  tags: [Skill]\n---\n\n# Skills\n\n## Entries\n");
        File.AppendAllText(fixture.PathFor(".agents/loader.md"), "\n- [Skills](skills/_skills.md) - #Skill\n");
        if (managed)
        {
            var ownership = WorkspaceOwnershipDocument.Empty with { Framework = new(new("open-forge", null), [source], []) };
            File.WriteAllBytes(fixture.PathFor(".agents/open-forge.lock.json"), WorkspaceOwnershipCodec.Write(ownership));
        }
        fixture.Move(old, successor);
        var evidence = new RoutedMaintenanceRenameEvidenceAdapter(new RenameEvidenceReader(new PhysicalPathResolver(),
            new GitRenameEvidenceReader(configureProcess: fixture.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync));
        var operation = new RepairOperation(RepairOperationFactory.CreateDefaultComponents(new(evidence.ReadEvidenceAsync, evidence)));
        var request = new RepairRequest(fixture.Workspace, RepairMode.Apply, automatic: !explicitRelink,
            relinks: explicitRelink ? [new(source, 8, 17, destination, successor, null)] : [], allowInteraction: false);
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        if (!managed)
        {
            Assert.Equal(skill.Replace("rounding.md)", "money-rounding.md)", StringComparison.Ordinal), File.ReadAllText(fixture.PathFor(source)));
            return;
        }

        Assert.Equal(skill, File.ReadAllText(fixture.PathFor(source)));
        Assert.DoesNotContain(result.Plan?.Effects ?? [], effect => effect.SourceCanonicalPath == source);
        var report = RepairReportSelector.Select(result, new(CliDetail.Standard));
        var actions = report.Findings.SelectMany(finding => finding.Actions).Select(action => action.Command)
            .Append(report.Next?.Command ?? "");
        Assert.Contains(actions, command => command.StartsWith("open-forge update", StringComparison.Ordinal));
    }
}

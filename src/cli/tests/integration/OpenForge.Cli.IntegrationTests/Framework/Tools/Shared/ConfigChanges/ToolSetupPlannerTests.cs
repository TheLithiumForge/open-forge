using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-native-setup"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolSetupPlannerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "First-install native planning describes future commands and outputs without invoking an executable")]
    public async Task FirstInstall()
    {
        using var scenario = new ToolServiceScenario();
        var output = Output(ToolRecipeDefinitions.WorkspaceToken + "/future.json");
        var setup = ToolServiceScenario.Setup(output) with { DryRunArguments = ["preview"], NativePlan = Inventory() };
        var plan = await scenario.Service.PlanAsync(Request(scenario, setup), Token);
        Assert.Empty(plan.Problems);
        Assert.Equal(ToolNativePreviewKind.Description, plan.PreviewKind);
        Assert.Null(plan.DetectedClients);
        Assert.Empty(scenario.Calls);
        Assert.Null(plan.ExpectedFileSha256[scenario.Files.Combine("future.json")]);
        Assert.Equal(scenario.Invocation.PrefixArguments.Concat(new[] { "install", scenario.Context.NativeInstallDirectory }), plan.Command.Arguments);
        Assert.False(File.Exists(scenario.Files.Combine(ToolBackupPaths.Beside("future.json"))));
    }

    [Fact(DisplayName = "Native inventory keeps the complete detected set and filters only declared client supplements")]
    public async Task NativeInventory()
    {
        using var scenario = new ToolServiceScenario();
        var first = scenario.Files.Combine("first.json");
        scenario.Inventory = new() { DetectedClients = ["alpha", "beta", "gamma"], Paths = [first], Problems = [] };
        var setup = ToolServiceScenario.Setup(Output(ToolRecipeDefinitions.WorkspaceToken + "/first.json"),
            Output(ToolRecipeDefinitions.WorkspaceToken + "/alpha.json") with { ClientId = "alpha" }, Output(ToolRecipeDefinitions.WorkspaceToken + "/absent.json") with { ClientId = "absent" })
            with
        { NativePlan = Inventory() };
        var plan = await scenario.Service.PlanAsync(Request(scenario, setup) with { InstalledInvocation = scenario.Invocation }, Token);
        Assert.Empty(plan.Problems);
        Assert.Equal(ToolNativePreviewKind.Plan, plan.PreviewKind);
        Assert.Equal(scenario.Inventory.DetectedClients, plan.DetectedClients);
        Assert.Equal(2, plan.ExpectedFileSha256.Count);
        Assert.True(plan.ExpectedFileSha256.ContainsKey(first));
        Assert.False(plan.ExpectedFileSha256.ContainsKey(scenario.Files.Combine("absent.json")));
        Assert.Single(scenario.Calls);
    }

    [Fact(DisplayName = "Native inventory outside the reviewed possible output set blocks setup")]
    public async Task UndeclaredOutput()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Inventory = new() { DetectedClients = [], Paths = [scenario.Files.Combine("foreign.json")], Problems = [] };
        var plan = await scenario.Service.PlanAsync(Request(scenario, ToolServiceScenario.Setup(Output(ToolRecipeDefinitions.WorkspaceToken + "/known.json")) with { NativePlan = Inventory() })
            with
        { InstalledInvocation = scenario.Invocation }, Token);
        Assert.NotEmpty(plan.Problems);
        Assert.False(File.Exists(scenario.Files.Combine("foreign.json")));
    }

    [Fact(DisplayName = "Profile output patterns include absent final files in matched existing directories")]
    public async Task ExistingProfiles()
    {
        using var scenario = new ToolServiceScenario();
        Directory.CreateDirectory(scenario.Files.Combine("profiles/one"));
        Directory.CreateDirectory(scenario.Files.Combine("profiles/two"));
        var output = Output(ToolRecipeDefinitions.WorkspaceToken + "/profiles/*/mcp.json") with { Match = ToolNativeOutputMatch.ExistingPattern };
        var plan = await scenario.Service.PlanAsync(Request(scenario, ToolServiceScenario.Setup(output)), Token);
        Assert.Empty(plan.Problems);
        Assert.Equal(2, plan.ExpectedFileSha256.Count);
        Assert.Null(plan.ExpectedFileSha256[scenario.Files.Combine("profiles/one/mcp.json")]);
        Assert.Null(plan.ExpectedFileSha256[scenario.Files.Combine("profiles/two/mcp.json")]);
    }

    [Fact(DisplayName = "Native dry-run output remains descriptive and never supplies file destinations")]
    public async Task DryRun()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Run = _ => ToolServiceScenario.Success(scenario.Files.Combine("unreviewed.json"));
        var plan = await scenario.Service.PlanAsync(Request(scenario, ToolServiceScenario.Setup() with { DryRunArguments = ["preview"] }) with { InstalledInvocation = scenario.Invocation }, Token);
        Assert.Empty(plan.Problems);
        Assert.Equal(ToolNativePreviewKind.DryRun, plan.PreviewKind);
        Assert.Empty(plan.ExpectedFileSha256);
        Assert.Null(plan.DetectedClients);
    }

    [Theory(DisplayName = "Unknown setup tokens and invalid process bounds fail read-only preflight")]
    [InlineData(true), InlineData(false)]
    public async Task InvalidDeclarations(bool token)
    {
        using var scenario = new ToolServiceScenario();
        var setup = token ? ToolServiceScenario.Setup() with { Arguments = ["${unknown}"] } : ToolServiceScenario.Setup() with { MaximumOutputBytes = 0 };
        var plan = await scenario.Service.PlanAsync(Request(scenario, setup), Token);
        Assert.NotEmpty(plan.Problems);
        Assert.Empty(scenario.Calls);
    }

    [Fact(DisplayName = "Report-only directory outputs are disclosed without reading directory trees")]
    public async Task ReportOnly()
    {
        using var scenario = new ToolServiceScenario();
        var output = Output(ToolRecipeDefinitions.NativeHomeToken) with { Scope = ToolEffectScope.Account, Purpose = ToolNativeOutputPurpose.ReportOnly };
        var plan = await scenario.Service.PlanAsync(Request(scenario, ToolServiceScenario.Setup(output)), Token);
        Assert.Empty(plan.Problems);
        Assert.Single(plan.PossibleOutputs);
        Assert.Empty(plan.ExpectedFileSha256);
    }

    [Fact(DisplayName = "An occupied sibling backup blocks native setup preflight without creating any backup")]
    public async Task OccupiedBackup()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText("config.json", "{}");
        scenario.Files.WriteText(ToolBackupPaths.Beside("config.json"), "unrelated backup");
        var plan = await scenario.Service.PlanAsync(Request(scenario, ToolServiceScenario.Setup(Output(ToolRecipeDefinitions.WorkspaceToken + "/config.json"))), Token);
        Assert.NotEmpty(plan.Problems);
        Assert.Empty(scenario.Calls);
    }

    [Fact(DisplayName = "Replanning after native effects recognizes this operation's original backup")]
    public async Task OwnBackupContinuation()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText("config.json", "{}");
        var path = scenario.Files.Combine("config.json");
        var prepared = await scenario.Backups.PrepareAsync(new(path), Token);
        Assert.Null(prepared.Failure);
        await File.WriteAllTextAsync(path, "{\"native\":true}", Token);
        var plan = await scenario.Service.PlanAsync(Request(scenario, ToolServiceScenario.Setup(Output(ToolRecipeDefinitions.WorkspaceToken + "/config.json"))), Token);
        Assert.Empty(plan.Problems);
        Assert.True(File.Exists(prepared.BackupPath));
        Assert.Null((await scenario.Backups.CompleteAsync(new(prepared, plan.ExpectedFileSha256[path], Verified: true), Token)).Failure);
    }

    [Fact(DisplayName = "Workspace-scoped native outputs cannot escape through a relative path")]
    public async Task WorkspaceEscape()
    {
        using var scenario = new ToolServiceScenario();
        var plan = await scenario.Service.PlanAsync(Request(scenario, ToolServiceScenario.Setup(Output("../outside.json"))), Token);
        Assert.NotEmpty(plan.Problems);
        Assert.Empty(scenario.Calls);
    }

    private static ToolSetupRequest Request(ToolServiceScenario scenario, ToolSetupSpec setup) => new() { Setup = setup, Context = scenario.Context, Invocation = scenario.Invocation };
    private static ToolNativeOutputSpec Output(string path) => new() { PathTemplate = path, Scope = ToolEffectScope.Workspace, Match = ToolNativeOutputMatch.Exact, Purpose = ToolNativeOutputPurpose.Backup };
    private static ToolNativePlanSpec Inventory() => new() { Arguments = ["inventory"], Parser = ToolNativePlanParser.CbmInstallPlanV1, Timeout = TimeSpan.FromSeconds(5), MaximumOutputBytes = 4096 };
}

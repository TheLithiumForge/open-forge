using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Presentation.Extension.Install.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Extension.Install;

[Trait("Feature", "task32-presentation"), Trait("Evidence", "Unit")]
public sealed class Task32ExtensionInstallPermissionNextPolicyTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Install permission alternatives precede the final Next line at every detail and filter")]
    public void PermissionAlternativesRemainBeforeNextForEveryDetailAndFilter()
    {
        var result = PermissionRequiredResult();
        const string alternative = "  Or add \".apm/agents/team.md\" to allowInstallPaths in .agents/open-forge.json.";
        const string nextLine = "Next: open-forge extension install team --allow-path .apm/agents/team.md";
        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full, CliDetail.Debug })
            foreach (var filter in new CliSeverity?[] { null, CliSeverity.Error, CliSeverity.Warning })
            {
                var rendered = Render(result, detail, filter);
                var alternativeIndex = rendered.Text.IndexOf(alternative, StringComparison.Ordinal);
                var nextIndex = rendered.Text.IndexOf(nextLine, StringComparison.Ordinal);
                var finalLine = rendered.Text
                    .Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Last();

                Assert.False(rendered.Selected.ShowNext);
                Assert.Empty(rendered.Selected.Report.Limitations);
                Assert.Empty(rendered.Selected.TextCounts);
                Assert.All(rendered.Selected.Report.Counts, count => Assert.NotNull(count.Value));
                Assert.True(alternativeIndex >= 0);
                Assert.True(nextIndex > alternativeIndex);
                Assert.Equal(nextLine, finalLine);

                using var json = JsonDocument.Parse(rendered.Json);
                var next = json.RootElement.GetProperty("next");
                Assert.Equal("open-forge extension install team --allow-path .apm/agents/team.md", next.GetProperty("command").GetString());
                Assert.Equal(
                    "Or add \".apm/agents/team.md\" to allowInstallPaths in .agents/open-forge.json.",
                    next.GetProperty("reason").GetString());
                var permissions = json.RootElement.GetProperty("data").GetProperty("permissions");
                Assert.Equal(".apm/agents/team.md", Assert.Single(permissions.GetProperty("required").EnumerateArray()).GetString());
                Assert.Equal(".apm/agents/team.md", Assert.Single(permissions.GetProperty("missing").EnumerateArray()).GetString());
            }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Install initial force preview keeps its existing Next line")]
    public void InitialForceRequiredNextRemainsUnchanged()
    {
        var rendered = Render(
            Result(
                ExtensionInstallFindingCode.InitialForceRequired,
                WorkspacePermissionResult.NotEvaluated),
            CliDetail.Minimal,
            filter: null);
        var finalLine = rendered.Text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Last();

        Assert.Equal(
            "Next: open-forge extension install team --force --dry-run  (preview replacing it)",
            finalLine);
    }

    private static ExtensionInstallResult PermissionRequiredResult()
        => Result(
            ExtensionInstallFindingCode.PermissionRequired,
            new WorkspacePermissionResult(
                [".apm/agents/team.md"],
                [".apm/agents/team.md"],
                WorkspacePermissionDecision.Required,
                WorkspacePermissionAction.Create,
                WorkspacePermissionOutcome.Planned));

    private static ExtensionInstallResult Result(
        ExtensionInstallFindingCode findingCode,
        WorkspacePermissionResult permissions)
    {
        var workspace = new CliWorkspace(
            "install-workspace",
            "install-workspace",
            CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(
            workspace,
            ExtensionInstallMode.Apply,
            ["team"],
            all: false,
            sourcePath: null,
            force: false,
            automatic: true,
            allowInteraction: false);
        var facts = new ExtensionInstallResultFacts
        {
            Selection = new ExtensionInstallSelection(ExtensionInstallSelectionKind.ExplicitIds, ["team"]),
            Source = null,
            Packages = [new ExtensionInstallPackage("team", "1.0.0", selectedRoot: true, dependencies: [])],
            Framework = null,
            Footprint = new ExtensionInstallFootprint(1, [".apm/agents/team.md"], [], []),
            Effects = [],
            GeneratedNavigation = new ExtensionInstallGeneratedNavigation([]),
            Permissions = permissions,
            Lifecycle = new ExtensionInstallLifecycle(
                ExtensionInstallLifecycleAction.None,
                ExtensionInstallLifecycleOutcome.NotRequested),
            Recovery = new ExtensionInstallRecovery(
                ExtensionInstallRecoveryState.NotRequired,
                [],
                residualPath: null),
            Verification = new ExtensionInstallVerification(
                ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested),
        };

        return new ExtensionInstallResult(
            request,
            facts,
            [new ExtensionInstallFinding(findingCode, "The selected Extension needs a reviewed action.")]);
    }

    private static RenderedResult Render(
        ExtensionInstallResult result,
        CliDetail detail,
        CliSeverity? filter)
    {
        var filterValues = filter is { } severity
            ? new HashSet<CliSeverity> { severity }
            : null;
        var rendering = ExtensionInstallPresentation.Rendering;
        var selected = CliReportSelection.Select(result, new CliSelection(detail, filterValues), rendering);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
        var json = CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo);
        return new RenderedResult(selected, text, json);
    }

    private sealed record RenderedResult(
        CliSelectedReport<ExtensionInstallData> Selected,
        string Text,
        string Json);
}

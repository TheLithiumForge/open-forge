using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

[Trait("Feature", "route-init-outside"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInitCreateContinuationIntegrationTests
{
    [Theory(DisplayName = "Create missing-anchor guidance initializes its parsed operand then permits the original Create")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, RouteInitOutsideWorkspace.Directory)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, RouteInitOutsideWorkspace.Directory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, "outside/index")]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, "outside/references")]
    public async Task SuggestedInitOperandCompletesTheContinuation(string root, string directory)
    {
        using var workspace = new RouteInitOutsideWorkspace(root, listed: false, directory: directory);
        var target = directory + "/note" + SourceFormClassifier.MarkdownExtension;
        var selectedWorkspace = new CliWorkspace(workspace.Files.Path, workspace.Files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new RouteCreateRequest(selectedWorkspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + target,
            new(RouteInitOutsideWorkspace.Description, [RouteInitOutsideWorkspace.Tag], null), null, RouteCreateMode.Apply);
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var create = await RouteCreateOperationFactory.Create().ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, create.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.Contains(create.Findings, finding => finding.Code == RouteCreateFindingCode.ParentMissing);
        var report = RouteCreateReportSelector.Select(create, new CliSelection(CliDetail.Standard));
        var command = new Command(CliSyntaxDefinitions.ExecutableName);
        var route = RouteBinding.CreateGroup();
        command.Subcommands.Add(route);
        var symbols = RouteInitBinding.CreateSymbols(route);
        var workspaceOption = new Option<string?>(CliSyntaxDefinitions.Workspace.Name) { Recursive = true };
        command.Options.Add(workspaceOption);
        var parsed = command.Parse(Assert.IsType<OpenForge.Cli.Core.Shell.Pipeline.Models.Operation.CliNextAction>(report.Next).Command);
        Assert.Empty(parsed.Errors);
        Assert.Same(symbols.InitCommand, parsed.CommandResult.Command);
        Assert.Null(parsed.GetValue(workspaceOption));
        var operand = Assert.IsType<string>(parsed.GetValue(symbols.RouteTarget));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + WorkspaceRoutePath.CanonicalEntrypointPath(directory), operand);
        var initialized = await workspace.Execute(new(workspace.Workspace, operand,
            Core.Commands.Route.Init.Models.Request.RouteInitScaffold.Generic,
            Core.Commands.Route.Init.Models.Request.RouteInitMode.Apply,
            Core.Commands.Route.Init.Models.Request.RouteInitMetadataInput.None));
        Assert.Equal(CliSemanticStatus.Complete, initialized.Status);
        var plan = Assert.IsType<RouteCreatePlan>((await new RouteCreatePlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken)).Plan);
        try
        {
            var retried = await RouteCreateOperationFactory.Create().ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, retried.Status);
            Assert.Equal(RouteCreateVerificationState.Verified, retried.Verification);
            Assert.Equal(plan.IntendedTargetBytes.ToArray(), File.ReadAllBytes(workspace.Files.Combine(target)));
            workspace.AssertPreserved(before);
        }
        finally
        {
            if (File.Exists(workspace.Files.Combine(target))) File.Delete(workspace.Files.Combine(target));
        }
    }
}

using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Binding;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveDefinitionsAndBindingContractTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Remove definitions expose the exact one-operand write-policy grammar"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void DefinitionsExposeExactGrammar()
    {
        Assert.Equal("route remove", RouteRemoveDefinitions.CommandIdentity);
        Assert.Equal(1, RouteRemoveDefinitions.SchemaVersion);
        Assert.Equal("remove", RouteRemoveDefinitions.RemoveCommand.Name);
        Assert.Equal("source-reference", RouteRemoveDefinitions.SourceReference.Name);
        Assert.Equal("--dry-run", RouteRemoveDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.None, RouteRemoveDefinitions.DryRun.Arity);
        Assert.False(RouteRemoveDefinitions.DryRun.DefaultValue);
        Assert.Equal("--automatic", RouteRemoveDefinitions.Automatic.Name);
        Assert.Equal(CliOptionArity.None, RouteRemoveDefinitions.Automatic.Arity);
        Assert.False(RouteRemoveDefinitions.Automatic.DefaultValue);
        Assert.Equal("open-forge route remove", CommandLines.RouteRemove);
        Assert.Equal("open-forge route remove --help", RouteRemoveDefinitions.RouteRemoveHelpCommand);
        Assert.Equal("open-forge cleanup", CommandLines.Cleanup);
        Assert.Equal("open-forge doctor", CommandLines.Doctor);
        Assert.Equal("open-forge route remove --detail debug", RouteRemoveDefinitions.VerboseRouteRemoveCommand);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Remove definitions preserve literal descriptions and terminal option boundaries"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void DefinitionsPreserveDescriptionsAndTerminalBoundaries()
    {
        Assert.Contains("Remove one routed source or category", RouteRemoveDefinitions.RemoveCommand.Description, StringComparison.Ordinal);
        Assert.Contains("record the removal", RouteRemoveDefinitions.RemoveCommand.Description, StringComparison.Ordinal);
        Assert.Contains("release selected content ownership", RouteRemoveDefinitions.RemoveCommand.Description, StringComparison.Ordinal);
        Assert.Contains("ID or exact path", RouteRemoveDefinitions.SourceReference.Description, StringComparison.Ordinal);
        Assert.Contains("without writing", RouteRemoveDefinitions.DryRun.Description, StringComparison.Ordinal);
        Assert.Contains("without asking for confirmation", RouteRemoveDefinitions.Automatic.Description, StringComparison.Ordinal);
        Assert.Equal(CliOptionArity.None, RouteRemoveDefinitions.DryRun.Arity);
        Assert.Empty(RouteRemoveDefinitions.DryRun.FiniteSpellings);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove exposes every finite finding code exactly once"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void FindingCodesAreFiniteAndUnique()
    {
        var values = Enum.GetValues<RouteRemoveFindingCode>();

        Assert.Equal(values, RouteRemoveDefinitions.FindingCodes);
        Assert.Equal(values.Length, RouteRemoveDefinitions.FindingCodes.Distinct().Count());
        Assert.Equal(33, values.Length);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Remove automatic binding disables both interactive questions"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void AutomaticBindingRetainsApplyModeWithoutInteractiveQuestions()
    {
        var route = RouteBinding.CreateGroup();
        var binding = new RouteRemoveBinding(
            new RouteRemoveBindingValidator(),
            new RouteRemoveInvalidResultFactory());
        var symbols = binding.CreateSymbols(route);
        string[] arguments = ["remove", RouteRemoveTestData.LeafId, "--automatic"];
        var parse = route.Parse(arguments);
        var workspace = RouteRemoveTestData.Workspace("automatic-binding");
        var invocation = new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliFormat.Text, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);

        var result = binding.Bind(parse, invocation, symbols);
        var request = Assert.IsType<RouteRemoveRequest>(result.Request);

        Assert.Null(result.InvalidResult);
        Assert.Equal(RouteRemoveMode.Apply, request.Mode);
        Assert.True(request.Automatic);
        Assert.False(request.AllowInteractiveSourceSelection);
        Assert.False(request.AllowInteractiveConfirmation);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Remove exposes durable direct callable seams"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void DirectCallableSeamsRemainBoundedAndConstructible()
    {
        var operation = RouteRemoveOperationFactory.Create();
        Func<RouteRemoveRequest, CancellationToken, ValueTask<RouteRemoveResult>> execute = operation.ExecuteAsync;
        var builder = RouteRemoveOperationFactory.CreatePlanBuilder();
        Func<RouteRemoveRequest, CancellationToken, ValueTask<RouteRemovePlanBuild>> build = builder.BuildAsync;
        var revalidator = RouteRemoveOperationFactory.CreatePlanRevalidator();
        Func<RouteRemovePlan, WorkspaceLockLease, CancellationToken, ValueTask<RouteRemovePlanRevalidation>> revalidate =
            revalidator.RevalidateAsync;
        Func<RouteRemoveHeldApplication, CancellationToken, ValueTask<RouteRemoveRecoveryPreparationResult>> prepare =
            RouteRemoveRecoveryLifecycle.PrepareAsync;
        Func<RouteRemoveRecoveryDeletionInput, CancellationToken, ValueTask<RouteRemoveRecoveryDeletionResult>> delete =
            RouteRemoveRecoveryLifecycle.DeleteExactAsync;
        var effects = RouteRemoveOperationFactory.CreateEffectApplication();
        Func<RouteRemoveEffectApplicationInput, CancellationToken, ValueTask<RouteRemoveApplicationProgress>> apply =
            effects.ApplyAsync;
        var verifier = RouteRemoveOperationFactory.CreateAppliedVerifier();
        Func<RouteRemoveAppliedVerificationInput, CancellationToken, ValueTask<RouteRemoveAppliedVerification>> verify =
            verifier.VerifyAsync;

        Assert.NotNull(operation);
        Assert.NotNull(execute);
        Assert.NotNull(builder);
        Assert.NotNull(build);
        Assert.NotNull(revalidator);
        Assert.NotNull(revalidate);
        Assert.NotNull(prepare);
        Assert.NotNull(delete);
        Assert.NotNull(effects);
        Assert.NotNull(apply);
        Assert.NotNull(verifier);
        Assert.NotNull(verify);
    }
}

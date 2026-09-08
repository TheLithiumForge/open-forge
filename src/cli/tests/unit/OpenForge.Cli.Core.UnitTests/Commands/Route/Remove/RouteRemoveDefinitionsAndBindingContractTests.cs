using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveDefinitionsAndBindingContractTests
{
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
        Assert.Equal("open-forge route remove", RouteRemoveDefinitions.RouteRemoveCommand);
        Assert.Equal("open-forge route remove --help", RouteRemoveDefinitions.RouteRemoveHelpCommand);
        Assert.Equal("open-forge cleanup", RouteRemoveDefinitions.CleanupCommand);
        Assert.Equal("open-forge doctor", RouteRemoveDefinitions.DoctorCommand);
        Assert.Equal("open-forge route remove --verbose", RouteRemoveDefinitions.VerboseRouteRemoveCommand);
    }

    [Fact(DisplayName = "Route Remove definitions preserve literal descriptions and terminal option boundaries"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void DefinitionsPreserveDescriptionsAndTerminalBoundaries()
    {
        Assert.Contains("one eligible routed source or complete category", RouteRemoveDefinitions.RemoveCommand.Description, StringComparison.Ordinal);
        Assert.Contains("ID or exact path", RouteRemoveDefinitions.SourceReference.Description, StringComparison.Ordinal);
        Assert.Contains("without writing", RouteRemoveDefinitions.DryRun.Description, StringComparison.Ordinal);
        Assert.Equal(CliOptionArity.None, RouteRemoveDefinitions.DryRun.Arity);
        Assert.Empty(RouteRemoveDefinitions.DryRun.FiniteSpellings);
    }

    [Fact(DisplayName = "Route Remove exposes every finite finding code exactly once"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void FindingCodesAreFiniteAndUnique()
    {
        var values = Enum.GetValues<RouteRemoveFindingCode>();

        Assert.Equal(values, RouteRemoveDefinitions.FindingCodes);
        Assert.Equal(values.Length, RouteRemoveDefinitions.FindingCodes.Distinct().Count());
        Assert.Equal(30, values.Length);
    }

    [Fact(DisplayName = "Route Remove exposes durable direct callable seams"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void DirectCallableSeamsRemainBoundedAndConstructible()
    {
        var operation = new RouteRemoveOperation();
        Func<RouteRemoveRequest, CancellationToken, ValueTask<RouteRemoveResult>> execute = operation.ExecuteAsync;
        var builder = RouteRemovePlanBuilder.Create();
        Func<RouteRemoveRequest, CancellationToken, ValueTask<RouteRemovePlanBuild>> build = builder.BuildAsync;
        var revalidator = RouteRemovePlanRevalidator.Create();
        Func<RouteRemovePlan, WorkspaceLockLease, CancellationToken, ValueTask<RouteRemovePlanRevalidation>> revalidate =
            revalidator.RevalidateAsync;
        Func<RouteRemoveRecoveryPreparationInput, CancellationToken, ValueTask<RouteRemoveRecoveryPreparationResult>> prepare =
            RouteRemoveRecoveryLifecycle.PrepareAsync;
        Func<RouteRemoveRecoveryDeletionInput, CancellationToken, ValueTask<RouteRemoveRecoveryDeletionResult>> delete =
            RouteRemoveRecoveryLifecycle.DeleteExactAsync;
        var effects = RouteRemoveEffectApplication.Create();
        Func<RouteRemoveEffectApplicationInput, CancellationToken, ValueTask<RouteRemoveApplicationProgress>> apply =
            effects.ApplyAsync;
        var verifier = RouteRemoveAppliedVerifier.Create();
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

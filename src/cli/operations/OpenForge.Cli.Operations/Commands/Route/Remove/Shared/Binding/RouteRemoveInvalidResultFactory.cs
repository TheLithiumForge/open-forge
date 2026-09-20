using OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Binding;

internal sealed class RouteRemoveInvalidResultFactory
{
    internal RouteRemoveResult Create(RouteRemoveInvalidResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new RouteRemoveResultBuilder().Build(FormFormation(input));
    }

    private static RouteRemoveResultFormation FormFormation(RouteRemoveInvalidResultInput input)
    {
        var source = ReadRequested(input.SourceReference, RouteRemoveDefinitions.SourceReference.Name);
        return new RouteRemoveResultFormation
        {
            Workspace = input.Workspace,
            Mode = input.Mode,
            Source = new RouteRemoveSource { Requested = source },
            Subject = new RouteRemoveSubject(),
            Ownership = new RouteRemoveOwnership
            {
                State = RouteRemoveOwnershipState.NotEstablished,
                Framework = RouteRemoveOwnershipTrust.NotEstablished,
                Extensions = RouteRemoveOwnershipTrust.NotEstablished,
            },
            References = new RouteRemoveReferences
            {
                Coverage = RouteRemoveCoverage.NotEstablished,
                ScannedSourceCount = 0,
                InspectedSourceCount = 0,
                OccurrenceCount = 0,
            },
            GeneratedNavigation = new RouteRemoveGeneratedNavigation
            {
                Coverage = RouteRemoveCoverage.NotEstablished,
            },
            Plan = new RouteRemovePlanFacts
            {
                Completeness = RouteRemovePlanCompleteness.NotEstablished,
                Safety = RouteRemovePlanSafety.NotEstablished,
            },
            Recovery = new RouteRemoveRecovery
            {
                State = input.Mode == Models.Request.RouteRemoveMode.DryRun
                    ? RouteRemoveRecoveryState.NotCreated
                    : RouteRemoveRecoveryState.NotRequired,
            },
            Verification = RouteRemoveVerificationState.NotRequested,
            Findings =
            [
                new RouteRemoveFinding(
                    input.Failure.Code,
                    CliSemanticStatus.Invalid,
                    source,
                    input.Failure.Cause),
            ],
        };
    }

    private static string ReadRequested(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value;
}

using OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;

internal sealed class RouteMoveInvalidResultFactory
{
    internal RouteMoveResult Create(RouteMoveInvalidResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new RouteMoveResultBuilder().Build(FormFormation(input));
    }

    private static RouteMoveResultFormation FormFormation(RouteMoveInvalidResultInput input)
    {
        var source = ReadRequested(input.SourceReference, RouteMoveDefinitions.SourceReference.Name);
        var destination = ReadRequested(input.DestinationTarget, RouteMoveDefinitions.DestinationTarget.Name);
        return new RouteMoveResultFormation
        {
            Workspace = input.Workspace,
            Mode = input.Mode,
            Source = new RouteMoveSource { Requested = source },
            Destination = new RouteMoveDestination { Requested = destination },
            Subject = new RouteMoveSubject(),
            Ownership = new RouteMoveOwnership
            {
                State = RouteMoveOwnershipState.NotEstablished,
                Framework = RouteMoveOwnershipTrust.NotEstablished,
                Extensions = RouteMoveOwnershipTrust.NotEstablished,
            },
            References = new RouteMoveReferences
            {
                Coverage = RouteMoveCoverage.NotEstablished,
                ScannedSourceCount = 0,
                InspectedSourceCount = 0,
                OccurrenceCount = 0,
            },
            GeneratedNavigation = new RouteMoveGeneratedNavigation
            {
                Coverage = RouteMoveCoverage.NotEstablished,
            },
            Plan = new RouteMovePlanFacts
            {
                Completeness = RouteMovePlanCompleteness.NotEstablished,
                Safety = RouteMovePlanSafety.NotEstablished,
            },
            Recovery = new RouteMoveRecovery
            {
                State = input.Mode == Models.Request.RouteMoveMode.DryRun
                    ? RouteMoveRecoveryState.NotCreated
                    : RouteMoveRecoveryState.NotRequired,
            },
            Verification = RouteMoveVerificationState.NotRequested,
            Findings =
            [
                new RouteMoveFinding(
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

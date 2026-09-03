using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal sealed record PreviewObservation(
    RouteMoveSource Source,
    RouteMoveDestination Destination,
    RouteMoveSubjectKind? SubjectKind,
    ImmutableArray<RouteMoveSubjectLayer> Layers,
    ImmutableArray<RouteMoveSubjectItem> Items,
    RouteMovePlanFacts Plan,
    ImmutableArray<string> UnchangedPaths,
    RouteMoveRecoveryState RecoveryState,
    ImmutableArray<string> ProtectedPaths,
    string? ResidualPath,
    RouteMoveVerificationState Verification,
    ImmutableArray<RouteMoveFinding> Findings)
{
    internal bool Matches(PreviewObservation actual)
        => Source == actual.Source
            && Destination == actual.Destination
            && SubjectKind == actual.SubjectKind
            && Layers.SequenceEqual(actual.Layers)
            && Items.SequenceEqual(actual.Items)
            && Plan == actual.Plan
            && UnchangedPaths.SequenceEqual(actual.UnchangedPaths)
            && RecoveryState == actual.RecoveryState
            && ProtectedPaths.SequenceEqual(actual.ProtectedPaths)
            && ResidualPath == actual.ResidualPath
            && Verification == actual.Verification
            && Findings.SequenceEqual(actual.Findings);
}

internal sealed record EffectObservation(
    string Kind,
    FileExpectation Expectation,
    string IntendedBytes);

internal sealed record RecoveryObservation(
    PlannedFileChangeKind Kind,
    FileExpectation Expectation,
    string IntendedBytes,
    FileExpectation BeforeExpectation,
    string BeforeBytes);

internal sealed record SemanticExecutionProjection(
    PreviewObservation Preview,
    ImmutableArray<EffectObservation> Effects,
    ImmutableArray<RecoveryObservation> RecoveryTargets);

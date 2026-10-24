using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Presentation.Route.Create;
using OpenForge.Cli.Core.Presentation.Route.Init;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Create;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Init;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Move;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Wording;

public sealed partial class WorkspaceLockFailurePresentationTests
{
    private void VerifyOtherRoutes(WorkspaceLockFailureInfo failure, CliSemanticStatus status, string cause)
    {
        var cancelled = failure.Reason == WorkspaceLockFailureReason.Cancelled;
        Verify(RouteCreateTestData.Result(RouteCreateTestData.BoundaryFormation() with
        {
            Findings = [new RouteCreateFinding(cancelled ? RouteCreateFindingCode.Interrupted : RouteCreateFindingCode.WorkspaceLockUnavailable, cause)
            { LockFailure = failure }],
        }), RouteCreatePresentation.Rendering, failure, RouteCreateDefinitions.ReadMachineName(cancelled ? RouteCreateFindingCode.Interrupted : RouteCreateFindingCode.WorkspaceLockUnavailable), status);

        var initFinding = new RouteInitFinding(cancelled ? RouteInitFindingCode.Interrupted : RouteInitFindingCode.WorkspaceLockUnavailable, cause)
        { LockFailure = failure };
        Verify(RouteInitRedTestData.Result(RouteInitRedTestData.Formation(effects: [], findings: [initFinding]), [initFinding], status),
            RouteInitPresentation.Rendering, failure, RouteInitDefinitions.ReadMachineName(cancelled ? RouteInitFindingCode.Interrupted : RouteInitFindingCode.WorkspaceLockUnavailable), status);

        var moveFinding = new RouteMoveFinding(cancelled ? RouteMoveFindingCode.Interrupted : RouteMoveFindingCode.WorkspaceLockUnavailable, status, null, cause)
        { LockFailure = failure };
        Verify(new RouteMoveResult(RouteMoveTestData.Formation(findings: [moveFinding]), status, null),
            RouteMovePresentation.Rendering, failure, RouteMoveDefinitions.ReadMachineName(cancelled ? RouteMoveFindingCode.Interrupted : RouteMoveFindingCode.WorkspaceLockUnavailable), status);

        Verify(new RouteRemoveResult(RouteRemoveTestData.Formation() with
        {
            Effects = [],
            Findings = [new RouteRemoveFinding(cancelled ? RouteRemoveFindingCode.Interrupted : RouteRemoveFindingCode.WorkspaceLockUnavailable, status, null, cause)
            { LockFailure = failure }],
        }, status, null), RouteRemovePresentation.Rendering, failure, RouteRemoveDefinitions.ReadMachineName(cancelled ? RouteRemoveFindingCode.Interrupted : RouteRemoveFindingCode.WorkspaceLockUnavailable), status);
    }
}

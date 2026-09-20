using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

[Trait("Feature", "route-move-output"), Trait("Evidence", "Unit")]
public sealed class RouteMoveOwnershipOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Move ownership uncertainty is presented as a warning")]
    public void OwnershipUnavailableIsAWarning()
    {
        var formation = RouteMoveBoundary.Start(RouteMoveTestData.Request(mode: RouteMoveMode.DryRun)) with
        {
            Workspace = null,
            Source = RouteMoveTestData.Formation().Source,
            Subject = new RouteMoveSubject { Kind = RouteMoveSubjectKind.Leaf },
            Ownership = new RouteMoveOwnership
            {
                State = RouteMoveOwnershipState.NotEstablished,
                Framework = RouteMoveOwnershipTrust.NotEstablished,
                Extensions = RouteMoveOwnershipTrust.NotEstablished,
                Claims = [],
            },
        };
        var result = new RouteMoveResultBuilder().Build(RouteMoveBoundary.Stop(formation,
            RouteMoveFindingCode.OwnershipUnavailable, CliSemanticStatus.Complete,
            RouteMoveTestData.SourcePath, "The ownership lock does not establish a complete inventory; no unmanaged state was inferred."));
        var request = new CliPresentationRequest<RouteMoveResult>(result, new(CliFormat.Text, CliDetail.Minimal, null));

        var output = CliRenderingStage.Render(request, RouteMovePresentation.Rendering);

        Assert.Equal(CliOutputTarget.StandardOutput, output.PrimaryTarget);
        Assert.Contains("Would move ", output.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains("Ownership could not be read", output.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", output.PrimaryContent, StringComparison.Ordinal);
    }
}

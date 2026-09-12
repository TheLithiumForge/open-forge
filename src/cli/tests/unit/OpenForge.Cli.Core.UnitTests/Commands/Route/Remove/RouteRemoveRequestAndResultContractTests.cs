using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveRequestAndResultContractTests
{
    [Fact(DisplayName = "Route Remove request normalizes the two write modes and preserves the operand"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void RequestPreservesOperandAndWriteMode()
    {
        var apply = RouteRemoveTestData.Request(sourceReference: ".agents/guidance/old guide.md");
        var dryRun = RouteRemoveTestData.Request(mode: RouteRemoveMode.DryRun);

        Assert.Equal(".agents/guidance/old guide.md", apply.SourceReference);
        Assert.Equal(RouteRemoveMode.Apply, apply.Mode);
        Assert.False(apply.IsDryRun);
        Assert.Equal(RouteRemoveMode.DryRun, dryRun.Mode);
        Assert.True(dryRun.IsDryRun);
    }

    [Fact(DisplayName = "Route Remove request rejects blank operands and undefined modes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void RequestRejectsBlankOperandAndUndefinedMode()
    {
        var workspace = RouteRemoveTestData.Workspace("request-validation");

        Assert.Throws<ArgumentException>(
            () => new RouteRemoveRequest(workspace, " ", RouteRemoveMode.Apply));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RouteRemoveRequest(workspace, RouteRemoveTestData.LeafId, (RouteRemoveMode)int.MaxValue));
        Assert.Throws<ArgumentNullException>(
            () => new RouteRemoveRequest(null!, RouteRemoveTestData.LeafId, RouteRemoveMode.Apply));
    }

    [Fact(DisplayName = "Route Remove file path state accepts only lowercase SHA-256 fingerprints"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void FilePathStateRequiresLowercaseSha256()
    {
        var valid = RouteRemoveTestData.FileState();
        Assert.Equal(RouteRemovePathStateKind.File, valid.Kind);
        Assert.Equal(64, valid.ContentSha256?.Length);

        Assert.Throws<ArgumentException>(
            () => new RouteRemovePathState(RouteRemovePathStateKind.File, null));
        Assert.Throws<ArgumentException>(
            () => new RouteRemovePathState(RouteRemovePathStateKind.File, new string('A', 64)));
        Assert.Throws<ArgumentException>(
            () => new RouteRemovePathState(RouteRemovePathStateKind.File, new string('a', 63)));
        Assert.Throws<ArgumentException>(
            () => new RouteRemovePathState(RouteRemovePathStateKind.Missing, new string('a', 64)));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RouteRemovePathState((RouteRemovePathStateKind)int.MaxValue, null));
    }

    [Fact(DisplayName = "Route Remove finding validation keeps non-complete statuses and bounds causes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void FindingValidationKeepsNonCompleteStatusAndBoundsCause()
    {
        var longCause = new string('x', 400);
        var finding = RouteRemoveTestData.Finding(
            RouteRemoveFindingCode.ReferenceUnsafe,
            CliSemanticStatus.Blocked,
            cause: longCause);

        Assert.Equal(RouteRemoveFindingCode.ReferenceUnsafe, finding.Code);
        Assert.Equal(CliSemanticStatus.Blocked, finding.Status);
        Assert.Equal(256, finding.Cause.Length);
        Assert.Equal(RouteRemoveTestData.LeafPath, finding.Target);

        Assert.Throws<ArgumentException>(
            () => RouteRemoveTestData.Finding(
                RouteRemoveFindingCode.InvalidInput,
                CliSemanticStatus.Complete));
        Assert.Throws<ArgumentException>(
            () => RouteRemoveTestData.Finding(
                RouteRemoveFindingCode.InvalidInput,
                CliSemanticStatus.Invalid,
                target: string.Empty));
        Assert.Throws<ArgumentException>(
            () => RouteRemoveTestData.Finding(
                RouteRemoveFindingCode.InvalidInput,
                CliSemanticStatus.Invalid,
                cause: " "));
    }

    [Fact(DisplayName = "Route Remove result retains the typed graph and next action without projection loss"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void ResultRetainsTypedGraphAndNextAction()
    {
        var formation = RouteRemoveTestData.Formation();
        var next = new CliNextAction("open-forge route remove", "Rerun the same request.");
        var result = new RouteRemoveResult(
            formation,
            CliSemanticStatus.Complete,
            next);

        Assert.Equal("route remove", result.Command);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Same(formation.Workspace, result.Workspace);
        Assert.Same(formation.Source, result.Source);
        Assert.Same(formation.Subject, result.Subject);
        Assert.Same(formation.Ownership, result.Ownership);
        Assert.Same(formation.References, result.References);
        Assert.Same(formation.GeneratedNavigation, result.GeneratedNavigation);
        Assert.Same(formation.Plan, result.Plan);
        Assert.Same(formation.Recovery, result.Recovery);
        Assert.Same(next, result.Next);
    }
}

using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using StatusValueState = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusValueState;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusMeasurementAndPolicyTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status preserves available values and derives signed differences without fabricating unavailable values"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void AvailableMeasurementsPreserveProjectionAndSignedDifferences()
    {
        Assert.Equal(StatusResultSeeds.Available(0), StatusMeasurementCalculator.Project(StatusObservationSeeds.Available(0)));
        Assert.Equal(StatusResultSeeds.Available(42), StatusMeasurementCalculator.Project(StatusObservationSeeds.Available(42)));
        Assert.Equal(
            StatusResultSeeds.Available(-7),
            StatusMeasurementCalculator.Difference(StatusResultSeeds.Available(3), StatusResultSeeds.Available(10)));
        Assert.Equal(
            StatusResultSeeds.Available(0),
            StatusMeasurementCalculator.Difference(StatusResultSeeds.Available(10), StatusResultSeeds.Available(10)));

        var unavailable = new StatusIntegerValue(StatusValueState.Unavailable, null);
        var notApplicable = new StatusIntegerValue(StatusValueState.NotApplicable, null);
        Assert.Equal(
            unavailable,
            StatusMeasurementCalculator.Project(StatusObservationSeeds.Unavailable()));
        Assert.Equal(
            notApplicable,
            StatusMeasurementCalculator.Project(StatusObservationSeeds.NotApplicable()));
        Assert.Equal(
            unavailable,
            StatusMeasurementCalculator.Difference(StatusResultSeeds.Available(3), unavailable));
        Assert.Equal(
            unavailable,
            StatusMeasurementCalculator.Difference(unavailable, StatusResultSeeds.Available(3)));
        Assert.Equal(
            notApplicable,
            StatusMeasurementCalculator.Difference(notApplicable, StatusResultSeeds.Available(3)));
        Assert.Equal(
            notApplicable,
            StatusMeasurementCalculator.Difference(StatusResultSeeds.Available(3), notApplicable));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status startup percentage distinguishes numeric zero unavailable and not-applicable inputs"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void StartupPercentageDistinguishesNumericBothZeroUnavailableAndNotApplicable()
    {
        Assert.Equal(
            new StatusDecimalValue(StatusValueState.Available, 25m),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(25), StatusResultSeeds.Available(100)));
        Assert.Equal(
            new StatusDecimalValue(StatusValueState.Available, 0m),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(0), StatusResultSeeds.Available(100)));
        Assert.Equal(
            new StatusDecimalValue(StatusValueState.NotApplicable, null),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(0), StatusResultSeeds.Available(0)));
        Assert.Equal(
            new StatusDecimalValue(StatusValueState.Unavailable, null),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(1), StatusResultSeeds.Available(0)));
        Assert.Equal(
            new StatusDecimalValue(StatusValueState.Unavailable, null),
            StatusMeasurementCalculator.StartupPercentage(
                new StatusIntegerValue(StatusValueState.Unavailable, null),
                StatusResultSeeds.Available(100)));
        Assert.Equal(
            new StatusDecimalValue(StatusValueState.NotApplicable, null),
            StatusMeasurementCalculator.StartupPercentage(
                new StatusIntegerValue(StatusValueState.NotApplicable, null),
                StatusResultSeeds.Available(100)));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status result precedence selects the contracted semantic result without parsing cause text"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void StatusPrecedenceSelectsTheContractedSemanticResultWithoutParsingCauseText()
    {
        Assert.Equal(CliSemanticStatus.Complete, StatusResultPolicy.ReadStatus([]));
        foreach (var status in new[]
        {
            CliSemanticStatus.Attention,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Interrupted,
        })
        {
            Assert.Equal(status, StatusResultPolicy.ReadStatus([StatusResultSeeds.Finding(status)]));
        }

        Assert.Equal(
            CliSemanticStatus.Interrupted,
            StatusResultPolicy.ReadStatus([
                StatusResultSeeds.Finding(CliSemanticStatus.Attention),
                StatusResultSeeds.Finding(CliSemanticStatus.Incomplete),
                StatusResultSeeds.Finding(CliSemanticStatus.Blocked),
                StatusResultSeeds.Finding(CliSemanticStatus.Invalid),
                StatusResultSeeds.Finding(CliSemanticStatus.Failed),
                StatusResultSeeds.Finding(CliSemanticStatus.Interrupted),
            ]));
        Assert.Equal(
            CliSemanticStatus.Failed,
            StatusResultPolicy.ReadStatus([
                StatusResultSeeds.Finding(CliSemanticStatus.Attention),
                StatusResultSeeds.Finding(CliSemanticStatus.Incomplete),
                StatusResultSeeds.Finding(CliSemanticStatus.Blocked),
                StatusResultSeeds.Finding(CliSemanticStatus.Invalid),
                StatusResultSeeds.Finding(CliSemanticStatus.Failed),
            ]));
        Assert.Equal(
            CliSemanticStatus.Invalid,
            StatusResultPolicy.ReadStatus([
                StatusResultSeeds.Finding(CliSemanticStatus.Attention),
                StatusResultSeeds.Finding(CliSemanticStatus.Incomplete),
                StatusResultSeeds.Finding(CliSemanticStatus.Blocked),
                StatusResultSeeds.Finding(CliSemanticStatus.Invalid),
            ]));
        Assert.Equal(
            CliSemanticStatus.Blocked,
            StatusResultPolicy.ReadStatus([
                StatusResultSeeds.Finding(CliSemanticStatus.Attention),
                StatusResultSeeds.Finding(CliSemanticStatus.Incomplete),
                StatusResultSeeds.Finding(CliSemanticStatus.Blocked),
            ]));
        Assert.Equal(
            CliSemanticStatus.Incomplete,
            StatusResultPolicy.ReadStatus([
                StatusResultSeeds.Finding(CliSemanticStatus.Attention),
                StatusResultSeeds.Finding(CliSemanticStatus.Incomplete),
            ]));

        var incomplete = StatusResultSeeds.Finding(CliSemanticStatus.Incomplete);
        var sameCodeDifferentMessage = incomplete with { Cause = "Different words must not select a different action." };
        Assert.Null(StatusResultPolicy.ReadNext(CliSemanticStatus.Complete, []));
        AssertNext("open-forge doctor", CliSemanticStatus.Attention,
            [StatusResultSeeds.Finding(CliSemanticStatus.Attention)]);
        var incompleteNext = AssertNext("open-forge doctor", CliSemanticStatus.Incomplete, [incomplete]);
        var sameCodeNext = AssertNext("open-forge doctor", CliSemanticStatus.Incomplete, [sameCodeDifferentMessage]);
        Assert.Equal(incompleteNext, sameCodeNext);
        AssertNext("open-forge status --help", CliSemanticStatus.Invalid,
            [StatusResultSeeds.Finding(CliSemanticStatus.Invalid)]);
        AssertNext("open-forge status", CliSemanticStatus.Blocked,
            [StatusResultSeeds.Finding(CliSemanticStatus.Blocked)]);
        AssertNext("open-forge status --detail debug", CliSemanticStatus.Failed,
            [StatusResultSeeds.Finding(CliSemanticStatus.Failed)]);
        AssertNext("open-forge status", CliSemanticStatus.Interrupted,
            [StatusResultSeeds.Finding(CliSemanticStatus.Interrupted)]);

        var undefined = (CliSemanticStatus)int.MaxValue;
        var undefinedFinding = new StatusFinding
        {
            Code = StatusFindingCode.OperationFailed,
            Status = undefined,
            Subject = null,
            Cause = "An undefined semantic state is not accepted.",
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusResultPolicy.ReadStatus([undefinedFinding]));
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusResultPolicy.ReadNext(undefined, []));
    }

    private static CliNextAction AssertNext(
        string expectedCommand,
        CliSemanticStatus status,
        IReadOnlyList<StatusFinding> findings)
    {
        var next = Assert.IsType<CliNextAction>(StatusResultPolicy.ReadNext(status, findings));
        Assert.Equal(expectedCommand, next.Command);
        Assert.False(string.IsNullOrWhiteSpace(next.Reason));
        Assert.DoesNotContain('\r', next.Reason);
        Assert.DoesNotContain('\n', next.Reason);
        return next;
    }
}

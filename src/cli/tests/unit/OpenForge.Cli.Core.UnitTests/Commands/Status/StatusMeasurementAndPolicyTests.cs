using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusMeasurementAndPolicyTests
{
    [Fact(DisplayName = "Status derives signed differences and ceiling token estimates only from available measurements"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void AvailableMeasurementsDeriveSignedDifferenceAndCeilingTokenEstimate()
    {
        Assert.Equal(StatusResultSeeds.Available(0), StatusMeasurementCalculator.Project(StatusObservationSeeds.Available(0)));
        Assert.Equal(StatusResultSeeds.Available(42), StatusMeasurementCalculator.Project(StatusObservationSeeds.Available(42)));
        Assert.Equal(StatusResultSeeds.Available(0), StatusMeasurementCalculator.EstimateTokens(StatusObservationSeeds.Available(0)));
        Assert.Equal(StatusResultSeeds.Available(1), StatusMeasurementCalculator.EstimateTokens(StatusObservationSeeds.Available(1)));
        Assert.Equal(StatusResultSeeds.Available(1), StatusMeasurementCalculator.EstimateTokens(StatusObservationSeeds.Available(4)));
        Assert.Equal(StatusResultSeeds.Available(2), StatusMeasurementCalculator.EstimateTokens(StatusObservationSeeds.Available(5)));
        Assert.Equal(
            StatusResultSeeds.Available(-7),
            StatusMeasurementCalculator.Difference(StatusResultSeeds.Available(3), StatusResultSeeds.Available(10)));
        Assert.Equal(
            StatusResultSeeds.Available(0),
            StatusMeasurementCalculator.Difference(StatusResultSeeds.Available(10), StatusResultSeeds.Available(10)));

        var unavailable = new StatusIntegerValue(OperationalValueState.Unavailable, null);
        var notApplicable = new StatusIntegerValue(OperationalValueState.NotApplicable, null);
        Assert.Equal(
            unavailable,
            StatusMeasurementCalculator.Project(StatusObservationSeeds.Unavailable()));
        Assert.Equal(
            notApplicable,
            StatusMeasurementCalculator.Project(StatusObservationSeeds.NotApplicable()));
        Assert.Equal(
            unavailable,
            StatusMeasurementCalculator.EstimateTokens(StatusObservationSeeds.Unavailable()));
        Assert.Equal(
            notApplicable,
            StatusMeasurementCalculator.EstimateTokens(StatusObservationSeeds.NotApplicable()));
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

    [Fact(DisplayName = "Status startup percentage distinguishes numeric zero unavailable and not-applicable inputs"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void StartupPercentageDistinguishesNumericBothZeroUnavailableAndNotApplicable()
    {
        Assert.Equal(
            new StatusDecimalValue(OperationalValueState.Available, 25m),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(25), StatusResultSeeds.Available(100)));
        Assert.Equal(
            new StatusDecimalValue(OperationalValueState.Available, 0m),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(0), StatusResultSeeds.Available(100)));
        Assert.Equal(
            new StatusDecimalValue(OperationalValueState.NotApplicable, null),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(0), StatusResultSeeds.Available(0)));
        Assert.Equal(
            new StatusDecimalValue(OperationalValueState.Unavailable, null),
            StatusMeasurementCalculator.StartupPercentage(StatusResultSeeds.Available(1), StatusResultSeeds.Available(0)));
        Assert.Equal(
            new StatusDecimalValue(OperationalValueState.Unavailable, null),
            StatusMeasurementCalculator.StartupPercentage(
                new StatusIntegerValue(OperationalValueState.Unavailable, null),
                StatusResultSeeds.Available(100)));
        Assert.Equal(
            new StatusDecimalValue(OperationalValueState.NotApplicable, null),
            StatusMeasurementCalculator.StartupPercentage(
                new StatusIntegerValue(OperationalValueState.NotApplicable, null),
                StatusResultSeeds.Available(100)));
    }

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
        AssertNext("open-forge status --verbose", CliSemanticStatus.Failed,
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

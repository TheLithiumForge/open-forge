using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Shared.Evaluation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle.Operational.Shared.Evaluation;

[Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
public sealed class FrameworkLifecycleEvaluationTests
{
    [Theory(DisplayName = "Framework lifecycle presence retains all seven named storage states")]
    [InlineData((int)LifecycleStoreReadState.Available, (int)OperationalLifecyclePresenceState.Present)]
    [InlineData((int)LifecycleStoreReadState.Invalid, (int)OperationalLifecyclePresenceState.Present)]
    [InlineData((int)LifecycleStoreReadState.Blocked, (int)OperationalLifecyclePresenceState.Present)]
    [InlineData((int)LifecycleStoreReadState.DocumentMissing, (int)OperationalLifecyclePresenceState.Missing)]
    [InlineData((int)LifecycleStoreReadState.SectionMissing, (int)OperationalLifecyclePresenceState.Missing)]
    [InlineData((int)LifecycleStoreReadState.Unavailable, (int)OperationalLifecyclePresenceState.Unavailable)]
    [InlineData((int)LifecycleStoreReadState.Cancelled, (int)OperationalLifecyclePresenceState.Unavailable)]
    public void NamedStatesRetainTheirPresence(int state, int presence)
    {
        Assert.Equal((OperationalLifecyclePresenceState)presence, FrameworkLifecycleEvaluation.ReadPresence((LifecycleStoreReadState)state));
    }

    [Fact(DisplayName = "Framework lifecycle presence rejects an undefined storage state with its exact owned error")]
    public void UndefinedStateRetainsExactFailure()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            FrameworkLifecycleEvaluation.ReadPresence((LifecycleStoreReadState)int.MaxValue));

        Assert.Equal("state", exception.ParamName);
        Assert.Equal((LifecycleStoreReadState)int.MaxValue, exception.ActualValue);
        Assert.Equal("The Framework lifecycle presence state is not defined. (Parameter 'state')"
            + Environment.NewLine + "Actual value was 2147483647.", exception.Message);
    }
}

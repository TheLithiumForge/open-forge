using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Observation;

public sealed class ToolReadObserverTests
{
    [Theory(DisplayName = "Runtime and PATH unavailable records make Tool reads incomplete while missing remains distinct"),
     InlineData(false, (int)ToolRecordReadState.Invalid), InlineData(false, (int)ToolRecordReadState.Unavailable),
     InlineData(true, (int)ToolRecordReadState.Invalid), InlineData(true, (int)ToolRecordReadState.Unavailable),
     InlineData(false, (int)ToolRecordReadState.Missing), InlineData(true, (int)ToolRecordReadState.Missing),
     Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void RecordAvailability(bool path, int state)
    {
        var records = Records();
        records = path ? records with { PathRecordState = (ToolRecordReadState)state } : records with { RuntimeRecordsState = (ToolRecordReadState)state };
        var expected = state == (int)ToolRecordReadState.Missing ? CliSemanticStatus.Complete : CliSemanticStatus.Incomplete;
        Assert.Equal(expected, ToolReadObserver.Status(records, new([])));
    }

    internal static ToolRecordsResult Records() => new()
    {
        PortableState = ToolRecordReadState.Available,
        MachineState = ToolRecordReadState.Missing,
        WorkspaceMachineState = ToolRecordReadState.Missing,
        RuntimeRecordsState = ToolRecordReadState.Missing,
        PathRecordState = ToolRecordReadState.Missing,
        Machine = new Dictionary<string, ToolMachineState>(),
        Runtimes = new Dictionary<string, ToolNodeRuntimeRecord>(),
        Problems = [],
        ExpectedFileSha256 = new Dictionary<string, string?>(),
    };
}

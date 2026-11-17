using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Observation;

internal static class ToolObservationSeeds
{
    internal const string Id = "synthetic";
    internal static ToolObservation Create(ToolObservationState state) => new()
    {
        Entry = new()
        {
            Identity = new() { Id = Id, Name = "Synthetic tool", Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.invalid/tool"), Licence = "synthetic" },
            Profiles = [],
            Channels = [],
            Detection = new()
            {
                Command = Id,
                Arguments = [],
                Environment = new Dictionary<string, string?>(),
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 128,
                Parser = ToolVersionParser.StableSemVer
            },
        },
        Detection = new() { ToolId = Id },
        MachineRecordState = ToolRecordReadState.Missing,
        Path = new() { BinPath = Path.Combine(Path.GetTempPath(), "synthetic-bin"), Readiness = ToolPathReadiness.Missing, DetectedShellFiles = [] },
        Managers = [],
        State = state,
        Issues = [],
    };
}

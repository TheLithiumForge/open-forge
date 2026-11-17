using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Observation;

internal sealed class ToolReadObserver(ToolCatalogue catalogue, IToolRecordService records, ToolObservationReader observations, string homePath)
{
    internal ToolCatalogue Catalogue => catalogue;
    internal Task<ToolRecordsResult> ReadRecordsAsync(CliWorkspace workspace, CancellationToken token)
        => records.ReadAsync(new(workspace.PhysicalRoot, homePath), token);

    internal async Task<ToolReadPayload> ReadAsync(CliWorkspace workspace, ToolRecordsResult found, IReadOnlyList<string> ids, CancellationToken token)
    {
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, token).ConfigureAwait(false);
        var root = settings.Document.MainRoot;
        var tools = new List<ToolReadTool>();
        foreach (var id in ids.Order(StringComparer.Ordinal))
        {
            if (!catalogue.Entries.TryGetValue(id, out var entry))
            {
                tools.Add(new ToolReadTool { Id = id, Name = id, Origin = "unavailable", Upstream = string.Empty, Licence = string.Empty, State = "unsupported", Channels = [], Profiles = [] });
                continue;
            }
            tools.Add(ToolReadProjection.Observed(await observations.ReadAsync(new(entry, new(workspace.PhysicalRoot, homePath), found, root), token).ConfigureAwait(false)));
        }
        return new(tools);
    }

    internal static CliSemanticStatus Status(ToolRecordsResult records, ToolReadPayload payload)
    {
        if (records.PortableState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable) return CliSemanticStatus.Incomplete;
        if (records.MachineState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || records.RuntimeRecordsState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || records.PathRecordState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || records.WorkspaceMachineState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || payload.Tools.Any(tool => tool.State == "unavailable")) return CliSemanticStatus.Incomplete;
        if (records.Problems.Count > 0 || payload.Tools.Any(tool => tool.State is "missing" or "unsupported" or "changed")) return CliSemanticStatus.Attention;
        return CliSemanticStatus.Complete;
    }

    private static string Next(ToolReadTool tool) => tool.State switch
    {
        "missing" => "open-forge tool install",
        "changed" => $"open-forge tool install {tool.Id}",
        _ => $"open-forge tool inspect {tool.Id}",
    };

    internal static IReadOnlyList<ToolReadFinding> Findings(ToolRecordsResult records, ToolReadPayload payload, string unavailableCode)
        => records.Problems.Select(problem => new ToolReadFinding(unavailableCode, null, problem, null))
            .Concat(payload.Tools.Where(tool => tool.State != "current").Select(tool => new ToolReadFinding(
                unavailableCode, tool.Id, tool.DetectionFailure ?? tool.State ?? "unavailable", Next(tool)))).ToArray();
}

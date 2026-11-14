using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Observation;

// The caller runs the declared bounded process. This reader never uses native command advice.
internal sealed class ToolNativePlanReader : IToolNativePlanReader
{
    internal const string CbmPlanType = "agent.install.plan.v1";

    public ToolNativePlanObservation Read(ToolNativePlanParser parser, ToolProcessResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (parser != ToolNativePlanParser.CbmInstallPlanV1) throw new ArgumentOutOfRangeException(nameof(parser));
        if (result.ExitCode != 0 || result.TimedOut || result.Cancelled || result.Failure is not null)
            return Failed(result.Failure ?? "The native inventory process didn't return complete successful output.");
        try
        {
            var plan = JsonSerializer.Deserialize(result.StandardOutput, ToolNativePlanJsonContext.Default.ToolCbmNativePlan)
                ?? throw new JsonException("The native inventory requires an object.");
            if (plan.Type != CbmPlanType || plan.WritesStarted || plan.NetworkAfterInstall)
                throw new JsonException("The native inventory type or read-only facts are invalid.");
            var clients = Strings(plan.AgentsDetected);
            var files = new[] { plan.ConfigFilesPlanned, plan.InstructionFilesPlanned, plan.SkillFilesPlanned, plan.AgentFilesPlanned, plan.PromptFilesPlanned };
            var paths = files.SelectMany(Strings).ToList();
            if (plan.HooksPlanned is null) throw new JsonException("The hook inventory requires an array.");
            foreach (var hook in plan.HooksPlanned)
            {
                if (hook is null || string.IsNullOrWhiteSpace(hook.Agent) || string.IsNullOrWhiteSpace(hook.Path))
                    throw new JsonException("A native hook requires agent and path strings.");
                paths.Add(hook.Path);
            }
            if (paths.Any(path => !Path.IsPathFullyQualified(path)))
                throw new JsonException("Native inventory paths must be absolute.");
            if (plan.NextSafeCommand is null) throw new JsonException("The native inventory requires its command advice string.");
            return new()
            {
                DetectedClients = clients.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                Paths = paths.Distinct(PhysicalIdentityTracker.PathComparer).Order(StringComparer.Ordinal).ToArray(),
                Problems = [],
            };
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        { return Failed(exception.Message); }
    }

    private static string[] Strings(string[]? values)
    {
        if (values is null || values.Any(string.IsNullOrWhiteSpace)) throw new JsonException("Native inventory arrays require nonempty strings.");
        return values;
    }

    private static ToolNativePlanObservation Failed(string problem) => new() { DetectedClients = [], Paths = [], Problems = [problem] };
}

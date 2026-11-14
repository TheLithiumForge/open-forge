using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Observation;

public sealed class ToolNativePlanReaderTests
{
    [Fact(DisplayName = "Native inventory keeps every client and the deduplicated union of all file arrays and hooks"), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void CompleteInventory()
    {
        var plan = Plan();
        var result = Read(plan);
        Assert.Empty(result.Problems);
        Assert.Equal(plan.AgentsDetected.Order(), result.DetectedClients);
        Assert.Equal(6, result.Paths.Count);
        Assert.Contains(plan.PromptFilesPlanned[0], result.Paths);
        Assert.Contains(plan.HooksPlanned[0].Path, result.Paths);
    }

    [Theory(DisplayName = "Unsafe or incomplete native inventory yields no usable paths"), InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5), InlineData(6), InlineData(7),
     Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void InvalidInventory(int scenario)
    {
        var plan = Plan();
        var root = JsonSerializer.SerializeToNode(plan, ToolNativePlanJsonContext.Default.ToolCbmNativePlan) ?? throw new InvalidOperationException();
        switch (scenario)
        {
            case 0: root[Field(nameof(plan.Type))] = "other"; break;
            case 1: root[Field(nameof(plan.WritesStarted))] = true; break;
            case 2: root[Field(nameof(plan.NetworkAfterInstall))] = true; break;
            case 3: root.AsObject().Remove(Field(nameof(plan.PromptFilesPlanned))); break;
            case 4: root[Field(nameof(plan.SkillFilesPlanned))] = new JsonArray(17); break;
            case 5: root[Field(nameof(plan.HooksPlanned))] = new JsonArray(new JsonObject()); break;
            case 6: root[Field(nameof(plan.ConfigFilesPlanned))] = new JsonArray("relative-path"); break;
            case 7: root[Field(nameof(plan.AgentsDetected))] = null; break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }
        var result = new ToolNativePlanReader().Read(ToolNativePlanParser.CbmInstallPlanV1, Process(root.ToJsonString()));
        Assert.NotEmpty(result.Problems);
        Assert.Empty(result.Paths);
        Assert.Empty(result.DetectedClients);
    }

    [Theory(DisplayName = "Native process failure or truncated JSON never becomes an inventory"), InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4),
     Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void ProcessFailure(int scenario)
    {
        var process = Process(JsonSerializer.Serialize(Plan(), ToolNativePlanJsonContext.Default.ToolCbmNativePlan));
        process = scenario switch
        {
            0 => process with { ExitCode = 1 },
            1 => process with { TimedOut = true },
            2 => process with { Cancelled = true },
            3 => process with { Failure = "synthetic output bound" },
            4 => process with { StandardOutput = process.StandardOutput[..^2] },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        Assert.NotEmpty(new ToolNativePlanReader().Read(ToolNativePlanParser.CbmInstallPlanV1, process).Problems);
    }

    [Fact(DisplayName = "Native inventory reader rejects an unknown parser"), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void UnknownParser() => Assert.Throws<ArgumentOutOfRangeException>(() => new ToolNativePlanReader().Read((ToolNativePlanParser)(-1), Process("{}")));

    private static ToolNativePlanObservation Read(ToolCbmNativePlan plan)
        => new ToolNativePlanReader().Read(ToolNativePlanParser.CbmInstallPlanV1, Process(JsonSerializer.Serialize(plan, ToolNativePlanJsonContext.Default.ToolCbmNativePlan)));
    private static ToolProcessResult Process(string json) => new() { ExitCode = 0, TimedOut = false, Cancelled = false, StandardOutput = json, StandardError = "synthetic separate diagnostic" };
    private static string Field(string name) => JsonNamingPolicy.SnakeCaseLower.ConvertName(name);
    private static ToolCbmNativePlan Plan()
    {
        var paths = Enumerable.Range(0, 6).Select(index => Path.Combine(Path.GetTempPath(), "synthetic-plan", index.ToString(System.Globalization.CultureInfo.InvariantCulture))).ToArray();
        return new()
        {
            Type = ToolNativePlanReader.CbmPlanType,
            AgentsDetected = ["synthetic-registry", "synthetic-built-in"],
            ConfigFilesPlanned = [paths[0]],
            InstructionFilesPlanned = [paths[1], paths[2]],
            SkillFilesPlanned = [paths[2]],
            AgentFilesPlanned = [paths[3]],
            PromptFilesPlanned = [paths[4]],
            HooksPlanned = [new() { Agent = "Synthetic display label", Path = paths[5] }],
            WritesStarted = false,
            NetworkAfterInstall = false,
            NextSafeCommand = "synthetic-advice-must-never-run",
        };
    }
}

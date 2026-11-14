using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;

internal sealed record ToolWritePlanningInput(
    ToolWriteRequest Request,
    ToolRecordsResult Records,
    ToolWriteHostFacts Facts,
    IReadOnlyDictionary<string, ToolNpmClient> Managers);

internal sealed record ToolWriteToolPlan
{
    public required ToolCatalogueEntry Entry { get; init; }
    public required ToolDetectionResult Detection { get; init; }
    public ToolChannelResolution? Channel { get; init; }
    public ToolVersionProfile? Profile { get; init; }
    public ToolRecipeContext? Context { get; init; }
    public ToolSetupPlan? Native { get; init; }
    public ToolConfigPlan? Config { get; init; }
    public ToolConfigRemovePlan? Removal { get; init; }
    public string? Digest { get; init; }
    public bool NoOp { get; init; }
}

internal sealed record ToolWritePlan
{
    public required ToolWritePlanningInput Input { get; init; }
    public required IReadOnlyList<ToolWriteToolPlan> Tools { get; init; }
    public required IReadOnlyList<ToolWriteFinding> Findings { get; init; }
    public required IReadOnlyList<ToolWriteManagerQuestion> ManagerQuestions { get; init; }
    public required bool UsedNetwork { get; init; }
}

internal sealed record ToolWriteManagerChoice(int Value, string Name, string Version, string? GlobalBin, string PathReadiness);
internal sealed record ToolWriteManagerQuestion(string ToolId, IReadOnlyList<ToolWriteManagerChoice> Choices);

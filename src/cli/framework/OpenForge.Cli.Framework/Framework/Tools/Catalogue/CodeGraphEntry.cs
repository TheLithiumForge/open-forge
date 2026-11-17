using OpenForge.Cli.Core.Framework.Tools.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue;

/// <summary>
/// CodeGraph 1.6.2. The Tool setup design records the upstream evidence for every command here.
/// </summary>
internal static class CodeGraphEntry
{
    internal const string Id = "codegraph";
    internal const string Version = "1.6.2";
    internal const string Package = "@colbymchenry/codegraph";
    internal const string ConfigurationPath = "codegraph.json";
    internal const string ExcludeMember = "exclude";
    private const string Agents = "--location local --target claude,codex";

    internal static ToolEntry Create() => new()
    {
        Id = Id,
        Name = "CodeGraph",
        Purpose = "Gives coding agents a searchable graph of the code's symbols and calls.",
        Category = ToolDefinitions.CodeSearchCategory,
        Upstream = new("https://github.com/colbymchenry/codegraph"),
        Licence = "MIT",
        TestedVersion = Version,
        Method = ToolMethod.Npm,
        Detection = new($"{Id} --version"),
        Install = new([new($"npm i -g {Package}@{Version}")]),
        Setup = new([new($"{Id} install --yes {Agents}")], "Adds CodeGraph to Claude Code and Codex in this project")
        {
            Notes = ["CodeGraph keeps its default telemetry. Turn it off with: codegraph telemetry off"],
        },
        Uninstall = new(
            [new($"{Id} uninstall --yes {Agents} --keep-cli"), new($"npm uninstall -g {Package}")],
            "Removes CodeGraph from Claude Code and Codex in this project and uninstalls the codegraph command. Your project index stays."),
        Integrations = [new ToolJsonUnion(ConfigurationPath, "Keep archived Open Forge memory out of the graph", ExcludeMember, "/**")],
    };
}

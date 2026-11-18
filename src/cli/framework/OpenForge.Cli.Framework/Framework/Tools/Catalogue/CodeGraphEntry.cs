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
    // --yes alone means --location global, so the project scope is explicit. The target stays CodeGraph's own default:
    // with --yes, install uses its detected agents and uninstall uses all of them.
    private const string Project = "--location local";

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
        Setup = new([new($"{Id} install --yes {Project}")], "Adds CodeGraph to the coding agents it finds in this project")
        {
            Notes = ["CodeGraph keeps its default telemetry. Turn it off with: codegraph telemetry off"],
        },
        Uninstall = new(
            [new($"{Id} uninstall --yes {Project} --keep-cli"), new($"npm uninstall -g {Package}")],
            "Removes CodeGraph from every coding agent in this project and uninstalls the codegraph command. Your project index stays."),
        Integrations = [new ToolJsonUnion(ConfigurationPath, "Keep archived Open Forge memory out of the graph", ExcludeMember, "/**")],
    };
}

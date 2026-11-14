using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliToolHostInputs
{
    public required string HomePath { get; init; }
    public string? NativeHomePath { get; init; }
    public required IReadOnlyDictionary<string, string> ClientRoots { get; init; }
    public required IReadOnlyDictionary<string, string> ProcessEnvironment { get; init; }
    public required IToolProcessHost Processes { get; init; }
    public required IToolDownloadHost Downloads { get; init; }
    public required IToolPathHost Path { get; init; }
}

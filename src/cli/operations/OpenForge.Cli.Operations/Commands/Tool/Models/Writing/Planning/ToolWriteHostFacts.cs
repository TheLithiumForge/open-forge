using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;

internal sealed record ToolWriteHostFacts
{
    public required string HomePath { get; init; }
    public required string MainRoot { get; init; }
    public required string OpenForgeVersion { get; init; }
    public required IReadOnlyDictionary<string, string>? NativeEnvironment { get; init; }
    public required ToolTarget Target { get; init; }
    public required IReadOnlyDictionary<ToolNativeHomePolicy, string> NativeHomes { get; init; }
    public required IReadOnlyDictionary<string, string> ClientRoots { get; init; }
    public required IReadOnlyList<ToolNpmClient> ProjectManagers { get; init; }
    public required ToolPathFacts Path { get; init; }
}

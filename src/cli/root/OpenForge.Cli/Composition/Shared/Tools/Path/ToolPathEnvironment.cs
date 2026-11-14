using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;

namespace OpenForge.Cli.Composition.Shared.Tools.Path;

internal abstract class ToolPathEnvironment
{
    internal abstract bool IsWindows { get; }
    internal abstract string? ProcessPath { get; }
    internal abstract string? UserPath { get; set; }
}

internal sealed class ToolUserPathEnvironment : ToolPathEnvironment
{
    internal override bool IsWindows => OperatingSystem.IsWindows();
    internal override string? ProcessPath => Environment.GetEnvironmentVariable(ToolPathPolicy.PathVariable);
    internal override string? UserPath
    {
        get => Environment.GetEnvironmentVariable(ToolPathPolicy.PathVariable, EnvironmentVariableTarget.User);
        set => Environment.SetEnvironmentVariable(ToolPathPolicy.PathVariable, value, EnvironmentVariableTarget.User);
    }
}

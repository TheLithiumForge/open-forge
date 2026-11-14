using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;

internal sealed record ToolLaunchLocation(string Root, string Entry, string[] PrefixArguments);

internal sealed record ToolLifecycleDescription
{
    public required ToolLifecycle Lifecycle { get; init; }
    public ToolRegistration? Registration { get; init; }
    public required string Action { get; init; }
    public required ToolLaunchLocation Executable { get; init; }
    public required ToolCommandValues Values { get; init; }
    public required string[] Hosts { get; init; }
    public required Dictionary<string, string> Environment { get; init; }
}

// This describes consent inputs. It cannot be passed to a process host.
internal sealed record ToolProcessDescription
{
    public required string Action { get; init; }
    public required string? Host { get; init; }
    public required string ExecutableRoot { get; init; }
    public required string Executable { get; init; }
    public required string WorkingDirectory { get; init; }
    public required string[] Arguments { get; init; }
    public required Dictionary<string, string> Environment { get; init; }
    public required int TimeoutSeconds { get; init; }
}

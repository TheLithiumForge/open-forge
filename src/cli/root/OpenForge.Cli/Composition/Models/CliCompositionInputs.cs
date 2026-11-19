using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliCompositionInputs
{
    public required TextReader StandardInput { get; init; }

    public required TextWriter PromptOutput { get; init; }

    public required bool StandardInputRedirected { get; init; }

    public required bool PromptOutputRedirected { get; init; }

    public WorkspaceLockStoreRoot? LockStoreRoot { get; init; }
    public CliTerminal? Terminal { get; init; }
    public bool StandardErrorColor { get; init; }

    /// <summary>Runs Tool commands. Integration tests pass a fake so no real tool runs.</summary>
    public IToolProcessHost? ToolProcesses { get; init; }

    /// <summary>The platform whose catalogue commands Tool shows and runs. Tests pin it so their snapshots match on every OS.</summary>
    public ToolPlatform? ToolPlatform { get; init; }
}

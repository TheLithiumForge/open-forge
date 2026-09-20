using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
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
}

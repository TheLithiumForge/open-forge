namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

internal sealed record CliNextAction
{
    internal CliNextAction(string command, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Command = command;
        Reason = reason;
    }

    internal string Command { get; }

    internal string Reason { get; }
}

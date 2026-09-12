namespace OpenForge.Cli.Core.Shell.Invocation.Models;

internal sealed record CliProcessEnvironment
{
    internal CliProcessEnvironment(string currentDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentDirectory);
        CurrentDirectory = currentDirectory;
    }

    internal string CurrentDirectory { get; }
}

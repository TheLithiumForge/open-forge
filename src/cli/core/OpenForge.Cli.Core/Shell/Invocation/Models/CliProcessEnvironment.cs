namespace OpenForge.Cli.Core.Shell.Invocation.Models;

internal sealed record CliProcessEnvironment
{
    internal const int DefaultHelpWidth = 80;

    internal CliProcessEnvironment(string currentDirectory, int helpWidth = DefaultHelpWidth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentDirectory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(helpWidth);
        CurrentDirectory = currentDirectory;
        HelpWidth = helpWidth;
    }

    internal string CurrentDirectory { get; }

    internal int HelpWidth { get; }
}

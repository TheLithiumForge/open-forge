using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Hosting.Shared.Presentation;

internal static class CliHostColorPolicy
{
    internal static CliOutputColors Read()
        => Resolve(
            terminal: Environment.GetEnvironmentVariable("TERM"),
            noColor: Environment.GetEnvironmentVariable("NO_COLOR"),
            isWindows: OperatingSystem.IsWindows(),
            standardOutputRedirected: Console.IsOutputRedirected,
            standardErrorRedirected: Console.IsErrorRedirected);

    internal static CliOutputColors Resolve(string? terminal, string? noColor, bool isWindows, bool standardOutputRedirected, bool standardErrorRedirected)
    {
        if (isWindows || string.IsNullOrEmpty(terminal) || terminal == "dumb" || !string.IsNullOrEmpty(noColor))
        {
            return CliOutputColors.Plain;
        }

        return new CliOutputColors(StandardOutput: !standardOutputRedirected, StandardError: !standardErrorRedirected);
    }
}

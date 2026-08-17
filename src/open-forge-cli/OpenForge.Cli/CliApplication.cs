using System.CommandLine;

namespace OpenForge.Cli;

internal static class CliApplication
{
    internal static Task<int> RunAsync(
        string[] args,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        cancellationToken.ThrowIfCancellationRequested();

        var rootCommand = new RootCommand(
            "Open Forge maintains deterministic, reviewable Framework workspaces.");
        rootCommand.SetAction(_ => 0);

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach (var error in parseResult.Errors)
            {
                Console.Error.WriteLine(error.Message);
            }

            return Task.FromResult(4);
        }

        return Task.FromResult(parseResult.Invoke());
    }
}

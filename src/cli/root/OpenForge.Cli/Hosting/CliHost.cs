using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Hosting;

internal static class CliHost
{
    internal static ValueTask<int> RunAsync(
        string[] arguments,
        CancellationToken cancellationToken = default)
    {
        return RunAsync(
            arguments,
            Environment.CurrentDirectory,
            new CliOutputWriters(Console.Out, Console.Error),
            cancellationToken);
    }

    internal static async ValueTask<int> RunAsync(
        string[] arguments,
        string currentDirectory,
        CliOutputWriters writers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentException.ThrowIfNullOrWhiteSpace(currentDirectory);
        ArgumentNullException.ThrowIfNull(writers);
        var process = new CliProcessIdentity(
            CliSyntaxDefinitions.ExecutableName,
            CliBuildVersion.InformationalVersion);
        var application = CliCompositionRoot.Create(process);
        var completion = await application
            .RunAsync(
                arguments,
                new CliProcessEnvironment(currentDirectory),
                writers,
                cancellationToken)
            .ConfigureAwait(false);
        return completion.ExitCode;
    }
}

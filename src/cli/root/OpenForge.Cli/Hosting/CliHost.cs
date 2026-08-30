using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Composition;
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
        var writers = new CliOutputWriters(Console.Out, Console.Error);
        return RunApplicationAsync(
            arguments: arguments,
            currentDirectory: Environment.CurrentDirectory,
            writers: writers,
            application: CliCompositionRoot.Create(
                CreateProcessIdentity(),
                new CliCompositionInputs
                {
                    StandardInput = Console.In,
                    PromptOutput = writers.StandardError,
                    StandardInputRedirected = Console.IsInputRedirected,
                    PromptOutputRedirected = Console.IsErrorRedirected,
                }),
            cancellationToken: cancellationToken);
    }

    internal static ValueTask<int> RunAsync(
        string[] arguments,
        string currentDirectory,
        CliOutputWriters writers,
        CancellationToken cancellationToken = default)
    {
        return RunApplicationAsync(
            arguments: arguments,
            currentDirectory: currentDirectory,
            writers: writers,
            application: CliCompositionRoot.Create(CreateProcessIdentity()),
            cancellationToken: cancellationToken);
    }

    private static async ValueTask<int> RunApplicationAsync(
        string[] arguments,
        string currentDirectory,
        CliOutputWriters writers,
        CliCoreApplication application,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentException.ThrowIfNullOrWhiteSpace(currentDirectory);
        ArgumentNullException.ThrowIfNull(writers);
        var completion = await application
            .RunAsync(
                arguments,
                new CliProcessEnvironment(currentDirectory),
                writers,
                cancellationToken)
            .ConfigureAwait(false);
        return completion.ExitCode;
    }

    private static CliProcessIdentity CreateProcessIdentity()
    {
        return new CliProcessIdentity(
            CliSyntaxDefinitions.ExecutableName,
            CliBuildVersion.InformationalVersion);
    }
}

using OpenForge.Cli.Hosting.Shared.Presentation;
using OpenForge.Cli.Hosting.Shared.Interaction;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

namespace OpenForge.Cli.Hosting;

internal static class CliHost
{
    internal static ValueTask<int> RunAsync(
        string[] arguments,
        CancellationToken cancellationToken = default)
    {
        var writers = new CliOutputWriters(Console.Out, Console.Error) { Colors = CliHostColorPolicy.Read() };
        return RunApplicationAsync(
            arguments: arguments,
            environment: new CliProcessEnvironment(Environment.CurrentDirectory, ReadHelpWidth()),
            writers: writers,
            application: CliCompositionRoot.Create(
                CreateProcessIdentity(),
                new CliCompositionInputs
                {
                    StandardInput = Console.In,
                    PromptOutput = writers.StandardError,
                    StandardInputRedirected = Console.IsInputRedirected,
                    PromptOutputRedirected = Console.IsErrorRedirected,
                    Terminal = CliHostTerminalFactory.Create(
                        Console.In,
                        writers.StandardError,
                        Console.IsInputRedirected,
                        Console.IsErrorRedirected,
                        Environment.GetEnvironmentVariable("TERM"),
                        OperatingSystem.IsWindows()),
                    StandardErrorColor = writers.Colors.StandardError,
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
            environment: new CliProcessEnvironment(currentDirectory),
            writers: writers,
            application: CliCompositionRoot.Create(CreateProcessIdentity()),
            cancellationToken: cancellationToken);
    }

    private static async ValueTask<int> RunApplicationAsync(
        string[] arguments,
        CliProcessEnvironment environment,
        CliOutputWriters writers,
        CliCoreApplication application,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(writers);
        var completion = await application
            .RunAsync(
                arguments,
                environment,
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

    private static int ReadHelpWidth()
    {
        if (Console.IsOutputRedirected)
        {
            return CliProcessEnvironment.DefaultHelpWidth;
        }

        var width = Console.WindowWidth;
        return width > 0 ? width : CliProcessEnvironment.DefaultHelpWidth;
    }
}

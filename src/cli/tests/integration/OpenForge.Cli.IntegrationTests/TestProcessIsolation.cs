using System.Runtime.CompilerServices;
using OpenForge.Cli.IntegrationTests.Commands.Cleanup.Shared.Processes;
using OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking.Shared.Processes;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests;

/// <summary>
/// Points this test process at its own data home before any test runs. The suites share one
/// process and run their collections in parallel, so the redirection happens once here rather
/// than per test, and nothing this process does reaches the signed-in user's profile.
/// </summary>
internal static class TestProcessIsolation
{
    [ModuleInitializer]
    internal static void Redirect()
    {
        var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (arguments.FirstOrDefault() is WorkspaceLockProbe.ChildArgument or CleanupStorageProbe.ChildArgument)
        {
            try
            {
                var run = arguments[0] == WorkspaceLockProbe.ChildArgument
                    ? WorkspaceLockProbe.RunAsync(arguments)
                    : CleanupStorageProbe.RunAsync(arguments);
                run.GetAwaiter().GetResult();
                Environment.Exit(0);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                Environment.Exit(1);
            }
        }

        TestDataHome.RedirectCurrentProcess("OpenForge.Cli.IntegrationTests");
    }
}

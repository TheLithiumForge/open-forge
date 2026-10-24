using System.Runtime.CompilerServices;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.EndToEndTests;

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
        if (arguments.Length > 0 && arguments[0] == PublishedLockHolder.ChildArgument)
        {
            Environment.Exit(PublishedLockHolder.RunChild(arguments));
            return;
        }

        TestDataHome.RedirectCurrentProcess("OpenForge.Cli.EndToEndTests");
    }
}

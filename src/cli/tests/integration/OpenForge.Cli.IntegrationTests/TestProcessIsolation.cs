using System.Runtime.CompilerServices;
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
    internal static void Redirect() => TestDataHome.RedirectCurrentProcess("OpenForge.Cli.IntegrationTests");
}

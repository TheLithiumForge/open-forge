using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Process;

namespace OpenForge.Cli.EndToEndTests.Commands.Route.List;

public sealed class RouteListVerboseRefinementProcessTests
{
    [Fact(DisplayName = "Published route-list verbose human diagnostics preserve complete primary output exit and workspace bytes"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task VerboseHumanPreservesCompletePrimaryResult()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var before = workspace.Snapshot();
        var normal = await RunCliAsync(
            "route", "list", "root", "--workspace", workspace.Path, "--depth=all", "--view=expanded");
        var verbose = await RunCliAsync(
            "route", "list", "root", "--workspace", workspace.Path, "--depth=all", "--view=expanded", "--verbose");

        Assert.Equal(0, normal.ExitCode);
        Assert.Equal(normal.ExitCode, verbose.ExitCode);
        Assert.Equal(normal.Stdout, verbose.Stdout);
        Assert.Equal(string.Empty, normal.Stderr);
        AssertBoundedDiagnostics(verbose.Stderr);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Published route-list verbose JSON keeps one parseable complete document and diagnostics only on stderr"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task VerboseJsonPreservesCompletePrimaryResult()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var before = workspace.Snapshot();
        var normal = await RunCliAsync(
            "route", "list", "root", "--workspace", workspace.Path, "--depth=all", "--json");
        var verbose = await RunCliAsync(
            "route", "list", "root", "--workspace", workspace.Path, "--depth=all", "--json", "--verbose");

        Assert.Equal(0, normal.ExitCode);
        Assert.Equal(normal.ExitCode, verbose.ExitCode);
        Assert.Equal(normal.Stdout, verbose.Stdout);
        Assert.Equal(string.Empty, normal.Stderr);
        using var document = JsonDocument.Parse(verbose.Stdout);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        AssertBoundedDiagnostics(verbose.Stderr);
        Assert.DoesNotContain("diagnostic", verbose.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Published route-list verbose JSON preserves invalid and blocked primary results with only bounded stderr context"),
     InlineData("unknown/source", 4, "invalid"),
     InlineData("root/collision", 5, "blocked"),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task VerboseJsonPreservesNonCompletePrimaryResult(
        string sourceReference,
        int expectedExit,
        string expectedStatus)
    {
        using var workspace = PublishedRouteWorkspace.Create();
        if (expectedStatus == "blocked")
        {
            workspace.WriteText(
                ".agents/root/collision.md",
                "---\nopen-forge:\n  description: Collision leaf\n  tags: [Collision]\n---\n\n# Collision leaf\n");
            workspace.WriteText(
                ".agents/root/collision/_collision.md",
                "---\nopen-forge:\n  description: Collision entrypoint\n  tags: [Collision]\n---\n\n# Collision entrypoint\n");
        }

        var before = workspace.Snapshot();
        var normal = await RunCliAsync(
            "route", "list", sourceReference, "--workspace", workspace.Path, "--json");
        var verbose = await RunCliAsync(
            "route", "list", sourceReference, "--workspace", workspace.Path, "--json", "--verbose");

        Assert.Equal(expectedExit, normal.ExitCode);
        Assert.Equal(normal.ExitCode, verbose.ExitCode);
        Assert.Equal(normal.Stdout, verbose.Stdout);
        Assert.Equal(string.Empty, normal.Stderr);
        using var document = JsonDocument.Parse(verbose.Stdout);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        AssertBoundedDiagnostics(verbose.Stderr);
        Assert.Equal(before, workspace.Snapshot());
    }

    private static void AssertBoundedDiagnostics(string diagnostics)
    {
        Assert.InRange(diagnostics.Length, 1, 4096);
        Assert.DoesNotContain("System.", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(" at OpenForge", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("Unhandled exception", diagnostics, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception:", diagnostics, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", diagnostics, StringComparison.OrdinalIgnoreCase);
    }

    private static Task<CliProcessResult> RunCliAsync(params string[] arguments)
    {
        var environment = CliEndToEndEnvironment.ReadRequired();
        return CliProcessRunner.RunAsync(
            new CliProcessRequest(environment.ExecutablePath, arguments),
            TestContext.Current.CancellationToken);
    }
}

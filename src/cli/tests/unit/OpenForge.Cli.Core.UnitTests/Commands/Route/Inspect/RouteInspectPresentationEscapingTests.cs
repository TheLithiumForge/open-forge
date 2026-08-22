using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectPresentationEscapingTests
{
    [Fact(DisplayName = "Route Inspect escaping is deterministic bounded control-safe and retains safe Unicode and option-like values")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void EscapingBoundsHostileValues()
    {
        var hostile = "nul\0 control\u0001\u001f\r\n\t quotes=\" slash=\\ option=--workspace "
            + new string('x', 5000)
            + " Ω 東京";
        const string safe = "--workspace Ω 東京";

        var escaped = RouteInspectTextEscaping.Escape(hostile);
        var repeated = RouteInspectTextEscaping.Escape(hostile);
        var bounded = RouteInspectTextEscaping.Escape(hostile, 128);
        var clamped = RouteInspectTextEscaping.Clamp(escaped, 32);

        Assert.Equal(escaped, repeated);
        Assert.Contains("--workspace", escaped, StringComparison.Ordinal);
        Assert.Contains("Ω 東京", escaped, StringComparison.Ordinal);
        Assert.Equal(safe, RouteInspectTextEscaping.Escape(safe));
        AssertNoC0Controls(escaped);
        AssertNoC0Controls(bounded);
        AssertNoC0Controls(clamped);
        Assert.InRange(bounded.Length, 1, 128);
        Assert.InRange(clamped.Length, 1, 32);
    }

    [Fact(DisplayName = "Route Inspect bounded escaping preserves complete Unicode scalar boundaries")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void BoundedEscapingDoesNotSplitSurrogatePairs()
    {
        var bounded = RouteInspectTextEscaping.Escape("😀😀\0", 6);

        Assert.InRange(bounded.Length, 1, 6);
        AssertValidUtf16(bounded);
        AssertNoC0Controls(bounded);
    }

    [Fact(DisplayName = "Route Inspect diagnostics bound and redact hostile fields without changing primary output or result status")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DiagnosticsAreBoundedEscapedAndSeparateFromPrimaryRendering()
    {
        var result = RouteInspectPresentationTestData.HostileResult();
        var verbosePresentation = RouteInspectPresentationTestData.Presentation(
            result,
            CliView.Expanded,
            CliOutputFormat.Human,
            CliVerbosity.Verbose);
        var statusBefore = result.Status;
        const string primary = "fixed primary output";

        var rendered = CliRenderingStage.Render(
            verbosePresentation,
            new CliRendererSet<OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result.RouteInspectResult>(
                _ => primary,
                _ => "{}"),
            RouteInspectDiagnosticRenderer.Render);
        var diagnostics = rendered.DiagnosticContent;

        Assert.NotNull(diagnostics);
        Assert.InRange(diagnostics!.Length, 1, CliRenderingStage.MaximumDiagnosticLength);
        Assert.Contains("status=invalid", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("The source value was rejected", diagnostics, StringComparison.Ordinal);
        foreach (var line in diagnostics.Split(Environment.NewLine, StringSplitOptions.None))
        {
            AssertNoInjectedC0Controls(line);
            foreach (var value in line.Split('=', 2).Skip(1))
            {
                AssertNoInjectedC0Controls(value);
            }
        }

        Assert.Equal(result.Status, rendered.Status);
        Assert.Equal(primary, rendered.PrimaryContent);
        Assert.Equal(statusBefore, result.Status);
    }

    private static void AssertNoC0Controls(string value)
    {
        for (var code = 0; code <= 0x1f; code++)
        {
            Assert.DoesNotContain((char)code, value);
        }
    }

    private static void AssertNoInjectedC0Controls(string value)
    {
        for (var code = 0; code <= 0x1f; code++)
        {
            if (code == '\r' || code == '\n')
            {
                continue;
            }

            Assert.DoesNotContain((char)code, value);
        }
    }

    private static void AssertValidUtf16(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (char.IsHighSurrogate(value[index]))
            {
                Assert.True(index + 1 < value.Length);
                Assert.True(char.IsLowSurrogate(value[++index]));
                continue;
            }

            Assert.False(char.IsLowSurrogate(value[index]));
        }
    }
}

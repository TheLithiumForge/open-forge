using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Rendering;

public sealed class RouteListTextEscapingTests
{
    [Fact(DisplayName = "Route-list escaping emits deterministic control quote backslash and surrogate tokens"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EscapeEmitsDeterministicTokens()
    {
        var value = "prefix" + '\0' + '\u0001' + '"' + '\\' + "😀" + '\uD800' + 'x' + '\uDC00';

        var escaped = RouteListTextEscaping.Escape(value);

        Assert.Equal("prefix\\u0000\\u0001\\\"\\\\😀\\ud800x\\udc00", escaped);
        Assert.Equal("ordinary 😀", RouteListTextEscaping.Escape("ordinary 😀"));
        AssertValidUtf16(escaped);
    }

    [Theory(DisplayName = "Route-list escaping and clamping reject non-positive limits"), InlineData(0), InlineData(-1), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TextOperationsRequirePositiveLimits(int maximumLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteListTextEscaping.Escape("value", maximumLength));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteListTextEscaping.Clamp("value", maximumLength));
    }

    [Theory(DisplayName = "Route-list escaping uses exactly the requested small truncation marker"), InlineData(1, "."), InlineData(2, ".."), InlineData(3, "..."), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EscapeUsesExactSmallMarkers(int maximumLength, string expected)
    {
        var escaped = RouteListTextEscaping.Escape("value", maximumLength);

        Assert.Equal(expected, escaped);
        Assert.Equal(maximumLength, escaped.Length);
    }

    [Fact(DisplayName = "Route-list escaping reserves its marker and keeps each token or scalar whole"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EscapeKeepsWholeEmissionsWithinTotalBound()
    {
        var backslash = RouteListTextEscaping.Escape("a\\zQ!!", 6);
        var quote = RouteListTextEscaping.Escape("a\"zQ!!", 6);
        var control = RouteListTextEscaping.Escape("a\0b", 6);
        var scalar = RouteListTextEscaping.Escape("😀😀x!!", 6);
        var unpairedSurrogate = RouteListTextEscaping.Escape("a\uD800b", 6);

        Assert.Equal("a" + "\\\\" + "...", backslash);
        Assert.Equal("a" + "\\\"" + "...", quote);
        Assert.Equal("a...", control);
        Assert.Equal("😀...", scalar);
        Assert.Equal("a...", unpairedSurrogate);
        Assert.Equal(6, backslash.Length);
        Assert.Equal(6, quote.Length);
        Assert.Equal(4, control.Length);
        Assert.Equal(5, scalar.Length);
        Assert.Equal(4, unpairedSurrogate.Length);
        AssertValidUtf16(backslash);
        AssertValidUtf16(quote);
        AssertValidUtf16(control);
        AssertValidUtf16(scalar);
        AssertValidUtf16(unpairedSurrogate);
    }

    [Theory(DisplayName = "Route-list escaping treats configured limits as total UTF-16 bounds"), InlineData(RouteListTextEscaping.ShortValueLimit), InlineData(RouteListTextEscaping.DiagnosticValueLimit), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void EscapeLimitsAreTotalUtf16Bounds(int maximumLength)
    {
        var below = new string('x', maximumLength - 1);
        var at = new string('x', maximumLength);
        var above = new string('x', maximumLength + 1);

        Assert.Equal(below, RouteListTextEscaping.Escape(below, maximumLength));
        Assert.Equal(at, RouteListTextEscaping.Escape(at, maximumLength));

        var escaped = RouteListTextEscaping.Escape(above, maximumLength);
        Assert.Equal(maximumLength, escaped.Length);
        Assert.EndsWith("...", escaped, StringComparison.Ordinal);
        Assert.Equal(maximumLength - 3, escaped[..^3].Length);
    }

    [Theory(DisplayName = "Route-list raw clamping treats configured limits as total UTF-16 bounds"), InlineData(RouteListTextEscaping.ShortValueLimit), InlineData(RouteListTextEscaping.DiagnosticValueLimit), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ClampLimitsAreTotalUtf16Bounds(int maximumLength)
    {
        var below = new string('x', maximumLength - 1);
        var at = new string('x', maximumLength);
        var above = new string('x', maximumLength + 1);

        Assert.Same(below, RouteListTextEscaping.Clamp(below, maximumLength));
        Assert.Same(at, RouteListTextEscaping.Clamp(at, maximumLength));

        var clamped = RouteListTextEscaping.Clamp(above, maximumLength);
        Assert.Equal(maximumLength, clamped.Length);
        Assert.EndsWith("...", clamped, StringComparison.Ordinal);
        Assert.Equal(maximumLength - 3, clamped[..^3].Length);
    }

    [Fact(DisplayName = "Route-list raw clamping does not split supplementary scalars"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ClampKeepsSupplementaryScalarsWhole()
    {
        var clamped = RouteListTextEscaping.Clamp("😀😀xx", 5);
        var markerOnly = RouteListTextEscaping.Clamp("abcd", 3);
        var shorterMarker = RouteListTextEscaping.Clamp("abcd", 2);
        var shortestMarker = RouteListTextEscaping.Clamp("abcd", 1);

        Assert.Equal("😀...", clamped);
        Assert.Equal("...", markerOnly);
        Assert.Equal("..", shorterMarker);
        Assert.Equal(".", shortestMarker);
        Assert.Equal(5, clamped.Length);
        AssertValidUtf16(clamped);
    }

    [Fact(DisplayName = "Route-list diagnostics bound an actual hostile selection value without splitting an escape token"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DiagnosticSelectionUsesBoundedEscaping()
    {
        var hostile = "prefix" + '\\' + '"' + "😀" + new string('x', RouteListTextEscaping.DiagnosticValueLimit);
        var result = RouteListResult.Create(
            CliSemanticStatus.Invalid,
            new CliWorkspace(Path.GetTempPath(), Path.GetTempPath(), CliWorkspaceSelectionMethod.CurrentDirectory),
            RouteListSelectionFactory.AttemptedId(hostile),
            RouteListCoverage.NotStarted(null),
            [],
            [new RouteListFinding(
                RouteListFindingCode.InvalidDepth,
                CliSemanticStatus.Invalid,
                "depth",
                "The supplied depth is invalid.")],
            new CliNextAction("open-forge route list --help", "Correct the depth and retry."));

        var diagnostics = RouteListDiagnosticRenderer.Render(
            new CliPresentationRequest<RouteListResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Verbose)));
        var selectionLine = Assert.Single(
            diagnostics!.Split(Environment.NewLine, StringSplitOptions.None),
            line => line.StartsWith("selection.attempted-id=", StringComparison.Ordinal));
        var selectionValue = selectionLine["selection.attempted-id=".Length..];

        Assert.Equal(RouteListTextEscaping.DiagnosticValueLimit, selectionValue.Length);
        Assert.EndsWith("...", selectionValue, StringComparison.Ordinal);
        Assert.Contains("prefix" + "\\\\" + "\\\"" + "😀", selectionValue, StringComparison.Ordinal);
        AssertValidUtf16(selectionValue);
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

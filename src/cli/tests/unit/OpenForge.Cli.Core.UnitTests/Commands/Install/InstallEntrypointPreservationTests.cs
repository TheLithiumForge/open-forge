using System.Text;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallEntrypointPreservationTests
{
    [Theory, InlineData("\n"), InlineData("\r\n")]
    [Trait("Boundary", "Processing"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void PlainContentAndExistingCompanionRetainTheirBytesAndOrder(string newline)
    {
        var original = Encoding.UTF8.GetBytes($"\uFEFF# Local context{newline}{newline}Keep café and [guide](guide.md).{newline}");
        var companion = Encoding.UTF8.GetBytes($"# Existing customization{newline}Prefer local guidance.{newline}");

        var result = InstallEntrypointPreservation.Preserve(original, companion);

        Assert.Equal(original, result[..original.Length]);
        Assert.Equal(companion, result[^companion.Length..]);
        Assert.Equal(original, InstallEntrypointPreservation.Preserve(original, null));
    }

    [Fact]
    [Trait("Boundary", "Processing"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void GeneratedNavigationIsOmittedWhileAuthoredProseSurvives()
    {
        const string original = "# Local rules\n\nKeep this rule.\n\n## Entries\n\n- [Old route](old.md)\n\n## Notes\n\nKeep these notes.\n";

        var result = Encoding.UTF8.GetString(InstallEntrypointPreservation.Preserve(
            Encoding.UTF8.GetBytes(original), null));

        Assert.Contains("Keep this rule.", result, StringComparison.Ordinal);
        Assert.Contains("## Notes\n\nKeep these notes.", result, StringComparison.Ordinal);
        Assert.DoesNotContain("old.md", result, StringComparison.Ordinal);
        Assert.DoesNotContain("## Entries", result, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Boundary", "Processing"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void AmbiguousNavigationIsPreservedRatherThanDiscarded()
    {
        var original = Encoding.UTF8.GetBytes("# Local\n\n## Entries\n\nKeep this prose.\n\n## Entries\n\nAlso keep this.\n");

        Assert.Equal(original, InstallEntrypointPreservation.Preserve(original, null));
    }
}

using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Stripping;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Uninstall;

public sealed partial class UninstallOutputSnapshotTests
{
    [Fact(DisplayName = "Uninstall previews keep their text and JSON plan at every detail level"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task Preview()
    {
        using var workspace = await UninstallIntegrationTests.InstalledAsync("uninstall-output-preview");

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Metadata], dryRun: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        // Framework file hashes follow the bundled payload, so the snapshot keeps their place, not their value.
        CommandOutputRenderers<StripResult>.From(StripPresentation.Rendering,
            normalizeText: Normalize,
            normalizeJson: Normalize).MatchDetails(result, "preview");
    }

    private static string Normalize(string text) => Sha256().Replace(text, "<sha256>");

    [GeneratedRegex("[0-9a-f]{64}")]
    private static partial Regex Sha256();
}

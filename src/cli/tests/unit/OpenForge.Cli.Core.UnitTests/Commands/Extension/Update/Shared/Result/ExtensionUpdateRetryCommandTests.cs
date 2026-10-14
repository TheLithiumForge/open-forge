using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update.Shared.Result;

public sealed class ExtensionUpdateRetryCommandTests
{
    [Theory(DisplayName = "Extension Update confirmation and prune previews preserve every selected root and request option")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void CompleteRequestSurvivesRetry(bool all)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "selected workspace's"));
        var request = new ExtensionUpdateRequest(new(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace), ExtensionUpdateMode.Apply,
            all ? [] : ["base", "toolkit"], all, "source folder's", force: true, prune: false, automatic: false, allowInteraction: false,
            allowPath: ["team folder"])
        { RetryOptions = new(CliFormat.Json, CliDetail.Standard, new HashSet<CliSeverity> { CliSeverity.Warning }) };
        var prefix = $"open-forge extension update {(all ? "--all" : "base toolkit")} --source {CliCommandText.Quote("source folder's")} --workspace {CliCommandText.Quote(root)} --allow-path 'team folder' --force";
        Assert.Equal($"{prefix} --automatic --format json --detail standard --detail-filter warning", ExtensionUpdateRetryCommand.From(request, confirmationRequired: true));
        Assert.Equal($"{prefix} --prune --dry-run --format json --detail standard --detail-filter warning", ExtensionUpdateRetryCommand.From(request, prunePreview: true));
    }
}

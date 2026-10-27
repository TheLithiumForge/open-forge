using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using static OpenForge.Cli.Core.Framework.Distribution.Shared.Content.WorkspaceLayoutPayloadGuidance;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Content;

[Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class WorkspaceLayoutPayloadGuidanceTests
{
    private static readonly string DiscoveryPath = Reference(DiscoveryReferenceName);
    private static readonly string RoutesPath = Reference(RoutesReferenceName);

    [Fact(DisplayName = "Every guidance replacement anchor still occurs in its shipped embedded template")]
    public void ShippedTemplatesRetainReplacementAnchors()
    {
        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        (string Name, string Path, string Value)[] anchors =
        [
            (nameof(ExampleContributionPath), SourceLogicalPath.HumanGuidePath, ExampleContributionPath),
            (nameof(ExampleExcludedPath), SourceLogicalPath.HumanGuidePath, ExampleExcludedPath),
            (nameof(ExampleRepairNotePath), RoutesPath, ExampleRepairNotePath),
            (nameof(ExampleRepairTargetPath), RoutesPath, ExampleRepairTargetPath),
            (nameof(ContextExamplePrefix), DiscoveryPath, ContextExamplePrefix + SourceLogicalPath.HumanGuidePath),
            (nameof(ExcludeCategoryTemplate), RoutesPath, ExcludeCategoryTemplate),
        ];
        foreach (var anchor in anchors)
        {
            var asset = Assert.IsType<FrameworkPayloadAsset>(payload.Find(anchor.Path));
            var template = Encoding.UTF8.GetString(asset.Bytes.AsSpan());
            Assert.True(template.Contains(anchor.Value, StringComparison.Ordinal), anchor.Name);
        }
    }

    private static string Reference(string name)
        => CliSkillDirectory + "/" + ReferencesDirectoryName + "/" + name + SourceFormClassifier.MarkdownExtension;
}

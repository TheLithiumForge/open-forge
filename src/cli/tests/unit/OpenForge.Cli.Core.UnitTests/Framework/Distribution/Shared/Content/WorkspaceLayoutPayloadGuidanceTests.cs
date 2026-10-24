using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using static OpenForge.Cli.Core.Framework.Distribution.Shared.Content.WorkspaceLayoutPayloadGuidance;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Content;

[Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class WorkspaceLayoutPayloadGuidanceTests
{
    private const string RootCode = "`" + WorkspaceLayout.DefaultMainRoot + "`";
    private const string RootDirectoryCode = "`" + WorkspaceLayout.DefaultMainRoot + "/`";
    private static readonly string CommonPath = Reference(CommonReferenceName);
    private static readonly string DiscoveryPath = Reference(DiscoveryReferenceName);
    private static readonly string RoutesPath = Reference(RoutesReferenceName);
    private static readonly string PackagesPath = Reference(PackagesReferenceName);

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
            (nameof(PreviousSchemaSentence), PackagesPath, PreviousSchemaSentence),
            (nameof(ExactPathIntroduction), DiscoveryPath, ExactPathIntroduction + RootDirectoryCode),
            (nameof(InitPathIntroduction), RoutesPath, InitPathIntroduction + RootCode),
            (nameof(MovePathIntroduction), RoutesPath, MovePathIntroduction + RootCode),
            (nameof(OutsidePathIntroduction), CommonPath, OutsidePathIntroduction + RootCode),
            (nameof(OutsidePathIntroduction), PackagesPath, OutsidePathIntroduction + RootCode),
            (nameof(IncludeCategoryTemplate), RoutesPath, IncludeCategoryTemplate),
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

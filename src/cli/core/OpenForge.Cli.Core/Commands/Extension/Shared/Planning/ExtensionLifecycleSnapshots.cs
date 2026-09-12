using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Planning;

internal static class ExtensionLifecycleSnapshots
{
    internal static FrameworkLifecycleState Framework(FrameworkLifecycleState value)
        => new()
        {
            Coverage = value.Coverage,
            Source = new FrameworkLifecycleSource
            {
                Id = value.Source.Id,
                Version = value.Source.Version,
                InventoryFingerprint = value.Source.InventoryFingerprint,
            },
            Targets =
            [
                .. value.Targets.Select(target => new FrameworkLifecycleTarget
                {
                    Path = target.Path,
                    SourceAssetPath = target.SourceAssetPath,
                    Region = target.Region,
                    BaselineFingerprint = target.BaselineFingerprint,
                    FingerprintKind = target.FingerprintKind,
                }),
            ],
            GeneratedRegions =
            [
                .. value.GeneratedRegions.Select(region => new FrameworkGeneratedRegion
                {
                    Path = region.Path,
                    Region = region.Region,
                }),
            ],
        };

    internal static ExtensionLifecycleState Extensions(ExtensionLifecycleState value)
        => new()
        {
            Coverage = value.Coverage,
            Packages =
            [
                .. value.Packages.Select(package => new LifecycleExtensionPackageV1
                {
                    Id = package.Id,
                    Version = package.Version,
                    Source = package.Source,
                    Dependencies = [.. package.Dependencies],
                    Paths = [.. package.Paths],
                }),
            ],
            Paths =
            [
                .. value.Paths.Select(path => new LifecycleExtensionPathV1
                {
                    Path = path.Path,
                    Owners = [.. path.Owners],
                    BaselineFingerprint = path.BaselineFingerprint,
                    FingerprintKind = path.FingerprintKind,
                }),
            ],
        };
}

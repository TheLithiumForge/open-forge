using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;

internal static class FrameworkPayloadSourceProjection
{
    internal static SourceLogicalSource Create(
        CliWorkspace workspace,
        FrameworkPayloadAsset asset)
    {
        if (!SourceFormClassifier.TryClassify(asset.Path, out var form))
        {
            throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' is not a recognized authored source.");
        }

        var id = SourceIdentity.DeriveId(asset.Path)
            ?? throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' has no canonical source identity.");
        var physicalPath = Path.GetFullPath(Path.Combine(
            workspace.PhysicalRoot,
            asset.Path.Replace('/', Path.DirectorySeparatorChar)));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(
                automaticId: id,
                canonicalBasePath: asset.Path),
            new SourceLayer(
                canonicalPath: asset.Path,
                physicalPath: physicalPath,
                form: form,
                kind: SourceLayerKind.Base));
    }
}

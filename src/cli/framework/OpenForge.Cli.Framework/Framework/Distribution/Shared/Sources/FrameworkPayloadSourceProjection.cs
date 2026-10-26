using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;

internal static class FrameworkPayloadSourceProjection
{

    internal static SourceLogicalSource Create(
        WorkspaceLayout layout,
        FrameworkPayloadAsset asset)
    {
        var destination = layout.MapPayloadPath(asset.Path);
        if (!SourceFormClassifier.TryClassify(layout, destination, out var form))
        {
            throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' is not a recognized authored source.");
        }

        var id = SourceIdentity.DeriveId(layout, destination)
            ?? throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' has no canonical source identity.");
        var physicalPath = Path.GetFullPath(Path.Combine(
            layout.Workspace.PhysicalRoot,
            destination.Replace('/', Path.DirectorySeparatorChar)));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(
                automaticId: id,
                canonicalBasePath: destination),
            new SourceLayer(
                canonicalPath: destination,
                physicalPath: physicalPath,
                form: form,
                kind: SourceLayerKind.Base));
    }
}

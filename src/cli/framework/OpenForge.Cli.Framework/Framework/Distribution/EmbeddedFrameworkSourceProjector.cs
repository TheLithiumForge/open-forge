using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Distribution;

internal sealed class EmbeddedFrameworkSourceProjector
{

    internal ImmutableArray<SourceLogicalSource> Project(
        WorkspaceLayout layout,
        FrameworkPayload payload)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(payload);

        var assetsByPath = payload.Assets.ToDictionary(
            asset => layout.MapPayloadPath(asset.Path),
            StringComparer.Ordinal);
        var sources = ImmutableArray.CreateBuilder<SourceLogicalSource>();
        foreach (var asset in payload.Assets)
        {
            var destination = layout.MapPayloadPath(asset.Path);
            if (!SourceLogicalPath.IsPrimarySource(layout, destination)
                || !SourceFormClassifier.TryClassify(layout, destination, out var form)
                || form == SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            var automaticId = SourceIdentity.DeriveId(layout, destination)
                ?? throw new InvalidDataException(
                    $"The embedded Framework asset '{asset.Path}' has no canonical source identity.");
            var baseLayer = CreateLayer(
                layout.Workspace,
                destination,
                form,
                SourceLayerKind.Base);
            var overwritePath = SourceOverwritePath.ReadAdjacentPath(destination);
            var overwrite = assetsByPath.TryGetValue(overwritePath, out var overwriteAsset)
                ? CreateOverwrite(layout, overwriteAsset)
                : null;
            sources.Add(new SourceLogicalSource(
                new SourceLogicalIdentity(automaticId, destination),
                baseLayer,
                overwrite));
        }

        foreach (var overwrite in payload.Assets.Where(asset =>
                     SourceLogicalPath.IsPrimarySource(layout, layout.MapPayloadPath(asset.Path))
                     && SourceFormClassifier.TryClassify(layout, layout.MapPayloadPath(asset.Path), out var form)
                     && form == SourceDocumentForm.OverwriteCompanion))
        {
            var basePath = SourceOverwritePath.ReadBasePath(layout.MapPayloadPath(overwrite.Path));
            if (!assetsByPath.TryGetValue(basePath, out var baseAsset)
                || !SourceFormClassifier.TryClassify(layout, layout.MapPayloadPath(baseAsset.Path), out var baseForm)
                || baseForm == SourceDocumentForm.OverwriteCompanion)
            {
                throw new InvalidDataException(
                    $"The embedded Framework overwrite '{overwrite.Path}' has no recognized adjacent base source.");
            }
        }

        return sources
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static SourceLayer CreateOverwrite(
        WorkspaceLayout layout,
        FrameworkPayloadAsset asset)
    {
        var destination = layout.MapPayloadPath(asset.Path);
        if (!SourceFormClassifier.TryClassify(layout, destination, out var form)
            || form != SourceDocumentForm.OverwriteCompanion)
        {
            throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' is not an overwrite companion.");
        }

        return CreateLayer(
            layout.Workspace,
            destination,
            form,
            SourceLayerKind.Overwrite);
    }

    private static SourceLayer CreateLayer(
        CliWorkspace workspace,
        string canonicalPath,
        SourceDocumentForm form,
        SourceLayerKind kind)
    {
        var physicalPath = Path.GetFullPath(SourceLogicalPath.ToLexicalPath(
            workspace.PhysicalRoot,
            canonicalPath));
        if (!PhysicalContainment.Contains(workspace.PhysicalRoot, physicalPath))
        {
            throw new InvalidDataException(
                $"The embedded Framework asset '{canonicalPath}' resolves outside the workspace.");
        }

        return new SourceLayer(canonicalPath, physicalPath, form, kind);
    }
}

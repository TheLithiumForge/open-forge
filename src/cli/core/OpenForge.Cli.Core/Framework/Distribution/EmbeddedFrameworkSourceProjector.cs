using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Distribution;

internal sealed class EmbeddedFrameworkSourceProjector
{
    internal ImmutableArray<SourceLogicalSource> Project(
        CliWorkspace workspace,
        FrameworkPayload payload)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(payload);

        var assetsByPath = payload.Assets.ToDictionary(
            asset => asset.Path,
            StringComparer.Ordinal);
        var sources = ImmutableArray.CreateBuilder<SourceLogicalSource>();
        foreach (var asset in payload.Assets)
        {
            if (!asset.Path.StartsWith(".agents/", StringComparison.Ordinal)
                || !SourceFormClassifier.TryClassify(asset.Path, out var form)
                || form == SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            var automaticId = SourceIdentity.DeriveId(asset.Path)
                ?? throw new InvalidDataException(
                    $"The embedded Framework asset '{asset.Path}' has no canonical source identity.");
            var baseLayer = CreateLayer(
                workspace,
                asset.Path,
                form,
                SourceLayerKind.Base);
            var overwritePath = ReadAdjacentOverwritePath(asset.Path);
            var overwrite = assetsByPath.TryGetValue(overwritePath, out var overwriteAsset)
                ? CreateOverwrite(workspace, overwriteAsset)
                : null;
            sources.Add(new SourceLogicalSource(
                new SourceLogicalIdentity(automaticId, asset.Path),
                baseLayer,
                overwrite));
        }

        foreach (var overwrite in payload.Assets.Where(asset =>
                     asset.Path.StartsWith(".agents/", StringComparison.Ordinal)
                     && SourceFormClassifier.TryClassify(asset.Path, out var form)
                     && form == SourceDocumentForm.OverwriteCompanion))
        {
            var basePath = ReadAdjacentBasePath(overwrite.Path);
            if (!assetsByPath.TryGetValue(basePath, out var baseAsset)
                || !SourceFormClassifier.TryClassify(baseAsset.Path, out var baseForm)
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
        CliWorkspace workspace,
        FrameworkPayloadAsset asset)
    {
        if (!SourceFormClassifier.TryClassify(asset.Path, out var form)
            || form != SourceDocumentForm.OverwriteCompanion)
        {
            throw new InvalidDataException(
                $"The embedded Framework asset '{asset.Path}' is not an overwrite companion.");
        }

        return CreateLayer(
            workspace,
            asset.Path,
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

    private static string ReadAdjacentOverwritePath(string basePath)
        => basePath[..^".md".Length] + ".overwrite.md";

    private static string ReadAdjacentBasePath(string overwritePath)
        => overwritePath[..^".overwrite.md".Length] + ".md";
}

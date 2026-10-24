using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal sealed class InstallConfigurationFrontmatterPlanner(PhysicalPathResolver paths)
{
    private readonly InstallTargetReader _reader = new(paths);
    private readonly InstallContentIdentity _identity = new();
    private readonly ExtensionSourceObservationReader _sources = new(new ExtensionSourceReader(paths));

    internal async ValueTask<InstallConfigurationFrontmatterPlan> PlanAsync(
        InstallConfigurationFrontmatterInput input, CancellationToken cancellationToken)
    {
        var replacements = ImmutableArray.CreateBuilder<InstallConfigurationFrontmatterReplacement>();
        var kept = ImmutableArray.CreateBuilder<InstallFrontmatterKeptFile>();
        var observations = ImmutableArray.CreateBuilder<InstallConfigurationFrontmatterObservation>();
        var ownership = input.Ownership.Document;
        var extensions = await _sources.ReadAsync(input.Workspace, ownership.Extensions,
            includeEmbedded: false, cancellationToken).ConfigureAwait(false);
        var otherForm = input.Selection.Form == FrontmatterForm.Root ? FrontmatterForm.Scoped : FrontmatterForm.Root;
        var otherPayload = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var path in (ownership.Framework?.Paths ?? []).Concat(ownership.Extensions.SelectMany(owner => owner.Paths))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!EligiblePath(input.Layout, path) || !FrameworkPayloadSelection.IncludesPath(input.Layout, path, input.IntendedSettings)
                || ownership.Libraries.Any(library => IsLibraryPath(path, library))) continue;
            var frameworkOwns = ownership.Framework?.Paths.Contains(path, StringComparer.Ordinal) == true;
            var extensionOwners = ownership.Extensions.Where(owner => owner.Paths.Contains(path, StringComparer.Ordinal)).ToArray();
            if (extensionOwners.Any(owner => WorkspaceRemovals.IsExtensionRemoved(owner.Id, input.IntendedSettings))) continue;
            if (frameworkOwns && extensionOwners.Length > 0)
                throw new InvalidDataException($"Multiple managers claim conversion destination '{path}'.");
            ReadOnlyMemory<byte>? canonical;
            byte[]? selected = null;
            byte[]? other = null;
            var sourcePath = path;
            if (frameworkOwns)
            {
                var asset = FrameworkSourceAlignment.ReadAsset(input.Layout, path, input.Payload);
                if (asset is null || !FrameworkPayloadSelection.IncludesPath(input.Layout, asset.Path, input.IntendedSettings)) continue;
                sourcePath = asset.Path;
                canonical = asset.Bytes.AsMemory();
                selected = input.RenderedPayload[asset.Path];
                if (!otherPayload.TryGetValue(asset.Path, out other))
                {
                    other = InstallPayloadRendering.Render(input.Layout, asset.Path, canonical.Value, otherForm);
                    otherPayload.Add(asset.Path, other);
                }
            }
            else
            {
                var file = ReadExtensionFile(input.Layout, path, extensionOwners, extensions);
                canonical = file?.Bytes;
                sourcePath = file?.TargetPath ?? path;
            }
            if (canonical is { } sourceBytes)
            {
                selected ??= InstallPayloadRendering.Render(input.Layout, sourcePath, sourceBytes, input.Selection.Form);
                other ??= InstallPayloadRendering.Render(input.Layout, sourcePath, sourceBytes, otherForm);
                if (selected.AsSpan().SequenceEqual(other)) continue;
            }
            var read = await _reader.ReadAsync(input.Workspace, path, cancellationToken).ConfigureAwait(false);
            if (read.State == InstallTargetReadState.Missing) continue;
            if (read.State != InstallTargetReadState.File || read.Snapshot is not { } snapshot)
                throw new InvalidDataException(read.Cause ?? $"Conversion destination '{path}' is unsafe or unavailable.");
            observations.Add(new(path, snapshot.Expectation, canonical is { } observedSource ? FileExpectation.Hash(observedSource.Span) : null));
            if (selected is null || other is null)
            {
                kept.Add(new(path, InstallFrontmatterKeptReason.SourceUnavailable));
                continue;
            }
            string currentIdentity;
            try { currentIdentity = _identity.ReadSourceFingerprint(snapshot.Bytes.AsSpan()); }
            catch (InvalidDataException)
            {
                kept.Add(new(path, InstallFrontmatterKeptReason.Edited));
                continue;
            }
            if (currentIdentity == _identity.ReadSourceFingerprint(selected)) continue;
            if (currentIdentity != _identity.ReadSourceFingerprint(other))
            {
                kept.Add(new(path, InstallFrontmatterKeptReason.Edited));
                continue;
            }
            replacements.Add(new() { Before = read, IntendedBytes = selected, SourceAssetPath = sourcePath });
        }
        return new() { Replacements = replacements.ToImmutable(), Kept = kept.ToImmutable(), Observations = observations.ToImmutable() };
    }

    private static bool EligiblePath(WorkspaceLayout layout, string path)
        => SourceLogicalPath.IsPrimaryMarkdownSource(new SourceContentRoots(layout), path)
            && !path.EndsWith("/" + SourceFormClassifier.NativeSkillFileName, StringComparison.Ordinal)
            && !SourceLogicalPath.IsPrimaryOverwriteSource(new SourceContentRoots(layout), path);

    private static bool IsLibraryPath(string path, LibraryOwnership library)
    {
        var key = PortableWorkspacePath.CreatePortableKey(path);
        var root = PortableWorkspacePath.CreatePortableKey(library.DestinationRoot);
        return key == root || key.StartsWith(root + "/", StringComparison.Ordinal);
    }

    private static ExtensionPackageFileFact? ReadExtensionFile(WorkspaceLayout layout, string path, IReadOnlyList<ExtensionOwnership> owners,
        IReadOnlyList<ExtensionSourceObservation> sources)
    {
        ExtensionPackageFileFact? canonical = null;
        foreach (var owner in owners)
        {
            var matches = sources.Where(source => source.RecordedSource == owner.Source && source.Read.State == ExtensionSourceReadState.Complete).ToArray();
            if (matches.Length != 1) return null;
            var packages = matches[0].Read.Packages.Where(package => package.Id == owner.Id).ToArray();
            if (packages.Length != 1) return null;
            var files = packages[0].Payload.Where(file => file.TargetPath is { } target && layout.MapPayloadPath(target) == path).ToArray();
            if (files.Length != 1 || files[0].State != ExtensionPackageFileReadState.Available || files[0].Bytes is not { } bytes) return null;
            if (canonical?.Bytes is { } previous && !previous.Span.SequenceEqual(bytes.Span))
                throw new InvalidDataException($"Extension owners disagree about conversion source '{path}'.");
            canonical = files[0];
        }
        return canonical;
    }
}

using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using System.Security.Cryptography;

namespace OpenForge.Cli.Core.Framework.Extensions;

internal sealed class ExtensionSourceReader(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal ValueTask<ExtensionSourceReadResult> ReadAsync(
        CliWorkspace workspace,
        string? explicitSource,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(Cancelled(explicitSource ?? "embedded catalogue"));
        }

        if (explicitSource is null)
        {
            return ValueTask.FromResult(EmbeddedExtensionCatalogueReader.Read());
        }

        return ReadExplicitAsync(workspace, explicitSource, cancellationToken);
    }

    private async ValueTask<ExtensionSourceReadResult> ReadExplicitAsync(
        CliWorkspace workspace,
        string explicitSource,
        CancellationToken cancellationToken)
    {
        string lexicalSource;
        try
        {
            lexicalSource = Path.GetFullPath(explicitSource);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Invalid(explicitSource, $"The explicit Extension source path is invalid: {exception.Message}");
        }

        if (Overlaps(workspace.LexicalRoot, lexicalSource))
        {
            return Blocked(lexicalSource, "The explicit Extension source overlaps the selected workspace.", ExtensionSourceFailureKind.Overlap);
        }

        var sourceResolution = _physicalPathResolver.ResolveRoot(lexicalSource);
        if (sourceResolution.State == PhysicalPathState.Contained
            && Overlaps(workspace.PhysicalRoot, sourceResolution.GetContainedPhysicalPath()))
        {
            return Blocked(lexicalSource, "The explicit Extension source aliases the selected workspace.", ExtensionSourceFailureKind.Overlap);
        }

        if (sourceResolution.State == PhysicalPathState.Missing)
        {
            return Missing(lexicalSource);
        }

        if (sourceResolution.State != PhysicalPathState.Contained)
        {
            return Blocked(lexicalSource, "The explicit Extension source physical identity is unavailable or unsafe.", ExtensionSourceFailureKind.Overlap);
        }

        if (!Directory.Exists(lexicalSource))
        {
            return Invalid(lexicalSource, "The explicit Extension source is not a directory.", ExtensionSourceFailureKind.Invalid);
        }

        var physicalSource = sourceResolution.GetContainedPhysicalPath();
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(lexicalSource);
        }

        var rootManifest = ResolveCandidate(lexicalSource, physicalSource, Path.Combine(lexicalSource, "extension.json"));
        if (rootManifest.State == PhysicalPathState.Contained)
        {
            bool hasCataloguePackage;
            try
            {
                hasCataloguePackage = HasCataloguePackage(lexicalSource, physicalSource, cancellationToken);
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                return Unavailable(lexicalSource, exception.Message);
            }

            if (hasCataloguePackage)
            {
                return Blocked(
                    lexicalSource,
                    "The explicit Extension source is both a package and a catalogue.",
                    ExtensionSourceFailureKind.Ambiguous);
            }

            return await ReadPackageAsync(
                lexicalSource,
                physicalSource,
                rootManifest.GetContainedPhysicalPath(),
                "extension.json",
                cancellationToken).ConfigureAwait(false);
        }

        if (rootManifest.State is not PhysicalPathState.Missing)
        {
            return Blocked(lexicalSource, "The package manifest path is unsafe or unavailable.", ExtensionSourceFailureKind.Invalid);
        }

        return await ReadCatalogueAsync(lexicalSource, physicalSource, cancellationToken).ConfigureAwait(false);
    }

    private bool HasCataloguePackage(
        string lexicalSource,
        string physicalSource,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directories = Directory.GetDirectories(lexicalSource);

        foreach (var directory in directories.Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.Equals(Path.GetFileName(directory), ExtensionPackageLayout.ContentDirectoryName, StringComparison.Ordinal))
            {
                continue;
            }

            var packageResolution = ResolveCandidate(lexicalSource, physicalSource, directory);
            if (packageResolution.State != PhysicalPathState.Contained)
            {
                continue;
            }

            var manifest = ResolveCandidate(
                directory,
                packageResolution.GetContainedPhysicalPath(),
                Path.Combine(directory, "extension.json"));
            if (manifest.State == PhysicalPathState.Contained)
            {
                return true;
            }
        }

        return false;
    }

    private async ValueTask<ExtensionSourceReadResult> ReadCatalogueAsync(
        string lexicalSource,
        string physicalSource,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string[] packageDirectories;
        try
        {
            packageDirectories = [.. Directory.GetDirectories(lexicalSource).Order(StringComparer.Ordinal)];
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unavailable(lexicalSource, exception.Message);
        }
        catch (IOException exception)
        {
            return Unavailable(lexicalSource, exception.Message);
        }

        if (packageDirectories.Length == 0)
        {
            return Invalid(lexicalSource, "The explicit Extension source is neither a package nor a non-empty catalogue.", ExtensionSourceFailureKind.Invalid);
        }

        var packages = new List<ExtensionPackageFact>();
        foreach (var directory in packageDirectories)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Cancelled(lexicalSource);
            }

            var packageResolution = ResolveCandidate(lexicalSource, physicalSource, directory);
            if (packageResolution.State != PhysicalPathState.Contained)
            {
                return Blocked(lexicalSource, $"Catalogue package directory '{Path.GetFileName(directory)}' is unsafe.", ExtensionSourceFailureKind.Overlap);
            }

            var physicalPackage = packageResolution.GetContainedPhysicalPath();
            var manifestResolution = ResolveCandidate(directory, physicalPackage, Path.Combine(directory, "extension.json"));
            if (manifestResolution.State != PhysicalPathState.Contained)
            {
                return manifestResolution.State == PhysicalPathState.Missing
                    ? Invalid(
                        lexicalSource,
                        $"Catalogue package directory '{Path.GetFileName(directory)}' has no manifest.",
                        ExtensionSourceFailureKind.ManifestMissing)
                    : Blocked(
                        lexicalSource,
                        $"Catalogue package directory '{Path.GetFileName(directory)}' has an unsafe manifest boundary.",
                        ExtensionSourceFailureKind.Invalid);
            }

            var manifest = await ReadPackageFactAsync(
                directory,
                physicalPackage,
                manifestResolution.GetContainedPhysicalPath(),
                Path.GetRelativePath(lexicalSource, Path.Combine(directory, "extension.json")).Replace('\\', '/'),
                cancellationToken).ConfigureAwait(false);
            switch (manifest)
            {
                case ExtensionManifestFileReadSuccess packageResult:
                    packages.Add(packageResult.Package);
                    break;
                case ExtensionManifestFileReadFailure packageResult:
                    return new ExtensionSourceReadResult(
                        packageResult.Result.State,
                        packageResult.Result.Kind,
                        lexicalSource,
                        packages.Concat(packageResult.Result.Packages),
                        packageResult.Result.Cause,
                        packageResult.Result.FailureKind);
                default:
                    throw new InvalidOperationException("The Extension package read outcome is not defined.");
            }
        }

        return ValidateSource(ExtensionSourceKind.Catalogue, lexicalSource, packages);
    }

    private async ValueTask<ExtensionSourceReadResult> ReadPackageAsync(
        string lexicalPackage,
        string physicalPackage,
        string physicalManifest,
        string manifestPath,
        CancellationToken cancellationToken)
    {
        var manifest = await ReadPackageFactAsync(
            lexicalPackage,
            physicalPackage,
            physicalManifest,
            manifestPath,
            cancellationToken).ConfigureAwait(false);
        return manifest switch
        {
            ExtensionManifestFileReadSuccess success => ValidateSource(
                ExtensionSourceKind.Package,
                lexicalPackage,
                [success.Package]),
            ExtensionManifestFileReadFailure failure => new ExtensionSourceReadResult(
                failure.Result.State,
                failure.Result.Kind,
                lexicalPackage,
                failure.Result.Packages,
                failure.Result.Cause,
                failure.Result.FailureKind),
            _ => throw new InvalidOperationException("The Extension manifest read outcome is not defined."),
        };
    }

    private async ValueTask<ExtensionManifestFileReadOutcome> ReadPackageFactAsync(
        string lexicalPackage,
        string physicalPackage,
        string physicalManifest,
        string manifestPath,
        CancellationToken cancellationToken)
    {
        var payload = await ReadPayloadAsync(lexicalPackage, physicalPackage, cancellationToken).ConfigureAwait(false);
        return await ExtensionManifestFileReader
            .ReadAsync(
                physicalManifest,
                manifestPath,
                payload,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<IReadOnlyList<ExtensionPackageFileFact>> ReadPayloadAsync(
        string lexicalPackage,
        string physicalPackage,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var payloadRoot = Path.Combine(lexicalPackage, ExtensionPackageLayout.ContentDirectoryName);
        var payloadResolution = ResolveCandidate(lexicalPackage, physicalPackage, payloadRoot);
        if (payloadResolution.State == PhysicalPathState.Missing)
        {
            return [];
        }

        if (payloadResolution.State != PhysicalPathState.Contained)
        {
            return
            [
                UnreadFile(ExtensionPackageLayout.ContentDirectoryName, null, ExtensionPackageFileReadState.Blocked),
            ];
        }

        var facts = new List<ExtensionPackageFileFact>();
        var pending = new Stack<string>();
        pending.Push(payloadRoot);
        while (pending.TryPop(out var directory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string[] files;
            string[] directories;
            try
            {
                files = [.. Directory.GetFiles(directory).Order(StringComparer.Ordinal)];
                directories = [.. Directory.GetDirectories(directory).Order(StringComparer.Ordinal)];
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                facts.Add(UnreadPayloadEntry(
                    lexicalPackage,
                    directory,
                    ExtensionPackageFileReadState.Unavailable));
                continue;
            }

            foreach (var file in files)
            {
                facts.Add(await ReadPayloadFileAsync(
                    lexicalPackage,
                    physicalPackage,
                    file,
                    cancellationToken).ConfigureAwait(false));
            }

            for (var index = directories.Length - 1; index >= 0; index--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var child = directories[index];
                var childResolution = ResolveCandidate(lexicalPackage, physicalPackage, child);
                if (childResolution.State == PhysicalPathState.Contained)
                {
                    pending.Push(child);
                }
                else
                {
                    facts.Add(UnreadPayloadEntry(
                        lexicalPackage,
                        child,
                        childResolution.State is PhysicalPathState.External or PhysicalPathState.Cycle
                            ? ExtensionPackageFileReadState.Blocked
                            : ExtensionPackageFileReadState.Unavailable));
                }
            }
        }

        return facts;
    }

    private async ValueTask<ExtensionPackageFileFact> ReadPayloadFileAsync(
        string lexicalPackage,
        string physicalPackage,
        string file,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var relative = Path.GetRelativePath(lexicalPackage, file).Replace('\\', '/');
        var target = relative.StartsWith(ExtensionPackageLayout.ContentPathPrefix, StringComparison.Ordinal)
            ? relative[ExtensionPackageLayout.ContentPathPrefix.Length..]
            : string.Empty;
        if (!PortableWorkspacePath.TryNormalize(target, out var normalizedTarget))
        {
            return UnreadFile(relative, null, ExtensionPackageFileReadState.Invalid);
        }

        var resolution = ResolveCandidate(lexicalPackage, physicalPackage, file);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return UnreadFile(
                relative,
                normalizedTarget,
                resolution.State is PhysicalPathState.External or PhysicalPathState.Cycle
                    ? ExtensionPackageFileReadState.Blocked
                    : ExtensionPackageFileReadState.Unavailable);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = await File.ReadAllBytesAsync(
                resolution.GetContainedPhysicalPath(),
                cancellationToken).ConfigureAwait(false);
            return AvailableFile(relative, normalizedTarget, bytes);
        }
        catch (FileNotFoundException)
        {
            return UnreadFile(relative, normalizedTarget, ExtensionPackageFileReadState.Missing);
        }
        catch (DirectoryNotFoundException)
        {
            return UnreadFile(relative, normalizedTarget, ExtensionPackageFileReadState.Missing);
        }
        catch (UnauthorizedAccessException)
        {
            return UnreadFile(relative, normalizedTarget, ExtensionPackageFileReadState.Unavailable);
        }
        catch (IOException)
        {
            return UnreadFile(relative, normalizedTarget, ExtensionPackageFileReadState.Unavailable);
        }
    }

    private static ExtensionPackageFileFact UnreadPayloadEntry(
        string lexicalPackage,
        string entry,
        ExtensionPackageFileReadState state)
    {
        var relative = Path.GetRelativePath(lexicalPackage, entry).Replace('\\', '/');
        var target = relative.StartsWith(ExtensionPackageLayout.ContentPathPrefix, StringComparison.Ordinal)
            ? relative[ExtensionPackageLayout.ContentPathPrefix.Length..]
            : string.Empty;
        return PortableWorkspacePath.TryNormalize(target, out var normalizedTarget)
            ? UnreadFile(relative, normalizedTarget, state)
            : UnreadFile(relative, null, ExtensionPackageFileReadState.Invalid);
    }

    private static ExtensionPackageFileFact UnreadFile(
        string path,
        string? targetPath,
        ExtensionPackageFileReadState state)
        => ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
        {
            Path = path,
            TargetPath = targetPath,
            State = state,
        });

    private static ExtensionPackageFileFact AvailableFile(
        string path,
        string targetPath,
        byte[] bytes)
        => ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
        {
            Path = path,
            TargetPath = targetPath,
            State = ExtensionPackageFileReadState.Available,
            ByteLength = bytes.LongLength,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            Bytes = bytes,
        });

    private static ExtensionSourceReadResult ValidateSource(
        ExtensionSourceKind kind,
        string source,
        IReadOnlyList<ExtensionPackageFact> packages)
    {
        var ordered = packages.OrderBy(package => package.Id, StringComparer.Ordinal).ToArray();
        if (ordered.Select(package => package.Id).Distinct(StringComparer.Ordinal).Count() != ordered.Length)
        {
            return BlockedWithPackages(
                source,
                "The Extension source contains duplicate stable IDs.",
                ordered,
                ExtensionSourceFailureKind.IdentityAmbiguous);
        }

        var ids = ordered.Select(package => package.Id).ToHashSet(StringComparer.Ordinal);
        if (ordered.Any(package => package.Dependencies.Any(dependency => !ids.Contains(dependency))))
        {
            return InvalidWithPackages(
                source,
                "The Extension source dependency closure is incomplete.",
                ordered,
                ExtensionSourceFailureKind.DependencyIncomplete);
        }

        try
        {
            ExtensionDependencyValidator.ValidateAcyclic(ordered);
        }
        catch (InvalidDataException exception)
        {
            return InvalidWithPackages(
                source,
                exception.Message,
                ordered,
                ExtensionSourceFailureKind.DependencyCycle);
        }

        return new(
            state: ExtensionSourceReadState.Complete,
            kind: kind,
            identity: source,
            packages: ordered,
            cause: null);
    }

    private PhysicalPathResolution ResolveCandidate(string lexicalRoot, string physicalRoot, string candidate)
        => _physicalPathResolver.ResolveCandidate(lexicalRoot, physicalRoot, candidate);

    private static bool Overlaps(string first, string second)
        => PhysicalContainment.Contains(first, second) || PhysicalContainment.Contains(second, first);

    private static ExtensionSourceReadResult Missing(string source)
        => new(
            state: ExtensionSourceReadState.Missing,
            kind: null,
            identity: source,
            packages: [],
            cause: "The explicit Extension source does not exist.",
            failureKind: ExtensionSourceFailureKind.Unavailable);

    private static ExtensionSourceReadResult Invalid(
        string source,
        string cause,
        ExtensionSourceFailureKind failureKind = ExtensionSourceFailureKind.Invalid)
        => new(
            state: ExtensionSourceReadState.Invalid,
            kind: null,
            identity: source,
            packages: [],
            cause: cause,
            failureKind: failureKind);

    private static ExtensionSourceReadResult InvalidWithPackages(
        string source,
        string cause,
        IReadOnlyList<ExtensionPackageFact> packages,
        ExtensionSourceFailureKind failureKind)
        => new(
            state: ExtensionSourceReadState.Invalid,
            kind: null,
            identity: source,
            packages: packages,
            cause: cause,
            failureKind: failureKind);

    private static ExtensionSourceReadResult Blocked(
        string source,
        string cause,
        ExtensionSourceFailureKind failureKind = ExtensionSourceFailureKind.Ambiguous)
        => new(
            state: ExtensionSourceReadState.Blocked,
            kind: null,
            identity: source,
            packages: [],
            cause: cause,
            failureKind: failureKind);

    private static ExtensionSourceReadResult BlockedWithPackages(
        string source,
        string cause,
        IReadOnlyList<ExtensionPackageFact> packages,
        ExtensionSourceFailureKind failureKind)
        => new(
            state: ExtensionSourceReadState.Blocked,
            kind: null,
            identity: source,
            packages: packages,
            cause: cause,
            failureKind: failureKind);

    private static ExtensionSourceReadResult Unavailable(string source, string cause)
        => new(
            state: ExtensionSourceReadState.Unavailable,
            kind: null,
            identity: source,
            packages: [],
            cause: cause,
            failureKind: ExtensionSourceFailureKind.Unavailable);

    private static ExtensionSourceReadResult Cancelled(string source)
        => new(
            state: ExtensionSourceReadState.Cancelled,
            kind: null,
            identity: source,
            packages: [],
            cause: "Extension source inspection was interrupted.",
            failureKind: ExtensionSourceFailureKind.Unavailable);

}

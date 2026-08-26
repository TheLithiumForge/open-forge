using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

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
            return Blocked(lexicalSource, "The explicit Extension source overlaps the selected workspace.");
        }

        var sourceResolution = _physicalPathResolver.ResolveRoot(lexicalSource);
        if (sourceResolution.State == PhysicalPathState.Contained
            && Overlaps(workspace.PhysicalRoot, sourceResolution.GetContainedPhysicalPath()))
        {
            return Blocked(lexicalSource, "The explicit Extension source aliases the selected workspace.");
        }

        if (sourceResolution.State == PhysicalPathState.Missing)
        {
            return Missing(lexicalSource);
        }

        if (sourceResolution.State != PhysicalPathState.Contained)
        {
            return Blocked(lexicalSource, "The explicit Extension source physical identity is unavailable or unsafe.");
        }

        if (!Directory.Exists(lexicalSource))
        {
            return Invalid(lexicalSource, "The explicit Extension source is not a directory.");
        }

        var physicalSource = sourceResolution.GetContainedPhysicalPath();
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(lexicalSource);
        }

        var rootManifest = ResolveCandidate(lexicalSource, physicalSource, Path.Combine(lexicalSource, "extension.json"));
        if (rootManifest.State == PhysicalPathState.Contained)
        {
            var manifest = await ExtensionManifestFileReader
                .ReadAsync(rootManifest.GetContainedPhysicalPath(), cancellationToken)
                .ConfigureAwait(false);
            return manifest switch
            {
                ExtensionManifestFileReadSuccess success => ValidateSource(
                    ExtensionSourceKind.Package,
                    lexicalSource,
                    [success.Package]),
                ExtensionManifestFileReadFailure failure => failure.Result,
                _ => throw new InvalidOperationException("The Extension manifest read outcome is not defined."),
            };
        }

        if (rootManifest.State is not PhysicalPathState.Missing)
        {
            return Blocked(lexicalSource, "The package manifest path is unsafe or unavailable.");
        }

        return await ReadCatalogueAsync(lexicalSource, physicalSource, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<ExtensionSourceReadResult> ReadCatalogueAsync(
        string lexicalSource,
        string physicalSource,
        CancellationToken cancellationToken)
    {
        string[] packageDirectories;
        try
        {
            packageDirectories = Directory.GetDirectories(lexicalSource)
                .Order(StringComparer.Ordinal)
                .ToArray();
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
            return Invalid(lexicalSource, "The explicit Extension source is neither a package nor a non-empty catalogue.");
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
                return Blocked(lexicalSource, $"Catalogue package directory '{Path.GetFileName(directory)}' is unsafe.");
            }

            var physicalPackage = packageResolution.GetContainedPhysicalPath();
            var manifestResolution = ResolveCandidate(directory, physicalPackage, Path.Combine(directory, "extension.json"));
            if (manifestResolution.State != PhysicalPathState.Contained)
            {
                return Invalid(lexicalSource, $"Catalogue package directory '{Path.GetFileName(directory)}' has no safe manifest.");
            }

            var manifest = await ExtensionManifestFileReader
                .ReadAsync(manifestResolution.GetContainedPhysicalPath(), cancellationToken)
                .ConfigureAwait(false);
            switch (manifest)
            {
                case ExtensionManifestFileReadSuccess success:
                    packages.Add(success.Package);
                    break;
                case ExtensionManifestFileReadFailure failure:
                    return failure.Result;
                default:
                    throw new InvalidOperationException("The Extension manifest read outcome is not defined.");
            }
        }

        return ValidateSource(ExtensionSourceKind.Catalogue, lexicalSource, packages);
    }

    private static ExtensionSourceReadResult ValidateSource(
        ExtensionSourceKind kind,
        string source,
        IReadOnlyList<ExtensionPackageFact> packages)
    {
        var ordered = packages.OrderBy(package => package.Id, StringComparer.Ordinal).ToArray();
        if (ordered.Select(package => package.Id).Distinct(StringComparer.Ordinal).Count() != ordered.Length)
        {
            return Blocked(source, "The Extension source contains duplicate stable IDs.");
        }

        var ids = ordered.Select(package => package.Id).ToHashSet(StringComparer.Ordinal);
        if (ordered.Any(package => package.Dependencies.Any(dependency => !ids.Contains(dependency))))
        {
            return Invalid(source, "The Extension source dependency closure is incomplete.");
        }

        try
        {
            ExtensionDependencyValidator.ValidateAcyclic(ordered);
        }
        catch (InvalidDataException exception)
        {
            return Invalid(source, exception.Message);
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
            cause: "The explicit Extension source does not exist.");

    private static ExtensionSourceReadResult Invalid(string source, string cause)
        => new(
            state: ExtensionSourceReadState.Invalid,
            kind: null,
            identity: source,
            packages: [],
            cause: cause);

    private static ExtensionSourceReadResult Blocked(string source, string cause)
        => new(
            state: ExtensionSourceReadState.Blocked,
            kind: null,
            identity: source,
            packages: [],
            cause: cause);

    private static ExtensionSourceReadResult Unavailable(string source, string cause)
        => new(
            state: ExtensionSourceReadState.Unavailable,
            kind: null,
            identity: source,
            packages: [],
            cause: cause);

    private static ExtensionSourceReadResult Cancelled(string source)
        => new(
            state: ExtensionSourceReadState.Cancelled,
            kind: null,
            identity: source,
            packages: [],
            cause: "Extension source inspection was interrupted.");

}

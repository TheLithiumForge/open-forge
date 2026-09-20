using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallSourceResolver(PhysicalPathResolver physicalPathResolver)
{
    private readonly ExtensionSourceReader _reader = new(physicalPathResolver);

    internal async ValueTask<ExtensionInstallSourceResolution> ReadAsync(
        ExtensionInstallRequest request,
        CancellationToken cancellationToken)
    {
        var source = await _reader.ReadAsync(
            request.Workspace,
            request.SourcePath,
            cancellationToken).ConfigureAwait(false);
        if (source.State != ExtensionSourceReadState.Invalid
            || source.FailureKind != ExtensionSourceFailureKind.DependencyIncomplete
            || source.Packages.Count != 1
            || request.SourcePath is null)
        {
            return Complete(source);
        }

        var packagePath = source.Identity;
        var parent = Path.GetDirectoryName(packagePath);
        if (parent is null)
        {
            return Complete(source);
        }

        var catalogue = await _reader.ReadAsync(
            request.Workspace,
            parent,
            cancellationToken).ConfigureAwait(false);
        if (catalogue.State != ExtensionSourceReadState.Complete
            || catalogue.Kind != ExtensionSourceKind.Catalogue)
        {
            return Complete(source);
        }

        var rootId = source.Packages[0].Id;
        var root = catalogue.Packages.SingleOrDefault(package =>
            string.Equals(package.Id, rootId, StringComparison.Ordinal)
            && IsExactPackagePath(parent, packagePath, package.ManifestPath));
        if (root is null)
        {
            return Complete(source);
        }

        return new ExtensionInstallSourceResolution(
            new ExtensionSourceReadResult(
                ExtensionSourceReadState.Complete,
                ExtensionSourceKind.Package,
                packagePath,
                catalogue.Packages,
                cause: null),
            root.Id);
    }

    private static bool IsExactPackagePath(
        string cataloguePath,
        string packagePath,
        string manifestPath)
    {
        var relativePackage = Path.GetDirectoryName(
            manifestPath.Replace('/', Path.DirectorySeparatorChar));
        return relativePackage is not null
            && string.Equals(
                Path.GetFullPath(Path.Combine(cataloguePath, relativePackage)),
                packagePath,
                PathComparison());
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static ExtensionInstallSourceResolution Complete(
        ExtensionSourceReadResult source)
        => new(
            source,
            source.State == ExtensionSourceReadState.Complete
                && source.Kind == ExtensionSourceKind.Package
                && source.Packages.Count == 1
                    ? source.Packages[0].Id
                    : null);
}

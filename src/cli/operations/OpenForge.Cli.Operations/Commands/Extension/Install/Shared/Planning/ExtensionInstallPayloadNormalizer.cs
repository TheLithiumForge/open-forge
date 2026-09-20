using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal static class ExtensionInstallPayloadNormalizer
{
    internal static ExtensionInstallPayloadNormalization Normalize(
        IReadOnlyList<ExtensionPackageFact> packages)
    {
        var entries = new List<ExtensionInstallNormalizedPayload>();
        foreach (var package in packages)
        {
            foreach (var file in package.Payload)
            {
                var target = file.TargetPath ?? file.Path;
                var availabilityFinding = ReadAvailabilityFinding(file, target);
                if (availabilityFinding is not null)
                {
                    return Stop(availabilityFinding);
                }

                if (!PortableWorkspacePath.TryNormalize(file.TargetPath, out var normalized)
                    || !ExtensionDestinationPolicy.IsAllowed(normalized))
                {
                    return Stop(ExtensionInstallFindingCode.TargetUnsafe,
                        "The Extension destination is not an eligible workspace file.", target);
                }
                var portableKey = PortableWorkspacePath.CreatePortableKey(normalized);

                entries.Add(new ExtensionInstallNormalizedPayload(package.Id, file, normalized, portableKey));
            }
        }

        var canonicalByPortableKey = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in entries.GroupBy(entry => entry.PortableKey, StringComparer.Ordinal))
        {
            var values = group.OrderBy(entry => entry.NormalizedPath, StringComparer.Ordinal)
                .ThenBy(entry => entry.PackageId, StringComparer.Ordinal)
                .ThenBy(entry => entry.File.Path, StringComparer.Ordinal)
                .ToArray();
            var canonical = values[0].NormalizedPath;
            if (!HaveEqualContent(values))
            {
                return Stop(
                    ExtensionInstallFindingCode.OwnershipConflict,
                    "Selected packages intend different content for one portable target.",
                    canonical);
            }

            canonicalByPortableKey.Add(group.Key, canonical);
        }

        var normalizedPackages = packages.Select(package => ExtensionPackageFact.Create(
            new ExtensionPackageManifestFact
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                Version = package.Version,
                Dependencies = [.. package.Dependencies],
            },
            new ExtensionPackageContentsFact
            {
                ManifestPath = package.ManifestPath,
                Payload = [.. entries.Where(entry => entry.PackageId == package.Id)
                    .GroupBy(entry => entry.PortableKey, StringComparer.Ordinal)
                    .Select(group => group.OrderBy(entry => entry.File.Path, StringComparer.Ordinal).First())
                    .Select(entry => ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
                    {
                        Path = entry.File.Path,
                        TargetPath = canonicalByPortableKey[entry.PortableKey],
                        State = entry.File.State,
                        ByteLength = entry.File.ByteLength,
                        Sha256 = entry.File.Sha256,
                        Bytes = entry.File.Bytes,
                    }))],
            })).ToArray();
        return new ExtensionInstallPayloadNormalization(normalizedPackages, finding: null);
    }

    private static ExtensionInstallFinding? ReadAvailabilityFinding(
        ExtensionPackageFileFact file,
        string target)
    {
        return file.State switch
        {
            ExtensionPackageFileReadState.Available
                when file.Bytes is not null && file.Sha256 is not null => null,
            ExtensionPackageFileReadState.Available or ExtensionPackageFileReadState.Missing or ExtensionPackageFileReadState.Unavailable => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.SourceUnavailable,
                "A selected Extension payload is unavailable or incomplete.",
                target),
            ExtensionPackageFileReadState.Invalid or ExtensionPackageFileReadState.Blocked => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.SourceInvalid,
                "A selected Extension payload is invalid or blocked.",
                target),
            ExtensionPackageFileReadState.Cancelled => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension payload reading was interrupted.",
                target),
            _ => throw new ArgumentOutOfRangeException(
                nameof(file),
                file.State,
                "The Extension package-file state is not defined."),
        };
    }

    private static bool HaveEqualContent(ExtensionInstallNormalizedPayload[] values)
    {
        if (values[0].File is not { Sha256: { } firstHash, Bytes: { } firstBytes })
        {
            throw new InvalidOperationException(
                "Available normalized Extension payloads require reviewed bytes and hashes.");
        }

        return values.Skip(1).All(value => value.File is { Sha256: { } hash, Bytes: { } bytes }
            && string.Equals(firstHash, hash, StringComparison.Ordinal)
            && firstBytes.Span.SequenceEqual(bytes.Span));
    }

    private static ExtensionInstallPayloadNormalization Stop(
        ExtensionInstallFindingCode code,
        string cause,
        string target)
        => Stop(new ExtensionInstallFinding(code, cause, target));

    private static ExtensionInstallPayloadNormalization Stop(ExtensionInstallFinding finding)
        => new(packages: [], finding);

}

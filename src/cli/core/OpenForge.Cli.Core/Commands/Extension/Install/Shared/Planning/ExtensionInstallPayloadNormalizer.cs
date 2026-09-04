using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallPayloadNormalizer
{
    internal ExtensionInstallPayloadNormalization Normalize(
        IReadOnlyList<ExtensionPackageFact> packages)
    {
        var entries = new List<NormalizedPayload>();
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

                if (!ExtensionTargetPath.TryNormalize(file.TargetPath, out var normalized)
                    || !normalized.StartsWith(".agents/", StringComparison.Ordinal)
                    || normalized.Length <= ".agents/".Length)
                {
                    return Stop(
                        ExtensionInstallFindingCode.TargetOutsideAgents,
                        "Every Extension payload target must be a strict descendant of .agents/.",
                        target);
                }

                var portableKey = ExtensionTargetPath.CreatePortableKey(normalized);
                if (portableKey == ExtensionTargetPath.CreatePortableKey(LifecycleSchema.RelativePath))
                {
                    return Stop(
                        ExtensionInstallFindingCode.TargetUnsafe,
                        "Extension packages cannot target the lifecycle document.",
                        normalized);
                }

                if (Framework.Sources.Identity.SourceFormClassifier.TryClassify(
                        portableKey,
                        out var form)
                    && form == Framework.Sources.Models.Identity.SourceDocumentForm.OverwriteCompanion)
                {
                    return Stop(
                        ExtensionInstallFindingCode.TargetUnsafe,
                        "Extension packages cannot target workspace-owned overwrite companions.",
                        normalized);
                }

                entries.Add(new NormalizedPayload(package.Id, file, normalized, portableKey));
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
                Dependencies = package.Dependencies.ToArray(),
            },
            new ExtensionPackageContentsFact
            {
                ManifestPath = package.ManifestPath,
                Payload = entries.Where(entry => entry.PackageId == package.Id)
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
                    }))
                    .ToArray(),
            })).ToArray();
        return new ExtensionInstallPayloadNormalization(normalizedPackages, finding: null);
    }

    private static ExtensionInstallFinding? ReadAvailabilityFinding(
        ExtensionPackageFileFact file,
        string target)
    {
        switch (file.State)
        {
            case ExtensionPackageFileReadState.Available
                when file.Bytes is not null && file.Sha256 is not null:
                return null;
            case ExtensionPackageFileReadState.Available:
            case ExtensionPackageFileReadState.Missing:
            case ExtensionPackageFileReadState.Unavailable:
                return new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.SourceUnavailable,
                    "A selected Extension payload is unavailable or incomplete.",
                    target);
            case ExtensionPackageFileReadState.Invalid:
            case ExtensionPackageFileReadState.Blocked:
                return new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.SourceInvalid,
                    "A selected Extension payload is invalid or blocked.",
                    target);
            case ExtensionPackageFileReadState.Cancelled:
                return new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.Interrupted,
                    "Extension payload reading was interrupted.",
                    target);
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(file),
                    file.State,
                    "The Extension package-file state is not defined.");
        }
    }

    private static bool HaveEqualContent(IReadOnlyList<NormalizedPayload> values)
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

    private sealed record NormalizedPayload(
        string PackageId,
        ExtensionPackageFileFact File,
        string NormalizedPath,
        string PortableKey);
}

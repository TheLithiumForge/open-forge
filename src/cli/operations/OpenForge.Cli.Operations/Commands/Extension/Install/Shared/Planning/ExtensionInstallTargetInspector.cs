using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using System.Collections.Immutable;

using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallTargetInspector
{
    private readonly CliPlanConfirmation<ExtensionInstallResult, CliConfirmQuestion> _forceConfirmation;
    private readonly Func<IReadOnlyList<string>, CliConfirmQuestion> _forceQuestion;
    private readonly FileExpectationValidator _validator;
    private readonly FrameworkContentIdentity _contentIdentity = new();

    internal ExtensionInstallTargetInspector(
        CliPlanConfirmation<ExtensionInstallResult, CliConfirmQuestion> forceConfirmation,
        Func<IReadOnlyList<string>, CliConfirmQuestion> forceQuestion,
        FileExpectationValidator validator)
    {
        ArgumentNullException.ThrowIfNull(forceConfirmation);
        ArgumentNullException.ThrowIfNull(forceQuestion);
        _forceConfirmation = forceConfirmation;
        _forceQuestion = forceQuestion;
        _validator = validator;
    }

    internal async ValueTask<ExtensionInstallTargetInspection> InspectAsync(
        ExtensionInstallTargetInspectionInput input,
        CancellationToken cancellationToken)
    {
        var request = input.Request;
        var packages = input.Packages;
        var sourceIdentity = input.SourceIdentity;
        var mutationPackageIds = input.MutationPackageIds.Count == 0
            ? packages.Select(package => package.Id).ToHashSet(StringComparer.Ordinal)
            : input.MutationPackageIds;
        var currentPackages = input.Ownership.Document.Extensions.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var currentPaths = input.Ownership.Document.Extensions.SelectMany(extension => extension.Paths)
            .Distinct(StringComparer.Ordinal).ToDictionary(path => path, input.Ownership.Document.OwnersOf, StringComparer.Ordinal);
        var frameworkPaths = (input.Ownership.Document.Framework?.Paths ?? [])
            .Concat(input.Ownership.Document.Framework?.Regions.Select(region => region.Path) ?? [])
            .Select(PortableWorkspacePath.CreatePortableKey)
            .ToHashSet(StringComparer.Ordinal);
        var intendedByPath = new Dictionary<string, ExtensionInstallIntendedPath>(StringComparer.Ordinal);
        foreach (var package in packages)
        {
            foreach (var file in package.Payload)
            {
                var path = RequiredTargetPath(file);
                var bytes = input.Topology.IntendedTargetBytes.TryGetValue(path, out var intendedBytes)
                    ? intendedBytes.ToArray()
                    : throw new InvalidDataException(
                        "A validated package file requires one final topology target.");
                var identity = ExtensionDestinationPolicy.IsImplicit(path)
                    ? Fingerprint(bytes)
                    : (Fingerprint: FileExpectation.Hash(bytes), Kind: MarkdownFingerprintState.ExactBytes);
                if (intendedByPath.TryGetValue(path, out var existing))
                {
                    if (!string.Equals(existing.Fingerprint, identity.Fingerprint, StringComparison.Ordinal)
                        || existing.FingerprintKind != identity.Kind)
                    {
                        return Stop(
                            ExtensionInstallFindingCode.OwnershipConflict,
                            "Selected packages intend different content for one target.",
                            path);
                    }

                    intendedByPath[path] = new ExtensionInstallIntendedPath(
                        existing.Path,
                        existing.Bytes,
                        existing.Fingerprint,
                        existing.FingerprintKind,
                        existing.Owners.Append(package.Id));
                    continue;
                }

                intendedByPath.Add(path, new ExtensionInstallIntendedPath(
                    path,
                    bytes,
                    identity.Fingerprint,
                    identity.Kind,
                    [package.Id]));
            }
        }

        var observations = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
        var eligible = new List<string>();
        foreach (var intended in intendedByPath.Values.OrderBy(value => value.Path, StringComparer.Ordinal))
        {
            if (frameworkPaths.Contains(PortableWorkspacePath.CreatePortableKey(intended.Path)))
            {
                return Stop(
                    ExtensionInstallFindingCode.OwnershipConflict,
                    "The Extension target is owned by the installed Framework.",
                    intended.Path);
            }

            var alias = currentPaths.Keys.FirstOrDefault(path => path != intended.Path
                && PortableWorkspacePath.CreatePortableKey(path) == PortableWorkspacePath.CreatePortableKey(intended.Path));
            if (alias is not null)
            {
                return Stop(ExtensionInstallFindingCode.OwnershipConflict, "The Extension destination aliases a managed path.", intended.Path);
            }
            var observation = await ObserveAsync(request, intended.Path, cancellationToken)
                .ConfigureAwait(false);
            if (observation.Finding is not null)
            {
                return new ExtensionInstallTargetInspection(null, observation.Finding);
            }

            var snapshot = observation.Snapshot
                ?? throw new InvalidOperationException(
                    "A complete Extension target observation requires its snapshot.");
            if (snapshot.Kind is not (FileExpectationKind.File or FileExpectationKind.Missing))
            {
                return Stop(
                    ExtensionInstallFindingCode.TargetUnsafe,
                    "The Extension target is not safely absent or an ordinary file.",
                    intended.Path);
            }
            observations.Add(intended.Path, snapshot);
            if (currentPaths.TryGetValue(intended.Path, out var managed))
            {
                if (managed.Any(owner => !intended.Owners.Contains(owner, StringComparer.Ordinal))
                    && (snapshot.Kind != FileExpectationKind.File
                        || !snapshot.Bytes.AsSpan().SequenceEqual(intended.Bytes.AsSpan())))
                {
                    return Stop(
                        ExtensionInstallFindingCode.OwnershipConflict,
                        "An existing Extension owner conflicts with the selected package target.",
                        intended.Path);
                }

                if (snapshot.Kind != FileExpectationKind.File
                    || !string.Equals(
                        _contentIdentity.ReadSourceFingerprint(snapshot.Bytes.AsSpan(), intended.FingerprintKind),
                        intended.Fingerprint,
                        StringComparison.Ordinal))
                {
                    return Stop(
                        ExtensionInstallFindingCode.ManagedDivergence,
                        "The installed Extension target differs from the selected source; use extension update.",
                        intended.Owners[0]);
                }

            }
            else if (snapshot.Kind == FileExpectationKind.File)
            {
                if (input.ProtectedAuthoredPaths.Contains(intended.Path)
                    && !input.InitialForceEligiblePaths.Contains(intended.Path))
                {
                    return Stop(
                        ExtensionInstallFindingCode.OwnershipConflict,
                        "The Extension target is a current authored source or companion outside Extension ownership.",
                        intended.Path);
                }

                if (ExtensionDestinationPolicy.IsImplicit(intended.Path) && !input.InitialForceEligiblePaths.Contains(intended.Path))
                {
                    return Stop(
                        ExtensionInstallFindingCode.OwnershipConflict,
                        "The Extension target is not a positively eligible initial-force occupant.",
                        intended.Path);
                }

                eligible.Add(intended.Path);
            }
            else if (snapshot.Kind != FileExpectationKind.Missing)
            {
                return Stop(
                    ExtensionInstallFindingCode.TargetUnsafe,
                    "The Extension target is not safely absent or an ordinary file.",
                    intended.Path);
            }
        }

        foreach (var package in packages)
        {
            if (!currentPackages.TryGetValue(package.Id, out var installed))
            {
                continue;
            }

            var intendedPaths = package.Payload
                .Select(RequiredTargetPath)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (!installed.Dependencies.SequenceEqual(package.Dependencies.Order(StringComparer.Ordinal))
                || !installed.Paths.SequenceEqual(intendedPaths))
            {
                return Stop(
                    ExtensionInstallFindingCode.PackageContentsChanged,
                    "The files or dependencies recorded for the package do not match the selected source.",
                    package.Id);
            }
        }

        var intendedPackages = input.Ownership.Document.Extensions
            .Where(package => !mutationPackageIds.Contains(package.Id))
            .Concat(packages
                .Where(package => mutationPackageIds.Contains(package.Id))
                .Where(package => currentPackages.ContainsKey(package.Id)
                    || package.Payload.Count != 0
                    || package.Dependencies.Count != 0)
                .Select(package => new ExtensionOwnership(
                package.Id, package.Version, sourceIdentity,
                [.. package.Dependencies.Order(StringComparer.Ordinal)],
                [.. package.Payload.Select(RequiredTargetPath).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
                input.Ownership.Document.Extensions.FirstOrDefault(current => current.Id == package.Id)?.Regions ?? [])))
            .OrderBy(package => package.Id, StringComparer.Ordinal)
            .ToImmutableArray();
        var state = new ExtensionInstallTargetState(observations, intendedByPath, eligible, intendedPackages);
        return new ExtensionInstallTargetInspection(state, Finding: null);
    }

    internal async ValueTask<ExtensionInstallFinding?> AuthorizeForceAsync(
        ExtensionInstallRequest request,
        ExtensionInstallTargetState state,
        ExtensionInstallResult preview,
        CancellationToken cancellationToken)
        => await AuthorizeForceAsync(
            request,
            state.EligibleOccupants,
            preview,
            cancellationToken).ConfigureAwait(false);

    internal async ValueTask<ExtensionInstallFinding?> AuthorizeForceAsync(
        ExtensionInstallRequest request,
        IReadOnlyList<string> eligibleOccupants,
        ExtensionInstallResult preview,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(eligibleOccupants);
        if (eligibleOccupants.Count == 0 || request.Force)
        {
            return null;
        }

        // A dry run still reports the existing --force prerequisite, but it
        // never acquires mutation authority from a prompt.
        if (request.Mode != ExtensionInstallMode.Apply)
        {
            return new ExtensionInstallFinding(
                ExtensionInstallFindingCode.InitialForceRequired,
                "Eligible initial occupants require explicit --force authority.",
                eligibleOccupants[0]);
        }

        if (!request.AllowInteraction || request.Automatic)
        {
            return new ExtensionInstallFinding(
                ExtensionInstallFindingCode.InitialForceRequired,
                "Eligible initial occupants require explicit --force authority.",
                eligibleOccupants[0]);
        }

        var reply = await _forceConfirmation(
            preview,
            _forceQuestion(eligibleOccupants),
            new CliPromptPolicy(!request.Automatic && request.AllowInteraction),
            cancellationToken).ConfigureAwait(false);
        return reply.State switch
        {
            CliPromptState.Answered when reply.Value => null,
            CliPromptState.Answered => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension install was cancelled. Nothing was changed.",
                eligibleOccupants[0]),
            CliPromptState.Unavailable => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.ConfirmationRequired,
                "Extension install needs confirmation, and this session cannot ask.",
                eligibleOccupants[0]),
            CliPromptState.Cancelled => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension install was cancelled. Nothing was changed.",
                eligibleOccupants[0]),
            _ => throw new ArgumentOutOfRangeException(nameof(reply), reply.State,
                "The Extension Install force confirmation state is not defined."),
        };
    }

    private async ValueTask<ExtensionInstallTargetObservation> ObserveAsync(
        ExtensionInstallRequest request,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var logical = Path.GetFullPath(Path.Combine(
            request.Workspace.LexicalRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var check = await _validator.ValidateAsync(
            request.Workspace,
            FileExpectation.Missing(logical),
            cancellationToken).ConfigureAwait(false);
        if (check.State == FileExpectationValidationState.Cancelled)
        {
            return ExtensionInstallTargetObservation.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension target inspection was interrupted.",
                relativePath));
        }

        if (check.State is FileExpectationValidationState.Blocked
            or FileExpectationValidationState.Failed
            || check.Actual is null)
        {
            return ExtensionInstallTargetObservation.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.TargetUnsafe,
                check.Cause ?? check.Failure?.DirectCause ?? "The Extension target is unsafe or unavailable.",
                relativePath));
        }

        return ExtensionInstallTargetObservation.Complete(check.Actual);
    }

    private (string Fingerprint, MarkdownFingerprintState Kind) Fingerprint(byte[] bytes)
    {
        var facts = _contentIdentity.ReadSourceFingerprint(bytes);
        return facts.State switch
        {
            MarkdownFingerprintState.Semantic => (
                RequiredFingerprint(facts),
                MarkdownFingerprintState.Semantic),
            MarkdownFingerprintState.ExactBytes => (
                RequiredFingerprint(facts),
                MarkdownFingerprintState.ExactBytes),
            MarkdownFingerprintState.Unavailable => (
                FileExpectation.Hash(bytes),
                MarkdownFingerprintState.ExactBytes),
            _ => throw new ArgumentOutOfRangeException(
                nameof(bytes),
                facts.State,
                "The Markdown fingerprint state is not defined."),
        };
    }

    private static string RequiredTargetPath(ExtensionPackageFileFact file)
        => file.TargetPath
            ?? throw new InvalidDataException(
                "A validated Extension package file requires its normalized target path.");

    private static string RequiredFingerprint(MarkdownFingerprintFacts facts)
        => facts.Sha256
            ?? throw new InvalidDataException(
                "A completed Extension content fingerprint requires its SHA-256 value.");

    private static ExtensionInstallTargetInspection Stop(
        ExtensionInstallFindingCode code,
        string cause,
        string target)
        => new(null, new ExtensionInstallFinding(code, cause, target));
}

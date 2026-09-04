using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallTargetInspector(
    CliInteractiveSession interactiveSession,
    FileExpectationValidator validator)
{
    private readonly CliInteractiveSession _interactiveSession = interactiveSession;
    private readonly FileExpectationValidator _validator = validator;
    private readonly FrameworkContentIdentity _contentIdentity = new();

    internal async ValueTask<ExtensionInstallTargetInspection> InspectAsync(
        ExtensionInstallTargetInspectionInput input,
        CancellationToken cancellationToken)
    {
        var request = input.Request;
        var packages = input.Packages;
        var current = input.CurrentExtensions;
        var sourceIdentity = input.SourceIdentity;
        var currentPackages = current.Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var currentPaths = current.Paths.ToDictionary(path => path.Path, StringComparer.Ordinal);
        var frameworkPaths = input.FrameworkLifecycle.Targets
            .Select(target => target.Path)
            .Concat(input.FrameworkLifecycle.GeneratedRegions.Select(region => region.Path))
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
                var identity = Fingerprint(bytes);
                if (intendedByPath.TryGetValue(path, out var existing))
                {
                    if (!string.Equals(existing.Fingerprint, identity.Fingerprint, StringComparison.Ordinal)
                        || !string.Equals(existing.FingerprintKind, identity.Kind, StringComparison.Ordinal))
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
            if (frameworkPaths.Contains(intended.Path))
            {
                return Stop(
                    ExtensionInstallFindingCode.OwnershipConflict,
                    "The Extension target is owned by the installed Framework.",
                    intended.Path);
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
            observations.Add(intended.Path, snapshot);
            if (currentPaths.TryGetValue(intended.Path, out var managed))
            {
                if (!managed.Owners.All(owner => intended.Owners.Contains(owner, StringComparer.Ordinal)))
                {
                    return Stop(
                        ExtensionInstallFindingCode.OwnershipConflict,
                        "An existing Extension owner conflicts with the selected package target.",
                        intended.Path);
                }

                if (snapshot.Kind != FileExpectationKind.File
                    || !string.Equals(
                        _contentIdentity.ReadSourceFingerprint(snapshot.Bytes.AsSpan(), managed.FingerprintKind),
                        managed.BaselineFingerprint,
                        StringComparison.Ordinal)
                    || !string.Equals(managed.BaselineFingerprint, intended.Fingerprint, StringComparison.Ordinal)
                    || !string.Equals(managed.FingerprintKind, intended.FingerprintKind, StringComparison.Ordinal))
                {
                    return Stop(
                        ExtensionInstallFindingCode.ManagedDivergence,
                        "The managed Extension target differs from its trusted baseline or selected source.",
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

                if (!input.InitialForceEligiblePaths.Contains(intended.Path))
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
            if (!string.Equals(installed.Source, sourceIdentity, StringComparison.Ordinal)
                || !string.Equals(installed.Version, package.Version, StringComparison.Ordinal)
                || !installed.Dependencies.SequenceEqual(package.Dependencies.Order(StringComparer.Ordinal))
                || !installed.Paths.SequenceEqual(intendedPaths))
            {
                return Stop(
                    ExtensionInstallFindingCode.ManagedDivergence,
                    "The managed Extension package identity differs from the selected source.",
                    package.Id);
            }
        }

        var intendedPackages = current.Packages
            .Where(package => packages.All(selected => selected.Id != package.Id))
            .Concat(packages.Select(package => new LifecycleExtensionPackageV1
            {
                Id = package.Id,
                Version = package.Version,
                Source = sourceIdentity,
                Dependencies = package.Dependencies.Order(StringComparer.Ordinal).ToArray(),
                Paths = package.Payload.Select(RequiredTargetPath)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
            }))
            .OrderBy(package => package.Id, StringComparer.Ordinal)
            .ToArray();
        var intendedPathsLifecycle = current.Paths
            .Where(path => !intendedByPath.ContainsKey(path.Path))
            .Concat(intendedByPath.Values.Select(path => new LifecycleExtensionPathV1
            {
                Path = path.Path,
                Owners = path.Owners.Order(StringComparer.Ordinal).ToArray(),
                BaselineFingerprint = path.Fingerprint,
                FingerprintKind = path.FingerprintKind,
            }))
            .OrderBy(path => path.Path, StringComparer.Ordinal)
            .ToArray();
        var state = new ExtensionInstallTargetState(
            observations,
            intendedByPath,
            eligible,
            new ExtensionLifecycleState
            {
                Coverage = LifecycleSchema.CompleteCoverage,
                Packages = intendedPackages,
                Paths = intendedPathsLifecycle,
            });
        return new ExtensionInstallTargetInspection(state, Finding: null);
    }

    internal async ValueTask<ExtensionInstallFinding?> AuthorizeForceAsync(
        ExtensionInstallRequest request,
        ExtensionInstallTargetState state,
        CancellationToken cancellationToken)
    {
        if (state.EligibleOccupants.Count == 0 || request.Force)
        {
            return null;
        }

        if (!request.AllowInteraction || !_interactiveSession.CanPrompt)
        {
            return new ExtensionInstallFinding(
                ExtensionInstallFindingCode.InitialForceRequired,
                "Eligible initial occupants require explicit --force authority.",
                state.EligibleOccupants[0]);
        }

        var prompt = $"Replace eligible initial occupants {string.Join(", ", state.EligibleOccupants)}? [y/N] ";
        while (true)
        {
            var response = await _interactiveSession.AskAsync(prompt, cancellationToken)
                .ConfigureAwait(false);
            if (response.IsEndOfInput)
            {
                return new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.InteractionEnded,
                    "Input ended before exact initial-force authority was supplied.");
            }

            if (response.Answer is not null
                && (response.Answer.Equals("y", StringComparison.OrdinalIgnoreCase)
                    || response.Answer.Equals("yes", StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            if (response.Answer is not null
                && (response.Answer.Equals("n", StringComparison.OrdinalIgnoreCase)
                    || response.Answer.Equals("no", StringComparison.OrdinalIgnoreCase)))
            {
                return new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.InitialForceRequired,
                    "The eligible initial occupants were not granted force authority.",
                    state.EligibleOccupants[0]);
            }
        }
    }

    private async ValueTask<Observation> ObserveAsync(
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
            return Observation.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension target inspection was interrupted.",
                relativePath));
        }

        if (check.State is FileExpectationValidationState.Blocked
            or FileExpectationValidationState.Failed
            || check.Actual is null)
        {
            return Observation.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.TargetUnsafe,
                check.Cause ?? check.Failure?.DirectCause ?? "The Extension target is unsafe or unavailable.",
                relativePath));
        }

        return Observation.Complete(check.Actual);
    }

    private (string Fingerprint, string Kind) Fingerprint(byte[] bytes)
    {
        var facts = _contentIdentity.ReadSourceFingerprint(bytes);
        return facts.State switch
        {
            MarkdownFingerprintState.Semantic => (
                RequiredFingerprint(facts),
                LifecycleSchema.SemanticFingerprintKind),
            MarkdownFingerprintState.ExactBytes => (
                RequiredFingerprint(facts),
                LifecycleSchema.ExactBytesFingerprintKind),
            MarkdownFingerprintState.Unavailable => (
                FileExpectation.Hash(bytes),
                LifecycleSchema.ExactBytesFingerprintKind),
            _ => throw new ArgumentOutOfRangeException(
                nameof(facts),
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

    private sealed record Observation(
        FileStateSnapshot? Snapshot,
        ExtensionInstallFinding? Finding)
    {
        internal static Observation Complete(FileStateSnapshot snapshot)
            => new(snapshot, null);

        internal static Observation Stop(ExtensionInstallFinding finding)
            => new(null, finding);
    }
}

using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectSubjectSourceBuilder
{
    private const string EmbeddedIdentity = "embedded catalogue";

    internal static ExtensionInspectSubjectPackageSelection ReadSelection(
        ExtensionInspectSubjectPackageInput input,
        int sourceMatches)
    {
        var subjectId = input.Request.StableId;
        var lifecyclePackage = input.Lifecycle.Packages
            .FirstOrDefault(package => package.Id == subjectId);
        var sourcePackage = CanSelectSourcePackage(input.Source)
            && sourceMatches == 1
            ? input.Source.Packages.First(package => package.Id == subjectId)
            : null;
        return new ExtensionInspectSubjectPackageSelection
        {
            InstalledPackage = lifecyclePackage,
            AvailablePackage = sourcePackage,
        };
    }

    internal static ExtensionInspectSubjectPackageSelection ReadEventSelection(
        ExtensionInspectEventSubjectPackageInput input)
    {
        var sourcePackage = ReadEventSourcePackage(input);
        var lifecyclePackage = input.Lifecycle?.Packages
            .FirstOrDefault(package => package.Id == input.Request.StableId);
        return new ExtensionInspectSubjectPackageSelection
        {
            InstalledPackage = lifecyclePackage,
            AvailablePackage = sourcePackage,
        };
    }

    internal static ExtensionInspectSubject ReadEventSubject(
        ExtensionInspectEventSubjectPackageInput input,
        ExtensionInspectSubjectPackageSelection selection,
        int sourceMatches)
    {
        if (input.Source is null && input.Lifecycle is null)
        {
            return new ExtensionInspectSubject
            {
                Supplied = input.Request.StableId,
                Form = ExtensionInspectSubjectForm.StableId,
                Id = input.Request.StableId,
                State = ExtensionInspectSubjectState.NotStarted,
                Candidates = [],
            };
        }

        return ReadSubject(
            input.Request.StableId,
            ReadCandidates(input.Source?.Packages ?? [], input.Request.StableId),
            selection.InstalledPackage,
            sourceMatches,
            input.Source?.State ?? ExtensionSourceReadState.Cancelled);
    }

    internal static string ReadSourceIdentity(ExtensionSourceReadResult source)
    {
        if (source.State == ExtensionSourceReadState.Complete
            && source.Kind == ExtensionSourceKind.EmbeddedCatalogue)
        {
            return EmbeddedIdentity;
        }

        return source.Identity;
    }

    internal static ExtensionInspectSubject ReadSubject(
        string subjectId,
        IReadOnlyList<ExtensionInspectCandidate> sourceCandidates,
        LifecycleInstalledPackage? lifecyclePackage,
        int sourceMatches,
        ExtensionSourceReadState sourceState)
    {
        var candidates = sourceCandidates;
        var ambiguous = sourceMatches > 1;
        var resolved = lifecyclePackage is not null || sourceMatches == 1;
        var state = ReadSubjectState(ambiguous, resolved, sourceState);
        return new ExtensionInspectSubject
        {
            Supplied = subjectId,
            Form = ExtensionInspectSubjectForm.StableId,
            Id = subjectId,
            State = state,
            Candidates = candidates,
        };
    }

    internal static IReadOnlyList<ExtensionInspectCandidate> ReadCandidates(
        IEnumerable<ExtensionPackageFact> packages,
        string subjectId)
        => packages
            .Where(package => package.Id == subjectId)
            .Select(package => new ExtensionInspectCandidate
            {
                Id = package.Id,
                Path = package.ManifestPath,
            })
            .OrderBy(candidate => candidate.Id, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Path, StringComparer.Ordinal)
            .ToArray();

    internal static int ReadSourceMatchCount(
        ExtensionSourceReadResult source,
        string subjectId)
        => source.Packages.Count(package => package.Id == subjectId);

    internal static ExtensionInspectSource ReadSource(
        ExtensionInspectRequest request,
        ExtensionSourceReadResult source)
        => new()
        {
            Supplied = request.ExplicitSource,
            Explicit = request.ExplicitSource is not null,
            Identity = ReadSourceIdentity(source),
            Kind = ReadSourceKind(source.Kind),
            State = ExtensionInspectResultMappings.ReadSourceState(source.State),
        };

    private static ExtensionPackageFact? ReadEventSourcePackage(
        ExtensionInspectEventSubjectPackageInput input)
    {
        if (input.Source is not { } source || !CanSelectSourcePackage(source))
        {
            return null;
        }

        var matches = source.Packages
            .Where(package => package.Id == input.Request.StableId)
            .Take(2)
            .ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static ExtensionInspectSubjectState ReadSubjectState(
        bool ambiguous,
        bool resolved,
        ExtensionSourceReadState sourceState)
    {
        if (ambiguous)
        {
            return ExtensionInspectSubjectState.Ambiguous;
        }

        if (resolved)
        {
            return ExtensionInspectSubjectState.Resolved;
        }

        return sourceState == ExtensionSourceReadState.Blocked
            ? ExtensionInspectSubjectState.Unsafe
            : ExtensionInspectSubjectState.Unknown;
    }

    private static ExtensionInspectSourceKind? ReadSourceKind(ExtensionSourceKind? kind)
        => kind is { } value ? ExtensionInspectResultMappings.ReadSourceKind(value) : null;

    private static bool CanSelectSourcePackage(ExtensionSourceReadResult source)
        => source.State == ExtensionSourceReadState.Complete
            || source.FailureKind is ExtensionSourceFailureKind.DependencyIncomplete
                or ExtensionSourceFailureKind.DependencyCycle;
}

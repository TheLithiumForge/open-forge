using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed partial class ExtensionInspectResultBuilder
{
    private static void SortFindings(List<ExtensionInspectFinding> findings)
    {
        var normalized = ExtensionInspectFindingPolicy.Normalize(findings);
        findings.Clear();
        findings.AddRange(normalized);
    }

    private static CliSemanticStatus ReadStatus(IEnumerable<ExtensionInspectFinding> findings)
    {
        var statuses = findings.Select(finding => finding.Status).ToHashSet();
        foreach (var status in new[]
        {
            CliSemanticStatus.Interrupted,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Attention,
        })
        {
            if (statuses.Contains(status))
            {
                return status;
            }
        }

        return CliSemanticStatus.Complete;
    }

    private static void AddFinding(
        ICollection<ExtensionInspectFinding> findings,
        ExtensionInspectFindingInput input)
        => findings.Add(ExtensionInspectFindingFactory.Create(input));

    private static ExtensionInspectFinding Finding(ExtensionInspectFindingInput input)
        => ExtensionInspectFindingFactory.Create(input);
}

internal static class ExtensionInspectFindingPolicy
{
    private const string InvalidFormationCause =
        "Extension Inspect result formation produced duplicate or invalid findings.";

    internal static IReadOnlyList<ExtensionInspectFinding> Normalize(
        IEnumerable<ExtensionInspectFinding> findings)
    {
        var values = findings
            .Select(NormalizeCandidates)
            .ToList();
        if (values.Any(finding => !IsValid(finding)))
        {
            return Failed();
        }

        values.Sort(Compare);
        for (var index = 1; index < values.Count; index++)
        {
            if (Compare(values[index - 1], values[index]) == 0)
            {
                return Failed();
            }
        }

        return values;
    }

    private static ExtensionInspectFinding NormalizeCandidates(ExtensionInspectFinding finding)
        => finding with
        {
            Candidates = finding.Candidates
                .OrderBy(candidate => candidate.Id, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.Path, StringComparer.Ordinal)
                .ToArray(),
        };

    private static bool IsValid(ExtensionInspectFinding finding)
    {
        if (!Enum.IsDefined(finding.Code) || !Enum.IsDefined(finding.Status))
        {
            return false;
        }

        return ExtensionInspectDefinitions.ReadFindingStatus(finding.Code) == finding.Status;
    }

    private static int Compare(ExtensionInspectFinding left, ExtensionInspectFinding right)
    {
        var code = left.Code.CompareTo(right.Code);
        if (code != 0)
        {
            return code;
        }

        if (IsAmbiguity(left.Code))
        {
            return CompareCandidates(left.Candidates, right.Candidates);
        }

        if (IsPackageOrDependency(left.Code))
        {
            return CompareValues(
                new FindingTextKey(
                    First: left.PackageId,
                    Second: left.Dependency,
                    Third: left.Path),
                new FindingTextKey(
                    First: right.PackageId,
                    Second: right.Dependency,
                    Third: right.Path));
        }

        if (IsPathFinding(left.Code))
        {
            var path = StringComparer.Ordinal.Compare(left.Path, right.Path);
            return path != 0 ? path : CompareLocations(left.Location, right.Location);
        }

        return CompareValues(
            new FindingTextKey(
                First: left.Subject,
                Second: left.PackageId,
                Third: left.Path),
            new FindingTextKey(
                First: right.Subject,
                Second: right.PackageId,
                Third: right.Path));
    }

    private static int CompareValues(
        FindingTextKey left,
        FindingTextKey right)
    {
        var first = StringComparer.Ordinal.Compare(left.First, right.First);
        if (first != 0)
        {
            return first;
        }

        var second = StringComparer.Ordinal.Compare(left.Second, right.Second);
        return second != 0
            ? second
            : StringComparer.Ordinal.Compare(left.Third, right.Third);
    }

    private static int CompareCandidates(
        IReadOnlyList<ExtensionInspectCandidate> left,
        IReadOnlyList<ExtensionInspectCandidate> right)
    {
        var count = Math.Min(left.Count, right.Count);
        for (var index = 0; index < count; index++)
        {
            var id = StringComparer.Ordinal.Compare(left[index].Id, right[index].Id);
            if (id != 0)
            {
                return id;
            }

            var path = StringComparer.Ordinal.Compare(left[index].Path, right[index].Path);
            if (path != 0)
            {
                return path;
            }
        }

        return left.Count.CompareTo(right.Count);
    }

    private static int CompareLocations(SourceLocation? left, SourceLocation? right)
    {
        if (left is null)
        {
            return right is null ? 0 : 1;
        }

        if (right is null)
        {
            return -1;
        }

        var offset = left.ByteOffset.CompareTo(right.ByteOffset);
        if (offset != 0)
        {
            return offset;
        }

        var line = left.Line.CompareTo(right.Line);
        return line != 0 ? line : left.Column.CompareTo(right.Column);
    }

    private static bool IsAmbiguity(ExtensionInspectFindingCode code)
        => code is ExtensionInspectFindingCode.SourceAmbiguous
            or ExtensionInspectFindingCode.IdentityAmbiguous;

    private static bool IsPackageOrDependency(ExtensionInspectFindingCode code)
        => code is ExtensionInspectFindingCode.PackageUnavailable
            or ExtensionInspectFindingCode.PackageInvalid
            or ExtensionInspectFindingCode.DependencyIncomplete
            or ExtensionInspectFindingCode.DependencyCycle
            or ExtensionInspectFindingCode.DependencyConflict
            or ExtensionInspectFindingCode.DependencyChanged;

    private static bool IsPathFinding(ExtensionInspectFindingCode code)
        => code is ExtensionInspectFindingCode.PathUnavailable
            or ExtensionInspectFindingCode.PathInvalid
            or ExtensionInspectFindingCode.OwnershipConflict
            or ExtensionInspectFindingCode.FingerprintUnavailable
            or ExtensionInspectFindingCode.FingerprintFallback
            or ExtensionInspectFindingCode.GeneratedBoundaryInvalid
            or ExtensionInspectFindingCode.PathChanged
            or ExtensionInspectFindingCode.PathCurrentDiverged
            or ExtensionInspectFindingCode.PathMissing
            or ExtensionInspectFindingCode.PathNew
            or ExtensionInspectFindingCode.PathRetired;

    private static IReadOnlyList<ExtensionInspectFinding> Failed()
        =>
        [
            ExtensionInspectFindingFactory.Create(new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.OperationFailed,
                Cause = InvalidFormationCause,
            }),
        ];

    private readonly record struct FindingTextKey(
        string? First,
        string? Second,
        string? Third);
}

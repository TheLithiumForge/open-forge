using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;

namespace OpenForge.Cli.Core.Commands.Update.Models.Effects;

internal enum UpdateLogicalChangeAction
{
    Create,
    Replace,
    Restore,
    Delete,
}

internal enum UpdatePhysicalEffectAction
{
    Create,
    Replace,
    Delete,
}

internal enum UpdatePhysicalEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum UpdatePhysicalEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal sealed record UpdateLogicalChange
{
    internal UpdateLogicalChange(
        UpdateComparisonTargetKind kind,
        UpdateLogicalChangeAction action,
        string? region,
        string? sourceAssetPath)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The logical change kind is not defined.");
        }
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(nameof(action), action, "The logical change action is not defined.");
        }

        ValidateCoordinates(kind, region, sourceAssetPath);
        Kind = kind;
        Action = action;
        Region = region;
        SourceAssetPath = sourceAssetPath;
    }

    internal UpdateComparisonTargetKind Kind { get; }

    internal UpdateLogicalChangeAction Action { get; }

    internal string? Region { get; }

    internal string? SourceAssetPath { get; }

    internal void Validate()
        => ValidateCoordinates(Kind, Region, SourceAssetPath);

    private static void ValidateCoordinates(
        UpdateComparisonTargetKind kind,
        string? region,
        string? sourceAssetPath)
    {
        if (kind == UpdateComparisonTargetKind.File)
        {
            if (region is not null)
            {
                throw new ArgumentException(
                    "A file logical change cannot carry a region identity.",
                    nameof(region));
            }

            if (sourceAssetPath is null || !IsCanonicalRelative(sourceAssetPath))
            {
                throw new ArgumentException(
                    "An authored file logical change requires canonical source provenance.",
                    nameof(sourceAssetPath));
            }

            return;
        }

        if (region is null || !IsCanonicalRelative(region))
        {
            throw new ArgumentException(
                "A managed or generated logical change requires one canonical region identity.",
                nameof(region));
        }

        if (kind == UpdateComparisonTargetKind.ManagedRegion)
        {
            if (sourceAssetPath is null || !IsCanonicalRelative(sourceAssetPath))
            {
                throw new ArgumentException(
                    "A managed logical change requires canonical source provenance.",
                    nameof(sourceAssetPath));
            }

            return;
        }

        if (sourceAssetPath is not null)
        {
            throw new ArgumentException(
                "A generated logical change requires null source provenance.",
                nameof(sourceAssetPath));
        }
    }

    private static bool IsCanonicalRelative(string value)
        => !string.IsNullOrWhiteSpace(value)
            && !value.StartsWith('/')
            && !IsDriveQualified(value)
            && !value.Contains('\\')
            && value.Split('/', StringSplitOptions.None).All(segment => segment.Length != 0
                && segment != "."
                && segment != ".."
                && segment.All(character => !char.IsControl(character)));

    private static bool IsDriveQualified(string value)
        => value.Length >= 2
            && char.IsAsciiLetter(value[0])
            && value[1] == ':';
}

internal sealed record UpdatePhysicalEffect
{
    internal UpdatePhysicalEffect(
        string path,
        UpdatePhysicalEffectAction action,
        IEnumerable<UpdateLogicalChange> changes,
        UpdatePhysicalEffectOutcome outcome,
        UpdatePhysicalEffectResidual residual)
    {
        if (!IsCanonicalRelative(path))
        {
            throw new ArgumentException(
                "Physical effect paths must be canonical workspace-relative paths.",
                nameof(path));
        }
        ArgumentNullException.ThrowIfNull(changes);
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(nameof(action), action, "The physical effect action is not defined.");
        }
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The physical effect outcome is not defined.");
        }
        if (!Enum.IsDefined(residual))
        {
            throw new ArgumentOutOfRangeException(nameof(residual), residual, "The physical effect residual is not defined.");
        }

        var materialized = changes
            .Select(change => change ?? throw new ArgumentException(
                "Physical effects cannot contain null logical changes.",
                nameof(changes)))
            .ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("Physical effects require one logical change.", nameof(changes));
        }

        var identities = new HashSet<(
            UpdateComparisonTargetKind Kind,
            string? Region,
            string? SourceAssetPath)>();
        foreach (var change in materialized)
        {
            change.Validate();
            if (!identities.Add((change.Kind, change.Region, change.SourceAssetPath)))
            {
                throw new ArgumentException(
                    "Physical effects cannot contain duplicate logical identities.",
                    nameof(changes));
            }
        }

        var ordered = materialized
            .OrderBy(change => change.Kind)
            .ThenBy(change => change.Region ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(change => change.SourceAssetPath ?? string.Empty, StringComparer.Ordinal)
            .ToArray();

        Path = path;
        Action = action;
        Changes = new ReadOnlyCollection<UpdateLogicalChange>(ordered);
        Outcome = outcome;
        Residual = residual;
    }

    internal string Path { get; }

    internal UpdatePhysicalEffectAction Action { get; }

    internal IReadOnlyList<UpdateLogicalChange> Changes { get; }

    internal UpdatePhysicalEffectOutcome Outcome { get; }

    internal UpdatePhysicalEffectResidual Residual { get; }

    private static bool IsCanonicalRelative(string value)
        => !string.IsNullOrWhiteSpace(value)
            && !value.StartsWith('/')
            && !IsDriveQualified(value)
            && !value.Contains('\\')
            && value.Split('/', StringSplitOptions.None).All(segment => segment.Length != 0
                && segment != "."
                && segment != ".."
                && segment.All(character => !char.IsControl(character)));

    private static bool IsDriveQualified(string value)
        => value.Length >= 2
            && char.IsAsciiLetter(value[0])
            && value[1] == ':';
}

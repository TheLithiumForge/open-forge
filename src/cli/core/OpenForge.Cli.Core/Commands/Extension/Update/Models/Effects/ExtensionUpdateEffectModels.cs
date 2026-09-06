using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;

internal sealed record ExtensionUpdateLogicalChange
{
    internal ExtensionUpdateLogicalChange(
        ExtensionUpdateComparisonTargetKind kind,
        ExtensionUpdateChangeAction action,
        string? region,
        string? sourceAssetPath)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Extension Update change target kind is not defined.");
        }

        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Extension Update change action is not defined.");
        }

        if (kind == ExtensionUpdateComparisonTargetKind.PackageFile
            && (region is not null || !IsCanonicalRelative(sourceAssetPath)))
        {
            throw new ArgumentException(
                "A package-file change requires source provenance and no region.",
                nameof(sourceAssetPath));
        }

        if (kind == ExtensionUpdateComparisonTargetKind.GeneratedRegion
            && (sourceAssetPath is not null || !IsCanonicalRelative(region)))
        {
            throw new ArgumentException(
                "A generated-region change requires one canonical region and no source provenance.",
                nameof(region));
        }

        Kind = kind;
        Action = action;
        Region = region;
        SourceAssetPath = sourceAssetPath;
    }

    internal ExtensionUpdateComparisonTargetKind Kind { get; }

    internal ExtensionUpdateChangeAction Action { get; }

    internal string? Region { get; }

    internal string? SourceAssetPath { get; }

    private static bool IsCanonicalRelative(string? value)
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

internal sealed record ExtensionUpdateEffect
{
    internal ExtensionUpdateEffect(
        string path,
        string? packageId,
        ExtensionUpdateEffectKind kind,
        ExtensionUpdateEffectAction action,
        IEnumerable<ExtensionUpdateLogicalChange> changes,
        ExtensionUpdateEffectOutcome outcome,
        ExtensionUpdateEffectResidual residual)
    {
        if (!IsCanonicalRelative(path))
        {
            throw new ArgumentException(
                "Extension Update effect paths must be canonical workspace-relative paths.",
                nameof(path));
        }

        if (packageId is not null && string.IsNullOrWhiteSpace(packageId))
        {
            throw new ArgumentException(
                "An Extension Update effect package ID cannot be blank.",
                nameof(packageId));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Extension Update effect kind is not defined.");
        }

        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Extension Update effect action is not defined.");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Extension Update effect outcome is not defined.");
        }

        if (!Enum.IsDefined(residual))
        {
            throw new ArgumentOutOfRangeException(
                nameof(residual),
                residual,
                "The Extension Update effect residual is not defined.");
        }

        ArgumentNullException.ThrowIfNull(changes);
        var values = changes
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update effects cannot contain null logical changes.",
                nameof(changes)))
            .ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException(
                "An Extension Update effect requires one logical change.",
                nameof(changes));
        }

        Path = path;
        PackageId = packageId;
        Kind = kind;
        Action = action;
        Changes = new ReadOnlyCollection<ExtensionUpdateLogicalChange>(values);
        Outcome = outcome;
        Residual = residual;
    }

    internal string Path { get; }

    internal string? PackageId { get; }

    internal ExtensionUpdateEffectKind Kind { get; }

    internal ExtensionUpdateEffectAction Action { get; }

    internal IReadOnlyList<ExtensionUpdateLogicalChange> Changes { get; }

    internal ExtensionUpdateEffectOutcome Outcome { get; }

    internal ExtensionUpdateEffectResidual Residual { get; }

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

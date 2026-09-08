using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Extensions.Identity;

namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal enum InstallManagementClassification
{
    SafeAbsence,
    TrustedExact,
    ManagedDivergence,
    EligibleInitialOccupant,
}

internal enum InstallEffectKind
{
    Directory,
    File,
    ManagedRegion,
    GeneratedRegion,
}

internal enum InstallEffectAction
{
    Create,
    Append,
    Replace,
}

internal enum InstallEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum InstallEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum InstallLifecycleAction
{
    None,
    Preserve,
    Publish,
}

internal enum InstallLifecycleOutcome
{
    NotRequested,
    Planned,
    AlreadyCurrent,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum InstallResultRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum InstallResultVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal sealed record InstallSource
{
    internal InstallSource(string inventoryFingerprint, int assetCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inventoryFingerprint);
        if (assetCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(assetCount),
                assetCount,
                "The Install source asset count cannot be negative.");
        }

        InventoryFingerprint = inventoryFingerprint;
        AssetCount = assetCount;
    }

    internal string InventoryFingerprint { get; }

    internal int AssetCount { get; }
}

internal sealed record InstallFootprint
{
    internal InstallFootprint(
        int payloadFiles,
        int managedRegions,
        int generatedRegions)
    {
        ValidateCount(payloadFiles, nameof(payloadFiles));
        ValidateCount(managedRegions, nameof(managedRegions));
        ValidateCount(generatedRegions, nameof(generatedRegions));
        PayloadFiles = payloadFiles;
        ManagedRegions = managedRegions;
        GeneratedRegions = generatedRegions;
    }

    internal int PayloadFiles { get; }

    internal int ManagedRegions { get; }

    internal int GeneratedRegions { get; }

    private static void ValidateCount(int value, string name)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                name,
                value,
                "Install footprint counts cannot be negative.");
        }
    }
}

internal sealed record InstallEffectInput
{
    public required string Path { get; init; }

    public required InstallEffectKind Kind { get; init; }

    public required InstallEffectAction Action { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required InstallEffectOutcome Outcome { get; init; }

    public required InstallEffectResidual Residual { get; init; }
}

internal sealed record InstallEffect
{
    internal InstallEffect(InstallEffectInput input)
    {
        ValidateCanonicalPath(input.Path, nameof(input.Path));
        ValidateKindAction(input.Kind, input.Action);
        ValidateSourceAssetPath(input.SourceAssetPath);
        _ = input.Outcome switch
        {
            InstallEffectOutcome.Planned
                or InstallEffectOutcome.NotStarted
                or InstallEffectOutcome.Verified
                or InstallEffectOutcome.VerificationFailed
                or InstallEffectOutcome.CompletionUnknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                input.Outcome,
                "The Install effect outcome is not defined."),
        };
        _ = input.Residual switch
        {
            InstallEffectResidual.None
                or InstallEffectResidual.Retained
                or InstallEffectResidual.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                input.Residual,
                "The Install effect residual is not defined."),
        };

        Path = input.Path;
        Kind = input.Kind;
        Action = input.Action;
        SourceAssetPath = input.SourceAssetPath;
        Outcome = input.Outcome;
        Residual = input.Residual;
    }

    internal string Path { get; }

    internal InstallEffectKind Kind { get; }

    internal InstallEffectAction Action { get; }

    internal string? SourceAssetPath { get; }

    internal InstallEffectOutcome Outcome { get; }

    internal InstallEffectResidual Residual { get; }

    private static void ValidateKindAction(
        InstallEffectKind kind,
        InstallEffectAction action)
    {
        var valid = kind switch
        {
            InstallEffectKind.Directory => action == InstallEffectAction.Create,
            InstallEffectKind.File => action is InstallEffectAction.Create or InstallEffectAction.Replace,
            InstallEffectKind.ManagedRegion or InstallEffectKind.GeneratedRegion
                => action is InstallEffectAction.Append or InstallEffectAction.Replace,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Install effect kind is not defined."),
        };
        if (!valid)
        {
            throw new ArgumentException(
                "The Install effect action is not valid for its kind.",
                nameof(action));
        }
    }

    private static void ValidateSourceAssetPath(string? sourceAssetPath)
    {
        if (sourceAssetPath is not null)
        {
            ValidateCanonicalPath(sourceAssetPath, nameof(sourceAssetPath));
        }
    }

    private static void ValidateCanonicalPath(string? path, string name)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalized)
            || !string.Equals(path, normalized, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Install effect paths must be canonical workspace-relative paths.",
                name);
        }
    }
}

internal sealed record InstallLifecycle
{
    internal InstallLifecycle(
        InstallLifecycleAction action,
        InstallLifecycleOutcome outcome)
    {
        _ = action switch
        {
            InstallLifecycleAction.None
                or InstallLifecycleAction.Preserve
                or InstallLifecycleAction.Publish => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Install lifecycle action is not defined."),
        };
        _ = outcome switch
        {
            InstallLifecycleOutcome.NotRequested
                or InstallLifecycleOutcome.Planned
                or InstallLifecycleOutcome.AlreadyCurrent
                or InstallLifecycleOutcome.NotStarted
                or InstallLifecycleOutcome.Verified
                or InstallLifecycleOutcome.VerificationFailed
                or InstallLifecycleOutcome.CompletionUnknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Install lifecycle outcome is not defined."),
        };

        Action = action;
        Outcome = outcome;
    }

    internal InstallLifecycleAction Action { get; }

    internal InstallLifecycleOutcome Outcome { get; }
}

internal sealed record InstallRecovery
{
    internal InstallRecovery(
        InstallResultRecoveryState state,
        string? residualPath)
    {
        var coherent = state switch
        {
            InstallResultRecoveryState.NotRequired
                or InstallResultRecoveryState.NotCreated
                or InstallResultRecoveryState.Removed => residualPath is null,
            InstallResultRecoveryState.Retained => !string.IsNullOrWhiteSpace(residualPath),
            InstallResultRecoveryState.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Install recovery state is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException(
                "The Install recovery residual path does not match its state.",
                nameof(residualPath));
        }

        if (residualPath is not null
            && (!Path.IsPathFullyQualified(residualPath)
                || !string.Equals(
                    Path.GetFullPath(residualPath),
                    residualPath,
                    StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "An Install recovery residual path must be absolute and normalized.",
                nameof(residualPath));
        }

        State = state;
        ResidualPath = residualPath;
    }

    internal InstallResultRecoveryState State { get; }

    internal string? ResidualPath { get; }
}

internal sealed record InstallVerification
{
    internal InstallVerification(InstallResultVerificationState state)
    {
        _ = state switch
        {
            InstallResultVerificationState.NotRequested
                or InstallResultVerificationState.Verified
                or InstallResultVerificationState.Failed
                or InstallResultVerificationState.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Install verification state is not defined."),
        };

        State = state;
    }

    internal InstallResultVerificationState State { get; }
}

internal sealed record InstallResultFacts
{
    internal InstallResultFacts(InstallResultFactsInput input)
    {
        if (input.Classification is { } value)
        {
            _ = value switch
            {
                InstallManagementClassification.SafeAbsence
                    or InstallManagementClassification.TrustedExact
                    or InstallManagementClassification.ManagedDivergence
                    or InstallManagementClassification.EligibleInitialOccupant => true,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(input),
                    value,
                    "The Install management classification is not defined."),
            };
        }

        var effectValues = input.Effects
            .Select(effect => effect ?? throw new ArgumentException(
                "Install effects cannot contain null members.",
                nameof(input)))
            .ToArray();

        Source = input.Source;
        Classification = input.Classification;
        Footprint = input.Footprint;
        Effects = new ReadOnlyCollection<InstallEffect>(effectValues);
        Lifecycle = input.Lifecycle;
        Recovery = input.Recovery;
        Verification = input.Verification;
    }

    internal InstallSource? Source { get; }

    internal InstallManagementClassification? Classification { get; }

    internal InstallFootprint? Footprint { get; }

    internal IReadOnlyList<InstallEffect> Effects { get; }

    internal InstallLifecycle Lifecycle { get; }

    internal InstallRecovery Recovery { get; }

    internal InstallVerification Verification { get; }
}

internal sealed record InstallResultFactsInput
{
    public required InstallSource? Source { get; init; }

    public required InstallManagementClassification? Classification { get; init; }

    public required InstallFootprint? Footprint { get; init; }

    public required IEnumerable<InstallEffect> Effects { get; init; }

    public required InstallLifecycle Lifecycle { get; init; }

    public required InstallRecovery Recovery { get; init; }

    public required InstallVerification Verification { get; init; }
}

internal sealed record InstallResultCompletion
{
    public required InstallLifecycle Lifecycle { get; init; }

    public required InstallRecovery Recovery { get; init; }

    public required InstallResultVerificationState Verification { get; init; }
}

using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;

internal enum ExtensionCreateFindingCode
{
    InvalidInput,
    CatalogueUnavailable,
    CatalogueUnsafe,
    DestinationCollision,
    DestinationChanged,
    ApplicationFailed,
    VerificationFailed,
    ConfirmationRequired,
    Interrupted,
}

internal enum ExtensionCreateVerificationState
{
    NotStarted,
    Planned,
    Verified,
    Failed,
    Unavailable,
}

internal sealed record ExtensionCreateVerification
{
    public required ExtensionCreateVerificationState Catalogue { get; init; }

    public required ExtensionCreateVerificationState Destination { get; init; }

    public required ExtensionCreateVerificationState Manifest { get; init; }

    public required ExtensionCreateVerificationState Payload { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record ExtensionCreateFinding
{
    public required ExtensionCreateFindingCode Code { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }
}

internal sealed record ExtensionCreateResult : ICliCommandResult
{
    private IReadOnlyList<ExtensionCreateEffect> _intendedEffects = [];
    private IReadOnlyList<ExtensionCreateEffect> _appliedEffects = [];
    private IReadOnlyList<ExtensionCreateFinding> _findings = [];

    public string Command => ExtensionCreateDefinitions.CommandIdentity;

    public required CliSemanticStatus Status { get; init; }

    public CliWorkspace? Workspace => null;

    public required CliNextAction? Next { get; init; }

    public required string? Catalogue { get; init; }

    public required string? Destination { get; init; }

    public required string? StableId { get; init; }

    public required ExtensionCreateManifest? Manifest { get; init; }

    public required ExtensionCreateMode Mode { get; init; }

    public bool Automatic { get; init; }

    public required IReadOnlyList<ExtensionCreateEffect> IntendedEffects
    {
        get => _intendedEffects;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _intendedEffects = new ReadOnlyCollection<ExtensionCreateEffect>(value.ToArray());
        }
    }

    public required IReadOnlyList<ExtensionCreateEffect> AppliedEffects
    {
        get => _appliedEffects;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _appliedEffects = new ReadOnlyCollection<ExtensionCreateEffect>(value.ToArray());
        }
    }

    public required ExtensionCreateVerification Verification { get; init; }

    public required IReadOnlyList<ExtensionCreateFinding> Findings
    {
        get => _findings;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _findings = new ReadOnlyCollection<ExtensionCreateFinding>(value.ToArray());
        }
    }
}

internal sealed record ExtensionCreateResultOutcome
{
    public required CliSemanticStatus Status { get; init; }

    public string? Catalogue { get; init; }

    public string? Destination { get; init; }

    public IReadOnlyList<ExtensionCreateEffect> IntendedEffects { get; init; } = [];

    public IReadOnlyList<ExtensionCreateEffect> AppliedEffects { get; init; } = [];

    public required ExtensionCreateVerification Verification { get; init; }

    public ExtensionCreateFinding? Finding { get; init; }
}

internal sealed record ExtensionCreateResultInput
{
    public required CliSemanticStatus Status { get; init; }

    public required string? Catalogue { get; init; }

    public required string? Destination { get; init; }

    public required string? StableId { get; init; }

    public required ExtensionCreateManifest? Manifest { get; init; }

    public required ExtensionCreateMode Mode { get; init; }

    public bool Automatic { get; init; }

    public required IReadOnlyList<ExtensionCreateEffect> IntendedEffects { get; init; }

    public required IReadOnlyList<ExtensionCreateEffect> AppliedEffects { get; init; }

    public required ExtensionCreateVerification Verification { get; init; }

    public required ExtensionCreateFinding? Finding { get; init; }
}

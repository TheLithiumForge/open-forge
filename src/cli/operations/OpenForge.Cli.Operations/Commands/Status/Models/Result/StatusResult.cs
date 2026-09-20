using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal enum StatusFindingCode
{
    InvalidInput,
    WorkspaceUnavailable,
    WorkspaceNotDirectory,
    WorkspaceUnsafe,
    EntryUnavailable,
    EmbeddedFrameworkUnavailable,
    ContextInventoryIncomplete,
    StartupContextUnavailable,
    ContinuityContextUnavailable,
    RootCategoriesUnavailable,
    GeneratedNavigationChanged,
    GeneratedNavigationMissing,
    GeneratedNavigationUnavailable,
    GeneratedNavigationBlocked,
    FrameworkLifecycleUntrusted,
    FrameworkLifecycleIncomplete,
    FrameworkOwnershipObservation,
    ExtensionOwnershipObservation,
    FrameworkLifecycleBlocked,
    FrameworkTargetChanged,
    FrameworkTargetMissing,
    FrameworkTargetUnavailable,
    FrameworkTargetBlocked,
    ExtensionLifecycleUntrusted,
    ExtensionLifecycleIncomplete,
    ExtensionLifecycleBlocked,
    ExtensionSourceUnavailable,
    ExtensionTargetChanged,
    ExtensionTargetMissing,
    ExtensionTargetUnavailable,
    ExtensionTargetBlocked,
    RecoveryCandidateVerified,
    RecoveryDraftIncomplete,
    RecoveryFinalMalformed,
    RecoveryFinalUnsupported,
    RecoveryFinalUnavailable,
    RecoveryCatalogueUnavailable,
    LibraryRecordMalformed,
    LibraryRecordUnavailable,
    LibraryOwnershipObservation,
    LibrarySourceRootInvalid,
    LibrarySourceRootAliased,
    LibrarySourceRootUnavailable,
    LibraryProjectionMissing,
    LibraryProjectionChanged,
    LibraryProjectionUnavailable,
    LibraryProjectionBlocked,
    LibraryExtensionCollision,
    OperationFailed,
    Interrupted,
    GeneratedNavigationMetadataInvalid,
}
internal sealed record StatusFinding
{
    public required StatusFindingCode Code { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }

    public string? Source { get; init; }

    // Ownership is retained when a shared physical target is reported for
    // more than one installed Extension or Library.  Presentation can then
    // name each owner without parsing a display subject.
    public string? Owner { get; init; }

    internal string MachineCode => StatusDefinitions.ReadFindingCode(Code);
}

internal sealed record StatusFacts
{
    public required StatusInstallation Installation { get; init; }

    public required StatusContext Context { get; init; }

    public required StatusStructure Structure { get; init; }

    public required StatusLifecycle Lifecycle { get; init; }

    public required StatusLibrary Library { get; init; }

    public required StatusRecovery Recovery { get; init; }
}

internal sealed record StatusResult : ICliCommandResult
{
    public string Command => StatusDefinitions.CommandIdentity;

    public required CliSemanticStatus Status { get; init; }

    public required CliWorkspace? Workspace { get; init; }

    public required CliNextAction? Next { get; init; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit
        => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal required StatusFacts Facts { get; init; }

    internal required IReadOnlyList<StatusFinding> Findings { get; init; }
}

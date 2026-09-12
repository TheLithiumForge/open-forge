using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorHumanVocabulary
{
    internal static string Outcome(CliSemanticStatus value) => value switch
    {
        CliSemanticStatus.Complete => "Workspace checks completed.",
        CliSemanticStatus.Attention => "The workspace needs attention.",
        CliSemanticStatus.Incomplete => "Some workspace checks could not finish.",
        CliSemanticStatus.Invalid => "Doctor could not start because the input is invalid.",
        CliSemanticStatus.Blocked => "Workspace checks are blocked.",
        CliSemanticStatus.Failed => "Workspace checks failed.",
        CliSemanticStatus.Interrupted => "Workspace checks were interrupted.",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The status is not defined."),
    };

    internal static string Status(CliSemanticStatus value)
        => value == CliSemanticStatus.Attention ? "requires attention" : CliStatusDefinitions.Read(value).MachineName;

    internal static string Selection(CliWorkspaceSelectionMethod value) => value switch
    {
        CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
        CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The workspace selection is not defined."),
    };

    internal static string Domain(DoctorDomainKind value) => value switch
    {
        DoctorDomainKind.WorkspaceEntry => "Workspace",
        DoctorDomainKind.RecoveryResiduals => "Recovery",
        DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation => "Routes and navigation",
        DoctorDomainKind.LocalReferences => "Links",
        DoctorDomainKind.FrameworkLifecycle => "Framework",
        DoctorDomainKind.ExtensionLifecycle => "Extensions",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The category is not defined."),
    };

    internal static string Severity(DoctorFindingSeverity value) => value switch
    {
        DoctorFindingSeverity.Information => "INFO",
        DoctorFindingSeverity.Warning => "WARNING",
        DoctorFindingSeverity.Error => "ERROR",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The severity is not defined."),
    };

    internal static string Resolution(DoctorResolutionLane value) => value switch
    {
        DoctorResolutionLane.SafeExact => "exact repair available for preview",
        DoctorResolutionLane.GuidedChoice => "choose a target after reviewing the evidence",
        DoctorResolutionLane.TargetedOperation => "use the indicated command",
        DoctorResolutionLane.ManualDecision => "manual decision required",
        DoctorResolutionLane.BlockedRepair => "repair is blocked",
        DoctorResolutionLane.Informational => "information only",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The resolution is not defined."),
    };

    internal static string Subject(DoctorSubjectKind value) => value switch
    {
        DoctorSubjectKind.Workspace => "Workspace",
        DoctorSubjectKind.Path => "Path",
        DoctorSubjectKind.Route => "Route",
        DoctorSubjectKind.GeneratedRegion => "Generated navigation",
        DoctorSubjectKind.SourceOccurrence => "Link",
        DoctorSubjectKind.Target => "Target",
        DoctorSubjectKind.RecoveryItem => "Recovery record",
        DoctorSubjectKind.ManagedFile => "Managed file",
        DoctorSubjectKind.Extension => "Extension",
        DoctorSubjectKind.Dependency => "Dependency",
        DoctorSubjectKind.Library => "Library",
        DoctorSubjectKind.LibrarySourceRoot => "Library source",
        DoctorSubjectKind.LibraryMapping => "Library mapping",
        DoctorSubjectKind.LibraryProjection => "Library link",
        DoctorSubjectKind.LibraryResidual => "Library recovery",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The subject kind is not defined."),
    };

    internal static string CandidateBasis(DoctorCandidateBasisKind value) => value switch
    {
        DoctorCandidateBasisKind.Filename => "Filename match",
        DoctorCandidateBasisKind.Title => "Title match",
        DoctorCandidateBasisKind.LiteralContent => "Content match",
        DoctorCandidateBasisKind.RouteNeighborhood => "Nearby route",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The candidate match is not defined."),
    };

    internal static string Provenance(DoctorProvenanceSource value) => value switch
    {
        DoctorProvenanceSource.WorkspaceEntry => "workspace files",
        DoctorProvenanceSource.RecoveryResiduals => "recovery records",
        DoctorProvenanceSource.RouteInventory => "route scan",
        DoctorProvenanceSource.RouteMetadata => "route metadata",
        DoctorProvenanceSource.GeneratedNavigation => "generated navigation",
        DoctorProvenanceSource.LocalReferences => "local links",
        DoctorProvenanceSource.FrameworkLifecycle => "Framework installation record",
        DoctorProvenanceSource.FrameworkPayload => "distributed Framework content",
        DoctorProvenanceSource.ExtensionLifecycle => "Extension installation record",
        DoctorProvenanceSource.ExtensionSource => "Extension source",
        DoctorProvenanceSource.LifecycleOwnership => "file ownership records",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The evidence source is not defined."),
    };
}

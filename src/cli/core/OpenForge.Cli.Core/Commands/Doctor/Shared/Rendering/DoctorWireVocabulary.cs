using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorWireVocabulary
{
    internal static string Status(CliSemanticStatus value) => CliStatusDefinitions.Read(value).MachineName;

    internal static string WorkspaceSelection(CliWorkspaceSelectionMethod value)
        => value switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
            _ => Undefined(value),
        };

    internal static string Domain(DoctorDomainKind value)
        => value switch
        {
            DoctorDomainKind.WorkspaceEntry => "workspace-entry",
            DoctorDomainKind.RecoveryResiduals => "recovery-residuals",
            DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation => "routes-metadata-overwrites-generated-navigation",
            DoctorDomainKind.LocalReferences => "local-references",
            DoctorDomainKind.FrameworkLifecycle => "framework-lifecycle",
            DoctorDomainKind.ExtensionLifecycle => "extension-lifecycle",
            _ => Undefined(value),
        };

    internal static string Boundary(DoctorBoundaryKind value)
        => value switch
        {
            DoctorBoundaryKind.Workspace => "workspace",
            DoctorBoundaryKind.RecoveryStore => "recovery-store",
            DoctorBoundaryKind.RouteUniverse => "route-universe",
            DoctorBoundaryKind.LocalReferenceUniverse => "local-reference-universe",
            DoctorBoundaryKind.FrameworkLifecycle => "framework-lifecycle",
            DoctorBoundaryKind.ExtensionLifecycle => "extension-lifecycle",
            _ => Undefined(value),
        };

    internal static string Coverage(DoctorCoverageState value)
        => value switch
        {
            DoctorCoverageState.Complete => "complete",
            DoctorCoverageState.Incomplete => "incomplete",
            DoctorCoverageState.Blocked => "blocked",
            _ => Undefined(value),
        };

    internal static string Lifecycle(OperationalLifecycleState value)
        => value switch
        {
            OperationalLifecycleState.Absent => "absent",
            OperationalLifecycleState.Trusted => "trusted",
            OperationalLifecycleState.Untrusted => "untrusted",
            OperationalLifecycleState.Incomplete => "incomplete",
            OperationalLifecycleState.Blocked => "blocked",
            _ => Undefined(value),
        };

    internal static string SourceAvailability(OperationalSourceAvailability value)
        => value switch
        {
            OperationalSourceAvailability.Available => "available",
            OperationalSourceAvailability.Unavailable => "unavailable",
            OperationalSourceAvailability.NotApplicable => "not-applicable",
            _ => Undefined(value),
        };

    internal static string ValueState(OperationalValueState value)
        => value switch
        {
            OperationalValueState.Available => "available",
            OperationalValueState.Unavailable => "unavailable",
            OperationalValueState.NotApplicable => "not-applicable",
            _ => Undefined(value),
        };

    internal static string Limitation(DoctorLimitationKind value)
        => value switch
        {
            DoctorLimitationKind.Unavailable => "unavailable",
            DoctorLimitationKind.Unsupported => "unsupported",
            DoctorLimitationKind.Incomplete => "incomplete",
            DoctorLimitationKind.Blocked => "blocked",
            _ => Undefined(value),
        };

    private static string Undefined<T>(T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(nameof(value), value, $"The {typeof(T).Name} value is not defined.");
}

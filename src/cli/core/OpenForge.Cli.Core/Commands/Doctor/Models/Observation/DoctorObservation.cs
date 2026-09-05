using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Observation;

internal sealed record DoctorObservation(
    WorkspaceEntryDoctorView WorkspaceEntry,
    RecoveryResidualDoctorView RecoveryResiduals,
    RouteDoctorView Routes,
    LocalReferenceDoctorView LocalReferences,
    FrameworkLifecycleDoctorView FrameworkLifecycle,
    ExtensionLifecycleDoctorView ExtensionLifecycle);

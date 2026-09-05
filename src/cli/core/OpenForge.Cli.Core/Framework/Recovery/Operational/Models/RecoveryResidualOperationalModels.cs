using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

internal sealed record RecoveryCandidateObservation(
    string Path,
    RecoveryBundleCandidateKind Kind,
    RecoveryBundleIntegrity Integrity);

internal sealed record RecoveryResidualStatusView(
    OperationalViewState State,
    IReadOnlyList<RecoveryCandidateObservation> Candidates);

internal sealed record RecoveryResidualDoctorView(
    OperationalViewState State,
    IReadOnlyList<RecoveryCandidateObservation> Candidates);

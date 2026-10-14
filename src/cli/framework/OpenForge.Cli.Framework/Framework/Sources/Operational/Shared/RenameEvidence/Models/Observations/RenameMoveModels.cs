using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

internal sealed record RenameCanonicalEntrypointMove(RenameEvidenceObservation Proof, string CanonicalPath);

internal sealed record RenameSourceMoveRequest(CliWorkspace Workspace, LocalReferenceObservation Reference);

internal sealed record RenameSourceMoveTarget(
    RenameEvidenceObservation SourceMove,
    RenameEvidenceObservation? TargetMove,
    string OldTargetPath,
    string NewTargetPath,
    string? Fragment,
    FileStateSnapshot TargetSnapshot);

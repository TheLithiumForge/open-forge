using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

internal sealed record LibraryRecoveryPermissionRead(
    WorkspacePermissionRead Observation,
    WorkspacePermissionEvaluation Evaluation,
    bool IsAdmitted);

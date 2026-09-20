using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

internal sealed record LibraryRecoveryPermissionRead(
    WorkspaceSettingsRead Observation,
    WorkspacePermissionEvaluation Evaluation,
    bool IsAdmitted);

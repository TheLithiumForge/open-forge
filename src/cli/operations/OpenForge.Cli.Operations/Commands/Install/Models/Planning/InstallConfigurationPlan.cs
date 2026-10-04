using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal sealed record InstallConfigurationPlan(
    WorkspaceSettingsRead Settings,
    InstallTargetRead Ignore,
    PlannedFileChange? SettingsChange,
    PlannedFileChange? IgnoreChange,
    bool UsesInitialAdoption = false);

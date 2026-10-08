using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;

namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal sealed record InstallConfigurationPlan(
    WorkspaceSettingsRead Settings,
    InstallTargetRead? Ignore,
    PlannedFileChange? SettingsChange,
    PlannedFileChange? IgnoreChange,
    bool UsesInitialAdoption = false)
{
    internal InstallFrontmatterSelection? Frontmatter { get; init; }
    internal InstallConfigurationFrontmatterPlan? FrontmatterPlan { get; init; }
    internal ImmutableArray<SourceSharingRoute> GitIgnoredRoutes { get; init; } = [];
}

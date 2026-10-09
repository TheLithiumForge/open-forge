using System.Collections.Immutable;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

internal enum InstallPreset { Essentials, FullCore, Custom }
internal enum InstallRouteAction { Add, Remove, GitIgnore }
internal sealed record InstallRouteSelection(string Id, InstallRouteAction Action);
internal sealed record InstallSetupInput(bool Configure, InstallPreset? Preset, ImmutableArray<InstallRouteSelection> Overrides)
{
    internal FrontmatterForm? Frontmatter { get; init; }
}
internal sealed record InstallConfiguration(bool Configure, InstallPreset Preset, ImmutableArray<InstallRouteSelection> Routes);
internal sealed record InstallRouteLock(string Id, string Action);
internal sealed record InstallRouteQuestion(ImmutableArray<InstallRouteSelection> Routes, ImmutableArray<InstallRouteLock> Locks, bool Installed);
internal sealed record InstallSetupInteraction(
    CliPrompt<InstallPreset, InstallPreset> Preset,
    CliPrompt<InstallRouteQuestion, ImmutableArray<InstallRouteSelection>> Routes,
    CliPrompt<InstallFrontmatterQuestion, string> Frontmatter);
internal sealed record InstallSetupBoundary(InstallFindingCode Code, string Cause);
internal sealed record InstallSetupResolution(InstallConfiguration? Configuration, InstallSetupBoundary? Boundary = null, bool Cancelled = false)
{
    internal InstallFrontmatterSelection? Frontmatter { get; init; }
}

internal sealed record InstallIgnoreSectionSpan(int Start, int EndExclusive, string Body);

using System.Collections.Immutable;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Commands.Install.Models.Result;

namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

internal enum InstallPreset { Essentials, FullCore, Custom }
internal enum InstallRouteAction { Add, Remove, GitIgnore }
internal sealed record InstallRouteSelection(string Id, InstallRouteAction Action);
internal sealed record InstallSetupInput(bool Configure, InstallPreset? Preset, ImmutableArray<InstallRouteSelection> Overrides);
internal sealed record InstallConfiguration(bool Configure, InstallPreset Preset, ImmutableArray<InstallRouteSelection> Routes);
internal sealed record InstallRouteQuestion(ImmutableArray<InstallRouteSelection> Routes, ImmutableArray<string> FixedIds);
internal sealed record InstallSetupInteraction(
    CliPrompt<InstallPreset, InstallPreset> Preset,
    CliPrompt<InstallRouteQuestion, string> Route,
    CliPrompt<InstallRouteSelection, InstallRouteAction> Action);
internal sealed record InstallSetupBoundary(InstallFindingCode Code, string Cause);
internal sealed record InstallSetupResolution(InstallConfiguration? Configuration, InstallSetupBoundary? Boundary = null, bool Cancelled = false);

internal sealed record InstallIgnoreSectionSpan(int Start, int EndExclusive, string Body);

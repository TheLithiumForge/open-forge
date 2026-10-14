using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

internal sealed record InstallSetupRecap(
    bool Configure, InstallPreset Preset, ImmutableArray<InstallRouteSelection> Routes, ImmutableArray<InstallRouteSelection> PreviousRoutes);

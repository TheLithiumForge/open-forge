namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

internal sealed record InstallFrontmatterResolution(
    InstallFrontmatterSelection? Selection,
    InstallFrontmatterQuestion? Question = null,
    InstallSetupBoundary? Boundary = null);

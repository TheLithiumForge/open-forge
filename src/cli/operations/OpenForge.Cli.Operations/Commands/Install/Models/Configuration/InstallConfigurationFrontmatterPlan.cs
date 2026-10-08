using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

internal sealed record InstallConfigurationFrontmatterInput
{
    public required CliWorkspace Workspace { get; init; }
    public required WorkspaceOwnershipRead Ownership { get; init; }
    public required FrameworkPayload Payload { get; init; }
    public required IReadOnlyDictionary<string, byte[]> RenderedPayload { get; init; }
    public required WorkspaceSettingsDocument IntendedSettings { get; init; }
    public required InstallFrontmatterSelection Selection { get; init; }
}

internal sealed record InstallConfigurationFrontmatterReplacement
{
    public required InstallTargetRead Before { get; init; }
    public required byte[] IntendedBytes { get; init; }
    public required string SourceAssetPath { get; init; }
    internal string Path => Before.RelativePath;
}

internal sealed record InstallConfigurationFrontmatterObservation(
    string Path, FileExpectation Expectation, string? SourceHash);

internal sealed record InstallConfigurationFrontmatterPlan
{
    public ImmutableArray<InstallConfigurationFrontmatterReplacement> Replacements { get; init; } = [];
    public ImmutableArray<InstallFrontmatterKeptFile> Kept { get; init; } = [];
    public ImmutableArray<InstallConfigurationFrontmatterObservation> Observations { get; init; } = [];
}

using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Tools.Models;

/// <summary>
/// One supported tool. Every command is a fixed catalogue constant, so no part of a command line ever comes from input.
/// </summary>
internal sealed record ToolEntry
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Purpose { get; init; }
    public required string Category { get; init; }
    public required Uri Upstream { get; init; }
    public required string Licence { get; init; }
    public required string TestedVersion { get; init; }
    public required ToolMethod Method { get; init; }
    public required ToolCommand Detection { get; init; }
    public required ToolStep Install { get; init; }
    public ToolStep? Setup { get; init; }
    public required ToolUninstall Uninstall { get; init; }
    public required ImmutableArray<ToolIntegration> Integrations { get; init; }
}

/// <summary>What a person needs before the install method can run.</summary>
internal enum ToolMethod
{
    Npm,
    Script,
}

internal enum ToolShell
{
    /// <summary>cmd.exe on Windows and /bin/sh elsewhere, so package-manager shims resolve as they do for a person.</summary>
    System,

    /// <summary>Windows PowerShell, for upstream commands documented only in PowerShell.</summary>
    PowerShell,
}

/// <summary>A command exactly as a person would type it in <see cref="Shell"/>.</summary>
internal sealed record ToolCommand(string Text, ToolShell Shell = ToolShell.System);

/// <summary>Ordered commands with an optional plain sentence about their effect and optional plain notes.</summary>
internal sealed record ToolStep(ImmutableArray<ToolCommand> Commands, string? Effect = null)
{
    public ImmutableArray<string> Notes { get; init; } = [];
}

internal sealed record ToolUninstall(ImmutableArray<ToolCommand> Commands, string Description);

/// <summary>Open Forge's own self-identifying change to one workspace file.</summary>
internal abstract record ToolIntegration(string Path, string Purpose);

/// <summary>Adds the archive pattern to a JSON string array, creating the file or member when missing.</summary>
internal sealed record ToolJsonUnion(string Path, string Purpose, string Member, string PatternSuffix) : ToolIntegration(Path, Purpose);

/// <summary>Adds the archive pattern between two fixed marker lines.</summary>
internal sealed record ToolMarkedBlock(string Path, string Purpose, string StartMarker, string EndMarker, string PatternPrefix, string PatternSuffix)
    : ToolIntegration(Path, Purpose);

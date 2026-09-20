using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Extension.Install.Models;

internal sealed record ExtensionInstallData
{
    public required string Mode { get; init; }

    public required bool Force { get; init; }

    public required bool Automatic { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExtensionInstallDataSource? Source { get; init; }

    public required IReadOnlyList<ExtensionInstallDataPackage> Packages { get; init; }

    public required ExtensionInstallDataPermissions Permissions { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExtensionInstallDataSelection? Selection { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Sections { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? EntriesUnchanged { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FrameworkFingerprint { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExtensionInstallDataVerification? Verification { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExtensionInstallDataRecovery? Recovery { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<ExtensionInstallDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetails { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextNextLines { get; init; } = [];

    [JsonIgnore]
    internal bool IsNoOp { get; init; }
}

internal sealed record ExtensionInstallDataSource
{
    public required string Kind { get; init; }

    public required string? Path { get; init; }
}

internal sealed record ExtensionInstallDataPackage
{
    public required string Id { get; init; }

    public required string Version { get; init; }

    public required bool Selected { get; init; }

    public required IReadOnlyList<string> RequiredBy { get; init; }
}

internal sealed record ExtensionInstallDataPermissions
{
    public required string Decision { get; init; }

    public required IReadOnlyList<string> Required { get; init; }

    public required IReadOnlyList<string> Missing { get; init; }

    public required bool Saved { get; init; }
}

internal sealed record ExtensionInstallDataSelection
{
    public required string Method { get; init; }
}

internal sealed record ExtensionInstallDataVerification
{
    public required string Targets { get; init; }

    public required string Topology { get; init; }

    public required string ExtensionsLifecycle { get; init; }

    public required string FrameworkLifecycle { get; init; }
}

internal sealed record ExtensionInstallDataRecovery
{
    public required string State { get; init; }

    public required IReadOnlyList<string> ProtectedPaths { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed record ExtensionInstallDataTextRow(string Path, string Text, string? Detail = null);

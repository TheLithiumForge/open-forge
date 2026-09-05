namespace OpenForge.Cli.Core.Commands.Status.Models.Presentation;

internal sealed class StatusJsonLifecycle
{
    public required StatusJsonFrameworkLifecycle Framework { get; init; }

    public required StatusJsonExtensionLifecycle Extensions { get; init; }
}
internal sealed class StatusJsonFrameworkLifecycle
{
    public required string State { get; init; }

    public required string SourceAvailability { get; init; }

    public required StatusJsonFrameworkTarget[] Targets { get; init; }
}

internal sealed class StatusJsonFrameworkTarget
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string? Region { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required string State { get; init; }
}

internal sealed class StatusJsonExtensionLifecycle
{
    public required string State { get; init; }

    public required string SourceAvailability { get; init; }

    public required StatusJsonInstalledExtension[] Installed { get; init; }

    public required StatusJsonManagedExtensionFiles ManagedFiles { get; init; }
}

internal sealed class StatusJsonInstalledExtension
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required string SourceAvailability { get; init; }

    public required string[] Dependencies { get; init; }

    public required string[] Paths { get; init; }
}

internal sealed class StatusJsonManagedExtensionFiles
{
    public required StatusJsonManagedTargetCounts Counts { get; init; }

    public required StatusJsonExtensionTarget[] Targets { get; init; }
}

internal sealed class StatusJsonManagedTargetCounts
{
    public required StatusJsonIntegerValue Current { get; init; }

    public required StatusJsonIntegerValue Changed { get; init; }

    public required StatusJsonIntegerValue Missing { get; init; }

    public required StatusJsonIntegerValue Unavailable { get; init; }

    public required StatusJsonIntegerValue Blocked { get; init; }
}

internal sealed class StatusJsonExtensionTarget
{
    public required string Path { get; init; }

    public required string[] Owners { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required string State { get; init; }
}

using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal sealed record ExtensionInspectSubjectPackageInput
{
    public required ExtensionInspectRequest Request { get; init; }

    public required ExtensionSourceReadResult Source { get; init; }

    public required WorkspaceOwnershipRead Ownership { get; init; }

    public required ICollection<ExtensionInspectFinding> Findings { get; init; }
}

internal sealed record ExtensionInspectEventSubjectPackageInput
{
    public required ExtensionInspectRequest Request { get; init; }

    public ExtensionSourceReadResult? Source { get; init; }

    public WorkspaceOwnershipRead? Ownership { get; init; }
}

internal sealed record ExtensionInspectSubjectPackageSelection
{
    public required ExtensionOwnership? InstalledPackage { get; init; }

    public required ExtensionPackageFact? AvailablePackage { get; init; }
}

internal sealed record ExtensionInspectSubjectPackageFacts
{
    public required ExtensionInspectSubject Subject { get; init; }

    public required ExtensionInspectSource Source { get; init; }

    public required ExtensionInspectLifecycle Lifecycle { get; init; }

    public required ExtensionInspectInstalled Installed { get; init; }

    public required ExtensionInspectAvailable Available { get; init; }

    public required ExtensionInspectSubjectPackageSelection Selection { get; init; }
}

internal sealed record ExtensionInspectEventSubjectPackageFacts
{
    public required ExtensionInspectSubject Subject { get; init; }

    public required ExtensionInspectSource? Source { get; init; }

    public required ExtensionInspectLifecycle? Lifecycle { get; init; }

    public required ExtensionInspectInstalled? Installed { get; init; }

    public required ExtensionInspectAvailable? Available { get; init; }
}

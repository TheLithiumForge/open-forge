using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal sealed record ExtensionInspectSubjectPackageInput
{
    public required ExtensionInspectRequest Request { get; init; }

    public required ExtensionSourceReadResult Source { get; init; }

    public required LifecycleReadResult Lifecycle { get; init; }

    public required ICollection<ExtensionInspectFinding> Findings { get; init; }
}

internal sealed record ExtensionInspectEventSubjectPackageInput
{
    public required ExtensionInspectRequest Request { get; init; }

    public ExtensionSourceReadResult? Source { get; init; }

    public LifecycleReadResult? Lifecycle { get; init; }
}

internal sealed record ExtensionInspectSubjectPackageSelection
{
    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

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

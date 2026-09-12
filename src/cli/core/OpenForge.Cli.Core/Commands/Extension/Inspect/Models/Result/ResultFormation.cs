using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal sealed record ExtensionInspectEmptyResultInput(
    CliSemanticStatus Status,
    CliWorkspace? Workspace,
    ExtensionInspectSubject Subject,
    IReadOnlyList<ExtensionInspectFinding> Findings,
    CliNextAction? Next);

internal sealed record ExtensionInspectCountsInput
{
    public required LifecycleReadResult Lifecycle { get; init; }

    public required ExtensionSourceReadResult Source { get; init; }

    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

    public required ExtensionPackageFact? AvailablePackage { get; init; }

    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required ExtensionInspectPathFacts PathFacts { get; init; }

    public required ExtensionInspectComparison Comparison { get; init; }

    public required ExtensionInspectGenerated Generated { get; init; }

    public required int FindingCount { get; init; }
}

/// <summary>
/// Cohesive inputs for one finding. Keeping the subject dimensions together
/// prevents call sites from forwarding the same identity and path values in
/// a long positional argument list.
/// </summary>
internal sealed record ExtensionInspectFindingInput
{
    public required ExtensionInspectFindingCode Code { get; init; }

    public string? Subject { get; init; }

    public string? PackageId { get; init; }

    public string? Dependency { get; init; }

    public string? Path { get; init; }

    public required string Cause { get; init; }

    public IReadOnlyList<ExtensionInspectCandidate> Candidates { get; init; } = [];
}

internal sealed record ExtensionInspectEventInput
{
    public required ExtensionInspectRequest Request { get; init; }

    public ExtensionSourceReadResult? Source { get; init; }

    public LifecycleReadResult? Lifecycle { get; init; }

    public IReadOnlyList<ExtensionInspectCurrentPath>? CurrentPaths { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required ExtensionInspectFindingCode Code { get; init; }

    public required string Cause { get; init; }
}

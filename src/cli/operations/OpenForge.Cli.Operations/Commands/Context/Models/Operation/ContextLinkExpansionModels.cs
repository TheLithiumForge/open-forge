using OpenForge.Cli.Core.Commands.Context.Models.Result;

namespace OpenForge.Cli.Core.Commands.Context.Models.Operation;

internal sealed record ContextLinkExpansionFormation
{
    public required IReadOnlyList<ContextSelectedGraphSource> StartupSources { get; init; }

    public required IReadOnlyList<ContextSelectedGraphSource> CombinedSources { get; init; }

    public required IReadOnlyList<ContextSelectedGraphSource> ResultSources { get; init; }

    public required IReadOnlyList<ContextLink> Links { get; init; }

    public required IReadOnlyList<ContextFinding> Findings { get; init; }

    public required bool Complete { get; init; }

    public required bool Blocked { get; init; }

    internal static ContextLinkExpansionFormation NotRequested(ContextClosureResolution closure)
    {
        return new ContextLinkExpansionFormation
        {
            StartupSources = closure.StartupSources,
            CombinedSources = closure.CombinedSources,
            ResultSources = closure.ResultSources,
            Links = [],
            Findings = [],
            Complete = true,
            Blocked = false,
        };
    }

    internal static ContextLinkExpansionFormation Expanded(
        IReadOnlyList<ContextSelectedGraphSource> startupSources,
        ContextLinkExpansionRun combined,
        IReadOnlyList<ContextSelectedGraphSource> resultSources)
    {
        return new ContextLinkExpansionFormation
        {
            StartupSources = startupSources,
            CombinedSources = combined.Sources,
            ResultSources = resultSources,
            Links = combined.Links,
            Findings = combined.Findings,
            Complete = combined.Complete,
            Blocked = combined.Blocked,
        };
    }
}

internal sealed record ContextLinkExpansionRun
{
    public required IReadOnlyList<ContextSelectedGraphSource> Sources { get; init; }

    public required IReadOnlyList<ContextLink> Links { get; init; }

    public required IReadOnlyList<ContextFinding> Findings { get; init; }

    public required bool Complete { get; init; }

    public required bool Blocked { get; init; }
}

using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Context.Models.Binding;

internal sealed record ContextBindingInput
{
    public required IReadOnlyList<string> Sources { get; init; }

    public required bool AdditionsOnly { get; init; }

    public required IReadOnlyList<string> ContentValues { get; init; }

    public required IReadOnlyList<string> FollowLinksValues { get; init; }

    public IReadOnlyList<string> ForValues { get; init; } = [];

    public IReadOnlyList<string> WorkingPaths { get; init; } = [];

    public bool WorkingPathsSupplied { get; init; }

    public required CliOptionResultFacts AdditionsOnlyFacts { get; init; }

    public required CliOptionResultFacts ContentFacts { get; init; }

    public required CliOptionResultFacts FollowLinksFacts { get; init; }

    public CliOptionResultFacts? ForFacts { get; init; }

    public required CliDetail? SuppliedDetail { get; init; }

    public required CliDetail EffectiveView { get; init; }
}

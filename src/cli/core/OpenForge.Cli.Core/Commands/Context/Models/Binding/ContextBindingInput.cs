using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Commands.Context.Models.Binding;

internal sealed record ContextBindingInput
{
    public required IReadOnlyList<string> Sources { get; init; }

    public required bool AdditionsOnly { get; init; }

    public required IReadOnlyList<string> ContentValues { get; init; }

    public required IReadOnlyList<string> FollowLinksValues { get; init; }

    public required CliOptionResultFacts AdditionsOnlyFacts { get; init; }

    public required CliOptionResultFacts ContentFacts { get; init; }

    public required CliOptionResultFacts FollowLinksFacts { get; init; }

    public required CliView? SuppliedView { get; init; }

    public required CliView EffectiveView { get; init; }
}

using OpenForge.Cli.Core.Commands.Context.Models.Result;

namespace OpenForge.Cli.Core.Commands.Context.Models.Operation;

internal sealed record ContextProjectionFormation
{
    public required IReadOnlyList<ContextPathProjection> Paths { get; init; }

    public required IReadOnlyList<ContextSource> Sources { get; init; }

    public required IReadOnlyList<ContextFinding> Findings { get; init; }

    public required bool ProjectionComplete { get; init; }

    public required bool HasAttention { get; init; }
}

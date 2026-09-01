using OpenForge.Cli.Core.Commands.Find.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal sealed record FindResultMatchInput(
    IReadOnlyList<FindMatch> Matches,
    IReadOnlyList<FindProjection> Projections,
    FindContentSelection Content);

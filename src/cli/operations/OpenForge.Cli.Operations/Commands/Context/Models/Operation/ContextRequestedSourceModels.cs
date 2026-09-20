using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Context.Models.Operation;

internal sealed record ContextResolvedRequest
{
    public required string Reference { get; init; }

    public required SourceReferenceResolution Resolution { get; init; }

    public required ContextGraphSource? Source { get; init; }

    public required ContextRequestedSource Result { get; init; }
}

internal sealed record ContextRequestedSourceResolution
{
    public required IReadOnlyList<ContextResolvedRequest> Requests { get; init; }

    public required IReadOnlyList<ContextFinding> Findings { get; init; }

    public required bool HasInvalid { get; init; }

    public required bool Blocked { get; init; }
}

internal sealed record ContextLoadingClosureResolution
{
    public required ContextLoadingSelection Selection { get; init; }

    public required IReadOnlyList<ContextFinding> Findings { get; init; }

    public required bool Incomplete { get; init; }

    public required bool Blocked { get; init; }
}

internal sealed record ContextLoadingSelection
{
    public required IReadOnlyList<ContextSelectedGraphSource> StartupSources { get; init; }

    public required IReadOnlyList<ContextSelectedGraphSource> CombinedSources { get; init; }
}

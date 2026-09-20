namespace OpenForge.Cli.Core.Commands.Index.Models.Result;

internal sealed record IndexFindingDetails
{
    internal string? ParentPath { get; init; }
    internal string? Operand { get; init; }
    internal IndexInputProblem? InputProblem { get; init; }
    internal string? CorrectedSourceId { get; init; }
    internal IndexMetadataProblem? MetadataProblem { get; init; }
    internal int? Line { get; init; }
    internal int? Column { get; init; }
}

internal enum IndexInputProblem { Invalid, UnknownId, UnknownPath, Unsupported, Folder }
internal enum IndexMetadataProblem { Missing, Invalid, UnclosedFrontmatter }

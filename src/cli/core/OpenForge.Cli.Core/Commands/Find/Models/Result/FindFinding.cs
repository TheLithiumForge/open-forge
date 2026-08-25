using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal enum FindSelectorRole
{
    Include,
    Exclude,
}

internal sealed record FindFinding
{
    internal FindFinding(
        FindFindingCode code,
        CliSemanticStatus status,
        string? subject,
        string cause,
        FindSelectorRole? selectorRole,
        int? selectorOccurrence,
        FindSourceIdentity? source,
        SourceLayerKind? layer,
        string? path,
        FindRegion? region,
        FindSourceLocation? location,
        IEnumerable<FindSourceIdentity> candidates)
    {
        _ = FindDefinitions.ReadFindingCode(code);
        if (FindDefinitions.ReadFindingStatus(code) != status)
        {
            throw new ArgumentException("The Find finding status does not match its machine code.", nameof(status));
        }

        if (subject is not null && subject.Length == 0)
        {
            throw new ArgumentException("A Find finding subject cannot be empty.", nameof(subject));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (selectorRole is { } role && !Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(selectorRole), selectorRole, "The Find selector role is not defined.");
        }

        if (selectorOccurrence is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(selectorOccurrence), selectorOccurrence, "A Find selector occurrence must be positive.");
        }

        if ((selectorRole is null) != (selectorOccurrence is null))
        {
            throw new ArgumentException("A selector occurrence requires a selector role and vice versa.");
        }

        if (layer is { } findingLayer && !Enum.IsDefined(findingLayer))
        {
            throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find finding layer is not defined.");
        }

        if (path is not null && path.Length == 0)
        {
            throw new ArgumentException("A Find finding path cannot be empty.", nameof(path));
        }

        if (path is not null && !SourceLogicalPath.IsCanonicalRoot(path))
        {
            throw new ArgumentException("Find finding paths must be canonical .agents paths.", nameof(path));
        }

        if (path is not null && layer is { } pathLayer
            && (!path.EndsWith(".md", StringComparison.Ordinal)
                || (pathLayer == SourceLayerKind.Overwrite) != path.EndsWith(".overwrite.md", StringComparison.Ordinal)))
        {
            throw new ArgumentException("The Find finding path and layer must agree.", nameof(path));
        }

        if (layer is not null && (source is null || path is null))
        {
            throw new ArgumentException("A Find finding layer requires source and path identity.", nameof(layer));
        }

        if (source is not null && path is not null && layer is { } identifiedLayer)
        {
            var expectedPath = identifiedLayer switch
            {
                SourceLayerKind.Base => source.Path,
                SourceLayerKind.Overwrite => $"{source.Path[..^3]}.overwrite.md",
                _ => throw new ArgumentOutOfRangeException(nameof(layer), identifiedLayer, "The Find finding layer is not defined."),
            };
            if (!string.Equals(path, expectedPath, StringComparison.Ordinal))
            {
                throw new ArgumentException("A Find finding path must belong to its source identity.", nameof(path));
            }
        }

        ArgumentNullException.ThrowIfNull(candidates);
        var materializedCandidates = candidates.ToArray();
        if (materializedCandidates.Any(candidate => candidate is null)
            || materializedCandidates
                .Select(candidate => (candidate.Id, candidate.Path))
                .Distinct()
                .Count() != materializedCandidates.Length)
        {
            throw new ArgumentException("Find finding candidates must be non-null and unique.", nameof(candidates));
        }


        for (var index = 1; index < materializedCandidates.Length; index++)
        {
            var idOrder = string.CompareOrdinal(materializedCandidates[index - 1].Id, materializedCandidates[index].Id);
            if (idOrder > 0
                || idOrder == 0
                    && string.CompareOrdinal(materializedCandidates[index - 1].Path, materializedCandidates[index].Path) >= 0)
            {
                throw new ArgumentException("Find finding candidates must use ordinal ID and path order.", nameof(candidates));
            }
        }

        Code = code;
        Status = status;
        Subject = subject;
        Cause = cause;
        SelectorRole = selectorRole;
        SelectorOccurrence = selectorOccurrence;
        Source = source;
        Layer = layer;
        Path = path;
        Region = region;
        Location = location;
        Candidates = Array.AsReadOnly(materializedCandidates);
    }

    internal FindFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Subject { get; }

    internal string Cause { get; }

    internal FindSelectorRole? SelectorRole { get; }

    internal int? SelectorOccurrence { get; }

    internal FindSourceIdentity? Source { get; }

    internal SourceLayerKind? Layer { get; }

    internal string? Path { get; }

    internal FindRegion? Region { get; }

    internal FindSourceLocation? Location { get; }

    internal IReadOnlyList<FindSourceIdentity> Candidates { get; }
}

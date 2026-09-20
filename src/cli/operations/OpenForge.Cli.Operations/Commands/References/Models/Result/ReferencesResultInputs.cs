using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Models.Result;

internal sealed record ReferencesFindingInput(
    ReferencesFindingCode Code,
    string Cause)
{
    internal ReferencesDirection? Direction { get; init; }

    internal string? Subject { get; init; }

    internal SourceUniverseSelectorRole? SelectorRole { get; init; }

    internal int? SelectorOccurrence { get; init; }

    internal ReferencesSourceIdentity? Source { get; init; }

    internal SourceLayerKind? Layer { get; init; }

    internal string? Path { get; init; }

    internal SourceLocation? Location { get; init; }

    internal SourceLocation? DestinationLocation { get; init; }

    internal IEnumerable<ReferencesSourceIdentity>? Candidates { get; init; }

    internal CliSemanticStatus? StatusOverride { get; init; }
}

internal sealed class ReferencesResultInput
{
    internal required ReferencesRequestEcho Request { get; init; }

    internal required ReferencesSource? Source { get; init; }

    internal required ReferencesIncomingSelection? IncomingSelection { get; init; }

    internal required IEnumerable<ReferencesOccurrence> IncomingOccurrences { get; init; }

    internal required IEnumerable<ReferencesOccurrence> OutgoingOccurrences { get; init; }

    internal required ReferencesCoverage IncomingCoverage { get; init; }

    internal required ReferencesCoverage OutgoingCoverage { get; init; }

    internal required IEnumerable<ReferencesFinding> Findings { get; init; }
}

using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;

namespace OpenForge.Cli.Core.Commands.References.Models.Binding;

internal sealed record ReferencesBindingInput(
    string? SourceReference,
    string? DirectionSpelling,
    ReferencesDirection? Direction,
    IReadOnlyList<SourceUniverseSelectorOccurrence> SelectorOccurrences,
    bool SourceWasSupplied,
    bool DirectionWasSupplied,
    bool FilterWasSupplied,
    string? DirectionCause,
    string? FilterCause);

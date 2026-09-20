using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Shared.Serialization;

/// <summary>
/// Owns the wire names for observed Library facts that commands outside the
/// Library family also report. Doctor and Status both read the same mappings.
/// </summary>
internal static class LibraryWireVocabulary
{
    internal static string MappingObservation(LibraryMappingObservationState value)
        => value switch
        {
            LibraryMappingObservationState.Current => "current",
            LibraryMappingObservationState.Missing => "missing",
            LibraryMappingObservationState.Changed => "changed",
            LibraryMappingObservationState.Blocked => "blocked",
            LibraryMappingObservationState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "The Library mapping observation state is not defined."),
        };
}

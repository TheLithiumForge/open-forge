using OpenForge.Cli.Core.Commands.References.Models.Presentation;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static partial class ReferencesJsonProjection
{
    private static ReferencesJsonFinding Finding(ReferencesFinding finding)
        => new()
        {
            Code = ReferencesDefinitions.ReadMachineName(finding.Code),
            Status = Status(finding.Status),
            Direction = finding.Direction is null ? null : Direction(finding.Direction.Value),
            Subject = finding.Subject,
            Cause = finding.Cause,
            SelectorRole = finding.SelectorRole is null ? null : Role(finding.SelectorRole.Value),
            SelectorOccurrence = finding.SelectorOccurrence,
            Source = finding.Source is null ? null : Identity(finding.Source),
            Layer = finding.Layer is null ? null : Layer(finding.Layer.Value),
            Path = finding.Path,
            Location = finding.Location is null ? null : Location(finding.Location),
            DestinationLocation = finding.DestinationLocation is null
                ? null
                : Location(finding.DestinationLocation),
            Candidates = finding.Candidates.Select(Identity).ToArray(),
        };

    private static ReferencesJsonIdentity Identity(ReferencesSourceIdentity identity)
        => new() { Id = identity.Id, Path = identity.Path };

    private static ReferencesJsonLocation Location(SourceLocation location)
        => new()
        {
            Line = location.Line,
            Column = location.Column,
            ByteOffset = location.ByteOffset,
            ByteLength = location.ByteLength,
        };
}

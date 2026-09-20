using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Source;

internal sealed class RouteMetadataParser
{
    internal RouteSourceMetadata Parse(
        SourceAuthoredMetadataFacts facts,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (facts.State != SourceAuthoredMetadataState.Complete)
        {
            return RouteSourceMetadata.WithoutValues(
                facts.State switch
                {
                    SourceAuthoredMetadataState.NotApplicable => RouteSourceMetadataState.NotApplicable,
                    SourceAuthoredMetadataState.Missing => RouteSourceMetadataState.Missing,
                    SourceAuthoredMetadataState.Malformed => RouteSourceMetadataState.Malformed,
                    _ => throw new ArgumentOutOfRangeException(nameof(facts), facts.State, "The authored metadata state is not defined."),
                },
                isCompatibilityEntrypoint,
                isOverwritePresent,
                facts.ObservedDescription);
        }

        return RouteSourceMetadata.Complete(
            facts.Description
                ?? throw new InvalidOperationException("Complete Open Forge metadata requires a description."),
            facts.Tags,
            isCompatibilityEntrypoint,
            isOverwritePresent);
    }
}

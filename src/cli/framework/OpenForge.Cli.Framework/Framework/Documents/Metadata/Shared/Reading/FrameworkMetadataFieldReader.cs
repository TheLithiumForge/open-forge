using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Reading;

internal static class FrameworkMetadataFieldReader
{
    internal const string Description = "description";
    internal const string Responsibility = "responsibility";
    internal const string Tags = "tags";

    internal static FrameworkMetadataField? Read(string? name)
        => name switch
        {
            Description => FrameworkMetadataField.Description,
            Responsibility => FrameworkMetadataField.Responsibility,
            Tags => FrameworkMetadataField.Tags,
            _ => null,
        };
}

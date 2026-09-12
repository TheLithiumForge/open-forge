using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Serialization;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

internal static class LifecycleJsonSyntaxValidator
{
    internal static void ValidateNoDuplicateProperties(
        ReadOnlySpan<byte> utf8Json,
        LifecycleSection selectedSection)
    {
        if (!Enum.IsDefined(selectedSection))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedSection),
                selectedSection,
                "The lifecycle section is not defined.");
        }

        JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(
            utf8Json,
            IsUnselectedSection(selectedSection));
    }

    private static string IsUnselectedSection(
        LifecycleSection selectedSection)
        => selectedSection switch
        {
            LifecycleSection.Framework => LifecycleSchema.ExtensionsProperty,
            LifecycleSection.Extensions => LifecycleSchema.FrameworkProperty,
            _ => throw new ArgumentOutOfRangeException(
                nameof(selectedSection),
                selectedSection,
                "The lifecycle section is not defined."),
        };
}

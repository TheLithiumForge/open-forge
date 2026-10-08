using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed partial class RouteUpdateMetadataEditPlanner
{
    private static RouteUpdateCanonicalMetadataValues PreserveRootScalarStyles(
        RouteUpdateMetadataEditPlanningInput input,
        FrameworkDocumentMetadata intended,
        RouteUpdateCanonicalMetadataValues canonical)
        => canonical with
        {
            Description = EmitRootScalar(input.Layout.DescriptionMember, intended.Description, canonical.Description),
            Responsibility = intended.Responsibility is { } responsibility
                ? EmitRootScalar(input.Layout.ResponsibilityMember, responsibility, canonical.Responsibility)
                : null,
        };

    private static string EmitRootScalar(
        RouteUpdateMetadataMember? member,
        string value,
        string? canonical)
    {
        var style = member?.Entry.Value.Scalar?.Style ?? ScalarStyle.Any;
        if (style is not (ScalarStyle.SingleQuoted or ScalarStyle.DoubleQuoted))
        {
            return canonical ?? throw new InvalidOperationException("A supplied scalar requires one emitted value.");
        }

        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        var emitter = new Emitter(writer);
        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());
        emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, value, style, true, true));
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new StreamEnd());
        return writer.ToString().TrimEnd('\r', '\n');
    }
}

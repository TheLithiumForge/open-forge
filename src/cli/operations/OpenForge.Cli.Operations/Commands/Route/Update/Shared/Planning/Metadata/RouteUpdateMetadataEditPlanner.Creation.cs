using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed partial class RouteUpdateMetadataEditPlanner
{
    private RouteUpdateMetadataEditPlanBuild BuildCreation(
        RouteUpdateMetadataEditPlanningInput input,
        FrameworkDocumentMetadata intended,
        RouteUpdateCanonicalMetadataValues canonical)
    {
        var layout = input.Layout;
        var emission = intended;
        if (!layout.ApplyToMembers.IsEmpty)
        {
            emission = new FrameworkDocumentMetadata(
                intended.Description, intended.Tags, intended.Responsibility, applyTo: []);
        }

        var emitted = _emitter.Emit(emission, input.DefaultForm)
            .Replace("\n", layout.Newline, StringComparison.Ordinal);
        var replacement = emitted;
        if (layout.CreateHeader)
        {
            replacement = $"---{layout.Newline}{emitted}---{layout.Newline}";
        }
        else if (layout.CreationLength > 0)
        {
            replacement = emitted.TrimEnd('\r', '\n');
        }
        else if (layout.Source.Length > 0 && layout.Source[^1] is not ('\r' or '\n'))
        {
            replacement = $"{layout.Newline}{emitted}";
        }

        var draft = new RouteUpdateMetadataEditDraft(layout);
        draft.Edits.Add(new RouteUpdateMetadataEdit
        {
            DocumentStart = layout.CreationStart,
            DocumentLength = layout.CreationLength,
            Replacement = replacement,
        });
        draft.Preview.Add(new RouteUpdatePreviewHunk
        {
            Kind = RouteUpdatePreviewKind.MetadataField,
            Before = string.Empty,
            Expected = replacement.TrimEnd('\r', '\n'),
        });
        if (!layout.ApplyToMembers.IsEmpty && !TryPlanApplyTo(input, canonical, draft))
        {
            return RouteUpdateMetadataEditPlanBuild.Unsafe(UnsafeCause);
        }

        return RouteUpdateMetadataEditPlanBuild.Complete(draft.Build());
    }
}

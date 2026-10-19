using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

internal sealed class LocalReferenceReadAccumulator(WorkspaceSettingsDocument settings)
{
    internal WorkspaceSettingsDocument Settings { get; } = settings;

    internal int IgnoredLinkCount { get; set; }

    internal List<LocalReferenceObservation> References { get; } = [];

    internal List<LocalReferenceSourceObservation> Sources { get; } = [];

    internal Dictionary<string, MarkdownDocumentFacts> Documents { get; } = new(StringComparer.Ordinal);
}

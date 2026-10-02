using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;

internal sealed class WorkspaceAdoptionEntrypointComposer
{
    private const string WorkspaceTag = "Workspace";
    private const string InheritedAxiomsSentinel =
        "- inherited - No local axioms; loaded ancestor axioms remain active.";

    internal WorkspaceAdoptionDocumentPlan Create(string canonicalPath, string newline)
    {
        if (newline is not ("\n" or "\r\n"))
        {
            return Blocked(canonicalPath, "field 'newline' must be LF or CRLF.");
        }

        if (!SourceLogicalPath.IsCanonicalSource(canonicalPath)
            || !SourceFormClassifier.TryClassify(canonicalPath, out var form)
            || !SourceFormClassifier.IsEntrypoint(form))
        {
            return Blocked(canonicalPath, "field 'canonicalPath' must identify a recognized entrypoint source.");
        }

        var directoryPath = SourceLogicalPath.ReadParent(canonicalPath);
        var title = ReadDirectoryName(directoryPath);
        var description = $"Workspace entrypoint for {title} at {directoryPath}";
        var source = $$"""
            ---
            open-forge:
              description: {{WorkspaceAdoptionDocumentText.QuoteYamlScalar(description)}}
              tags: [{{WorkspaceTag}}]
            ---

            # {{WorkspaceAdoptionDocumentText.EscapeMarkdownHeading(title)}}

            ## Axioms

            {{InheritedAxiomsSentinel}}

            ## Entries

            {{MarkdownEntriesSectionReader.EmptyEntry}}
            """.ReplaceLineEndings(newline);
        if (!source.EndsWith(newline, StringComparison.Ordinal))
        {
            source += newline;
        }

        if (!WorkspaceAdoptionDocumentText.TryEncode(source, out var intendedBytes))
        {
            return Blocked(canonicalPath, "field 'description' cannot be represented as strict UTF-8.");
        }

        return new WorkspaceAdoptionDocumentPlan(
            intendedBytes,
            [WorkspaceAdoptionAction.EntrypointCreated],
            ["open-forge.description", "open-forge.tags"],
            [
                new WorkspaceAdoptionDerivation("open-forge.description", WorkspaceAdoptionDerivationSource.RelativePath),
                new WorkspaceAdoptionDerivation("open-forge.tags", WorkspaceAdoptionDerivationSource.RequiredTag),
            ],
            cause: null);
    }

    private static string ReadDirectoryName(string directoryPath)
    {
        var separator = directoryPath.LastIndexOf('/');
        return directoryPath[(separator + 1)..];
    }

    private static WorkspaceAdoptionDocumentPlan Blocked(string path, string reason)
        => new(
            intendedBytes: null,
            actions: [],
            fields: [],
            derivation: [],
            cause: $"{path}: {reason}");
}

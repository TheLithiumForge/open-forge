using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Content;

internal static class WorkspaceLayoutPayloadGuidance
{
    internal const string CliSkillDirectory = WorkspaceLayout.NativeSkillsPath + "/open-forge-cli";
    internal const string ReferencesDirectoryName = "references";
    internal const string CommonReferenceName = "common";
    internal const string DiscoveryReferenceName = "discovery";
    internal const string RoutesReferenceName = "routes";
    internal const string PackagesReferenceName = "packages";
    private const string PathPlaceholder = "...";

    // Shipped-template anchors are checked against the embedded payload by the guidance tests.
    internal const string ExampleContributionPath = "../CONTRIBUTING.md";
    internal const string ExampleExcludedPath = WorkspaceLayout.DefaultMainRoot + "/patterns/_patterns.md";
    internal const string ExampleRepairNotePath = WorkspaceLayout.DefaultMainRoot + "/memory/crystallized/repair-note.md";
    internal const string ExampleRepairTargetPath = WorkspaceLayout.DefaultMainRoot + "/memory/crystallized/order-rule.md";
    internal const string ContextExamplePrefix = "context ";
    internal const string PreviousSchemaSentence = "New settings use schema version 3.";
    internal const string ExactPathIntroduction = "An exact ";
    internal const string InitPathIntroduction = "Targets are route IDs or exact ";
    internal const string MovePathIntroduction = "The destination is an exact workspace-relative ";
    internal const string OutsidePathIntroduction = "outside ";
    internal const string IncludeCategoryTemplate = "Inside `" + WorkspaceLayout.DefaultMainRoot + "/`, use only a direct child folder's catalogue.";
    internal const string ExcludeCategoryTemplate = "Inside `" + WorkspaceLayout.DefaultMainRoot + "/`, use a direct child folder's catalogue to remove its row from the loader.";
    private static readonly Lazy<FrameworkPayload?> ShippedPayload = new(() => EmbeddedFrameworkPayloadReader.Read().Payload);
    private static readonly HashSet<string> ReferenceNames =
    [
        CommonReferenceName + SourceFormClassifier.MarkdownExtension,
        DiscoveryReferenceName + SourceFormClassifier.MarkdownExtension,
        RoutesReferenceName + SourceFormClassifier.MarkdownExtension,
        PackagesReferenceName + SourceFormClassifier.MarkdownExtension,
    ];

    internal static void AddReplacements(
        WorkspaceLayout layout, string packagePath, ReadOnlyMemory<byte> bytes, MarkdownDocumentFacts document,
        IDictionary<MarkdownTextSpan, string> replacements)
    {
        if (!IsGuidance(packagePath) || ShippedPayload.Value?.Find(packagePath) is not { } template
            || !bytes.Span.SequenceEqual(template.Bytes.AsSpan()))
            return;

        var categoryReference = $"a direct child folder's catalogue under `{layout.MainRoot}/`, or the native Skills catalogue under `{WorkspaceLayout.NativeSkillsPath}/`";
        AddTemplateReference(document, IncludeCategoryTemplate, $"Use {categoryReference}.", replacements);
        AddTemplateReference(document, ExcludeCategoryTemplate, $"Use {categoryReference} to remove its row from the loader.", replacements);

        // Only the exact shipped templates own these examples and code references.
        foreach (var code in document.OpaqueSpans.Where(code => code.IsCode))
        {
            if (replacements.Keys.Any(span => span.Start <= code.Span.Start && span.End >= code.Span.End)) continue;
            var original = document.Source[code.Span.Start..code.Span.End];
            var rendered = RenderCode(layout, packagePath, original, document.Source[..code.Span.Start]);
            if (rendered != original) replacements[code.Span] = rendered;
        }

        if (document.Headings.FirstOrDefault(heading => heading.Level == 1) is { } title)
        {
            var introduction = $"\n\nContext files live in `{layout.MainRoot}/`. Native Skills stay in `{WorkspaceLayout.NativeSkillsPath}/`, "
                + $"and settings and ownership stay in `{WorkspaceLayout.DefaultMainRoot}/`. "
                + $"Use `{SourceWorkspaceRelativePath.LiteralReferencePrefix}{PathPlaceholder}` for exact workspace-relative CLI operands, "
                + $"such as `{SourceWorkspaceRelativePath.LiteralReferencePrefix}{layout.HumanGuidePath}`.";
            replacements[new MarkdownTextSpan(title.Span.End, 0)] = introduction;
        }

        foreach (var visible in document.VisibleText)
        {
            var text = document.Source[visible.Span.Start..visible.Span.End];
            if (text.Contains(PreviousSchemaSentence, StringComparison.Ordinal))
                replacements[visible.Span] = text.Replace(PreviousSchemaSentence,
                    $"New settings use schema version {WorkspaceSettingsDefinitions.SchemaVersion}.", StringComparison.Ordinal);
        }
    }

    private static void AddTemplateReference(
        MarkdownDocumentFacts document, string original, string rendered, IDictionary<MarkdownTextSpan, string> replacements)
    {
        var start = document.Source.IndexOf(original, StringComparison.Ordinal);
        if (start >= 0) replacements[new MarkdownTextSpan(start, original.Length)] = rendered;
    }

    private static bool IsGuidance(string path)
        => path == SourceLogicalPath.HumanGuidePath
            || path == CliSkillDirectory + "/" + SourceFormClassifier.NativeSkillFileName
            || path.StartsWith(CliSkillDirectory + "/" + ReferencesDirectoryName + "/", StringComparison.Ordinal)
                && ReferenceNames.Contains(path[(path.LastIndexOf('/') + 1)..]);

    private static string RenderCode(WorkspaceLayout layout, string packagePath, string code, string preceding)
    {
        if (code == $"`{WorkspaceLayout.DefaultMainRoot}/{PathPlaceholder}`")
            return $"`{SourceWorkspaceRelativePath.LiteralReferencePrefix}{PathPlaceholder}`";
        if ((code == $"`{WorkspaceLayout.DefaultMainRoot}`" || code == $"`{WorkspaceLayout.DefaultMainRoot}/`")
            && (preceding.EndsWith(ExactPathIntroduction, StringComparison.Ordinal)
                || preceding.EndsWith(InitPathIntroduction, StringComparison.Ordinal)
                || preceding.EndsWith(MovePathIntroduction, StringComparison.Ordinal)))
            return $"`{SourceWorkspaceRelativePath.LiteralReferencePrefix}{PathPlaceholder}`";
        if (code == $"`{WorkspaceLayout.DefaultMainRoot}/`")
            return $"the content folders (`{layout.MainRoot}/` and `{WorkspaceLayout.NativeSkillsPath}/`)";
        if (code == $"`{WorkspaceLayout.DefaultMainRoot}`")
        {
            if (preceding.EndsWith(OutsidePathIntroduction, StringComparison.Ordinal))
                return $"the content folders (`{layout.MainRoot}/` and `{WorkspaceLayout.NativeSkillsPath}/`)";
            return $"`{layout.MainRoot}`";
        }
        if (code == $"`{ContextExamplePrefix}{SourceLogicalPath.HumanGuidePath}`")
            return $"`{ContextExamplePrefix}{SourceWorkspaceRelativePath.LiteralReferencePrefix}{layout.HumanGuidePath}`";
        if (packagePath == SourceLogicalPath.HumanGuidePath && code == $"`{ExampleExcludedPath}`")
            return $"`{SourceWorkspaceRelativePath.LiteralReferencePrefix}{layout.MapPayloadPath(ExampleExcludedPath)}`";
        if (code == $"`[\"{WorkspaceSettingsDefinitions.MemoryArchivePath}\"]`")
            return $"`[\"{layout.MapPayloadPath(WorkspaceSettingsDefinitions.MemoryArchivePath)}\"]`";

        if (code.StartsWith('`') && code.EndsWith('`')
            && PortableWorkspacePath.TryNormalize(code[1..^1], out var path)
            && path.StartsWith(WorkspaceLayout.DefaultMainRoot + "/", StringComparison.Ordinal))
            return $"`{layout.MapPayloadPath(path)}`";

        if (packagePath == SourceLogicalPath.HumanGuidePath
            && WorkspaceLayoutPayloadRenderer.RebaseDestination(layout, packagePath, ExampleContributionPath) is { } contribution)
            return code.Replace(ExampleContributionPath, contribution, StringComparison.Ordinal);

        if (packagePath == CliSkillDirectory + "/" + ReferencesDirectoryName + "/" + RoutesReferenceName + SourceFormClassifier.MarkdownExtension)
            return code.Replace(ExampleRepairNotePath, layout.MapPayloadPath(ExampleRepairNotePath), StringComparison.Ordinal)
                .Replace(ExampleRepairTargetPath, layout.MapPayloadPath(ExampleRepairTargetPath), StringComparison.Ordinal);
        return code;
    }
}

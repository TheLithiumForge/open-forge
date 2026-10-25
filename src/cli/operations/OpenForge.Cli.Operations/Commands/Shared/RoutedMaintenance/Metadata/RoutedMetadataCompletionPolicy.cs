using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;

// Index, Repair and Doctor share the ordinary eligibility defined by the Forgiving Maintenance design.
// A user-owned Skill package is ordinary content: its SKILL.md gets native metadata and other Markdown gets the usual defaults.
internal sealed class RoutedMetadataCompletionPolicy
{
    private readonly FrameworkMetadataCompleter _completer = new();
    private readonly RoutedSkillMetadataCompleter _skills = new();

    internal RoutedMetadataEligibility Evaluate(RoutedMetadataFileFacts file, RoutedMetadataPolicyContext context)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(context);
        if (!file.IsSelected) return RoutedMetadataEligibility.NotSelected;
        var path = file.CanonicalPath;
        if (!SourceFormClassifier.TryClassify(context.Layout, path, out var form)
            || form is SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion)
            return RoutedMetadataEligibility.UnsupportedForm;
        if (!file.IsReadable) return RoutedMetadataEligibility.Unreadable;
        if (!file.IsSafeOrdinaryLeaf) return RoutedMetadataEligibility.UnsafeLeaf;
        var protection = Protection(path, context);
        if (protection != RoutedMetadataEligibility.Eligible) return protection;
        if (file.IsGeneratedFile || file.HasManagedAuthoredRegions) return RoutedMetadataEligibility.GeneratedContent;
        if (file.LoaderRootEntrypointPath is not { } root
            || !SourceFormClassifier.TryClassify(context.Layout, root, out var rootForm) || !SourceFormClassifier.IsEntrypoint(rootForm)
            || !RoutedMetadataDefaults.IsAncestor(path, SourceLogicalPath.ReadParent(root)))
            return RoutedMetadataEligibility.OutsideRoot;
        if (context.Layout.TryGetMaintenanceBoundary(path, out var boundary) && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside)
            return SourceLogicalPath.ReadParent(root) == boundary.DirectoryPath && context.AvailableOutsideBoundary(boundary) is not null
                ? RoutedMetadataEligibility.Eligible : RoutedMetadataEligibility.OutsideRoot;
        if (!SourceLogicalPath.IsPrimaryCategoryDirectory(context.Roots, SourceLogicalPath.ReadParent(root)))
            return RoutedMetadataEligibility.OutsideRoot;
        return RoutedMetadataEligibility.Eligible;
    }

    internal RoutedMetadataEligibility Protection(string path, RoutedMetadataPolicyContext context)
    {
        if (!SourceFormClassifier.TryClassify(context.Layout, path, out var form) || form == SourceDocumentForm.OverwriteCompanion)
            return RoutedMetadataEligibility.UnsupportedForm;
        if (context.Ownership.HasManagedPath(path)) return RoutedMetadataEligibility.ManagedContent;
        if ((context.Ownership.Framework?.Regions ?? []).Concat(context.Ownership.Extensions.SelectMany(owner => owner.Regions))
                .Any(region => region.Path == path && region.Region != WorkspaceOwnershipDefinitions.EntriesRegion))
            return RoutedMetadataEligibility.GeneratedContent;
        if (WorkspaceRemovals.IsPathRemoved(context.Layout, path, context.Settings)) return RoutedMetadataEligibility.SettingsExcluded;

        var privateRoutes = context.Ownership.Framework?.GitIgnoredRoutes ?? [];
        if (!privateRoutes.IsEmpty && !new SourceSharing(privateRoutes).Includes(path))
            return RoutedMetadataEligibility.PrivateContent;
        if (context.Ownership.Libraries.Any(library => library.Paths.Contains(path, StringComparer.Ordinal)))
            return RoutedMetadataEligibility.LibraryProjection;
        return RoutedMetadataEligibility.Eligible;
    }

    internal RoutedMetadataCompletionResult Complete(RoutedMetadataCompletionInput input, RoutedMetadataPolicyContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        var eligibility = Evaluate(input.File, context);
        var completion = FrameworkMetadataCompletionResult.Unchanged(input.Bytes);
        if (eligibility == RoutedMetadataEligibility.Eligible && SourceFormClassifier.TryClassify(context.Layout, input.File.CanonicalPath, out var form)
            && form == SourceDocumentForm.Skill)
        {
            completion = _skills.Complete(input);
        }
        else if (eligibility == RoutedMetadataEligibility.Eligible)
        {
            completion = _completer.Complete(new()
            {
                CanonicalPath = input.File.CanonicalPath,
                Bytes = input.Bytes,
                Document = input.Document,
                Form = context.Settings.Frontmatter,
                DefaultTags = RoutedMetadataDefaults.Tags(context.Layout, input.File),
            });
        }

        return new() { Eligibility = eligibility, Completion = completion };
    }
}

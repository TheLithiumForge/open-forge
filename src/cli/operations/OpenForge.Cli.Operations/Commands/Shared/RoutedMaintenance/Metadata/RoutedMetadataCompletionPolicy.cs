using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;

// Index, Repair and Doctor share the ordinary eligibility defined by the Forgiving Maintenance design.
// Native Skill policy and mutation orchestration retain their existing owners.
internal sealed class RoutedMetadataCompletionPolicy
{
    private readonly FrameworkMetadataCompleter _completer = new();

    internal RoutedMetadataEligibility Evaluate(RoutedMetadataFileFacts file, RoutedMetadataPolicyContext context)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(context);
        if (!file.IsSelected) return RoutedMetadataEligibility.NotSelected;
        var path = file.CanonicalPath;
        if (!SourceFormClassifier.TryClassify(path, out var form)
            || form is SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion)
            return RoutedMetadataEligibility.UnsupportedForm;
        if (!file.IsReadable) return RoutedMetadataEligibility.Unreadable;
        if (!file.IsSafeOrdinaryLeaf) return RoutedMetadataEligibility.UnsafeLeaf;
        var protection = Protection(path, context);
        if (protection != RoutedMetadataEligibility.Eligible) return protection;
        if (file.IsGeneratedFile || file.HasManagedAuthoredRegions) return RoutedMetadataEligibility.GeneratedContent;
        if (file.LoaderRootEntrypointPath is not { } root
            || !SourceFormClassifier.TryClassify(root, out var rootForm) || !SourceFormClassifier.IsEntrypoint(rootForm)
            || SourceLogicalPath.ReadParent(SourceLogicalPath.ReadParent(root)) != SourceLogicalPath.AgentsRoot
            || !RoutedMetadataDefaults.IsAncestor(path, SourceLogicalPath.ReadParent(root)))
            return RoutedMetadataEligibility.OutsideRoot;
        return RoutedMetadataEligibility.Eligible;
    }

    internal RoutedMetadataEligibility Protection(string path, RoutedMetadataPolicyContext context)
    {
        if (!SourceFormClassifier.TryClassify(path, out var form) || form == SourceDocumentForm.OverwriteCompanion)
            return RoutedMetadataEligibility.UnsupportedForm;
        if (form == SourceDocumentForm.Skill
            || context.NativeSkillPaths.Any(skill => RoutedMetadataDefaults.IsAncestor(path, SourceLogicalPath.ReadParent(skill))))
            return RoutedMetadataEligibility.NativePackage;
        if (context.Ownership.ManagedPaths().Any(claim => claim.Path == path)) return RoutedMetadataEligibility.ManagedContent;
        if ((context.Ownership.Framework?.Regions ?? []).Concat(context.Ownership.Extensions.SelectMany(owner => owner.Regions))
                .Any(region => region.Path == path && region.Region != WorkspaceOwnershipDefinitions.EntriesRegion))
            return RoutedMetadataEligibility.GeneratedContent;
        if (WorkspaceRemovals.IsPathRemoved(path, context.Settings)) return RoutedMetadataEligibility.SettingsExcluded;

        var sharing = new SourceSharing(context.Ownership.Framework?.GitIgnoredRoutes ?? []);
        if (!sharing.Includes(path))
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
        if (eligibility == RoutedMetadataEligibility.Eligible)
        {
            completion = _completer.Complete(new()
            {
                CanonicalPath = input.File.CanonicalPath,
                Bytes = input.Bytes,
                Form = context.Settings.Frontmatter,
                DefaultTags = RoutedMetadataDefaults.Tags(input.File),
            });
        }

        return new() { Eligibility = eligibility, Completion = completion };
    }
}

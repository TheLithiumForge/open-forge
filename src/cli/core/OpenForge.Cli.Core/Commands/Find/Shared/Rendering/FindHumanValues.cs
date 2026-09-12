using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindHumanValues
{
    internal static string Workspace(CliWorkspace? workspace)
        => workspace is null
            ? "none"
            : workspace.LexicalRoot;

    internal static string SelectedBy(CliWorkspace? workspace)
        => workspace?.SelectedBy switch
        {
            null => "none",
            CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
            _ => throw new ArgumentOutOfRangeException(
                nameof(workspace),
                workspace.SelectedBy,
                "The Find workspace selection method is not defined."),
        };

    internal static string Requirement(FindRequirement requirement)
        => requirement switch
        {
            FindRequirement.All => FindDefinitions.All,
            FindRequirement.Any => FindDefinitions.Any,
            _ => throw new ArgumentOutOfRangeException(
                nameof(requirement),
                requirement,
                "The Find requirement is not defined."),
        };

    internal static string Regions(IEnumerable<FindRegion> regions)
    {
        var values = regions
            .Select(region => FindTextEscaping.Escape(region.CanonicalValue))
            .ToArray();
        return values.Length == 0 ? "omitted" : string.Join(", ", values);
    }

    internal static string Count(int? count)
        => count?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "not established";

    internal static string Status(CliSemanticStatus status)
        => CliStatusDefinitions.Read(status).MachineName;

    internal static string Coverage(FindCoverageState state)
        => state switch
        {
            FindCoverageState.NotStarted => "not-started",
            FindCoverageState.Complete => "complete",
            FindCoverageState.Incomplete => "incomplete",
            FindCoverageState.Blocked => "blocked",
            FindCoverageState.Failed => "failed",
            FindCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find coverage state is not defined."),
        };

    internal static string ProjectionCoverage(FindProjectionCoverageState state)
        => state switch
        {
            FindProjectionCoverageState.NotRequested => "not-requested",
            FindProjectionCoverageState.NotStarted => "not-started",
            FindProjectionCoverageState.Complete => "complete",
            FindProjectionCoverageState.Incomplete => "incomplete",
            FindProjectionCoverageState.Blocked => "blocked",
            FindProjectionCoverageState.Failed => "failed",
            FindProjectionCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find projection coverage state is not defined."),
        };

    internal static string UniverseMode(FindUniverseMode mode)
        => mode switch
        {
            FindUniverseMode.Default => "default",
            FindUniverseMode.Filtered => "filtered",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Find universe mode is not defined."),
        };

    internal static string PredicateKind(FindPredicateKind kind)
        => kind switch
        {
            FindPredicateKind.Tag => "Tag:",
            FindPredicateKind.Heading => "Heading:",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find predicate kind is not defined."),
        };

    internal static string SelectorForm(SourceReferenceKind? form)
        => form switch
        {
            null => "none",
            SourceReferenceKind.SourceId => "id",
            SourceReferenceKind.SourcePath => "path",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The Find selector form is not defined."),
        };

    internal static string SelectorResolution(FindSelectorResolution resolution)
        => resolution switch
        {
            FindSelectorResolution.Resolved => "resolved",
            FindSelectorResolution.Invalid => "invalid",
            FindSelectorResolution.Unknown => "unknown",
            FindSelectorResolution.Unsupported => "unsupported",
            FindSelectorResolution.Ambiguous => "ambiguous",
            FindSelectorResolution.Unsafe => "unsafe",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution,
                "The Find selector resolution is not defined."),
        };

    internal static string SourceKind(FindSourceKind? kind)
        => kind switch
        {
            null => "none",
            FindSourceKind.Loader => "loader",
            FindSourceKind.Entrypoint => "entrypoint",
            FindSourceKind.Skill => "skill",
            FindSourceKind.Ordinary => "ordinary",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find source kind is not defined."),
        };

    internal static string SelectorExpansion(FindSelectorExpansion? expansion)
        => expansion switch
        {
            null => "none",
            FindSelectorExpansion.Folder => "folder",
            FindSelectorExpansion.Source => "source",
            _ => throw new ArgumentOutOfRangeException(
                nameof(expansion),
                expansion,
                "The Find selector expansion is not defined."),
        };

    internal static string Identity(FindSourceIdentity? identity)
        => identity is null
            ? "none"
            : $"{FindTextEscaping.Escape(identity.Id)} -> {FindTextEscaping.Escape(identity.Path)}";

    internal static string Candidates(IEnumerable<FindSourceIdentity> candidates)
    {
        var values = candidates
            .Select(candidate => Identity(candidate))
            .ToArray();
        return values.Length == 0 ? "none" : $"[{string.Join(", ", values)}]";
    }

    internal static string SelectorRole(FindSelectorRole? role)
        => role switch
        {
            null => "none",
            FindSelectorRole.Include => "include",
            FindSelectorRole.Exclude => "exclude",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The Find selector role is not defined."),
        };

    internal static string Number(int? value)
        => value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none";

    internal static string Layer(SourceLayerKind? layer)
        => layer switch
        {
            null => "none",
            SourceLayerKind.Base => "base",
            SourceLayerKind.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    internal static string Region(FindRegion? region)
        => region is null ? "none" : FindTextEscaping.Escape(region.CanonicalValue);

    internal static string Optional(string? value)
        => value is null ? "none" : FindTextEscaping.Escape(value);

}

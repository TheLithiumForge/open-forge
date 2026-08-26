using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextHumanRenderingSupport
{
    internal static bool IsPathsOnly(ContextResult result)
        => result.Presentation.Content.Effective is [{ Kind: ContextContentPartKind.Paths }];

    internal static bool HasAuthoredText(ContextProjection projection)
        => projection.Text is not null;

    internal static string ProjectionName(ContextProjection projection)
        => projection.Part == ContextProjectionPart.Section
            ? $"Section {projection.Name}"
            : projection.Part.ToString();

    internal static string ProjectionState(ContextProjectionState state)
        => state.ToString().ToLowerInvariant();

    internal static string Reason(ContextInclusionReason reason)
    {
        var value = reason.Kind switch
        {
            ContextInclusionReasonKind.WorkspaceEntry => "workspace entry",
            ContextInclusionReasonKind.Loader => "Loader",
            ContextInclusionReasonKind.LoadNow => "#LoadNow",
            ContextInclusionReasonKind.KeepInMind => "#KeepInMind",
            ContextInclusionReasonKind.AncestorRequired => "ancestor required",
            ContextInclusionReasonKind.SelectedSource => "selected source",
            ContextInclusionReasonKind.ScopeLocal => "scope-local loading",
            ContextInclusionReasonKind.LinkedSource => "linked source",
            ContextInclusionReasonKind.OverwriteCompanion => "overwrite companion",
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason.Kind, "The inclusion reason is not defined."),
        };
        if (reason.Source is not null)
        {
            value = $"{value} from {reason.Source.Path}";
        }

        if (reason.Reference is not null)
        {
            value = $"{value} for {reason.Reference}";
        }

        if (reason.Depth is { } depth)
        {
            value = string.Create(CultureInfo.InvariantCulture, $"{value} at depth {depth}");
        }

        return value;
    }

    internal static string ReadCount(ContextResult result)
        => result.Selection.SourceCount is { } count
            ? string.Create(CultureInfo.InvariantCulture, $"{count}")
            : "unknown";

    internal static string LinkExpansion(ContextLinkExpansion expansion)
        => expansion.Mode switch
        {
            ContextLinkExpansionMode.None => "none",
            ContextLinkExpansionMode.All => "all",
            ContextLinkExpansionMode.Bounded when expansion.Depth is { } depth
                => string.Create(CultureInfo.InvariantCulture, $"{depth}"),
            ContextLinkExpansionMode.Bounded
                => throw new InvalidOperationException("A bounded link expansion requires its depth."),
            _ => throw new ArgumentOutOfRangeException(nameof(expansion), expansion.Mode, "The link expansion mode is not defined."),
        };

    internal static string SourceResolution(SourceReferenceResolutionState state)
        => state.ToString().ToLowerInvariant();

    internal static string Layer(ContextSourceLayerKind layer)
        => layer == ContextSourceLayerKind.Base ? "base" : "overwrite";

    internal static string Coverage(ContextCoverageState coverage)
        => coverage.ToString().ToLowerInvariant().Replace("notstarted", "not-started", StringComparison.Ordinal);

    internal static string OptionalCoverage(ContextOptionalCoverageState coverage)
        => coverage.ToString().ToLowerInvariant()
            .Replace("notrequested", "not-requested", StringComparison.Ordinal)
            .Replace("notstarted", "not-started", StringComparison.Ordinal);

    internal static string LinkDisposition(ContextLinkDisposition disposition)
        => disposition.ToString().ToLowerInvariant()
            .Replace("alreadyselected", "already-selected", StringComparison.Ordinal)
            .Replace("externalunchecked", "external-unchecked", StringComparison.Ordinal);

    internal static string LinkResolution(ContextLinkResolution resolution)
        => resolution.ToString().ToLowerInvariant()
            .Replace("fragmentmissing", "fragment-missing", StringComparison.Ordinal)
            .Replace("casemismatch", "case-mismatch", StringComparison.Ordinal)
            .Replace("encodingunsupported", "encoding-unsupported", StringComparison.Ordinal)
            .Replace("outsideworkspace", "outside-workspace", StringComparison.Ordinal)
            .Replace("physicalescape", "physical-escape", StringComparison.Ordinal)
            .Replace("externalunchecked", "external-unchecked", StringComparison.Ordinal);

    internal static void EnsureLineBoundary(StringBuilder builder)
    {
        if (builder.Length != 0 && builder[^1] != '\n')
        {
            builder.AppendLine();
        }
    }

    internal static void EnsureBlankFramingLine(StringBuilder builder)
    {
        EnsureLineBoundary(builder);
        if (builder.Length != 0 && (builder.Length < 2 || builder[^2] != '\n'))
        {
            builder.AppendLine();
        }
    }
}

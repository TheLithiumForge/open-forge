using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Find;

internal static class FindDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "find";

    internal const string All = "all";
    internal const string Any = "any";
    internal const string Document = "document";
    internal const string Frontmatter = "frontmatter";
    internal const string Body = "body";
    internal const string Metadata = "metadata";
    internal const string Headings = "headings";
    internal const string SectionPrefix = "section:";

    internal static readonly CliSyntaxDefinition FindCommand = new(
        CommandIdentity,
        "Find Markdown sources by tags and headings.");

    internal static readonly CliOptionDefinition<string[]> Include = new(
        "--include",
        "Include one source in the search; repeat to include more.",
        CliOptionArity.ExactlyOne,
        [],
        "source-reference");

    internal static readonly CliOptionDefinition<string[]> Exclude = new(
        "--exclude",
        "Exclude one source from the search; repeat to exclude more.",
        CliOptionArity.ExactlyOne,
        [],
        "source-reference");

    internal static readonly CliOptionDefinition<string[]> Tag = new(
        "--tag",
        "Match one authored tag value.",
        CliOptionArity.ExactlyOne,
        [],
        "tag");

    internal static readonly CliOptionDefinition<string[]> Heading = new(
        "--heading",
        "Match one complete Markdown heading.",
        CliOptionArity.ExactlyOne,
        [],
        "heading");

    internal static readonly CliOptionDefinition<string?> Require = new(
        "--require",
        "Require all or any of the supplied tag and heading matches.",
        CliOptionArity.ExactlyOne,
        null,
        "all|any",
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [All] = All,
            [Any] = Any,
        });

    internal static readonly CliOptionDefinition<string?> Within = new(
        "--within",
        "Choose where to match tags and headings.",
        CliOptionArity.ExactlyOne,
        null,
        "part[,part...]");

    internal static readonly CliOptionDefinition<string?> Content = new(
        "--content",
        "Choose which parts of matched sources to show.",
        CliOptionArity.ExactlyOne,
        null,
        "part[,part...]");

    internal static string ReadFindingCode(FindFindingCode code)
    {
        return code switch
        {
            FindFindingCode.InvalidInput => "find.invalid-input",
            FindFindingCode.InvalidSelector => "find.invalid-selector",
            FindFindingCode.WorkspaceUnavailable => "find.workspace-unavailable",
            FindFindingCode.WorkspaceUnsafe => "find.workspace-unsafe",
            FindFindingCode.SelectorAmbiguous => "find.selector-ambiguous",
            FindFindingCode.SelectorUnsafe => "find.selector-unsafe",
            FindFindingCode.IdentityCollision => "find.identity-collision",
            FindFindingCode.CandidateUnsafe => "find.candidate-unsafe",
            FindFindingCode.LayerUnresolved => "find.layer-unresolved",
            FindFindingCode.InspectionUnavailable => "find.inspection-unavailable",
            FindFindingCode.InvalidEncoding => "find.invalid-encoding",
            FindFindingCode.FrontmatterUnavailable => "find.frontmatter-unavailable",
            FindFindingCode.SectionAmbiguous => "find.section-ambiguous",
            FindFindingCode.ProjectionMissing => "find.projection-missing",
            FindFindingCode.ProjectionUnavailable => "find.projection-unavailable",
            FindFindingCode.OperationFailed => "find.operation-failed",
            FindFindingCode.Interrupted => "find.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Find finding code is not defined."),
        };
    }

    internal static CliSemanticStatus ReadFindingStatus(FindFindingCode code)
    {
        return code switch
        {
            FindFindingCode.InvalidInput
                or FindFindingCode.InvalidSelector => CliSemanticStatus.Invalid,
            FindFindingCode.WorkspaceUnavailable
                or FindFindingCode.WorkspaceUnsafe
                or FindFindingCode.SelectorAmbiguous
                or FindFindingCode.SelectorUnsafe => CliSemanticStatus.Blocked,
            FindFindingCode.IdentityCollision
                or FindFindingCode.ProjectionMissing => CliSemanticStatus.Attention,
            FindFindingCode.CandidateUnsafe
                or FindFindingCode.LayerUnresolved
                or FindFindingCode.InspectionUnavailable
                or FindFindingCode.InvalidEncoding
                or FindFindingCode.FrontmatterUnavailable
                or FindFindingCode.SectionAmbiguous
                or FindFindingCode.ProjectionUnavailable => CliSemanticStatus.Incomplete,
            FindFindingCode.OperationFailed => CliSemanticStatus.Failed,
            FindFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Find finding code is not defined."),
        };
    }

    internal static CliNextAction IncompleteNextAction { get; } = new(
        CommandLines.Doctor,
        "Inspect the unavailable source or projection facts before relying on this Find result.");

    internal static CliNextAction InvalidNextAction { get; } = new(
        "open-forge find --help",
        "Correct the named Find input, then rerun the request.");

    internal static CliNextAction SelectorAmbiguousNextAction { get; } = new(
        CommandLines.Find,
        "Replace every ambiguous selector with one listed exact path, then rerun the same request.");

    internal static CliNextAction BlockedNextAction { get; } = new(
        CommandLines.Doctor,
        "Inspect the blocked workspace or source boundary before rerunning Find.");

    internal static CliNextAction FailedNextAction { get; } = new(
        "open-forge find --detail debug",
        "Report the failure and retry the same request with bounded diagnostics.");

    internal static CliNextAction InterruptedNextAction { get; } = new(
        CommandLines.Find,
        "Rerun the same Find request.");
}

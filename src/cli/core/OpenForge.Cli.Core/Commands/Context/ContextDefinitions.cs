using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Context;

internal static class ContextDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "context";
    internal const string Metadata = "metadata";
    internal const string Paths = "paths";
    internal const string Frontmatter = "frontmatter";
    internal const string Headings = "headings";
    internal const string Body = "body";
    internal const string SectionPrefix = "section:";
    internal const string All = "all";
    internal const string SourcesName = "source-reference";
    internal const string SourcesDescription = "Zero or more source IDs or exact .agents paths.";

    internal static readonly CliSyntaxDefinition ContextCommand = new(
        name: CommandIdentity,
        description: "Return ordered startup and selected Open Forge context.");

    internal static readonly CliOptionDefinition<bool> AdditionsOnly = new(
        name: "--additions-only",
        description: "Return only sources added beyond the startup-required closure.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<string[]> Content = new(
        name: "--content",
        description: "Project one comma-separated content selection.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: [],
        valueName: "part[,part...]");

    internal static readonly CliOptionDefinition<string[]> FollowLinks = new(
        name: "--follow-links",
        description: "Follow contained local Markdown links to a positive depth or all.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: [],
        valueName: "positive-depth|all");

    internal static ContextFindingDefinition Read(ContextFindingCode code)
        => code switch
        {
            ContextFindingCode.InvalidInput => new(Code: "context.invalid-input", Status: CliSemanticStatus.Invalid),
            ContextFindingCode.InvalidSource => new(Code: "context.invalid-source", Status: CliSemanticStatus.Invalid),
            ContextFindingCode.InvalidContent => new(Code: "context.invalid-content", Status: CliSemanticStatus.Invalid),
            ContextFindingCode.InvalidLinkDepth => new(Code: "context.invalid-link-depth", Status: CliSemanticStatus.Invalid),
            ContextFindingCode.WorkspaceUnavailable => new(Code: "context.workspace-unavailable", Status: CliSemanticStatus.Blocked),
            ContextFindingCode.WorkspaceUnsafe => new(Code: "context.workspace-unsafe", Status: CliSemanticStatus.Blocked),
            ContextFindingCode.SourceAmbiguous => new(Code: "context.source-ambiguous", Status: CliSemanticStatus.Blocked),
            ContextFindingCode.SourceUnsafe => new(Code: "context.source-unsafe", Status: CliSemanticStatus.Blocked),
            ContextFindingCode.OverwriteAmbiguous => new(Code: "context.overwrite-ambiguous", Status: CliSemanticStatus.Blocked),
            ContextFindingCode.TargetAmbiguous => new(Code: "context.target-ambiguous", Status: CliSemanticStatus.Blocked),
            ContextFindingCode.TargetUnsafe => new(Code: "context.target-unsafe", Status: CliSemanticStatus.Blocked),
            ContextFindingCode.ClosureUnavailable => new(Code: "context.closure-unavailable", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.LayerUnavailable => new(Code: "context.layer-unavailable", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.InvalidEncoding => new(Code: "context.invalid-encoding", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.MarkdownUnavailable => new(Code: "context.markdown-unavailable", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.TargetMissing => new(Code: "context.target-missing", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.FragmentMissing => new(Code: "context.fragment-missing", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.LinkEncodingInvalid => new(Code: "context.link-encoding-invalid", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.TargetUnreadable => new(Code: "context.target-unreadable", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.SectionAmbiguous => new(Code: "context.section-ambiguous", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.ProjectionUnavailable => new(Code: "context.projection-unavailable", Status: CliSemanticStatus.Incomplete),
            ContextFindingCode.IdentityCollision => new(Code: "context.identity-collision", Status: CliSemanticStatus.Attention),
            ContextFindingCode.TargetCaseMismatch => new(Code: "context.target-case-mismatch", Status: CliSemanticStatus.Attention),
            ContextFindingCode.FrontmatterMissing => new(Code: "context.frontmatter-missing", Status: CliSemanticStatus.Attention),
            ContextFindingCode.SectionMissing => new(Code: "context.section-missing", Status: CliSemanticStatus.Attention),
            ContextFindingCode.OperationFailed => new(Code: "context.operation-failed", Status: CliSemanticStatus.Failed),
            ContextFindingCode.Interrupted => new(Code: "context.interrupted", Status: CliSemanticStatus.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Context finding code is not defined."),
        };

    internal static CliNextAction InvalidNextAction { get; } = new(
        command: "open-forge context --help",
        reason: "Correct the named Context input, then rerun the request.");

    internal static CliNextAction SourceAmbiguousNextAction { get; } = new(
        command: "open-forge context",
        reason: "Replace every ambiguous source reference with one listed exact path, then rerun the same request.");

    internal static CliNextAction BlockedNextAction { get; } = new(
        command: "open-forge doctor",
        reason: "Inspect the blocked workspace, source, overwrite, or link-target boundary before rerunning Context.");

    internal static CliNextAction IncompleteNextAction { get; } = new(
        command: "open-forge doctor",
        reason: "Inspect the unavailable closure, source, link, or projection facts before relying on this Context result.");

    internal static CliNextAction FailedNextAction { get; } = new(
        command: "open-forge context --verbose",
        reason: "Report the failure and retry the same Context request with bounded diagnostics.");

    internal static CliNextAction InterruptedNextAction { get; } = new(
        command: "open-forge context",
        reason: "Rerun the same Context request.");
}

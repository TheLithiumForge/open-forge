using System.Collections.Frozen;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal enum RouteListDepthKind
{
    Finite,
    All,
}

internal sealed record RouteListDepth
{
    private RouteListDepth(RouteListDepthKind kind, int? value)
    {
        Kind = kind;
        Value = value;
    }

    internal RouteListDepthKind Kind { get; }

    internal int? Value { get; }

    internal int FiniteValue
    {
        get
        {
            if (Kind != RouteListDepthKind.Finite
                || Value is not { } finiteValue)
            {
                throw new InvalidOperationException(
                    "A finite route-list depth requires a finite value.");
            }

            return finiteValue;
        }
    }

    internal string MachineValue => Kind switch
    {
        RouteListDepthKind.Finite => FiniteValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        RouteListDepthKind.All => RouteListDefinitions.AllDepth,
        _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "The route-list depth kind is not defined."),
    };

    internal static RouteListDepth Finite(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Route-list depth cannot be negative.");
        }

        return new RouteListDepth(RouteListDepthKind.Finite, value);
    }

    internal static RouteListDepth All { get; } = new(RouteListDepthKind.All, null);

    internal static RouteListDepth Default { get; } = Finite(1);
}

internal enum RouteListFindingCode
{
    InvalidDepth,
    InvalidSourceReference,
    InvalidWorkspace,
    WorkspaceUnavailable,
    UnknownSource,
    UnsupportedSource,
    LoaderSubject,
    AmbiguousSource,
    UnsafeSource,
    LoaderUnavailable,
    LoaderMalformed,
    RouteAmbiguous,
    MetadataMissing,
    MetadataMalformed,
    AuthoredForm,
    IdentityCollision,
    ReadUnavailable,
    PhysicalBoundary,
    OperationFailed,
    Interrupted,
}

internal sealed record RouteListFindingResultStatusRule(
    CliSemanticStatus ResultStatus,
    CliSemanticStatus? RequiredFindingStatus,
    FrozenSet<CliSemanticStatus> AllowedFindingStatuses);

internal static class RouteListDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route list";
    internal const string AllDepth = "all";

    private static readonly FrozenDictionary<CliSemanticStatus, RouteListFindingResultStatusRule>
        FindingResultStatusRules = new[]
        {
            CreateFindingResultStatusRule(CliSemanticStatus.Complete, null),
            CreateFindingResultStatusRule(CliSemanticStatus.Attention, CliSemanticStatus.Attention, CliSemanticStatus.Attention),
            CreateFindingResultStatusRule(
                CliSemanticStatus.Incomplete, CliSemanticStatus.Incomplete, CliSemanticStatus.Attention, CliSemanticStatus.Incomplete),
            CreateFindingResultStatusRule(CliSemanticStatus.Invalid, CliSemanticStatus.Invalid, CliSemanticStatus.Invalid),
            CreateFindingResultStatusRule(
                CliSemanticStatus.Blocked, CliSemanticStatus.Blocked,
                CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Blocked),
            CreateFindingResultStatusRule(
                CliSemanticStatus.Failed,
                CliSemanticStatus.Failed,
                CliSemanticStatus.Attention,
                CliSemanticStatus.Incomplete,
                CliSemanticStatus.Blocked,
                CliSemanticStatus.Failed),
            CreateFindingResultStatusRule(
                CliSemanticStatus.Interrupted,
                CliSemanticStatus.Interrupted,
                CliSemanticStatus.Attention,
                CliSemanticStatus.Incomplete,
                CliSemanticStatus.Blocked,
                CliSemanticStatus.Interrupted),
        }.ToFrozenDictionary(rule => rule.ResultStatus);

    internal static readonly CliSyntaxDefinition ListCommand = new(
        "list",
        "List routed sources and descendants at a structural depth.");

    internal static readonly CliSyntaxDefinition SourceReference = new(
        "source-reference",
        "Select one routed entrypoint or routed leaf by exact source ID or .agents path.");

    internal static readonly CliOptionDefinition<string> Depth = new(
        "--depth",
        "Include routed descendants through a non-negative structural depth or all.",
        CliOptionArity.ExactlyOne,
        RouteListDepth.Default.MachineValue,
        "non-negative-integer|all");

    internal static string ReadFindingCode(RouteListFindingCode code)
    {
        return code switch
        {
            RouteListFindingCode.InvalidDepth => "route-list.invalid-depth",
            RouteListFindingCode.InvalidSourceReference => "route-list.invalid-source-reference",
            RouteListFindingCode.InvalidWorkspace => "route-list.invalid-workspace",
            RouteListFindingCode.WorkspaceUnavailable => "route-list.workspace-unavailable",
            RouteListFindingCode.UnknownSource => "route-list.unknown-source",
            RouteListFindingCode.UnsupportedSource => "route-list.unsupported-source",
            RouteListFindingCode.LoaderSubject => "route-list.loader-subject",
            RouteListFindingCode.AmbiguousSource => "route-list.ambiguous-source",
            RouteListFindingCode.UnsafeSource => "route-list.unsafe-source",
            RouteListFindingCode.LoaderUnavailable => "route-list.loader-unavailable",
            RouteListFindingCode.LoaderMalformed => "route-list.loader-malformed",
            RouteListFindingCode.RouteAmbiguous => "route-list.route-ambiguous",
            RouteListFindingCode.MetadataMissing => "route-list.metadata-missing",
            RouteListFindingCode.MetadataMalformed => "route-list.metadata-malformed",
            RouteListFindingCode.AuthoredForm => "route-list.authored-form",
            RouteListFindingCode.IdentityCollision => "route-list.identity-collision",
            RouteListFindingCode.ReadUnavailable => "route-list.read-unavailable",
            RouteListFindingCode.PhysicalBoundary => "route-list.physical-boundary",
            RouteListFindingCode.OperationFailed => "route-list.operation-failed",
            RouteListFindingCode.Interrupted => "route-list.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The route-list finding code is not defined."),
        };
    }

    internal static RouteListFindingResultStatusRule ReadFindingResultStatusRule(CliSemanticStatus status)
    {
        _ = CliStatusDefinitions.Read(status);
        return FindingResultStatusRules[status];
    }

    internal static bool IsFindingStatusAllowed(RouteListFindingCode code, CliSemanticStatus status)
    {
        _ = ReadFindingCode(code);
        _ = CliStatusDefinitions.Read(status);
        return code switch
        {
            RouteListFindingCode.InvalidDepth
                or RouteListFindingCode.InvalidSourceReference
                or RouteListFindingCode.InvalidWorkspace
                or RouteListFindingCode.UnknownSource
                or RouteListFindingCode.UnsupportedSource
                or RouteListFindingCode.LoaderSubject => status == CliSemanticStatus.Invalid,
            RouteListFindingCode.WorkspaceUnavailable
                or RouteListFindingCode.AmbiguousSource
                or RouteListFindingCode.UnsafeSource
                or RouteListFindingCode.RouteAmbiguous
                or RouteListFindingCode.PhysicalBoundary => status == CliSemanticStatus.Blocked,
            RouteListFindingCode.LoaderUnavailable =>
                status is CliSemanticStatus.Incomplete or CliSemanticStatus.Blocked,
            RouteListFindingCode.LoaderMalformed
                or RouteListFindingCode.MetadataMissing
                or RouteListFindingCode.MetadataMalformed => status == CliSemanticStatus.Incomplete,
            RouteListFindingCode.AuthoredForm => status == CliSemanticStatus.Attention,
            RouteListFindingCode.IdentityCollision =>
                status is CliSemanticStatus.Attention or CliSemanticStatus.Blocked,
            RouteListFindingCode.ReadUnavailable =>
                status is CliSemanticStatus.Incomplete or CliSemanticStatus.Failed,
            RouteListFindingCode.OperationFailed => status == CliSemanticStatus.Failed,
            RouteListFindingCode.Interrupted => status == CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The route-list finding code is not defined."),
        };
    }

    private static RouteListFindingResultStatusRule CreateFindingResultStatusRule(
        CliSemanticStatus resultStatus,
        CliSemanticStatus? requiredFindingStatus,
        params CliSemanticStatus[] allowedFindingStatuses)
    {
        return new(resultStatus, requiredFindingStatus, allowedFindingStatuses.ToFrozenSet());
    }
}

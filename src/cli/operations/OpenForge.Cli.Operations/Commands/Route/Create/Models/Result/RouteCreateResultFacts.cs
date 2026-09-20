using System.Collections.Immutable;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Result;

internal sealed record RouteCreateTarget
{
    public required string Requested { get; init; }

    public string? Id { get; init; }

    public string? Path { get; init; }
}

internal sealed record RouteCreateParent
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required RouteCreateParentForm Form { get; init; }
}

internal sealed record RouteCreateMetadata
{
    public required string? Description { get; init; }

    public string? Responsibility { get; init; }

    public required ImmutableArray<string> Tags { get; init; }
}

internal sealed record RouteCreateTemplate
{
    public required string Requested { get; init; }

    public required string Id { get; init; }

    public required string Path { get; init; }

    public required RouteCreateTemplateClassification Classification { get; init; }

    public required long BodyByteLength { get; init; }
}

internal sealed record RouteCreateSection
{
    public required string Path { get; init; }

    public required string Before { get; init; }

    public required string After { get; init; }
}

internal sealed record RouteCreatePlanFacts
{
    public required RouteCreatePlanCompleteness Completeness { get; init; }

    public required RouteCreatePlanSafety Safety { get; init; }
}

internal sealed record RouteCreateEffectChange
{
    public string? Before { get; init; }

    public string? Expected { get; init; }
}

internal sealed record RouteCreateEffect
{
    public required string Path { get; init; }

    public required RouteCreateEffectKind Kind { get; init; }

    public required RouteCreateEffectAction Action { get; init; }

    public RouteCreateEffectChange? Change { get; init; }

    public required RouteCreateEffectOutcome Outcome { get; init; }

    public required RouteCreateEffectResidual Residual { get; init; }
}

internal sealed record RouteCreateRecovery
{
    public required RouteCreateRecoveryState State { get; init; }

    public string? ResidualPath { get; init; }
}

internal sealed class RouteCreateFinding
{
    private const int MaximumCauseLength = 256;

    internal RouteCreateFinding(
        RouteCreateFindingCode code,
        string cause,
        string? target = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException(
                "A Route Create finding target cannot be empty.",
                nameof(target));
        }

        RouteCreateDefinitions.ReadStatus(code);
        Code = code;
        Target = target;
        Cause = cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    internal RouteCreateFindingCode Code { get; }

    internal CliSemanticStatus Status => RouteCreateDefinitions.ReadStatus(Code);

    internal string? Target { get; }

    internal string Cause { get; }
}

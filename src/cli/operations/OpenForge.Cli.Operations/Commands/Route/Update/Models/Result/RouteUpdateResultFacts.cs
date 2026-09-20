using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

internal sealed record RouteUpdateTarget
{
    public required string Requested { get; init; }

    public RouteUpdateTargetSelection? SelectedBy { get; init; }

    public string? Id { get; init; }

    public string? Path { get; init; }

    public RouteUpdateTargetForm? Form { get; init; }

    public required ImmutableArray<string> OverwritePaths { get; init; }

    internal static RouteUpdateTarget Unresolved(string requested)
        => new()
        {
            Requested = requested,
            SelectedBy = null,
            Id = null,
            Path = null,
            Form = null,
            OverwritePaths = [],
        };
}

internal sealed record RouteUpdateDescriptionPatch
{
    public required bool Requested { get; init; }

    public string? Before { get; init; }

    public string? Expected { get; init; }

    public required RouteUpdatePatchState State { get; init; }
}

internal sealed record RouteUpdateResponsibilityPatch
{
    public required bool Requested { get; init; }

    public required RouteUpdateResponsibilityOperation Operation { get; init; }

    public string? Before { get; init; }

    public string? Expected { get; init; }

    public required RouteUpdatePatchState State { get; init; }
}

internal sealed record RouteUpdateTagsPatch
{
    public required bool Requested { get; init; }

    public ImmutableArray<string>? Before { get; init; }

    public ImmutableArray<string>? Expected { get; init; }

    public required RouteUpdatePatchState State { get; init; }
}

internal sealed record RouteUpdatePatch
{
    public required RouteUpdateDescriptionPatch Description { get; init; }

    public required RouteUpdateResponsibilityPatch Responsibility { get; init; }

    public required RouteUpdateTagsPatch Tags { get; init; }

    internal static RouteUpdatePatch Unresolved(RouteUpdatePatchRequest request)
        => new()
        {
            Description = new RouteUpdateDescriptionPatch
            {
                Requested = request.Description.Requested,
                Before = null,
                Expected = request.Description.Value,
                State = request.Description.Requested
                    ? RouteUpdatePatchState.Unresolved
                    : RouteUpdatePatchState.NotRequested,
            },
            Responsibility = new RouteUpdateResponsibilityPatch
            {
                Requested = request.Responsibility.Operation
                    != RouteUpdateResponsibilityOperation.NotRequested,
                Operation = request.Responsibility.Operation,
                Before = null,
                Expected = request.Responsibility.Value,
                State = request.Responsibility.Operation
                    != RouteUpdateResponsibilityOperation.NotRequested
                        ? RouteUpdatePatchState.Unresolved
                        : RouteUpdatePatchState.NotRequested,
            },
            Tags = new RouteUpdateTagsPatch
            {
                Requested = request.Tags.Requested,
                Before = null,
                Expected = request.Tags.Requested ? request.Tags.Values : null,
                State = request.Tags.Requested
                    ? RouteUpdatePatchState.Unresolved
                    : RouteUpdatePatchState.NotRequested,
            },
        };
}

internal sealed record RouteUpdateTemplate
{
    public required string Requested { get; init; }

    public string? Id { get; init; }

    public string? Path { get; init; }

    public RouteUpdateTemplateClassification? Classification { get; init; }

    public long? BodyByteLength { get; init; }

    public required RouteUpdateTemplateDecision Decision { get; init; }

    internal static RouteUpdateTemplate Unresolved(string requested)
        => new()
        {
            Requested = requested,
            Id = null,
            Path = null,
            Classification = null,
            BodyByteLength = null,
            Decision = RouteUpdateTemplateDecision.Unresolved,
        };
}

internal sealed record RouteUpdatePlanFacts
{
    public required RouteUpdatePlanCompleteness Completeness { get; init; }

    public required RouteUpdatePlanSafety Safety { get; init; }

    public required RouteUpdateBodyState Body { get; init; }
}

internal sealed record RouteUpdateEffectChange
{
    public required string Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed record RouteUpdatePreviewHunk
{
    public required RouteUpdatePreviewKind Kind { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed record RouteUpdateEffect
{
    public required string Path { get; init; }

    public required RouteUpdateEffectKind Kind { get; init; }

    public required RouteUpdateEffectAction Action { get; init; }

    public required RouteUpdateEffectChange Change { get; init; }

    public required ImmutableArray<RouteUpdatePreviewHunk> Preview { get; init; }

    public required RouteUpdateEffectOutcome Outcome { get; init; }

    public required RouteUpdateEffectResidual Residual { get; init; }
}

internal sealed record RouteUpdateRecovery
{
    public required RouteUpdateRecoveryState State { get; init; }

    public string? ResidualPath { get; init; }

    internal static RouteUpdateRecovery NotRequired()
        => new()
        {
            State = RouteUpdateRecoveryState.NotRequired,
            ResidualPath = null,
        };

    internal static RouteUpdateRecovery NotCreated()
        => new()
        {
            State = RouteUpdateRecoveryState.NotCreated,
            ResidualPath = null,
        };

    internal static RouteUpdateRecovery Removed()
        => new()
        {
            State = RouteUpdateRecoveryState.Removed,
            ResidualPath = null,
        };

    internal static RouteUpdateRecovery Retained(string residualPath)
        => new()
        {
            State = RouteUpdateRecoveryState.Retained,
            ResidualPath = residualPath,
        };

    internal static RouteUpdateRecovery Unknown(string? residualPath)
        => new()
        {
            State = RouteUpdateRecoveryState.Unknown,
            ResidualPath = residualPath,
        };
}

internal sealed class RouteUpdateFinding
{
    private const int MaximumCauseLength = 256;

    internal RouteUpdateFinding(
        RouteUpdateFindingCode code,
        string cause,
        string? target = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException(
                "A Route Update finding target cannot be empty.",
                nameof(target));
        }

        RouteUpdateDefinitions.ReadStatus(code);
        Code = code;
        Target = target;
        Cause = cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    internal RouteUpdateFindingCode Code { get; }

    internal CliSemanticStatus Status => RouteUpdateDefinitions.ReadStatus(Code);

    internal string? Target { get; }

    internal string Cause { get; }
}

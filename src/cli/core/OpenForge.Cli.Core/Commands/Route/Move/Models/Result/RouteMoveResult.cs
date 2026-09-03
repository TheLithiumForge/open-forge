using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

internal sealed record RouteMoveResult : ICliCommandResult
{
    internal RouteMoveResult(
        RouteMoveResultFormation formation,
        CliSemanticStatus status,
        CliNextAction? next)
    {
        ArgumentNullException.ThrowIfNull(formation);
        _ = CliStatusDefinitions.Read(status);
        ValidateFormation(formation);
        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Source = formation.Source;
        Destination = formation.Destination;
        Subject = formation.Subject;
        Ownership = formation.Ownership;
        References = formation.References;
        GeneratedNavigation = formation.GeneratedNavigation;
        Plan = formation.Plan;
        Effects = formation.Effects;
        UnchangedPaths = formation.UnchangedPaths;
        Recovery = formation.Recovery;
        Verification = formation.Verification;
        Findings = formation.Findings;
        Status = status;
        Next = next;
    }

    public string Command => RouteMoveDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal RouteMoveMode Mode { get; }

    internal RouteMoveSource Source { get; }

    internal RouteMoveDestination Destination { get; }

    internal RouteMoveSubject Subject { get; }

    internal RouteMoveOwnership Ownership { get; }

    internal RouteMoveReferences References { get; }

    internal RouteMoveGeneratedNavigation GeneratedNavigation { get; }

    internal RouteMovePlanFacts Plan { get; }

    internal ImmutableArray<RouteMoveEffect> Effects { get; }

    internal ImmutableArray<string> UnchangedPaths { get; }

    internal RouteMoveRecovery Recovery { get; }

    internal RouteMoveVerificationState Verification { get; }

    internal ImmutableArray<RouteMoveFinding> Findings { get; }

    private static void ValidateFormation(RouteMoveResultFormation formation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formation.Source.Requested);
        ArgumentException.ThrowIfNullOrWhiteSpace(formation.Destination.Requested);
        Validate(formation.Mode, nameof(formation.Mode));
        ValidateNullable(formation.Source.SelectedBy, nameof(formation.Source.SelectedBy));
        ValidateNullable(formation.Source.Form, nameof(formation.Source.Form));
        ValidateNullable(formation.Subject.Kind, nameof(formation.Subject.Kind));
        Validate(formation.Ownership.State, nameof(formation.Ownership.State));
        Validate(formation.Ownership.Framework, nameof(formation.Ownership.Framework));
        Validate(formation.Ownership.Extensions, nameof(formation.Ownership.Extensions));
        Validate(formation.References.Coverage, nameof(formation.References.Coverage));
        Validate(formation.GeneratedNavigation.Coverage, nameof(formation.GeneratedNavigation.Coverage));
        Validate(formation.Plan.Completeness, nameof(formation.Plan.Completeness));
        Validate(formation.Plan.Safety, nameof(formation.Plan.Safety));
        Validate(formation.Recovery.State, nameof(formation.Recovery.State));
        Validate(formation.Verification, nameof(formation.Verification));
        ValidateReferenceCounts(formation.References);
        ValidateArrays(formation);
    }

    private static void ValidateReferenceCounts(RouteMoveReferences references)
    {
        ArgumentNullException.ThrowIfNull(references);
        if (references.ScannedSourceCount < 0
            || references.InspectedSourceCount < 0
            || references.InspectedSourceCount > references.ScannedSourceCount
            || references.OccurrenceCount < 0)
        {
            throw new ArgumentException(
                "Route Move reference counts are invalid.",
                nameof(references));
        }
    }

    private static void ValidateArrays(RouteMoveResultFormation formation)
    {
        ValidateSubject(formation);
        ValidateOwnershipAndReferences(formation);
        ValidateNavigationAndEffects(formation);
    }

    private static void ValidateSubject(RouteMoveResultFormation formation)
    {
        ValidateItems(formation.Subject.Layers, layer =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(layer.SourcePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(layer.DestinationPath);
            Validate(layer.Layer, nameof(layer.Layer));
        }, "subject layers");
        ValidateUniqueOrdered(
            formation.Subject.Layers,
            layer => layer.SourcePath,
            "subject layer source paths");
        ValidateUniqueOrdered(
            formation.Subject.Layers,
            layer => layer.DestinationPath,
            "subject layer destination paths");
        ValidateItems(formation.Subject.Items, item =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(item.SourcePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(item.DestinationPath);
            Validate(item.Kind, nameof(item.Kind));
            ValidateNullable(item.Layer, nameof(item.Layer));
        }, "subject items");
        ValidateUniqueOrdered(
            formation.Subject.Items,
            item => item.SourcePath,
            "subject item source paths");
        ValidateUniqueOrdered(
            formation.Subject.Items,
            item => item.DestinationPath,
            "subject item destination paths");
        if (formation.Subject.Kind != RouteMoveSubjectKind.Category
            && !formation.Subject.Items.IsEmpty)
        {
            throw new ArgumentException(
                "Route Move subject items are reserved for a complete category inventory.",
                nameof(formation));
        }

    }

    private static void ValidateOwnershipAndReferences(RouteMoveResultFormation formation)
    {
        ValidateItems(formation.Ownership.Claims, claim =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(claim.Path);
            ArgumentException.ThrowIfNullOrWhiteSpace(claim.Owner);
            Validate(claim.Manager, nameof(claim.Manager));
        }, "ownership claims");
        ValidateUniqueOrdered(
            formation.Ownership.Claims,
            claim => $"{claim.Path}\u0000{(int)claim.Manager:D10}\u0000{claim.Owner}",
            "ownership claims");
        ValidateItems(formation.References.Rewrites, rewrite =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rewrite.SourcePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(rewrite.DestinationSourcePath);
            ArgumentNullException.ThrowIfNull(rewrite.Location);
            ArgumentNullException.ThrowIfNull(rewrite.Before);
            ArgumentNullException.ThrowIfNull(rewrite.Expected);
            ArgumentNullException.ThrowIfNull(rewrite.OldTarget);
            ArgumentNullException.ThrowIfNull(rewrite.ExpectedTarget);
            ArgumentException.ThrowIfNullOrWhiteSpace(rewrite.OldTarget.Path);
            ArgumentException.ThrowIfNullOrWhiteSpace(rewrite.ExpectedTarget.Path);
            ValidateNullable(rewrite.Layer, nameof(rewrite.Layer));
        }, "reference rewrites");
        if (formation.References.Rewrites.Length > formation.References.OccurrenceCount)
        {
            throw new ArgumentException(
                "Route Move reference rewrites cannot exceed inspected occurrences.");
        }

        ValidateUniqueOrdered(
            formation.References.Rewrites,
            rewrite => $"{rewrite.SourcePath}\u0000{rewrite.Location.ByteOffset:D20}",
            "reference rewrites");
    }

    private static void ValidateNavigationAndEffects(RouteMoveResultFormation formation)
    {
        ValidateItems(formation.GeneratedNavigation.Regions, region =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(region.Path);
            Validate(region.State, nameof(region.State));
            ValidateValues(region.Reasons, "generated-region reasons");
        }, "generated regions");
        ValidateUniqueOrdered(
            formation.GeneratedNavigation.Regions,
            region => region.Path,
            "generated regions");
        ValidateItems(formation.Effects, effect =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(effect.Path);
            ArgumentNullException.ThrowIfNull(effect.Before);
            ArgumentNullException.ThrowIfNull(effect.Expected);
            Validate(effect.Kind, nameof(effect.Kind));
            Validate(effect.Action, nameof(effect.Action));
            Validate(effect.Outcome, nameof(effect.Outcome));
            Validate(effect.Residual, nameof(effect.Residual));
        }, "effects");
        if (formation.Effects.Select(effect => effect.Path)
            .Distinct(StringComparer.Ordinal).Count() != formation.Effects.Length)
        {
            throw new ArgumentException("Route Move effects must identify unique paths.");
        }
        ValidateStrings(formation.UnchangedPaths, "unchanged paths");
        ValidateStrings(formation.Recovery.ProtectedPaths, "recovery protected paths");
        ValidateItems(formation.Findings, _ => { }, "findings");
    }

    private static void ValidateItems<T>(
        ImmutableArray<T> values,
        Action<T> validate,
        string name)
        where T : class
    {
        if (values.IsDefault)
        {
            throw new ArgumentException($"Route Move {name} must be initialized.");
        }

        foreach (var value in values)
        {
            ArgumentNullException.ThrowIfNull(value);
            validate(value);
        }
    }

    private static void ValidateStrings(ImmutableArray<string> values, string name)
    {
        if (values.IsDefault
            || values.Any(string.IsNullOrWhiteSpace)
            || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException(
                $"Route Move {name} must be initialized, nonempty, and unique.");
        }
    }

    private static void ValidateValues<T>(ImmutableArray<T> values, string name)
        where T : struct, Enum
    {
        if (values.IsDefault)
        {
            throw new ArgumentException($"Route Move {name} must be initialized.");
        }

        foreach (var value in values)
        {
            Validate(value, name);
        }

        if (values.Distinct().Count() != values.Length
            || values.Zip(
                values.Skip(1),
                (prior, next) => Comparer<T>.Default.Compare(prior, next) >= 0)
                .Any(outOfOrder => outOfOrder))
        {
            throw new ArgumentException($"Route Move {name} must be unique and ordered.");
        }
    }

    private static void ValidateUniqueOrdered<T>(
        ImmutableArray<T> values,
        Func<T, string> key,
        string name)
    {
        var keys = values.Select(key).ToArray();
        if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length
            || keys.Zip(
                keys.Skip(1),
                (prior, next) => string.CompareOrdinal(prior, next) >= 0)
                .Any(outOfOrder => outOfOrder))
        {
            throw new ArgumentException($"Route Move {name} must be unique and ordered.");
        }
    }

    private static void ValidateNullable<T>(T? value, string name)
        where T : struct, Enum
    {
        if (value is { } established)
        {
            Validate(established, name);
        }
    }

    private static void Validate<T>(T value, string name)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                name,
                value,
                $"The Route Move {name} value is not defined.");
        }
    }
}

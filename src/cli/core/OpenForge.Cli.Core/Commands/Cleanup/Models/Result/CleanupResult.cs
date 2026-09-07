using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Result;

internal sealed record CleanupResult : ICliCommandResult
{
    public string Command => CleanupDefinitions.CommandIdentity;

    public required CliSemanticStatus Status { get; init; }

    public required CliWorkspace? Workspace { get; init; }

    public required CliNextAction? Next { get; init; }

    internal required CleanupResultFacts Facts { get; init; }

    internal CleanupMode Mode => Facts.Mode;

    internal CleanupCatalogue Catalogue => Facts.Catalogue;

    internal CleanupPlan Plan => Facts.Plan;

    internal CleanupPreflight Preflight => Facts.Preflight;

    internal CleanupLease Lease => Facts.Lease;

    internal CleanupCatalogueComparison Revalidation => Facts.Revalidation;

    internal ImmutableArray<CleanupEffect> Effects => Facts.Effects;

    internal ImmutableArray<CleanupResidual> Residuals => Facts.Residuals;

    internal CleanupVerification Verification => Facts.Verification;

    internal ImmutableArray<CleanupFinding> Findings => Facts.Findings;
}

using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallPlanResultProjector
{
    internal InstallPlanBuild ProjectBoundary(InstallPlanningBoundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);

        return new InstallPlanBuild
        {
            ManagementState = boundary.State,
            Plan = null,
            Findings = [new(
                code: boundary.Code,
                cause: boundary.Cause,
                subject: boundary.Subject)],
            Evidence = Evidence(boundary.Payload, boundary.IntendedState),
        };
    }

    internal InstallPlanBuild ProjectTrustedExact(InstallPlanContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return ProjectCompleted(
            context,
            InstallManagementState.TrustedExact,
            new InstallPlanEffects
            {
                DirectoryCreations = [],
                TargetEffects = [],
                LifecycleEffect = null,
            });
    }

    internal InstallPlanBuild Project(InstallPlanningDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        return decision switch
        {
            InstallPlanningCompleted completed => ProjectCompleted(
                completed.Context,
                completed.State,
                completed.Effects),
            InstallPlanningWithFindings withFindings => ProjectWithFindings(
                withFindings.Context,
                withFindings.State,
                withFindings.Findings),
            _ => throw new ArgumentOutOfRangeException(
                nameof(decision),
                decision,
                "The Install planning decision is not defined."),
        };
    }

    private static InstallPlanBuild ProjectCompleted(
        InstallPlanContext context,
        InstallManagementState managementState,
        InstallPlanEffects effects)
        => new()
        {
            ManagementState = managementState,
            Findings = [],
            Evidence = Evidence(context.Payload, context.IntendedState),
            Plan = new InstallPlan
            {
                Request = context.Request,
                Payload = context.Payload,
                IntendedState = context.IntendedState,
                ManagementState = managementState,
                IntendedLifecycle = context.IntendedLifecycle,
                DirectoryCreations = effects.DirectoryCreations,
                TargetEffects = effects.TargetEffects,
                LifecycleEffect = effects.LifecycleEffect,
                Effects = CreateEffectIdentities(context, effects),
                Findings = [],
            },
        };

    private static InstallPlanBuild ProjectWithFindings(
        InstallPlanContext context,
        InstallManagementState managementState,
        IReadOnlyList<InstallFinding> findings)
        => new()
        {
            ManagementState = managementState,
            Findings = findings,
            Evidence = Evidence(context.Payload, context.IntendedState),
            Plan = new InstallPlan
            {
                Request = context.Request,
                Payload = context.Payload,
                IntendedState = context.IntendedState,
                ManagementState = managementState,
                IntendedLifecycle = context.IntendedLifecycle,
                DirectoryCreations = [],
                TargetEffects = [],
                LifecycleEffect = null,
                Effects = [],
                Findings = findings,
            },
        };

    private static InstallPlanningEvidence Evidence(
        FrameworkPayload? payload = null,
        InstallIntendedState? intendedState = null)
        => new()
        {
            Payload = payload,
            IntendedState = intendedState,
        };

    private static IReadOnlyList<InstallEffectIdentity> CreateEffectIdentities(
        InstallPlanContext context,
        InstallPlanEffects effects)
    {
        var identities = new List<InstallEffectIdentity>();
        identities.AddRange(effects.DirectoryCreations.Select(creation =>
            new InstallEffectIdentity
            {
                Path = CanonicalRelativePath(
                    context.Request.Workspace.LexicalRoot,
                    creation.LogicalPath),
                Kind = InstallEffectKind.Directory,
                Action = InstallEffectAction.Create,
                SourceAssetPath = null,
            }));
        identities.AddRange(effects.TargetEffects.Select(effect => effect.Identity));
        if (effects.LifecycleEffect is { } lifecycle)
        {
            identities.Add(lifecycle.Identity);
        }

        return identities;
    }

    private static string CanonicalRelativePath(string root, string path)
        => Path.GetRelativePath(root, path)
            .Replace(Path.DirectorySeparatorChar, '/');
}

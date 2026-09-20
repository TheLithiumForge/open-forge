using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Init;

internal static partial class RouteInitDefinitions
{
    internal static string ReadMachineName(RouteInitMode value)
        => value switch
        {
            RouteInitMode.Apply => "apply",
            RouteInitMode.DryRun => "dry-run",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitScaffold value)
        => value switch
        {
            RouteInitScaffold.Generic => "generic",
            RouteInitScaffold.Framework => "framework",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitPlanCompleteness value)
        => value switch
        {
            RouteInitPlanCompleteness.NotEstablished => "not-established",
            RouteInitPlanCompleteness.Incomplete => "incomplete",
            RouteInitPlanCompleteness.Complete => "complete",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitPlanSafety value)
        => value switch
        {
            RouteInitPlanSafety.NotEstablished => "not-established",
            RouteInitPlanSafety.Safe => "safe",
            RouteInitPlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitFrameworkSegmentRole value)
        => value switch
        {
            RouteInitFrameworkSegmentRole.InstalledRoot => "installed-root",
            RouteInitFrameworkSegmentRole.Managed => "managed",
            RouteInitFrameworkSegmentRole.Scope => "scope",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEntrypointForm value)
        => value switch
        {
            RouteInitEntrypointForm.Canonical => "canonical",
            RouteInitEntrypointForm.Compatibility => "compatibility",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEntrypointCurrent value)
        => value switch
        {
            RouteInitEntrypointCurrent.Existing => "existing",
            RouteInitEntrypointCurrent.Missing => "missing",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEntrypointOwnership value)
        => value switch
        {
            RouteInitEntrypointOwnership.User => "user",
            RouteInitEntrypointOwnership.Framework => "framework",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitDescriptionSource value)
        => value switch
        {
            RouteInitDescriptionSource.Draft => "draft",
            RouteInitDescriptionSource.Explicit => "explicit",
            RouteInitDescriptionSource.Embedded => "embedded",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitResponsibilitySource value)
        => value switch
        {
            RouteInitResponsibilitySource.DefaultOmitted => "default-omitted",
            RouteInitResponsibilitySource.ExplicitOmitted => "explicit-omitted",
            RouteInitResponsibilitySource.Explicit => "explicit",
            RouteInitResponsibilitySource.Embedded => "embedded",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitTagsSource value)
        => value switch
        {
            RouteInitTagsSource.Draft => "draft",
            RouteInitTagsSource.Explicit => "explicit",
            RouteInitTagsSource.Mixed => "mixed",
            RouteInitTagsSource.Embedded => "embedded",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEntrypointOutcome value)
        => value switch
        {
            RouteInitEntrypointOutcome.Unchanged => "unchanged",
            RouteInitEntrypointOutcome.Planned => "planned",
            RouteInitEntrypointOutcome.NotStarted => "not-started",
            RouteInitEntrypointOutcome.Created => "created",
            RouteInitEntrypointOutcome.VerificationFailed => "verification-failed",
            RouteInitEntrypointOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEffectKind value)
        => value switch
        {
            RouteInitEffectKind.Directory => "directory",
            RouteInitEffectKind.Entrypoint => "entrypoint",
            RouteInitEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEffectAction value)
        => value switch
        {
            RouteInitEffectAction.Create => "create",
            RouteInitEffectAction.Replace => "replace",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEffectOutcome value)
        => value switch
        {
            RouteInitEffectOutcome.Planned => "planned",
            RouteInitEffectOutcome.NotStarted => "not-started",
            RouteInitEffectOutcome.Verified => "verified",
            RouteInitEffectOutcome.VerificationFailed => "verification-failed",
            RouteInitEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitEffectResidual value)
        => value switch
        {
            RouteInitEffectResidual.None => "none",
            RouteInitEffectResidual.Retained => "retained",
            RouteInitEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitLifecycleAction value)
        => value switch
        {
            RouteInitLifecycleAction.None => "none",
            RouteInitLifecycleAction.Preserve => "preserve",
            RouteInitLifecycleAction.Publish => "publish",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitLifecycleOutcome value)
        => value switch
        {
            RouteInitLifecycleOutcome.NotRequested => "not-requested",
            RouteInitLifecycleOutcome.Planned => "planned",
            RouteInitLifecycleOutcome.AlreadyCurrent => "already-current",
            RouteInitLifecycleOutcome.NotStarted => "not-started",
            RouteInitLifecycleOutcome.Verified => "verified",
            RouteInitLifecycleOutcome.VerificationFailed => "verification-failed",
            RouteInitLifecycleOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitRecoveryState value)
        => value switch
        {
            RouteInitRecoveryState.NotRequired => "not-required",
            RouteInitRecoveryState.NotCreated => "not-created",
            RouteInitRecoveryState.Removed => "removed",
            RouteInitRecoveryState.Retained => "retained",
            RouteInitRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(RouteInitVerificationState value)
        => value switch
        {
            RouteInitVerificationState.NotRequested => "not-requested",
            RouteInitVerificationState.Verified => "verified",
            RouteInitVerificationState.Failed => "failed",
            RouteInitVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };
}

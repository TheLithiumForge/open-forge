using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusLifecycleHumanRenderingTests
{
    [Fact(DisplayName = "Status human rendering distinguishes lifecycle absence trusted emptiness and unavailable facts"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void HumanLifecycleLanguagePreservesTypedEmptyAndUnavailableStates()
    {
        var result = StatusResultSeeds.Representative(CliSemanticStatus.Complete);
        var absent = Render(WithExtensionLifecycle(result, OperationalLifecycleState.Absent));
        var trusted = Render(WithExtensionLifecycle(result, OperationalLifecycleState.Trusted));
        var incomplete = Render(WithExtensionLifecycle(
            result,
            OperationalLifecycleState.Incomplete));
        var unavailablePercentage = Render(result with
        {
            Facts = result.Facts with
            {
                Context = result.Facts.Context with
                {
                    StartupPercentage = new StatusDecimalValue(
                        OperationalValueState.Unavailable,
                        Value: null),
                },
            },
        });
        var notApplicablePercentage = Render(result with
        {
            Facts = result.Facts with
            {
                Installation = new StatusInstallation(
                    OperationalInstallationState.Uninstalled,
                    EntryPath: null,
                    LoaderPath: null),
                Context = result.Facts.Context with
                {
                    StartupPercentage = new StatusDecimalValue(
                        OperationalValueState.NotApplicable,
                        Value: null),
                },
            },
        });

        Assert.Contains("Extensions: 0 recorded", absent, StringComparison.Ordinal);
        Assert.Contains("Managed files: none recorded", absent, StringComparison.Ordinal);
        Assert.Contains("Extensions: 0", trusted, StringComparison.Ordinal);
        Assert.DoesNotContain("Extensions: 0 recorded", trusted, StringComparison.Ordinal);
        Assert.Contains("Managed files: none recorded", trusted, StringComparison.Ordinal);
        Assert.Contains("Extensions: unavailable", incomplete, StringComparison.Ordinal);
        Assert.Contains("Managed files: unavailable", incomplete, StringComparison.Ordinal);
        Assert.Contains("Startup percentage: unavailable", unavailablePercentage, StringComparison.Ordinal);
        Assert.DoesNotContain("Startup percentage: 0%", unavailablePercentage, StringComparison.Ordinal);
        Assert.Contains("Installation: uninstalled", notApplicablePercentage, StringComparison.Ordinal);
        Assert.Contains("Open Forge is not installed.", notApplicablePercentage, StringComparison.Ordinal);
        Assert.Contains("Startup percentage: not-applicable", notApplicablePercentage, StringComparison.Ordinal);
    }

    private static string Render(StatusResult result)
        => StatusHumanRenderer.Render(new CliPresentationRequest<StatusResult>(
            result,
            new CliPresentation(
                CliOutputFormat.Human,
                CliView.Compact,
                CliVerbosity.Normal)));

    private static StatusResult WithExtensionLifecycle(
        StatusResult result,
        OperationalLifecycleState state)
    {
        var valueState = state is OperationalLifecycleState.Absent
            or OperationalLifecycleState.Trusted
            ? OperationalValueState.Available
            : OperationalValueState.Unavailable;
        var value = new StatusIntegerValue(
            valueState,
            valueState == OperationalValueState.Available ? 0L : null);
        return result with
        {
            Facts = result.Facts with
            {
                Lifecycle = result.Facts.Lifecycle with
                {
                    Extensions = new StatusExtensionLifecycle
                    {
                        State = state,
                        SourceAvailability = state == OperationalLifecycleState.Absent
                            ? OperationalSourceAvailability.NotApplicable
                            : OperationalSourceAvailability.Unavailable,
                        Installed = [],
                        ManagedFiles = new StatusManagedExtensionFiles
                        {
                            Counts = new StatusManagedTargetCounts
                            {
                                Current = value,
                                Changed = value,
                                Missing = value,
                                Unavailable = value,
                                Blocked = value,
                            },
                            Targets = [],
                        },
                    },
                },
            },
        };
    }
}

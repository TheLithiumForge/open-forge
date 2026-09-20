using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal static class UpdatePlanningPolicy
{
    internal static UpdatePlanningPlan Plan(
        IReadOnlyList<UpdateComparison> comparisons,
        UpdatePlanningAuthority authority)
    {
        ArgumentNullException.ThrowIfNull(comparisons);
        ArgumentNullException.ThrowIfNull(authority);
        return new UpdatePlanningPlan(
            authority,
            comparisons.Select(comparison => new UpdatePlanningDecision(
                comparison ?? throw new ArgumentException(
                    "Update comparisons cannot contain null members.",
                    nameof(comparisons)),
                ReadDisposition(comparison, authority))));
    }

    private static UpdatePlanningDisposition ReadDisposition(
        UpdateComparison comparison,
        UpdatePlanningAuthority authority)
    {
        comparison.Validate();
        return comparison.IntendedState switch
        {
            UpdateComparisonIntendedState.Same => ReadCurrentDisposition(
                comparison,
                authority,
                equivalent: UpdatePlanningDisposition.NoOp),
            UpdateComparisonIntendedState.Changed => ReadCurrentDisposition(
                comparison,
                authority,
                equivalent: UpdatePlanningDisposition.Replace),
            UpdateComparisonIntendedState.New => comparison.CurrentState == UpdateComparisonCurrentState.Missing
                ? UpdatePlanningDisposition.Create
                : UpdatePlanningDisposition.Blocked,
            UpdateComparisonIntendedState.Retired => ReadRetiredDisposition(comparison, authority),
            UpdateComparisonIntendedState.Unavailable => UpdatePlanningDisposition.Blocked,
            UpdateComparisonIntendedState.Blocked => UpdatePlanningDisposition.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(comparison),
                comparison.IntendedState,
                "The Update intended state is not defined."),
        };
    }

    private static UpdatePlanningDisposition ReadCurrentDisposition(
        UpdateComparison comparison,
        UpdatePlanningAuthority authority,
        UpdatePlanningDisposition equivalent)
        => comparison.CurrentState switch
        {
            UpdateComparisonCurrentState.Same => equivalent,
            UpdateComparisonCurrentState.FormatOnly => equivalent,
            UpdateComparisonCurrentState.Changed => UpdatePlanningDisposition.Replace,
            UpdateComparisonCurrentState.Missing => UpdatePlanningDisposition.Restore,
            UpdateComparisonCurrentState.Unavailable => UpdatePlanningDisposition.Blocked,
            UpdateComparisonCurrentState.Blocked => UpdatePlanningDisposition.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(comparison),
                comparison.CurrentState,
                "The Update current state is not defined."),
        };

    private static UpdatePlanningDisposition ReadRetiredDisposition(
        UpdateComparison comparison,
        UpdatePlanningAuthority authority)
    {
        if (comparison.CurrentState == UpdateComparisonCurrentState.Missing)
        {
            return UpdatePlanningDisposition.NoOp;
        }

        if (!authority.Prune)
        {
            return UpdatePlanningDisposition.Preserve;
        }

        return comparison.RetirementEligibility switch
        {
            UpdateRetirementEligibility.Eligible => UpdatePlanningDisposition.Delete,
            UpdateRetirementEligibility.NotApplicable => UpdatePlanningDisposition.Blocked,
            UpdateRetirementEligibility.Ineligible => UpdatePlanningDisposition.Blocked,
            UpdateRetirementEligibility.Unavailable => UpdatePlanningDisposition.Blocked,
            UpdateRetirementEligibility.Blocked => UpdatePlanningDisposition.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(comparison),
                comparison.RetirementEligibility,
                "The Update retirement eligibility is not defined."),
        };
    }
}

using System.Text.Json;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed partial class LifecycleStore
{
    internal LifecycleWritePlanResult PlanFrameworkUpdate(
        LifecycleStoreReadResult current,
        FrameworkLifecycleState framework)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(framework);
        if (current.SelectedSection != LifecycleSection.Framework)
        {
            return LifecycleWritePlanResult.Blocked(
                "A Framework lifecycle update requires a Framework-selected read.");
        }

        var validation = LifecycleFrameworkValidator.Validate(framework);
        if (validation.State != LifecycleSectionValidationState.Valid)
        {
            return LifecycleWritePlanResult.Blocked(
                validation.Cause ?? "The intended Framework lifecycle section is blocked.");
        }

        if (!TryCreateBasis(current, out var basis, out var cause)
            || basis is null)
        {
            return LifecycleWritePlanResult.Blocked(cause);
        }

        try
        {
            var extensions = ReadExtensionsForWrite(current, basis.Envelope);
            var frameworkElement = JsonSerializer.SerializeToElement(
                framework,
                LifecycleJsonContext.Default.FrameworkLifecycleState);
            var candidate = ReplaceFramework(
                basis.Envelope,
                frameworkElement,
                CanonicalizeExtensionsForWrite(extensions));
            var collision = LifecycleFrameworkValidator.ValidateNoCrossSectionCollisions(
                framework,
                extensions);
            if (collision.State != LifecycleSectionValidationState.Valid)
            {
                return LifecycleWritePlanResult.Blocked(
                    collision.Cause ?? "The intended lifecycle sections collide.");
            }

            return basis.Envelope.Framework is { } existing
                && JsonElement.DeepEquals(existing, frameworkElement)
                ? LifecycleWritePlanResult.Unchanged()
                : FormPlan(basis, candidate);
        }
        catch (JsonException exception)
        {
            return LifecycleWritePlanResult.Blocked(
                $"The existing Extension lifecycle section is invalid: {exception.Message}");
        }
    }

    internal LifecycleWritePlanResult PlanExtensionUpdate(
        LifecycleStoreReadResult current,
        ExtensionLifecycleState extensions)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(extensions);
        if (current.SelectedSection != LifecycleSection.Extensions)
        {
            return LifecycleWritePlanResult.Blocked(
                "An Extension lifecycle update requires an Extension-selected read.");
        }

        if (!TryCreateBasis(current, out var basis, out var cause)
            || basis is null)
        {
            return LifecycleWritePlanResult.Blocked(cause);
        }

        try
        {
            var framework = ReadFrameworkForWrite(current, basis.Envelope);
            var extensionElement = JsonSerializer.SerializeToElement(
                extensions,
                LifecycleJsonContext.Default.ExtensionLifecycleState);
            var candidate = ReplaceExtensions(
                basis.Envelope,
                extensionElement,
                CanonicalizeFrameworkForWrite(framework));
            var validation = LifecycleDocumentValidator.ValidateExtensions(
                current.Workspace,
                candidate,
                extensions);
            if (validation.State != LifecycleReadState.Complete)
            {
                return LifecycleWritePlanResult.Blocked(
                    validation.Cause ?? "The intended Extension lifecycle section is blocked.");
            }

            var collision = LifecycleFrameworkValidator.ValidateNoCrossSectionCollisions(
                framework,
                extensions);
            if (collision.State != LifecycleSectionValidationState.Valid)
            {
                return LifecycleWritePlanResult.Blocked(
                    collision.Cause ?? "The intended lifecycle sections collide.");
            }

            return basis.Envelope.Extensions is { } existing
                && JsonElement.DeepEquals(existing, extensionElement)
                ? LifecycleWritePlanResult.Unchanged()
                : FormPlan(basis, candidate);
        }
        catch (JsonException exception)
        {
            return LifecycleWritePlanResult.Blocked(
                $"The existing Framework lifecycle section is invalid: {exception.Message}");
        }
    }

}

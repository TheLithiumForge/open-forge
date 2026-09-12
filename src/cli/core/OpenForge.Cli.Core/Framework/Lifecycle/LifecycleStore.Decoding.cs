using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed partial class LifecycleStore
{
    private static LifecycleStoreReadResult DecodeFramework(
        CliWorkspace workspace,
        FileStateSnapshot file,
        LifecycleEnvelopeV1 envelope)
    {
        if (envelope.Framework is not { } value || value.ValueKind == JsonValueKind.Null)
        {
            return LifecycleStoreReadResult.SectionMissing(
                workspace,
                LifecycleSection.Framework,
                file,
                envelope);
        }

        var framework = value.Deserialize(LifecycleJsonContext.Default.FrameworkLifecycleState)
            ?? throw new JsonException("The lifecycle Framework section cannot be null.");
        var validation = LifecycleFrameworkValidator.Validate(framework);
        return validation.State == LifecycleSectionValidationState.Valid
            ? LifecycleStoreReadResult.Available(
                workspace,
                LifecycleSection.Framework,
                file,
                envelope,
                framework,
                extensions: null)
            : LifecycleStoreReadResult.Blocked(
                workspace,
                LifecycleSection.Framework,
                file,
                validation.Cause ?? "The lifecycle Framework section is blocked.");
    }

    private static LifecycleStoreReadResult DecodeExtensions(
        CliWorkspace workspace,
        FileStateSnapshot file,
        LifecycleEnvelopeV1 envelope)
    {
        if (envelope.Extensions is not { } value || value.ValueKind == JsonValueKind.Null)
        {
            return LifecycleStoreReadResult.SectionMissing(
                workspace,
                LifecycleSection.Extensions,
                file,
                envelope);
        }

        var extensions = value.Deserialize(LifecycleJsonContext.Default.ExtensionLifecycleState)
            ?? throw new JsonException("The lifecycle Extension section cannot be null.");
        var validation = LifecycleDocumentValidator.ValidateExtensions(
            workspace,
            envelope,
            extensions);
        if (validation.State == LifecycleReadState.Complete)
        {
            return LifecycleStoreReadResult.Available(
                workspace,
                LifecycleSection.Extensions,
                file,
                envelope,
                framework: null,
                extensions);
        }

        return validation.Trust == LifecycleExtensionTrust.Blocked
            ? LifecycleStoreReadResult.Blocked(
                workspace,
                LifecycleSection.Extensions,
                file,
                validation.Cause ?? "The lifecycle Extension section is blocked.")
            : LifecycleStoreReadResult.Invalid(
                workspace,
                LifecycleSection.Extensions,
                file,
                validation.Cause ?? "The lifecycle Extension section is invalid.");
    }

}

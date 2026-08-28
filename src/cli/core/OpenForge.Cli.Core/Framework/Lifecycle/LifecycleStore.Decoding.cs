using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

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

    private static LifecycleStoreReadResult FromResolution(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        PhysicalPathResolution resolution)
        => resolution.Failure is not null
            ? LifecycleStoreReadResult.Unavailable(
                workspace,
                selectedSection,
                resolution.Failure)
            : LifecycleStoreReadResult.Blocked(
                workspace,
                selectedSection,
                file: null,
                "The lifecycle document physical boundary is unsafe or unavailable.");

    private static LifecycleStoreReadResult Unavailable(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FilesystemFailureKind kind,
        Exception exception)
        => LifecycleStoreReadResult.Unavailable(
            workspace,
            selectedSection,
            FilesystemFailure.FromException(kind, exception));
}

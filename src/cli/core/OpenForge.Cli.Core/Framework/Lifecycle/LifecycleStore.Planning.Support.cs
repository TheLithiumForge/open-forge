using System.Text.Json;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed partial class LifecycleStore
{
    private static bool TryCreateBasis(
        LifecycleStoreReadResult current,
        out LifecyclePlanBasis? basis,
        out string cause)
    {
        cause = string.Empty;
        if (current.State == LifecycleStoreReadState.DocumentMissing)
        {
            var file = current.File
                ?? throw new InvalidOperationException("A missing lifecycle read requires file facts.");
            basis = new LifecyclePlanBasis(
                file.Expectation,
                file,
                new LifecycleEnvelopeV1
                {
                    SchemaVersion = LifecycleSchema.Version,
                    FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
                    WorkspacePath = NormalizeRoot(current.Workspace.LexicalRoot),
                    Framework = null,
                    Extensions = null,
                });
            return true;
        }

        if ((current.State is LifecycleStoreReadState.Available
                or LifecycleStoreReadState.SectionMissing)
            && current.File is { } existingFile
            && current.Envelope is { } existingEnvelope)
        {
            basis = new LifecyclePlanBasis(
                existingFile.Expectation,
                existingFile,
                existingEnvelope);
            return true;
        }

        basis = null;
        cause = "Lifecycle update planning requires an available, section-missing, or document-missing read.";
        return false;
    }

    private static FrameworkLifecycleState? ReadFrameworkForWrite(
        LifecycleStoreReadResult current,
        LifecycleEnvelopeV1 envelope)
    {
        ValidateOtherSectionDuplicates(current, LifecycleSection.Framework);
        if (envelope.Framework is not { } element || element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var framework = element.Deserialize(LifecycleJsonContext.Default.FrameworkLifecycleState)
            ?? throw new JsonException("The lifecycle Framework section cannot be null.");
        var validation = LifecycleFrameworkValidator.Validate(framework);
        return validation.State == LifecycleSectionValidationState.Valid
            ? framework
            : throw new JsonException(
                validation.Cause ?? "The lifecycle Framework section is blocked.");
    }

    private static JsonElement? CanonicalizeFrameworkForWrite(
        FrameworkLifecycleState? framework)
        => framework is null
            ? null
            : JsonSerializer.SerializeToElement(
                framework,
                LifecycleJsonContext.Default.FrameworkLifecycleState);

    private static ExtensionLifecycleState? ReadExtensionsForWrite(
        LifecycleStoreReadResult current,
        LifecycleEnvelopeV1 envelope)
    {
        ValidateOtherSectionDuplicates(current, LifecycleSection.Extensions);
        if (envelope.Extensions is not { } element || element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var extensions = element.Deserialize(LifecycleJsonContext.Default.ExtensionLifecycleState)
            ?? throw new JsonException("The lifecycle Extension section cannot be null.");
        var validation = LifecycleDocumentValidator.ValidateExtensions(
            current.Workspace,
            envelope,
            extensions);
        return validation.State == LifecycleReadState.Complete
            ? extensions
            : throw new JsonException(
                validation.Cause ?? "The lifecycle Extension section is blocked.");
    }

    private static JsonElement? CanonicalizeExtensionsForWrite(
        ExtensionLifecycleState? extensions)
        => extensions is null
            ? null
            : JsonSerializer.SerializeToElement(
                extensions,
                LifecycleJsonContext.Default.ExtensionLifecycleState);

    private static void ValidateOtherSectionDuplicates(
        LifecycleStoreReadResult current,
        LifecycleSection section)
    {
        if (current.State != LifecycleStoreReadState.DocumentMissing)
        {
            var file = current.File
                ?? throw new InvalidOperationException(
                    "An existing lifecycle read requires exact file bytes.");
            LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(
                file.Bytes.AsSpan(),
                section);
        }
    }

    private static LifecycleWritePlanResult FormPlan(
        LifecyclePlanBasis basis,
        LifecycleEnvelopeV1 candidate)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            candidate,
            LifecycleJsonContext.Default.LifecycleEnvelopeV1);
        if (basis.File.Kind == FileExpectationKind.File
            && basis.File.Bytes.AsSpan().SequenceEqual(bytes))
        {
            return LifecycleWritePlanResult.Unchanged();
        }

        var change = basis.Expectation.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(basis.Expectation, bytes)
            : PlannedFileChange.Replace(basis.Expectation, bytes);
        return LifecycleWritePlanResult.Planned(change);
    }

    private static LifecycleEnvelopeV1 ReplaceFramework(
        LifecycleEnvelopeV1 envelope,
        JsonElement framework,
        JsonElement? extensions)
        => new()
        {
            SchemaVersion = envelope.SchemaVersion,
            FingerprintPolicy = envelope.FingerprintPolicy,
            WorkspacePath = envelope.WorkspacePath,
            Framework = framework,
            Extensions = extensions,
        };

    private static LifecycleEnvelopeV1 ReplaceExtensions(
        LifecycleEnvelopeV1 envelope,
        JsonElement extensions,
        JsonElement? framework)
        => new()
        {
            SchemaVersion = envelope.SchemaVersion,
            FingerprintPolicy = envelope.FingerprintPolicy,
            WorkspacePath = envelope.WorkspacePath,
            Framework = framework,
            Extensions = extensions,
        };

    private static string NormalizeRoot(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private sealed record LifecyclePlanBasis(
        FileExpectation Expectation,
        FileStateSnapshot File,
        LifecycleEnvelopeV1 Envelope);
}

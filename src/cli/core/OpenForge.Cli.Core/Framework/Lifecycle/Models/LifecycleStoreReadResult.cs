using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models;

internal enum LifecycleStoreReadState
{
    Available,
    DocumentMissing,
    SectionMissing,
    Invalid,
    Blocked,
    Unavailable,
    Cancelled,
}

internal sealed partial record LifecycleStoreReadResult
{
    private const int MaximumCauseLength = 256;

    private LifecycleStoreReadResult(
        LifecycleStoreReadState state,
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Workspace = workspace;
        SelectedSection = selectedSection;
        File = file;
        Envelope = envelope;
        Framework = framework;
        Extensions = extensions;
        Failure = failure;
        Cause = cause;
    }

    internal LifecycleStoreReadState State { get; }

    internal CliWorkspace Workspace { get; }

    internal LifecycleSection SelectedSection { get; }

    internal FileStateSnapshot? File { get; }

    internal LifecycleEnvelopeV1? Envelope { get; }

    internal FrameworkLifecycleState? Framework { get; }

    internal ExtensionLifecycleState? Extensions { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    private static LifecycleStoreReadResult Create(
        LifecycleStoreReadState state,
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (!Enum.IsDefined(selectedSection))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedSection),
                selectedSection,
                "The lifecycle section is not defined.");
        }

        var validatedCause = ValidateState(
            state,
            workspace,
            selectedSection,
            file,
            envelope,
            framework,
            extensions,
            failure,
            cause);

        return new LifecycleStoreReadResult(
            state: state,
            workspace: workspace,
            selectedSection: selectedSection,
            file: file,
            envelope: envelope,
            framework: framework,
            extensions: extensions,
            failure: failure,
            cause: validatedCause);
    }

    private static string? ValidateState(
        LifecycleStoreReadState state,
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The lifecycle read state is not defined.");
        }

        ValidateSnapshotPath(workspace, file);
        switch (state)
        {
            case LifecycleStoreReadState.Available:
                ValidateAvailable(
                    workspace,
                    selectedSection,
                    file,
                    envelope,
                    framework,
                    extensions,
                    failure,
                    cause);
                return null;

            case LifecycleStoreReadState.DocumentMissing:
                ValidateDocumentMissing(
                    file,
                    envelope,
                    framework,
                    extensions,
                    failure,
                    cause);
                return ValidateRequiredCause(cause);

            case LifecycleStoreReadState.SectionMissing:
                ValidateSectionMissing(
                    workspace,
                    selectedSection,
                    file,
                    envelope,
                    framework,
                    extensions,
                    failure,
                    cause);
                return ValidateRequiredCause(cause);

            case LifecycleStoreReadState.Invalid:
                ValidateInvalid(
                    file,
                    envelope,
                    framework,
                    extensions,
                    failure,
                    cause);
                return ValidateRequiredCause(cause);

            case LifecycleStoreReadState.Blocked:
                ValidateBlocked(
                    file,
                    envelope,
                    framework,
                    extensions,
                    failure,
                    cause);
                return ValidateRequiredCause(cause);

            case LifecycleStoreReadState.Unavailable:
                ValidateUnavailable(
                    file,
                    envelope,
                    framework,
                    extensions,
                    failure,
                    cause);
                return cause;

            case LifecycleStoreReadState.Cancelled:
                ValidateCancelled(
                    file,
                    envelope,
                    framework,
                    extensions,
                    failure,
                    cause);
                return null;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "The lifecycle read state is not defined.");
        }
    }

    private static void ValidateSnapshotPath(
        CliWorkspace workspace,
        FileStateSnapshot? file)
    {
        if (file is null)
        {
            return;
        }

        var canonicalPath = FileExpectation.NormalizeAbsolutePath(
            Path.Combine(workspace.LexicalRoot, LifecycleSchema.RelativePath),
            nameof(file));
        if (!string.Equals(file.LogicalPath, canonicalPath, PathComparison()))
        {
            throw new ArgumentException(
                "A lifecycle file snapshot must use the canonical workspace lifecycle path.",
                nameof(file));
        }

        if (file.PhysicalPath is { } physicalPath
            && !PhysicalContainment.Contains(workspace.PhysicalRoot, physicalPath))
        {
            throw new ArgumentException(
                "A lifecycle file snapshot physical path must be contained by the workspace.",
                nameof(file));
        }
    }

    private static void ValidateAvailable(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        ValidateExistingFile(file);
        var validEnvelope = ValidateCommonEnvelope(workspace, envelope);
        ValidateNoFailureOrCause(failure, cause);

        if (selectedSection == LifecycleSection.Framework)
        {
            if (framework is null || extensions is not null)
            {
                throw new ArgumentException(
                    "An available Framework lifecycle read requires only its selected typed state.",
                    nameof(framework));
            }

            var raw = RequireRawSection(validEnvelope, selectedSection);
            ValidateFramework(framework, raw);
            return;
        }

        if (extensions is null || framework is not null)
        {
            throw new ArgumentException(
                "An available Extension lifecycle read requires only its selected typed state.",
                nameof(extensions));
        }

        var extensionRaw = RequireRawSection(validEnvelope, selectedSection);
        ValidateExtensions(workspace, validEnvelope, extensions, extensionRaw);
    }

    private static void ValidateDocumentMissing(
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        if (file is null || file.Kind != FileExpectationKind.Missing || file.HasBytes)
        {
            throw new ArgumentException(
                "A missing lifecycle document requires a missing file snapshot.",
                nameof(file));
        }

        ValidateNoDocumentFacts(envelope, framework, extensions, failure);
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
    }

    private static void ValidateSectionMissing(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        ValidateExistingFile(file);
        var validEnvelope = ValidateCommonEnvelope(workspace, envelope);
        ValidateNoDocumentFacts(
            null,
            framework,
            extensions,
            failure);
        if (HasRawSection(validEnvelope, selectedSection))
        {
            throw new ArgumentException(
                "A section-missing lifecycle read cannot carry its selected raw section.",
                nameof(envelope));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
    }

    private static void ValidateInvalid(
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        ValidateExistingDocument(file);
        ValidateNoDocumentFacts(envelope, framework, extensions, failure);
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
    }

    private static void ValidateBlocked(
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        if (file is not null)
        {
            ValidateExistingDocument(file);
        }

        ValidateNoDocumentFacts(envelope, framework, extensions, failure);
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
    }

    private static void ValidateUnavailable(
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        if (file is not null)
        {
            throw new ArgumentException(
                "An unavailable lifecycle read cannot carry file facts.",
                nameof(file));
        }

        if (envelope is not null
            || framework is not null
            || extensions is not null)
        {
            throw new ArgumentException(
                "An unavailable lifecycle read cannot carry document facts.");
        }

        ArgumentNullException.ThrowIfNull(failure);
        if (!string.Equals(cause, failure.DirectCause, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An unavailable lifecycle read cause must describe its filesystem failure.",
                nameof(cause));
        }
    }

    private static void ValidateCancelled(
        FileStateSnapshot? file,
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure,
        string? cause)
    {
        if (file is not null
            || envelope is not null
            || framework is not null
            || extensions is not null
            || failure is not null
            || cause is not null)
        {
            throw new ArgumentException(
                "A cancelled lifecycle read cannot carry document or failure facts.");
        }
    }

    private static LifecycleEnvelopeV1 ValidateCommonEnvelope(
        CliWorkspace workspace,
        LifecycleEnvelopeV1? envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var common = LifecycleDocumentValidator.ValidateCommon(workspace, envelope);
        if (common.State != LifecycleCommonValidationState.Valid)
        {
            throw new ArgumentException(
                common.Cause ?? "The lifecycle common envelope is not valid.",
                nameof(envelope));
        }

        return envelope;
    }

    private static void ValidateFramework(
        FrameworkLifecycleState framework,
        JsonElement raw)
    {
        var validation = LifecycleFrameworkValidator.Validate(framework);
        if (validation.State != LifecycleSectionValidationState.Valid)
        {
            throw new ArgumentException(
                validation.Cause ?? "The lifecycle Framework state is not valid.",
                nameof(framework));
        }

        var canonical = JsonSerializer.SerializeToElement(
            framework,
            LifecycleJsonContext.Default.FrameworkLifecycleState);
        if (!JsonElement.DeepEquals(raw, canonical))
        {
            throw new ArgumentException(
                "The selected Framework raw value does not equal its typed state.",
                nameof(raw));
        }
    }

    private static void ValidateExtensions(
        CliWorkspace workspace,
        LifecycleEnvelopeV1 envelope,
        ExtensionLifecycleState extensions,
        JsonElement raw)
    {
        var validation = LifecycleDocumentValidator.ValidateExtensions(
            workspace,
            envelope,
            extensions);
        if (validation.State != LifecycleReadState.Complete)
        {
            throw new ArgumentException(
                validation.Cause ?? "The lifecycle Extension state is not valid.",
                nameof(extensions));
        }

        var canonical = JsonSerializer.SerializeToElement(
            extensions,
            LifecycleJsonContext.Default.ExtensionLifecycleState);
        if (!JsonElement.DeepEquals(raw, canonical))
        {
            throw new ArgumentException(
                "The selected Extension raw value does not equal its typed state.",
                nameof(raw));
        }
    }

    private static JsonElement RequireRawSection(
        LifecycleEnvelopeV1 envelope,
        LifecycleSection selectedSection)
    {
        var raw = selectedSection == LifecycleSection.Framework
            ? envelope.Framework
            : envelope.Extensions;
        if (raw is not { } value || value.ValueKind == JsonValueKind.Null)
        {
            throw new ArgumentException(
                "An available lifecycle read requires a present selected raw section.",
                nameof(envelope));
        }

        return value;
    }

    private static bool HasRawSection(
        LifecycleEnvelopeV1 envelope,
        LifecycleSection selectedSection)
    {
        var raw = selectedSection == LifecycleSection.Framework
            ? envelope.Framework
            : envelope.Extensions;
        return raw is { } value && value.ValueKind != JsonValueKind.Null;
    }

    private static void ValidateExistingFile(FileStateSnapshot? file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Kind != FileExpectationKind.File || !file.HasBytes)
        {
            throw new ArgumentException(
                "A lifecycle document read requires exact ordinary file bytes.",
                nameof(file));
        }
    }

    private static void ValidateExistingDocument(FileStateSnapshot? file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Kind is not (FileExpectationKind.File or FileExpectationKind.Directory)
            || file.Kind == FileExpectationKind.File && !file.HasBytes
            || file.Kind == FileExpectationKind.Directory && file.HasBytes)
        {
            throw new ArgumentException(
                "An existing lifecycle read requires an ordinary file with bytes or a directory snapshot.",
                nameof(file));
        }
    }

    private static void ValidateNoDocumentFacts(
        LifecycleEnvelopeV1? envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions,
        FilesystemFailure? failure)
    {
        if (envelope is not null
            || framework is not null
            || extensions is not null
            || failure is not null)
        {
            throw new ArgumentException(
                "This lifecycle read state cannot carry document or failure facts.");
        }
    }

    private static void ValidateNoFailureOrCause(
        FilesystemFailure? failure,
        string? cause)
    {
        if (failure is not null || cause is not null)
        {
            throw new ArgumentException(
                "An available lifecycle read cannot carry failure or cause facts.");
        }
    }

    private static void ValidateDocument(
        FileStateSnapshot file,
        LifecycleEnvelopeV1 envelope)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(envelope);
        if (file.Kind != FileExpectationKind.File || !file.HasBytes)
        {
            throw new ArgumentException(
                "A lifecycle document read requires exact file bytes.",
                nameof(file));
        }
    }

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    private static string ValidateRequiredCause(string? cause)
    {
        if (string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "This lifecycle read state requires a cause.",
                nameof(cause));
        }

        return ValidateCause(cause);
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}

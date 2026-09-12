using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;

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

    private sealed record LifecycleReadFacts(
        LifecycleStoreReadState State,
        CliWorkspace Workspace,
        LifecycleSection SelectedSection,
        FileStateSnapshot? File,
        LifecycleEnvelopeV1? Envelope,
        FrameworkLifecycleState? Framework,
        ExtensionLifecycleState? Extensions,
        FilesystemFailure? Failure,
        string? Cause);

    private LifecycleStoreReadResult(
        LifecycleReadFacts facts,
        string? validatedCause)
    {
        State = facts.State;
        Workspace = facts.Workspace;
        SelectedSection = facts.SelectedSection;
        File = facts.File;
        Envelope = facts.Envelope;
        Framework = facts.Framework;
        Extensions = facts.Extensions;
        Failure = facts.Failure;
        Cause = validatedCause;
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

    private static LifecycleStoreReadResult Create(LifecycleReadFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts.Workspace, "workspace");
        if (!Enum.IsDefined(facts.SelectedSection))
        {
            throw new ArgumentOutOfRangeException(
                "selectedSection",
                facts.SelectedSection,
                "The lifecycle section is not defined.");
        }

        var validatedCause = ValidateState(facts);

        return new LifecycleStoreReadResult(facts, validatedCause);
    }

    private static string? ValidateState(LifecycleReadFacts facts)
    {
        if (!Enum.IsDefined(facts.State))
        {
            throw new ArgumentOutOfRangeException(
                "state",
                facts.State,
                "The lifecycle read state is not defined.");
        }

        ValidateSnapshotPath(facts.Workspace, facts.File);
        switch (facts.State)
        {
            case LifecycleStoreReadState.Available:
                ValidateAvailable(facts);
                return null;

            case LifecycleStoreReadState.DocumentMissing:
                ValidateDocumentMissing(facts);
                return ValidateRequiredCause(facts.Cause);

            case LifecycleStoreReadState.SectionMissing:
                ValidateSectionMissing(facts);
                return ValidateRequiredCause(facts.Cause);

            case LifecycleStoreReadState.Invalid:
                ValidateInvalid(facts);
                return ValidateRequiredCause(facts.Cause);

            case LifecycleStoreReadState.Blocked:
                ValidateBlocked(facts);
                return ValidateRequiredCause(facts.Cause);

            case LifecycleStoreReadState.Unavailable:
                ValidateUnavailable(facts);
                return facts.Cause;

            case LifecycleStoreReadState.Cancelled:
                ValidateCancelled(facts);
                return null;

            default:
                throw new ArgumentOutOfRangeException(
                    "state",
                    facts.State,
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

        LifecycleDocumentValidator.ValidateSnapshotPath(workspace, file);
    }

    private static void ValidateAvailable(LifecycleReadFacts facts)
    {
        ValidateExistingFile(facts.File);
        var validEnvelope = ValidateCommonEnvelope(facts.Workspace, facts.Envelope);
        ValidateNoFailureOrCause(facts.Failure, facts.Cause);

        if (facts.SelectedSection == LifecycleSection.Framework)
        {
            if (facts.Framework is null || facts.Extensions is not null)
            {
                throw new ArgumentException(
                    "An available Framework lifecycle read requires only its selected typed state.",
                    "framework");
            }

            var raw = RequireRawSection(validEnvelope, facts.SelectedSection);
            ValidateFramework(facts.Framework, raw);
            return;
        }

        if (facts.Extensions is null || facts.Framework is not null)
        {
            throw new ArgumentException(
                "An available Extension lifecycle read requires only its selected typed state.",
                "extensions");
        }

        var extensionRaw = RequireRawSection(validEnvelope, facts.SelectedSection);
        ValidateExtensions(facts.Workspace, validEnvelope, facts.Extensions, extensionRaw);
    }

    private static void ValidateDocumentMissing(LifecycleReadFacts facts)
    {
        if (facts.File is null || facts.File.Kind != FileExpectationKind.Missing || facts.File.HasBytes)
        {
            throw new ArgumentException(
                "A missing lifecycle document requires a missing file snapshot.",
                "file");
        }

        ValidateNoDocumentFacts(facts.Envelope, facts.Framework, facts.Extensions, facts.Failure);
        ArgumentException.ThrowIfNullOrWhiteSpace(facts.Cause, "cause");
    }

    private static void ValidateSectionMissing(LifecycleReadFacts facts)
    {
        ValidateExistingFile(facts.File);
        var validEnvelope = ValidateCommonEnvelope(facts.Workspace, facts.Envelope);
        ValidateNoDocumentFacts(
            null,
            facts.Framework,
            facts.Extensions,
            facts.Failure);
        if (HasRawSection(validEnvelope, facts.SelectedSection))
        {
            throw new ArgumentException(
                "A section-missing lifecycle read cannot carry its selected raw section.",
                "envelope");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(facts.Cause, "cause");
    }

    private static void ValidateInvalid(LifecycleReadFacts facts)
    {
        ValidateExistingDocument(facts.File);
        ValidateNoDocumentFacts(facts.Envelope, facts.Framework, facts.Extensions, facts.Failure);
        ArgumentException.ThrowIfNullOrWhiteSpace(facts.Cause, "cause");
    }

    private static void ValidateBlocked(LifecycleReadFacts facts)
    {
        if (facts.File is not null)
        {
            ValidateExistingDocument(facts.File);
        }

        ValidateNoDocumentFacts(facts.Envelope, facts.Framework, facts.Extensions, facts.Failure);
        ArgumentException.ThrowIfNullOrWhiteSpace(facts.Cause, "cause");
    }

    private static void ValidateUnavailable(LifecycleReadFacts facts)
    {
        if (facts.File is not null)
        {
            throw new ArgumentException(
                "An unavailable lifecycle read cannot carry file facts.",
                "file");
        }

        if (facts.Envelope is not null
            || facts.Framework is not null
            || facts.Extensions is not null)
        {
            throw new ArgumentException(
                "An unavailable lifecycle read cannot carry document facts.");
        }

        ArgumentNullException.ThrowIfNull(facts.Failure, "failure");
        if (!string.Equals(facts.Cause, facts.Failure.DirectCause, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An unavailable lifecycle read cause must describe its filesystem failure.",
                "cause");
        }
    }

    private static void ValidateCancelled(LifecycleReadFacts facts)
    {
        if (facts.File is not null
            || facts.Envelope is not null
            || facts.Framework is not null
            || facts.Extensions is not null
            || facts.Failure is not null
            || facts.Cause is not null)
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
}

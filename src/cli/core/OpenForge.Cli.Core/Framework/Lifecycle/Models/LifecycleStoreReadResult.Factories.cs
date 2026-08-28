using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models;

internal sealed partial record LifecycleStoreReadResult
{
    internal static LifecycleStoreReadResult Available(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file,
        LifecycleEnvelopeV1 envelope,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions)
    {
        ValidateDocument(file, envelope);
        if (selectedSection == LifecycleSection.Framework
            ? framework is null || extensions is not null
            : extensions is null || framework is not null)
        {
            throw new ArgumentException(
                "An available lifecycle read requires only its selected typed section.",
                nameof(selectedSection));
        }

        return Create(
            state: LifecycleStoreReadState.Available,
            workspace: workspace,
            selectedSection: selectedSection,
            file: file,
            envelope: envelope,
            framework: framework,
            extensions: extensions,
            failure: null,
            cause: null);
    }

    internal static LifecycleStoreReadResult DocumentMissing(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Kind != FileExpectationKind.Missing)
        {
            throw new ArgumentException(
                "A missing lifecycle document requires a missing file snapshot.",
                nameof(file));
        }

        return Create(
            state: LifecycleStoreReadState.DocumentMissing,
            workspace: workspace,
            selectedSection: selectedSection,
            file: file,
            envelope: null,
            framework: null,
            extensions: null,
            failure: null,
            cause: "The lifecycle document is missing.");
    }

    internal static LifecycleStoreReadResult SectionMissing(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file,
        LifecycleEnvelopeV1 envelope)
    {
        ValidateDocument(file, envelope);
        return Create(
            state: LifecycleStoreReadState.SectionMissing,
            workspace: workspace,
            selectedSection: selectedSection,
            file: file,
            envelope: envelope,
            framework: null,
            extensions: null,
            failure: null,
            cause: "The selected lifecycle section is missing.");
    }

    internal static LifecycleStoreReadResult Invalid(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(file);
        return Create(
            state: LifecycleStoreReadState.Invalid,
            workspace: workspace,
            selectedSection: selectedSection,
            file: file,
            envelope: null,
            framework: null,
            extensions: null,
            failure: null,
            cause: ValidateCause(cause));
    }

    internal static LifecycleStoreReadResult Blocked(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot? file,
        string cause)
        => Create(
            state: LifecycleStoreReadState.Blocked,
            workspace: workspace,
            selectedSection: selectedSection,
            file: file,
            envelope: null,
            framework: null,
            extensions: null,
            failure: null,
            cause: ValidateCause(cause));

    internal static LifecycleStoreReadResult Unavailable(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FilesystemFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return Create(
            state: LifecycleStoreReadState.Unavailable,
            workspace: workspace,
            selectedSection: selectedSection,
            file: null,
            envelope: null,
            framework: null,
            extensions: null,
            failure: failure,
            cause: failure.DirectCause);
    }

    internal static LifecycleStoreReadResult Cancelled(
        CliWorkspace workspace,
        LifecycleSection selectedSection)
        => Create(
            state: LifecycleStoreReadState.Cancelled,
            workspace: workspace,
            selectedSection: selectedSection,
            file: null,
            envelope: null,
            framework: null,
            extensions: null,
            failure: null,
            cause: null);
}

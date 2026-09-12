using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;

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

        return Create(new LifecycleReadFacts(
            State: LifecycleStoreReadState.Available,
            Workspace: workspace,
            SelectedSection: selectedSection,
            File: file,
            Envelope: envelope,
            Framework: framework,
            Extensions: extensions,
            Failure: null,
            Cause: null));
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

        return Create(new LifecycleReadFacts(
            State: LifecycleStoreReadState.DocumentMissing,
            Workspace: workspace,
            SelectedSection: selectedSection,
            File: file,
            Envelope: null,
            Framework: null,
            Extensions: null,
            Failure: null,
            Cause: "The lifecycle document is missing."));
    }

    internal static LifecycleStoreReadResult SectionMissing(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file,
        LifecycleEnvelopeV1 envelope)
    {
        ValidateDocument(file, envelope);
        return Create(new LifecycleReadFacts(
            State: LifecycleStoreReadState.SectionMissing,
            Workspace: workspace,
            SelectedSection: selectedSection,
            File: file,
            Envelope: envelope,
            Framework: null,
            Extensions: null,
            Failure: null,
            Cause: "The selected lifecycle section is missing."));
    }

    internal static LifecycleStoreReadResult Invalid(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(file);
        return Create(new LifecycleReadFacts(
            State: LifecycleStoreReadState.Invalid,
            Workspace: workspace,
            SelectedSection: selectedSection,
            File: file,
            Envelope: null,
            Framework: null,
            Extensions: null,
            Failure: null,
            Cause: ValidateCause(cause)));
    }

    internal static LifecycleStoreReadResult Blocked(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot? file,
        string cause)
        => Create(new LifecycleReadFacts(
            State: LifecycleStoreReadState.Blocked,
            Workspace: workspace,
            SelectedSection: selectedSection,
            File: file,
            Envelope: null,
            Framework: null,
            Extensions: null,
            Failure: null,
            Cause: ValidateCause(cause)));

    internal static LifecycleStoreReadResult Unavailable(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FilesystemFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return Create(new LifecycleReadFacts(
            State: LifecycleStoreReadState.Unavailable,
            Workspace: workspace,
            SelectedSection: selectedSection,
            File: null,
            Envelope: null,
            Framework: null,
            Extensions: null,
            Failure: failure,
            Cause: failure.DirectCause));
    }

    internal static LifecycleStoreReadResult Cancelled(
        CliWorkspace workspace,
        LifecycleSection selectedSection)
        => Create(new LifecycleReadFacts(
            State: LifecycleStoreReadState.Cancelled,
            Workspace: workspace,
            SelectedSection: selectedSection,
            File: null,
            Envelope: null,
            Framework: null,
            Extensions: null,
            Failure: null,
            Cause: null));
}

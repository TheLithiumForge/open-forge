using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Identity;

internal enum RecoveryBundleProducer
{
    Framework,
    Extension,
    Index,
    Route,
    Repair,
    Library,
}

internal enum RecoveryBundleOperation
{
    Install,
    Index,
    Create,
    Init,
    Move,
    Update,
    Remove,
    Repair,
    Attach,
    Sync,
    Detach,
}

internal enum RecoveryBundleSubjectKind
{
    Workspace,
}

internal sealed record RecoveryBundleSubject
{
    private RecoveryBundleSubject(RecoveryBundleSubjectKind kind, string identity)
    {
        Kind = kind;
        Identity = identity;
    }

    internal RecoveryBundleSubjectKind Kind { get; }

    internal string Identity { get; }

    internal static RecoveryBundleSubject Workspace(string identity)
    {
        if (!IsWorkspaceKey(identity))
        {
            throw new ArgumentException(
                "A recovery workspace subject requires one lowercase SHA-256 workspace key.",
                nameof(identity));
        }

        return new RecoveryBundleSubject(RecoveryBundleSubjectKind.Workspace, identity);
    }

    private static bool IsWorkspaceKey(string? value)
        => value is { Length: 64 }
            && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
}

internal sealed record RecoveryBundleAttribution
{
    private RecoveryBundleAttribution(
        RecoveryBundleProducer producer,
        RecoveryBundleOperation operation,
        RecoveryBundleSubject subject)
    {
        Producer = producer;
        Operation = operation;
        Subject = subject;
    }

    internal RecoveryBundleProducer Producer { get; }

    internal RecoveryBundleOperation Operation { get; }

    internal RecoveryBundleSubject Subject { get; }

    internal static RecoveryBundleAttribution Create(
        RecoveryBundleProducer producer,
        RecoveryBundleOperation operation,
        CliWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (!IsValidTuple(producer, operation))
        {
            throw new ArgumentException(
                "The recovery producer and operation combination is not defined for schema v1.",
                nameof(operation));
        }

        var normalizedWorkspace = WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot);
        return new RecoveryBundleAttribution(
            producer,
            operation,
            RecoveryBundleSubject.Workspace(WorkspaceIdentity.Key(normalizedWorkspace)));
    }

    internal static RecoveryBundleAttribution Read(
        RecoveryBundleProducer producer,
        RecoveryBundleOperation operation,
        RecoveryBundleSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        if (!IsValidTuple(producer, operation)
            || subject.Kind != RecoveryBundleSubjectKind.Workspace)
        {
            throw new ArgumentException(
                "The recovery attribution is not an admissible schema-v1 tuple.",
                nameof(subject));
        }

        return new RecoveryBundleAttribution(producer, operation, subject);
    }

    private static bool IsValidTuple(
        RecoveryBundleProducer producer,
        RecoveryBundleOperation operation)
        => (producer, operation) switch
        {
            (RecoveryBundleProducer.Framework, RecoveryBundleOperation.Install) => true,
            (RecoveryBundleProducer.Framework, RecoveryBundleOperation.Update) => true,
            (RecoveryBundleProducer.Extension, RecoveryBundleOperation.Install) => true,
            (RecoveryBundleProducer.Extension, RecoveryBundleOperation.Update) => true,
            (RecoveryBundleProducer.Extension, RecoveryBundleOperation.Remove) => true,
            (RecoveryBundleProducer.Index, RecoveryBundleOperation.Index) => true,
            (RecoveryBundleProducer.Route, RecoveryBundleOperation.Create) => true,
            (RecoveryBundleProducer.Route, RecoveryBundleOperation.Init) => true,
            (RecoveryBundleProducer.Route, RecoveryBundleOperation.Move) => true,
            (RecoveryBundleProducer.Route, RecoveryBundleOperation.Update) => true,
            (RecoveryBundleProducer.Route, RecoveryBundleOperation.Remove) => true,
            (RecoveryBundleProducer.Repair, RecoveryBundleOperation.Repair) => true,
            (RecoveryBundleProducer.Library, RecoveryBundleOperation.Attach) => true,
            (RecoveryBundleProducer.Library, RecoveryBundleOperation.Sync) => true,
            (RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach) => true,
            _ => false,
        };
}

using System.Collections.Frozen;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Serialization;

internal static class RecoveryBundleAttributionCodec
{
    private const string WorkspaceSubject = "workspace";
    private static readonly FrozenDictionary<RecoveryBundleProducer, string> ProducerWire =
        new Dictionary<RecoveryBundleProducer, string>
        {
            [RecoveryBundleProducer.Framework] = "framework",
            [RecoveryBundleProducer.Extension] = "extension",
            [RecoveryBundleProducer.Index] = "index",
            [RecoveryBundleProducer.Route] = "route",
            [RecoveryBundleProducer.Repair] = "repair",
            [RecoveryBundleProducer.Library] = "library",
        }.ToFrozenDictionary();
    private static readonly FrozenDictionary<string, RecoveryBundleProducer> WireProducer =
        ProducerWire.ToFrozenDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);
    private static readonly FrozenDictionary<RecoveryBundleOperation, string> OperationWire =
        new Dictionary<RecoveryBundleOperation, string>
        {
            [RecoveryBundleOperation.Install] = "install",
            [RecoveryBundleOperation.Index] = "index",
            [RecoveryBundleOperation.Create] = "create",
            [RecoveryBundleOperation.Init] = "init",
            [RecoveryBundleOperation.Move] = "move",
            [RecoveryBundleOperation.Update] = "update",
            [RecoveryBundleOperation.Remove] = "remove",
            [RecoveryBundleOperation.Repair] = "repair",
            [RecoveryBundleOperation.Attach] = "attach",
            [RecoveryBundleOperation.Sync] = "sync",
            [RecoveryBundleOperation.Detach] = "detach",
        }.ToFrozenDictionary();
    private static readonly FrozenDictionary<string, RecoveryBundleOperation> WireOperation =
        OperationWire.ToFrozenDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);

    internal static RecoveryBundleAttributionV1 Serialize(
        RecoveryBundleAttribution attribution)
    {
        ArgumentNullException.ThrowIfNull(attribution);
        return new RecoveryBundleAttributionV1
        {
            Producer = Producer(attribution.Producer),
            Operation = Operation(attribution.Operation),
            Subject = new RecoveryBundleSubjectV1
            {
                Kind = Subject(attribution.Subject.Kind),
                Identity = attribution.Subject.Identity,
            },
        };
    }

    internal static bool TryRead(
        RecoveryBundleAttributionV1? document,
        out RecoveryBundleAttribution? attribution)
    {
        attribution = null;
        if (document?.Subject is not { } subject
            || document.Producer is null
            || !WireProducer.TryGetValue(document.Producer, out var producer)
            || document.Operation is null
            || !WireOperation.TryGetValue(document.Operation, out var operation)
            || !string.Equals(subject.Kind, WorkspaceSubject, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            attribution = RecoveryBundleAttribution.Read(
                producer,
                operation,
                RecoveryBundleSubject.Workspace(subject.Identity));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string Producer(RecoveryBundleProducer producer)
        => ProducerWire.TryGetValue(producer, out var wire)
            ? wire
            : throw new ArgumentOutOfRangeException(
                nameof(producer),
                producer,
                "The recovery producer is not defined.");

    private static string Operation(RecoveryBundleOperation operation)
        => OperationWire.TryGetValue(operation, out var wire)
            ? wire
            : throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "The recovery operation is not defined.");

    private static string Subject(RecoveryBundleSubjectKind subject)
        => subject switch
        {
            RecoveryBundleSubjectKind.Workspace => WorkspaceSubject,
            _ => throw new ArgumentOutOfRangeException(
                nameof(subject),
                subject,
                "The recovery subject kind is not defined."),
        };

}

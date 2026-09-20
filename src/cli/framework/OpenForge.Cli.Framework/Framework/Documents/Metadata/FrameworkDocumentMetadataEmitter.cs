using System.Globalization;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

internal sealed class FrameworkDocumentMetadataEmitter
{
    private readonly ISerializer _serializer = new StaticSerializerBuilder(
            new FrameworkMetadataYamlContext())
        .WithQuotingNecessaryStrings()
        .WithEventEmitter(next => new FrameworkMetadataTagsFlowStyleEmitter(next))
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .WithNewLine("\n")
        .Build();

    internal string Emit(FrameworkDocumentMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return Serialize(new FrameworkAuthoredMetadataYamlDocument
        {
            OpenForge = new FrameworkOpenForgeMetadataYamlModel
            {
                Description = metadata.Description,
                Tags = metadata.Tags.ToArray(),
                Responsibility = metadata.Responsibility,
            },
        });
    }

    internal string EmitOptional(FrameworkDocumentMetadataEmission metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return Serialize(new FrameworkAuthoredMetadataYamlDocument
        {
            OpenForge = new FrameworkOpenForgeMetadataYamlModel
            {
                Description = metadata.Description,
                Tags = metadata.Tags.IsEmpty ? null : metadata.Tags.ToArray(),
                Responsibility = metadata.Responsibility,
            },
        });
    }

    private string Serialize(FrameworkAuthoredMetadataYamlDocument document)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        _serializer.Serialize(
            writer,
            document,
            typeof(FrameworkAuthoredMetadataYamlDocument));
        return writer.ToString();
    }

    private sealed class FrameworkMetadataTagsFlowStyleEmitter(IEventEmitter next)
        : ChainedEventEmitter(next)
    {
        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            if (eventInfo.Source.StaticType == typeof(string[]))
            {
                eventInfo.Style = SequenceStyle.Flow;
            }

            base.Emit(eventInfo, emitter);
        }
    }
}

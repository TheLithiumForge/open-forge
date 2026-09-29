using System.Globalization;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

internal sealed class FrameworkDocumentMetadataEmitter
{
    internal string Emit(FrameworkDocumentMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var applyTo = metadata.ApplyTo.IsEmpty ? null : metadata.ApplyTo.Select(pattern => pattern.Text).ToArray();
        return Serialize(new FrameworkAuthoredMetadataYamlDocument
        {
            OpenForge = new FrameworkOpenForgeMetadataYamlModel
            {
                Description = metadata.Description,
                Tags = metadata.Tags.ToArray(),
                Responsibility = metadata.Responsibility,
                ApplyTo = applyTo,
            },
        }, applyTo);
    }

    internal string EmitOptional(FrameworkDocumentMetadataEmission metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var applyTo = metadata.ApplyTo.IsEmpty ? null : metadata.ApplyTo.Select(pattern => pattern.Text).ToArray();
        return Serialize(new FrameworkAuthoredMetadataYamlDocument
        {
            OpenForge = new FrameworkOpenForgeMetadataYamlModel
            {
                Description = metadata.Description,
                Tags = metadata.Tags.IsEmpty ? null : metadata.Tags.ToArray(),
                Responsibility = metadata.Responsibility,
                ApplyTo = applyTo,
            },
        }, applyTo);
    }

    private static string Serialize(FrameworkAuthoredMetadataYamlDocument document, string[]? applyTo)
    {
        var serializer = new StaticSerializerBuilder(new FrameworkMetadataYamlContext())
            .WithQuotingNecessaryStrings()
            .WithEventEmitter(next => new FrameworkMetadataTagsFlowStyleEmitter(next, applyTo))
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithNewLine("\n")
            .Build();
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        serializer.Serialize(
            writer,
            document,
            typeof(FrameworkAuthoredMetadataYamlDocument));
        return writer.ToString();
    }

    private sealed class FrameworkMetadataTagsFlowStyleEmitter(IEventEmitter next, string[]? applyTo)
        : ChainedEventEmitter(next)
    {
        private bool _inApplyTo;

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            if (ReferenceEquals(eventInfo.Source.Value, applyTo))
            {
                _inApplyTo = true;
                eventInfo.Style = SequenceStyle.Flow;
            }
            else if (eventInfo.Source.StaticType == typeof(string[]))
            {
                eventInfo.Style = SequenceStyle.Flow;
            }

            base.Emit(eventInfo, emitter);
        }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            if (_inApplyTo)
            {
                eventInfo.Style = ScalarStyle.DoubleQuoted;
            }

            base.Emit(eventInfo, emitter);
        }

        public override void Emit(SequenceEndEventInfo eventInfo, IEmitter emitter)
        {
            base.Emit(eventInfo, emitter);
            _inApplyTo = false;
        }
    }
}

using System.Globalization;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

internal sealed class FrameworkDocumentMetadataEmitter
{
    internal string Emit(FrameworkDocumentMetadata metadata, FrontmatterForm form)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var applyTo = metadata.ApplyTo.IsEmpty ? null : metadata.ApplyTo.Select(pattern => pattern.Text).ToArray();
        return Serialize(new FrameworkOpenForgeMetadataYamlModel
        {
            Description = metadata.Description,
            Tags = metadata.Tags.ToArray(),
            Responsibility = metadata.Responsibility,
            ApplyTo = applyTo,
        }, form);
    }

    internal string EmitOptional(FrameworkDocumentMetadataEmission metadata, FrontmatterForm form)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var applyTo = metadata.ApplyTo.IsEmpty ? null : metadata.ApplyTo.Select(pattern => pattern.Text).ToArray();
        return Serialize(new FrameworkOpenForgeMetadataYamlModel
        {
            Description = metadata.Description,
            Tags = metadata.Tags.IsEmpty ? null : metadata.Tags.ToArray(),
            Responsibility = metadata.Responsibility,
            ApplyTo = applyTo,
        }, form);
    }

    private static string Serialize(FrameworkOpenForgeMetadataYamlModel metadata, FrontmatterForm form)
    {
        var (document, type) = form switch
        {
            FrontmatterForm.Scoped => ((object)new FrameworkAuthoredMetadataYamlDocument { OpenForge = metadata }, typeof(FrameworkAuthoredMetadataYamlDocument)),
            FrontmatterForm.Root => ((object)metadata, typeof(FrameworkOpenForgeMetadataYamlModel)),
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The frontmatter form is not defined."),
        };
        var serializer = new StaticSerializerBuilder(new FrameworkMetadataYamlContext())
            .WithQuotingNecessaryStrings()
            .WithEventEmitter(next => new FrameworkMetadataTagsFlowStyleEmitter(next, metadata.ApplyTo))
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithNewLine("\n")
            .Build();
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        serializer.Serialize(
            writer,
            document,
            type);
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

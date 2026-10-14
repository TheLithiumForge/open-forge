using System.Globalization;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Reading;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;

internal static class FrameworkMetadataCompletionEmitter
{
    private const string OpenForgeKey = "open-forge";

    internal static string Emit(FrameworkDocumentMetadataEmission metadata, FrontmatterForm form, bool flow)
    {
        if (!flow) return new FrameworkDocumentMetadataEmitter().EmitOptional(metadata, form);
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        var emitter = new Emitter(writer);
        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());
        emitter.Emit(new MappingStart(null, null, true, MappingStyle.Flow));
        if (form == FrontmatterForm.Scoped)
        {
            emitter.Emit(new Scalar(OpenForgeKey));
            emitter.Emit(new MappingStart(null, null, true, MappingStyle.Flow));
        }

        if (metadata.Description is { } description)
        {
            emitter.Emit(new Scalar(FrameworkMetadataFieldReader.Description));
            emitter.Emit(new Scalar(null, null, description, ScalarStyle.DoubleQuoted, true, true));
        }

        if (!metadata.Tags.IsEmpty)
        {
            emitter.Emit(new Scalar(FrameworkMetadataFieldReader.Tags));
            emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Flow));
            foreach (var tag in metadata.Tags) emitter.Emit(new Scalar(tag));
            emitter.Emit(new SequenceEnd());
        }

        if (form == FrontmatterForm.Scoped) emitter.Emit(new MappingEnd());
        emitter.Emit(new MappingEnd());
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new StreamEnd());
        var mapping = writer.ToString().TrimEnd('\r', '\n');
        return mapping[1..^1];
    }
}

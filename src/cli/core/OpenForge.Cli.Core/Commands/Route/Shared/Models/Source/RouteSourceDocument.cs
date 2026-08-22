using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal enum RouteSourceForm
{
    Loader,
    CanonicalEntrypoint,
    IndexEntrypoint,
    UnderscoreIndexEntrypoint,
    ReferencesEntrypoint,
    UnderscoreReferencesEntrypoint,
    Skill,
    Markdown,
    OverwriteCompanion,
}

internal sealed class RouteSourceDocument
{
    internal RouteSourceDocument(
        string canonicalLogicalPath,
        string physicalPath,
        RouteSourceForm form,
        FileReadState readState,
        string? body)
    {
        if (!RouteLogicalPath.IsCanonical(canonicalLogicalPath))
        {
            throw new ArgumentException("The source document logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!Path.IsPathRooted(physicalPath)
            || !string.Equals(normalizedPhysicalPath, physicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The source document physical path must be absolute and normalized.", nameof(physicalPath));
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The source document form is not defined.");
        }

        if (!RouteSourceFormClassifier.Matches(canonicalLogicalPath, form))
        {
            throw new ArgumentException("The source document form does not match its canonical logical path.", nameof(form));
        }
        if (!Enum.IsDefined(readState))
        {
            throw new ArgumentOutOfRangeException(nameof(readState), readState, "The file read state is not defined.");
        }

        if ((readState == FileReadState.Complete) != (body is not null))
        {
            throw new ArgumentException("A complete source document read must carry a body, and only it may carry a body.", nameof(body));
        }

        CanonicalLogicalPath = canonicalLogicalPath;
        PhysicalPath = normalizedPhysicalPath;
        Form = form;
        ReadState = readState;
        Body = body;
    }

    internal string CanonicalLogicalPath { get; }

    internal string PhysicalPath { get; }

    internal RouteSourceForm Form { get; }

    internal FileReadState ReadState { get; }

    internal string? Body { get; }

}

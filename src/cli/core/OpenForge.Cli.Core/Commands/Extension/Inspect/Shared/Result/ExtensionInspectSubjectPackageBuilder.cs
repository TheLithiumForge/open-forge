using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectSubjectPackageBuilder
{
    internal static ExtensionInspectSubjectPackageFacts Build(
        ExtensionInspectSubjectPackageInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var subjectId = input.Request.StableId;
        var inspectSource = ExtensionInspectSubjectSourceBuilder.ReadSource(input.Request, input.Source);
        var inspectLifecycle = ExtensionInspectPackageLifecycleBuilder.ReadLifecycle(input.Lifecycle);
        var sourceCandidates = ExtensionInspectSubjectSourceBuilder.ReadCandidates(input.Source.Packages, subjectId);
        var sourceMatches = ExtensionInspectSubjectSourceBuilder.ReadSourceMatchCount(input.Source, subjectId);
        var selection = ExtensionInspectSubjectSourceBuilder.ReadSelection(input, sourceMatches);
        var subject = ExtensionInspectSubjectSourceBuilder.ReadSubject(
            subjectId,
            sourceCandidates,
            selection.InstalledPackage,
            sourceMatches,
            input.Source.State);
        var installed = ExtensionInspectPackageLifecycleBuilder.ReadInstalled(
            input.Lifecycle,
            selection.InstalledPackage);
        var available = ExtensionInspectPackageLifecycleBuilder.ReadAvailable(
            input.Source,
            selection.AvailablePackage,
            sourceMatches);
        ExtensionInspectPackageLifecycleBuilder.AddBoundaryFindings(input, selection, sourceMatches);

        return new ExtensionInspectSubjectPackageFacts
        {
            Subject = subject,
            Source = inspectSource,
            Lifecycle = inspectLifecycle,
            Installed = installed,
            Available = available,
            Selection = selection,
        };
    }

    internal static ExtensionInspectEventSubjectPackageFacts BuildEvent(
        ExtensionInspectEventSubjectPackageInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var selection = ExtensionInspectSubjectSourceBuilder.ReadEventSelection(input);
        var sourceMatches = input.Source is null
            ? 0
            : ExtensionInspectSubjectSourceBuilder.ReadSourceMatchCount(input.Source, input.Request.StableId);
        var subject = ExtensionInspectSubjectSourceBuilder.ReadEventSubject(input, selection, sourceMatches);

        return new ExtensionInspectEventSubjectPackageFacts
        {
            Subject = subject,
            Source = input.Source is null
                ? null
                : ExtensionInspectSubjectSourceBuilder.ReadSource(input.Request, input.Source),
            Lifecycle = input.Lifecycle is null
                ? null
                : ExtensionInspectPackageLifecycleBuilder.ReadLifecycle(input.Lifecycle),
            Installed = input.Lifecycle is null
                ? null
                : ExtensionInspectPackageLifecycleBuilder.ReadInstalled(
                    input.Lifecycle,
                    selection.InstalledPackage),
            Available = input.Source is null
                ? null
                : ExtensionInspectPackageLifecycleBuilder.ReadAvailable(
                    input.Source,
                    selection.AvailablePackage,
                    sourceMatches),
        };
    }

    internal static string ReadSourceIdentity(ExtensionSourceReadResult source)
        => ExtensionInspectSubjectSourceBuilder.ReadSourceIdentity(source);
}

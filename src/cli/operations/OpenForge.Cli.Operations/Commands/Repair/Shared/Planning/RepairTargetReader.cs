using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal sealed class RepairTargetReader
{
    private readonly PhysicalPathResolver _physicalPaths = new();

    internal async ValueTask<SourceLinkDestinationFacts> ResolveAsync(
        CliWorkspace workspace, string sourcePath, RepairTargetSelection target, CancellationToken cancellationToken)
    {
        var session = await new SourceReadSessionReader(_physicalPaths).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        var resolver = new SourceLinkDestinationResolver(
            (selectedWorkspace, lexicalPath) => _physicalPaths.ResolveCandidate(
                selectedWorkspace.LexicalRoot, selectedWorkspace.PhysicalRoot, lexicalPath),
            StrictUtf8FileReader.ReadAsync, new MarkdownDocumentParser().Parse);
        return await resolver.ResolveAsync(new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = sourcePath,
            RawDestination = RepairDestinationFormatter.Format(sourcePath, target),
        }, cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<IReadOnlyList<FileExpectation>?> ReadAsync(RepairPlan plan, CancellationToken cancellationToken)
    {
        var expectations = new List<FileExpectation>();
        foreach (var selected in plan.Selection.Selected)
        {
            var target = selected.Resolution.Target;
            var facts = await ResolveAsync(plan.Request.Workspace, selected.Proposal.SourceCanonicalPath, target, cancellationToken)
                .ConfigureAwait(false);
            if (facts.Target.Resolution != SourceLinkTargetResolution.Complete || facts.Target.PhysicalPath is not { } physicalPath
                || facts.Target.Path != Uri.UnescapeDataString(target.CanonicalTargetPath))
            {
                return null;
            }

            var logicalPath = Path.Combine(plan.Request.Workspace.LexicalRoot,
                Uri.UnescapeDataString(target.CanonicalTargetPath).Replace('/', Path.DirectorySeparatorChar));
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            expectations.Add(FileStateSnapshot.File(logicalPath, physicalPath, bytes).Expectation);
        }

        return expectations;
    }

    internal async ValueTask<bool> ValidateAsync(
        RepairPlan plan, IReadOnlyList<FileExpectation> targets, CancellationToken cancellationToken)
    {
        var validator = new FileExpectationValidator(_physicalPaths);
        foreach (var target in targets)
        {
            var result = await validator.ValidateAsync(plan.Request.Workspace, target, cancellationToken).ConfigureAwait(false);
            if (result.State != FileExpectationValidationState.Matched)
            {
                return false;
            }
        }

        return true;
    }
}

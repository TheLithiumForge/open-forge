using OpenForge.Cli.Core.Commands.Export.Models.Copy;
using OpenForge.Cli.Core.Commands.Export.Shared.Copy;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Export;

/// <summary>
/// Writes the shared plan to a destination folder and never changes the project. The export
/// Interface and Behavior contracts define the destination, the copy and what a failed write leaves.
/// </summary>
internal sealed class ExportOperation(ExportFileList files)
{
    private readonly PhysicalPathResolver _resolver = new();

    internal async ValueTask<StripResult> ExecuteAsync(StripRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var root = Path.GetFullPath(request.Workspace.LexicalRoot);
        var destination = Path.GetFullPath(request.Destination ?? throw new InvalidOperationException("A bound export request requires a destination."));
        if (DestinationProblem(root, destination) is { } problem)
        {
            return StripResultFactory.Invalid(StripCommandKind.Export, request.Workspace, StripResultFactory.Cannot(StripCommandKind.Export, problem))
                with
            { Destination = destination };
        }

        var built = await new StripPlanBuilder(_resolver).BuildAsync(request, token).ConfigureAwait(false);
        if (built.Plan is not { } plan)
        {
            return (built.Stopped ?? throw new InvalidOperationException("A stopped plan needs its result.")) with { Destination = destination };
        }

        ExportCopy? listed;
        try
        {
            listed = await ExportCopyPlanner.PlanAsync(plan, root, destination, files, token).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return StripResultFactory.Stop(request, CliSemanticStatus.Blocked,
                StripResultFactory.Error(StripFindingCode.WorkspaceUnavailable, $"The project's files cannot be listed: {exception.Message}")) with
            { Destination = destination };
        }

        if (listed is not { } copy)
        {
            return StripResultFactory.Stop(request, CliSemanticStatus.Blocked,
                StripResultFactory.Error(StripFindingCode.WorkspaceUnavailable, "The project's files cannot be listed.")) with
            { Destination = destination };
        }

        var linkedDocuments = plan.Findings.Where(finding => finding.Code == StripFindingCode.LinkedFileSkipped).Select(finding => finding.Path).ToHashSet(StringComparer.Ordinal);
        var result = StripResultFactory.Preview(plan) with
        {
            Plan = copy.Entries,
            Destination = destination,
            Copied = copy.Files.Count(file => file.Bytes is null),
            Findings = [.. plan.Findings, .. copy.Linked.Where(path => !linkedDocuments.Contains(path))
                .Select(path => StripResultFactory.Info(StripFindingCode.LinkedFileSkipped, $"'{path}' is a link and was left out of the copy.", path))],
            Verified = false,
            Next = null,
        };
        if (request.DryRun)
        {
            return result with { Next = new CliNextAction(Rerun(request), "Write the copy.") { AppliesRequest = true } };
        }

        var written = 0;
        try
        {
            Directory.CreateDirectory(destination);
            foreach (var file in copy.Files)
            {
                token.ThrowIfCancellationRequested();
                var target = Path.Combine(destination, file.Target.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                if (file.Bytes is { } bytes)
                {
                    File.WriteAllBytes(target, bytes.AsSpan());
                }
                else
                {
                    File.Copy(Path.Combine(root, file.Source.Replace('/', Path.DirectorySeparatorChar)), target, overwrite: false);
                }

                written++;
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return Stopped(request, result, written, CliSemanticStatus.Interrupted, StripFindingCode.Interrupted,
                $"Export was cancelled after writing {written} of {copy.Files.Length} files.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Stopped(request, result, written, CliSemanticStatus.Failed, StripFindingCode.WriteFailed,
                $"{exception.Message} Export stopped after writing {written} of {copy.Files.Length} files.");
        }

        return result with { Outcome = StripRunOutcome.Done, Verified = true };
    }

    // The destination must be a new or empty folder that does not hold the project. Inside the project is allowed.
    private static string? DestinationProblem(string root, string destination)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var folder = Path.TrimEndingDirectorySeparator(destination);
        var project = Path.TrimEndingDirectorySeparator(root);
        if (File.Exists(destination))
        {
            return $"'{destination}' is a file. Choose a new or empty folder, ideally outside the project.";
        }

        if (string.Equals(folder, project, comparison) || project.StartsWith(folder + Path.DirectorySeparatorChar, comparison))
        {
            return $"'{destination}' holds the project. Choose a new or empty folder outside it.";
        }

        return Directory.Exists(destination) && Directory.EnumerateFileSystemEntries(destination).Any()
            ? $"'{destination}' is not empty. Choose a new or empty folder, ideally outside the project."
            : null;
    }

    private static string Rerun(StripRequest request) => StripResultFactory.RetryCommand(request, automatic: false, dryRun: false);

    private static StripResult Stopped(StripRequest request, StripResult result, int written, CliSemanticStatus status, StripFindingCode code, string cause)
        => result with
        {
            Status = status,
            Outcome = written == 0 ? StripRunOutcome.NotStarted : StripRunOutcome.Unknown,
            Findings = [StripResultFactory.Error(code, $"{cause} Delete {result.Destination} and run the same command again."), .. result.Findings],
            Next = new CliNextAction(Rerun(request), $"Delete {result.Destination}, then run the same command again."),
        };
}

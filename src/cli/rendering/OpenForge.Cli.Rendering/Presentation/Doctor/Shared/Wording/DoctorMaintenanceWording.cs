using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.OutputText.Doctor;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;

internal static class DoctorMaintenanceWording
{
    internal static string Message(DoctorFinding finding, DoctorMaintenanceAdvice advice)
    {
        var path = finding.Subject.Path ?? "the reported file";
        return advice.Condition switch
        {
            DoctorMaintenanceCondition.MissingFields => DoctorMaintenanceText.MissingFields(path),
            DoctorMaintenanceCondition.ChainGap => DoctorMaintenanceText.ChainGap(path),
            DoctorMaintenanceCondition.OutsideFolders => DoctorMaintenanceText.OutsideFolders(path),
            DoctorMaintenanceCondition.ProvenRename => Proof(advice),
            DoctorMaintenanceCondition.SeveralStrongCandidates => DoctorMaintenanceText.SeveralStrong(advice.OldPath ?? path),
            DoctorMaintenanceCondition.WeakOnly => DoctorMaintenanceText.WeakOnly(advice.OldPath ?? path),
            DoctorMaintenanceCondition.NoCandidate => DoctorMaintenanceText.NoCandidate(advice.OldPath ?? path),
            DoctorMaintenanceCondition.MalformedFrontmatter => DoctorMaintenanceText.Malformed(path, advice.Cause),
            DoctorMaintenanceCondition.OldNavigation => DoctorMaintenanceText.OldNavigation(path),
            DoctorMaintenanceCondition.ManagedMetadata => DoctorMaintenanceText.ManagedMetadata(path),
            DoctorMaintenanceCondition.NativeMetadata => DoctorMaintenanceText.NativeMetadata(path),
            DoctorMaintenanceCondition.NativeResourceMetadata => DoctorMaintenanceText.NativeResourceMetadata(path),
            DoctorMaintenanceCondition.RestorationBoundary => DoctorMaintenanceText.RestoreRoute(path),
            DoctorMaintenanceCondition.UnsafeBoundary => DoctorMaintenanceText.ReviewBoundary(path),
            DoctorMaintenanceCondition.ProtectedReference => ProtectedReferenceMessage(finding, advice, path),
            DoctorMaintenanceCondition.RenamedFragmentMissing => DoctorMaintenanceText.RenamedFragmentMissing(advice.OldPath ?? path,
                advice.NewPath ?? "the successor", advice.Fragment),
            _ => throw new ArgumentOutOfRangeException(nameof(advice), advice.Condition, "The maintenance condition is not defined."),
        };
    }

    internal static IReadOnlyList<CliNextAction> Actions(DoctorFinding finding, DoctorMaintenanceAdvice advice)
    {
        var path = finding.Subject.Path ?? "the reported file";
        var message = Message(finding, advice);
        var command = finding.Actions.FirstOrDefault()?.Command;
        if (command is not null)
        {
            var reason = advice.Condition switch
            {
                DoctorMaintenanceCondition.ProvenRename => DoctorMaintenanceText.RepairReason(),
                DoctorMaintenanceCondition.MissingFields or DoctorMaintenanceCondition.ChainGap => DoctorMaintenanceText.IndexReason(),
                DoctorMaintenanceCondition.SeveralStrongCandidates or DoctorMaintenanceCondition.WeakOnly => DoctorMaintenanceText.ChooseTarget(),
                DoctorMaintenanceCondition.RenamedFragmentMissing => DoctorMaintenanceText.ChooseFragment(),
                DoctorMaintenanceCondition.ProtectedReference => DoctorMaintenanceText.UpdateProtectedLink(),
                _ => message,
            };
            return [new(command, reason)];
        }
        var sentence = advice.Condition switch
        {
            DoctorMaintenanceCondition.OutsideFolders => DoctorMaintenanceText.MoveIntoRoute(path),
            DoctorMaintenanceCondition.NoCandidate => DoctorMaintenanceText.EditLink(path, finding.Subject.LocationView?.Line, finding.Subject.LocationView?.Column),
            DoctorMaintenanceCondition.MalformedFrontmatter => DoctorMaintenanceText.FixFrontmatter(path, advice.IndexCommand ?? "open-forge index"),
            DoctorMaintenanceCondition.OldNavigation => OldRulesAction(path, advice),
            DoctorMaintenanceCondition.NativeResourceMetadata => DoctorMaintenanceText.AddNativeResourceMetadata(path),
            DoctorMaintenanceCondition.ProtectedReference => advice.ProtectedByNativePackage && advice.ProtectedPath == path
                ? DoctorMaintenanceText.EditPackageLink(path)
                : DoctorMaintenanceText.EditLink(path, finding.Subject.LocationView?.Line, finding.Subject.LocationView?.Column),
            _ => message,
        };
        return [new(sentence, sentence) { Kind = CliNextActionKind.Sentence }];
    }

    private static string ProtectedReferenceMessage(DoctorFinding finding, DoctorMaintenanceAdvice advice, string path)
    {
        var protectedPath = advice.ProtectedPath ?? path;
        if (finding.Actions.Any(action => action.Command is not null)) return DoctorMaintenanceText.ManagedLink(protectedPath);
        if (advice.ProtectedByNativePackage) return DoctorMaintenanceText.NativeLink(protectedPath);
        return DoctorMaintenanceText.ProtectedLink(protectedPath);
    }

    private static string OldRulesAction(string path, DoctorMaintenanceAdvice advice)
    {
        var command = advice.IndexCommand ?? "open-forge index";
        if (advice.ExistingCanonicalPath is { } canonicalPath)
            return DoctorMaintenanceText.CopyOldRules(path, canonicalPath, command);
        return DoctorMaintenanceText.KeepOldRules(path, advice.CanonicalName ?? "the current navigation name", command);
    }

    private static string Proof(DoctorMaintenanceAdvice advice)
    {
        var basis = DoctorMaintenanceText.MetadataMatch();
        if (advice.SameContent) basis = DoctorMaintenanceText.SameContent();
        else if (advice.Similarity is { } similarity) basis = DoctorMaintenanceText.Unchanged(similarity);
        var oldPath = advice.OldPath ?? "the missing file";
        var newPath = advice.NewPath ?? "the proven successor";
        var oldRoot = RouteRoot(oldPath);
        var newRoot = RouteRoot(newPath);
        if (oldRoot == newRoot && oldRoot.Length > 0)
        {
            oldPath = oldPath[oldRoot.Length..];
            newPath = newPath[newRoot.Length..];
        }
        return DoctorMaintenanceText.Rename(oldPath, newPath, advice.Moved, basis);
    }

    private static string RouteRoot(string path)
    {
        if (!path.StartsWith(".agents/", StringComparison.Ordinal)) return string.Empty;
        var separator = path.IndexOf('/', ".agents/".Length);
        return separator < 0 ? string.Empty : path[..(separator + 1)];
    }
}

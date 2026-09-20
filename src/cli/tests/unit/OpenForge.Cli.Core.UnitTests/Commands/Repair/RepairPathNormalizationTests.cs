using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Update.Shared.Validation;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairPathNormalizationTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install Update Remove Inspect Doctor and Repair agree on awkward logical path forms"), Trait("Feature", "workspace-paths"), Trait("Evidence", "Unit")]
    public void CommandPathBoundariesShareOneLogicalAnswer()
    {
        var inputs = new[]
        {
            "./a/b",
            @"a\b",
            "a/b/",
            ".agents/x",
            "x",
            "a/b\\c",
        };
        var handlers = new (string Name, Func<string, string?> Read)[]
        {
            ("Install", InstallPath),
            ("Update", UpdatePath),
            ("Remove", RemovePath),
            ("Inspect", InspectPath),
            ("Doctor", DoctorPath),
            ("Repair", RepairPath),
        };

        foreach (var input in inputs)
        {
            var expected = PortableWorkspacePath.TryNormalize(input, out var normalized)
                ? normalized
                : null;

            foreach (var (name, read) in handlers)
            {
                var actual = read(input);
                Assert.True(
                    string.Equals(expected, actual, StringComparison.Ordinal),
                    $"{name} returned '{actual ?? "<rejected>"}' for '{input}', expected '{expected ?? "<rejected>"}'.");
            }
        }
    }

    private static string? InstallPath(string path)
    {
        try
        {
            return new InstallEffect(new InstallEffectInput
            {
                Path = path,
                Kind = InstallEffectKind.File,
                Action = InstallEffectAction.Create,
                SourceAssetPath = null,
                Outcome = InstallEffectOutcome.Planned,
                Residual = InstallEffectResidual.None,
            }).Path;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static string? UpdatePath(string path)
        => UpdateValueSyntax.IsCanonicalRelative(path) ? path : null;

    private static string? RemovePath(string path)
        => ExtensionDestinationPolicy.IsAllowed(path) ? path : null;

    private static string? InspectPath(string path)
    {
        var ownership = new WorkspaceOwnershipRead(
            WorkspaceOwnershipReadState.Complete,
            new WorkspaceOwnershipDocument(
                WorkspaceOwnershipDefinitions.SchemaVersion,
                Framework: null,
                Extensions:
                [
                    new ExtensionOwnership("toolkit", null, null, [], [path], []),
                ],
                Libraries: []),
            ".agents/open-forge.lock.json",
            Snapshot: null,
            Cause: null);
        return ExtensionInspectInstalledClosureReader.ReadOwnership(ownership).IsTrustworthy
            ? path
            : null;
    }

    private static string? DoctorPath(string path)
    {
        try
        {
            return WorkspaceRelativeEligiblePath.Create(path).Value;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static string? RepairPath(string path)
    {
        try
        {
            return RepairTargetSelection.Parse(path).CanonicalTargetPath;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}

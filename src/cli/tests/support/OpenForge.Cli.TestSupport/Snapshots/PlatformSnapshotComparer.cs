using System.Text.RegularExpressions;
using TheLithium.Imprint;

namespace OpenForge.Cli.TestSupport.Snapshots;

// Existing baselines were captured on Windows. Adapt only the expected physical
// path separators and the known held-lock error; received output stays untouched.
// Failure artifacts therefore retain the actual platform's bytes and error code.
public sealed partial class PlatformSnapshotComparer(bool heldLock, bool manifestSharing = false) : ISnapshotComparer
{
    public SnapshotComparisonResult Compare(
        string expected, string received, SnapshotFormat format, ResolvedSnapshotComparison options)
    {
        var platform = OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "macos" : "linux";
        return AdaptExpected(expected, platform, heldLock, manifestSharing).ReplaceLineEndings("\n") == received.ReplaceLineEndings("\n")
            ? SnapshotComparisonResult.Match
            : new SnapshotComparisonResult(false, "Output differs from the platform-specific expectation; see the unchanged received artifact.");
    }

    public static string AdaptExpected(string expected, string platform, bool heldLock, bool manifestSharing = false)
    {
        if (platform == "windows") return expected;
        if (platform is not ("linux" or "macos")) throw new ArgumentOutOfRangeException(nameof(platform));

        expected = PlaceholderPath().Replace(expected, match => match.Value
            .Replace("\\\\", "/", StringComparison.Ordinal).Replace('\\', '/'));
        if (heldLock)
        {
            // EWOULDBLOCK: Linux 11; Darwin 35 (bsd/sys/errno.h).
            var code = platform == "linux" ? "0x0000000B" : "0x00000023";
            expected = expected.Replace("IOException (0x80070020): The filesystem operation failed.",
                $"IOException ({code}): The filesystem operation failed.", StringComparison.Ordinal);
        }
        if (manifestSharing)
        {
            // Manifest classification recognizes Windows sharing HRESULTs only.
            // Keep the raw OS evidence intact; adapt the two typed output facts.
            expected = expected.Replace("could not be read because it is in use.",
                "could not be read because a filesystem operation failed.", StringComparison.Ordinal)
                .Replace("Close the program holding the file, then retry.",
                    "Check that the file is accessible, then retry.", StringComparison.Ordinal);
        }
        return expected;
    }

    [GeneratedRegex("""<(?:workspace|extension-source|recovery-bundle|temp)>[\\/][^\s"',;)\]}]*""", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderPath();
}

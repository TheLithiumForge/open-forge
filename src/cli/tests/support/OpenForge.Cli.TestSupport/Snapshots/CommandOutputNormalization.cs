using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenForge.Cli.TestSupport.Snapshots;

public static partial class CommandOutputNormalization
{
    // Caller-supplied machine/run coordinates are matched in raw and JSON-encoded form at a
    // delimiter, so an adjacent path identity remains evidence instead of becoming the same
    // placeholder. The temporary root is the one coordinate taken from the environment rather than
    // the caller: a clamped diagnostic can leave a fragment no caller-supplied path can claim, and
    // that fragment still carries the developer's home directory, which must never be committed.
    public static string Normalize(
        string output,
        string? workspacePath,
        string version,
        string? recoveryBundlePath = null,
        string? extensionSourcePath = null,
        bool normalizeTextPaths = false)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        var normalized = output;
        if (extensionSourcePath is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(extensionSourcePath);
            normalized = ReplacePath(normalized, extensionSourcePath, "<extension-source>");
        }

        if (recoveryBundlePath is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recoveryBundlePath);
            normalized = ReplacePath(normalized, recoveryBundlePath, "<recovery-bundle>");
        }

        if (workspacePath is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workspacePath);
            normalized = ReplacePath(normalized, workspacePath, "<workspace>");
        }

        // A clamped value can cut a caller-supplied path so early that no placeholder above can
        // claim the remainder. What still identifies the machine at that point is the temporary
        // root, which carries the developer's home directory, so collapse it once the specific
        // paths have had their chance and fail if anything machine-specific survives.
        normalized = ReplacePrefix(normalized, EncodedTemporaryRoot, "<temp>");
        normalized = ReplacePrefix(normalized, TemporaryRoot, "<temp>");
        foreach (var path in new[] { extensionSourcePath, recoveryBundlePath, workspacePath })
        {
            if (path is not null) AssertNoUnattributableClamp(normalized, path);
        }

        // Checked-in captures use LF; canonicalize CRLF and lone CR before any
        // optional text handling so this representation is portable.
        normalized = normalized
            .Replace(version, "<version>", StringComparison.Ordinal)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        if (normalizeTextPaths)
        {
            normalized = NormalizeTextPaths(normalized);
        }

        return Sha256().Replace(normalized, "<sha256>");
    }

    public static string NormalizeRecoveryPaths(string output, IReadOnlyList<string> candidates, string? store = null)
    {
        // Cleanup and Repair expose several observed bundles. Keep each
        // candidate's ordinal visible so distinct recovery identities survive.
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(candidates);
        for (var index = 0; index < candidates.Count; index++)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(candidates[index]);
            output = ReplacePath(output, candidates[index], $"<recovery-bundle-{index + 1}>");
        }

        return store is null ? output : ReplacePath(output, store, "<recovery-store>");
    }

    public static string NormalizeCleanupLeaseIdentity(string output, Guid operationId)
    {
        // The generated lease member is volatile; other identifiers remain
        // diagnostic evidence and must not be scrubbed with it.
        ArgumentNullException.ThrowIfNull(output);
        return output
            .Replace($"\"operationId\":\"{operationId:N}\"", "\"operationId\":\"<cleanup-lease-id>\"", StringComparison.Ordinal)
            .Replace($"\"operationId\": \"{operationId:N}\"", "\"operationId\": \"<cleanup-lease-id>\"", StringComparison.Ordinal);
    }

    // Two unit snapshot helpers need the same boundary rule as a full capture without the rest of
    // Normalize's run-coordinate handling. Exposing the delimited replacement on its own keeps one
    // definition of where a root ends, instead of a second unguarded `String.Replace` per helper.
    public static string ReplaceDelimitedPath(string output, string path, string placeholder)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(placeholder);
        return ReplaceDelimited(
            ReplaceDelimited(output, JsonEncodedText.Encode(path).ToString(), placeholder), path, placeholder);
    }

    private static string ReplacePath(string output, string path, string placeholder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var encoded = JsonEncodedText.Encode(path).ToString();
        var replaced = ReplaceDelimited(ReplaceDelimited(output, encoded, placeholder), path, placeholder);
        return ReplaceClamped(ReplaceClamped(replaced, encoded, placeholder), path, placeholder);
    }

    private static string ReplaceClamped(string output, string path, string placeholder)
    {
        // `CliDiagnosticRenderer` clamps each debug diagnostic value to 240 characters and appends
        // `...`, which can cut a caller-supplied path in half. What survives is still the running
        // machine's real path, but it is no longer the whole value, so the delimited replacement
        // above cannot see it and the capture would carry a developer's home directory into the
        // repository. Match the longest prefix the clamp left behind instead, and keep the `...` so
        // the capture still records that the renderer clamped the value.
        var builder = new StringBuilder();
        var copied = 0;
        var search = 0;
        while (output.IndexOf(Clamp, search, StringComparison.Ordinal) is var marker && marker >= 0)
        {
            search = marker + Clamp.Length;
            var length = ClampedPrefixLength(output, marker, path);
            if (length == 0 || marker - length < copied)
            {
                continue;
            }

            builder.Append(output, copied, marker - length - copied).Append(placeholder);
            copied = marker;
        }

        return builder.Append(output, copied, output.Length - copied).ToString();
    }

    private static int ClampedPrefixLength(string output, int marker, string path)
    {
        // Only a prefix that reaches past the path's own parent directory identifies this path
        // rather than a sibling under the same temporary root, so that is the shortest match
        // allowed. `AssertNoUnattributableClamp` fails the capture when the clamp cut even earlier,
        // because that remainder cannot be attributed to one placeholder and must not be guessed.
        var minimum = path.LastIndexOfAny(Separators) + 2;
        var length = Math.Min(marker, path.Length);
        while (length >= minimum)
        {
            if (string.CompareOrdinal(output, marker - length, path, 0, length) == 0)
            {
                return length;
            }

            length--;
        }

        return 0;
    }

    private static void AssertNoUnattributableClamp(string output, string path)
    {
        var parent = path[..(path.LastIndexOfAny(Separators) + 1)];
        if (parent.Length == 0)
        {
            return;
        }

        var marker = output.IndexOf(Clamp, StringComparison.Ordinal);
        while (marker >= 0)
        {
            var length = Math.Min(marker, parent.Length);
            while (length > 0)
            {
                if (string.CompareOrdinal(output, marker - length, parent, 0, length) == 0
                    && parent.AsSpan(0, length).ContainsAny(Separators))
                {
                    throw new InvalidOperationException(
                        $"A clamped diagnostic value left an unattributable fragment of '{parent}' in a capture. "
                        + "Shorten the rendered value or widen the normalization rather than committing the path.");
                }

                length--;
            }

            marker = output.IndexOf(Clamp, marker + Clamp.Length, StringComparison.Ordinal);
        }
    }

    private static string ReplacePrefix(string output, string prefix, string placeholder)
    {
        // The temporary root has no delimiter after it when the clamp lands inside the directory
        // name it introduces, so this pass matches it wherever it appears rather than at a
        // boundary. It runs last, so an intact path has already become its own placeholder.
        return output.Replace(prefix, placeholder, StringComparison.Ordinal);
    }

    private static readonly string TemporaryRoot = Path.TrimEndingDirectorySeparator(Path.GetTempPath());

    private static readonly string EncodedTemporaryRoot = JsonEncodedText.Encode(TemporaryRoot).ToString();

    private const string Clamp = "...";

    private static readonly char[] Separators = ['/', '\\'];

    private static string ReplaceDelimited(string output, string path, string placeholder)
    {
        // Requiring a path delimiter prevents a root such as `source` from
        // consuming the distinct value `source-other`.
        var builder = new StringBuilder();
        var copied = 0;
        var search = 0;
        while (output.IndexOf(path, search, StringComparison.Ordinal) is var position && position >= 0)
        {
            var end = position + path.Length;
            if (end == output.Length || char.IsWhiteSpace(output[end]) || output[end] is '/' or '\\' or '"' or '\'' or '<' or '>' or ';' or ',' or ':' or ')' or ']' or '}' || IsSentencePeriod(output, end))
            {
                builder.Append(output, copied, position - copied).Append(placeholder);
                copied = end;
            }

            search = end;
        }

        return builder.Append(output, copied, output.Length - copied).ToString();
    }

    private static string NormalizeTextPaths(string output)
    {
        // Only the path that follows a placeholder is rewritten to forward slashes. A previous rule
        // rewrote every remaining backslash in the whole capture, which silently turned promised
        // content such as `literal\marker` into `literal/marker`: a capture is evidence of what a
        // command prints, so it may not quietly edit the text it is recording. Every placeholder a
        // path can start from belongs in this list, `<temp>` included, because that scoping is now
        // the only thing making a capture portable between a Windows and a POSIX checkout.
        var builder = new StringBuilder(output);
        foreach (var placeholder in new[] { "<workspace>", "<extension-source>", "<recovery-bundle>", "<temp>" })
        {
            var search = 0;
            while (builder.ToString().IndexOf(placeholder, search, StringComparison.Ordinal) is var position && position >= 0)
            {
                var end = position + placeholder.Length;
                while (end < builder.Length && !IsPathTerminator(builder[end])) end++;
                for (var index = position; index < end; index++)
                {
                    if (builder[index] == '\\') builder[index] = '/';
                }

                search = end;
            }
        }

        return builder.ToString();
    }

    private static bool IsPathTerminator(char value)
        => char.IsWhiteSpace(value) || value is '"' or '\'' or ',' or ';' or ')' or ']' or '}';

    private static bool AreHexDigits(StringBuilder value, int start)
    {
        for (var index = start; index < start + 4; index++)
        {
            if (!Uri.IsHexDigit(value[index])) return false;
        }

        return true;
    }

    private static bool IsSentencePeriod(string output, int position)
    {
        if (output[position] != '.') return false;
        var next = position + 1;
        return next == output.Length || char.IsWhiteSpace(output[next]) || output[next] is '"' or '\'';
    }

    // A fingerprint is recognised by the member or label that carries it, not by looking like one.
    // Matching on shape alone would silently rewrite any 64-character hex value a command prints,
    // such as an id or a workspace name of that form, and the capture would then assert the wrong
    // value for good. These are every anchor the corpus actually uses; a new one must be added here
    // rather than by relaxing the rule.
    [GeneratedRegex(
        @"(?<=file:|""[A-Za-z]*(?:[Ss]ha256|[Ff]ingerprint)""\s*:\s*""|""(?:before|after)""\s*:\s*""|SHA-256:?\s|[Ff]ingerprint[:=]\s?|(?:Before|After):\s)[0-9a-fA-F]{64}(?![0-9a-fA-F])",
        RegexOptions.CultureInvariant)]
    private static partial Regex Sha256();
}

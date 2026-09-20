using System.Security.Cryptography;
using System.Text;

namespace OpenForge.Cli.TestSupport.Isolation;

/// <summary>
/// The one place a test decides where Open Forge keeps machine state: workspace locks and
/// recovery bundles. Every suite uses this instead of resolving the current user's profile,
/// because a test that writes into the signed-in user's profile leaks between runs, cannot be
/// cleaned up, and forces the whole suite to run one test at a time.
///
/// Locations come from the runtime: <see cref="Directory.CreateTempSubdirectory(string)"/> chooses
/// the platform's temporary directory and <see cref="System.IO.Path.Combine(string, string)"/>
/// joins segments. No test composes a platform-specific location itself, so there is nothing to
/// keep in step with Windows, Linux and macOS.
///
/// The store layout below is this fixture's own oracle. It is stated once here rather than
/// imported, so a change to the production layout fails a test instead of silently moving the
/// assertions with it. TestSupport deliberately does not reference the product.
/// </summary>
public sealed class TestDataHome : IDisposable
{
    /// <summary>Names an absolute directory the CLI uses instead of the user's profile.</summary>
    public const string Variable = "OPENFORGE_DATA_HOME";

    private const string ApplicationDirectoryName = "OpenForge";
    private const string RecoveryDirectoryName = "recovery";
    private const string LockDirectoryName = "locks";
    private const string StoreVersionName = "v1";
    private const string HomeDirectoryName = "home";

    private readonly string _root;
    private bool _disposed;

    private TestDataHome(string root, string path)
    {
        _root = root;
        Path = path;
    }

    /// <summary>
    /// The data home. It is named but deliberately not created: its absence is how a test proves
    /// a command wrote no machine state at all.
    /// </summary>
    public string Path { get; }

    public string RecoveryStoreRoot => System.IO.Path.Combine(
        Path,
        ApplicationDirectoryName,
        RecoveryDirectoryName,
        StoreVersionName);

    public string LockStoreRoot => System.IO.Path.Combine(
        Path,
        ApplicationDirectoryName,
        LockDirectoryName,
        StoreVersionName);

    public string ApplicationDirectory => System.IO.Path.Combine(Path, ApplicationDirectoryName);

    public string RecoveryDirectory => System.IO.Path.Combine(
        Path,
        ApplicationDirectoryName,
        RecoveryDirectoryName);

    public string RecoveryWorkspaceDirectory(string workspacePath)
        => System.IO.Path.Combine(RecoveryStoreRoot, WorkspaceKey(workspacePath));

    /// <summary>
    /// The environment that points a child process at this data home. Windows is not redirected
    /// by <c>LOCALAPPDATA</c>, because <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/>
    /// reads the Known Folder there, so the explicit override carries every platform and the
    /// others remain for anything that still reads them.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables(string? homeDirectory = null)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Variable] = Path,
            ["XDG_DATA_HOME"] = Path,
            ["LOCALAPPDATA"] = Path,
        };
        if (homeDirectory is not null)
        {
            values["HOME"] = homeDirectory;
        }

        return values;
    }

    public static TestDataHome Create(string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        var root = Directory.CreateTempSubdirectory($"open-forge-data-{Sanitize(purpose)}-").FullName;
        return new TestDataHome(root, System.IO.Path.Combine(root, HomeDirectoryName));
    }

    /// <summary>
    /// Points this whole test process away from the user's profile. In-process suites share one
    /// process and run their collections in parallel, so the redirection is made once before any
    /// test runs rather than per test. Call it from a module initializer.
    /// </summary>
    public static void RedirectCurrentProcess(string purpose)
    {
        var home = Create(purpose);
        Directory.CreateDirectory(home.Path);
        Environment.SetEnvironmentVariable(Variable, home.Path);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => home.Dispose();
    }

    /// <summary>The identity Open Forge derives from a workspace path to name its store folder.</summary>
    public static string WorkspaceKey(string workspacePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePath);
        var fullPath = System.IO.Path.GetFullPath(workspacePath);
        var trimmed = System.IO.Path.TrimEndingDirectorySeparator(fullPath);
        var normalized = string.IsNullOrEmpty(trimmed)
            ? System.IO.Path.GetPathRoot(fullPath)
                ?? throw new InvalidOperationException("A workspace identity requires a rooted path.")
            : trimmed;
        var identity = OperatingSystem.IsWindows() ? normalized.ToUpperInvariant() : normalized;
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    /// <summary>
    /// A unique path under the platform temporary directory that deliberately does not exist,
    /// for a test that needs an absent location. Nothing is created, so nothing needs cleaning up.
    /// </summary>
    public static string AbsentPath(string purpose, string extension = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        ArgumentNullException.ThrowIfNull(extension);
        return System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"open-forge-{Sanitize(purpose)}-{Guid.NewGuid():N}{extension}");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static string Sanitize(string purpose)
    {
        var builder = new StringBuilder(purpose.Length);
        foreach (var character in purpose)
        {
            builder.Append(char.IsAsciiLetterOrDigit(character) ? character : '-');
        }

        return builder.ToString();
    }
}

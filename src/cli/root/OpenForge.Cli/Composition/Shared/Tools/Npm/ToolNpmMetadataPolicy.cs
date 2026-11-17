using System.Net;
using OpenForge.Cli.Composition.Shared.Tools.Npm.Models;

namespace OpenForge.Cli.Composition.Shared.Tools.Npm;

internal sealed class ToolNpmMetadataPolicy(ToolNpmMetadataEnvironment environment)
{
    internal const string DefaultRegistry = "https://registry.npmjs.org/";
    internal const string ConfigFileName = ".npmrc";
    internal const string GlobalConfigFileName = "npmrc";
    internal const string ConfigPrefix = "NPM_CONFIG_";
    internal const string RegistryVariable = "NPM_CONFIG_REGISTRY";
    internal const string ProxyVariable = "NPM_CONFIG_PROXY";
    internal const string HttpsProxyVariable = "NPM_CONFIG_HTTPS_PROXY";
    internal const string StrictSslVariable = "NPM_CONFIG_STRICT_SSL";
    internal const string UserConfigVariable = "NPM_CONFIG_USERCONFIG";
    internal const string GlobalConfigVariable = "NPM_CONFIG_GLOBALCONFIG";
    internal const string PrefixVariable = "NPM_CONFIG_PREFIX";
    internal const string AppDataVariable = "APPDATA";
    internal const string NoProxyVariable = "NO_PROXY";
    internal const string NpmNoProxyVariable = "NPM_CONFIG_NOPROXY";
    internal static readonly IReadOnlyList<string> StandardProxyVariables = ["HTTPS_PROXY", "HTTP_PROXY", "ALL_PROXY"];
    internal static readonly IReadOnlySet<string> CredentialVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "NPM_TOKEN", "NODE_AUTH_TOKEN" };
    internal static readonly IReadOnlySet<string> TrustVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "NPM_CONFIG_CA", "NPM_CONFIG_CAFILE", "NPM_CONFIG_CERT", "NPM_CONFIG_KEY", "NODE_EXTRA_CA_CERTS", "SSL_CERT_FILE", "SSL_CERT_DIR" };

    internal ToolNpmMetadataConnection Read(string workspace)
    {
        if (!System.IO.Path.IsPathFullyQualified(workspace) || !System.IO.Path.IsPathFullyQualified(environment.UserHome))
            throw new InvalidOperationException("Npm metadata needs resolved workspace and user-home paths.");
        foreach (var key in environment.Variables.Keys)
        {
            if (CredentialVariables.Contains(key) || TrustVariables.Contains(key)
                || key.StartsWith(ConfigPrefix, StringComparison.OrdinalIgnoreCase)
                && (key.Contains("AUTH", StringComparison.OrdinalIgnoreCase) || key.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("USERNAME", StringComparison.OrdinalIgnoreCase) || key.Contains("CERT", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("KEY", StringComparison.OrdinalIgnoreCase) || key.Contains(":REGISTRY", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Npm metadata needs a native client for the configured authentication or trust settings.");
        }
        if (Value(StrictSslVariable) is { } strict && !strict.Equals("true", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Npm metadata won't bypass configured TLS verification. Use a native client for that configuration.");
        foreach (var config in ConfigurationPaths(workspace))
        {
            var file = new FileInfo(config);
            if (file.LinkTarget is not null || file.Exists && file.Length != 0)
                throw new InvalidOperationException("Npm configuration files need a native client. Their contents weren't opened for metadata lookup.");
            if (Directory.Exists(config)) throw new InvalidOperationException("An npm configuration path is unavailable.");
        }
        var registry = ReadAddress(Value(RegistryVariable) ?? DefaultRegistry, proxy: false);
        if (!registry.AbsolutePath.EndsWith('/')) registry = new(registry.AbsoluteUri + "/");
        var proxyAddress = Value(HttpsProxyVariable) ?? Value(ProxyVariable);
        IWebProxy? proxy = environment.DefaultProxy;
        foreach (var name in StandardProxyVariables)
            if (Value(name) is { } configured) _ = ReadAddress(configured, proxy: true);
        if (proxyAddress is not null)
        {
            if (Value(NoProxyVariable) is not null || Value(NpmNoProxyVariable) is not null)
                throw new InvalidOperationException("Npm proxy bypass configuration needs a native client.");
            proxy = new WebProxy(ReadAddress(proxyAddress, proxy: true));
        }
        else if (Value(NpmNoProxyVariable) is not null)
            throw new InvalidOperationException("Npm proxy bypass configuration needs a native client.");
        else if (proxy is null)
        {
            var configured = StandardProxyVariables.Select(Value).FirstOrDefault(value => value is not null);
            if (configured is not null)
            {
                if (Value(NoProxyVariable) is not null) throw new InvalidOperationException("Proxy bypass configuration needs the Root platform proxy.");
                proxy = new WebProxy(ReadAddress(configured, proxy: true));
            }
        }
        if (proxy?.Credentials is not null) throw new InvalidOperationException("Authenticated proxies need a native client for npm metadata.");
        return new(registry, proxy);
    }

    private string? Value(string name)
    {
        var keys = environment.Variables.Keys.Where(key => key.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        var values = keys.Select(key => environment.Variables[key]).Where(value => !string.IsNullOrEmpty(value)).Distinct(StringComparer.Ordinal).ToArray();
        if (values.Length > 1) throw new InvalidOperationException("Conflicting npm environment settings need a native client.");
        return values.FirstOrDefault();
    }

    private IEnumerable<string> ConfigurationPaths(string workspace)
    {
        yield return System.IO.Path.Combine(workspace, ConfigFileName);
        yield return System.IO.Path.Combine(environment.UserHome, ConfigFileName);
        foreach (var variable in new[] { UserConfigVariable, GlobalConfigVariable })
            if (Value(variable) is { } config) yield return System.IO.Path.GetFullPath(config, workspace);
        if (Value(PrefixVariable) is { } prefix) yield return System.IO.Path.Combine(System.IO.Path.GetFullPath(prefix, workspace), "etc", GlobalConfigFileName);
        foreach (var path in environment.AdditionalConfigPaths) yield return path;
    }

    internal static IReadOnlyList<string> GlobalConfigPaths(string userHome, IReadOnlyDictionary<string, string?> variables)
    {
        var root = System.IO.Path.GetPathRoot(userHome) ?? throw new IOException("The user-home filesystem root is unavailable.");
        if (!OperatingSystem.IsWindows())
            return [System.IO.Path.Combine(root, "etc", GlobalConfigFileName), System.IO.Path.Combine(root, "usr", "local", "etc", GlobalConfigFileName)];
        var appData = variables.FirstOrDefault(pair => pair.Key.Equals(AppDataVariable, StringComparison.OrdinalIgnoreCase)).Value;
        return appData is not null && System.IO.Path.IsPathFullyQualified(appData)
            ? [System.IO.Path.Combine(appData, "npm", "etc", GlobalConfigFileName)] : [];
    }

    private static Uri ReadAddress(string value, bool proxy)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var address) || address.UserInfo.Length != 0 || address.Query.Length != 0 || address.Fragment.Length != 0
            || address.Scheme != Uri.UriSchemeHttps && (!proxy || address.Scheme != Uri.UriSchemeHttp))
            throw new InvalidOperationException("Npm registry or proxy configuration isn't supported without a native client.");
        return address;
    }
}

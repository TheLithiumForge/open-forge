namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.Shared;

internal static class ReviewedAssetPins
{
    // Independent external oracle: published checksum files and release asset sizes checked 2026-11-14.
    internal static TheoryData<string, string, long> CodeGraph => new()
    {
        { "codegraph-win32-x64.zip", "7455e817ee8032666867a1badeb799b8a884c5f4fea9b444e088a02069b6deea", 54260850 },
        { "codegraph-win32-arm64.zip", "308f7bf92e7886f4d5f767a4755d703e92d5cae78ac042b8907f1e99db0fc071", 50258514 },
        { "codegraph-darwin-x64.tar.gz", "53d1a4d1a9af31d6cec11b346df2ae792087081e13f623f583e27783c1e7f9bc", 59393903 },
        { "codegraph-darwin-arm64.tar.gz", "d74d1bfb4060db63ec3c2b72c4e17f76c31978af3bad6ace4370d1501ac0662e", 58280485 },
        { "codegraph-linux-x64.tar.gz", "ef0af416092128fb1ccc723786000b7edf4a6971fe604acd08bf966e4f37b828", 63433974 },
        { "codegraph-linux-arm64.tar.gz", "c8c6be292be21d00dea26bad8b28d434731cf612fb8cc475dc10f5b3038b5b64", 63017097 },
    };

    internal static TheoryData<string, string, long> CodebaseMemory => new()
    {
        { "codebase-memory-mcp-windows-amd64.zip", "4e4e0f16b45233af5c757a1fcbe3c3b16e9056375b1fbe2ce020b6de75dedcc0", 48508123 },
        { "codebase-memory-mcp-windows-arm64.zip", "934ab6bd67a76c6ebd9e110d87b7090ffd629919bedaeddabe5a19175038b063", 47934918 },
        { "codebase-memory-mcp-darwin-amd64.tar.gz", "ca6e64397a8ae80cec898a8205a6aa16750ac798acc05a0c3ed983837944a581", 50555159 },
        { "codebase-memory-mcp-darwin-arm64.tar.gz", "9bfcbe38414b47969a3e1d1c1999f5357409387c86fa3d24221b5458267660d2", 49875423 },
        { "codebase-memory-mcp-linux-amd64-portable.tar.gz", "8c3d0341064301e78ecc84cf7348d4e2c6562a0f57448243ea560aaa34de36c1", 48920994 },
        { "codebase-memory-mcp-linux-arm64-portable.tar.gz", "ed0f1f548cd79b121c401c2e9f7841c189a001af26d3768368088c24f6a53533", 48466763 },
    };
}

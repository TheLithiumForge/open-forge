import { PythonAliasPackage } from "../package-model.ts";
import { WheelFixture } from "./wheel-fixtures.ts";

export const ResolverFile = { script: "resolver.py", request: "request.json", report: (index: number) => `report-${index}.json` } as const;
export const ResolverLimits = { milliseconds: 60_000, traceSeconds: 55 } as const;
export const WindowsResolverCases = [
  { name: "x64 Python on x64 Windows", machine: "AMD64", interpreterPlatform: "win_amd64", runtime: "win-x64" },
  { name: "x64 Python on ARM64 Windows", machine: "ARM64", interpreterPlatform: "win_amd64", runtime: "win-arm64" },
  { name: "ARM64 Python on ARM64 Windows", machine: "ARM64", interpreterPlatform: "win_arm64", runtime: "win-arm64" },
] as const;

export function resolverArguments(directory: string, report: string, interpreterPlatform: string): string[] {
  return [
    "--isolated",
    "install",
    "--dry-run",
    "--ignore-installed",
    "--disable-pip-version-check",
    "--no-cache-dir",
    "--no-index",
    "--only-binary=:all:",
    "--platform",
    interpreterPlatform,
    "--find-links",
    directory,
    "--report",
    report,
    `${PythonAliasPackage}==${WheelFixture.pythonVersion}`,
  ];
}

// Test-only host override: pip resolves our actual wheels using its normal marker and tag implementations.
export const ResolverProbe = `import faulthandler
faulthandler.dump_traceback_later(${ResolverLimits.traceSeconds})
import json
import sys
from pip._vendor.packaging import markers
from pip._internal.cli.main import main

with open(sys.argv[1], encoding="utf-8") as source:
    request = json.load(source)
def environment(request):
    return {
        "implementation_name": sys.implementation.name,
        "implementation_version": ".".join(map(str, sys.version_info[:3])),
        "os_name": "nt",
        "platform_machine": request["machine"],
        "platform_release": "11",
        "platform_system": "Windows",
        "platform_version": "10.0",
        "platform_python_implementation": "CPython",
        "python_full_version": ".".join(map(str, sys.version_info[:3])),
        "python_version": ".".join(map(str, sys.version_info[:2])),
        "sys_platform": request["platform"],
    }

result = 0
for scenario in request:
    markers.default_environment = lambda: environment(scenario)
    result = main(scenario["arguments"])
    if result:
        break
faulthandler.cancel_dump_traceback_later()
raise SystemExit(result)
`;

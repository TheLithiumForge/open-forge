import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";

// Python reports OS-specific machine spellings. Markers and launcher selection share these values.
const PythonMachines = {
  darwin: { x64: ["x86_64"], arm64: ["arm64"] },
  linux: { x64: ["x86_64"], arm64: ["aarch64", "arm64"] },
  win32: { x64: ["AMD64", "amd64", "x86_64"], arm64: ["ARM64", "arm64"] },
} as const;

export function pythonEnvironment(runtime: SupportedRuntime) {
  const platform = PlatformPackages[runtime];
  return { platform: platform.nodePlatform, machines: PythonMachines[platform.nodePlatform][platform.nodeArchitecture] };
}

export function platformMarker(runtime: SupportedRuntime): string {
  const environment = pythonEnvironment(runtime);
  const machines = environment.machines.map((machine) => `platform_machine == "${machine}"`).join(" or ");
  return `sys_platform == "${environment.platform}" and ${environment.machines.length === 1 ? machines : `(${machines})`}`;
}

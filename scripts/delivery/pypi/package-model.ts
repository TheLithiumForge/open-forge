import { AliasPackageName, MainPackageName, PlatformPackages, type SupportedRuntime } from "../package-model.ts";

export function pythonPackageName(npmName: string): string {
  return npmName.replace(/^@/u, "").replaceAll("/", "-");
}

export function pythonModule(name: string): string {
  return name.replaceAll("-", "_");
}

export const PythonMainPackage = pythonPackageName(MainPackageName);
export const PythonAliasPackage = pythonPackageName(AliasPackageName);

export function pythonPlatformPackage(runtime: SupportedRuntime): string {
  return pythonPackageName(PlatformPackages[runtime].packageName);
}

export function pythonExecutable(runtime: SupportedRuntime): string {
  return `${pythonModule(pythonPlatformPackage(runtime))}/bin/${PlatformPackages[runtime].nativeFileName}`;
}

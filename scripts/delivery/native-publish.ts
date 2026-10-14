import type { SupportedRuntime } from "./package-model.ts";

/** The native publication folder of the shipped CLI, which keeps the default optimization everywhere. */
export const ShippedCliFolder = "open-forge";

/** MSBuild properties for one native publication on one target. */
export function nativeCompilerProperties(rid: SupportedRuntime, folder: string): string[] {
  // On osx-arm64, linking the native Integration tests failed once they grew, while the other five
  // targets linked them. Optimizing the test executables for size keeps their code smaller there.
  // The shipped CLI keeps its default optimization.
  return rid === "osx-arm64" && folder !== ShippedCliFolder ? ["-p:OptimizationPreference=Size"] : [];
}

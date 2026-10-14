import type { SupportedRuntime } from "./package-model.ts";

/** MSBuild properties that keep native compilation within a hosted runner's memory. */
export function nativeCompilerProperties(rid: SupportedRuntime): string[] {
  // GitHub's standard macOS arm64 runner has 7 GB of memory, the least of the six targets, and its
  // native publication failed once the Integration tests grew. The workstation collector keeps the
  // compiler's own memory lower there at a small cost in compile time.
  return rid === "osx-arm64" ? ["-p:IlcUseServerGc=false"] : [];
}

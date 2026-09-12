import assert from "node:assert/strict";
import { PlatformPackages, type SupportedRuntime } from "./package-model.ts";

export const AllTargets = Object.values(PlatformPackages).map((platform) => platform.runtime);

export function readTargets(value: unknown): SupportedRuntime[] {
  assert.ok(Array.isArray(value) && value.length > 0, "Select at least one target.");
  const selected = value.map((entry: unknown) => {
    const target = AllTargets.find((runtime) => runtime === entry);
    assert.ok(target, `Unsupported target: ${String(entry)}. Choose ${AllTargets.join(",")}.`);
    return target;
  });
  assert.equal(new Set(selected).size, selected.length, "Repeated targets are not allowed.");
  return AllTargets.filter((target) => selected.includes(target));
}

export function parseTargets(value?: string): SupportedRuntime[] {
  return value === undefined ? [...AllTargets] : readTargets(value.split(",").map((target) => target.trim()));
}

export function targetDependencies(version: string, targets: readonly SupportedRuntime[]): Record<string, string> {
  return Object.fromEntries(readTargets(targets).map((target) => [PlatformPackages[target].packageName, version]));
}

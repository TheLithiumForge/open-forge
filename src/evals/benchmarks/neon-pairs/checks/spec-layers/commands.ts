import { object } from "./requirements.ts";
import { decodeSnapshot, snapshotsEqual } from "./snapshot.ts";
import { expectedReplay } from "./replay.ts";
import { Evidence } from "./outcomes.ts";
import { reporter } from "./browser-play.ts";
import type { Vector } from "./vectors.ts";
import { passedTests } from "./test-summary.ts";
import { NamedToolchain, parseToolchain, ToolchainClause } from "./toolchain.ts";

export const CommandId = { ci: "ci", test: "test", build: "build", typecheck: "typecheck", dev: "dev", e2e: "e2e", stale: "stale", replay: "replay" } as const;
export const UnitCommandClause = "T07.4";
export type CommandId = (typeof CommandId)[keyof typeof CommandId];
export interface CommandResult { readonly code: number | null; readonly timedOut: boolean; readonly stdout: string; readonly stderr: string; readonly html?: string }
export function commandResult(value: unknown): CommandResult {
  if (!object(value) || (value["code"] !== null && !Number.isInteger(value["code"])) || typeof value["timedOut"] !== "boolean" || typeof value["stdout"] !== "string" || typeof value["stderr"] !== "string" || value["stdout"].length > 2 * 1024 * 1024 || value["stderr"].length > 2 * 1024 * 1024 || value["html"] !== undefined && typeof value["html"] !== "string") throw new Error("Invalid isolated command result.");
  return { code: typeof value["code"] === "number" ? value["code"] : null, timedOut: value["timedOut"], stdout: value["stdout"], stderr: value["stderr"], ...(typeof value["html"] === "string" ? { html: value["html"] } : {}) };
}
export function compareReplay(vector: Vector, result: CommandResult): boolean {
  if (result.timedOut) throw new Error("Replay checker timed out.");
  const expected = expectedReplay(vector.text);
  if (!expected) return result.code !== null && result.code !== 0 && result.stdout === "" && result.stderr.trim().length > 0;
  const actual = decodeSnapshot(result.stdout);
  return result.code === 0 && result.stderr === "" && actual !== null && snapshotsEqual(actual, expected);
}
export function replayOutcomes(vectors: readonly Vector[], results: readonly CommandResult[], evidence: Evidence): void {
  const collected = new Map<string, { status: "pass" | "fail" | "error"; detail: string[]; observed: unknown[] }>();
  vectors.forEach((vector, index) => {
    const result = results[index];
    let status: "pass" | "fail" | "error" = "error";
    let error: string | undefined;
    try { if (result) status = compareReplay(vector, result) ? "pass" : "fail"; } catch (failure) { error = failure instanceof Error ? failure.message : String(failure); }
    for (const clause of vector.clauses) {
      const prior = collected.get(clause) ?? { status: "pass", detail: [], observed: [] };
      if (status === "fail" || status === "error" && prior.status !== "fail") prior.status = status;
      prior.detail.push(`${vector.name}: ${status}`); collected.set(clause, prior);
      if (status !== "pass" && prior.observed.length < 5) prior.observed.push({ vector: vector.name, ...replayObservation(vector, result), error });
    }
  });
  for (const [id, result] of collected) evidence.merge(id, result.status, `Expected replay exit and streams to follow the published rules and snapshot to equal the independent oracle. ${result.detail.join(". ")}`, result.observed);
}
function replayObservation(vector: Vector, result: CommandResult | undefined): object {
  const expected = expectedReplay(vector.text), actual = result ? decodeSnapshot(result.stdout) : null;
  const fields = expected && actual ? Object.keys(expected).filter((key) => JSON.stringify(Reflect.get(expected, key)) !== JSON.stringify(Reflect.get(actual, key))) : [];
  return { expected: expected ? { code: 0, stderr: "", snapshot: expected } : { code: "nonzero", stdout: "", stderr: "nonempty" },
    actual: result ? { code: result.code, timedOut: result.timedOut, stdout: result.stdout.slice(0, 2200), stdoutLength: result.stdout.length, stderr: result.stderr.slice(0, 2200), stderrLength: result.stderr.length } : "missing result", differentFields: fields };
}
export function buildOutcomes(results: ReadonlyMap<CommandId, CommandResult>, evidence: Evidence, full: boolean, toolchain?: CommandResult): void {
  const put = reporter(evidence), ok = (id: CommandId): boolean => { const result = results.get(id); return result !== undefined && result.code === 0 && !result.timedOut; };
  const build = results.get(CommandId.build), unit = results.get(CommandId.test);
  const observed = (id: CommandId) => { const result = results.get(id); return result ? { command: id, code: result.code, timedOut: result.timedOut, stdout: result.stdout.slice(-6000), stderr: result.stderr.slice(-6000), htmlBytes: result.html?.length ?? 0 } : { command: id, result: "missing" }; };
  put(UnitCommandClause, ok(CommandId.test) && passedTests(`${unit?.stdout ?? ""}\n${unit?.stderr ?? ""}`), "The required unit-test command exits successfully and reports tests that passed.", observed(CommandId.test));
  put("T07.6", ok(CommandId.build) && Boolean(build?.html) && /(?:typecheck|tsc|tsgo)/.test(build?.stdout ?? ""), "The required build command type checks and writes dist/index.html.", observed(CommandId.build));
  const html = build?.html ?? "";
  put("T08.1", html.length > 0 && !/<script\b[^>]*\bsrc=|<link\b[^>]*rel=["']stylesheet["'][^>]*href=|<(?:img|image)\b[^>]*(?:src|href)=["'](?!data:|#)/i.test(html), "The built HTML has inline scripts, styles and artwork.", { htmlBytes: html.length, externalTags: html.match(/<(?:script|link|img|image)\b[^>]*(?:src|href)=["'][^"']+["'][^>]*>/gi)?.slice(0, 8) ?? [] });
  if (!full) return;
  put("T07.1", ok(CommandId.ci), "npm ci installs the lock with lifecycle scripts disabled in the verifier container.", observed(CommandId.ci));
  put("T07.2", /(?:Local:|localhost|127\.0\.0\.1)/.test(results.get(CommandId.dev)?.stdout ?? ""), "The required dev command starts a local server. The command wrapper then stops its process group.", observed(CommandId.dev));
  put("T07.3", ok(CommandId.typecheck), "The required typecheck command exits successfully. Static compiler checks cover all included projects.", observed(CommandId.typecheck));
  const e2e = results.get(CommandId.e2e), stale = results.get(CommandId.stale);
  put("T07.5", ok(CommandId.e2e) && /(?:npm run build|vite.*build|building)/i.test(e2e?.stdout ?? "") && passedTests(`${e2e?.stdout ?? ""}\n${e2e?.stderr ?? ""}`), "The browser-test command builds current source and reports passing browser tests.", observed(CommandId.e2e));
  put("T07.8", stale !== undefined && stale.code !== 0 && !stale.timedOut, "A type error injected into a compiler input makes browser testing fail while stale dist is present.", observed(CommandId.stale));
  try {
    if (!toolchain || toolchain.code !== 0 || toolchain.timedOut) throw new Error("The verifier didn't collect its toolchain versions.");
    const actual = parseToolchain(toolchain.stdout);
    put(ToolchainClause, Object.entries(NamedToolchain).every(([name, version]) => actual[name as keyof typeof actual] === version), `Verifier versions: ${JSON.stringify(actual)}. Install, unit, build and browser results are recorded separately.`);
  } catch (error) { evidence.record(ToolchainClause, "error", `Expected verifier toolchain versions. Observed error: ${error instanceof Error ? error.message : String(error)}`, toolchain ?? "missing toolchain result"); }
}

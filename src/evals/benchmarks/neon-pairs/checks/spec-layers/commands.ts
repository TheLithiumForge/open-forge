import { object } from "./requirements.ts";
import { decodeSnapshot, snapshotsEqual } from "./snapshot.ts";
import { expectedReplay } from "./replay.ts";
import { Evidence } from "./outcomes.ts";
import { reporter } from "./browser-play.ts";
import type { Vector } from "./vectors.ts";

export const CommandId = { ci: "ci", test: "test", build: "build", typecheck: "typecheck", dev: "dev", e2e: "e2e", stale: "stale", replay: "replay" } as const;
export type CommandId = (typeof CommandId)[keyof typeof CommandId];
export interface CommandResult { readonly code: number | null; readonly timedOut: boolean; readonly stdout: string; readonly stderr: string; readonly html?: string }
export function commandResult(value: unknown): CommandResult {
  if (!object(value) || (value["code"] !== null && !Number.isInteger(value["code"])) || typeof value["timedOut"] !== "boolean" || typeof value["stdout"] !== "string" || typeof value["stderr"] !== "string" || value["stdout"].length > 2 * 1024 * 1024 || value["stderr"].length > 2 * 1024 * 1024 || value["html"] !== undefined && typeof value["html"] !== "string") throw new Error("Invalid isolated command result.");
  return { code: typeof value["code"] === "number" ? value["code"] : null, timedOut: value["timedOut"], stdout: value["stdout"], stderr: value["stderr"], ...(typeof value["html"] === "string" ? { html: value["html"] } : {}) };
}
export function compareReplay(vector: Vector, result: CommandResult): boolean {
  if (result.timedOut) throw new Error("Replay checker timed out.");
  const expected = expectedReplay(vector.text);
  if (!expected) return result.code !== 0 && decodeSnapshot(result.stdout) === null;
  const actual = decodeSnapshot(result.stdout);
  return result.code === 0 && actual !== null && snapshotsEqual(actual, expected);
}
export function replayOutcomes(vectors: readonly Vector[], results: readonly CommandResult[], evidence: Evidence): void {
  const collected = new Map<string, { status: "pass" | "fail" | "error"; detail: string[] }>();
  vectors.forEach((vector, index) => {
    const result = results[index];
    let status: "pass" | "fail" | "error" = "error";
    try { if (result) status = compareReplay(vector, result) ? "pass" : "fail"; } catch { status = "error"; }
    for (const clause of vector.clauses) {
      const prior = collected.get(clause) ?? { status: "pass", detail: [] };
      if (status === "fail" || status === "error" && prior.status !== "fail") prior.status = status;
      prior.detail.push(`${vector.name}: ${status}`); collected.set(clause, prior);
    }
  });
  for (const [id, result] of collected) evidence.merge(id, result.status, `Required replay CLI compared with an independent oracle. ${result.detail.join(". ")}`);
}
export function buildOutcomes(results: ReadonlyMap<CommandId, CommandResult>, evidence: Evidence, full: boolean): void {
  const put = reporter(evidence), ok = (id: CommandId): boolean => { const result = results.get(id); return result !== undefined && result.code === 0 && !result.timedOut; };
  const build = results.get(CommandId.build), unit = results.get(CommandId.test);
  put("T07.4", ok(CommandId.test) && /(?:Tests\s+\d+\s+passed|# pass [1-9]|[1-9]\d* passing)/.test(unit?.stdout ?? ""), "The required unit-test command exits successfully and reports tests that passed.");
  put("T07.6", ok(CommandId.build) && Boolean(build?.html) && /(?:typecheck|tsc|tsgo)/.test(build?.stdout ?? ""), "The required build command type checks and writes dist/index.html.");
  const html = build?.html ?? "";
  put("T08.1", html.length > 0 && !/<script\b[^>]*\bsrc=|<link\b[^>]*rel=["']stylesheet["'][^>]*href=|<(?:img|image)\b[^>]*(?:src|href)=["'](?!data:|#)/i.test(html), "The built HTML has inline scripts, styles and artwork.");
  evidence.record("T07.9", "error", "An isolated checker cannot reconstruct processes left in the earlier authoring session. Verify the authoring container process list before its shutdown.");
  if (!full) return;
  put("T07.1", ok(CommandId.ci), "npm ci installs the lock with lifecycle scripts disabled in the verifier container.");
  put("T07.2", /(?:Local:|localhost|127\.0\.0\.1)/.test(results.get(CommandId.dev)?.stdout ?? ""), "The required dev command starts a local server. The command wrapper then stops its process group.");
  put("T07.3", ok(CommandId.typecheck), "The required typecheck command exits successfully. Static compiler checks cover all included projects.");
  const e2e = results.get(CommandId.e2e), stale = results.get(CommandId.stale);
  put("T07.5", ok(CommandId.e2e) && /(?:npm run build|vite.*build|building)/i.test(e2e?.stdout ?? "") && /(?:[1-9]\d* passed)/.test(e2e?.stdout ?? ""), "The browser-test command builds current source and reports passing browser tests.");
  put("T07.8", stale !== undefined && stale.code !== 0 && !stale.timedOut, "A type error injected into a compiler input makes browser testing fail while stale dist is present.");
  evidence.record("T03.4", "error", "The shared verifier image uses a different Node version. Validate the named Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4 combination in that toolchain.");
}

import { homedir } from "node:os";
import { join } from "node:path";
import { fileURLToPath } from "node:url";

/** The evals folder, which holds the config, the benchmarks, the local runs and the pinned tools. */
export const evalsRoot = fileURLToPath(new URL(".", import.meta.url));

export const EvalPaths = {
  config: join(evalsRoot, "eval.config.json"),
  localConfig: join(evalsRoot, "eval.config.local.json"),
  suites: join(evalsRoot, "benchmarks", "neon-pairs", "cases"),
  checks: join(evalsRoot, "benchmarks", "neon-pairs", "checks"),
  results: join(evalsRoot, "runs"),
  auth: join(evalsRoot, ".auth"),
  codexAuthHome: join(evalsRoot, ".auth", "codex"),
  envFile: join(evalsRoot, ".env.local"),
  uiDist: join(evalsRoot, "web", "dist"),
  docker: join(evalsRoot, "images"),
  modules: join(evalsRoot, "node_modules"),
  // Runs work outside any Git repository, so no parent AGENTS.md or CLAUDE.md reaches the agent.
  // Not under the temp folder, where Codex refuses to create its helper commands, and named so
  // the agent's own path doesn't say what's being tested. EVALS_SCRATCH moves it to a roomier drive,
  // which must also sit outside any Git repository.
  scratch: process.env["EVALS_SCRATCH"] ?? join(homedir(), ".workspaces"),
} as const;

export const ResultFiles = {
  batch: "batch.json",
  run: "run.json",
  setups: "setups.json",
  preferences: "preferences.json",
  events: "events.jsonl",
  game: "game.html",
  files: "files",
  shots: "shots",
} as const;

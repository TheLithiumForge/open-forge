// Signs Codex in once into a home kept only for evals. Each run gets a copy of that sign-in and nothing else.
import { spawnSync } from "node:child_process";
import { mkdirSync } from "node:fs";
import { EvalPaths } from "../paths.ts";
import { localToolCommand, PinnedTool } from "../agents/pinned-tools.ts";

mkdirSync(EvalPaths.codexAuthHome, { recursive: true });
const codex = localToolCommand(PinnedTool.codex);
const result = spawnSync(codex.command, [...codex.args, "login", ...process.argv.slice(2)], {
  stdio: "inherit",
  env: { ...process.env, CODEX_HOME: EvalPaths.codexAuthHome },
});
process.exitCode = result.status ?? 1;

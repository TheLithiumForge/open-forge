import { existsSync } from "node:fs";
import { join } from "node:path";
import { EvalPaths } from "../paths.ts";
import { Tool } from "./choices.ts";

/** Sign-in variables the runs may receive. Every other variable from the maintainer's shell stays out. */
export const AuthVariables = {
  claudeToken: "CLAUDE_CODE_OAUTH_TOKEN",
  anthropicKey: "ANTHROPIC_API_KEY",
  openAiKey: "OPENAI_API_KEY",
  codexKey: "CODEX_API_KEY",
} as const;

export const CodexAuthFile = "auth.json";

export interface SignInStatus {
  readonly claude: boolean;
  readonly codex: boolean;
}

/** Reads `.env.local` once, so a token from `claude setup-token` can live there instead of the shell. */
export function loadLocalEnv(): void {
  if (existsSync(EvalPaths.envFile)) process.loadEnvFile(EvalPaths.envFile);
}

export function signInStatus(): SignInStatus {
  const has = (name: string): boolean => (process.env[name] ?? "") !== "";
  return {
    claude: has(AuthVariables.claudeToken) || has(AuthVariables.anthropicKey),
    codex: existsSync(join(EvalPaths.codexAuthHome, CodexAuthFile)) || has(AuthVariables.openAiKey) || has(AuthVariables.codexKey),
  };
}

/** Refuses to start work that calls a tool without a sign-in, because every run would fail the same way. */
export function requireSignIns(tools: readonly Tool[], status: SignInStatus = signInStatus()): void {
  const missing = [...new Set(tools)].filter((tool) => tool !== Tool.mock && !status[tool]);
  if (missing.length > 0) throw new Error(`No ${missing.join(" or ")} sign-in. Run npm run evals -- doctor for the steps.`);
}

/** The sign-in variables to pass into a run, by name only, so values never land in a command line. */
export function authVariableNames(provider?: Tool, env: NodeJS.ProcessEnv = process.env): readonly string[] {
  const names = provider === Tool.claude ? [AuthVariables.claudeToken, AuthVariables.anthropicKey]
    : provider === Tool.codex ? [AuthVariables.openAiKey, AuthVariables.codexKey]
      : provider === Tool.mock ? [] : Object.values(AuthVariables);
  return names.filter((name) => (env[name] ?? "") !== "");
}

export function providerAuth(provider: Tool, env: NodeJS.ProcessEnv = process.env): Record<string, string> {
  return Object.fromEntries(authVariableNames(provider, env).map((name) => [name, env[name] ?? ""]));
}

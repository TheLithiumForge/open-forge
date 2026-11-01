import { existsSync } from "node:fs";
import { join } from "node:path";
import { EvalPaths } from "../paths.ts";

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

/** The sign-in variables to pass into a run, by name only, so values never land in a command line. */
export function authVariableNames(): readonly string[] {
  return Object.values(AuthVariables).filter((name) => (process.env[name] ?? "") !== "");
}

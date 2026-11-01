import { existsSync } from "node:fs";
import { join } from "node:path";
import { CodexAuthFile, providerAuth } from "../../config/auth.ts";
import { Tool } from "../../config/choices.ts";
import { stageSignInCopy, keepRefreshedSignIn, type CodexSignInCopy } from "../providers/codex-sign-in.ts";
import { ContainerPaths } from "./settings.ts";
import { copyOutPlain, freshHome } from "./files.ts";
import type { ContainerStage } from "./stage.ts";
import type { RunSpace } from "./executor.ts";

export const CodexDirectory = ".codex";
export const ClaudeDirectory = ".claude";

export function toolEnvironment(provider: Tool): Record<string, string> {
  return { HOME: ContainerPaths.home, USERPROFILE: ContainerPaths.home,
    ...(provider === Tool.codex ? { CODEX_HOME: `${ContainerPaths.home}/${CodexDirectory}` } : { CLAUDE_CONFIG_DIR: `${ContainerPaths.home}/${ClaudeDirectory}` }),
    CLAUDE_CODE_DISABLE_AUTO_MEMORY: "1", DISABLE_AUTOUPDATER: "1", IS_SANDBOX: "1" };
}

export interface ToolHomeOptions {
  readonly env?: NodeJS.ProcessEnv;
  readonly codexSignInHome?: string;
}

export class ToolHome {
  readonly auth: Readonly<Record<string, string>>;
  private signIn: CodexSignInCopy | null = null;
  private release = (): void => {};
  private readonly provider: Tool;
  private readonly space: RunSpace;
  private readonly options: ToolHomeOptions;

  constructor(provider: Tool, space: RunSpace, options: ToolHomeOptions) {
    this.provider = provider;
    this.space = space;
    this.options = options;
    this.auth = providerAuth(provider, options.env);
  }

  async copyIn(stage: ContainerStage): Promise<void> {
    await freshHome(stage, this.space.root, this.space.home);
    if (this.provider !== Tool.codex) return;
    const staged = stageSignInCopy(this.space.home, this.options.codexSignInHome);
    this.signIn = staged.copy;
    this.release = staged.release;
    if (this.signIn !== null) await stage.copyIn(this.signIn.path, `${ContainerPaths.home}/${CodexDirectory}/${CodexAuthFile}`);
  }

  async sync(stage: ContainerStage): Promise<void> {
    if (this.provider !== Tool.codex) return;
    await copyOutPlain(stage, `${ContainerPaths.home}/${CodexDirectory}`, join(this.space.home, CodexDirectory), this.space.root);
    if (this.signIn !== null && existsSync(this.signIn.path)) keepRefreshedSignIn(this.signIn);
  }

  dispose(): void { this.release(); }
}

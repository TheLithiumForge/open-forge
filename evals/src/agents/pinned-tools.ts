import { join } from "node:path";
import { EvalPaths } from "../paths.ts";

/** The tools pinned in package.json, so every machine runs the same versions. */
export const PinnedTool = {
  claude: "claude",
  codex: "codex",
  openForge: "open-forge",
} as const;
export type PinnedTool = (typeof PinnedTool)[keyof typeof PinnedTool];

export interface ToolCommand {
  readonly command: string;
  readonly args: readonly string[];
}

const pinnedPaths = {
  [PinnedTool.claude]: join(EvalPaths.modules, "@anthropic-ai", "claude-code", "bin", "claude.exe"),
  [PinnedTool.codex]: join(EvalPaths.modules, "@openai", "codex", "bin", "codex.js"),
  [PinnedTool.openForge]: join(EvalPaths.modules, "@thelithiumforge", "open-forge", "bin", "open-forge.js"),
} as const satisfies Record<PinnedTool, string>;

/** Claude Code ships a native binary. Codex and Open Forge ship Node launchers, run with this Node. */
export function localToolCommand(tool: PinnedTool): ToolCommand {
  const path = pinnedPaths[tool];
  return tool === PinnedTool.claude ? { command: path, args: [] } : { command: process.execPath, args: [path] };
}

/** Inside the eval image, the same pinned versions are installed globally. */
export function imageToolCommand(tool: PinnedTool): ToolCommand {
  return { command: tool, args: [] };
}

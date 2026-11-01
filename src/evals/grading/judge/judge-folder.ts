import { cp, mkdir } from "node:fs/promises";
import { basename } from "node:path";

// The judge sees only the app. Open Forge's files, instruction files, the agent's notes and
// build output come out first, so the judge can't tell which setup a run had.
const hiddenNames = new Set([".agents", "AGENTS.md", "CLAUDE.md", ".claude", ".codex", "node_modules", "dist", "package-lock.json"]);
const hiddenExtensions = [".md"];

function visibleToJudge(path: string): boolean {
  const name = basename(path);
  return !hiddenNames.has(name) && !hiddenExtensions.some((extension) => name.endsWith(extension));
}

export async function prepareJudgeFolder(frozenFiles: string, judgeFolder: string): Promise<void> {
  await mkdir(judgeFolder, { recursive: true });
  await cp(frozenFiles, judgeFolder, { recursive: true, filter: visibleToJudge });
}

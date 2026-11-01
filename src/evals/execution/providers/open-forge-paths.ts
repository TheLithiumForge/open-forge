// Open Forge's own files in a run: the .agents folder and the root instruction files it writes.
const openForgePattern = /(^|[\\/])\.agents[\\/]|(^|[\\/])(AGENTS|CLAUDE)\.md$/;

export function isOpenForgePath(path: string): boolean {
  return openForgePattern.test(path);
}

/** A shell command that reads Open Forge's files, as Codex reports them. */
export function readsOpenForge(command: string): boolean {
  return /\.agents[\\/]|\b(AGENTS|CLAUDE)\.md\b/.test(command);
}

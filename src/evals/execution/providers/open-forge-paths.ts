// Open Forge's own files in a run: the loader and its guide, skills, templates and catalogues, and the root instruction files it writes. A project's rules and notes inside the categories are its own material.
const openForgePattern = /(^|[\\/])\.agents[\\/]([^\\/]+\.(md|json)$|skills[\\/]|templates[\\/])|(^|[\\/])\.agents[\\/](.*[\\/])?_[^\\/]+\.md$|(^|[\\/])(AGENTS|CLAUDE)\.md$/;

export function isOpenForgePath(path: string): boolean {
  return openForgePattern.test(path);
}

/** A shell command that reads Open Forge's files, as Codex reports them. */
export function readsOpenForge(command: string): boolean {
  return /\.agents[\\/]([^\s\\/'"]+\.(md|json)\b|skills[\\/]|templates[\\/])|\.agents[\\/]\S*_[\w-]+\.md\b|\b(AGENTS|CLAUDE)\.md\b/.test(command);
}

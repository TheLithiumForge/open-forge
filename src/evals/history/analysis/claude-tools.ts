import { ContextClass, ToolKind } from "./classes.ts";
import { commandFacts, type CommandFacts } from "./command-facts.ts";
import { fileClass, projectPath } from "./paths.ts";

/** Claude Code's own tools, by what they do. Bash commands are read like any shell command. */
export const ClaudeTool = { read: "Read", grep: "Grep", glob: "Glob", list: "LS", bash: "Bash", task: "Task", agent: "Agent" } as const;
const Edits = new Set(["Write", "Edit", "MultiEdit", "NotebookEdit"]);
const Delegations = new Set<string>([ClaudeTool.task, ClaudeTool.agent]);

const commandOutput = (kind: ToolKind, type: ContextClass): CommandFacts => ({ kind, sources: [{ class: type, path: null }], search: false, reads: [], writes: [] });

/** What one Claude tool call did, in the same terms as a shell command. */
export function claudeToolFacts(name: string, input: Readonly<Record<string, unknown>>): CommandFacts {
  const path = typeof input["file_path"] === "string" ? projectPath(input["file_path"]) : typeof input["notebook_path"] === "string" ? projectPath(input["notebook_path"]) : null;
  if (name === ClaudeTool.bash) return commandFacts(typeof input["command"] === "string" ? input["command"] : "");
  if (name === ClaudeTool.read && path !== null) return { kind: ToolKind.read, sources: [{ class: fileClass(path), path }], search: false, reads: [path], writes: [] };
  if (name === ClaudeTool.grep) return { kind: ToolKind.search, sources: [{ class: ContextClass.search, path: null }], search: true, reads: [], writes: [] };
  if (name === ClaudeTool.glob || name === ClaudeTool.list) return commandOutput(ToolKind.search, ContextClass.search);
  if (Edits.has(name)) return { ...commandOutput(ToolKind.edit, ContextClass.command), writes: path === null ? [] : [path] };
  if (Delegations.has(name)) return commandOutput(ToolKind.other, ContextClass.report);
  return commandOutput(ToolKind.other, ContextClass.command);
}

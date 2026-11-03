import { ContextClass, ToolKind } from "./classes.ts";
import { commandFacts } from "./command-facts.ts";
import { record } from "./log-values.ts";
import { allocate, outputSegments, type KnownFiles, type Segment } from "./segments.ts";

/** One model tool call from Codex's session log, with what it asked for. */
export interface CodexCall {
  readonly name: string;
  /** The call's arguments or input as the model wrote them. */
  readonly input: unknown;
}

/** A shell command Codex ran, with its full output. Code mode can run several within one call. */
export interface ExecutedCommand {
  readonly command: string | readonly string[];
  readonly output: string;
}

// Tools whose output is shell command output. Code mode's exec runs commands from a script, and wait returns their later output.
const CommandTools = new Set(["exec_command", "shell", "shell_command", "container.exec", "local_shell", "exec", "wait", "write_stdin", "unified_exec"]);
/** Subagent tools. A wait returns the subagent's final report to the orchestrator. */
const ReportTools = new Set(["wait_agent"]);
const PatchTool = "apply_patch";
const ScriptCommand = /\bcmd\s*:\s*("(?:[^"\\]|\\.)*"|'(?:[^'\\]|\\.)*')/g;

/** The text of a tool output as the model saw it. Older logs wrap it in JSON with an output field. */
export function outputText(output: unknown): string {
  if (Array.isArray(output)) return output.map((part) => outputText(record(part)?.["text"] ?? part)).join("");
  const fields = record(output);
  if (fields !== null) return outputText(fields["output"] ?? fields["content"] ?? fields["text"] ?? "");
  if (typeof output !== "string") return "";
  if (!output.startsWith("{")) return output;
  try {
    const wrapped = record(JSON.parse(output));
    return typeof wrapped?.["output"] === "string" ? wrapped["output"] : output;
  } catch { return output; }
}

function literal(quoted: string): string {
  if (quoted.startsWith("\"")) { try { return String(JSON.parse(quoted)); } catch { return quoted.slice(1, -1); } }
  return quoted.slice(1, -1).replaceAll(/\\(.)/g, "$1");
}

/** The commands a call asked to run, from its arguments: a command array, a cmd string or code mode's script. */
export function callCommands(call: CodexCall): (string | readonly string[])[] {
  const input = call.input;
  if (typeof input === "string" && !input.trimStart().startsWith("{")) return [...input.matchAll(ScriptCommand)].map((match) => literal(match[1] ?? ""));
  let fields = record(input);
  if (typeof input === "string") { try { fields = record(JSON.parse(input)); } catch { fields = null; } }
  const command = fields?.["cmd"] ?? fields?.["command"] ?? record(fields?.["action"])?.["command"];
  if (typeof command === "string") return [command];
  return Array.isArray(command) && command.every((word) => typeof word === "string") ? [command.map(String)] : [];
}

/** Whether a call changed files: the patch tool, or a command that writes. */
export function isEditCall(call: CodexCall | undefined): boolean {
  if (call === undefined) return false;
  return call.name === PatchTool || (CommandTools.has(call.name) && callCommands(call).some((command) => commandFacts(command).kind === ToolKind.edit));
}

/** Splits one call's output, as the model saw it, into what it came from. Several commands share it by their output size. */
export function callSegments(call: CodexCall | undefined, text: string, executed: readonly ExecutedCommand[], files: KnownFiles): Segment[] {
  if (text.length === 0) return [];
  if (call !== undefined && ReportTools.has(call.name)) return [{ class: ContextClass.report, fileType: null, chars: text.length }];
  if (call === undefined || !CommandTools.has(call.name)) return [{ class: ContextClass.command, fileType: null, chars: text.length }];
  const commands = executed.length > 0 ? executed : callCommands(call).map((command) => ({ command, output: "" }));
  if (commands.length === 0) return [{ class: ContextClass.command, fileType: null, chars: text.length }];
  if (commands.length === 1) return outputSegments(commandFacts(commands[0]?.command ?? ""), text, files);
  const shares = allocate(text.length, commands.map((item) => (item.output.length > 0 ? item.output.length : null)));
  return commands.flatMap((item, index) => {
    const segments = outputSegments(commandFacts(item.command), item.output.length > 0 ? item.output : text, files);
    const size = segments.reduce((sum, part) => sum + part.chars, 0);
    return size === 0 ? [] : segments.map((part) => ({ ...part, chars: (part.chars * (shares[index] ?? 0)) / size }));
  });
}

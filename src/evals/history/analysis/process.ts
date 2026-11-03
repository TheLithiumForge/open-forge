import { Tool } from "../../config/choices.ts";
import type { ProcessMeasures } from "./analysis.types.ts";
import { ToolKind } from "./classes.ts";
import { claudeToolFacts } from "./claude-tools.ts";
import { commandFacts } from "./command-facts.ts";
import { list, logLines, record, string, type LogRecord } from "./log-values.ts";
import { expandGlob, isGlob, projectPath } from "./paths.ts";
import type { KnownFiles } from "./segments.ts";

/** One tool call as the main thread's events report it. */
interface Call { readonly kind: ToolKind; readonly failed: boolean; readonly reads: readonly string[]; readonly writes: readonly string[] }

const Completed = "item.completed";
const CodexItem = { command: "command_execution", fileChange: "file_change", mcp: "mcp_tool_call", search: "web_search", collab: "collab_tool_call" } as const;
const FailedStatus = new Set(["failed", "error", "declined"]);

function codexCall(item: LogRecord): Call | null {
  const type = string(item["type"]);
  const failed = FailedStatus.has(string(item["status"]) ?? "") || (typeof item["exit_code"] === "number" && item["exit_code"] !== 0);
  if (type === CodexItem.command) {
    const facts = commandFacts(string(item["command"]) ?? "");
    return { kind: facts.kind, failed, reads: facts.reads, writes: facts.writes };
  }
  if (type === CodexItem.fileChange) return { kind: ToolKind.edit, failed, reads: [], writes: list(item["changes"]).map((change) => projectPath(string(record(change)?.["path"]) ?? "")) };
  if (type === CodexItem.search) return { kind: ToolKind.search, failed, reads: [], writes: [] };
  if (type === CodexItem.mcp || type === CodexItem.collab) return { kind: ToolKind.other, failed, reads: [], writes: [] };
  return null;
}

function codexCalls(lines: readonly LogRecord[]): Call[] {
  return lines.flatMap((line) => {
    const item = record(line["item"]);
    const call = line["type"] === Completed && item !== null ? codexCall(item) : null;
    return call === null ? [] : [call];
  });
}

function claudeCalls(lines: readonly LogRecord[]): Call[] {
  const failed = new Set<string>();
  for (const line of lines) {
    if (line["type"] !== "user") continue;
    for (const block of list(record(line["message"])?.["content"]).map(record)) if (block?.["type"] === "tool_result" && block["is_error"] === true) failed.add(string(block["tool_use_id"]) ?? "");
  }
  // Helpers' calls carry the tool call that started them. They are the helpers' work, so the main thread's process leaves them out.
  return lines.filter((line) => line["type"] === "assistant" && line["parent_tool_use_id"] == null).flatMap((line) => list(record(line["message"])?.["content"]).map(record).flatMap((block) => {
    if (block?.["type"] !== "tool_use") return [];
    const facts = claudeToolFacts(string(block["name"]) ?? "", record(block["input"]) ?? {});
    return [{ kind: facts.kind, failed: failed.has(string(block["id"]) ?? ""), reads: facts.reads, writes: facts.writes }];
  }));
}

const expanded = (paths: readonly string[], files: KnownFiles) => paths.flatMap((path) => (isGlob(path) ? expandGlob(path, files.keys()) : [path]));

/** Tool calls by kind, failures, the steps before the first edit, repeated reads and the files read and edited. */
export function processMeasures(tool: string, events: string, files: KnownFiles): ProcessMeasures {
  const lines = logLines(events);
  const calls = tool === Tool.claude ? claudeCalls(lines) : codexCalls(lines);
  const counts = { read: 0, search: 0, edit: 0, run: 0, other: 0 } satisfies Record<ToolKind, number>;
  const read = new Set<string>(), edited = new Set<string>();
  let repeatedReads = 0;
  for (const call of calls) {
    counts[call.kind] += 1;
    for (const path of expanded(call.reads, files)) {
      if (read.has(path)) repeatedReads += 1;
      read.add(path);
    }
    for (const path of call.writes) edited.add(path);
  }
  const firstEdit = calls.findIndex((call) => call.kind === ToolKind.edit);
  return { calls: counts, failed: calls.filter((call) => call.failed).length, steps: calls.length, stepsBeforeEdit: firstEdit < 0 ? null : firstEdit,
    repeatedReads, filesRead: read.size, filesEdited: edited.size };
}

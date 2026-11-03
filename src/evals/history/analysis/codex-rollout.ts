import { ContextClass, ThreadRole, ToolKind } from "./classes.ts";
import { callSegments, isEditCall, outputText, type CodexCall, type ExecutedCommand } from "./codex-calls.ts";
import { commandFacts } from "./command-facts.ts";
import { count, list, logLines, record, string, text, type LogRecord } from "./log-values.ts";
import type { KnownFiles } from "./segments.ts";
import { ThreadBuilder, type RequestUsage, type ThreadLog } from "./thread-log.ts";

/**
 * Reads Codex's saved session log, which keeps every thread's requests with their exact usage, the tool calls and what
 * the model saw of each output. A run's log joins every thread's file, each starting with its session line.
 */

const UsageRecord = "token_usage_record";
const CallItems = new Set(["function_call", "custom_tool_call", "local_shell_call"]);
const OutputItems = new Set(["function_call_output", "custom_tool_call_output"]);
const CommandItems = new Set(["CommandExecution", "command_execution"]);
const FileChangeItems = new Set(["FileChange", "file_change"]);
const ParentKey = /parent.*(thread|session|conversation).*id|parent_?id/i;
const MainThread = "main";

function usageOf(value: unknown): RequestUsage | null {
  const usage = record(value);
  if (usage === null) return null;
  const reasoning = usage["reasoning_output_tokens"];
  return { input: count(usage["input_tokens"]), cached: count(usage["cached_input_tokens"]), cacheWrite: 0, output: count(usage["output_tokens"]),
    reasoning: typeof reasoning === "number" ? reasoning : null };
}

/** The first string under a key that names a parent thread, searched through nested fields. */
function parentId(value: unknown): string | null {
  const fields = record(value);
  if (fields === null) return null;
  for (const [key, item] of Object.entries(fields)) {
    if (ParentKey.test(key) && typeof item === "string") return item;
    const nested = parentId(item);
    if (nested !== null) return nested;
  }
  return null;
}

class RolloutReader {
  readonly threads = new Map<string, ThreadBuilder>();
  private current: ThreadBuilder | null = null;
  private readonly calls = new Map<string, CodexCall>();
  private executed: ExecutedCommand[] = [];
  private readonly files: KnownFiles;
  constructor(files: KnownFiles) { this.files = files; }

  thread(id: string = MainThread, meta: LogRecord | null = null): ThreadBuilder {
    const existing = this.threads.get(id);
    if (existing !== undefined) return existing;
    const parent = meta === null ? null : parentId({ ...meta, git: null });
    const subagent = parent !== null || JSON.stringify(meta?.["source"] ?? "").includes("subagent");
    const thread = new ThreadBuilder(id, parent, subagent ? ThreadRole.subagent : ThreadRole.main);
    this.threads.set(id, thread);
    return thread;
  }

  session(payload: LogRecord): void {
    this.current = this.thread(string(payload["id"]) ?? string(payload["session_id"]) ?? `${MainThread}-${this.threads.size}`, payload);
  }

  item(payload: LogRecord): void {
    const thread = this.current ?? this.thread();
    const type = string(payload["type"]) ?? "";
    if (type === "message") {
      const role = payload["role"], chars = text(list(payload["content"]).map((part) => record(part)?.["text"] ?? "")).length;
      // Before a thread's first request its messages are up front, except a subagent's brief from the orchestrator.
      if (role === "user") thread.add([{ class: thread.requests === 0 ? (thread.role === ThreadRole.subagent ? ContextClass.delegation : ContextClass.upfront) : ContextClass.user, fileType: null, chars }]);
      else if (role !== "assistant" && thread.requests > 0) thread.add([{ class: ContextClass.other, fileType: null, chars }]);
    } else if (CallItems.has(type)) {
      thread.toolCalls += 1;
      this.calls.set(string(payload["call_id"]) ?? "", { name: string(payload["name"]) ?? type, input: payload["arguments"] ?? payload["input"] ?? payload["action"] ?? null });
    } else if (OutputItems.has(type)) {
      const call = this.calls.get(string(payload["call_id"]) ?? "");
      thread.add(callSegments(call, outputText(payload["output"]), this.executed, this.files));
      if (isEditCall(call) || this.executed.some((item) => commandFacts(item.command).kind === ToolKind.edit)) thread.markEdit();
      this.executed = [];
    }
  }

  event(payload: LogRecord, usageRecords: boolean): void {
    const type = string(payload["type"]) ?? "";
    const item = record(payload["item"]);
    if (type === "item_completed" && CommandItems.has(string(item?.["type"]) ?? "")) this.command(item);
    if (type === "exec_command_end") this.command(payload);
    if ((type === "item_completed" && FileChangeItems.has(string(item?.["type"]) ?? "")) || type === "patch_apply_end") (this.current ?? this.thread()).markEdit();
    if (type === "context_compacted") this.compact();
    if (type === "token_count" && !usageRecords) this.request(record(payload["info"])?.["last_token_usage"], null);
  }

  compact(): void { (this.current ?? this.thread()).compact(); }

  command(fields: LogRecord | null): void {
    const command = fields?.["command"];
    const words = Array.isArray(command) ? command.map(String) : string(command) ?? "";
    this.executed.push({ command: words, output: string(fields?.["aggregated_output"]) ?? string(fields?.["formatted_output"]) ?? string(fields?.["stdout"]) ?? "" });
  }

  request(value: unknown, threadId: string | null): void {
    const usage = usageOf(value);
    if (usage === null) return;
    const thread = threadId === null ? this.current ?? this.thread() : this.threads.get(threadId) ?? this.current ?? this.thread(threadId);
    thread.request(usage);
  }
}

/** Every thread of a saved Codex session log, in the order they started. */
export function codexThreads(content: string, files: KnownFiles): ThreadLog[] {
  const lines = logLines(content);
  const usageRecords = lines.some((line) => line["type"] === UsageRecord);
  const reader = new RolloutReader(files);
  for (const line of lines) {
    const type = line["type"], payload = record(line["payload"]) ?? {};
    if (type === "session_meta") reader.session(payload);
    else if (type === "response_item") reader.item(payload);
    else if (type === "event_msg") reader.event(payload, usageRecords);
    else if (type === "compacted") reader.compact();
    else if (type === UsageRecord) reader.request(payload["usage"], string(payload["thread_id"]));
  }
  return [...reader.threads.values()].map((thread) => thread.log()).filter((thread) => thread.steps.length > 0);
}

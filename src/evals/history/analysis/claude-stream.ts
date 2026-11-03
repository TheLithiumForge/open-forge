import { CharactersPerToken, ContextClass, ThreadRole, ToolKind } from "./classes.ts";
import { claudeToolFacts } from "./claude-tools.ts";
import { count, list, logLines, record, string, text, type LogRecord } from "./log-values.ts";
import { outputSegments, type KnownFiles } from "./segments.ts";
import { ThreadBuilder, type ThreadLog } from "./thread-log.ts";

/**
 * Reads Claude Code's stream events. Every model message carries its request's exact input, so the stream gives the same
 * per-request view as Codex's session log. A helper's messages name the tool call that started it, which makes it its own thread.
 */

const MainThread = "main";

interface ToolUse { readonly name: string; readonly input: LogRecord; readonly thread: string }

class StreamReader {
  readonly threads = new Map<string, ThreadBuilder>();
  private readonly seen = new Set<string>();
  private readonly uses = new Map<string, ToolUse>();
  private readonly files: KnownFiles;
  constructor(files: KnownFiles) { this.files = files; }

  thread(id: string | null): ThreadBuilder {
    const key = id ?? MainThread;
    const existing = this.threads.get(key);
    if (existing !== undefined) return existing;
    const thread = id === null ? new ThreadBuilder(MainThread, null, ThreadRole.main) : new ThreadBuilder(id, this.uses.get(id)?.thread ?? MainThread, ThreadRole.subagent);
    this.threads.set(key, thread);
    return thread;
  }

  assistant(event: LogRecord, message: LogRecord): void {
    const thread = this.thread(string(event["parent_tool_use_id"]));
    const id = string(message["id"]) ?? "", usage = record(message["usage"]);
    if (usage !== null && !this.seen.has(id)) {
      this.seen.add(id);
      const cached = count(usage["cache_read_input_tokens"]), written = count(usage["cache_creation_input_tokens"]);
      // Output comes from each turn's result, since stream events carry a message's usage from its start.
      thread.request({ input: count(usage["input_tokens"]) + cached + written, cached, cacheWrite: written, output: 0, reasoning: null }, 0);
    }
    for (const block of list(message["content"]).map(record)) {
      const type = string(block?.["type"]);
      const written = type === "tool_use" ? JSON.stringify(block?.["input"] ?? {}) : string(block?.["text"]) ?? string(block?.["thinking"]) ?? "";
      thread.addOutput(written.length / CharactersPerToken);
      if (type !== "tool_use" || block === null) continue;
      thread.toolCalls += 1;
      this.uses.set(string(block["id"]) ?? "", { name: string(block["name"]) ?? "", input: record(block["input"]) ?? {}, thread: thread.id });
    }
  }

  user(event: LogRecord, message: LogRecord): void {
    const thread = this.thread(string(event["parent_tool_use_id"]));
    const content = message["content"];
    const blocks = typeof content === "string" ? [{ type: "text", text: content }] : list(content).map(record);
    for (const block of blocks) {
      if (block?.["type"] === "tool_result") {
        const use = this.uses.get(string(block["tool_use_id"]) ?? "");
        const output = text(block["content"]);
        const facts = use === undefined ? null : claudeToolFacts(use.name, use.input);
        thread.add(facts === null ? [{ class: ContextClass.command, fileType: null, chars: output.length }] : outputSegments(facts, output, this.files));
        if (facts?.kind === ToolKind.edit) thread.markEdit();
      } else if (block?.["type"] === "text") {
        const opening = thread.role === ThreadRole.subagent ? ContextClass.delegation : ContextClass.upfront;
        thread.add([{ class: thread.requests === 0 ? opening : ContextClass.user, fileType: null, chars: text(block["text"]).length }]);
      }
    }
  }
}

/** Every thread of a Claude Code run's stream events: the main agent first, then each helper it started. */
export function claudeThreads(content: string, files: KnownFiles): ThreadLog[] {
  const reader = new StreamReader(files);
  for (const event of logLines(content)) {
    const message = record(event["message"]);
    if (event["type"] === "assistant" && message !== null) reader.assistant(event, message);
    if (event["type"] === "user" && message !== null) reader.user(event, message);
    if (event["type"] === "system" && event["subtype"] === "compact_boundary") reader.thread(null).compact();
  }
  return [...reader.threads.values()].map((thread) => thread.log()).filter((thread) => thread.steps.length > 0);
}

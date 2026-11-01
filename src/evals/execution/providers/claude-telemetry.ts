import type { AgentTelemetry } from "../../history/results.types.ts";
import { isOpenForgePath } from "./open-forge-paths.ts";

interface ClaudeUsage {
  readonly input_tokens?: number;
  readonly cache_creation_input_tokens?: number;
  readonly cache_read_input_tokens?: number;
  readonly output_tokens?: number;
}

interface ContentBlock {
  readonly type?: string;
  readonly id?: string;
  readonly name?: string;
  readonly input?: { readonly file_path?: string };
  readonly tool_use_id?: string;
  readonly is_error?: boolean;
  readonly content?: unknown;
}

/** The fields of Claude Code's stream-json events that the eval reads. */
export interface ClaudeEvent {
  readonly type?: string;
  readonly subtype?: string;
  readonly session_id?: string;
  readonly model?: string;
  readonly message?: { readonly id?: string; readonly usage?: ClaudeUsage; readonly content?: readonly ContentBlock[] | string };
  readonly total_cost_usd?: number;
  readonly result?: string;
  readonly is_error?: boolean;
  readonly modelUsage?: Readonly<Record<string, { readonly contextWindow?: number }>>;
}

const count = (value: number | undefined): number => value ?? 0;
const contextOf = (usage: ClaudeUsage): number =>
  count(usage.input_tokens) + count(usage.cache_creation_input_tokens) + count(usage.cache_read_input_tokens);

/** Adds up one Claude Code session. Each model request counts once, by its message ID, helpers included. */
export class ClaudeTelemetry {
  private readonly usage = new Map<string, ClaudeUsage>();
  private readonly openForgeTools = new Set<string>();
  private toolCalls = 0;
  private failedToolCalls = 0;
  private openForgeBytes = 0;
  private compactions = 0;
  private cost = 0;
  private contextWindow: number | null = null;
  private model: string | null = null;
  private sessionId: string | null = null;

  record(event: ClaudeEvent): void {
    this.sessionId = event.session_id ?? this.sessionId;
    if (event.type === "system" && event.subtype === "init") this.model = event.model ?? null;
    if (event.type === "system" && event.subtype === "compact_boundary") this.compactions += 1;
    if (event.type === "result") this.recordResult(event);
    const content = typeof event.message?.content === "string" ? [] : (event.message?.content ?? []);
    if (event.type === "assistant" && event.message?.id !== undefined && event.message.usage !== undefined) {
      this.usage.set(event.message.id, event.message.usage); // A later event of the same message has the final count.
    }
    for (const block of content) this.recordBlock(block);
  }

  private recordResult(event: ClaudeEvent): void {
    this.cost += event.total_cost_usd ?? 0;
    const windows = Object.values(event.modelUsage ?? {}).map((entry) => entry.contextWindow ?? 0);
    if (windows.length > 0) this.contextWindow = Math.max(...windows);
  }

  private recordBlock(block: ContentBlock): void {
    if (block.type === "tool_use" && block.id !== undefined) {
      this.toolCalls += 1;
      if (block.name === "Read" && isOpenForgePath(block.input?.file_path ?? "")) this.openForgeTools.add(block.id);
    }
    if (block.type !== "tool_result" || block.tool_use_id === undefined) return;
    if (block.is_error === true) this.failedToolCalls += 1;
    if (this.openForgeTools.has(block.tool_use_id)) this.openForgeBytes += JSON.stringify(block.content ?? "").length;
  }

  summary(): AgentTelemetry {
    const requests = [...this.usage.values()];
    const sum = (pick: (usage: ClaudeUsage) => number | undefined): number => requests.reduce((total, usage) => total + count(pick(usage)), 0);
    return {
      tokens: requests.length === 0 ? null : {
        input: sum((usage) => usage.input_tokens),
        cachedInput: sum((usage) => usage.cache_read_input_tokens),
        cacheWrite: sum((usage) => usage.cache_creation_input_tokens),
        output: sum((usage) => usage.output_tokens),
        reasoning: 0, // Included in output tokens. Claude Code doesn't report it apart.
      },
      requests: requests.length,
      peakContext: requests.length === 0 ? null : Math.max(...requests.map(contextOf)),
      contextWindow: this.contextWindow,
      compactions: this.compactions,
      toolCalls: this.toolCalls,
      failedToolCalls: this.failedToolCalls,
      openForgeReads: this.openForgeTools.size,
      openForgeBytes: this.openForgeBytes,
      reportedCostUsd: this.cost,
      model: this.model,
      sessionId: this.sessionId,
    };
  }
}

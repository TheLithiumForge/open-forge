import type { TokenTotals } from "./analysis.types.ts";
import { CharactersPerToken, ContextClass, type ThreadRole } from "./classes.ts";
import type { Estimate, LedgerStep } from "./ledger.ts";
import type { Segment } from "./segments.ts";

/** One request's exact usage. Input counts every input token: new, cached and written to the cache. */
export interface RequestUsage {
  readonly input: number;
  readonly cached: number;
  readonly cacheWrite: number;
  readonly output: number;
  readonly reasoning: number | null;
}

/** One agent thread's requests, ready for the ledger, with its exact totals. */
export interface ThreadLog {
  readonly id: string;
  readonly parentId: string | null;
  readonly role: ThreadRole;
  readonly steps: readonly LedgerStep[];
  readonly tokens: TokenTotals;
  readonly peakContext: number | null;
  readonly toolCalls: number;
  readonly compactions: number;
  /** The model its requests ran on, when the log names one. */
  readonly model?: string;
  /** The first request sent after an edit's result, or null when the thread never edited a file. */
  readonly firstEditStep: number | null;
}

const estimate = (segment: Segment): Estimate => ({ class: segment.class, fileType: segment.fileType, tokens: segment.chars / CharactersPerToken });

/** Collects what joins one thread's context between requests, and closes a ledger step at each request. */
export class ThreadBuilder {
  readonly id: string;
  readonly parentId: string | null;
  readonly role: ThreadRole;
  toolCalls = 0;
  model: string | null = null;
  private compactions = 0;
  private readonly steps: LedgerStep[] = [];
  private pending: Estimate[] = [];
  private lastOutput = 0;
  private compacted = false;
  private edited = false;
  private firstEditStep: number | null = null;
  private peak: number | null = null;
  private tokens: TokenTotals = { newInput: 0, cachedInput: 0, cacheWrite: 0, output: 0, reasoning: null };

  constructor(id: string, parentId: string | null, role: ThreadRole) {
    this.id = id;
    this.parentId = parentId;
    this.role = role;
  }

  get requests(): number { return this.steps.length; }
  add(segments: readonly Segment[]): void { this.pending.push(...segments.map(estimate)); }
  /** An edit's result joined the context, so the next request is the first of the implementation. */
  markEdit(): void { this.edited = true; }

  /** A compaction before the next request. Logs can report one compaction twice, so it counts once per request. */
  compact(): void {
    if (!this.compacted) this.compactions += 1;
    this.compacted = true;
  }

  /** Closes a step. The previous request's output joins this one's context, sized by the estimate given or its exact output. */
  request(usage: RequestUsage, outputEstimate = usage.output): void {
    if (this.edited && this.firstEditStep === null) this.firstEditStep = this.steps.length;
    const agent = this.steps.length === 0 || this.lastOutput <= 0 ? [] : [{ class: ContextClass.agent, fileType: null, tokens: this.lastOutput }];
    this.steps.push({ input: usage.input, fresh: usage.input - usage.cached, added: [...this.pending, ...agent], compacted: this.compacted });
    this.pending = [];
    this.compacted = false;
    this.lastOutput = outputEstimate;
    this.peak = Math.max(this.peak ?? 0, usage.input);
    const reasoning = usage.reasoning === null ? this.tokens.reasoning : (this.tokens.reasoning ?? 0) + usage.reasoning;
    this.tokens = { newInput: this.tokens.newInput + usage.input - usage.cached - usage.cacheWrite, cachedInput: this.tokens.cachedInput + usage.cached,
      cacheWrite: this.tokens.cacheWrite + usage.cacheWrite, output: this.tokens.output + usage.output, reasoning };
  }

  /** Adds to the latest request's estimated output, for logs that report it block by block. */
  addOutput(tokens: number): void { this.lastOutput += tokens; }

  log(): ThreadLog {
    return { ...(this.model === null ? {} : { model: this.model }), id: this.id, parentId: this.parentId, role: this.role, steps: this.steps, tokens: this.tokens,
      peakContext: this.peak, toolCalls: this.toolCalls, compactions: this.compactions, firstEditStep: this.firstEditStep };
  }
}

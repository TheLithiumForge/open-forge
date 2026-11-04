import type { Run, ThreadTelemetry } from "../records/records.types.ts";
import type { ThreadRoles } from "../records/schema-values.ts";

/** Standard API prices in US dollars per million tokens. Batch and fast modes are left out, because no run uses them. */
export interface TokenPrices {
  readonly input: number;
  readonly cachedInput: number;
  readonly cacheWrite: number;
  readonly output: number;
}

/** A model's prices. Some models charge more once one request's input passes a size, at the long rates. */
export interface ModelPrice extends TokenPrices {
  readonly name: string;
  readonly long?: TokenPrices & { readonly above: number };
}

/** The prices that took effect on one date, with where they were read. */
export interface PriceVersion {
  readonly from: string;
  readonly sources: readonly { readonly provider: string; readonly url: string }[];
  readonly models: Readonly<Record<string, ModelPrice>>;
}

/**
 * The price list, newest first. A price change adds a version at the top, so earlier prices stay on record.
 * Cost always reads the newest price of a model, so a price cut can't make a newer result look cheaper than an older one.
 */
export const PriceVersions: readonly PriceVersion[] = [{
  from: "2026-11-04",
  sources: [
    { provider: "OpenAI", url: "https://developers.openai.com/api/docs/pricing" },
    { provider: "Anthropic", url: "https://platform.claude.com/docs/en/about-claude/pricing" },
  ],
  models: {
    "gpt-6-astra": { name: "GPT-6 Astra", input: 10, cachedInput: 1, cacheWrite: 12.5, output: 50 },
    "gpt-6.1-sol": { name: "GPT-6.1 Sol", input: 2, cachedInput: 0.1, cacheWrite: 2.5, output: 10, long: { above: 272_000, input: 4, cachedInput: 0.2, cacheWrite: 5, output: 15 } },
    "gpt-6-sol": { name: "GPT-6 Sol", input: 2, cachedInput: 0.2, cacheWrite: 2.5, output: 10, long: { above: 272_000, input: 4, cachedInput: 0.4, cacheWrite: 5, output: 15 } },
    "gpt-6.1-terra": { name: "GPT-6.1 Terra", input: 1, cachedInput: 0.05, cacheWrite: 1.25, output: 5 },
    "gpt-6-luna": { name: "GPT-6 Luna", input: 0.1, cachedInput: 0.01, cacheWrite: 0.125, output: 0.5, long: { above: 272_000, input: 0.2, cachedInput: 0.02, cacheWrite: 0.25, output: 0.75 } },
    "claude-fable-5-1": { name: "Claude Fable 5.1", input: 10, cachedInput: 0.25, cacheWrite: 12.5, output: 50 },
    "claude-opus-5-5": { name: "Claude Opus 5.5", input: 4, cachedInput: 0.2, cacheWrite: 5, output: 20 },
    "claude-sonnet-5-5": { name: "Claude Sonnet 5.5", input: 2, cachedInput: 0.1, cacheWrite: 2.5, output: 10 },
    "claude-haiku-5-5": { name: "Claude Haiku 5.5", input: 0.1, cachedInput: 0.01, cacheWrite: 0.125, output: 0.5, long: { above: 100_000, input: 0.5, cachedInput: 0.05, cacheWrite: 0.625, output: 2.5 } },
  },
}];

/** Claude Code takes these short names, and a run that reported no model records only the name it was asked for. */
const ModelAliases: Readonly<Record<string, string>> = { opus: "claude-opus-5-5", sonnet: "claude-sonnet-5-5", haiku: "claude-haiku-5-5", fable: "claude-fable-5-1" };
const TokensPerPrice = 1_000_000;
const MainThread: (typeof ThreadRoles)[number] = "main";

/** Providers can report a model with a context suffix, a date stamp or another case, such as claude-opus-5-5[1m]. */
export const canonicalModel = (model: string): string => model.toLowerCase().replace(/\[[^\]]*\]$/, "").replace(/-\d{8}$/, "");

/** A model's newest price, or null for a model the list doesn't have. */
export function priceOf(model: string | null): ModelPrice | null {
  const known = model === null ? null : canonicalModel(model);
  const id = known === null ? null : ModelAliases[known] ?? known;
  if (id === null) return null;
  for (const version of PriceVersions) {
    const price = version.models[id];
    if (price !== undefined) return price;
  }
  return null;
}

const tokenCost = (tokens: { readonly input: number; readonly cachedInput: number; readonly cacheWrite: number; readonly output: number }, price: TokenPrices): number =>
  (tokens.input * price.input + tokens.cachedInput * price.cachedInput + tokens.cacheWrite * price.cacheWrite + tokens.output * price.output) / TokensPerPrice;

/** The model a thread ran on. Records before threads named their model follow the implementer: its lead, then its team. */
function threadModel(run: Run, thread: ThreadTelemetry): string | null {
  const lead = run.reportedModel ?? run.agent.model;
  return thread.model ?? (thread.role === MainThread ? lead : run.agent.team?.model ?? lead);
}

/** One thread's cost, or null when its model has no price. */
export function threadCost(run: Run, thread: ThreadTelemetry): number | null {
  const price = priceOf(threadModel(run, thread));
  return price === null ? null : tokenCost({ ...thread.tokens, input: thread.tokens.newInput }, price);
}

/**
 * What a run's tokens would cost on the API, or null without tokens or a price. Output already includes reasoning.
 * Totals can't show a single request's size, so long rates don't apply: Codex's context window stays below GPT's threshold.
 */
export function runCost(run: Run): number | null {
  const threads = run.telemetry?.threads;
  if (threads !== undefined && threads.length > 0) {
    const costs = threads.map((thread) => threadCost(run, thread));
    return costs.some((cost) => cost === null) ? null : costs.reduce<number>((sum, cost) => sum + (cost ?? 0), 0);
  }
  const tokens = run.telemetry?.tokens;
  const price = priceOf(run.reportedModel ?? run.agent.model);
  return tokens == null || price === null ? null : tokenCost(tokens, price);
}

/** The orchestrator thread's share of a run's cost, or null for a run without subagents. */
export function orchestratorCostShare(run: Run): number | null {
  const threads = run.telemetry?.threads ?? [];
  const main = threads.find((thread) => thread.role === MainThread);
  const total = runCost(run);
  if (main === undefined || threads.length < 2 || total === null || total === 0) return null;
  return (threadCost(run, main) ?? 0) / total;
}

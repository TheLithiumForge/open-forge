import { ContextClass } from "./classes.ts";

/** A part of the context with its estimated size in tokens. */
export interface Estimate {
  readonly class: ContextClass;
  readonly fileType: string | null;
  readonly tokens: number;
}

/** One model request. Input and fresh are exact. Added is what joined the context since the previous request, estimated. */
export interface LedgerStep {
  /** Every input token of the request: new, cached and written to the cache. */
  readonly input: number;
  /** Input the provider hadn't cached: new input and cache writes. */
  readonly fresh: number;
  readonly added: readonly Estimate[];
  /** A compaction replaced the history before this request. */
  readonly compacted: boolean;
}

/** Tokens by part key. Input and fresh add up every request. Peak is the largest request's context. */
export interface LedgerTotals {
  readonly input: ReadonlyMap<string, number>;
  readonly fresh: ReadonlyMap<string, number>;
  readonly peak: ReadonlyMap<string, number>;
}

type Tokens = Map<string, number>;

const KeySeparator = "\u0000";
export const partKey = (type: ContextClass, fileType: string | null): string => `${type}${KeySeparator}${fileType ?? ""}`;
/** The class and file type a part key names. */
export function partOf(key: string): { readonly type: ContextClass; readonly fileType: string | null } {
  const [name = "", extension = ""] = key.split(KeySeparator);
  return { type: Object.values(ContextClass).find((value) => value === name) ?? ContextClass.other, fileType: extension === "" ? null : extension };
}
const total = (tokens: ReadonlyMap<string, number>) => [...tokens.values()].reduce((sum, value) => sum + value, 0);
const scaled = (tokens: ReadonlyMap<string, number>, factor: number): Tokens => new Map([...tokens].map(([key, value]) => [key, value * factor]));
function addTo(target: Tokens, tokens: ReadonlyMap<string, number>, factor = 1): void {
  for (const [key, value] of tokens) target.set(key, (target.get(key) ?? 0) + value * factor);
}
function estimates(added: readonly Estimate[]): Tokens {
  const tokens: Tokens = new Map();
  for (const item of added) if (item.tokens > 0) addTo(tokens, new Map([[partKey(item.class, item.fileType), item.tokens]]));
  return tokens;
}

/** Known parts keep their estimates within the total, and the class given takes the rest. */
function fitted(added: Tokens, size: number, rest: ContextClass): Tokens {
  const known = total(added);
  if (known > size) return scaled(added, size / known);
  const tokens = new Map(added);
  if (size > known) addTo(tokens, new Map([[partKey(rest, null), size - known]]));
  return tokens;
}

/** Fresh input goes to the newest parts first. More fresh input than they hold means older parts left the cache. */
function freshShares(context: Tokens, joined: Tokens, fresh: number): Tokens {
  const newest = total(joined);
  if (fresh <= newest) return newest === 0 ? new Map() : scaled(joined, fresh / newest);
  const older: Tokens = new Map([...context].map(([key, value]) => [key, Math.max(0, value - (joined.get(key) ?? 0))]));
  const base = total(older) > 0 ? older : context;
  const shares = new Map(joined);
  if (total(base) > 0) addTo(shares, base, (fresh - newest) / total(base));
  return shares;
}

/**
 * Splits every request's exact input across the parts of its context. Each request's growth splits across what joined
 * it by estimated size, so the parts of every request sum exactly to its input.
 */
export function runLedger(steps: readonly LedgerStep[]): LedgerTotals {
  const input: Tokens = new Map(), fresh: Tokens = new Map();
  let context: Tokens = new Map(), peak: Tokens = new Map(), peakInput = -1, upfront = 0;
  steps.forEach((step, index) => {
    const added = estimates(step.added);
    let joined: Tokens;
    if (index === 0) {
      joined = fitted(added, step.input, ContextClass.upfront);
      upfront = joined.get(partKey(ContextClass.upfront, null)) ?? 0;
      context = new Map(joined);
    } else if (step.compacted) {
      // The summary replaces the history. The up-front text stays, and the summary with what joined after it counts as other.
      joined = fitted(new Map([[partKey(ContextClass.upfront, null), Math.min(upfront, step.input)]]), step.input, ContextClass.other);
      context = new Map(joined);
    } else {
      const growth = step.input - total(context);
      joined = growth >= 0 ? (total(added) > 0 ? scaled(added, growth / total(added)) : new Map([[partKey(ContextClass.other, null), growth]])) : added;
      addTo(context, joined);
      // The context shrank, for example when earlier reasoning was dropped, so every part shrinks alike.
      if (growth < 0 && total(context) > 0) {
        const factor = step.input / total(context);
        context = scaled(context, factor);
        joined = scaled(joined, factor);
      }
    }
    addTo(input, context);
    addTo(fresh, freshShares(context, joined, step.fresh));
    if (step.input > peakInput) { peakInput = step.input; peak = new Map(context); }
  });
  return { input, fresh, peak };
}

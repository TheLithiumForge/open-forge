import type { Run } from "./records/records.types.ts";
import { compact, percent } from "./data/format.ts";
import { cacheHitRate, tokenClass, TokenClass } from "./data/run-measures.ts";
import { ClaudeTool, TokenNote } from "./info/notes.ts";

const Classes = [
  [TokenClass.newInput, "New input"], [TokenClass.cacheWrites, "Cache writes"], [TokenClass.cachedInput, "Cached input"], [TokenClass.output, "Output"],
] as const;

/** Reads the reasoning count, which the run records carry but their synced types don't list. */
function reasoningTokens(run: Run): number | null {
  const counts: unknown = run.telemetry?.tokens;
  return typeof counts === "object" && counts !== null && "reasoning" in counts && typeof counts.reasoning === "number" ? counts.reasoning : null;
}

/** A run's tokens by class. Claude output without any reasoning count might come from the old telemetry, which undercounted it. */
export function RunTokens({ run }: { readonly run: Run }) {
  if (run.telemetry?.tokens == null) return null;
  const doubtful = run.agent.tool === ClaudeTool && reasoningTokens(run) === 0;
  return <>
    <p>{Classes.map(([kind, title]) => `${title} ${compact(tokenClass(run, kind))}`).join(". ")}{doubtful ? ", which might be undercounted" : ""}. Cache hit rate {percent(cacheHitRate(run))}.</p>
    {doubtful ? <p className="eval-muted">{TokenNote}</p> : null}
  </>;
}

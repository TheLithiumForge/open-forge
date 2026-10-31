import type { EvalConfig } from "../src/config/config.types.ts";
import type { BatchRecord } from "../src/results/results.types.ts";
import type { PreferenceWinner } from "../src/results/statuses.ts";
import type { BatchResponse, ComparePair, StateResponse } from "../src/server/api.types.ts";
import type { IntentItem } from "../src/suite/suite.types.ts";

interface ErrorBody {
  readonly error?: string;
}

async function call<T>(path: string, method = "GET", body?: unknown): Promise<T> {
  const response = await fetch(path, {
    method,
    headers: { "content-type": "application/json" },
    ...(body === undefined ? {} : { body: JSON.stringify(body) }),
  });
  const value: unknown = await response.json();
  if (!response.ok) throw new Error((value as ErrorBody | null)?.error ?? response.statusText); // The server's own error shape.
  return value as T; // The eval server answers with the types in api.types.ts.
}

export const api = {
  state: () => call<StateResponse>("/api/state"),
  saveConfig: (config: EvalConfig) => call<EvalConfig>("/api/config", "PUT", config),
  saveIntent: (intent: readonly IntentItem[]) => call<StateResponse>("/api/intent", "PUT", intent),
  startBatch: () => call<BatchRecord>("/api/batches", "POST"),
  batch: (id: string) => call<BatchResponse>(`/api/batches/${id}`),
  resume: (id: string) => call<null>(`/api/batches/${id}/resume`, "POST"),
  pair: (id: string) => call<ComparePair | null>(`/api/batches/${id}/pair`),
  prefer: (id: string, left: string, right: string, winner: PreferenceWinner) => call<BatchResponse>(`/api/batches/${id}/preference`, "POST", { left, right, winner }),
  reveal: (id: string) => call<BatchResponse>(`/api/batches/${id}/reveal`, "POST"),
};

export function shotUrl(batchId: string, runId: string, name: string): string {
  return `/shots/${batchId}/${runId}/${name}`;
}

export function percent(value: number | null): string {
  return value === null ? "–" : `${Math.round(value * 100)}%`;
}

export function number(value: number | null, digits = 0): string {
  return value === null ? "–" : value.toLocaleString(undefined, { maximumFractionDigits: digits });
}

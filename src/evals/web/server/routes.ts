import type { IncomingMessage, ServerResponse } from "node:http";
import { dockerAvailable } from "../../execution/containers/docker-image.ts";
import { resumeBatch, runningBatch, startBatch } from "../../execution/batch.ts";
import { progress } from "../../execution/progress.ts";
import { signInStatus } from "../../config/auth.ts";
import { loadConfig, saveConfig } from "../../config/config.ts";
import { PreferenceWinner } from "../../history/statuses.ts";
import { addPreference, listBatches, readBatch, readPreferences, readRuns, saveBatch } from "../../history/store.ts";
import { summarize } from "../../history/summary.ts";
import { loadSuite, saveIntent } from "../../benchmarks/suite.ts";
import type { BatchResponse, StateResponse } from "./api.types.ts";
import { allPairs, nextPair } from "./compare.ts";

export const ApiPaths = {
  state: "/api/state",
  config: "/api/config",
  intent: "/api/intent",
  batches: "/api/batches",
  events: "/api/events",
} as const;

const batchPath = /^\/api\/batches\/([\w-]+)(?:\/(resume|pair|preference|reveal))?$/;
const winners: readonly string[] = Object.values(PreferenceWinner);

export async function readBody(request: IncomingMessage): Promise<unknown> {
  const chunks: Buffer[] = [];
  for await (const chunk of request) chunks.push(Buffer.from(chunk));
  return JSON.parse(Buffer.concat(chunks).toString("utf8") || "null");
}

export function sendJson(response: ServerResponse, status: number, value: unknown): void {
  response.writeHead(status, { "content-type": "application/json" }).end(JSON.stringify(value));
}

async function state(): Promise<StateResponse> {
  const config = await loadConfig();
  const suite = await loadSuite(config.suite);
  const setups = Object.fromEntries(Object.entries(suite.setups).map(([id, setup]) => [id, setup.label]));
  return {
    config,
    suite: { id: suite.id, title: suite.title, prompt: suite.prompt, intent: suite.intent, rubric: suite.rubric, setups, fingerprint: suite.fingerprint, intentEdited: suite.intentEdited },
    signIn: signInStatus(),
    docker: await dockerAvailable(),
    batches: await listBatches(),
    running: runningBatch(),
  };
}

/** Until the reveal, a batch shows only what can't tell a run's setup. */
async function batchView(batchId: string): Promise<BatchResponse> {
  const batch = await readBatch(batchId);
  const runs = await readRuns(batch);
  const preferences = await readPreferences(batchId);
  return {
    batch,
    hidden: runs.map((run) => ({ id: run.id, status: run.status, agent: run.agent })),
    runs: batch.revealed ? runs : null,
    summary: batch.revealed ? summarize(runs, preferences) : null,
    progress: progress.history(batchId),
    compared: preferences.length,
    pairs: allPairs(runs).length,
  };
}

export async function handleApi(request: IncomingMessage, response: ServerResponse, url: URL, port: number): Promise<void> {
  const method = request.method ?? "GET";
  if (url.pathname === ApiPaths.state) return sendJson(response, 200, await state());
  if (url.pathname === ApiPaths.config && method === "PUT") return sendJson(response, 200, await saveConfig(await readBody(request)));
  if (url.pathname === ApiPaths.intent && method === "PUT") {
    const config = await loadConfig();
    const body = await readBody(request);
    if (!Array.isArray(body)) return sendJson(response, 400, { error: "Send the intent as a list." });
    await saveIntent(config.suite, body);
    return sendJson(response, 200, await state());
  }
  if (url.pathname === ApiPaths.batches && method === "POST") return sendJson(response, 200, await startBatch());
  const match = batchPath.exec(url.pathname);
  const batchId = match?.[1];
  if (batchId === undefined) return sendJson(response, 404, { error: "Not found." });
  const action = match?.[2];
  const expected = action === undefined || action === "pair" ? "GET" : "POST";
  if (method !== expected) return sendJson(response, 405, { error: `Use ${expected}.` });
  if (action === undefined) return sendJson(response, 200, await batchView(batchId));
  if (action === "resume") return sendJson(response, 200, await resumeBatch(batchId));
  if (action === "pair") {
    const batch = await readBatch(batchId);
    return sendJson(response, 200, nextPair(await readRuns(batch), await readPreferences(batchId), port));
  }
  if (action === "preference") {
    const body = await readBody(request);
    const isPreference = typeof body === "object" && body !== null && "left" in body && "right" in body && "winner" in body;
    if (!isPreference || typeof body.left !== "string" || typeof body.right !== "string" || !winners.includes(String(body.winner))) {
      return sendJson(response, 400, { error: "Send left, right and winner." });
    }
    const winner = Object.values(PreferenceWinner).find((value) => value === body.winner) ?? PreferenceWinner.tie;
    await addPreference(batchId, { left: body.left, right: body.right, winner, at: new Date().toISOString() });
    return sendJson(response, 200, await batchView(batchId));
  }
  await saveBatch({ ...(await readBatch(batchId)), revealed: true });
  return sendJson(response, 200, await batchView(batchId));
}

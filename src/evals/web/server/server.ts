import { spawn } from "node:child_process";
import { existsSync } from "node:fs";
import { readdir } from "node:fs/promises";
import { createServer, type IncomingMessage, type ServerResponse } from "node:http";
import { extname, join, normalize, sep } from "node:path";
import { pauseInterruptedBatches } from "../../execution/batch.ts";
import { progress } from "../../execution/progress.ts";
import { loadLocalEnv } from "../../config/auth.ts";
import { EvalPaths, ResultFiles } from "../../paths.ts";
import { assessmentEvidenceFolder, readBatch, runFolder } from "../../history/store.ts";
import { refusal, type RequestFacts } from "./guard.ts";
import { ApiPaths, handleApi, sendJson } from "./routes.ts";
import { gameHeaders } from "../game-policy.ts";
import { readPlainFile } from "./plain-file.ts";

const Port = Number(process.env["EVALS_PORT"] ?? "4318");
const contentTypes: Readonly<Record<string, string>> = { ".html": "text/html", ".js": "text/javascript", ".css": "text/css", ".png": "image/png", ".svg": "image/svg+xml" };
const runHost = /^([0-9a-f]{12})\.localhost(?::\d+)?$/;
const shotPath = /^\/shots\/([\w-]+)\/([0-9a-f]{12})\/([\w.-]+\.png)$/;
const assessmentShotPath = /^\/shots\/([\w-]+)\/([0-9a-f]{12})\/([\w-]+)\/([\w.-]+\.png)$/;

async function sendFile(response: ServerResponse, path: string, headers: Readonly<Record<string, string>> = {}): Promise<void> {
  if (!existsSync(path)) return void response.writeHead(404).end();
  const root = path.startsWith(EvalPaths.uiDist + sep) ? EvalPaths.uiDist : EvalPaths.results;
  const bytes = await readPlainFile(root, path);
  response.writeHead(200, { "content-type": contentTypes[extname(path)] ?? "application/octet-stream", "x-content-type-options": "nosniff", ...headers }).end(bytes);
}

/** Finds which batch a run belongs to, for a game opened on its own origin. */
async function batchOfRun(runId: string): Promise<string | null> {
  if (!existsSync(EvalPaths.results)) return null;
  for (const batchId of await readdir(EvalPaths.results)) if (existsSync(runFolder(batchId, runId))) return batchId;
  return null;
}

function streamEvents(request: IncomingMessage, response: ServerResponse, batchId: string): void {
  response.writeHead(200, { "content-type": "text/event-stream", "cache-control": "no-cache", connection: "keep-alive" });
  const unsubscribe = progress.subscribe((message) => {
    if (message.batchId === batchId) void readBatch(batchId).then((batch) => {
      if (batch.revealed && !response.destroyed) response.write(`data: ${JSON.stringify(message)}\n\n`);
    }).catch(() => {});
  });
  request.on("close", unsubscribe);
}

function requestFacts(request: IncomingMessage): RequestFacts {
  const header = (name: string): string | undefined => {
    const value = request.headers[name];
    return Array.isArray(value) ? value[0] : value;
  };
  return { method: request.method ?? "GET", host: header("host"), origin: header("origin"), fetchSite: header("sec-fetch-site"), contentType: header("content-type") };
}

async function handle(request: IncomingMessage, response: ServerResponse): Promise<void> {
  const url = new URL(request.url ?? "/", `http://${request.headers.host ?? "localhost"}`);
  const refused = refusal(requestFacts(request), Port, url.pathname.startsWith("/api/"));
  if (refused !== null) return sendJson(response, 403, { error: refused });
  const game = runHost.exec(request.headers.host ?? "");
  if (game?.[1] !== undefined) {
    const batchId = await batchOfRun(game[1]);
    return sendFile(response, batchId === null ? "" : join(runFolder(batchId, game[1]), ResultFiles.game), gameHeaders(Port));
  }
  if (url.pathname === ApiPaths.events) return streamEvents(request, response, url.searchParams.get("batch") ?? "");
  if (url.pathname.startsWith("/api/")) return handleApi(request, response, url, Port);
  const shot = shotPath.exec(url.pathname);
  if (shot?.[1] !== undefined && shot[2] !== undefined && shot[3] !== undefined) return sendFile(response, join(runFolder(shot[1], shot[2]), ResultFiles.shots, shot[3]));
  const assessedShot = assessmentShotPath.exec(url.pathname);
  if (assessedShot?.[1] !== undefined && assessedShot[3] !== undefined && assessedShot[4] !== undefined) {
    return sendFile(response, join(assessmentEvidenceFolder(assessedShot[1], assessedShot[3]), ResultFiles.shots, assessedShot[4]));
  }
  const file = normalize(join(EvalPaths.uiDist, url.pathname === "/" ? "index.html" : url.pathname));
  return sendFile(response, file.startsWith(EvalPaths.uiDist + sep) ? file : "");
}

function openBrowser(url: string): void {
  const [command, args] = process.platform === "win32" ? ["cmd", ["/c", "start", "", url]] : [process.platform === "darwin" ? "open" : "xdg-open", [url]];
  spawn(command, args, { detached: true, stdio: "ignore", windowsHide: true }).unref();
}

loadLocalEnv();
await pauseInterruptedBatches();
createServer((request, response) => {
  handle(request, response).catch((error: unknown) => sendJson(response, 500, { error: error instanceof Error ? error.message : String(error) }));
}).listen(Port, "127.0.0.1", () => {
  const url = `http://localhost:${Port}/`;
  console.log(`Open Forge evals are running at ${url}`);
  if (process.env["EVALS_NO_BROWSER"] === undefined) openBrowser(url);
});

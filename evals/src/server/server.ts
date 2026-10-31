import { spawn } from "node:child_process";
import { existsSync } from "node:fs";
import { readdir, readFile } from "node:fs/promises";
import { createServer, type IncomingMessage, type ServerResponse } from "node:http";
import { extname, join, normalize } from "node:path";
import { pauseInterruptedBatches } from "../batch/batch.ts";
import { progress } from "../batch/progress.ts";
import { loadLocalEnv } from "../config/auth.ts";
import { EvalPaths, ResultFiles } from "../paths.ts";
import { runFolder } from "../results/store.ts";
import { ApiPaths, handleApi, sendJson } from "./routes.ts";

const Port = Number(process.env["EVALS_PORT"] ?? "4318");
const contentTypes: Readonly<Record<string, string>> = { ".html": "text/html", ".js": "text/javascript", ".css": "text/css", ".png": "image/png", ".svg": "image/svg+xml" };
const runHost = /^([0-9a-f]{12})\.localhost(?::\d+)?$/;
const shotPath = /^\/shots\/([\w-]+)\/([0-9a-f]{12})\/([\w.-]+\.png)$/;

async function sendFile(response: ServerResponse, path: string): Promise<void> {
  if (!existsSync(path)) return void response.writeHead(404).end();
  response.writeHead(200, { "content-type": contentTypes[extname(path)] ?? "application/octet-stream" }).end(await readFile(path));
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
    if (message.batchId === batchId) response.write(`data: ${JSON.stringify(message)}\n\n`);
  });
  request.on("close", unsubscribe);
}

async function handle(request: IncomingMessage, response: ServerResponse): Promise<void> {
  const url = new URL(request.url ?? "/", `http://${request.headers.host ?? "localhost"}`);
  const game = runHost.exec(request.headers.host ?? "");
  if (game?.[1] !== undefined) {
    const batchId = await batchOfRun(game[1]);
    return sendFile(response, batchId === null ? "" : join(runFolder(batchId, game[1]), ResultFiles.game));
  }
  if (url.pathname === ApiPaths.events) return streamEvents(request, response, url.searchParams.get("batch") ?? "");
  if (url.pathname.startsWith("/api/")) return handleApi(request, response, url, Port);
  const shot = shotPath.exec(url.pathname);
  if (shot?.[1] !== undefined && shot[2] !== undefined && shot[3] !== undefined) return sendFile(response, join(runFolder(shot[1], shot[2]), ResultFiles.shots, shot[3]));
  const file = normalize(join(EvalPaths.uiDist, url.pathname === "/" ? "index.html" : url.pathname));
  return sendFile(response, file.startsWith(EvalPaths.uiDist) ? file : "");
}

function openBrowser(url: string): void {
  const [command, args] = process.platform === "win32" ? ["cmd", ["/c", "start", "", url]] : [process.platform === "darwin" ? "open" : "xdg-open", [url]];
  spawn(command, args, { detached: true, stdio: "ignore", windowsHide: true }).unref();
}

loadLocalEnv();
await pauseInterruptedBatches();
createServer((request, response) => {
  handle(request, response).catch((error: unknown) => sendJson(response, 500, { error: error instanceof Error ? error.message : String(error) }));
}).listen(Port, () => {
  const url = `http://localhost:${Port}/`;
  console.log(`Open Forge evals are running at ${url}`);
  if (process.env["EVALS_NO_BROWSER"] === undefined) openBrowser(url);
});

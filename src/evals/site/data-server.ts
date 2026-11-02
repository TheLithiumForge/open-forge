import { createServer, type Server } from "node:http";
import type { ResultsReader } from "./results-reader.ts";

/** Serves results to the docs site on this machine only. Every request is a read, and only the docs site's origin may read. */
export function startDataServer(reader: ResultsReader, port: number, docsOrigin: string): Promise<Server> {
  const server = createServer((request, response) => {
    response.setHeader("Access-Control-Allow-Origin", docsOrigin);
    response.setHeader("Cache-Control", "no-store");
    response.setHeader("X-Content-Type-Options", "nosniff");
    if (request.method !== "GET") { response.writeHead(405).end(); return; }
    let path: string;
    try { path = decodeURIComponent(new URL(request.url ?? "/", "http://localhost").pathname.slice(1)); }
    catch { response.writeHead(400).end(); return; }
    reader(path).then((file) => {
      if (file === null) { response.writeHead(404).end(); return; }
      response.writeHead(200, { "Content-Type": file.type }).end(file.bytes);
    }, (error: unknown) => {
      response.writeHead(500, { "Content-Type": "text/plain; charset=utf-8" }).end(error instanceof Error ? error.message : String(error));
    });
  });
  return new Promise((resolve, reject) => {
    server.once("error", reject);
    server.listen(port, "127.0.0.1", () => resolve(server));
  });
}

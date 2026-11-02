import { spawn } from "node:child_process";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { setTimeout } from "node:timers/promises";
import { EvalPaths, evalsRoot } from "../paths.ts";
import { HistoryStore } from "../history/store.ts";
import { startDataServer } from "./data-server.ts";
import { folderReader, localRunsReader } from "./results-reader.ts";

export const SiteDefaults = { port: 4320, readyMs: 5 * 60_000, pollMs: 2_000 } as const;
export const DocsFolder = join(evalsRoot, "..", "docusaurus");
// The docs site's baseUrl and the Results page under it.
export const ResultsPagePath = "/open-forge/docs/evals/results";
/** The query parameter that tells the Results page which local port serves the data. */
export const LocalDataParameter = "local";

export interface SiteOptions {
  readonly port: number;
  /** A folder in the results repository's layout to show instead of the local runs. */
  readonly repository: string | null;
  readonly browser: boolean;
}

export function openBrowser(url: string): void {
  const [command, args] = process.platform === "win32" ? ["cmd", ["/c", "start", "", url]] : [process.platform === "darwin" ? "open" : "xdg-open", [url]];
  spawn(command, args, { detached: true, stdio: "ignore", windowsHide: true }).unref();
}

async function waitForDocs(url: string): Promise<void> {
  const deadline = Date.now() + SiteDefaults.readyMs;
  while (Date.now() < deadline) {
    try { if ((await fetch(url)).ok) return; } catch { /* The dev server is still compiling. */ }
    await setTimeout(SiteDefaults.pollMs);
  }
  throw new Error(`The docs site didn't answer at ${url} within ${SiteDefaults.readyMs / 60_000} minutes.`);
}

/** Starts the docs site with a data server on the next port, then opens the Results page reading that data. Runs until the docs site stops. */
export async function runSite(options: SiteOptions, print: (text: string) => void): Promise<number> {
  const docusaurus = join(DocsFolder, "node_modules", "@docusaurus", "core", "bin", "docusaurus.mjs");
  if (!existsSync(docusaurus)) throw new Error("Install the docs site first: npm ci --prefix src/docusaurus");
  const reader = options.repository === null ? localRunsReader(new HistoryStore(EvalPaths.runs)) : folderReader(options.repository);
  const dataPort = options.port + 1, origin = `http://localhost:${options.port}`;
  const server = await startDataServer(reader, dataPort, origin);
  const docs = spawn(process.execPath, [docusaurus, "start", "--port", String(options.port), "--no-open"], { cwd: DocsFolder, stdio: "inherit", windowsHide: true });
  const stopped = new Promise<number>((resolve) => docs.once("exit", (code) => resolve(code ?? 1)));
  try {
    await Promise.race([waitForDocs(`${origin}${ResultsPagePath}`), stopped.then((code) => { throw new Error(`The docs site stopped (${code}).`); })]);
    const url = `${origin}${ResultsPagePath}?${LocalDataParameter}=${dataPort}`;
    print(`Results from ${options.repository ?? "the local runs"} are at ${url}`);
    if (options.browser) openBrowser(url);
    return await stopped;
  } finally {
    if (docs.exitCode === null) docs.kill();
    server.close();
  }
}

// Keeps the eval server to its own pages, so other sites, other hostnames and the games it serves can't drive it.

export interface RequestFacts {
  readonly method: string;
  readonly host: string | undefined;
  readonly origin: string | undefined;
  readonly fetchSite: string | undefined;
  readonly contentType: string | undefined;
}

export function appHosts(port: number): readonly string[] {
  return [`localhost:${port}`, `127.0.0.1:${port}`];
}

/** Each finished game plays on its own origin, named after its run. */
export function isGameHost(host: string, port: number): boolean {
  return new RegExp(`^[0-9a-f]{12}\\.localhost:${port}$`).test(host);
}

// "none" is a request the person typed or bookmarked, which carries no other site.
const sameSite = new Set(["same-origin", "none"]);

/** Null when the request may go on, otherwise the reason it's refused. */
export function refusal(facts: RequestFacts, port: number, api: boolean): string | null {
  const hosts = appHosts(port);
  const host = facts.host ?? "";
  if (!api) return hosts.includes(host) || isGameHost(host, port) ? null : `Open the evals at http://localhost:${port}/.`;
  if (!hosts.includes(host)) return `Open the evals at http://localhost:${port}/.`;
  if (facts.origin !== undefined && !hosts.some((allowed) => facts.origin === `http://${allowed}`)) return "Requests from other pages are refused.";
  if (facts.fetchSite !== undefined && !sameSite.has(facts.fetchSite)) return "Requests from other pages are refused.";
  const changes = facts.method !== "GET" && facts.method !== "HEAD";
  if (changes && !(facts.contentType ?? "").startsWith("application/json")) return "Send requests that change something as JSON.";
  return null;
}
